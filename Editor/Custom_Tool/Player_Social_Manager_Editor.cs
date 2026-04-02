using UnityEngine;
using CLIP.Project_Mouse.Game_Play_System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CLIP.Project_Mouse.Custom_Tool
{
#if UNITY_EDITOR
    [CustomEditor(typeof(Player_Social_Manager))]
    public class Player_Social_Manager_Editor : Editor
    {
        public Player_Social_Manager _instance;
        private string friendIdToFind = "";
        private string friendIdToAdd = "";
        private string friendIdToConfirm = "";

        private string friendIdToRemove = "";
        private string friendIdToRefuse = "";
        private string friendIdToEnterRoom = "";


        private int chatMsgId = 0;
        private string chatMsgFriendName = "";

        private string updateChatMsgFriendName = "";

        private string presentFriendName = "";
        private string presentName = "";
        private string friendIdToVisitRoom = "";


        private void OnEnable()
        {
            _instance = (Player_Social_Manager)target;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("从服务器更新社交配置"))
            {
                if (_instance != null)
                {
                    // Instance.update_social_info_from_server();
                    Debug.Log($"Player_Social_Manager '{_instance.name}' 调用 update_social_info_from_server()");
                }
                else
                {
                    Debug.LogWarning("Player_Social_Manager 实例为空，无法调用 update_social_info_from_server()");
                }
            }
            if (GUILayout.Button("发送社交配置到服务器"))
            {
                if (_instance != null)
                {
                    // Instance.send_social_info_to_server();
                    Debug.Log($"Player_Social_Manager '{_instance.name}' 调用 send_social_info_to_server()");
                }
                else
                {
                    Debug.LogWarning("Player_Social_Manager 实例为空，无法调用 send_social_info_to_server()");
                }
            }

            // --- 添加查找好友的功能区 ---
            GUILayout.Space(20);
            GUILayout.Label("Friend_Basic_Operation", EditorStyles.boldLabel);

            // 创建一个文本框用于输入好友ID
            friendIdToFind = EditorGUILayout.TextField("Friend ID to Find", friendIdToFind);

            // 创建触发 on_try_find_friend 事件的按钮
            if (GUILayout.Button("Find Friend"))
            {
                if (!string.IsNullOrEmpty(friendIdToFind))
                {
                    // 调用公共方法来触发事件
                    _instance.on_try_find_friend(friendIdToFind);
                    Debug.Log($"已触发查找好友事件，ID: {friendIdToFind}");
                }
                else
                {
                    Debug.LogWarning("请输入要查找的好友ID。");
                }
            }

            // --- 添加添加好友的功能区 ---
            GUILayout.Space(10);
            friendIdToAdd = EditorGUILayout.TextField("Friend ID to Add", friendIdToAdd);

            if (GUILayout.Button("Add Friend"))
            {
                if (!string.IsNullOrEmpty(friendIdToAdd))
                {
                    _instance.on_try_add_friend(friendIdToAdd);
                    Debug.Log($"已触发添加好友事件，ID: {friendIdToAdd}");
                }
                else
                {
                    Debug.LogWarning("请输入要添加的好友ID。");
                }
            }

            // --- 确认好友 ---
            GUILayout.Space(10);
            friendIdToConfirm = EditorGUILayout.TextField("Friend ID to Confirm", friendIdToConfirm);

            if (GUILayout.Button("Confirm Friend"))
            {
                if (!string.IsNullOrEmpty(friendIdToConfirm))
                {
                    _instance.on_confirm_friend(friendIdToConfirm);
                    Debug.Log($"已触发确认好友事件，ID: {friendIdToConfirm}");
                }
                else
                {
                    Debug.LogWarning("请输入要确认的好友ID。");
                }
            }

            // --- 移除好友 ---
            GUILayout.Space(10);
            friendIdToRemove = EditorGUILayout.TextField("Friend ID to Remove", friendIdToRemove);
            if (GUILayout.Button("Remove Friend"))
            {
                if (!string.IsNullOrEmpty(friendIdToRemove))
                {
                    _instance.on_remove_friend(friendIdToRemove);
                    Debug.Log($"已触发移除好友事件，ID: {friendIdToRemove}");
                }
                else
                {
                    Debug.LogWarning("请输入要移除的好友ID。");
                }
            }

            // --- 拒绝好友 ---
            GUILayout.Space(10);
            friendIdToRefuse = EditorGUILayout.TextField("Friend ID to Refuse", friendIdToRefuse);
            if (GUILayout.Button("Refuse Friend"))
            {
                if (!string.IsNullOrEmpty(friendIdToRefuse))
                {
                    _instance.on_refuse_friend(friendIdToRefuse);
                    Debug.Log($"已触发拒绝好友事件，ID: {friendIdToRefuse}");
                }
                else
                {
                    Debug.LogWarning("请输入要拒绝的好友ID。");
                }
            }

            // --- 进入好友房间 ---
            GUILayout.Space(10);
            friendIdToEnterRoom = EditorGUILayout.TextField("Friend ID to Enter RoomData", friendIdToEnterRoom);
            if (GUILayout.Button("Enter Friend RoomData"))
            {
                if (!string.IsNullOrEmpty(friendIdToEnterRoom))
                {
                    _instance.on_enter_friend_room(friendIdToEnterRoom);
                    Debug.Log($"已触发进入好友房间事件，ID: {friendIdToEnterRoom}");
                }
                else
                {
                    Debug.LogWarning("请输入要进入房间的好友ID。");
                }
            }

            // --- 发送社交聊天消息 ---
            GUILayout.Space(10);
            GUILayout.Label("Send Social Chat Msg", EditorStyles.boldLabel);
            chatMsgFriendName = EditorGUILayout.TextField("Chat Friend Name", chatMsgFriendName);
            chatMsgId = EditorGUILayout.IntField("Chat Msg ID", chatMsgId);
            if (GUILayout.Button("Send Social Chat Msg"))
            {
                if (!string.IsNullOrEmpty(chatMsgFriendName))
                {
                    _instance.on_send_social_chat_msg(chatMsgFriendName, chatMsgId);
                    Debug.Log($"已触发发送社交聊天消息事件，FriendName: {chatMsgFriendName}, MsgID: {chatMsgId}");
                }
                else
                {
                    Debug.LogWarning("请输入聊天好友名称和消息ID。");
                }
            }

            // --- 更新社交聊天消息（从服务器） ---
            GUILayout.Space(10);
            GUILayout.Label("Update Social Chat Msg From Server", EditorStyles.boldLabel);
            updateChatMsgFriendName = EditorGUILayout.TextField("Friend Name to Update Chat Msg", updateChatMsgFriendName);
            if (GUILayout.Button("Update Social Chat Msg From Server"))
            {
                if (!string.IsNullOrEmpty(updateChatMsgFriendName))
                {
                    _instance.update_social_chat_msg_from_server(updateChatMsgFriendName);
                    Debug.Log($"已触发从服务器更新社交聊天消息事件，FriendName: {updateChatMsgFriendName}");
                }
                else
                {
                    Debug.LogWarning("请输入要更新聊天消息的好友名称。");
                }
            }

            // --- 送礼物功能 ---
            GUILayout.Space(10);
            GUILayout.Label("Send Present To Friend", EditorStyles.boldLabel);
            presentFriendName = EditorGUILayout.TextField("Present Receiver Name", presentFriendName);
            presentName = EditorGUILayout.TextField("Present Name", presentName);
            if (GUILayout.Button("Send Present To Friend"))
            {
                if (!string.IsNullOrEmpty(presentFriendName) && !string.IsNullOrEmpty(presentName))
                {
                    _instance.on_send_present_to_friend(presentFriendName, presentName);
                    Debug.Log($"已触发送礼物事件，Receiver: {presentFriendName}, Present: {presentName}");
                }
                else
                {
                    Debug.LogWarning("请输入收礼人和礼物名称。");
                }
            }

            GUILayout.Space(10);
            // --- 送礼物功能 ---
            GUILayout.Label("送礼物功能", EditorStyles.boldLabel);
            if (GUILayout.Button("Upload_Present_Records_to_Server"))
            {
                _instance.upload_present_records_to_server();
                Debug.Log($"Upload_Present_Records_to_Server_OK");
            }

            // --- 进入并设置为访问房间（使用 set_up_visit_room） ---
            GUILayout.Space(6);
            friendIdToVisitRoom = EditorGUILayout.TextField("Friend ID to Visit (setup)", friendIdToVisitRoom);
            if (GUILayout.Button("Enter Friend RoomData (Visit)"))
            {
                if (!string.IsNullOrEmpty(friendIdToVisitRoom))
                {
                    _instance.on_enter_friend_room(friendIdToVisitRoom);
                    Debug.Log($"已设置访问房间: {friendIdToVisitRoom} (set_up_visit_room)");
                }
                else
                {
                    Debug.LogWarning("请输入要访问的好友ID。");
                }
            }
        }
    }
#endif

}

