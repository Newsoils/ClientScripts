using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CLIP.Project_Mouse.UI;
//using static UnityEngine.GraphicsBuffer;

namespace CLIP.Project_Mouse
{

    [CustomEditor(typeof(Phone_Animator))]
    public class Phone_Animator_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 保留原有 Inspector
            DrawDefaultInspector();

            // 添加自定义按钮
            Phone_Animator phone = (Phone_Animator)target;
            GUILayout.Space(10);
            if (GUILayout.Button("关闭手机 (Close_Phone)"))
            {
                phone.Close_Phone();
            }

            if (GUILayout.Button("开手机 (Close_Phone)"))
            {
                phone.Open_Phone();
            }

            if (GUILayout.Button("Match_UI"))
            {
                phone.MatchUI();
            }
        }
    }
}
