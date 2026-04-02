using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class MultipleOperationsCondition : ICondition
    {
        private string _conditionName;
        public string ConditionName => _conditionName;

        private int _count;

        private int _targetCount;


        private bool _isMet;
        public bool IsMet => _isMet;

        public float Progress { get { return (float)_count / (float)_targetCount; } }


        private Enum_ConditionType _conditionType;
        public Enum_ConditionType ConditionType => _conditionType;


        public MultipleOperationsCondition(Enum_ConditionType conditionType, string conditionName, int targetCount)
        {
            this._conditionType = conditionType;
            this._conditionName = conditionName;
            this._count = 0;
            this._targetCount = targetCount;
            this._isMet = false;
        }

        public void DoOperations(int operationTimes)
        {
            _count += operationTimes;

            if(_count >= _targetCount)
            {
                _isMet = true;
            }
        }
    }
}
