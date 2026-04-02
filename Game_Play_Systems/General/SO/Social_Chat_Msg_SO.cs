using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using CLIP.Project_Mouse.Kernel.Social;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "Social_Chat_Msg_SO", menuName = "Project_Mouse/Social/Social_Chat_Msg_SO")]
    public class Social_Chat_Msg_SO : ScriptableObject
    {
        [Header("配置文件")]
        public TextAsset chat_msg_json_file;

        [Header("聊天消息数据库")]
        public List<Social_Chat_Msg> chat_msg_db = new List<Social_Chat_Msg>();

        public void load_chat_msg_from_json()
        {
            chat_msg_db = JsonConvert.DeserializeObject<List<Social_Chat_Msg>>(chat_msg_json_file.text);
        }

        /// <summary>
        /// 从 JSON 文件加载聊天消息配置表
        /// </summary>
        public void LoadChatMsgDbFromJson()
        {
            if (chat_msg_json_file != null)
            {
                load_chat_msg_from_json();
                Debug.Log($"Social_Chat_Msg_SO: 已加载 {chat_msg_db.Count} 条聊天消息配置。");
            }
            else
            {
                Debug.LogWarning("Social_Chat_Msg_SO: chat_msg_json_file 未分配，无法加载聊天消息配置。");
            }
        }
    }
}