using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class TaskPanel : UIPanelBase
    {
        [SerializeField] private ScrollerController_Task _taskScroller;
        [SerializeField] private GameObject obj;
        [SerializeField] private Button _exitButton;

        public void Start()
        {
            _exitButton.onClick.AddListener(ClosePanel);
            ClosePanel();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EvtDsp.AddEvt(EvtNames.Task_Unlocked, OnTaskUnlocked);
        }

        private void OnDisable()
        {
            EvtDsp.RemoveEvt(EvtNames.Task_Unlocked, OnTaskUnlocked);
        }

        private void OnTaskUnlocked()
        {
            if (!obj.activeSelf)
                return;
            _taskScroller.ReloadData();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _exitButton.onClick.RemoveAllListeners();
            EvtDsp.RemoveEvt(EvtNames.Task_Unlocked, OnTaskUnlocked);
        }

        public override void OpenPanel(params object[] data)
        {
            obj.SetActive(true);
            _taskScroller.ReloadData();
        }

        public override void ClosePanel()
        {
            obj.SetActive(false);
        }

        public override void UpdatePanel(params object[] data)
        {
        }
    }
}
