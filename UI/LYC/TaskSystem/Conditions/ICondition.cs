using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    // 确保子类对象都有一个对应的事件需要去监听（一对一关系）
    public interface ICondition
    {
        // 是否满足要求
        public string ConditionName { get; }

        // 是否满足要求
        public bool IsMet { get; }

        // 当前任务进度 0 ~ 1
        public float Progress { get; }

        public Enum_ConditionType ConditionType { get; }


        // 任务完成判断（原本逻辑为主动监听，现在改为被外部调用，然后做内部判断）
        public void DoOperations(int operationTimes);

        //public void Reset();
    }

    public interface ICondition<T> : ICondition where T : EventCenter.IEvent
    {
        // 事件监听器
        //public void OnEventListener(T publishEvent);
    }
}
