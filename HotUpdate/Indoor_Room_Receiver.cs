//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Framework_Core.Network;
//using CLIP.Framework_Core.Serialization;
//using CLIP.Framework_Unity;
//using CLIP.Project_Mouse.Game_Play_System;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using UnityEngine;

//public class Indoor_Room_Receiver : MonoBehaviour, IMsg_Receiver
//{
//    public static Indoor_Room_Receiver _instance;
//    private Indoor_Room_Game_Manager _roomManager;
//    private NetWork_Center_WSS _networkCenter;
//    private Player_Social_Manager _socialManager;

//    private string ReceiverName;
//    private const string BaseReceiverName = "";

//    #region Unity Life Cycle

//    void Awake()
//    {
//        _instance = this;
//    }
//    private void Start()
//    {
//        _roomManager = GetComponent<Indoor_Room_Game_Manager>();
//        if (_roomManager == null)
//        {
//            Debug.LogError($"{BaseReceiverName}: Room_Manager not found");
//            return;
//        }

//        // 使用游戏对象名称作为接收器名称的一部分，确保唯一性
//        ReceiverName = $"{BaseReceiverName}_{gameObject.name}";

//        _networkCenter = NetWork_Center_WSS.instance;
//        if (_networkCenter == null)
//        {
//            Debug.LogError($"{BaseReceiverName}: NetworkCenter not initialized");
//            return;
//        }

//        _socialManager = Player_Social_Manager._instance;

//        BindUnityEvents();
//        RegisterToNetwork();

//        Debug.Log($"{BaseReceiverName} initialized on {gameObject.name}");
//    }

//    private void OnDestroy()
//    {
//        //UnbindUnityEvents();
//        //UnregisterFromNetwork();
//    }

//    #endregion

//    #region UnityEvent Binding

//    private void BindUnityEvents()
//    {
//        if (_roomManager != null)
//        {
//            _roomManager._update_data_from_server.AddListener(UpdateDataFromServer);
//            _roomManager._upload_data_to_server.AddListener(UploadDataToServer);
//        }
//    }

//    private void UnbindUnityEvents()
//    {
//        if (_roomManager != null)
//        {
//            _roomManager._update_data_from_server.RemoveListener(UpdateDataFromServer);
//            _roomManager._upload_data_to_server.RemoveListener(UploadDataToServer);
//        }
//    }

//    #endregion

//    #region Network Send Helpers

//    private void SendMsg(string action, string detail)
//    {
//        if (_networkCenter == null || !_networkCenter._connect_to_player_server)
//            return;

//        Network_Msg msg = new Network_Msg
//        {
//            sender = ReceiverName,
//            action_target = "Player_Server",
//            action = action,
//            detail_info = detail,
//            _sending_mode = Msg_Sending_Mode.Client_to_Server
//        };

//        _networkCenter.send_via_wss(msg);
//    }

//    #endregion

//    #region Client -> Server Methods

//    private void UpdateDataFromServer()
//    {
//        if (_roomManager == null)
//        {
//            Debug.Log($"{BaseReceiverName}: Room_Manager is not initialized");
//            return;
//        }

//        bool isLoadFriend = false;
//        string friendName = string.Empty;

//        if (_socialManager != null)
//        {
//            friendName = _socialManager._next_visit_room_friend_name;
//            isLoadFriend = _socialManager._on_visit_friend_room;
//        }

//        if (isLoadFriend)
//        {
//            Debug.Log($"on visit friend roomData - RoomName = {ReceiverName}");

//            // 发送JSON数组：[friendName, roomDataKey]
//            string roomDataKey = $"Indoor_Data_{ReceiverName}";
//            string jsonArray = $"[\"{friendName}\",\"{roomDataKey}\"]";
//            SendMsg("Get_Friend_Data", jsonArray);
//            return;
//        }

//        Debug.Log($"{ReceiverName} Update_Data_From_Server");
//        SendMsg("Get_Data", $"Indoor_Data_{ReceiverName}");
//    }

//    private void UploadDataToServer()
//    {
//        if (_roomManager == null || _roomManager._level_info == null)
//        {
//            Debug.Log($"{BaseReceiverName}: RoomData data is not initialized");
//            return;
//        }

//        // 序列化房间数据
//        string serializedData = Serialization_Provider.SerializeObject(_roomManager._level_info);

//        List<string> data = new List<string>() { "Indoor_Data_" + ReceiverName, serializedData };

//        string lastData = Serialization_Provider.SerializeObject(data);
//        SendMsg("Save_Data", lastData);
//    }

//    #endregion

//    #region IMsg_Receiver (Server -> Client)

//    public string get_receiver_name()
//    {
//        return ReceiverName;
//    }

//    public void receive_msg(Network_Msg msg)
//    {
//        if (_roomManager == null)
//            return;

//        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", ReceiverName);

//        switch (msg.action)
//        {
//            case "Response_Data":
//                HandleResponseData(msg.detail_info);
//                break;

//            default:
//                Debug.LogWarning($"{BaseReceiverName}: Unknown action {msg.action}");
//                break;
//        }

//        Msg_Dispatcher._instance.set_locking(false);
//        Debug.Log($"in {BaseReceiverName} receive_msg_OK - Msg_Dispatcher.Instance._is_locking set to false");
//    }

//    private void HandleResponseData(string detailInfo)
//    {
//        try
//        {
//            // 使用提供的JSON解析方式
//            var data = Serialization_Provider.DeserializeObject<string[]>(detailInfo);

//            //if (data != null && data.Length == 2)
//            //{
//            //    if (data[0] == "Indoor_Data")
//            //    {
//            //        roomSystem.load_current_state_from_json(data[1]);
//            //    }
//            //}
//        }
//        catch (System.Exception ex)
//        {
//            Debug.LogError($"{BaseReceiverName}: Failed to parse response data - {ex.Message}");
//        }
//    }

//    #endregion

//    #region Coroutine Helpers


//    #endregion

//    #region Network Register

//    private void RegisterToNetwork()
//    {
//        _networkCenter?.Add_Receiver(ReceiverName, this);
//    }

//    private void UnregisterFromNetwork()
//    {
//        _networkCenter?.Remove_Receiver(ReceiverName);
//    }

//    #endregion

//    #region Public Methods

//    // 提供外部调用的方法来获取房间数据
//    public void RequestRoomData()
//    {
//        UpdateDataFromServer();
//    }

//    // 提供外部调用的方法来上传房间数据
//    public void RequestUploadRoomData()
//    {
//        UploadDataToServer();
//    }

//    #endregion
//}