using System;
using CLIP.Framework_Core.Event;

namespace CLIP.Framework_Core.LYC.TaskSystem
{
    public static class TaskTriggers
    {
        // ===== 通用方法 =====

        // 触发 MultipleOperations 事件
        public static void TriggerEventOfMultipleOperations(int taskId, int triggerTimes)
        {
            EvtDsp.TriggerEvt<int, int>("MultipleOperations", taskId, triggerTimes);
        }

        // 由得到的 Func 返回值，决定是否触发 MultipleOperations 事件
        public static void TriggerEventOfMulOprByJudgeJunc(int taskId, int triggerTimes, Func<bool> judge)
        {
            if (judge == null || !judge.Invoke())
            {
                return;
            }

            EvtDsp.TriggerEvt<int, int>("MultipleOperations", taskId, triggerTimes);
        }

        // ===== 自定义方法 =====


    }
}
