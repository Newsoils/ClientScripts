using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using Cmd;
using EnhancedUI.EnhancedScroller;
using Google.Protobuf;
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
        }

        public void ReloadData()
        {
            _dataList.Clear();

            var taskRuntimeDatas = MissionManager.Instance.missionRuntimeDic;

            foreach (var kvp in taskRuntimeDatas)
            {
                if (kvp.Value.status == 2) continue;
                if (MissionManager.Instance.missionStaticDic.TryGetValue(kvp.Value.missionId, out var model))
                    _dataList.Add(new ScrollData_Task(model, kvp.Value));
            }

            // 加载未解锁的任务（存在于静态表但不在运行时表中）
            foreach (var staticKvp in MissionManager.Instance.missionStaticDic)
            {
                bool hasRuntime = false;
                foreach (var runtimeKvp in taskRuntimeDatas)
                {
                    if (runtimeKvp.Value.missionId == staticKvp.Key)
                    {
                        hasRuntime = true;
                        break;
                    }
                }
                if (!hasRuntime)
                    _dataList.Add(new ScrollData_Task(staticKvp.Value, true));
            }

            _dataList.Sort((a, b) =>
            {
                int GetCategory(ScrollData_Task item)
                {
                    if (item.isLocked)
                        return 2; // 未解锁，最后

                    if (item.runtime.status == 1)
                        return 0; // 可领取，最前

                    if (item.runtime.status == 0)
                        return 1; // 未完成，中间

                    return 3; // 其他状态兜底，放最后
                }

                int aCategory = GetCategory(a);
                int bCategory = GetCategory(b);

                if (aCategory != bCategory)
                    return aCategory.CompareTo(bCategory);

                // 同类内部按解锁等级从低到高排列
                return a.model.unlockExp.CompareTo(b.model.unlockExp);
            });

            if (scroller != null)
                scroller.ReloadData();
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
            cellView.SetRewardIcons();

            return cellView;
        }

        #endregion

        private void OnClaimClicked(ulong taskUId)
        {
            var req = new MissionRewardsReq() { GroupID = 0, MissionType = 0, MissionUID = taskUId, ModuleID = 0 };
            //EvtDsp.TriggerEvt<int>(EvtNames.Task_ClaimReward, missionId);
            EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);
        }
    }
}
