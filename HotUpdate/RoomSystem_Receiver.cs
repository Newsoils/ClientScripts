using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomSystem_Receiver : MonoBehaviour, IMsg_Receiver
{
    public static RoomSystem_Receiver _instance;
    private RoomSystem roomSystem;
    private NetWork_Center_WSS _networkCenter;
    //private Player_Social_Manager _socialManager;
    private string ReceiverName;


    public void OnSceneLoaded(Scene _s, LoadSceneMode _m)
    {
        if(SceneLoadHelper.IsMainScene)
        {
            RequestRoomData();
        }
    }


    #region Unity Life Cycle

    void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        roomSystem = GetComponent<RoomSystem>();
        if (roomSystem == null)
        {
            Debug.LogError($"RoomSystem not found");
            return;
        }

        // 使用游戏对象名称作为接收器名称的一部分，确保唯一性
        ReceiverName = $"{gameObject.name}";

        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError($"{ReceiverName}: NetworkCenter not initialized");
            return;
        }

        //_socialManager = Player_Social_Manager.Instance;

        BindEvents();
        RegisterToNetwork();

        UpdateDataFromServer();

        Debug.Log($"{ReceiverName} initialized on {gameObject.name}");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindUnityEvents();
        UnregisterFromNetwork();
    }

    

    #endregion

    #region UnityEvent Binding

    private void BindEvents()
    {
        if (roomSystem != null)
        {
            roomSystem.Update_Data_From_Server += UpdateDataFromServer;
            roomSystem.Upload_Data_To_Server += UploadDataToServer;
        }
    }

    private void UnbindUnityEvents()
    {
        if (roomSystem != null)
        {
            roomSystem.Update_Data_From_Server -= UpdateDataFromServer;
            roomSystem.Upload_Data_To_Server -= UploadDataToServer;
        }
    }

    #endregion

    #region Network Send Helpers

    private void SendMsg(string action, string detail)
    {
        if (_networkCenter == null || !_networkCenter._connect_to_player_server)
            return;

        Network_Msg msg = new Network_Msg
        {
            sender = ReceiverName,
            action_target = "Player_Server",
            action = action,
            detail_info = detail,
            _sending_mode = Msg_Sending_Mode.Client_to_Server
        };

        _networkCenter.send_via_wss(msg);
    }

    #endregion

    #region Client -> Server Methods

    private void UpdateDataFromServer()
    {
        if (roomSystem == null)
        {
            Debug.Log($"{ReceiverName}: Room_Manager is not initialized");
            return;
        }

        //bool isLoadFriend = false;
        //string friendName = string.Empty;

        //if (_socialManager != null)
        //{
        //    friendName = _socialManager._next_visit_room_friend_name;
        //    isLoadFriend = _socialManager._on_visit_friend_room;
        //}

        //if (isLoadFriend)
        //{
        //    Debug.Log($"on visit friend roomSaveData - RoomName = {ReceiverName}");

        //    // 发送JSON数组：[friendName, roomDataKey]
        //    string roomDataKey = $"{ReceiverName}";
        //    string jsonArray = $"[\"{friendName}\",\"{roomDataKey}\"]";
        //    SendMsg("Get_Friend_Data", jsonArray);
        //    return;
        //}

        Debug.Log($"{ReceiverName} Ask_data_from_server");
        SendMsg("Get_Data", "Room_Data");
    }

    private void UploadDataToServer()
    {
        if (RoomSystem.Instance == null || RoomSystem.Instance.RoomDatas == null)
        {
            Debug.LogError("RoomSystem not ready");
            return;
        }

        // 构建存档对象
        RoomSaveData saveData = new RoomSaveData
        {
            rooms = RoomSystem.Instance.RoomDatas
        };

        // 序列化
        string serializedData = Serialization_Provider.SerializeObject(saveData);

        List<string> data = new() { "Room_Data", serializedData };

        string lastData = Serialization_Provider.SerializeObject(data);

        SendMsg("Save_Data", lastData);
    }

    #endregion

    #region IMsg_Receiver (Server -> Client)

    public string get_receiver_name()
    {
        return ReceiverName;
    }

    public void receive_msg(Network_Msg msg)
    {
        if (roomSystem == null)
            return;

        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", ReceiverName);

        switch (msg.action)
        {
            case "Response_Data":

                HandleResponseData(msg.detail_info);
                break;

            default:
                Debug.LogWarning($"{ReceiverName}: Unknown action {msg.action}");
                break;
        }

        Msg_Dispatcher._instance.set_locking(false);
        Debug.Log($"in {ReceiverName} receive_msg_OK - Msg_Dispatcher.Instance._is_locking set to false");
    }


    //这里以后和服务器上的代码都要修改成 根据是不是首次分开处理，或者另写个不包含roomUID的数据结构专门处理首次的默认数据
    //上传首次的时候不上传UID（RoomSystem_Editor.cs )
    //否则Room的UID不正确（现在oomSystem.ResetStateFromData 会直接覆盖UID

    private void HandleResponseData(string detailInfo)
    {
        try
        {
            var data = Serialization_Provider.DeserializeObject<string[]>(detailInfo);

            if (data == null || data.Length < 2)
                return;

            string dataKey = data[0];
            string dataJson = data[1];

            if (dataKey == "Room_Data")
            {
                if (string.IsNullOrEmpty(dataJson))
                {
                    Debug.Log($"{ReceiverName}: Room_Data is empty, requesting default data for first-time player");
                    RequestDefaultRoomData();
                    return;
                }

                var roomSaveData = Serialization_Provider.DeserializeObject<RoomSaveData>(dataJson);
                if (roomSaveData != null && roomSaveData.rooms != null && roomSaveData.rooms.Count > 0)
                {
                    ApplyRoomSaveDataFromServerAsync(roomSaveData);
                }
                else
                {
                    Debug.Log($"{ReceiverName}: Room_Data parsed but empty, requesting default data");
                    RequestDefaultRoomData();
                }
            }
            //这里实际上目前不会触发
            //else if (dataKey == "Room_Default_Data")
            //{
            //    if (string.IsNullOrEmpty(dataJson))
            //    {
            //        Debug.LogWarning($"{ReceiverName}: No default room data on server, falling back to GenerateDefaultRoomData");
            //        return;
            //    }

            //    var defaultData = Serialization_Provider.DeserializeObject<RoomSaveData>(dataJson);
            //    if (defaultData != null && defaultData.rooms != null && defaultData.rooms.Count > 0)
            //    {
            //        Debug.Log($"{ReceiverName}: Applying default room data ({defaultData.rooms.Count} rooms)");
            //        ApplyRoomSaveDataFromServerAsync(defaultData);
            //        UploadDataToServer();
            //    }
            //    else
            //    {
            //        Debug.LogWarning($"{ReceiverName}: Default data invalid, falling back to GenerateDefaultRoomData");
            //    }
            //}
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ReceiverName}: Failed to parse response data - {ex.Message}");
        }
    }

    private void RequestDefaultRoomData()
    {
        Debug.Log($"{ReceiverName}: Requesting Room_Default_Data from server");
        SendMsg("Get_Data", "Room_Default_Data");
    }

    private async void ApplyRoomSaveDataFromServerAsync(RoomSaveData roomSaveData)
    {
        try
        {
            await roomSystem.ResetStateFromData(roomSaveData);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ReceiverName}: ResetStateFromData failed - {ex.Message}");
        }
    }

    #endregion


    #region Network Register

    private void RegisterToNetwork()
    {
        _networkCenter?.Add_Receiver(ReceiverName, this);
    }

    private void UnregisterFromNetwork()
    {
        _networkCenter?.Remove_Receiver(ReceiverName);
    }

    #endregion

    #region Public Methods

    // 提供外部调用的方法来获取房间数据
    public void RequestRoomData()
    {
        UpdateDataFromServer();
    }

    // 提供外部调用的方法来上传房间数据
    public void RequestUploadRoomData()
    {
        UploadDataToServer();
    }

    /// <summary>
    /// 将当前场上所有房间的家具布局作为「新玩家默认家具」上传到服务器。
    /// 服务器收到 Room_Default_Data 后应存为模板，新玩家首次登录时下发此数据。
    /// 仅在编辑器运行时由 Inspector 按钮调用。
    /// </summary>
    public void UploadDefaultRoomData()
    {
        if (roomSystem == null || roomSystem.RoomDatas == null)
        {
            Debug.LogError($"{ReceiverName}: RoomSystem not ready, cannot upload default data");
            return;
        }

        RoomSaveData saveData = new RoomSaveData
        {
            rooms = roomSystem.RoomDatas
        };

        string serializedData = Serialization_Provider.SerializeObject(saveData);
        List<string> data = new() { "Room_Default_Data", serializedData };
        string lastData = Serialization_Provider.SerializeObject(data);

        SendMsg("Save_Data", lastData);
        Debug.Log($"{ReceiverName}: Default room data uploaded. Rooms={saveData.rooms.Count}");
    }

    #endregion
}