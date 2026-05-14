using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    [System.Serializable]
    public class TaskRuntimeSaveData
    {
        [JsonProperty("taskId")]
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

        [JsonProperty("cycle")]
        public int cycle;

        public static TaskRuntimeSaveData From(TaskModel model, Task_RuntimeData runtime)
        {
            return new TaskRuntimeSaveData
            {
                taskId = model.taskId,
                isAccept = runtime.IsAccept,
                canGetReward = runtime.CanGetReward,
                isFinish = runtime.IsFinish,
                currentCount = runtime.CurrentCount,
                targetCount = runtime.TargetCount,
                cycle = runtime.Cycle
            };
        }

        public void ApplyTo(Task_RuntimeData runtime)
        {
            runtime.RestoreState(isAccept, canGetReward, isFinish, currentCount, targetCount, cycle);
        }
    }

    [System.Serializable]
    public class TaskSaveData
    {
        [JsonProperty("tasks")]
        public List<TaskRuntimeSaveData> tasks = new();
    }
}

