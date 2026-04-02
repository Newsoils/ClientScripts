using UnityEditor;
using UnityEngine;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
namespace CLIP.Project_Mouse.Custom_Tool
{

    [CustomEditor(typeof(Global_Home_Room_Manager))]
    public class Global_Home_Room_Manager_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Global_Home_Room_Manager manager = (Global_Home_Room_Manager)target;

            GUILayout.Space(16);
            GUILayout.Label("NavMesh Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Re-Bake NavMesh"))
            {
                manager.re_bake_navmesh();
                Debug.Log("已触发 NavMesh 重烘焙 (re_bake_navmesh)。");
            }
        }


    }
}

