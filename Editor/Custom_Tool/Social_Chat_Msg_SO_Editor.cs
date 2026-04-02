using UnityEngine;
using CLIP.Project_Mouse.Game_Play_System;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CLIP.Project_Mouse.Custom_Tool
{

#if UNITY_EDITOR
    [CustomEditor(typeof(Social_Chat_Msg_SO))]
    public class Social_Chat_Msg_SO_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Social_Chat_Msg_SO so = (Social_Chat_Msg_SO)target;

            GUILayout.Space(16);
            GUILayout.Label("Social Chat Msg DB Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("加载聊天消息配置 (LoadChatMsgDbFromJson)"))
            {
                so.LoadChatMsgDbFromJson();
                Debug.Log("已触发加载聊天消息配置。");
            }
        }
    }
#endif
}