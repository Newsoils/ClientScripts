using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class ClickConfirmButtonEvent : EventCenter.IEvent
    {
        public Task_RuntimeData taskInstance;

        public ClickConfirmButtonEvent(Task_RuntimeData taskInstance)
        {
            this.taskInstance = taskInstance;
        }
    }
}

