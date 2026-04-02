using System.Collections;
using CLIP.Framework_Core.Network;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using CS_Resource_Manager = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
using CLIP.Framework_Unity;


public class Player_Social_Receiver : MonoBehaviour, IMsg_Receiver
{
    private NetWork_Center_WSS _networkCenter;
    private Player_Social_Manager _socialManager;

    public string ReceiverName => "Player_Social_Receiver";

    #region Unity Lifecycle

    private void Start()
    {
        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError("Player_Social_Network_Receiver: NetworkCenter not initialized");
            return;
        }

        _socialManager = GetComponent<Player_Social_Manager>();
        if (_socialManager == null)
        {
            Debug.LogError("Player_Social_Network_Receiver: CS_Social_Manager not found");
            return;
        }

        BindUnityEvents();
        RegisterToNetwork();

        Debug.Log("Player_Social_Network_Receiver initialized");
    }

    private void OnDestroy()
    {
        UnbindUnityEvents();
        UnregisterFromNetwork();
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

    #region Unity Event Binding

    private void BindUnityEvents()
    {
        _socialManager._on_try_find_friend.AddListener(OnTryFindFriend);
        _socialManager._on_try_add_friend.AddListener(OnTryAddFriend);
        _socialManager._on_refuse_friend.AddListener(OnRefuseFriend);
        _socialManager._on_remove_friend.AddListener(OnRemoveFriend);

        _socialManager._on_confirm_friend.AddListener(OnConfirmFriend);
        _socialManager._on_update_social_info_from_server.AddListener(RequestSocialInfo);
        _socialManager._on_send_social_chat_msg.AddListener(SendChatMsg);

        _socialManager._on_send_present_to_friend.AddListener(SendPresent);
        _socialManager._update_present_records_from_server.AddListener(RequestSocialInfo);
        _socialManager._upload_present_records_to_server.AddListener(UploadPresentRecords);
        _socialManager._update_social_chat_msg_from_server.AddListener(RequestChat);

        _socialManager._on_enter_friend_room.AddListener(OnEnterFriendRoom);
    }

    private void UnbindUnityEvents()
    {
        if (_socialManager == null) return;

        _socialManager._on_try_find_friend.RemoveListener(OnTryFindFriend);
        _socialManager._on_try_add_friend.RemoveListener(OnTryAddFriend);
        _socialManager._on_refuse_friend.RemoveListener(OnRefuseFriend);
        _socialManager._on_remove_friend.RemoveListener(OnRemoveFriend);

        _socialManager._on_confirm_friend.RemoveListener(OnConfirmFriend);
        _socialManager._on_update_social_info_from_server.RemoveListener(RequestSocialInfo);
        _socialManager._on_send_social_chat_msg.RemoveListener(SendChatMsg);

        _socialManager._on_send_present_to_friend.RemoveListener(SendPresent);
        _socialManager._update_present_records_from_server.RemoveListener(RequestSocialInfo);
        _socialManager._upload_present_records_to_server.RemoveListener(UploadPresentRecords);
        _socialManager._update_social_chat_msg_from_server.RemoveListener(RequestChat);

        _socialManager._on_enter_friend_room.RemoveListener(OnEnterFriendRoom);
    }

    private void OnEnterFriendRoom()
    {
        StartCoroutine(EnterFriendRoomCoroutine());
    }

    #endregion

    #region Network Receive

    public void receive_msg(Network_Msg msg)
    {
        if (_socialManager == null) return;
        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", ReceiverName);

        switch (msg.action)
        {
            case "Response_Find_Friend":
                _socialManager.load_find_friend_from_json(msg.detail_info);
                _socialManager.on_refresh_social_state();
                break;

            case "Response_Social_Info":
                _socialManager.load_social_info_from_json(msg.detail_info);
                _socialManager.on_refresh_social_state();
                if (_socialManager._current_social_info._present_records.Count > 0)
                {
                    _socialManager.on_refresh_present_view();
                }
                break;

            case "Response_Social_Chat_Info":
                _socialManager.load_chat_msg_from_json(msg.detail_info);
                _socialManager.on_refresh_social_chat_msg();
                break;
        }

        Msg_Dispatcher._instance.set_locking(false);
    }

    #endregion

    #region Network Send Helpers

    private void SendToServer(string action, string detail)
    {
        if (_networkCenter == null || !_networkCenter._connect_to_player_server)
            return;

        var msg = new Network_Msg
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

    #region Social Actions

    private void OnTryFindFriend()
    {
        SendToServer("Try_Find_Friend", _socialManager._temp_cache_input_str);
    }

    private void OnTryAddFriend()
    {
        SendToServer("Try_Add_Friend", _socialManager._temp_cache_input_str);
    }

    private void OnConfirmFriend(string friendID)
    {
        SendToServer("Confirm_Friend", friendID);
    }

    private void OnRefuseFriend()
    {
        SendToServer("Refuse_Friend", _socialManager._temp_cache_input_str);
    }

    private void OnRemoveFriend()
    {
        SendToServer("Remove_Friend", _socialManager._temp_cache_input_str);
    }

    private void RequestSocialInfo()
    {
        SendToServer("Get_Social_Info", "");
    }

    public void RequestChat(string friendName)
    {
        SendToServer("Get_Chat", friendName);
    }

    private void SendChatMsg()
    {
        var payload = GF_SP.SerializeObject(new string[]
        {
            _socialManager._current_chat_friend_name,
            GF_SP.SerializeObject(_socialManager._current_chat_msg)
        });

        SendToServer("Post_Chat", payload);
    }

    private void SendPresent()
    {
        var payload = GF_SP.SerializeObject(new string[]
        {
            _socialManager.temp_present_receiver_name,
            _socialManager.temp_present_name
        });

        SendToServer("Send_Present", payload);
    }

    private void UploadPresentRecords()
    {
        SendToServer(
            "Save_Present_Records",
            GF_SP.SerializeObject(_socialManager._current_social_info._present_records)
        );
    }

    #endregion

    #region Scene

    private IEnumerator EnterFriendRoomCoroutine()
    {
        var friendName = _socialManager._temp_cache_input_str;
        _socialManager.set_up_visit_room(friendName);

        SendToServer("Visit_Friend_Room", friendName);

        yield return new WaitForSeconds(1f);

        CS_Resource_Manager.load_scene_async(
            "Scenes/Yin/Scene_Indoor_Main_Back_007_Yin_Stable",
            () => Debug.Log("Friend roomData scene loaded")
        );
    }

    public string get_receiver_name()
    {
        return ReceiverName;
    }

    #endregion
}
