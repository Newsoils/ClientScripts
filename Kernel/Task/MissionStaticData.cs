using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// Mission任务静态数据
    /// </summary>
    [System.Serializable]
    public class MissionStaticData
    {
        //解锁等级
        public int unlockExp;

        // 任务 id
        public int taskId;

        // 任务描述
        public string desc;

        public int taskType;

        public int taskParameter;

        // 任务需要完成的次数
        public int targetCount;

        // 任务完成后应该得到的奖励
        public List<RewardData> Rewards;

        // 奖励图标 name
        public List<string> RewardNames;

    
    }
}
