using System;
using CLIP.Framework_Core.Event;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class TaskPanel : UIPanelBase
    {
        [SerializeField] private TaskUnitView _taskUnitPrefab;
        [SerializeField] private Transform _taskUnitRoot;
        [SerializeField] private GameObject obj;
        [SerializeField] private Button _exitButton;

        public  void Start()
        {
            _exitButton.onClick.AddListener(ClosePanel);
            ClosePanel();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _exitButton.onClick.RemoveAllListeners();
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

        public async void CreateTaskUnit(TaskModel model, Task_RuntimeData runtime, bool isAlreadyFinished)
        {
            var view = UnityEngine.Object.Instantiate(_taskUnitPrefab, _taskUnitRoot);
            view.descriptionText.text = model.desc;
            view.finishedText.text = "可领取";
            view.gameObject.SetActive(true);

            runtime.OnProgressChange += view.UpdateProgress;
            runtime.OnTaskFinished += view.SetAsFinished;
            runtime.OnTaskDestroy += () => UnityEngine.Object.Destroy(view.gameObject);
            view.ConfirmButton.onClick.AddListener(() =>
                EventCenter.Publish(new ClickConfirmButtonEvent(runtime)));

            await view.SetRewardIcons(model.RewardNames);

            if (isAlreadyFinished)
            {
                view.SetAsFinished();
            }
            else
            {
                view.UpdateProgress(runtime.Condition.Progress);
            }
        }
    }
}
