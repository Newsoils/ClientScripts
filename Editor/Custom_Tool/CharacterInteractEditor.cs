using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.Custom_Tool
{
    [CustomEditor(typeof(CharacterInteractPointTool))]
    public class CharacterInteractEditor : Editor
    {
        CharacterInteractPointTool instance;
        private void OnEnable()
        {
            instance = (CharacterInteractPointTool)target;
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if(GUILayout.Button("生成家具"))
            {
                instance.GenerateFurniture();
            }
            if(GUILayout.Button("设置动作"))
            {
                instance.ChangeInteract();
            }
            if(GUILayout.Button("旋转角色"))
            {
                instance.RotatePlayer();
            }
            if(GUILayout.Button("保存配置"))
            {
                instance.SaveInteractPositionInfo();
            }
            if(GUILayout.Button("加载配置"))
            {
                instance.LoadInteractPositionInfo();
            }
        }
    }
}

