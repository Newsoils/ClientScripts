using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Room))]
class Room_Editor:Editor
{
    Room _obj;
    //GameObject script_object;

    void OnEnable()
    {
        _obj = (Room)target;
        // script_object = _obj.gameObject;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(32);
        if (GUILayout.Button("GetRenderer"))
        {
            _obj.GetRenders();
            EditorUtility.SetDirty(_obj);
        }

        if(GUILayout.Button("ClearRenderer"))
        {
            _obj.ClearRenders();
        }

        if(GUILayout.Button("GetDoors"))
        {
            _obj.GetDoorTs();
            EditorUtility.SetDirty(_obj);
        }

        if(GUILayout.Button("GetGridLayers"))
        {
            _obj.GetGridLayers();
            EditorUtility.SetDirty( _obj);
        }

        GUILayout.Space(32);

        if (GUILayout.Button("Active_Room"))
        {
            _obj.ActiveRoom(true);

        }
        if (GUILayout.Button("Deactive_Room"))
        {
            _obj.ActiveRoom(false);
        }
      
    }
}