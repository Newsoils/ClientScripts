using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// NPCChat 系统内事件携带的数据结构。
    /// 只有需要多个参数的事件才用包装类传 payload；单一参数直接用裸类型。
    /// </summary>

    /// <summary>
    /// 打开选项面板。Handlers 已在 Controller 层闭包捕获业务逻辑，Panel 直接调用即可。
    /// </summary>
    public class EvtData_OpenOption
    {
        public readonly DialogueModel Dialogue;
        public readonly List<UnityAction> ButtonHandlers;

        public EvtData_OpenOption(DialogueModel dialogue, List<UnityAction> buttonHandlers)
        {
            Dialogue = dialogue;
            ButtonHandlers = buttonHandlers;
        }
    }
}
