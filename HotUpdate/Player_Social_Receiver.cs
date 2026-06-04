using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Network;
using CLIP.Project_Mouse.Game_Play_System;
using Cmd;
using Common;
using UnityEngine;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using CS_Resource_Manager = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Network;
using CLIP.Project_Mouse.Kernel;

/// <summary>
/// 玩家好友：好友列表 (1013/1014)、搜索 (1141/1142)、申请/处理申请 (1133–1136)。
/// 回包由 <see cref="Global_Game_Data_Sync_Receiver"/> 注册到 RegisterMap 后转发至本类静态方法。
/// </summary>
public class Player_Social_Receiver : MonoBehaviour, IMsg_Receiver
{
    public static Player_Social_Receiver Instance { get; private set; }

    private Player_Social_Manager _socialManager;

    public string ReceiverName => "Player_Social_Receiver";

    enum PendingFriendOp
    {
        None,
        SendApplication,
        AcceptApplication,
        RefuseApplication,
    }

    PendingFriendOp _pendingOp = PendingFriendOp.None;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _socialManager = GetComponent<Player_Social_Manager>();
        if (_socialManager == null)
        {
            Debug.LogError("Player_Social_Receiver: Player_Social_Manager not found");
            return;
        }

        BindUnityEvents();
        Debug.Log("Player_Social_Receiver initialized");
    }

    private void OnDestroy()
    {
        UnbindUnityEvents();
        if (Instance == this)
            Instance = null;
    }

    #endregion

    #region Unity Event Binding

    private void BindUnityEvents()
    {
        _socialManager._on_try_find_friend.AddListener(OnTryFindFriend);
        _socialManager._on_try_add_friend.AddListener(OnTryAddFriend);
        _socialManager._on_refuse_friend.AddListener(OnRefuseFriend);
        _socialManager._on_confirm_friend.AddListener(OnConfirmFriend);
        _socialManager._on_update_social_info_from_server.AddListener(RequestFriendsList);

        // 聊天 / 礼物 / 拜访仍走旧协议，暂不发送
        _socialManager._on_remove_friend.AddListener(OnRemoveFriend);
        _socialManager._on_send_social_chat_msg.AddListener(SendChatMsg);
        _socialManager._on_send_present_to_friend.AddListener(SendPresent);
        _socialManager._update_present_records_from_server.AddListener(RequestFriendsList);
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
        _socialManager._on_confirm_friend.RemoveListener(OnConfirmFriend);
        _socialManager._on_update_social_info_from_server.RemoveListener(RequestFriendsList);
        _socialManager._on_remove_friend.RemoveListener(OnRemoveFriend);
        _socialManager._on_send_social_chat_msg.RemoveListener(SendChatMsg);
        _socialManager._on_send_present_to_friend.RemoveListener(SendPresent);
        _socialManager._update_present_records_from_server.RemoveListener(RequestFriendsList);
        _socialManager._upload_present_records_to_server.RemoveListener(UploadPresentRecords);
        _socialManager._update_social_chat_msg_from_server.RemoveListener(RequestChat);
        _socialManager._on_enter_friend_room.RemoveListener(OnEnterFriendRoom);
    }

    #endregion

    #region Protobuf Handlers (由 Global_Game_Data_Sync_Receiver 调用)

    public static void HandleFriendsListRes(FriendsListRes res)
    {
        if (res?.Info == null)
        {
            Debug.LogWarning("[Player_Social_Receiver] FriendsListRes.Info 为空");
            return;
        }

        Player_Social_Manager._instance?.ApplyFriendshipInfoFromProto(res.Info);
    }

    public static void HandleSearchPlayerRes(SearchPlayerRes res)
    {
        var mgr = Player_Social_Manager._instance;
        if (mgr == null) return;

        mgr.ApplySearchResultsFromProto(res?.Info);
        mgr.on_refresh_social_state();
    }

    public static void HandleEditFriendsListRes(EditFriendsListRes res)
    {
        var mgr = Player_Social_Manager._instance;
        if (mgr == null) return;

        mgr.ApplyFriendsListFromProto(res?.FriendsInfo);

        var receiver = Instance;
        if (receiver != null && receiver._pendingOp == PendingFriendOp.SendApplication)
        {
            PromptManager.ShowUpPrompt(PromptId.FriendRequestSent);
            receiver._pendingOp = PendingFriendOp.None;
            RequestFriendsListStatic();
        }
    }

    public static void HandleEditApplicationListRes(EditApplicationListRes res)
    {
        var mgr = Player_Social_Manager._instance;
        if (mgr == null) return;

        mgr.ApplyApplicationListsFromProto(res?.ApplicationListInfo, res?.FriendsList);

        var receiver = Instance;
        if (receiver == null) return;

        switch (receiver._pendingOp)
        {
            case PendingFriendOp.AcceptApplication:
                PromptManager.ShowUpPrompt(PromptId.FriendRequestAccepted);
                RequestFriendsListStatic();
                break;
            case PendingFriendOp.RefuseApplication:
                PromptManager.ShowUpPrompt(PromptId.FriendRequestRejected);
                RequestFriendsListStatic();
                break;
        }

        receiver._pendingOp = PendingFriendOp.None;
    }

    public static void HandleSyncChatChannelInfoS2C(SyncChatChannelInfoS2C res) =>
        Player_Social_Manager._instance?.ApplySyncChatChannelInfo(res);

    public static void HandleOpenOrCloseChatChannelS2C(OpenOrCloseChatChannelS2C res) =>
        Player_Social_Manager._instance?.ApplyOpenOrCloseChatChannel(res);

    public static void HandleSyncChatChannelMsgS2C(SyncChatChannelMsgS2C res) =>
        Player_Social_Manager._instance?.ApplySyncChatChannelMsg(res);

    public static void HandleChatSendRes(ChatSendRes res)
    {
        // 发送方仅确认回包，消息展示依赖 SyncChatChannelMsgS2C 推送
    }

    public static void HandleFriendChangeS2C(FriendChangeS2C res) =>
        Player_Social_Manager._instance?.ApplyFriendChangeS2C(res);

    #endregion

    #region Client -> Server

    private static bool CanSend()
    {
        if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            Debug.LogWarning("[Player_Social_Receiver] 未连接玩家服，跳过发送");
            return false;
        }

        return true;
    }

    private void RequestFriendsList()
    {
        RequestFriendsListStatic();
    }

    static void RequestFriendsListStatic()
    {
        if (!CanSend()) return;
        NetWork_Center_WSS.SendMsg(new FriendsListReq());
    }

    private void OnTryFindFriend()
    {
        if (_socialManager == null || !CanSend()) return;

        var keyword = _socialManager._temp_cache_input_str?.Trim();
        if (string.IsNullOrEmpty(keyword))
            return;

        // if (TryParseRoleId(keyword, out _))
        //     keyword = FormatRoleIdSearch(keyword);

        NetWork_Center_WSS.SendMsg(new SearchPlayerReq { Search = keyword });
    }

    private void OnTryAddFriend()
    {
        if (_socialManager == null || !CanSend()) return;
        if (!TryParseRoleId(_socialManager._temp_cache_input_str, out var roleId))
        {
            PromptManager.ShowUpPrompt(PromptId.PlayerIdInvalid);
            return;
        }

        _pendingOp = PendingFriendOp.SendApplication;
        NetWork_Center_WSS.SendMsg(new EditFriendsListReq
        {
            Type = 1,
            RoleID = roleId
        });
    }

    private void OnConfirmFriend(string friendId)
    {
        if (_socialManager == null || !CanSend()) return;
        if (!TryParseRoleId(friendId, out var roleId))
        {
            PromptManager.ShowUpPrompt(PromptId.PlayerIdInvalid);
            return;
        }

        _pendingOp = PendingFriendOp.AcceptApplication;
        NetWork_Center_WSS.SendMsg(new EditApplicationListReq
        {
            Type = 1,
            RoleID = roleId
        });
    }

    private void OnRefuseFriend()
    {
        if (_socialManager == null || !CanSend()) return;
        if (!TryParseRoleId(_socialManager._temp_cache_input_str, out var roleId))
        {
            PromptManager.ShowUpPrompt(PromptId.PlayerIdInvalid);
            return;
        }

        _pendingOp = PendingFriendOp.RefuseApplication;
        NetWork_Center_WSS.SendMsg(new EditApplicationListReq
        {
            Type = 0,
            RoleID = roleId
        });
    }

    private void OnRemoveFriend()
    {
        if (_socialManager == null || !CanSend()) return;
        if (!TryParseRoleId(_socialManager._temp_cache_input_str, out var roleId))
        {
            PromptManager.ShowUpPrompt(PromptId.PlayerIdInvalid);
            return;
        }

        NetWork_Center_WSS.SendMsg(new EditFriendsListReq
        {
            Type = 0,
            RoleID = roleId
        });
    }

    #endregion

    #region Legacy (未接入)

    public void receive_msg(Network_Msg msg)
    {
        // 好友列表/搜索/申请已改 Protobuf，旧 JSON action 不再处理
    }

    public string get_receiver_name() => ReceiverName;

    private void RequestChat(string friendKey)
    {
        if (_socialManager == null)
            return;

        var roleId = _socialManager.ResolveFriendRoleId(friendKey);
        if (roleId != 0)
            _socialManager.SyncFriendChatFromMap(roleId);

        _socialManager.on_refresh_social_chat_msg();
    }

    private void SendChatMsg()
    {
        if (_socialManager == null || !CanSend())
            return;

        var roleId = _socialManager._current_chat_friend_role_id;
        if (roleId == 0)
            roleId = _socialManager.ResolveFriendRoleId(_socialManager._current_chat_friend_name);
        if (roleId == 0)
        {
            PromptManager.ShowUpPrompt(PromptId.FriendNotFound);
            return;
        }

        var friend = _socialManager._current_social_info?._friend_accepted_info_record
            ?.Find(f => f.RoleId == roleId);
        if (friend?.Info == null)
        {
            PromptManager.ShowUpPrompt(PromptId.FriendInfoInvalid);
            return;
        }

        var chatMsg = _socialManager._current_chat_msg;
        string text = !string.IsNullOrEmpty(chatMsg?.res_url)
            ? chatMsg.res_url
            : chatMsg?.msg_symbol;
        if (string.IsNullOrEmpty(text))
        {
            PromptManager.ShowUpPrompt(PromptId.MessageEmpty);
            return;
        }

        var req = new ChatSendReq
        {
            ChannelType = Player_Social_Manager.PrivateChatChannelType,
            ReceiveID = roleId,
            MsgType = ChatMsgType.Expression,
            Text = text,
            Receiver = new SimpleRoleInfo
            {
                ServerID = friend.Info.ServerID,
            },
        };

        NetWork_Center_WSS.SendMsg(req);
    }

    private void SendPresent()
    {
        Debug.LogWarning("[Player_Social_Receiver] 送礼尚未接入 Protobuf");
    }

    private void UploadPresentRecords()
    {
        Debug.LogWarning("[Player_Social_Receiver] 礼物记录同步尚未接入 Protobuf");
    }

    private void OnEnterFriendRoom()
    {
        StartCoroutine(EnterFriendRoomCoroutine());
    }

    private IEnumerator EnterFriendRoomCoroutine()
    {
        var friendName = _socialManager._temp_cache_input_str;
        _socialManager.set_up_visit_room(friendName);

        Debug.LogWarning("[Player_Social_Receiver] 拜访好友房间尚未接入 Protobuf");

        yield return new WaitForSeconds(1f);

        CS_Resource_Manager.load_scene_async(
            "Scenes/Yin/Scene_Indoor_Main_Back_007_Yin_Stable",
            () => Debug.Log("Friend roomData scene loaded")
        );
    }

    #endregion

    #region Helpers

    static bool TryParseRoleId(string input, out ulong roleId)
    {
        roleId = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var s = input.Trim();
        if (s.StartsWith("#"))
            s = s.Substring(1);

        return ulong.TryParse(s, out roleId) && roleId > 0;
    }

    static string FormatRoleIdSearch(string input)
    {
        if (TryParseRoleId(input, out var roleId))
            return "#" + roleId;
        return input?.Trim() ?? "";
    }

    static void ShowPrompt(string text)
    {
        EvtDsp.TriggerEvt(EvtNames.ShowUpPrompt, text);
    }

    #endregion
}
