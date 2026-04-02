using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ButtonSFX))]
public class ButtonSFXEditor : Editor
{
    ButtonSFX instance;
    private void OnEnable()
    {
        instance = (ButtonSFX)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("GetButtons"))
        {
            instance.GetButtons();
        }
    }
}
