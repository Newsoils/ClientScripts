namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public interface ICondition
    {
        string ConditionName { get; }
        bool IsMet { get; }
        float Progress { get; }
        Enum_ConditionType ConditionType { get; }
        int CurrentCount { get; }
        int TargetCount { get; }

        void DoOperations(int count);
        void RestoreCondition(string conditionName, int currentCount, int targetCount);
    }
}
