using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel.Dispatch;
using UnityEngine;

public class Dispatch_Receiver : SingletonMono<Dispatch_Receiver>, IMsg_Receiver
{
    [System.Serializable]
    private class DispatchSavePayload
    {
        public string dispatchStateJson;
        public List<DispatchBagInfo> dispatchBags;
    }

    private Dispatch_Manager dispatchManager;
    private NetWork_Center_WSS _networkCenter;

    private const string ReceiverName = "Dispatch_Receiver";
    private const string DataKey = "Dispatch_Info";
    private const int ExpectedBagCount = 3;

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

        EvtDsp.AddEvt<string>(EvtNames.Login_Messsage, OnLoginMessage);

        if (_networkCenter._connect_to_player_server)
        {
            UpdateDispatchInfoFromServer();
        }

        Debug.Log($"{ReceiverName} initialized on {gameObject.name}");
    }

    private void OnDestroy()
    {
        EvtDsp.RemoveEvt<string>(EvtNames.Login_Messsage, OnLoginMessage);

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
        {
            Debug.LogWarning($"{ReceiverName}: SendMsg skipped. action={action}, connected={_networkCenter?._connect_to_player_server}");
            return;
        }

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

    private void OnLoginMessage(string loginMsg)
    {
        if (loginMsg != "Login_Success")
        {
            return;
        }

        // Login_Messsage 事件在 _connect_to_player_server 被置 true 之前就触发了，
        // 所以用协程等连接就绪再拉，避免和 Login_Manager 的执行顺序耦合。
        Debug.Log($"{ReceiverName}: OnLoginMessage received, waiting for connection ready");
        StartCoroutine(RequestDispatchInfoWhenReady());
    }

    private IEnumerator RequestDispatchInfoWhenReady()
    {
        const float timeoutSeconds = 10f;
        float elapsed = 0f;
        while (_networkCenter == null || !_networkCenter._connect_to_player_server)
        {
            if (elapsed >= timeoutSeconds)
            {
                Debug.LogWarning($"{ReceiverName}: Wait for player-server connection timeout, skip dispatch info request");
                yield break;
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log($"{ReceiverName}: Connection ready after {elapsed:F2}s, requesting dispatch info");
        UpdateDispatchInfoFromServer();
    }

    private void UpdateDispatchInfoFromServer()
    {
        if (_networkCenter == null || !_networkCenter._connect_to_player_server)
        {
            Debug.Log($"{ReceiverName}: Skip update, not connected to player server");
            return;
        }

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

        string serializedState = Serialization_Provider.SerializeObject(dispatchManager._player_dispatch_state);

        // 上传前把 bags 逐项深拷贝一份，断开与 Dispatch_Manager 内部 list 的任何引用共享；
        // 同时检查三包是否意外指向同一引用（那会导致序列化出来 3 份一模一样的 JSON）。
        var snapshot = new List<DispatchBagInfo>(ExpectedBagCount);
        if (dispatchManager.dispatch_Bags != null)
        {
            for (int i = 0; i < dispatchManager.dispatch_Bags.Count; i++)
            {
                var src = dispatchManager.dispatch_Bags[i];
                snapshot.Add(src == null
                    ? new DispatchBagInfo()
                    : new DispatchBagInfo
                    {
                        foodName = src.foodName,
                        snackName = src.snackName,
                        tapeName = src.tapeName,
                        isPacked = src.isPacked
                    });
            }

            for (int i = 0; i < dispatchManager.dispatch_Bags.Count; i++)
            {
                for (int j = i + 1; j < dispatchManager.dispatch_Bags.Count; j++)
                {
                    if (dispatchManager.dispatch_Bags[i] != null
                        && ReferenceEquals(dispatchManager.dispatch_Bags[i], dispatchManager.dispatch_Bags[j]))
                    {
                        Debug.LogError($"{ReceiverName}: dispatch_Bags[{i}] and [{j}] share the same reference before upload! 这会导致三个背包内容相同。");
                    }
                }
            }
        }
        EnsureDispatchBagsCount(snapshot, ExpectedBagCount);

        var payload = new DispatchSavePayload
        {
            dispatchStateJson = serializedState,
            dispatchBags = snapshot
        };

        string serializedPayload = Serialization_Provider.SerializeObject(payload);

        List<string> data = new() { DataKey, serializedPayload };
        string lastData = Serialization_Provider.SerializeObject(data);

        Debug.Log($"{ReceiverName}: Uploading dispatch info. bags={DescribeBags(payload.dispatchBags)}");

        SendMsg("Save_Data", lastData);
    }

    private void OnClearPreviousDispatch()
    {
        if (dispatchManager == null || dispatchManager._player_dispatch_state == null)
        {
            Debug.Log($"{ReceiverName}: Cannot clear dispatch - manager not initialized");
            return;
        }

        Debug.Log($"{ReceiverName}: Clearing previous dispatch info");

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
            var data = Serialization_Provider.DeserializeObject<string[]>(detailInfo);

            if (data == null || data.Length != 2)
            {
                Debug.LogWarning($"{ReceiverName}: HandleResponseData invalid shape");
                return;
            }

            if (data[0] != DataKey)
            {
                return;
            }

            string payloadJson = data[1];
            Debug.Log($"{ReceiverName}: Response payload len={payloadJson?.Length ?? 0}");

            if (!TryLoadDispatchPayload(payloadJson))
            {
                // 兼容旧格式：payload 就是 _player_dispatch_state 的 json
                Debug.Log($"{ReceiverName}: Payload is legacy format, load state only");
                dispatchManager._player_dispatch_state.load_dispatch_info_from_json(payloadJson);
            }

            dispatchManager.dispatch_tick();

            Debug.Log($"{ReceiverName}: Dispatch data loaded. bags={DescribeBags(dispatchManager.dispatch_Bags)}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"{ReceiverName}: Failed to parse response data - {ex.Message}");
        }
    }

    private bool TryLoadDispatchPayload(string payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson) || dispatchManager == null || dispatchManager._player_dispatch_state == null)
        {
            return false;
        }

        try
        {
            var payload = Serialization_Provider.DeserializeObject<DispatchSavePayload>(payloadJson);
            if (payload == null || string.IsNullOrEmpty(payload.dispatchStateJson))
            {
                return false;
            }

            dispatchManager._player_dispatch_state.load_dispatch_info_from_json(payload.dispatchStateJson);

            if (payload.dispatchBags != null && payload.dispatchBags.Count > 0)
            {
                // 深拷贝每一项，防止 JSON 里意外的共享引用或后续 list 操作污染原数据。
                var cloned = new List<DispatchBagInfo>(payload.dispatchBags.Count);
                foreach (var src in payload.dispatchBags)
                {
                    cloned.Add(src == null
                        ? new DispatchBagInfo()
                        : new DispatchBagInfo
                        {
                            foodName = src.foodName,
                            snackName = src.snackName,
                            tapeName = src.tapeName,
                            isPacked = src.isPacked
                        });
                }
                dispatchManager.dispatch_Bags = cloned;
                EnsureDispatchBagsCount(dispatchManager.dispatch_Bags, ExpectedBagCount);
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"{ReceiverName}: TryLoadDispatchPayload fallback, reason={ex.Message}");
            return false;
        }
    }

    private static void EnsureDispatchBagsCount(List<DispatchBagInfo> bags, int expectedCount)
    {
        if (bags == null) return;

        while (bags.Count < expectedCount)
        {
            bags.Add(new DispatchBagInfo());
        }

        if (bags.Count > expectedCount)
        {
            bags.RemoveRange(expectedCount, bags.Count - expectedCount);
        }
    }

    private static string DescribeBags(List<DispatchBagInfo> bags)
    {
        if (bags == null) return "null";
        var parts = new List<string>(bags.Count);
        for (int i = 0; i < bags.Count; i++)
        {
            var b = bags[i];
            if (b == null)
            {
                parts.Add($"[{i}] null");
                continue;
            }
            parts.Add($"[{i}] packed={b.isPacked} food={b.foodName} snack={b.snackName} tape={b.tapeName}");
        }
        return string.Join(" | ", parts);
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

    public void RequestUpdateDispatchInfo()
    {
        UpdateDispatchInfoFromServer();
    }

    public void RequestUploadDispatchInfo()
    {
        UploadDispatchInfoToServer();
    }

    public void RequestClearPreviousDispatch()
    {
        OnClearPreviousDispatch();
    }

    #endregion
}
