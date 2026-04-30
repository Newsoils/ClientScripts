using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class RoomSystem : SingletonMono<RoomSystem>
    {
        public Room_SO Room_SO;

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

        string testMsg = "{\"rooms\":[{\"roomType\":2,\"roomName\":\"娴村\",\"roomUID\":\"291fa79b-74f9-46f0-a11f-7cf8526cace9\",\"placementDatas\":[]}," +
            "{\"roomType\":0,\"roomName\":\"鍗у\",\"roomUID\":\"5df5ff7a-008c-4639-a850-085615c8fa49\",\"placementDatas\":[]},{\"roomType\":1," +
            "\"roomName\":\"瀹㈠巺\",\"roomUID\":\"09a813bb-da54-4987-a5ee-7b579fcd789a\",\"placementDatas\":[{\"UID\":\"f19a603b-015b-4743-937c-47c0b876fd20\"," +
            "\"name\":\"鍘熸湪绠€鏄撳簥\",\"placementId\":10070,\"position\":{\"x\":2,\"y\":8},\"rotation\":\"Deg0\",\"gridLayerUID\":\"7ee20510-310b-4d76-bbd0-32be9f8acc86\",\"subInstanceIds\":[]}]}]}";

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

            foreach (var room in Room_SO.roomConfigs)
            {
                RoomData roomData = new RoomData(room.RoomType, room.RoomName);
                roomDataDic[roomData.roomUID] = roomData;
                roomDataNameDic[roomData.roomName] = roomData;
            }

            GenerateGridData();
            BindRoomMonos();
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
        /// 浠庢暟鎹腑鍔犺浇鎴块棿鐘舵€侊紙寮傛锛夈€傝皟鐢ㄦ柟鍔″繀 await 瀹屾垚鍚庡啀鎵ц涓婁紶绛変緷璧栧満鏅氨缁殑閫昏緫銆?
        /// </summary>
        public async Task ResetStateFromData(RoomSaveData saveData)
        {
            if (saveData == null || saveData.rooms == null)
            {
                Debug.LogError("SaveData invalid");
                return;
            }

            GridObjectSystem.runtimeDic.Clear();

            foreach (var room in rooms)
            {
                room.ClearAllPlacements();
                room.ClearAllPots();
                room.GridState.ClearAll();
            }

            roomDataDic.Clear();
            roomDataNameDic.Clear();
            roomDic.Clear();
            roomNameDic.Clear();

            foreach (var loadedRoom in saveData.rooms)
            {
                RoomData roomData = new RoomData(loadedRoom.roomType, loadedRoom.roomName, loadedRoom.roomUID);
                roomDataDic.Add(roomData.roomUID, roomData);
                roomDataNameDic.Add(roomData.roomName, roomData);

                roomData.wallPlacementId = loadedRoom.wallPlacementId;
                roomData.floorPlacementId = loadedRoom.floorPlacementId;
                roomData.doorPlacementId = loadedRoom.doorPlacementId;
            }

            GenerateGridData();
            BindRoomMonos();

            foreach (var loadedRoom in saveData.rooms)
            {
                if (!roomDic.TryGetValue(loadedRoom.roomUID, out var roomMono))
                    continue;

                if (loadedRoom.wallPlacementId.HasValue)
                {
                    var info = GridObjectSystem.GetPlacementInfo(loadedRoom.wallPlacementId.Value);
                    if (info != null)
                        roomMono.RestoreSpecialDecoration(Placement_Second_Category.Wallpaper, loadedRoom.wallPlacementId.Value, info.res_url);
                }

                if (loadedRoom.floorPlacementId.HasValue)
                {
                    var info = GridObjectSystem.GetPlacementInfo(loadedRoom.floorPlacementId.Value);
                    if (info != null)
                        roomMono.RestoreSpecialDecoration(Placement_Second_Category.Floor, loadedRoom.floorPlacementId.Value, info.res_url);
                }

                if (loadedRoom.doorPlacementId.HasValue)
                {
                    var info = GridObjectSystem.GetPlacementInfo(loadedRoom.doorPlacementId.Value);
                    if (info != null)
                        roomMono.RestoreSpecialDecoration(Placement_Second_Category.Door, loadedRoom.doorPlacementId.Value, info.res_url);
                }
            }

            foreach (var loadedRoom in saveData.rooms)
            {
                if (!roomDic.TryGetValue(loadedRoom.roomUID, out var roomMono))
                    continue;

                if (loadedRoom.placementDatas != null)
                {
                    foreach (var pData in loadedRoom.placementDatas)
                    {
                        var runtime = await GridObjectSystem.CreatePlacement(pData.placementId);
                        if (runtime == null)
                            continue;

                        runtime.data.UID = pData.UID;
                        runtime.data.name = pData.name;
                        runtime.data.placementId = pData.placementId;
                        runtime.data.position = pData.position;
                        runtime.data.rotation = pData.rotation;
                        runtime.data.gridLayerUID = pData.gridLayerUID;
                        runtime.data.subInstanceIds = pData.subInstanceIds != null
                            ? new List<string>(pData.subInstanceIds)
                            : new List<string>();
                        runtime.info = GridObjectSystem.GetPlacementInfo(pData.placementId);

                        GridObjectSystem.RegisterGridObjectFromLoad(
                            runtime,
                            roomMono,
                            pData.gridLayerUID,
                            pData.position);
                    }
                }

                if (loadedRoom.potDatas != null)
                {
                    foreach (var potData in loadedRoom.potDatas)
                    {
                        var runtime = await GridObjectSystem.CreatePot(potData.name);
                        if (runtime == null)
                            continue;

                        runtime.data.UID = potData.UID;
                        runtime.data.name = potData.name;
                        runtime.data.placementId = potData.placementId;
                        runtime.data.position = potData.position;
                        runtime.data.rotation = potData.rotation;
                        runtime.data.gridLayerUID = potData.gridLayerUID;
                        runtime.data.subInstanceIds = potData.subInstanceIds != null
                            ? new List<string>(potData.subInstanceIds)
                            : new List<string>();

                        GridObjectSystem.RegisterGridObjectFromLoad(
                            runtime,
                            roomMono,
                            potData.gridLayerUID,
                            potData.position);
                    }
                }
            }

            var initPlantTask = EvtDsp.ReturnEvt<Task>(EvtNames.InitPlant);
            if (initPlantTask != null)
                await initPlantTask;

            ResetRoomObjectRenderer();
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

        public void Clear_All_Placement()
        {
            if (currentRoom == null)
            {
                Debug.LogWarning("[RoomSystem] No current room to clear");
                return;
            }

            var uidsToRemove = new List<string>(GridObjectSystem.runtimeDic.Keys);

            currentRoom.ClearAllPlacements();
            currentRoom.ClearAllPots();

            foreach (var uid in uidsToRemove)
            {
                GridObjectSystem.runtimeDic.Remove(uid);
            }

            currentRoom.GridState.ClearAllOccupied();

            var allGridUids = currentRoom.GridState.GetAllGridUids();
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, allGridUids, MGridState.Normal);

            Global_Home_Room_Manager._instance.re_bake_navmesh();

            Debug.Log($"[RoomSystem] Cleared all placements in room: {currentRoom.RoomName}");
        }

        public void ResetRoomObjectRenderer()
        {
            foreach (var room in rooms)
            {
                if (room != currentRoom)
                {
                    foreach (var placement in room.placements)
                    {
                        placement.SetRenderState(false);
                    }
                    foreach (var pot in room.pots)
                    {
                        pot.SetRenderState(false);
                    }
                }
                else
                {
                    foreach (var placement in room.placements)
                    {
                        placement.SetRenderState(true);
                    }
                    foreach (var pot in room.pots)
                    {
                        pot.SetRenderState(true);
                    }
                }
            }
        }
    }
}
