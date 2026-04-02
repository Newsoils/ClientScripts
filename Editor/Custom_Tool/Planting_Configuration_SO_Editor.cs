#if UNITY_EDITOR
using CLIP.Project_Mouse.Game_Play_System.Planting_System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(planting_configuration_SO))]
class planting_configuration_SO_Editor : Editor
{

    public planting_configuration_SO _obj;

    void OnEnable()
    {
        _obj = (planting_configuration_SO)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(32);
        if (GUILayout.Button("Update_Data_From_JSON"))
        {
            _obj.RefrehData();
        }

    }
}
#endif