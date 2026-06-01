using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 对话数据模型，统一普通对话和选项对话。
    /// - isOptionDialogue == false 时为普通对话，使用 contents 显示文本
    /// - isOptionDialogue == true 时为选项对话，使用 options 选择列表
    /// </summary>
    [System.Serializable]
    public class DialogueModel
    {
        public int dialogueId;

        public int paragraphId;

        public int speakerId;

        public EnumDialogueSpeaker speaker;

        /// <summary>
        /// 普通对话文本。选项对话时为 null。
        /// </summary>
        public string contents;

        public string chatHints;

        public int nextDialogueId;

        /// <summary>
        /// 是否为选项对话。
        /// </summary>
        public bool isOptionDialogue;

        public bool isLastDialogue;

        /// <summary>
        /// 选项列表：选项文本 → 下一句对话 id。仅 isOptionDialogue == true 时有效。
        /// </summary>
        public List<OptionInfo> options;

        public Dictionary<int, OptionInfo> optionsLookup;

        public void BuildLookupDict()
        {
            optionsLookup = new Dictionary<int, OptionInfo>(options?.Count ?? 0);
            if (options == null) return;
            foreach (var opt in options)
                optionsLookup[opt.optionIndex] = opt;
        }

    }


    public class OptionInfo
    {
        [SerializeField] public int optionIndex;
        [SerializeField] public string content;
        [SerializeField] public int nextDialogueId;
    }
}
