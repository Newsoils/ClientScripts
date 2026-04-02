using UnityEditor;
using UnityEngine;
using CLIP.Project_Mouse.Game_Play_System;

namespace CLIP.Project_Mouse.Custom_Tool
{

    [CustomEditor(typeof(Achievement_Design_Info_DB_SO))]
    public class Achievement_Design_Info_DB_SO_Editor : Editor
    {
        Achievement_Design_Info_DB_SO so;

        void OnEnable()
        {
            so = (Achievement_Design_Info_DB_SO)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(12);
            GUILayout.Label("Achievement DB Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Load Achievement DB From JSON"))
            {
                if (so != null)
                {
                    so.LoadFromJson();
                    EditorUtility.SetDirty(so);
                    Debug.Log("Achievement_Design_Info_DB_SO: LoadFromJson() executed.");
                }
            }

            if (GUILayout.Button("Export DB to JSON File"))
            {
                if (so == null)
                {
                    Debug.LogWarning("SO is null.");
                    return;
                }

                string json = so.SerializeToJson(true);
                string path = EditorUtility.SaveFilePanel("Export Achievement DB", Application.dataPath, "achievement_db", "json");
                if (!string.IsNullOrEmpty(path))
                {
                    System.IO.File.WriteAllText(path, json);
                    AssetDatabase.Refresh();
                    Debug.Log($"Achievement DB exported to: {path}");
                }
            }
        }
    }
}