using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

namespace CLIP.Project_Mouse.Custom_Tool
{
    using CLIP.Framework_Core.Event;
#if UNITY_EDITOR
    using UnityEditor;

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

            GUILayout.Space(6f);
            EditorGUILayout.LabelField("任务系统调试", EditorStyles.boldLabel);

            if (GUILayout.Button("手动触发 Task_Unlocked 事件（模拟升级）"))
                EvtDsp.TriggerEvt(EvtNames.Task_Unlocked);

            EditorGUI.EndDisabledGroup();

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("进入运行模式后可用（会走加经验 / 存档与服务器同步逻辑）。", MessageType.Info);
        }
    }
#endif
}
