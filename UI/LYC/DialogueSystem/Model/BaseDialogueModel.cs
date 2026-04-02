using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.LYC.DialogueSystem;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.DialogueSystem
{
    [System.Serializable]
    public abstract class BaseDialogueModel : IDialogueModel
    {
        public abstract int DialogueId { get; }
        public abstract int SpeakerId { get; }
        public abstract EnumDialogueSpeaker Speaker { get; }
        public abstract string Contents { get; }
        public abstract string ChatHints { get; }
        public abstract int NextDialogueId { get; }
        public abstract bool HasOptionDialogue { get; }
        public abstract bool IsLastDialogue { get; }
    }
}
