using UnityEngine;
using UnityEditor;
using CLIP.Project_Mouse.Game_Play_System;
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

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("立刻出门"))
                {
                    if (!Application.isPlaying)
                        return;
                    var ggm = Global_Game_Manager.Instance;
                    if (ggm != null && ggm.IsCatCurrentlyTraveling())
                    {
                        Debug.Log("Dispatch tools: 已在出游中，无法再次出门。");
                        return;
                    }
                    _instance.force_dispatch_start();


                }
            }

            //if (GUILayout.Button("Clear Previous Dispatch"))
            //{
            //    if (Instance == null)
            //    {
            //        Debug.LogWarning("Dispatch_Manager_v2 Instance is null.");
            //    }
            //    else
            //    {
            //        if (EditorUtility.DisplayDialog("Confirm", "Invoke on_clear_previous_dispatch()?", "Yes", "No"))
            //        {
            //            Instance.on_clear_previous_dispatch();
            //            Debug.Log("Dispatch_Manager_v2: on_clear_previous_dispatch invoked.");
            //        }
            //    }
            //}
        }
    }
}