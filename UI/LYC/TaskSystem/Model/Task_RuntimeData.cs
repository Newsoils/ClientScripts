using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    /// <summary>
    /// 任务实例运行时数据，管理单个任务的状态、奖励和达成条件。
    /// </summary>
    public class Task_RuntimeData
    {
        public bool IsAccept { get; private set; }
        public bool CanGetReward { get; private set; }
        public bool IsFinish { get; private set; }
        public ICondition Condition { get; }
        public List<RewardData> Rewards { get; }

        public event Action<float> OnProgressChange;
        public event Action OnTaskFinished;
        public event Action OnTaskDestroy;

        public Task_RuntimeData(List<RewardData> rewards, ICondition condition)
        {
            Rewards = rewards;
            _condition = condition;
        }

        private readonly ICondition _condition;

        public void AcceptTask()
        {
            IsAccept = true;
            OnProgressChange?.Invoke(0f);
        }

        public void FinishTask()
        {
            OnTaskFinished?.Invoke();
            IsFinish = true;
        }

        public void AddOperationTimes(int count)
        {
            if (!IsAccept || IsFinish)
                return;

            _condition.DoOperations(count);
            OnProgressChange?.Invoke(_condition.Progress);

            if (_condition.IsMet)
            {
                CanGetReward = true;
                Debug.Log("达到完成任务的条件");
            }
        }

        public bool TryGetRewards(out PlayerRewardData reward)
        {
            if (!CanGetReward)
            {
                reward = null;
                return false;
            }

            reward = new PlayerRewardData();
            foreach (var r in Rewards)
                r.CreateRewardHandler().GrantReward(reward);

            CanGetReward = false;
            return true;
        }

        public void DestroyTaskDatas()
        {
            OnTaskDestroy?.Invoke();
            OnProgressChange = null;
            OnTaskFinished = null;
            OnTaskDestroy = null;
        }

        public void RestoreState(bool isAccept, bool canGetReward, bool isFinish,
            string conditionName, int currentCount, int targetCount)
        {
            IsAccept = isAccept;
            CanGetReward = canGetReward;
            IsFinish = isFinish;
            _condition.RestoreCondition(conditionName, currentCount, targetCount);
            OnProgressChange?.Invoke(_condition.Progress);
        }
    }
}
