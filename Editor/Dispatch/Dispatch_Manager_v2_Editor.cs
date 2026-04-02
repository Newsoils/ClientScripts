using UnityEngine;
using UnityEditor;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;


namespace CLIP.Project_Mouse.Custom_Tool
{
    [CustomEditor(typeof(Dispatch_Manager))]
    public class Dispatch_Manager_v2_Editor : Editor
    {
        Dispatch_Manager _instance;

        void OnEnable()
        {
            _instance = (Dispatch_Manager)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(8);
            GUILayout.Label("Dispatch Manager Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Clear Previous Dispatch"))
            {
                if (_instance == null)
                {
                    Debug.LogWarning("Dispatch_Manager_v2 instance is null.");
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Confirm", "Invoke on_clear_previous_dispatch()?", "Yes", "No"))
                    {
                        _instance.on_clear_previous_dispatch();
                        Debug.Log("Dispatch_Manager_v2: on_clear_previous_dispatch invoked.");
                    }
                }
            }
        }
    }
}