using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 对话数据模型，统一普通对话和选项对话。
    /// - IsOptionDialogue == false 时为普通对话，使用 Contents 显示文本
    /// - IsOptionDialogue == true 时为选项对话，使用 Options 选择列表
    /// </summary>
    [System.Serializable]
    public class DialogueModel
    {
        public int _dialogueId;

        public int _paragraphId;

        public int _speakerId;

        public EnumDialogueSpeaker _speaker;

        /// <summary>
        /// 普通对话文本。选项对话时为 null。
        /// </summary>
        public string _contents;

        public string _chatHints;

        public int _nextDialogueId;

        /// <summary>
        /// 是否为选项对话。
        /// </summary>
        public bool _isOptionDialogue;

        public bool _isLastDialogue;

        /// <summary>
        /// 选项列表：选项文本 → 下一句对话 id。仅 IsOptionDialogue == true 时有效。
        /// </summary>
        [JsonConverter(typeof(StringIntDictConverter))]
        public Dictionary<string, int> Options;

        private List<string> _cachedOptionList = null;
        public int DialogueId => _dialogueId;
        public int ParagraphId => _paragraphId;
        public int SpeakerId => _speakerId;
        public EnumDialogueSpeaker Speaker => _speaker;
        public string Contents => _contents;
        public string ChatHints => _chatHints;
        public int NextDialogueId => _nextDialogueId;
        public bool IsOptionDialogue
        {
            get
            {
                return string.IsNullOrEmpty(_contents) && Options.Count > 0;
            }
        }

        public bool IsLastDialogue => _isLastDialogue;

        [JsonConstructor]
        public DialogueModel(
            int _dialogueId,
            int _paragraphId,
            EnumDialogueSpeaker _speaker,
            string _contents,
            int _nextDialogueId,
            bool _isOptionDialogue = false,
            bool _isLastDialogue = false)
        {
            this._dialogueId = _dialogueId;
            this._paragraphId = _paragraphId;
            this._speaker = _speaker;
            this._contents = _contents;
            this._nextDialogueId = _nextDialogueId;
            this._isOptionDialogue = _isOptionDialogue;
            this._isLastDialogue = _isLastDialogue;
        }

        public void SetContents(string newContents) => _contents = newContents;


        /// <summary>
        /// 获取选项文本列表（带缓存）。
        /// </summary>
        public List<string> GetOptionList()
        {
            if (_cachedOptionList == null || Options == null)
                InvalidateOptionListCache();
            return _cachedOptionList;
        }

        /// <summary>
        /// 选中某个选项后，设置下一句对话 id。
        /// </summary>
        public void SetNextDialogueId(int nextDialogueId)
        {
            _nextDialogueId = nextDialogueId;
        }

        private void InvalidateOptionListCache()
        {
            _cachedOptionList = Options != null
                ? new List<string>(Options.Keys)
                : new List<string>();
        }
    }
}
