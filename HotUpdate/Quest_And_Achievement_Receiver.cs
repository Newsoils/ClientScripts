using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using CLIP.Project_Mouse.Network;

public class Quest_And_Achievement_Receiver : SingletonMono<Quest_And_Achievement_Receiver>
{
    private Quest_And_Achievement_Manager _manager;

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

        BindUnityEvents();

        Debug.Log($"Quest_And_Achievement_Receiver initialized on {_manager.gameObject.name}");
    }

    private void OnDestroy()
    {
        UnbindUnityEvents();
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

    // private void SendMsg(string action, string detail)
    // {
    //     if (_networkCenter == null || !_networkCenter._connect_to_player_server)
    //         return;

    //     Network_Msg msg = new Network_Msg
    //     {
    //         sender = ReceiverName,
    //         action_target = "Player_Server",
    //         action = action,
    //         detail_info = detail,
    //         _sending_mode = Msg_Sending_Mode.Client_to_Server
    //     };

    //     _networkCenter.send_via_wss(msg);
    // }

    #endregion

    #region Client -> Server

    private void UploadPlayerActionToServer()
    {
        if (_manager == null)
            return;

        // string actionName = _manager.current_player_action_name;
        // string actionDetailJson = _manager.current_player_action_detial_json;

        // if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(actionDetailJson))
        // {
        //     Debug.Log("Quest_Receiver: No current player action to upload");
        //     return;
        // }


        // List<string> data = new() { actionName, actionDetailJson };

        // string lastData = Serialization_Provider.SerializeObject(data);

        // string json = $"[\"{actionName}\",{actionDetailJson}]";
        // SendMsg("Post_Player_Action", lastData);
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }

    public void UpdateAchievementInfoFromServer()
    {
        // SendMsg("Get_Achievement_Data", string.Empty);
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
        Log.Custom("Quest_Receiver: Requesting achievement data from server", ReceiverName);

    }

    private void OnGetAchievementReward()
    {
        if (_manager == null)
            return;

        // string achievementName = _manager.current_get_reward_achievement_name;
        // if (string.IsNullOrEmpty(achievementName))
        // {
        //     Debug.Log("Quest_Receiver: No achievement selected for reward");
        //     return;
        // }

        // SendMsg("On_Get_Achievement_Reward", achievementName);
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
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
}
