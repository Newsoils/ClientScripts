using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.LYC.DialogueSystem;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.DialogueSystem
{
    public interface IDialogueModel
    {
        // 对话 id
        public int DialogueId { get; }
        // 说话人 id
        public int SpeakerId { get; }
        // 说话者
        public EnumDialogueSpeaker Speaker { get; }
        // 对话内容
        public string Contents { get; }
        // 对话提示（比如：好感度 + 1）
        public string ChatHints { get; }
        // 下一句对话 id
        public int NextDialogueId { get; }
        // 有没有选项对话（如果有，则代表是 OptionDialogueModel，进行特殊处理）
        public bool HasOptionDialogue { get; }
        // 是否是最后一句对话
        public bool IsLastDialogue { get; }
    }
}
