using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class Quest_And_Achievement_Receiver : SingletonMono<Quest_And_Achievement_Receiver>, IMsg_Receiver
{
    private Quest_And_Achievement_Manager _manager;
    private NetWork_Center_WSS _networkCenter;

    private const string ReceiverName = "Quest_And_Achievement_Receiver";

    #region Unity Life Cycle

    private void Start()
    {
        _manager = GetComponent<Quest_And_Achievement_Manager>();
        if (_manager == null)
        {
            Debug.LogError("Quest_Receiver: Quest_And_Achievement_Manager not found");
            return;
        }

        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError("Quest_Receiver: NetworkCenter not initialized");
            return;
        }

        BindUnityEvents();
        RegisterToNetwork();

        Debug.Log($"Quest_And_Achievement_Receiver initialized on {_manager.gameObject.name}");
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
        _manager._upload_player_action_to_server.AddListener(UploadPlayerActionToServer);
        _manager._update_achievement_info_from_server.AddListener(UpdateAchievementInfoFromServer);
        _manager._on_get_achievement_reward.AddListener(OnGetAchievementReward);
    }

    private void UnbindUnityEvents()
    {
        _manager._upload_player_action_to_server.RemoveListener(UploadPlayerActionToServer);
        _manager._update_achievement_info_from_server.RemoveListener(UpdateAchievementInfoFromServer);
        _manager._on_get_achievement_reward.RemoveListener(OnGetAchievementReward);
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

    #region Client -> Server

    private void UploadPlayerActionToServer()
    {
        if (_manager == null)
            return;

        string actionName = _manager.current_player_action_name;
        string actionDetailJson = _manager.current_player_action_detial_json;

        if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(actionDetailJson))
        {
            Debug.Log("Quest_Receiver: No current player action to upload");
            return;
        }


        List<string> data = new() { actionName, actionDetailJson };

        string lastData = Serialization_Provider.SerializeObject(data);

        string json = $"[\"{actionName}\",{actionDetailJson}]";
        SendMsg("Post_Player_Action", lastData);
    }

    public void UpdateAchievementInfoFromServer()
    {
        SendMsg("Get_Achievement_Data", string.Empty);
        Log.Custom("Quest_Receiver: Requesting achievement data from server", ReceiverName);

    }

    private void OnGetAchievementReward()
    {
        if (_manager == null)
            return;

        string achievementName = _manager.current_get_reward_achievement_name;
        if (string.IsNullOrEmpty(achievementName))
        {
            Debug.Log("Quest_Receiver: No achievement selected for reward");
            return;
        }

        SendMsg("On_Get_Achievement_Reward", achievementName);
    }

    #endregion

    #region IMsg_Receiver (Server -> Client)

    public string get_receiver_name()
    {
        return ReceiverName;
    }

    public void receive_msg(Network_Msg msg)
    {
        if (_manager == null)
            return;

        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", ReceiverName);

        switch (msg.action)
        {
            case "Response_Achievement_Data":
                _manager.load_obtained_achievement_info(msg.detail_info);
                break;

            case "New_Achievement_Unlocked":
                StartCoroutine(HandleNewAchievementUnlocked(msg.detail_info));
                break;

            default:
                Debug.LogWarning($"Quest_Receiver: Unknown action {msg.action}");
                break;
        }

        Msg_Dispatcher._instance.set_locking(false);
    }

    #endregion

    #region Coroutine Helpers

    private IEnumerator HandleNewAchievementUnlocked(string detailInfo)
    {
        _manager.add_achievement_list_to_pending_list(detailInfo);

        yield return new WaitForSeconds(1.024f);

        _manager.on_pending_finished_achievement_changed();
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
}
