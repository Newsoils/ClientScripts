using System.Collections.Generic;
using Common;
using Newtonsoft.Json;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            namespace Social
            {
                /// <summary>
                /// 好友社交记录：玩家详情与私聊频道由服务端 Protobuf 驱动。
                /// </summary>
                [System.Serializable]
                public class Friend_Social_Record
                {
                    /// <summary>好友玩家信息（原 friend_id / friend_name / _brief_info）。</summary>
                    public PlayerDetailedInfo Info;

                    /// <summary>私聊频道（原 _chat_msg 列表）。</summary>
                    public PrivateChatChannel ChatInfo;

                    /// <summary>今日互动热度，来自 <see cref="FriendshipInfo.TodayPoint"/>。</summary>
                    public int hot_daily_count;

                    public List<achievement_record> achievements_obtained = new List<achievement_record>();

                    [JsonIgnore]
                    public List<Social_Behavior> _behavior_record = new List<Social_Behavior>();

                    [JsonIgnore]
                    public ulong RoleId => Info?.RoleID ?? 0;

                    [JsonIgnore]
                    public string FriendId => RoleId > 0 ? RoleId.ToString() : string.Empty;

                    [JsonIgnore]
                    public string DisplayName =>
                        string.IsNullOrEmpty(Info?.RoleName) ? "玩家" : Info.RoleName;

                    public void EnsureChatInfo()
                    {
                        ChatInfo ??= new PrivateChatChannel();
                    }

                    public bool MatchesRoleKey(string roleKey)
                    {
                        if (string.IsNullOrWhiteSpace(roleKey))
                            return false;
                        var key = roleKey.Trim();
                        if (key.StartsWith("#"))
                            key = key.Substring(1);
                        return FriendId == key || DisplayName == roleKey.Trim();
                    }

                    public static Friend_Social_Record Create(PlayerDetailedInfo info, PrivateChatChannel chat = null)
                    {
                        if (info == null || info.RoleID == 0)
                            return null;

                        return new Friend_Social_Record
                        {
                            Info = info,
                            ChatInfo = chat ?? new PrivateChatChannel(),
                        };
                    }
                }
            }
        }
    }
}
