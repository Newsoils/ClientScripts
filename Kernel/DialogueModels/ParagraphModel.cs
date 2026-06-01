using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 对话段落容器。对话数据本身由 <see cref="NPCChatManager"/> 统一加载后注入。
    /// </summary>
    [System.Serializable]
    public class ParagraphModel
    {
        [JsonProperty("_paraId")]
        [SerializeField] private int _paraId;

        /// <summary>
        /// 仅用于 JSON 反序列化初始化。初始化后不再使用，运行时查找全由 <see cref="_dialogueMap"/> 承担。
        /// </summary>
        [JsonProperty("_allDialogueIdList")]
        [SerializeField, HideInInspector] private List<int> _allDialogueIdList;

        private Dictionary<int, DialogueModel> _dialogueMap;

        public int ParaId => _paraId;
        public int FirstDialogueId => _allDialogueIdList.First();

        public void InjectDialogues(Dictionary<int, DialogueModel> globalDialogueDic)
        {
            _dialogueMap = new Dictionary<int, DialogueModel>(_allDialogueIdList.Count);
            foreach (int id in _allDialogueIdList)
            {
                if (!globalDialogueDic.TryGetValue(id, out var model))
                {
                    Debug.LogError($"[ParagraphModel] 段落 {ParaId} 引用的对话 id={id} 在全局字典中未找到。");
                    continue;
                }
                _dialogueMap[id] = model;
            }
        }

        /// <summary>
        /// 根据 id 获取对话模型。
        /// </summary>
        public DialogueModel GetDialogueById(int id)
        {
            if (_dialogueMap != null && _dialogueMap.TryGetValue(id, out var model))
                return model;

            Debug.LogError($"[ParagraphModel] 段落 {ParaId} 中不存在 id={id} 的对话。");
            return null;
        }

        public bool TryGetDialogueById(int id, out DialogueModel model)
        {
            if (_dialogueMap != null && _dialogueMap.TryGetValue(id, out model))
                return true;

            model = null;
            return false;
        }
    }
}
