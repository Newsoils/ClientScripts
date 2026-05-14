using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class TaskManager  : SingletonMono<TaskManager>
    {
        public Dictionary<int, TaskModel> taskModelsDic = new();
        public Dictionary<int, Task_RuntimeData> _taskRuntimeDatas = new();

        private Dictionary<int, TaskRuntimeSaveData> _savedTaskDict;
        private Dictionary<int, List<TaskModel>> _taskGroupByLevel;

        private List<TaskModel> _recurTasks;
        private HashSet<int> _recurTaskIds = new();
        private int _currentCycle;

        public int CurrentCycle => _currentCycle;

        public bool IsRecurTask(int taskId) => _recurTaskIds.Contains(taskId);


        private void Start()
        {
            JsonData_Manager.LoadTaskData(out taskModelsDic, out _taskGroupByLevel,out _recurTasks);
            foreach (var t in _recurTasks)
                _recurTaskIds.Add(t.taskId);
            RestoreTaskData();
        }

        private void OnEnable()
        {
            EvtDsp.AddEvt<int>(EvtNames.Task_ClaimReward, OnClaimReward);
            EvtDsp.AddEvt<int, int>(TaskEvent.Task_Progress, OnReceiveOperation);
            EvtDsp.AddEvt(EvtNames.Task_Unlocked, OnTaskUnlocked);
            EvtDsp.AddEvt(EvtNames.PlayerLevelDataLoaded, OnPlayerLevelDataLoaded);
        }

        private void OnDisable()
        {
            EvtDsp.RemoveEvt<int>(EvtNames.Task_ClaimReward, OnClaimReward);
            EvtDsp.RemoveEvt<int, int>(TaskEvent.Task_Progress, OnReceiveOperation);
            EvtDsp.RemoveEvt(EvtNames.Task_Unlocked, OnTaskUnlocked);
            EvtDsp.RemoveEvt(EvtNames.PlayerLevelDataLoaded, OnPlayerLevelDataLoaded);
            SaveTaskData();
        }

        private void OnPlayerLevelDataLoaded()
        {
            CreateAllTasks();
            NotifyClaimableChanged();
        }

        /// <summary>
        /// 重置所有任务，测试使用
        /// </summary>
        public void ReloadAllTasks()
        {
            _taskRuntimeDatas.Clear();
            _savedTaskDict.Clear();
            CreateAllTasks();
            NotifyClaimableChanged();
            SaveTaskData();
        }

        private void OnTaskUnlocked()
        {
            int currentLevel = ExpManager.Instance.curLevel;
            TryAcceptTasksForLevel(currentLevel);
            NotifyClaimableChanged();
        }

        private void TryAcceptTasksForLevel(int level)
        {
            if (!_taskGroupByLevel.TryGetValue(level, out var models))
                return;

            foreach (var model in models)
            {
                if (!_taskRuntimeDatas.TryGetValue(model.taskId, out var runtime))
                    continue;
                if (runtime.IsAccept)
                    continue;

                runtime.AcceptTask();
            }
        }

        private void OnApplicationQuit()
        {
            SaveTaskData();
        }

        public void TriggerEvent(int taskId, int triggerTimes)
        {
            if (_taskRuntimeDatas.TryGetValue(taskId, out var task))
                task.AddOperationTimes(triggerTimes);
        }

        public void RemoveTask(int taskId)
        {
            if (!_taskRuntimeDatas.TryGetValue(taskId, out var task))
                return;

            task.DestroyTaskDatas();
            _taskRuntimeDatas.Remove(taskId);
            NotifyClaimableChanged();
        }

        public bool HasClaimableReward()
        {
            foreach (var t in _taskRuntimeDatas.Values)
            {
                if (t.CanGetReward && !t.IsFinish)
                    return true;
            }
            return false;
        }

        private void CreateAllTasks()
        {
            int currentLevel = ExpManager.Instance.curLevel;

            foreach (var kvp in _taskGroupByLevel)
            {
                int level = kvp.Key;
                bool shouldAccept = level <= currentLevel;

                foreach (var model in kvp.Value)
                {
                    if (_taskRuntimeDatas.ContainsKey(model.taskId))
                        continue;

                    var runtime = new Task_RuntimeData(model.taskId, model.desc, model.targetCount);
                    _taskRuntimeDatas.Add(model.taskId, runtime);

                    if (_savedTaskDict != null && _savedTaskDict.TryGetValue(model.taskId, out var saved))
                    {
                        saved.ApplyTo(runtime);
                    }
                   if (shouldAccept)
                    {
                        runtime.AcceptTask();
                    }
                    else
                    {
                        runtime.RejectTask();
                    }
                }

            }

            if(AllTasksFinishedOnce())
            {
                _currentCycle++;
                ActivateRecurTasks(_currentCycle);
            }

        }

        private void RestoreTaskData()
        {
            var savedData = TaskSaveDataExtensions.Load();
            if (savedData != null)
                _savedTaskDict = savedData.tasks.Count > 0
                    ? new Dictionary<int, TaskRuntimeSaveData>(savedData.tasks.Count)
                    : null;
            if (_savedTaskDict != null)
            {
                foreach (var t in savedData.tasks)
                {
                    _savedTaskDict[t.taskId] = t;
                    if (t.cycle > _currentCycle)
                        _currentCycle = t.cycle;
                }
                Debug.Log($"任务数据加载成功，共 {savedData.tasks.Count} 个");
            }
        }

        private void SaveTaskData()
        {
            if (_taskRuntimeDatas.Count == 0)
                return;

            var saveData = new TaskSaveData();
            foreach (var pair in _taskRuntimeDatas)
            {
                if (!taskModelsDic.TryGetValue(pair.Key, out var model))
                    continue;
                saveData.tasks.Add(TaskRuntimeSaveData.From(model, pair.Value));
            }
            TaskSaveDataExtensions.Save(saveData);
        }

        private void OnReceiveOperation(int taskId, int triggerTimes)
        {
            if (_taskRuntimeDatas.TryGetValue(taskId, out var task))
            {
                task.AddOperationTimes(triggerTimes);
                NotifyClaimableChanged();
            }
        }

        private void OnClaimReward(int taskId)
        {
            if (!_taskRuntimeDatas.TryGetValue(taskId, out var task))
                return;

            if (!task.CanGetReward)
            {
                Debug.Log("当前不可以领取奖励");
                return;
            }

            if (!taskModelsDic.TryGetValue(taskId, out var model))
                return;

            var rewardItems = new List<(int itemId, int amount)>();
            foreach (var r in model.Rewards)
            {
                if (r.rewardType == Enum_RewardType.Item)
                    rewardItems.Add((r.targetId, r.amount));
            }

            RewardDistributor.Distribute(rewardItems, model.Rewards, model.RewardNames);
            task.FinishTask();

            if (AllTasksFinishedOnce())
            {
                _currentCycle++;
                ActivateRecurTasks(_currentCycle);
            }

            NotifyClaimableChanged();
        }

        /// <summary>
        /// 检查 taskModelsDic 中所有任务（包括普通任务和循环任务）是否都已完成一遍。
        /// </summary>
        public bool AllTasksFinishedOnce()
        {
            foreach (var model in taskModelsDic.Values)
            {
                if (!_taskRuntimeDatas.TryGetValue(model.taskId, out var runtime))
                    continue;
                if (!runtime.IsFinish)
                    return false;
            }
            return taskModelsDic.Count > 0;
        }

        public void ClaimAllRewardForEditor()
        {
            var rewardItems = new List<(int itemId, int amount)>();

            foreach (var kvp in _taskRuntimeDatas)
            {
                if (kvp.Value.CanGetReward && !kvp.Value.IsFinish)
                {
                    if (!taskModelsDic.TryGetValue(kvp.Key, out var model))
                        continue;
                    foreach (var r in model.Rewards)
                    {
                        if (r.rewardType == Enum_RewardType.Item)
                            rewardItems.Add((r.targetId, r.amount));
                    }
                    kvp.Value.FinishTask();
                }
            }

            if (rewardItems.Count > 0)
                RewardDistributor.Distribute(rewardItems, null, null);

            if (AllTasksFinishedOnce())
            {
                _currentCycle++;
                ActivateRecurTasks(_currentCycle);
            }

            NotifyClaimableChanged();
        }

        public void ActivateRecurTasksForEditor(int cycle)
        {
            ActivateRecurTasks(cycle);
        }

        private void ActivateRecurTasks(int cycle)
        {
            foreach (var model in _recurTasks)
            {
                if (!_taskRuntimeDatas.TryGetValue(model.taskId, out var runtime))
                    continue;
                runtime.AcceptRecurTask(cycle);
            }
            Debug.Log($"所有任务完成一遍，循环任务已激活（Cycle {_currentCycle}）");
        }

        private static void NotifyClaimableChanged()
        {
            EvtDsp.TriggerEvt(EvtNames.Task_ClaimableChanged);
        }
    }
}
