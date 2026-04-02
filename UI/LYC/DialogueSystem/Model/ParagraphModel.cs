using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.LYC.DialogueSystem
{
    [System.Serializable]
    public class ParagraphModel
    {
        // 段落 Id
        [JsonProperty("_paraId")]
        [SerializeField] private int _paraId;

        // 段落首句对话 Id
        [JsonProperty("_firstDialogueId")]
        [SerializeField] private int _firstDialogueId;

        // 段落中所有对话的 Id
        [JsonProperty("_allDialogueIdList")]
        [SerializeField] private List<int> _allDialogueIdList;

        // 所有对话 [DialogueId, IDialogueModel]
        private Dictionary<int, IDialogueModel> _allDialoguesInPara;

        public int ParaId => _paraId;
        public int FirstDialogueId => _firstDialogueId;

        [JsonConstructor]
        public ParagraphModel()
        {

        }

        /// <summary>
        /// 初始化段落中所有对话
        /// </summary>
        public void InitAllDialoguesInPara(Dictionary<int, IDialogueModel> dialogueDicInController)
        {
            _allDialoguesInPara = new Dictionary<int, IDialogueModel>();
            foreach (int id in _allDialogueIdList)
            {
                _allDialoguesInPara[id] = dialogueDicInController[id];
            }
        }

        /// <summary>
        /// 获取对应 Id 的对话
        /// </summary>
        public IDialogueModel GetDialogueById(int id)
        {
            if (!_allDialoguesInPara.ContainsKey(id))
            {
                Debug.LogError($"id为 {id} 的对话不存在");
                return null;
            }

            return _allDialoguesInPara[id];
        }
    }
}

