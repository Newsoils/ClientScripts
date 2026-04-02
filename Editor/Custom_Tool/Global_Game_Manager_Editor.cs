using UnityEngine;
using CLIP.Project_Mouse.Game_Play_System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CLIP.Project_Mouse.Custom_Tool
{

#if UNITY_EDITOR
            [CustomEditor(typeof(Global_Game_Manager))]
            public class Global_Game_Manager_Editor : Editor
            {
                public Global_Game_Manager _instance;

                private void OnEnable()
                {
                    _instance = (Global_Game_Manager)target;

                }
                public override void OnInspectorGUI()
                {
                    DrawDefaultInspector();
                    if (GUILayout.Button("上传天气到服务器"))
                    {
                        if (_instance != null)
                        {
                            _instance.upload_weather_state();

                        }

                    }
                    if (GUILayout.Button("上传角色服装到服务器"))
                    {
                        if (_instance != null)
                        {
                            _instance.upload_main_character_cloth();

                        }

                    }
                    if (GUILayout.Button("Update Player Brief From Server"))
                    {
                        if (_instance != null)
                        {
                            _instance.on_update_player_brief_from_server();
                            Debug.Log("Called on_update_player_brief_from_server()");
                        }
                        else
                        {
                            Debug.LogWarning("Global_Game_Manager instance is null.");
                        }
                    }

                    if (GUILayout.Button("Upload Player Brief To Server"))
                    {
                        if (_instance != null)
                        {
                            _instance.on_upload_player_brief_to_server();
                            Debug.Log("Called on_upload_player_brief_to_server()");
                        }
                        else
                        {
                            Debug.LogWarning("Global_Game_Manager instance is null.");
                        }
                    }
                }
            }
#endif
}