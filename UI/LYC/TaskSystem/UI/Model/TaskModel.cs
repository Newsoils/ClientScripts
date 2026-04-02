using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    /// <summary>
    /// 静态数据：任务模板 Model，包含任务描述，
    /// </summary>
    [System.Serializable]
    public class TaskModel
    {
        // 任务 id
        [JsonProperty("taskId")]
        public int taskId;

        // 任务描述
        [JsonProperty("desc")]
        public string desc;

        // 任务进度表示方面，先统一做成这样的形式：( 0 / 1 ) 或者是 ( 3 / 5 )

        // 任务需要完成的次数
        [JsonProperty("targetCount")]
        public int targetCount;

        // 任务完成后应该得到的奖励
        [JsonProperty("Rewards")]
        public List<RewardData> Rewards;

        // 奖励图标 name
        [JsonProperty("RewardNames")]
        public List<string> RewardNames;

        [JsonConstructor]
        public TaskModel()
        {

        }
    }
}
