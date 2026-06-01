using UnityEngine;

/// <summary>
/// 已废弃：NPC 消息收发已合并到 <see cref="Global_Game_Data_Sync_Receiver"/>。
/// 请从 <c>NPC_Manager</c> 预制体上移除此组件。
/// </summary>
//[System.Obsolete("NPC 网络收发已合并到 Global_Game_Data_Sync_Receiver，请移除此组件。", false)]
//public sealed class NPC_Receiver : MonoBehaviour
//{
//    private NPCManager _manager;
//    private NetWork_Center_WSS _networkCenter;
//    private string receiver_name = "NPC_Receiver";

//    #region Unity Life Cycle

//    private void Start()
//    {
//        _manager = GetComponent<NPCManager>();
//        if (_manager == null)
//        {
//            Debug.LogError("NPC_Receiver: NPCManager not found on GameObject");
//            return;
//        }

//        _networkCenter = NetWork_Center_WSS.instance;
//        if (_networkCenter == null)
//        {
//            Debug.LogError("NPC_Receiver: NetworkCenter not initialized");
//            return;
//        }

//        BindUnityEvents();
//        RegisterToNetwork();

//        Log.Info($"NPC_Receiver initialized on {_manager.gameObject.name}");
//    }

//    private void OnDestroy()
//    {
//        UnbindUnityEvents();
//        UnregisterFromNetwork();
//    }

//    #endregion

//    #region Event Binding (C# <-> Game Logic)

//    private void BindUnityEvents()
//    {
//        _manager.on_Meet_NPC.AddListener(OnMeetNpc);
//        _manager.on_Give_Gift_ToNPC.AddListener(OnGiveGiftToNpc);
//        _manager.ask_Single_NPC_Data.AddListener(OnAskSingleNpcData);
//        _manager.ask_All_NPC_Data.AddListener(OnAskAllNpcData);
//    }

//    private void UnbindUnityEvents()
//    {
//        _manager.on_Meet_NPC.RemoveListener(OnMeetNpc);
//        _manager.on_Give_Gift_ToNPC.RemoveListener(OnGiveGiftToNpc);
//        _manager.ask_Single_NPC_Data.RemoveListener(OnAskSingleNpcData);
//        _manager.ask_All_NPC_Data.RemoveListener(OnAskAllNpcData);
//    }

//    #endregion

//    #region UnityEvent Callbacks (Client -> Server)

//    /// <summary>
//    /// 遇到NPC
//    /// </summary>
//    /// <param name="json"></param>
//    private void OnMeetNpc(string json)
//    {
//        SendMsg(
//            action: "Meet_NPC_Event",
//            msgId: 1,
//            detail: json
//        );
//    }

//    /// <summary>
//    /// 给NPC送礼物
//    /// </summary>
//    /// <param name="json"></param>
//    private void OnGiveGiftToNpc(string json)
//    {
//        SendMsg(
//            action: "Give_Gift_To_NPC",
//            msgId: 2,
//            detail: json
//        );
//    }

//    /// <summary>
//    /// 请求单个NPC数据
//    /// </summary>
//    /// <param name="jsonNpcId"></param>
//    private void OnAskSingleNpcData(string jsonNpcId)
//    {
//        SendMsg(
//            action: "Get_Player_NPC_Data",
//            msgId: 3,
//            detail: jsonNpcId
//        );
//    }

//    /// <summary>
//    /// 请求所有NPC数据
//    /// </summary>
//    private void OnAskAllNpcData()
//    {
//        SendMsg(
//            action: "Get_All_NPC_Data",
//            msgId: 4,
//            detail: string.Empty
//        );
//    }

//    #endregion

//    #region Network Send

//    private void SendMsg(string action, int msgId, string detail)
//    {
//        if (_networkCenter == null)
//            return;

//        Network_Msg msg = new Network_Msg
//        {
//            player_id = "Player_Client",
//            msg_id = msgId,
//            sender = "Client",
//            action_target = "Player_Server",
//            action = action,
//            detail_info = detail,
//            _sending_mode = Msg_Sending_Mode.Client_to_Server
//        };

//        _networkCenter.send_via_wss(msg);
//    }

//    #endregion

//    #region IMsg_Receiver (Server -> Client)

//    public string get_receiver_name()
//    {
//        return receiver_name;
//    }

//    public void receive_msg(Network_Msg msg)
//    {
//        if (_manager == null)
//            return;
//        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", receiver_name);
//        switch (msg.action)
//        {
//            case "NPC_Send_Gift_TO_Player":
//                _manager.Receive_NPC_Gift_TO_Player(msg.detail_info);
//                break;

//            case "Response_Player_NPC_Data":
//                _manager.Receive_NPC_Data(msg.detail_info);
//                break;

//            case "Response_All_NPC_Data":
//                _manager.Receive_All_NPC_Data(msg.detail_info);
//                break;

//            default:
//                Debug.LogWarning($"NPC_Receiver: Unknown msg action {msg.action}");
//                break;
//        }
//    }

//    #endregion

//    #region Network Register

//    private void RegisterToNetwork()
//    {
//        _networkCenter?.Add_Receiver(receiver_name, this);
//    }

//    private void UnregisterFromNetwork()
//    {
//        _networkCenter?.Remove_Receiver(receiver_name);
//    }

//    #endregion
//}
