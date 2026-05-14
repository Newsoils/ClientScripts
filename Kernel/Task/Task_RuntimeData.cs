using System;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 任务实例运行时数据，管理单个任务的状态和达成条件。
    /// </summary>
    public class Task_RuntimeData
    {
        public bool IsAccept { get; set; }
        public bool CanGetReward { get; set; }
        public bool IsFinish { get;  set; }
        public int TaskId { get; }
        /// <summary>
        /// 所属循环轮次（Cycle）。非循环任务固定为 0，
        /// 循环任务每轮激活时从 TaskManager 获得当前 Cycle 值。
        /// </summary>
        public int Cycle { get; private set; }

        public event Action<float> OnProgressChange;
        public event Action OnTaskFinished;
        public event Action OnTaskDestroy;

        public string ConditionName { get; private set; }
        public int CurrentCount => _count;
        public int TargetCount => _targetCount;
        public float Progress => _targetCount == 0 ? 1f : (float)_count / _targetCount;

        private int _count;
        private int _targetCount;

        public Task_RuntimeData(int taskId, string conditionName, int targetCount)
        {
            TaskId = taskId;
            ConditionName = conditionName;
            _count = 0;
            _targetCount = targetCount;
        }

        public void AcceptTask()
        {
            IsAccept = true;
            OnProgressChange?.Invoke(0f);
        }

        public void RejectTask()
        {
            IsAccept = false;
            OnProgressChange?.Invoke(0f);
        }

        public void AcceptRecurTask(int cycle)
        {
            IsAccept = true;
            IsFinish = false;
            CanGetReward = false;
            _count = 0;
            Cycle = cycle;
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

            _count += count;
            OnProgressChange?.Invoke(Progress);

            if (_count >= _targetCount)
            {
                CanGetReward = true;
                //Debug.Log("达到完成任务的条件");
            }
        }

        public void DestroyTaskDatas()
        {
            OnTaskDestroy?.Invoke();
            OnProgressChange = null;
            OnTaskFinished = null;
            OnTaskDestroy = null;
        }

        public void RestoreState(bool isAccept, bool canGetReward, bool isFinish,
            int currentCount, int targetCount, int cycle)
        {
            IsAccept = isAccept;
            CanGetReward = canGetReward;
            IsFinish = isFinish;
            _count = currentCount;
            _targetCount = targetCount;
            Cycle = cycle;
            OnProgressChange?.Invoke(Progress);
        }
    }
}
