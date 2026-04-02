using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cloth_DB_SO))]
class Cloth_DB_SO_Editor : Editor
{
    public Cloth_DB_SO _obj;

    void OnEnable()
    {
        _obj = (Cloth_DB_SO)target;
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
