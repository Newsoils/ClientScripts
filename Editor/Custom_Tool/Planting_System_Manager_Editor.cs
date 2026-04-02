//using UnityEngine;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using UnityEditor;
//namespace CLIP.Project_Mouse.Custom_Tool
//{

//    [CustomEditor(typeof(Planting_System_Manager))]
//    public class Planting_System_Manager_Editor : Editor
//    {
//        public Planting_System_Manager _instance;

//        private void OnEnable()
//        {
//            _instance = (Planting_System_Manager)target;

//        }
//        public override void OnInspectorGUI()
//        {
//            DrawDefaultInspector();
//            GUILayout.Space(10);
//            GUILayout.Label("Custom Editor Tool", EditorStyles.boldLabel);
//            if (GUILayout.Button("Saving_Current_State_to_Disk"))
//            {
//                if (_instance != null)
//                {
//                    _instance.saving_current_state_to_disk_as_json("Planting_saved.json");
//                }
//            }

//            if (GUILayout.Button("Upload_Data_to_Server"))
//            {
//                if (_instance != null)
//                {
//                    _instance.upload_data_to_server();
//                }

//            }
//        }
//    }

//}
