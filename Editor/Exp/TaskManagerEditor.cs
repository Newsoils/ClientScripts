using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using System.Linq;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.UI;


namespace CLIP.Project_Mouse.Custom_Tool
{
#if UNITY_EDITOR
 
    [CustomEditor(typeof(TaskManager))]
    public class TaskManagerEditor : Editor
    {
        private int _selectedTaskId = -1;
        private int _triggerCount = 1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10f);
            EditorGUILayout.LabelField("调试", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("进入运行模式后可使用以下调试工具。", MessageType.Info);
                return;
            }

            var mgr = TaskManager.Instance;
            if (mgr == null)
            {
                EditorGUILayout.HelpBox("TaskManager.Instance 为空，请确认游戏已启动。", MessageType.Warning);
                return;
            }

            // ---------- 任务列表 ----------
            GUILayout.Space(6f);
            EditorGUILayout.LabelField("所有任务状态", EditorStyles.boldLabel);

            var taskIds = mgr._taskRuntimeDatas.Keys.OrderBy(id => id).ToList();
            if (taskIds.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无比展开的任务。", MessageType.None);
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                foreach (var id in taskIds)
                {
                    if (!mgr.taskModelsDic.TryGetValue(id, out var model))
                        continue;
                    if (!mgr._taskRuntimeDatas.TryGetValue(id, out var runtime))
                        continue;

                    string status;
                    if (runtime.IsFinish)
                        status = "已完成";
                    else if (!runtime.IsAccept)
                        status = "未解锁";
                    else if (runtime.CanGetReward)
                        status = "可领取";
                    else
                        status = $"进行中 {runtime.CurrentCount}/{runtime.TargetCount}";

                    EditorGUILayout.LabelField($"  [{id}] {model.desc}", status);
                }
                EditorGUILayout.EndVertical();
            }

            // ---------- 触发单个任务 ----------
            GUILayout.Space(6f);
            EditorGUILayout.LabelField("触发单个任务进度", EditorStyles.boldLabel);

            _selectedTaskId = EditorGUILayout.IntField("Task ID", _selectedTaskId);
            _triggerCount = EditorGUILayout.IntField("触发次数", _triggerCount);

            EditorGUI.BeginDisabledGroup(_selectedTaskId < 0);
            if (GUILayout.Button($"触发 Task {_selectedTaskId}"))
            {
                TaskEvent.Trigger(_selectedTaskId, _triggerCount);
                Debug.Log($"[TaskManagerEditor] 触发 TaskID={_selectedTaskId}, count={_triggerCount}");
            }
            EditorGUI.EndDisabledGroup();

            // ---------- 快捷操作 ----------
            GUILayout.Space(6f);
            EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(taskIds.Count == 0);

            if (GUILayout.Button("立即完成所有已接受任务（强制）"))
            {
                int count = 0;
                foreach (var kvp in mgr._taskRuntimeDatas)
                {
                    if (kvp.Value.IsAccept && !kvp.Value.IsFinish && !mgr.IsRecurTask(kvp.Key))
                    {
                        var runtime = kvp.Value;
                        var field = typeof(Task_RuntimeData).GetField("_count",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        field?.SetValue(runtime, runtime.TargetCount);
                        typeof(Task_RuntimeData).GetProperty("CanGetReward")?.SetValue(runtime, true);
                        count++;
                    }
                }
                Debug.Log($"[TaskManagerEditor] 强制完成 {count} 个任务（循环任务已跳过）");

                if (count > 0 && mgr.AllTasksFinishedOnce())
                    mgr.ActivateRecurTasksForEditor(mgr.CurrentCycle + 1);
                EvtDsp.TriggerEvt(EvtNames.Task_ClaimableChanged);
            }

            if (GUILayout.Button("一键领取所有可领取任务"))
            {
                mgr.ClaimAllRewardForEditor();
                EvtDsp.TriggerEvt(EvtNames.Task_ClaimableChanged);
                Debug.Log($"[TaskManagerEditor] 批量领取所有可领取任务奖励");
            }

            if (GUILayout.Button("手动触发 Task_Unlocked（模拟升级）"))
            {
                EvtDsp.TriggerEvt(EvtNames.Task_Unlocked);
                Debug.Log("[TaskManagerEditor] 触发 Task_Unlocked");
            }

            //if (GUILayout.Button("强制激活所有循环任务"))
            //{
            //    int count = 0;
            //    foreach (var model in mgr.taskModelsDic.Values)
            //    {
            //        if (!model.isRecurTask)
            //            continue;
            //        if (!mgr._taskRuntimeDatas.TryGetValue(model.taskId, out var runtime))
            //            continue;
            //        runtime.AcceptRecurTask(1);
            //        count++;
            //    }
            //    Debug.Log($"[TaskManagerEditor] 激活 {count} 个循环任务");
            //}

            //if (GUILayout.Button("模拟所有普通任务完成（激活循环任务）"))
            //{
            //    foreach (var kvp in mgr._taskRuntimeDatas)
            //    {
            //        if (!mgr.taskModelsDic.TryGetValue(kvp.Key, out var model))
            //            continue;
            //        if (model.isRecurTask)
            //            continue;
            //        kvp.Value.AcceptRecurTask(1);
            //    }
            //    Debug.Log("[TaskManagerEditor] 模拟所有普通任务完成");
            //}

            EditorGUI.EndDisabledGroup();

            // ---------- 存档 ----------
            GUILayout.Space(6f);
            EditorGUILayout.LabelField("存档", EditorStyles.boldLabel);

            if (GUILayout.Button("删除任务存档（重新开始）"))
            {
                var savePath = Application.persistentDataPath + "/TaskData.json";
                if (System.IO.File.Exists(savePath))
                {
                    System.IO.File.Delete(savePath);
                    Debug.Log($"[TaskManagerEditor] 已删除存档：{savePath}");
                }
                else
                {
                    Debug.Log("[TaskManagerEditor] 未找到存档文件。");
                }
                mgr.ReloadAllTasks();
            }
        }
    }
#endif
}
