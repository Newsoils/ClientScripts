using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.DialogueSystem
{
    [System.Serializable]
    public class NormalDialogueModel : BaseDialogueModel
    {
        [JsonProperty("_dialogueId")]
        [SerializeField] private int _dialogueId;

        [JsonProperty("_speakerId")]
        [SerializeField] private int _speakerId;

        [JsonProperty("_speaker")]
        [SerializeField] private EnumDialogueSpeaker _speaker;

        [JsonProperty("_contents")]
        [SerializeField] private string _contents;

        [JsonProperty("_chatHints")]
        [SerializeField] private string _chatHints;

        [JsonProperty("_nextDialogueId")]
        [SerializeField] private int _nextDialogueId;

        [JsonProperty("_hasOptionDialogue")]
        [SerializeField] private bool _hasOptionDialogue;

        [JsonProperty("_isLastDialogue")]
        [SerializeField] private bool _isLastDialogue;
        // 是否是选项对话
        // [SerializeField] private bool _isOptionDialogue;

        public override int DialogueId => _dialogueId;
        public override int SpeakerId => _speakerId;
        public override EnumDialogueSpeaker Speaker => _speaker;
        public override string Contents => _contents;
        public override string ChatHints => _chatHints;
        public override int NextDialogueId => _nextDialogueId;
        public override bool HasOptionDialogue => _hasOptionDialogue;
        public override bool IsLastDialogue => _isLastDialogue;
        // public bool IsOptionDialogue => _isOptionDialogue;

        [JsonConstructor]
        public NormalDialogueModel(int _dialogueId, EnumDialogueSpeaker _speaker, string _contents, int _nextDialogueId, bool _hasOptionDialogue = false, bool _isLastDialogue = false)
        {
            this._dialogueId = _dialogueId;
            this._speaker = _speaker;
            this._contents = _contents;
            this._nextDialogueId = _nextDialogueId;

            this._hasOptionDialogue = false;
            this._isLastDialogue = false;
        }

        public void SetContents(string newContents)
        {
            _contents = newContents;
        }

    }
}
