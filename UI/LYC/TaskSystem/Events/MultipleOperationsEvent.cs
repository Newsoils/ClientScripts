using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    // 多次触发的某个事件
    public class MultipleOperationsEvent : EventCenter.IEvent
    {
        public string operationName;

        public int count;

        public MultipleOperationsEvent(string operationName, int count)
        {
            this.operationName = operationName;
            this.count = count;
        }
    }
}
