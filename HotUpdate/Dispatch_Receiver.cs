using System.Collections.Generic;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel.Dispatch;
using UnityEngine;

public class Dispatch_Receiver : SingletonMono<Dispatch_Receiver>, IMsg_Receiver
{
    private Dispatch_Manager dispatchManager;
    private NetWork_Center_WSS _networkCenter;

    private const string ReceiverName = "Dispatch_Receiver";
    private const string DataKey = "Dispatch_Info";

    #region Unity Life Cycle

    private void Start()
    {
        dispatchManager = GetComponent<Dispatch_Manager>();
        if (dispatchManager == null)
        {
            Debug.LogError($"{ReceiverName}: DispatchMgr not found");
            return;
        }

        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError($"{ReceiverName}: NetworkCenter not initialized");
            return;
        }


        BindUnityEvents();
        RegisterToNetwork();

        // 如果已连接服务器，则更新调度信息
        if (_networkCenter._connect_to_player_server)
        {
            UpdateDispatchInfoFromServer();
        }

        Debug.Log($"{ReceiverName} initialized on {gameObject.name}");
    }

    private void OnDestroy()
    {
        UnbindUnityEvents();
        UnregisterFromNetwork();
    }

    #endregion

    #region UnityEvent Binding

    private void BindUnityEvents()
    {
        if (dispatchManager != null)
        {
            dispatchManager._update_dispatch_info_from_server.AddListener(UpdateDispatchInfoFromServer);
            dispatchManager._upload_dispatch_info_to_server.AddListener(UploadDispatchInfoToServer);
            dispatchManager._on_get_middle_way_photo.AddListener(UploadDispatchInfoToServer);
            dispatchManager._on_finishing_dispatch.AddListener(UploadDispatchInfoToServer);
            dispatchManager._on_clear_previous_dispatch.AddListener(OnClearPreviousDispatch);
        }
    }

    private void UnbindUnityEvents()
    {
        if (dispatchManager != null)
        {
            dispatchManager._update_dispatch_info_from_server.RemoveListener(UpdateDispatchInfoFromServer);
            dispatchManager._upload_dispatch_info_to_server.RemoveListener(UploadDispatchInfoToServer);
            dispatchManager._on_get_middle_way_photo.RemoveListener(UploadDispatchInfoToServer);
            dispatchManager._on_finishing_dispatch.RemoveListener(UploadDispatchInfoToServer);
            dispatchManager._on_clear_previous_dispatch.RemoveListener(OnClearPreviousDispatch);
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

    private void UpdateDispatchInfoFromServer()
    {
        Debug.Log($"{ReceiverName}: Requesting dispatch info from server");
        SendMsg("Get_Data", DataKey);
    }

    private void UploadDispatchInfoToServer()
    {
        if (dispatchManager == null || dispatchManager._player_dispatch_state == null)
        {
            Debug.Log($"{ReceiverName}: Dispatch data is not initialized");
            return;
        }

        // 序列化调度信息
        string serializedData = Serialization_Provider.SerializeObject(dispatchManager._player_dispatch_state);

        // 发送JSON数组：[dataKey, serializedData]
        //string jsonArray = $"[\"{DataKey}\",\"{serializedData}\"]";


        List<string> data = new() { DataKey, serializedData };

        string lastData = Serialization_Provider.SerializeObject(data);


        SendMsg("Save_Data", lastData);

        Debug.Log($"{ReceiverName}: Dispatch info sent to server");
    }

    private void OnClearPreviousDispatch()
    {
        if (dispatchManager == null || dispatchManager._player_dispatch_state == null)
        {
            Debug.Log($"{ReceiverName}: Cannot clear dispatch - manager not initialized");
            return;
        }

        Debug.Log($"{ReceiverName}: Clearing previous dispatch info");

        // 重置调度状态
        dispatchManager._player_dispatch_state._current_dispatch_info = new single_dispatch_info();
        dispatchManager._player_dispatch_state.player_state = "At_Home";
        dispatchManager._player_dispatch_state.player_at_home_length_in_minute = 0;
        dispatchManager._player_dispatch_state.player_on_dispatch_length_in_minute = 0;
        dispatchManager._player_dispatch_state.last_visited_map = "";
        dispatchManager._player_dispatch_state.last_reward_info = "";
        dispatchManager._player_dispatch_state.last_reward_currency = 0;
        dispatchManager._player_dispatch_state.last_reward_exp = 0;
        dispatchManager._player_dispatch_state.last_reward_photo_name = "";

        if (dispatchManager._player_dispatch_state.last_reward_item_list != null)
        {
            dispatchManager._player_dispatch_state.last_reward_item_list.Clear();
        }

        dispatchManager._player_dispatch_state.last_friend_event_name = "";

        // 上传清空后的状态到服务器
        UploadDispatchInfoToServer();
    }

    #endregion

    #region IMsg_Receiver (Server -> Client)

    public string get_receiver_name()
    {
        return ReceiverName;
    }

    public void receive_msg(Network_Msg msg)
    {
        if (dispatchManager == null)
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
        Debug.Log($"{ReceiverName}: Msg_Dispatcher.Instance._is_locking set to false");
    }

    private void HandleResponseData(string detailInfo)
    {
        try
        {
            // 使用提供的JSON解析方式
            var data = Serialization_Provider.DeserializeObject<string[]>(detailInfo);

            if (data != null && data.Length == 2)
            {
                if (data[0] == DataKey)
                {
                    // 加载调度信息
                    dispatchManager._player_dispatch_state.load_dispatch_info_from_json(data[1]);

                    // 触发调度tick
                    dispatchManager.dispatch_tick();

                    Debug.Log($"{ReceiverName}: Dispatch data loaded and tick triggered");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ReceiverName}: Failed to parse response data - {ex.Message}");
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

    // 提供外部调用的方法来更新调度信息
    public void RequestUpdateDispatchInfo()
    {
        UpdateDispatchInfoFromServer();
    }

    // 提供外部调用的方法来上传调度信息
    public void RequestUploadDispatchInfo()
    {
        UploadDispatchInfoToServer();
    }

    // 提供外部调用的方法来清空之前的调度
    public void RequestClearPreviousDispatch()
    {
        OnClearPreviousDispatch();
    }

    #endregion
}