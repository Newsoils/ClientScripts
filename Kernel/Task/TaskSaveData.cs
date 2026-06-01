using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    [System.Serializable]
    public class TaskRuntimeSaveData
    {
        [JsonProperty("missionId")]
        public int taskId;

        [JsonProperty("isAccept")]
        public bool isAccept;

        [JsonProperty("canGetReward")]
        public bool canGetReward;

        [JsonProperty("isFinish")]
        public bool isFinish;

        [JsonProperty("currentCount")]
        public int currentCount;

        [JsonProperty("targetCount")]
        public int targetCount;

       
    }

    [System.Serializable]
    public class TaskSaveData
    {
        [JsonProperty("tasks")]
        public List<TaskRuntimeSaveData> tasks = new();
    }
}

