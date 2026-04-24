using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using CLIP.Framework_Core.Event;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    /// <summary>
    /// 数据层：任务实例类，主要包含：单个任务状态，奖励和达成条件等信息的定义
    /// 主要实现但个任务的逻辑相关，包括：
    /// 1.管理任务状态 Taskstatus
    /// 2.依据奖励数据 RewardData 发放奖励 
    /// 3.判断任务完成条件
    /// </summary>
    public class Task_RuntimeData   // 这里先假设每个任务都只监听 MultipleOperationsEvent 事件
    {
        // ========== 任务状态 ==========

        // 接取状态
        private bool _isAccept;
        public bool IsAccept => _isAccept;

        // 可领取奖励状态
        private bool _canGetReward;
        public bool CanGetReward => _canGetReward;

        // 完成状态（任务在可领取状态时并不代表已经完成，要等玩家点击领取按钮获得奖励后，这个任务才算彻底完成）
        private bool _isFinish;
        public bool IsFinish => _isFinish;

        // ========== 事件 ==========

        // 任务的进度改变
        public event Action<float> OnProgressChange;

        // 任务完成
        public event Action OnTaskFinished;

        // 任务被销毁
        public event Action OnTaskDestroy;

        // ========== 其他 ==========

        // 数据：奖励 List
        public List<RewardData> Rewards;

        // 逻辑：任务完成条件
        private ICondition _condition;
        public ICondition Condition => _condition;


        public Task_RuntimeData(List<RewardData> rewardDatas, ICondition condition) 
        {
            this._isAccept = false;
            this._canGetReward = false;
            this._isFinish = false;
            this.Rewards = rewardDatas;
            this._condition = condition;
        }



        // ========== 任务状态设置 ==========

        // 接取
        public void AcceptTask()
        {
            // 设置状态：任务已接取
            _isAccept = true;
            // 初始化状态
            OnProgressChange?.Invoke(0f);
        }

        // 取消接取
        public void UnacceptTask()
        {
            // 设置状态：任务未接取
            _isAccept = false;
        }

        // 完成任务，设置任务显示，状态等
        public void FinishTask()
        {
            OnTaskFinished?.Invoke();

            // 设置状态：任务已完成
            _isFinish = true;
        }


        // ========== 事件 handlers ==========

        // 事件 handler：用于处理对应操作后 UI 显示的数据的变化
        private void OnReceiveOperation((string, int) evtPair)
        {
            // 判断是否是我要监听的事件
            if (evtPair.Item1 != Condition.ConditionName) return;

            // 进行操作前，先对任务是否接取做个检测
            if (!IsAccept) return;

            // 再对任务状态做个判断
            if (IsFinish) return;

            // 1.更新数据
            Condition.DoOperations(evtPair.Item2);

            // 2.更新 UI 显示
            OnProgressChange.Invoke(Condition.Progress);
            //taskUnitView.UpdateView(Condition.Progress);

            // 3.检测并打印当前状态信息
            if (!Condition.IsMet)
            {
                // 如果没达成条件
                Debug.Log($"检测到一次操作，当前进度是 {Condition.Progress}");
                return;
            }

            // 达成任务奖励领取条件
            _canGetReward = true;
            Debug.Log("达到完成任务的条件");

        }


        // ========== 其他 ==========

        // 增加操作次数
        public void AddOperationTimes(int operationTimes)
        {
            // 进行操作前，先对任务是否接取做个检测
            if (!IsAccept) return;

            // 再对任务状态做个判断
            if (IsFinish) return;

            // 1.更新数据
            Condition.DoOperations(operationTimes);

            // 2.更新 UI 显示
            OnProgressChange.Invoke(Condition.Progress);
            //taskUnitView.UpdateView(Condition.Progress);

            // 3.检测并打印当前状态信息
            if (!Condition.IsMet)
            {
                // 如果没达成条件
                Debug.Log($"检测到一次操作，当前进度是 {Condition.Progress}");
                return;
            }

            // 达成任务奖励领取条件
            _canGetReward = true;
            Debug.Log("达到完成任务的条件");
        }

        /// <summary>
        /// 返回获取是否成功的结果，传出奖励参数
        /// 注意在使用时判断当前任务奖励是否已被领取过
        /// </summary>
        /// <param name="reward"></param>
        /// <returns></returns>
        public bool TryGetRewards(out PlayerRewardData reward)
        {
            if (CanGetReward)
            {
                reward = new PlayerRewardData();
                foreach (RewardData r in Rewards)
                {
                    // 创建句柄，授予奖励
                    r.CreateRewardHandler().GrantReward(reward);
                    _canGetReward = false;
                }
                if (reward == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            reward = null;
            return false;
        }

        // 销毁任务数据
        public void DestroyTaskDatas()
        {
            OnTaskDestroy?.Invoke();

            OnProgressChange = null;
            OnTaskFinished = null;
            OnTaskDestroy = null;
        }


    }
}
