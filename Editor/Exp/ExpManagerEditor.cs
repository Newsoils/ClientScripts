using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.Custom_Tool
{
    [CustomEditor(typeof(ExpManager))]
    public class ExpManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("调试", EditorStyles.boldLabel);

            var exp = (ExpManager)target;
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (GUILayout.Button("立刻获得 100 经验"))
                exp.Editor_DebugAddExp100();
            if (GUILayout.Button("清空等级与经验（Lv.1 / 0 EXP）"))
                exp.Editor_DebugResetLevelAndExp();
            EditorGUI.EndDisabledGroup();

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("进入运行模式后可用（会走加经验 / 存档与服务器同步逻辑）。", MessageType.Info);
        }
    }
}
