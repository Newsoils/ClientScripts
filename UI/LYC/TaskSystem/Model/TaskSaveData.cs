using System.Collections.Generic;
using CLIP.Project_Mouse.LYC.TaskSystem;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.LYC.TaskSystem
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

        [JsonProperty("conditionType")]
        public Enum_ConditionType conditionType;

        [JsonProperty("conditionName")]
        public string conditionName;

        [JsonProperty("currentCount")]
        public int currentCount;

        [JsonProperty("targetCount")]
        public int targetCount;

        public static TaskRuntimeSaveData From(TaskModel model, Task_RuntimeData runtime)
        {
            return new TaskRuntimeSaveData
            {
                taskId = model.taskId,
                isAccept = runtime.IsAccept,
                canGetReward = runtime.CanGetReward,
                isFinish = runtime.IsFinish,
                conditionType = runtime.Condition.ConditionType,
                conditionName = runtime.Condition.ConditionName,
                currentCount = runtime.Condition.CurrentCount,
                targetCount = runtime.Condition.TargetCount
            };
        }

        public void ApplyTo(Task_RuntimeData runtime)
        {
            runtime.RestoreState(isAccept, canGetReward, isFinish, conditionName, currentCount, targetCount);
        }
    }

    [System.Serializable]
    public class TaskSaveData
    {
        [JsonProperty("tasks")]
        public List<TaskRuntimeSaveData> tasks = new();
    }
}

namespace CLIP.Framework_Core.Serialization
{
    public static class TaskSaveDataExtensions
    {
        private const string SaveFileName = "TaskData.json";

        public static TaskSaveData Load()
        {
            return Save_Load_Tools.Load<TaskSaveData>(SaveFileName);
        }

        public static void Save(TaskSaveData data)
        {
            Save_Load_Tools.Save(SaveFileName, data);
        }
    }
}
