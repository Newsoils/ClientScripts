using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.NPCDialogueSystem
{
    /// <summary>
    /// 好感度对话触发项
    /// </summary>
    [System.Serializable]
    public class FavorDialogueTrigger
    {
        // 触发所需要的好感度等级
        [JsonProperty("favorLevel")]
        public int FavorLevel { get; set; }

        // 触发的对话 id
        [JsonProperty("paraId")]
        public int ParagraphId { get; set; }

        // 是否触发过
        [JsonProperty("isTriggered")]
        public bool IsTriggered { get; set; }

        // 当前对话是否解锁（对话是否能够被触发）
        [JsonProperty("isUnlock")]
        public bool IsUnlock { get; set; }

        public FavorDialogueTrigger() { }

        public FavorDialogueTrigger(int favorLevel, int paraId, bool isTriggered = false)
        {
            FavorLevel = favorLevel;
            ParagraphId = paraId;
            IsTriggered = isTriggered;
            IsUnlock = false;
        }
    }


    /// <summary>
    /// 单个 NPC 的用于存储好感度数据的本地文件
    /// </summary>
    [SerializeField]
    public class LocalNPCFavorData
    {
        // NPC Id
        [JsonProperty("npcId")]
        public int NPCId { get; private set; }

        // 当前好感度等级（与服务器数据同步）
        [JsonProperty("currentFavorLevel")]
        public int CurrentFavorLevel { get; private set; }

        // 当前好感度经验值（与服务器数据同步）
        [JsonProperty("currentFavorValue")]
        public int CurrentFavorValue { get; private set; }

        // 该 NPC 最大 达到过 的好感度
        [JsonProperty("maxFavorLevelReached")]
        public int MaxFavorLevelReached { get; private set; }

        // 记录要触发对话的 List
        // 这个数据需要在一开始的加载中就注入数据 --》 从玩家本地的 LocalFavorDatas Json 数据中加载进来
        // 但是这里需要进行初始化
        // 该数据中的元素会在游戏进行过程中被修改，但是 DialoguesToTrigger 本身所持有的元素数量不会改变
        [JsonProperty("dialoguesToTrigger")]
        public List<FavorDialogueTrigger> DialoguesToTrigger { get; private set; } = new();

        // 选择过的选项记录 Dictionary<OptionDialogueModel.dialogueId, optionIndex>
        [JsonProperty("OptionChoicesDic")]
        [JsonConverter(typeof(IntStringDictConverter))]
        public Dictionary<int, string> OptionChoicesDic;

        // 上一次更新时间
        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; private set; }

        // 辅助属性：是否有待触发的对话
        [JsonIgnore]
        public bool HasPendingDialogues
        {
            get
            {
                foreach (var trigger in DialoguesToTrigger)
                {
                    // 如果满足：已解锁 且未触发，则表示有对话要触发
                    //if (trigger.FavorLevel <= CurrentFavorLevel && !trigger.IsTriggered && trigger.IsUnlock)
                    if (!trigger.IsTriggered && trigger.IsUnlock)
                        return true;
                }
                return false;
            }
        }

        // 辅助属性：下一个待触发的对话
        //[JsonIgnore]
        //public FavorDialogueTrigger GetNextPendingDialogue
        //{
        //    get
        //    {
        //        foreach (var trigger in DialoguesToTrigger)
        //        {
        //            if (trigger.FavorLevel <= CurrentFavorLevel && !trigger.IsTriggered && trigger.IsUnlock)
        //                return trigger;
        //        }
        //        return null;
        //    }
        //}

        /// <summary>
        /// 返回下一个待触发对话的 paraId，同时 out 出来所对应的好感度等级
        /// </summary>
        /// <param name="newFavorLevel"></param>
        /// <param name="nextDiaAtFavorLevel"></param>
        /// <returns></returns>
        public FavorDialogueTrigger GetNextPendingDialogue(int newFavorLevel, out int nextDiaAtFavorLevel)
        {
            foreach (var trigger in DialoguesToTrigger)
            {
                //if (newFavorLevel <= CurrentFavorLevel && !trigger.IsTriggered && trigger.IsUnlock)
                if (!trigger.IsTriggered && trigger.IsUnlock)
                {
                    nextDiaAtFavorLevel = trigger.FavorLevel;
                    return trigger;
                }
            }
            nextDiaAtFavorLevel = -1;
            return null;
        }


        [JsonConstructor]
        public LocalNPCFavorData() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LocalNPCFavorData(int npcId, int initialLevel = 1, int initialValue = 0)
        {
            NPCId = npcId;
            CurrentFavorLevel = initialLevel;
            CurrentFavorValue = initialValue;
            MaxFavorLevelReached = initialLevel;
            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// 初始化对话触发列表（可以多次调用，不会重复添加）
        /// </summary>
        public void InitDialoguesToTrigger(Dictionary<int, int> paraIdOfEachFavorLevel)
        {
            if (paraIdOfEachFavorLevel == null) return;

            foreach (var kvp in paraIdOfEachFavorLevel)
            {
                int favorLevel = kvp.Key;
                int paraId = kvp.Value;

                // 检查是否已存在
                var existing = DialoguesToTrigger.Find(t => t.FavorLevel == favorLevel);
                if (existing == null)
                {
                    // 新增
                    DialoguesToTrigger.Add(new FavorDialogueTrigger(favorLevel, paraId, false));
                }
                else if (existing.ParagraphId != paraId)
                {
                    // 更新段落ID，但保持触发状态
                    existing.ParagraphId = paraId;
                }
            }

            // 按好感度等级排序
            DialoguesToTrigger.Sort((a, b) => a.FavorLevel.CompareTo(b.FavorLevel));
        }

        public void PrintDialogueToTriggerData()
        {
            Debug.Log($"当前判断是否要触发对话的 NPC id 是 {NPCId}");
            foreach (var d in DialoguesToTrigger)
            {
                Debug.Log($"好感度等级：{d.FavorLevel}, 对话 id：{d.ParagraphId}， 解锁状态：{d.IsUnlock}， 触发状态：{d.IsTriggered})");
            }
        }

        // 更新对话解锁状态
        public void UpdateUnlockStatus(int newFavorLevel)
        {
            foreach(FavorDialogueTrigger trigger in DialoguesToTrigger)
            {
                // 如果剧情对话所需好感度等级 <= 当前最新等级，且该剧情对话还没有解锁
                if (trigger.FavorLevel <= newFavorLevel && !trigger.IsUnlock)
                {
                    // 就解锁该对话
                    trigger.IsUnlock = true;
                    trigger.IsTriggered = false;
                }
            }
        }

        // 检查是否需要更新本地数据（依据服务器的逻辑）
        public void CheckIfNeedUpdateLocalStatus(int newFavorLevel)
        {
            //trigger.
        }

        // 重置本地对话解锁状态
        public void ResetLocalStatus()
        {
            foreach (FavorDialogueTrigger trigger in DialoguesToTrigger)
            {
                trigger.IsUnlock = true;
                trigger.IsTriggered = false;
            }
        }

        /// <summary>
        /// 获取当前 NPC 的 好感度 --》 对应段落 id Pair 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> GetPaddingParaIds()
        {
            Dictionary<int, int> temp = new Dictionary<int, int>();
            foreach (var trigger in DialoguesToTrigger)
            {
                if (trigger.IsUnlock && !trigger.IsTriggered)
                {
                    temp.Add(trigger.FavorLevel, trigger.ParagraphId);
                }
            }

            return temp;
        }

        /// <summary>
        /// 获取当前 NPC 之前触发过的 ParagraghId
        /// </summary>
        /// <returns></returns>
        public List<int> GetParaIdListTriggered(int newFavorLevel)
        {
            List<int> temp = new List<int>();
            foreach(var trigger in DialoguesToTrigger)
            {
                // 如果好感度等级满足要求，且该对话之前触发过了
                if(trigger.FavorLevel <= newFavorLevel && trigger.IsTriggered)
                {
                    temp.Add(trigger.ParagraphId);
                }
            }            
            // 按好感度等级排序
            DialoguesToTrigger.Sort((a, b) => a.FavorLevel.CompareTo(b.FavorLevel));

            return temp;
        }

        /// <summary>
        /// 该好感度等级对应的对话是否解锁
        /// </summary>
        /// <param name="favorLevel"></param>
        /// <returns></returns>
        public bool IsUnlock(int favorLevel)
        {
            if (!ContainDialogueAt(favorLevel))
            {
                Debug.LogWarning($"不存在好感的等级为 {favorLevel} 的对话");
                return false;
            }

            foreach(var trigger in DialoguesToTrigger)
            {
                if(trigger.FavorLevel == favorLevel)
                {
                    return trigger.IsUnlock;
                }
            }
            return false;
        }

        /// <summary>
        /// 该好感度等级是否存在对话待触发
        /// </summary>
        /// <param name="favorLevel"></param>
        /// <returns></returns>
        public bool ContainDialogueAt(int favorLevel)
        {
            foreach (var trigger in DialoguesToTrigger)
            {
                if (trigger.FavorLevel == favorLevel)
                {
                    return true;
                }
            }
            return false;
        }

        public void UnlockDialogueAt(int favorLevel)
        {
            foreach (var trigger in DialoguesToTrigger)
            {
                if (trigger.FavorLevel == favorLevel)
                {
                    trigger.IsUnlock = true;
                }
            }

        }

        /// <summary>
        /// 已经触发完了对话
        /// </summary>
        public void TriggeredDialogueAt(int favorLevel)
        {
            foreach (var trigger in DialoguesToTrigger)
            {
                if (trigger.FavorLevel == favorLevel)
                {
                    trigger.IsTriggered = true;
                }
            }
        }

        public void SetCurrentFavorLevel(int favorLevel)
        {
            this.CurrentFavorLevel = favorLevel;
        }

        public void SetCurrentFavorValue(int favorValue)
        {
            this.CurrentFavorValue = favorValue;
        }

        // 添加选择记录
        public void AddChoiceRecord(int dialogueId, string optionText)
        {
            if (OptionChoicesDic == null)
            {
                OptionChoicesDic = new Dictionary<int, string>();
            }
            // 如果已经存在就修改更新
            if (OptionChoicesDic.ContainsKey(dialogueId))
            {
                OptionChoicesDic[dialogueId] = optionText;
            }
            else
            {
                OptionChoicesDic.Add(dialogueId, optionText);
            }
        }

        // 事件：好感度等级提升
        //public event Action<int, int> OnFavorLevelIncreased;

        // 事件：对话被触发，并且播放到了结束
        //public event Action<int, int> OnDialogueTriggeredAndPlayCompletely;
    }
}
