
using UnityEngine;
using System.Linq;
using CLIP.Project_Mouse.Game_Play_System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Custom_Tool
        {
#if UNITY_EDITOR
            [CustomEditor(typeof(Quest_And_Achievement_Manager))]
            public class Quest_And_Achievement_Manager_Editor : Editor
            {
                Quest_And_Achievement_Manager _instance;
                private string achievementNameToGet = "";

                void OnEnable()
                {
                    _instance = (Quest_And_Achievement_Manager)target;
                }

                public override void OnInspectorGUI()
                {
                    DrawDefaultInspector();

                    GUILayout.Space(10);
                    GUILayout.Label("Achievement Tools", EditorStyles.boldLabel);

                    achievementNameToGet = EditorGUILayout.TextField("Achievement_Name", achievementNameToGet);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("On_get_achievement_reward"))
                    {
                        _instance.on_get_achievement_reward(achievementNameToGet);
                        Debug.Log($"Invoked on_get_achievement_reward with '{achievementNameToGet}'.");
                    }


                    GUILayout.EndHorizontal();
                }
            }
#endif
        }
    }
}
