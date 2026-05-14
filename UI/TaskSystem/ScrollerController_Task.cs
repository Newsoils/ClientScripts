using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace CLIP.Project_Mouse.UI
{
    public class ScrollerController_Task : MonoBehaviour, IEnhancedScrollerDelegate
    {
        public EnhancedScroller scroller;
        public CellView_Task cellViewPrefab;

        private List<ScrollData_Task> _dataList = new List<ScrollData_Task>();

        private void Start()
        {
            if (scroller != null)
                scroller.Delegate = this;
            EvtDsp.AddEvt(EvtNames.Task_ClaimableChanged, OnClaimableChanged);
        }

        private void OnDestroy()
        {
            EvtDsp.RemoveEvt(EvtNames.Task_ClaimableChanged, OnClaimableChanged);
        }

        private void OnClaimableChanged()
        {
            if (!gameObject.activeInHierarchy)
                return;
            ReloadData();
        }

        public void ReloadData()
        {
            int? firstVisibleTaskId = null;
            if (scroller != null && _dataList.Count > 0)
            {
                int firstDataIndex = scroller.StartDataIndex;
                if (firstDataIndex >= 0 && firstDataIndex < _dataList.Count)
                    firstVisibleTaskId = _dataList[firstDataIndex].model.taskId;
            }

            _dataList.Clear();

            var taskRuntimeDatas = TaskManager.Instance._taskRuntimeDatas;

            foreach (var kvp in taskRuntimeDatas)
            {
                if (kvp.Value.IsFinish)
                    continue;

                if (TaskManager.Instance.taskModelsDic.TryGetValue(kvp.Key, out var model))
                    _dataList.Add(new ScrollData_Task(model, kvp.Value));
            }

            if (scroller != null)
            {
                scroller.ReloadData();

                if (firstVisibleTaskId.HasValue)
                {
                    int newIndex = _dataList.FindIndex(d => d.model.taskId == firstVisibleTaskId.Value);
                    if (newIndex >= 0)
                        scroller.JumpToDataIndex(newIndex, 0, 0, true, EnhancedScroller.TweenType.immediate, 0);
                }
            }
        }

        public void ClearData() => _dataList.Clear();

        #region IEnhancedScrollerDelegate

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _dataList.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 230f;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(cellViewPrefab) as CellView_Task;

            var data = _dataList[dataIndex];
            cellView.SetData(data, OnClaimClicked);
            _ = cellView.SetRewardIcons(data.model.RewardNames);

            return cellView;
        }

        #endregion

        private void OnClaimClicked(int taskId)
        {
            EvtDsp.TriggerEvt<int>(EvtNames.Task_ClaimReward, taskId);
        }
    }
}
