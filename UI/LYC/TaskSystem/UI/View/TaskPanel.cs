using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class TaskPanel : UIPanelBase
    {
        // 任务单元预制体
        [SerializeField] public TaskUnitView taskUnitPrefab;

        [SerializeField] 
        public Transform taskUnitRoot;
        public GameObject obj;
        public Button exitButton;

        public void Start()
        {
            exitButton.onClick.AddListener(ClosePanel);
            ClosePanel();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            exitButton.onClick.RemoveAllListeners();
        }

        public override void OpenPanel(params object[] data)
        {
            obj.SetActive(true);

        }

        public override void ClosePanel()
        {
            obj.SetActive(false);
        }

        public override void UpdatePanel(params object[] data)
        {

        }



        // 创建单个任务单元 DoCreateTaskUnitView
        public async void DoCreateTaskUnitView(TaskModel taskData, Task_RuntimeData taskInstance, List<RewardData> rewards)
        {
            // 实例化一个 taskUnitPrefab
            TaskUnitView taskUnitView = Instantiate(taskUnitPrefab, taskUnitRoot);

            // 直接利用 Task_RuntimeData 的数据对 taskUnitView 进行创建
            // 1. 添加奖励文字
            taskUnitView.descriptionText.text = taskData.desc;
            // 2. 为 taskInstance 绑定对应的 taskUnitView 的方法
            //taskInstance.SetTaskUnitView(taskUnitView);
            taskInstance.OnProgressChange += taskUnitView.UpdateView;
            taskInstance.OnTaskFinished += taskUnitView.SetAsFinished;
            taskInstance.OnTaskDestroy += taskUnitView.SelfDestroy;
            // 3. 设置 taskUnitView 回调
            taskUnitView.confirmButton.onClick.AddListener(() => EventCenter.Publish(new ClickConfirmButtonEvent(taskInstance)));

            taskUnitView.gameObject.SetActive(true);

            // 4. 最后再添加奖励图标，防止前面的逻辑延误执行
            await taskUnitView.SetRewardIcons(taskData.RewardNames);

            Debug.Log("创建了一个任务单元");
        }



    }


}
