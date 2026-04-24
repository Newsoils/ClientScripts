using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.LYC.DialogueSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.DialogueSystem
{
    [System.Serializable]
    public class OptionDialogueModel : BaseDialogueModel
    {
        [JsonProperty("_dialogueId")]
        [SerializeField] private int _dialogueId;

        [JsonIgnore]
        [SerializeField] private int _speakerId;

        [JsonIgnore]
        [SerializeField] private EnumDialogueSpeaker _speaker = EnumDialogueSpeaker.Character;

        [JsonProperty("_chatHints")]
        [SerializeField] private string _chatHints;

        [JsonIgnore]
        [SerializeField] private int _nextDialogueId;

        [JsonIgnore]
        [SerializeField] private bool _hasOptionDialogue = true;

        [JsonProperty("_isLastDialogue")]
        [SerializeField] private bool _isLastDialogue;

        // 选项对话 [ OptionId, CharacterDialogueModel ]
        [JsonProperty("Options")]
        [JsonConverter(typeof(StringIntDictConverter))]
        //[SerializeField] public Dictionary<int, string> Options = null;
        [SerializeField] public Dictionary<string, int> Options = null;
        // [SerializeField] public List<string> Options = null;


        public override int DialogueId => _dialogueId;
        public override int SpeakerId => _speakerId;
        public override EnumDialogueSpeaker Speaker => _speaker;
        public override string Contents
        {
            get { Debug.LogWarning("该对话为选项对话，不存在文字内容"); return null; }
        }
        public override string ChatHints => _chatHints;
        public override int NextDialogueId => _nextDialogueId;
        public override bool HasOptionDialogue => _hasOptionDialogue;       // 为 true 时，代表这个对话是 OptionDialogueModel，需要进行特殊处理
        public override bool IsLastDialogue => _isLastDialogue;

        // public OptionDialogueModel() { }


        [JsonConstructor]
        public OptionDialogueModel()
        {

        }

        public OptionDialogueModel(int dialogueId, int nextDialogueId, params (string, int)[] optionTexts)
        {
            _dialogueId = dialogueId;
            _nextDialogueId = nextDialogueId;
            Options = new Dictionary<string, int>();
            if (optionTexts != null)
            {
                for (int i = 0; i < optionTexts.Length; ++i)
                {
                    Options.Add(optionTexts[i].Item1, optionTexts[i].Item2);
                }
            }

            _hasOptionDialogue = true;
            _speaker = EnumDialogueSpeaker.Character;
        }

        public List<string> GetOptionList()
        {
            List<string> returnList = new List<string>();
            foreach(var dia in Options)
            {
                returnList.Add(dia.Key);
            }

            return returnList;
        }

        /// <summary>
        /// 设置下一个对话的 id，用在选择对应选项之后
        /// </summary>
        public void SetNextDialogueId(int nextDialogueId)
        {
            _nextDialogueId = nextDialogueId;
        }
    }
}
