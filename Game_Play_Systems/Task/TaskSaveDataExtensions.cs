using CLIP.Project_Mouse.Kernel;

namespace CLIP.Project_Mouse.Game_Play_System
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
