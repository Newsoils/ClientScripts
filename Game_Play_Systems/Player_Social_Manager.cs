using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Social;
using CLIP.Project_Mouse.UI;
using Cmd;
using Common;
using UnityEngine;
using UnityEngine.Events;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Player_Social_Manager : MonoBehaviour
    {
        public static Player_Social_Manager _instance;
        public Player_Social_Setting _social_setting;
        public Current_Player_Social_Info _current_social_info;
        [Header("Friend Apply")]
        /// <summary>好友申请列表（原 <see cref="Current_Player_Social_Info._friend_pending_info_record"/>）。</summary>
        public List<PlayerDetailedInfo> ApplyInfos = new List<PlayerDetailedInfo>();

        [Header("Temp_Data")]
        public List<Friend_Social_Record> _temp_search_result;
        public string _current_chat_friend_name = "";
        public Social_Chat_Msg _current_chat_msg;
        [Header("For_Visit_Room")]
        public bool _on_visit_friend_room = false;
        public string _next_visit_room_friend_name = "";
        //public CLIP.Project_Mouse.Kernel.Cloth_Suit _friend_main_character_suit;
        [Header("Data_SO")]
        public Social_Chat_Msg_SO _social_chat_msg_so;

        [Space(16)]
        [Header("Event")]
        public string _temp_cache_input_str;
        public UnityEvent _on_refresh_social_state = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_try_find_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_try_add_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_remove_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent<string> _on_confirm_friend = new UnityEvent<string>();
        [HideInInspector]
        public UnityEvent _on_refuse_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_enter_friend_room = new UnityEvent();


        [HideInInspector]
        public UnityEvent _on_take_photo_with_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_send_photo = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_pin_achievement = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_select_photo_in_brief = new UnityEvent();

        [HideInInspector]
        public UnityEvent _on_update_social_setting_from_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_send_social_setting_to_server = new UnityEvent();

        [HideInInspector]
        public UnityEvent _on_update_social_info_from_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_send_social_info_to_server = new UnityEvent();
        /// <summary>
        ///  For Chat
        /// </summary>
        [Header("For_Chat")]
        /// <summary>私聊频道，key 为好友 <see cref="PlayerDetailedInfo.RoleID"/>（与 He.RoleInfo.RoleID 一致）。</summary>
        public Dictionary<ulong, PrivateChatChannel> PrivateChatChannelMap =
            new Dictionary<ulong, PrivateChatChannel>();

        /// <summary><see cref="common.ChatChannelType"/> 私聊（SINGLE_PRIVATE = 5），用于 ChatSend / SyncChatChannelMsg。</summary>
        public const ChatChannelType PrivateChatChannelType = ChatChannelType.SinglePrivate;

        /// <summary><see cref="OpenOrCloseChatChannelS2C"/> 专用：int32 ChannelType，2 = 私人频道（与枚举 5 不同）。</summary>
        public const int OpenOrCloseChatPrivateChannelType = 2;

        [HideInInspector]
        public ulong _current_chat_friend_role_id;

        [HideInInspector]
        public UnityEvent _on_send_social_chat_msg = new UnityEvent();
        [HideInInspector]
        public UnityEvent<string> _update_social_chat_msg_from_server = new UnityEvent<string>();

        public UnityEvent _on_refresh_social_chat_msg = new UnityEvent();

        [Header("For_Present")]
        public string temp_present_receiver_name;
        public string temp_present_name;
        [HideInInspector]
        public UnityEvent _on_send_present_to_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _update_present_records_from_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _upload_present_records_to_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_refresh_present_view = new UnityEvent();
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                if (_instance != this)
                {
#if UNITY_EDITOR
                    DestroyImmediate(this.gameObject);
#else
           Destroy(this.gameObject);
#endif
                }
            }
        }

        void Start()
        {
            if (_instance != this)
                return;

            _social_chat_msg_so.load_chat_msg_from_json();
            _on_refresh_present_view.AddListener(ReceivePresent);
            EnsureSocialInfo();
        }
        private void OnDestroy()
        {
            _on_refresh_present_view.RemoveListener(ReceivePresent);
            if (_instance == this)
                _instance = null;
        }


        #region Event Triggers       

        public void on_try_find_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_try_find_friend != null) _on_try_find_friend.Invoke();
        }

        public void on_try_add_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_try_add_friend != null) _on_try_add_friend.Invoke();
        }
        public void on_remove_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_remove_friend != null) _on_remove_friend.Invoke();
        }

        public void on_confirm_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_confirm_friend != null) _on_confirm_friend.Invoke(_friend_id);
        }
        public void on_refuse_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_refuse_friend != null) _on_refuse_friend.Invoke();
        }

        public void on_enter_friend_room(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_enter_friend_room != null) _on_enter_friend_room.Invoke();
        }


        public void on_take_photo_with_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_take_photo_with_friend != null) _on_take_photo_with_friend.Invoke();
        }

        public void on_send_photo(string _photo_info_json)
        {
            if (_on_send_photo != null) _on_send_photo.Invoke();
        }

        public void on_pin_achievement(string achievement_name)
        {
            _temp_cache_input_str = achievement_name;
            if (_on_pin_achievement != null) _on_pin_achievement.Invoke();
        }

        public void on_select_photo_in_brief(string photo_name)
        {
            _temp_cache_input_str = photo_name;
            if (_on_select_photo_in_brief != null) _on_select_photo_in_brief.Invoke();
        }


        public void on_send_present_to_friend(string _friend_name, string present_name)
        {
            temp_present_name = present_name;
            temp_present_receiver_name = _friend_name;

            if (_on_send_present_to_friend != null) _on_send_present_to_friend.Invoke();
        }

        public void update_present_records_from_server()
        {
            if (_update_present_records_from_server != null) _update_present_records_from_server.Invoke();
        }

        public void upload_present_records_to_server()
        {
            if (_upload_present_records_to_server != null) _upload_present_records_to_server.Invoke();
        }
        public void on_refresh_present_view()
        {
            Debug.Log("on_refresh_present_view_Triggered");
            if (_on_refresh_present_view != null) _on_refresh_present_view.Invoke();
        }
        public void on_update_social_setting_from_server()
        {
            if (_on_update_social_setting_from_server != null) _on_update_social_setting_from_server.Invoke();
        }

        public void on_send_social_setting_to_server()
        {
            if (_on_send_social_setting_to_server != null) _on_send_social_setting_to_server.Invoke();
        }
        public void on_update_social_info_from_server()
        {
            if (_on_update_social_info_from_server != null) _on_update_social_info_from_server.Invoke();
        }

        public void on_send_social_info_to_server()
        {
            if (_on_send_social_info_to_server != null) _on_send_social_info_to_server.Invoke();
        }
        public void on_refresh_social_state()
        {
            Debug.Log("on_refresh_social_state_Triggered");
            if (_on_refresh_social_state != null) _on_refresh_social_state.Invoke();
        }

        #endregion

        public void load_find_friend_from_json(string _json)
        {
            _temp_search_result = GF_SP.DeserializeObject<List<Friend_Social_Record>>(_json);

            // on_refresh_social_state();

        }
        public void on_send_social_chat_msg(string _friend_name, int _chat_msg_id)
        {
            _current_chat_friend_name = _friend_name;
            _current_chat_friend_role_id = ResolveFriendRoleId(_friend_name);
            _current_chat_msg = _social_chat_msg_so.chat_msg_db.Find(_m => _m.msg_id == _chat_msg_id);
            _on_send_social_chat_msg.Invoke();
        }

        public void SetCurrentChatFriend(Friend_Social_Record record)
        {
            if (record == null)
            {
                _current_chat_friend_role_id = 0;
                _current_chat_friend_name = string.Empty;
                return;
            }

            _current_chat_friend_role_id = record.RoleId;
            _current_chat_friend_name = record.DisplayName;
            SyncFriendChatFromMap(record.RoleId);
        }

        public ulong ResolveFriendRoleId(string friendKey)
        {
            if (string.IsNullOrWhiteSpace(friendKey))
                return 0;

            if (ulong.TryParse(friendKey.Trim().TrimStart('#'), out var roleId) && roleId > 0)
                return roleId;

            var friend = _current_social_info?._friend_accepted_info_record
                ?.Find(f => f.MatchesRoleKey(friendKey) || f.DisplayName == friendKey.Trim());
            return friend?.RoleId ?? 0;
        }

        public static ulong GetPrivateChatRoleId(PrivateChatChannel channel)
        {
            return channel?.He?.RoleInfo?.RoleID ?? 0;
        }

        public void update_social_chat_msg_from_server(string friend_name)
        {
            _update_social_chat_msg_from_server.Invoke(friend_name);
        }

        public void on_refresh_social_chat_msg()
        {
            Debug.Log("on_refresh_social_chat_msg_Triggered");
            _on_refresh_social_chat_msg.Invoke();
        }


        public void load_chat_msg_from_json(string _json)
        {
            var _data = GF_SP.DeserializeObject<List<string>>(_json);
            if (_data == null || _data.Count < 2)
                return;

            var _friend_info = _current_social_info._friend_accepted_info_record
                .Find(_f => _f.MatchesRoleKey(_data[0]));
            if (_friend_info == null)
                return;

            var legacy = GF_SP.DeserializeObject<List<Social_Chat_Msg_Record>>(_data[1]);
            ApplyLegacyChatRecords(_friend_info, legacy);
        }

        static void ApplyLegacyChatRecords(Friend_Social_Record record, List<Social_Chat_Msg_Record> legacy)
        {
            if (record == null || legacy == null)
                return;

            record.EnsureChatInfo();
            record.ChatInfo.Content.Clear();
            foreach (var leg in legacy)
            {
                var msg = new ChatMessage
                {
                    Text = leg._msg_content?.res_url ?? leg._msg_good_item_name ?? string.Empty,
                    TimeTag = leg._msg_date != default
                        ? new DateTimeOffset(leg._msg_date).ToUnixTimeSeconds()
                        : 0,
                    MsgType = string.IsNullOrEmpty(leg._msg_good_item_name)
                        ? ChatMsgType.Text
                        : ChatMsgType.Expression,
                };
                if (!string.IsNullOrEmpty(leg._msg_sender))
                {
                    msg.Person = new Spokesman
                    {
                        RoleInfo = new SimpleRoleInfo
                        {
                            RoleID = leg._msg_sender == record.DisplayName ? record.RoleId : 0,
                            RoleName = leg._msg_sender,
                        }
                    };
                }

                record.ChatInfo.Content.Add(msg);
            }
        }
        public void load_social_info_from_json(string _json)
        {
            Debug.Log("Player_Social_Manager_load_social_info_from_json");
            var _temp_record_accepted = _current_social_info._friend_accepted_info_record;
            _current_social_info = GF_SP.DeserializeObject<Current_Player_Social_Info>(_json);
            if (_temp_record_accepted != null)
            {
                foreach (var item in _current_social_info._friend_accepted_info_record)
                {
                    var _record = _temp_record_accepted.Find(_r => _r.RoleId == item.RoleId && _r.RoleId != 0);
                    if (_record != null)
                    {
                        item._behavior_record = _record._behavior_record;
                        item.ChatInfo = _record.ChatInfo?.Clone() ?? item.ChatInfo;
                        item.hot_daily_count = _record.hot_daily_count;
                    }
                }
            }

        }
        public void set_up_visit_room(string _friend_name)
        {
            _on_visit_friend_room = true;
            _next_visit_room_friend_name = _friend_name;
        }

        public void clear_visit_room()
        {
            _on_visit_friend_room = false;
            _next_visit_room_friend_name = "";
        }

        public void ReceivePresent()
        {
            var presents = _current_social_info._present_records;

            List<(string,int)> items = new List<(string,int)>();
            foreach (var presentRecord in presents)
            {
                items.Add((presentRecord._msg_good_item_name, 1));
            }
            Global_Inventory_Manager.Change_Items_Count(items, "present");
            EvtDsp.TriggerEvt(EvtNames.ReloadPlacementData);
            EvtDsp.TriggerEvt(EvtNames.ReloadPlantData);
        }

        #region Protobuf (好友列表 / 搜索 / 申请)

        public void EnsureSocialInfo()
        {
            if (_current_social_info == null)
                _current_social_info = new Current_Player_Social_Info();
        }

        public static Friend_Social_Record FromPlayerDetailed(PlayerDetailedInfo p)
        {
            return Friend_Social_Record.Create(p?.Clone());
        }

        static List<Friend_Social_Record> ToFriendRecords(
            IEnumerable<PlayerDetailedInfo> src,
            List<Friend_Social_Record> previous)
        {
            var list = new List<Friend_Social_Record>();
            if (src == null)
                return list;

            foreach (var p in src)
            {
                var r = FromPlayerDetailed(p);
                if (r != null)
                    list.Add(r);
            }

            return MergeChatAndBehavior(list, previous);
        }

        static List<Friend_Social_Record> MergeChatAndBehavior(
            List<Friend_Social_Record> incoming,
            List<Friend_Social_Record> previous)
        {
            if (previous == null || incoming == null)
                return incoming ?? new List<Friend_Social_Record>();

            foreach (var item in incoming)
            {
                var old = previous.Find(r => r.RoleId == item.RoleId && r.RoleId != 0);
                if (old == null) continue;
                item._behavior_record = old._behavior_record;
                item.ChatInfo = old.ChatInfo?.Clone() ?? item.ChatInfo;
                item.hot_daily_count = old.hot_daily_count;
            }

            return incoming;
        }

        void ApplyTodayPoints(FriendshipInfo info, List<Friend_Social_Record> friends)
        {
            if (info?.TodayPoint == null || friends == null)
                return;

            foreach (var f in friends)
            {
                if (f.RoleId == 0)
                    continue;
                if (info.TodayPoint.TryGetValue(f.RoleId, out var pt))
                    f.hot_daily_count = pt;
            }
        }

        public void SetApplyInfos(IEnumerable<PlayerDetailedInfo> applications)
        {
            ApplyInfos ??= new List<PlayerDetailedInfo>();
            ApplyInfos.Clear();
            if (applications == null)
                return;

            foreach (var p in applications)
            {
                if (p != null && p.RoleID != 0)
                    ApplyInfos.Add(p.Clone());
            }
        }

        public bool IsInApplyList(ulong roleId) =>
            roleId != 0 && ApplyInfos != null && ApplyInfos.Exists(p => p.RoleID == roleId);

        public void ApplyFriendshipInfoFromProto(FriendshipInfo info)
        {
            if (info == null) return;
            EnsureSocialInfo();

            var prevAccepted = _current_social_info._friend_accepted_info_record;

            _current_social_info._friend_accepted_info_record =
                ToFriendRecords(info.FriendsInfo, prevAccepted);
            SetApplyInfos(info.ApplicationList);

            ApplyTodayPoints(info, _current_social_info._friend_accepted_info_record);
            on_refresh_social_state();
        }

        public void ApplySearchResultsFromProto(IEnumerable<PlayerDetailedInfo> players)
        {
            _temp_search_result = ToFriendRecords(players, _temp_search_result);
        }

        public void ApplyFriendsListFromProto(IEnumerable<PlayerDetailedInfo> friends)
        {
            EnsureSocialInfo();
            _current_social_info._friend_accepted_info_record = ToFriendRecords(
                friends,
                _current_social_info._friend_accepted_info_record);
            on_refresh_social_state();
        }

        public void ApplyApplicationListsFromProto(
            IEnumerable<PlayerDetailedInfo> applications,
            IEnumerable<PlayerDetailedInfo> friends)
        {
            EnsureSocialInfo();
            SetApplyInfos(applications);
            if (friends != null)
            {
                _current_social_info._friend_accepted_info_record = ToFriendRecords(
                    friends,
                    _current_social_info._friend_accepted_info_record);
            }

            on_refresh_social_state();
        }

        public void ApplyFriendChangeS2C(FriendChangeS2C msg)
        {
            if (msg == null) return;
            EnsureSocialInfo();
            ApplyInfos ??= new List<PlayerDetailedInfo>();

            if (msg.FriendAdd != null)
            {
                foreach (var p in msg.FriendAdd)
                {
                    if (p == null || p.RoleID == 0)
                        continue;
                    if (!_current_social_info._friend_accepted_info_record.Exists(f => f.RoleId == p.RoleID))
                    {
                        var record = FromPlayerDetailed(p);
                        if (record != null)
                            _current_social_info._friend_accepted_info_record.Add(record);
                    }
                }
            }

            if (msg.FriendDel != null)
            {
                foreach (var p in msg.FriendDel)
                {
                    if (p == null || p.RoleID == 0)
                        continue;
                    _current_social_info._friend_accepted_info_record.RemoveAll(f => f.RoleId == p.RoleID);
                }
            }

            if (msg.FriendApplyAdd != null)
            {
                foreach (var p in msg.FriendApplyAdd)
                {
                    if (p == null || p.RoleID == 0)
                        continue;
                    if (!ApplyInfos.Exists(a => a.RoleID == p.RoleID))
                        ApplyInfos.Add(p.Clone());
                }
            }

            if (msg.FriendApplyDel != null)
            {
                foreach (var p in msg.FriendApplyDel)
                {
                    if (p == null || p.RoleID == 0)
                        continue;
                    ApplyInfos.RemoveAll(a => a.RoleID == p.RoleID);
                }
            }

            on_refresh_social_state();
        }

        #endregion

        #region Private Chat (Protobuf)

        public void ApplySyncChatChannelInfo(SyncChatChannelInfoS2C msg)
        {
            if (msg?.PrivateChatChannels == null)
                return;

            foreach (var channel in msg.PrivateChatChannels)
            {
                var roleId = GetPrivateChatRoleId(channel);
                if (roleId == 0)
                    continue;
                PrivateChatChannelMap[roleId] = channel.Clone();
            }

            SyncAllFriendRecordsChatFromMap();
            on_refresh_social_chat_msg();
        }

        public void ApplyOpenOrCloseChatChannel(OpenOrCloseChatChannelS2C msg)
        {
            if (msg == null || msg.ChannelType != OpenOrCloseChatPrivateChannelType)
                return;

            var roleId = GetPrivateChatRoleId(msg.PrivateChannel);
            if (roleId == 0)
                return;

            if (msg.OpType == 1)
            {
                PrivateChatChannelMap[roleId] = msg.PrivateChannel?.Clone() ?? new PrivateChatChannel();
            }
            else if (msg.OpType == 2)
            {
                PrivateChatChannelMap.Remove(roleId);
            }

            SyncAllFriendRecordsChatFromMap();
            on_refresh_social_chat_msg();
        }

        public void ApplySyncChatChannelMsg(SyncChatChannelMsgS2C msg)
        {
            if (msg == null || msg.ChannelType != PrivateChatChannelType)
                return;

            ulong mapKey = msg.ChannelTypeID;
            if (mapKey == 0)
                return;

            if (!PrivateChatChannelMap.TryGetValue(mapKey, out var channel) || channel == null)
            {
                channel = new PrivateChatChannel();
                if (msg.He != null)
                    channel.He = msg.He.Clone();
                PrivateChatChannelMap[mapKey] = channel;
            }

            if (msg.Content != null)
            {
                foreach (var piece in msg.Content)
                    channel.Content.Add(piece.Clone());
            }

            SyncAllFriendRecordsChatFromMap();
            if (_current_chat_friend_role_id == mapKey ||
                ResolveFriendRoleId(_current_chat_friend_name) == mapKey)
                on_refresh_social_chat_msg();
        }

        public void SyncFriendChatFromMap(ulong friendRoleId)
        {
            if (friendRoleId == 0 || _current_social_info == null)
                return;

            var friend = _current_social_info._friend_accepted_info_record
                .Find(f => f.RoleId == friendRoleId);
            if (friend == null)
                return;

            if (PrivateChatChannelMap.TryGetValue(friendRoleId, out var channel) && channel != null)
                friend.ChatInfo = channel.Clone();
            else
                friend.EnsureChatInfo();
        }

        void SyncAllFriendRecordsChatFromMap()
        {
            if (_current_social_info?._friend_accepted_info_record == null)
                return;

            foreach (var friend in _current_social_info._friend_accepted_info_record)
            {
                if (friend.RoleId == 0)
                    continue;
                if (PrivateChatChannelMap.TryGetValue(friend.RoleId, out var channel) && channel != null)
                    friend.ChatInfo = channel.Clone();
            }
        }

        #endregion

    }
}
