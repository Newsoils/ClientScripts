using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 静态数据：任务模板 Model，包含任务描述，
    /// </summary>
    [System.Serializable]
    public class TaskModel
    {
        //解锁等级
        public int unlockExp;

        public bool isRecurTask;

        // 任务 id
        public int taskId;

        // 任务描述
        public string desc;

        // 任务需要完成的次数
        public int targetCount;

        // 任务完成后应该得到的奖励
        public List<RewardData> Rewards;

        // 奖励图标 name
        public List<string> RewardNames;
    }
}
