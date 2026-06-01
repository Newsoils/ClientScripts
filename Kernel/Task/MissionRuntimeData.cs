using System;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 任务实例运行时数据，管理单个任务的状态和达成条件。
    /// </summary>
    public class MissionRuntimeData
    {
        /// <summary>
        /// 是否接受了该任务
        /// </summary>
        public bool isAccept;

        /// <summary>
        /// 任务状态 0：进行中 1：已完成 2：已领取奖励
        /// </summary>
        public int status;

        /// <summary>
        /// 任务配置表ID
        /// </summary>
        public int missionId;

        /// <summary>
        /// 任务唯一UID（服务器派发）
        /// </summary>
        public ulong missionUId;

        /// <summary>
        /// 当前完成进度数量
        /// </summary>
        public int current;

        /// <summary>
        /// 目标需要完成的总数量
        /// </summary>
        public int target;

        /// <summary>
        /// 任务进度 0~1
        /// </summary>
        public float Progress => target == 0 ? 1f : (float)current / target;

        /// <summary>
        /// 是否可以领取奖励
        /// </summary>
        public bool CanGetReward => status == 1;

        public event Action<float> OnProgressChange;
        public event Action OnTaskFinished;
        public event Action OnTaskDestroy;

        public void AcceptTask()
        {
            isAccept = true;
        }

        //public void FinishTask()
        //{
        //    OnTaskFinished?.Invoke();
        //    IsFinish = true;
        //}

        //public void AddOperationTimes(int count)
        //{
        //    if (!isAccept || IsFinish)
        //        return;

        //    current += count;
        //    OnProgressChange?.Invoke(Progress);

        //    if (current >= target)
        //    {
        //        CanGetReward = true;
        //        //Debug.Log("达到完成任务的条件");
        //    }
        //}



        //public void RestoreState(bool isAccept, bool canGetReward, bool isFinish,
        //    int currentCount, int targetCount)
        //{
        //    this.isAccept = isAccept;
        //    CanGetReward = canGetReward;
        //    IsFinish = isFinish;
        //    current = currentCount;
        //    target = targetCount;
        //    OnProgressChange?.Invoke(Progress);
        //}
    }
}
