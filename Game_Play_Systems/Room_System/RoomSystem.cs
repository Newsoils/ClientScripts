using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class RoomSystem : SingletonMono<RoomSystem>
    {
        public Room_SO Room_SO;

        // RoomData system implementation goes here
        public Dictionary<string, RoomData> roomDataDic = new Dictionary<string, RoomData>();
        public Dictionary<string, RoomData> roomDataNameDic = new Dictionary<string, RoomData>();

        public List<RoomData> RoomDatas => roomDataDic.Values.ToList();

        public static Room currentRoom = null;

        public Dictionary<string, Room> roomDic = new Dictionary<string, Room>();
        public Dictionary<string, Room> roomNameDic = new Dictionary<string, Room>();

        public List<Room> rooms = new List<Room>();

        public Action Update_Data_From_Server;
        public Action Upload_Data_To_Server;

        public bool canSwitchRoom = true;

        string testMsg = "{\"rooms\":[{\"roomType\":2,\"roomName\":\"浴室\",\"roomUID\":\"291fa79b-74f9-46f0-a11f-7cf8526cace9\",\"placementDatas\":[]}," +
            "{\"roomType\":0,\"roomName\":\"卧室\",\"roomUID\":\"5df5ff7a-008c-4639-a850-085615c8fa49\",\"placementDatas\":[]},{\"roomType\":1," +
            "\"roomName\":\"客厅\",\"roomUID\":\"09a813bb-da54-4987-a5ee-7b579fcd789a\",\"placementDatas\":[{\"UID\":\"f19a603b-015b-4743-937c-47c0b876fd20\"," +
            "\"name\":\"原木简易床\",\"placementId\":10070,\"position\":{\"x\":2,\"y\":8},\"rotation\":\"Deg0\",\"gridLayerUID\":\"7ee20510-310b-4d76-bbd0-32be9f8acc86\",\"subInstanceIds\":[]}]}]}";

        void Start()
        {
            if (Room_SO == null)
            {
                Debug.LogError("Room_SO not assigned in RoomSystem");
                return;
            }
            else
            {
                Debug.Log("Room_SO assigned,RoomSystem Init RoomStart");
            }

            //从配置中读取房间数据
            foreach (var room in Room_SO.roomConfigs)
            {
                RoomData roomData = new RoomData(room.RoomType, room.RoomName);
                roomDataDic[roomData.roomUID] = roomData;
                roomDataNameDic[roomData.roomName] = roomData;
            }

            //读取网格大小数据
            GenerateGridData();

            BindRoomMonos();

            //var roomSaveData = Serialization_Provider.DeserializeObject<RoomSaveData>(testMsg);
            //if (roomSaveData != null)
            //{
            //    if (roomSaveData != null) ResetStateFromData(roomSaveData);
            //}

            SwitchRoom(RoomType.LivingRoom);
        }

        private void GenerateGridData()
        {
            foreach (var data in Room_SO.sizes)
            {
                GridLayer layer = new GridLayer(data.id, data.layer_Name, data.layer_UId, data.layerType, data.width, data.height);
                roomDataNameDic.TryGetValue(data.room_Name, out var roomData);
                if (roomData == null) continue;
                roomData.GridState.AddLayer(layer);
            }
        }


        private void BindRoomMonos()
        {
            rooms = GetComponentsInChildren<Room>(true).ToList();

            foreach (var room in rooms)
            {
                if (!roomDataNameDic.TryGetValue(room.RoomName, out var data))
                {
                    Log.Error($"[RoomSystem] No RoomData for {room.RoomName}");
                    continue;
                }

                // 类型校验（非常重要）
                if (room.RoomType != data.roomType)
                {
                    Log.Error($"[RoomSystem] Room type mismatch: {room.RoomName}");
                    continue;
                }
                room.Init(data);
                roomDic[data.roomUID] = room;
                roomNameDic[data.roomName] = room;
            }

            Debug.Log("[RoomSystem] BindRoomMonos finished");
        }


        /// <summary>
        /// 从数据中加载房间状态（异步）。调用方务必 <c>await</c> 完成后再执行上传等依赖场景就绪的逻辑。
        /// 内部会在全部家具/花盆创建后 <c>await InitPlant</c>（若已注册）。
        /// </summary>
        public async Task ResetStateFromData(RoomSaveData saveData)
        {
            if (saveData == null || saveData.rooms == null)
            {
                Debug.LogError("SaveData invalid");
                return;
            }

            // ⭐ STEP 1：清空所有运行时家具
            GridObjectSystem.runtimeDic.Clear();

            foreach (var room in rooms)
            {
                room.ClearAllPlacements(); // 你需要实现
                room.GridState.ClearAll();
            }

            roomDataDic.Clear();
            roomDataNameDic.Clear();


            //这里如果是首次会有问题，roomUID会被覆盖
            // STEP 2：覆盖 RoomData
            foreach (var loadedRoom in saveData.rooms)
            {
                RoomData roomData = new RoomData(loadedRoom.roomType, loadedRoom.roomName, loadedRoom.roomUID);
                roomDataDic.Add(roomData .roomUID,roomData);
                roomDataNameDic.Add(roomData.roomName, roomData);
                // 覆盖 placement 数据（深拷贝更安全）
                //roomData.placementDatas = loadedRoom.placementDatas;
                //roomData.potDatas = loadedRoom.potDatas;
            }

            GenerateGridData();
            BindRoomMonos();
            //STEP 3：重建所有 PlacementRuntime和Pot
            foreach (var roomData in saveData.rooms)
            {
                if (!roomDic.TryGetValue(roomData.roomUID, out var roomMono))
                    continue;

                foreach (var pData in roomData.placementDatas)
                {
                    var runtime = await GridObjectSystem.CreatePlacement(pData.placementId);
                    if (runtime == null)
                        continue;

                    // ⭐ 恢复数据（关键）
                    runtime.data.position = pData.position;
                    runtime.data.rotation = pData.rotation;
                    runtime.data.gridLayerUID = pData.gridLayerUID;
                    runtime.info =
                        GridObjectSystem.GetPlacementInfo(pData.placementId);

                    //注册到系统（不要走 TryAdd）
                    GridObjectSystem.RegisterGridObjectFromLoad(
                        runtime,
                        roomMono,
                        pData.gridLayerUID,
                        pData.position);
                }
                foreach (var potData in roomData.potDatas)
                {
                    var runtime = await GridObjectSystem.CreatePot(potData.name);
                    if (runtime == null)
                        continue;
                    runtime.data.UID = potData.UID;
                    GridObjectSystem.RegisterGridObjectFromLoad(runtime, roomMono, potData.gridLayerUID, potData.position);
                }
            }

            // 等花盆场景与植物数据就绪（PlantManager.LoadPlant 为 async Task，只应在全部 Pot 注册后调用一次）
            var initPlantTask = EvtDsp.ReturnEvt<Task>(EvtNames.InitPlant);
            if (initPlantTask != null)
                await initPlantTask;

            ResetRoomObjectRenderer();

            //STEP 4：统一 rebake（非常重要）
            Global_Home_Room_Manager._instance.re_bake_navmesh();
            SwitchRoom(RoomType.LivingRoom);

        }

        public bool TryGetRoomData(string roomUID, out RoomData roomData)
        {
            return roomDataDic.TryGetValue(roomUID, out roomData);
        }

        public bool TryGetRoomDataByName(string roomName, out RoomData roomData)
        {
            return roomDataNameDic.TryGetValue(roomName, out roomData);
        }

        public bool TryGetRoom(string roomUID, out Room room)
        {
            return roomDic.TryGetValue(roomUID, out room);
        }

        public bool TryGetRoomByName(string roomName, out Room room)
        {
            return roomNameDic.TryGetValue(roomName, out room);
        }

        public void SwitchRoom(Room room)
        {
            if (roomDataDic.TryGetValue(room.RoomUID, out _))
            {
                currentRoom = room;
                room.isActive = true;
                room.ActiveRoom(true);
                foreach (var r in rooms)
                {
                    if (r != room)
                    {
                        r.isActive = false;
                        r.ActiveRoom(false);
                    }
                }
                CameraManager.Instance.ChangeRoom(room.RoomName);
                EvtDsp.TriggerEvt<Room>(EvtNames.SwitchRoom, room);
            }
            ResetRoomObjectRenderer();
        }

        public void SwitchRoomByName(string roomName)
        {
            if (roomNameDic.TryGetValue(roomName, out var room))
            {
                SwitchRoom(room);
            }
        }

        public void SwitchRoom(string roomUID)
        {
            if (roomDic.TryGetValue(roomUID, out var room))
            {
                SwitchRoom(room);
            }
        }
        public void SwitchRoom(RoomType roomType)
        {
            var room = rooms.Where(r => r.RoomType == roomType).FirstOrDefault();
            if (room != null)
            {
                SwitchRoom(room);
            }
        }


        public static Room GetRoomByFloorPosition(Vector3 position)
        {
            foreach (var room in Instance.rooms)
            {
                if (room.Is_FloorPosition_InRoom(position)) return room;
            }
            return null;
        }

        /// <summary>
        /// 清除当前房间的所有家具和花盆，并刷新表现
        /// </summary>
        public void Clear_All_Placement()
        {
            if (currentRoom == null)
            {
                Debug.LogWarning("[RoomSystem] No current room to clear");
                return;
            }

            // 1. 清空运行时字典并收集需要处理的对象
            var uidsToRemove = new List<string>(GridObjectSystem.runtimeDic.Keys);

            // 2. 清除当前房间的家具和花盆表现
            currentRoom.ClearAllPlacements();
            currentRoom.ClearAllPots();

            // 3. 清空运行时字典
            foreach (var uid in uidsToRemove)
            {
                GridObjectSystem.runtimeDic.Remove(uid);
            }

            // 4. 清空当前房间的网格占据状态（保留网格结构）
            currentRoom.GridState.ClearAllOccupied();

            // 5. 获取所有网格UID并触发刷新事件
            var allGridUids = currentRoom.GridState.GetAllGridUids();
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, allGridUids, MGridState.Normal);

            // 6. 重新烘焙导航网格
            Global_Home_Room_Manager._instance.re_bake_navmesh();

            Debug.Log($"[RoomSystem] Cleared all placements in room: {currentRoom.RoomName}");
        }

        /// <summary>
        /// 关掉其他房间里面的Placement和Pot的Renderer
        /// </summary>
        public void ResetRoomObjectRenderer()
        {
            foreach(var room in rooms)
            {
                if(room != currentRoom)
                {
                    foreach(var placment in room.placements)
                    {
                        placment.SetRenderState(false);
                    }
                    foreach(var pot in room.pots)
                    {
                        pot.SetRenderState(false);
                    }
                }
                else
                {
                    foreach(var placement in room.placements)
                    {
                        placement.SetRenderState(true);
                    }
                    foreach(var pot in room.pots)
                    {
                        pot.SetRenderState(true);
                    }
                }
            }
        }
    }
}