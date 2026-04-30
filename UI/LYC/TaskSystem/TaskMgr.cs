using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class TaskMgr : MonoBehaviour
    {
        public static TaskMgr Instance { get; private set; }

        private Dictionary<int, Task_RuntimeData> _taskRuntimeDatas = new();
        private Dictionary<int, TaskModel> _allTaskModels = new();
        private const string TaskDataPath = "Json/project_mouse_lyc_tb_task";

        private void Awake()
        {
            Instance = this;
            LoadAllTaskModels();
            RestoreTaskData();
            CreateAllTasks();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            foreach (var t in _taskRuntimeDatas.Values)
            {
                if (!t.IsAccept)
                    t.AcceptTask();
            }
            NotifyClaimableChanged();
        }

        private void OnEnable()
        {
            EventCenter.Subscribe<ClickConfirmButtonEvent>(OnGetRewards);
            EvtDsp.AddEvt<int, int>("MultipleOperations", OnReceiveOperation);
        }

        private void OnDisable()
        {
            EventCenter.Unsubscribe<ClickConfirmButtonEvent>(OnGetRewards);
            EvtDsp.RemoveEvt<int, int>("MultipleOperations", OnReceiveOperation);
            SaveTaskData();
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

        private void LoadAllTaskModels()
        {
            var jsonFile = Resources.Load<TextAsset>(TaskDataPath);
            var taskModels = JsonConvert.DeserializeObject<List<TaskModel>>(jsonFile.text);
            foreach (var task in taskModels)
                _allTaskModels[task.taskId] = task;
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
                    _savedTaskDict[t.taskId] = t;
                Debug.Log($"任务数据加载成功，共 {savedData.tasks.Count} 个");
            }
        }

        private void CreateAllTasks()
        {
            foreach (var model in _allTaskModels.Values)
            {
                var condition = new MultipleOperationsCondition(
                    Enum_ConditionType.MultipleOperations, model.desc, model.targetCount);
                var runtime = new Task_RuntimeData(model.Rewards, condition);
                _taskRuntimeDatas.Add(model.taskId, runtime);

                bool isAlreadyFinished = _savedTaskDict != null
                    && _savedTaskDict.TryGetValue(model.taskId, out var saved)
                    && saved.isFinish;

                var panel = UIManager.Instance.GetPanel<TaskPanel>();
                panel.CreateTaskUnit(model, runtime, isAlreadyFinished);

                if (_savedTaskDict != null && _savedTaskDict.TryGetValue(model.taskId, out saved))
                {
                    runtime.AcceptTask();
                    saved.ApplyTo(runtime);
                }
            }
        }

        private void SaveTaskData()
        {
            if (_taskRuntimeDatas.Count == 0)
                return;

            var saveData = new TaskSaveData();
            foreach (var pair in _taskRuntimeDatas)
            {
                if (!_allTaskModels.TryGetValue(pair.Key, out var model))
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

        private void OnGetRewards(ClickConfirmButtonEvent evt)
        {
            if (!evt.taskInstance.TryGetRewards(out var rewardData))
            {
                Debug.Log("当前不可以领取奖励");
                return;
            }

            RewardDistributor.DistributePlayerRewards(rewardData);
            evt.taskInstance.FinishTask();
            NotifyClaimableChanged();

            Debug.Log($"奖励面板显示，收获奖励：经验值 {rewardData.ex_value}");
            var itemCount = rewardData.ItemRewards.ConvertAll(
                x => (Global_Inventory_Manager.GameItem_DB.Find(y => y.item_id == x.itemId).name, x.amount));
            EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, itemCount);
        }

        private static void NotifyClaimableChanged()
        {
            EvtDsp.TriggerEvt(EvtNames.Task_ClaimableChanged);
        }

        private Dictionary<int, TaskRuntimeSaveData> _savedTaskDict;
    }
}
