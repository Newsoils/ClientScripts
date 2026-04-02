using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CLIP.Project_Mouse.Custom_Tool
{

    [CustomEditor(typeof(ColliderGenerateTool))]
    public class Collider_Generate_Tool_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ColliderGenerateTool tool = (ColliderGenerateTool)target;

            if (GUILayout.Button("GenerateCollider"))
            {
                tool.GenerateCollider();
            }
        }
    }
}
