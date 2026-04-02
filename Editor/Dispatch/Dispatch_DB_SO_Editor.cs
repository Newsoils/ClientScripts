using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dispatch_DB_SO))]
class Dispatch_DB_SO_Editor : Editor
{
    public Dispatch_DB_SO _obj;

    void OnEnable()
    {
        _obj = (Dispatch_DB_SO)target;
        _obj.RefreshData();
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(32);
        if (GUILayout.Button("Update_Data_From_JSON"))
        {
            _obj.RefreshData();
        }
    }
}
