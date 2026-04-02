using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class TaskMgr : MonoBehaviour
    {
        // 所有任务动态数据
        private Dictionary<int, Task_RuntimeData> TaskRuntimeDatas;

        // 静态数据：所有任务相关数据，从 Json 数据中加载得到
        // Dictionary<taskId, TaskModel>
        private Dictionary<int, TaskModel> AllTaskDatas;

        private string taskDataPath = "Json/project_mouse_lyc_tb_task";

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            foreach (var c in TaskRuntimeDatas.Values)
            {
                c.AcceptTask();
            }
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
        }


        #region 内部方法

        // ========== TaskPanelController 初始化相关 ==========

        // 初始化
        private void Init()
        {
            // 加载所有任务数据
            LoadAllTaskDatas();
            CreateAllTasks();

        }

        // 加载所有的任务数据
        private void LoadAllTaskDatas()
        {
            AllTaskDatas = new Dictionary<int, TaskModel>();

            TextAsset jsonFile = Resources.Load<TextAsset>(taskDataPath);
            List<TaskModel> taskDatas = JsonConvert.DeserializeObject<List<TaskModel>>(jsonFile.text);

            foreach (TaskModel task in taskDatas)
            {
                AllTaskDatas[task.taskId] = task;
            }

        }

        // 创建所有任务
        private void CreateAllTasks()
        {
            TaskRuntimeDatas = new Dictionary<int, Task_RuntimeData>();
            foreach (TaskModel task in AllTaskDatas.Values)
            {
                // 得到对应的完成任务的条件（保持和 TaskModel 同样的数据）
                ICondition condition = new MultipleOperationsCondition(Enum_ConditionType.MultipleOperations,
                    task.desc, task.targetCount);
                // 创建动态数据
                Task_RuntimeData newTask = new Task_RuntimeData(task.Rewards, condition);
                // 利用动态数据添加任务单元 TaskUnitView
                UIManager.Instance.GetPanel<TaskPanel>().DoCreateTaskUnitView(task, newTask, task.Rewards);
                // 添加到 TaskRuntimeDatas 中管理 
                TaskRuntimeDatas.Add(task.taskId, newTask);
            }

        }



        // ========== 事件 handlers ==========

        // 事件 handler：这里实现一个处理收获奖励（确认）的处理方法，是为了之后显现奖励面板的需求
        private void ShowRewardPanel(PlayerRewardData rewardData)
        {
            // 这里实现奖励面板的显示
            Debug.Log("奖励面板显示");
            Debug.Log($"收获奖励：经验值 {rewardData.ex_value}");

            List<(string, int)> itemCount = rewardData.ItemRewards.Select
                (x => (Global_Inventory_Manager.GameItem_DB.Find(y => y.item_id == x.itemId).name, x.amount)).ToList();

            EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, itemCount);
        }

        // 事件 handler：发放奖励到玩家背包
        private void OnGetRewards(ClickConfirmButtonEvent evt)
        {
            // 检测是否成功拿到奖励
            if (!evt.taskInstance.TryGetRewards(out PlayerRewardData rewardData))
            {
                Debug.Log("当前不可以领取奖励");
                return;
            }

            // 要先保证面板显示
            ShowRewardPanel(rewardData);

            Debug.Log("成功领取奖励，奖励已经发放到玩家仓库中");
            // 真正分发奖励，就是将奖励数据真正加入到玩家数据中
            RewardDistributor.DistributePlayerRewards(rewardData);
            // 完成任务，并且失活确认按钮以防止重复点击，取消事件订阅
            evt.taskInstance.FinishTask();

        }

        // 事件 handler：收到任务相关的操作
        private void OnReceiveOperation(int taskId, int operationTimes)
        {
            TaskRuntimeDatas[taskId].AddOperationTimes(operationTimes);
        }


        // ========== 其他 ==========

        #endregion


        #region 公开方法

        // 触发任务达成相关事件（比如：ClickConfirmButtonEvent）
        // 使用方法：依照 eventName 进行匹配，要触发什么事件，就写上对应事件的名字
        public void TriggerEvent(int taskId, int triggerTimes)
        {
            //EventCenter.Publish<MultipleOperationsEvent>(new MultipleOperationsEvent(eventName, triggerTimes));
            EvtDsp.TriggerEvt<int, int>("MultipleOperations", taskId, triggerTimes);
        }

        // 移除任务（按任务 id）
        public void RemoveTask(int id)
        {
            TaskRuntimeDatas[id].DestroyTaskDatas();
            TaskRuntimeDatas.Remove(id);
        }



        #endregion


    }
}
