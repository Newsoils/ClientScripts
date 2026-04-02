using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.Custom_Tool
{
    [CustomEditor(typeof(NPC_Data_SO))]
    class NPC_Data_SO_Editor : Editor
    {
        public NPC_Data_SO _obj;
        //GameObject script_object;
        void OnEnable()
        {
            _obj = (NPC_Data_SO)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(32);
            if (GUILayout.Button("Update_NPC_Data_From_JSON"))
            {
                _obj.RefreshData();
            }
        }

    }
}
