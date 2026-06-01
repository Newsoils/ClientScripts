using System;
using System.Collections.Generic;
using System.IO;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSystem_Receiver : SingletonMono<RoomSystem_Receiver>
{   
    protected override bool PersistAcrossScenes => true;

    private string ReceiverName;

    #region Unity Life Cycle

    private void Start()
    {

        // 使用游戏对象名称作为接收器名称的一部分，确保唯一性
        ReceiverName = $"{gameObject.name}";

        //_socialManager = Player_Social_Manager.Instance;

        BindEvents();

        if (Global_Game_Manager.Instance != null)
            Global_Game_Manager.Instance.RegisterApplyRoomSaveDataFromServerHandler(ApplyRoomSaveDataFromServerAsync);

        Debug.Log($"{ReceiverName} initialized on {gameObject.name}");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnbindUnityEvents();
    }

    

    #endregion

    #region UnityEvent Binding

    private void BindEvents()
    {
        if (RoomSystem.Instance != null)
        {
            RoomSystem.Instance.Upload_Data_To_Server += UploadDataToServer;
        }
    }

    private void UnbindUnityEvents()
    {
        if (RoomSystem.Instance != null)
        {
            RoomSystem.Instance.Upload_Data_To_Server -= UploadDataToServer;
        }
    }

    #endregion


    #region Client -> Server Methods
    private void UploadDataToServer()
    {
        if (RoomSystem.Instance == null || RoomSystem.Instance.RoomDatas == null)
        {
            Debug.LogError("RoomSystem not ready");
            return;
        }

        var req = new Cmd.ChangeRoomDataReq();
        foreach (var room in RoomSystem.Instance.RoomDatas)
        {
            string serializedData = Serialization_Provider.SerializeObject(room);
            req.RoomData.Add(serializedData);
        }
        Debug.Log("UploadDataToServer: " + req.RoomData.Count + " rooms=" + req.RoomData);
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(req);
    }

    #endregion

    /// <summary>1308 GetRoomDataRes：解析为 <see cref="RoomSaveData"/> 暂存于 <see cref="Global_Game_Manager"/>，待 <see cref="RoomSystem"/> 初始化后再应用。</summary>
    public static void ApplyGetRoomDataRes(Cmd.GetRoomDataRes res)
    {
        if (res == null)
            return;

        //这里以后和服务器上的代码都要修改成 根据是不是首次分开处理，或者另写个不包含roomUID的数据结构专门处理首次的默认数据
        //上传首次的时候不上传UID（RoomSystem_Editor.cs )
        //否则Room的UID不正确（现在oomSystem.ResetStateFromData 会直接覆盖UID

        var roomSaveData = new RoomSaveData();
        if (res.RoomData == null || res.RoomData.Count == 0)
        {
            roomSaveData = LoadDefaultRoomConfig();
        }
        else
        {
            Debug.Log("[RoomSystem_Receiver] ApplyGetRoomDataRes → stash Global_Game_Manager, rooms=" + res.RoomData.Count);
            foreach (var roomData in res.RoomData)
            {
                roomSaveData.rooms.Add(Serialization_Provider.DeserializeObject<RoomData>(roomData));
            }
        }
        Global_Game_Manager.Instance?.SetPendingRoomSaveDataFromServer(roomSaveData);
    }

    /// <summary>
    /// 1310 ChangeRoomDataRes：由 <see cref="Global_Game_Data_Sync_Receiver"/> 注册并转发。
    /// </summary>
    public void ApplyChangeRoomDataRes(Cmd.ChangeRoomDataRes res)
    {
        if (res == null)
            return;

        if (res.RoomData != null && res.RoomData.Count > 0)
        {
            Debug.Log("ApplyChangeRoomDataRes: " + res.RoomData.Count + " rooms=" + res.RoomData);
            var roomSaveData = new RoomSaveData();
            foreach (var roomData in res.RoomData)
            {
                roomSaveData.rooms.Add(Serialization_Provider.DeserializeObject<RoomData>(roomData));
            }

            ValidateChangeRoomData(roomSaveData);
            Global_Game_Manager.Instance?.CacheRoomSaveData(roomSaveData);
        }
    }

    private void ValidateChangeRoomData(RoomSaveData roomSaveData)
    {
        if (roomSaveData == null || roomSaveData.rooms == null || roomSaveData.rooms.Count == 0)
        {
            Debug.LogWarning("[RoomSystem_Receiver] ChangeRoomDataRes room data is empty.");
            return;
        }

        if (RoomSystem.Instance == null)
        {
            Debug.LogWarning("[RoomSystem_Receiver] ChangeRoomDataRes received but RoomSystem.Instance is null; cached data only.");
            return;
        }

        var localRooms = RoomSystem.Instance.RoomDatas;
        if (localRooms == null || localRooms.Count != roomSaveData.rooms.Count)
        {
            Debug.LogWarning($"[RoomSystem_Receiver] Room count mismatch. local={localRooms?.Count ?? 0}, server={roomSaveData.rooms.Count}");
            return;
        }

        foreach (var serverRoom in roomSaveData.rooms)
        {
            if (serverRoom == null || string.IsNullOrEmpty(serverRoom.roomUID))
            {
                Debug.LogWarning("[RoomSystem_Receiver] ChangeRoomDataRes contains invalid room data.");
                continue;
            }

            if (!RoomSystem.Instance.TryGetRoomData(serverRoom.roomUID, out var localRoom))
            {
                Debug.LogWarning($"[RoomSystem_Receiver] Server room not found locally. roomUID={serverRoom.roomUID}, roomName={serverRoom.roomName}");
                continue;
            }

            int localPlacementCount = localRoom.placementDatas?.Count ?? 0;
            int serverPlacementCount = serverRoom.placementDatas?.Count ?? 0;
            int localPotCount = localRoom.potDatas?.Count ?? 0;
            int serverPotCount = serverRoom.potDatas?.Count ?? 0;

            if (localPlacementCount != serverPlacementCount || localPotCount != serverPotCount ||
                localRoom.wallPlacementId != serverRoom.wallPlacementId ||
                localRoom.floorPlacementId != serverRoom.floorPlacementId ||
                localRoom.doorPlacementId != serverRoom.doorPlacementId)
            {
                Debug.LogWarning(
                    $"[RoomSystem_Receiver] Room data mismatch. room={serverRoom.roomName}, " +
                    $"placements local/server={localPlacementCount}/{serverPlacementCount}, " +
                    $"pots local/server={localPotCount}/{serverPotCount}");
            }
        }
    }

    public static RoomSaveData LoadDefaultRoomConfig()
    {
        var ta = Resources.Load<TextAsset>("Config/RoomDefaultConfig");
        if (ta == null)
        {
            Debug.LogError("[RoomSystem] RoomDefaultConfig not found in Resources/Config/");
            return null;
        }

        var saveData = JsonConvert.DeserializeObject<RoomSaveData>(ta.text);
        if (saveData == null || saveData.rooms == null || saveData.rooms.Count == 0)
        {
            Debug.LogError("[RoomSystem] RoomDefaultConfig deserialized to null or empty");
            return null;
        }

        Debug.Log($"[RoomSystem] Loaded default room config: {saveData.rooms.Count} rooms");
        return saveData;
    }

    public async void ApplyRoomSaveDataFromServerAsync(RoomSaveData roomSaveData)
    {
        try
        {
            await RoomSystem.Instance.ResetStateFromData(roomSaveData);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ReceiverName}: ResetStateFromData failed - {ex.Message}");
        }
    }

   
}
