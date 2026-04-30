using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class GridObjectSystem : SingletonMono<GridObjectSystem>
    {
        public Placement_SO Placement_SO;
        public static Dictionary<int, Room_Placement_Info> InfoIdDic => Instance.Placement_SO.placementDic;
        public static Dictionary<string, Room_Placement_Info> InfoNameDic => Instance.Placement_SO.placementNameDic;

        public IEnumerable<Room_Placement_Info> InfoList => Instance.Placement_SO.placementDic.Values;

        public static Dictionary<string, GridObject> runtimeDic = new Dictionary<string, GridObject>();

        public static Room_Placement_Info GetPlacementInfo(int id)
        {
            Instance.Placement_SO.placementDic.TryGetValue(id, out var info);
            return info;
        }

        public static Room_Placement_Info GetPlacementInfo(string name)
        {
            Instance.Placement_SO.placementNameDic.TryGetValue(name, out var info);
            return info;
        }
        public static PotData GetPotData(string name)
        {
            PlantManager.Instance.potDatas.TryGetValue(name, out var data);
            return data;
        }

        /// <summary>
        /// 创建家具预制体
        /// </summary>
        /// <param name="id"></param>
        public static async Task<PlacementRuntime> CreatePlacement(int id)
        {
            var info = GetPlacementInfo(id);
            return await CreatePlacementInternal(info);
        }

        /// <summary>
        /// 创建家具预制体
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<PlacementRuntime> CreatePlacement(string name)
        {
            var info = GetPlacementInfo(name);

            return await CreatePlacementInternal(info);
        }
        public static async Task<Pot> CreatePot(string name)
        {
            var info = GetPotData(name);
            return await CreatePotInternal(info);
        }
        private static async Task<PlacementRuntime> CreatePlacementInternal(Room_Placement_Info info)
        {
            switch (info.second_Category)
            {
                case Placement_Second_Category.Door:
                case Placement_Second_Category.Floor:
                case Placement_Second_Category.Wallpaper:
                    var mat = await GameAssets.LoadAsyncByPath<Material>(info.res_url);
                    RoomSystem.currentRoom.SetSpecialDecoration(info.second_Category, mat, info.room_placement_id);
                    break;
                default:
                    var prefab = await GameAssets.LoadAsyncByPath<GameObject>(info.res_url);
                    var model = Instantiate(prefab, Instance.transform);
                    var placement = model.GetComponent<PlacementRuntime>();
                    if (placement != null)
                    {
                        placement.Init(info);

                    }
                    return placement;

            }
            return null;
        }
        private static async Task<Pot> CreatePotInternal(PotData data)
        {
            var prefab = await GameAssets.LoadAsyncByPath<GameObject>(data.resUrl);
            var model = Instantiate(prefab, Instance.transform);
            var pot = model.GetComponent<Pot>();
            if (pot != null)
            {
                pot.Init(data);
            }
            return pot;
        }
        public static bool TryAddGridObject(GridObject obj, Room room, string gridLayerUId, Int2 pos)
        {
            if (obj == null || room == null || gridLayerUId == null || pos == null)
            {
                Log.Error("有数据为空，添加placement失败");
                return false;
            }

            if (CheckGridObjectVaild(obj, room, gridLayerUId, pos))
            {
                if (obj is PlacementRuntime placement)
                {
                    Global_Inventory_Manager.Change_Item_Count(placement.ID, -1);
                    EvtDsp.TriggerEvt(EvtNames.ReloadPlacementData);

                }
                if (obj is Pot pot)
                {
                    Global_Inventory_Manager.Change_Item_Count(pot.info.id, -1);
                    EvtDsp.TriggerEvt(EvtNames.ReloadPlantData);
                }
                RegisterGridObject(obj, room, gridLayerUId, pos);
                AudioManager.Instance.PlayAudioByRefKey("setPlacement");
                EvtDsp.TriggerEvt<GridObject>(EvtNames.OnPutPlacement, obj);

                return true;
            }

        
            return false;

        }

        private static void RegisterGridObject(GridObject obj, Room room, string gridLayerUId, Int2 pos)
        {
            obj.data.position = pos;
            obj.data.gridLayerUID = gridLayerUId;

            if (!runtimeDic.TryAdd(obj.UId, obj))
            {
                Log.Error("PLacementSystem:添加失败，可能是Placement已存在");
                return;
            }

            HashSet<Int2> occupiedPositions = CalculateOccpiedPos(obj, pos);

            var uids = room.GridState.SetOccupied(gridLayerUId, occupiedPositions, true);

            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Occupied);

            if(obj is PlacementRuntime placement)
            {
                room.AddPlacementToRoom(placement, gridLayerUId);
            }
            if(obj is Pot pot)
            {
                room.AddPotToRoom(pot, gridLayerUId);
            }
            obj.room = room;
            Global_Home_Room_Manager._instance.re_bake_navmesh();
        }

        /// <summary>
        /// 房间自动加载流程使用函数，直接注册数据并更新状态，不进行合法性检查（假设存档数据是合法的）
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="room"></param>
        /// <param name="gridLayerUId"></param>
        /// <param name="pos"></param>
        public static void RegisterGridObjectFromLoad(GridObject obj, Room room, string gridLayerUId, Int2 pos)
        {
            obj.data.position = pos;
            obj.data.gridLayerUID = gridLayerUId;

            runtimeDic[obj.UId] = obj;

            var occupied = CalculateOccpiedPos(obj, pos);
            room.GridState.SetOccupied(gridLayerUId, occupied, true);
            if (obj is PlacementRuntime placement)
            {
                room.AddPlacementToRoom(placement, gridLayerUId);
            }
            if(obj is Pot pot)
            {
                room.AddPotToRoom(pot, gridLayerUId);
            }
            obj.ApplyPositon(room);
            obj.ApplyRotation();
            obj.room = room;
        }
    

        public static bool TryMoveObject(GridObject obj, Room room, string gridLayerUId, Int2 newPos, string originalLayerUid, Int2 originalPosition)
        {
            //更新家具位置
            obj.data.position = newPos;
            obj.data.gridLayerUID = gridLayerUId;

            //更新网格状态
            //占据新位置
            HashSet<Int2> occupiedPositions = CalculateOccpiedPos(obj, newPos);
            bool canOccupy = room.GridState.CheckVaild(gridLayerUId, occupiedPositions);
            if (canOccupy)
            {
                var uids = room.GridState.SetOccupied(gridLayerUId, occupiedPositions, true);
                EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Occupied);

                //清除旧位置的占据数据，并刷新表现层
                HashSet<Int2> originalPositions = CalculateOccpiedPos(obj, originalPosition);
                var originalUids = room.GridState.SetOccupied(originalLayerUid, originalPositions, false);
                EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, originalUids, MGridState.Normal);

                if (originalLayerUid != gridLayerUId)
                {
                    SyncMovedObjectLayerCache(obj, room, originalLayerUid, gridLayerUId);
                }

                Global_Home_Room_Manager._instance.re_bake_navmesh();
            }
             return canOccupy;
        }

        private static void SyncMovedObjectLayerCache(GridObject obj, Room room, string originalLayerUid, string newLayerUid)
        {
            if (obj is PlacementRuntime placement)
            {
                room.UnbindPlacementWallVisibility(placement, originalLayerUid);

                if (room.gridLayerPlacementDic.TryGetValue(originalLayerUid, out var oldPlacements))
                {
                    oldPlacements?.Remove(placement);
                }

                if (!room.gridLayerPlacementDic.TryGetValue(newLayerUid, out var newPlacements) || newPlacements == null)
                {
                    newPlacements = new List<PlacementRuntime>();
                    room.gridLayerPlacementDic[newLayerUid] = newPlacements;
                }

                if (!newPlacements.Contains(placement))
                {
                    newPlacements.Add(placement);
                }

                room.BindPlacementWallVisibility(placement, newLayerUid);
            }

            if (obj is Pot pot)
            {
                if (room.gridLayerPotDic.TryGetValue(originalLayerUid, out var oldPots))
                {
                    oldPots?.Remove(pot);
                }

                if (!room.gridLayerPotDic.TryGetValue(newLayerUid, out var newPots) || newPots == null)
                {
                    newPots = new List<Pot>();
                    room.gridLayerPotDic[newLayerUid] = newPots;
                }

                if (!newPots.Contains(pot))
                {
                    newPots.Add(pot);
                }
            }
        }

        public static HashSet<Int2> CalculateOccpiedPos(GridObject placement, Int2 pos)
        {
            var x = placement.gridData.length;
            var y = placement.gridData.width;
            return GridUtility.CalculateOccpiedPos(x, y, pos);
        }

        public static HashSet<Int2> CalculateOccpiedPosAfterRotate(GridObject placement, Int2 pos)
        {
            var (x,y) = placement.GetRotated90Size();
            return GridUtility.CalculateOccpiedPos(x, y, pos);
        }

        public static bool DeleteGridObject(GridObject obj, Room room, string gridLayerUId, Int2 pos)
        {
            if (obj == null || room == null)
            {
                Log.Error("有数据为空，删除placement失败");
                return false;
            }
            if (obj is Pot potBlocked && PlantManager.Instance != null && PlantManager.Instance.GetPlantByPot(potBlocked) != null)
            {
                EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, "花盆里有植物时不能删除花盆，请先收获或铲除植物");
                return false;
            }
            HashSet<Int2> occupiedPositions = CalculateOccpiedPos(obj, pos);
            obj.data.position = Int2.zero;

            runtimeDic.Remove(obj.UId);

            var uids = room.GridState.SetOccupied(gridLayerUId, occupiedPositions, false);
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Normal);

            if(obj is PlacementRuntime placement)
            {
                room.RemovePlacementFromRoom(placement, gridLayerUId);
                Global_Inventory_Manager.Change_Item_Count(placement.ID, 1);
                EvtDsp.TriggerEvt(EvtNames.ReloadPlacementData);

            }
            if (obj is Pot pot)
            {
                room.RemovePotFromRoom(pot, gridLayerUId);
                Global_Inventory_Manager.Change_Item_Count(pot.info.id, 1);
                EvtDsp.TriggerEvt(EvtNames.ReloadPlantData);

            }

            Destroy(obj.gameObject);
            Global_Home_Room_Manager._instance.re_bake_navmesh();
            return true;
        }

        public static void RotatePlacement()
        {
            Global_Home_Room_Manager._instance.re_bake_navmesh();
        }

        /// <summary>
        /// 检测放置位置是否合法
        /// </summary>
        public static bool CheckGridObjectVaild(GridObject placement, Room room, string gridLayerUId, Int2 pos)
        {
            HashSet<Int2> occupiedPositions = CalculateOccpiedPos(placement, pos);
            return room.GridState.CheckVaild(gridLayerUId, occupiedPositions);
        }

        public static bool CheckGridObjectVaildAfterRotate(GridObject placement, Room room, string gridLayerUId, Int2 pos)
        {
            HashSet<Int2> occupiedPositions = CalculateOccpiedPosAfterRotate(placement, pos);
            return room.GridState.CheckVaild(gridLayerUId, occupiedPositions);
        }

        public void ClearAll()
        {
            foreach (var placement in runtimeDic.Values)
            {
                Destroy(placement.gameObject);
            }
            runtimeDic.Clear();
        }

        public void CancelAllModifacation()
        {

        }
        public void ConfirmModification()
        {
            //Upload_Data_To_Server();
            Global_Inventory_Manager._instance.Send_inventory_to_server();
        }
    }


}
