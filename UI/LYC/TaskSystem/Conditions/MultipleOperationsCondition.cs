namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class MultipleOperationsCondition : ICondition
    {
        public string ConditionName { get; private set; }
        public int CurrentCount => _count;
        public int TargetCount => _targetCount;
        public bool IsMet => _isMet;
        public float Progress => _targetCount == 0 ? 1f : (float)_count / _targetCount;
        public Enum_ConditionType ConditionType { get; }

        private int _count;
        private int _targetCount;
        private bool _isMet;

        public MultipleOperationsCondition(Enum_ConditionType conditionType, string conditionName, int targetCount)
        {
            ConditionType = conditionType;
            ConditionName = conditionName;
            _count = 0;
            _targetCount = targetCount;
            _isMet = false;
        }

        public void DoOperations(int count)
        {
            _count += count;
            if (_count >= _targetCount)
                _isMet = true;
        }

        public void RestoreCondition(string conditionName, int currentCount, int targetCount)
        {
            _count = currentCount;
            _targetCount = targetCount;
            ConditionName = conditionName;
            _isMet = _count >= _targetCount;
        }
    }
}
