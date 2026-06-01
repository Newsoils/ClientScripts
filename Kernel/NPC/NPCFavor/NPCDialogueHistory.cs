using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 单个 NPC 的对话进度与选项记录。
    /// </summary>
    [System.Serializable]
    public class OptionChoiceRecord
    {
        [JsonProperty("dialogueId")]
        public int DialogueId;

        public int choiceIndex;

        public OptionChoiceRecord(int dialogueId, int index)
        {
            DialogueId = dialogueId;
            choiceIndex = index;
        }
    }

    /// <summary>
    /// 单个段落的进度记录（仅按 paragraphId 索引）。
    /// </summary>
    [System.Serializable]
    public class DialogueProgress
    {
        public int paragraphId;
        public bool isUnlocked;
        public bool isFinished;

        public DialogueProgress() { }

        public DialogueProgress(int paragraphId)
        {
            this.paragraphId = paragraphId;
            isUnlocked = false;
            isFinished = false;
        }

        public DialogueProgress(int paragraphId, bool isUnlocked, bool isFinished)
        {
            this.paragraphId = paragraphId;
            this.isUnlocked = isUnlocked;
            this.isFinished = isFinished;
        }
    }

    /// <summary>
    /// 单个 NPC 的对话进度与选项记录。
    /// </summary>
    [System.Serializable]
    public class NPCDialogueHistory
    {
        public int npcId;

        /// <summary>
        /// 各段落的对话进度记录（静态配置初始化后不再修改结构）。
        /// </summary>
        public List<DialogueProgress> dialogueProgressList = new List<DialogueProgress>();

        /// <summary>
        /// 已触发过的选项对话记录。
        /// </summary>
        public List<OptionChoiceRecord> optionChoices = new List<OptionChoiceRecord>();

        /// <summary>
        /// 该 NPC 最后播放到的对话 id。-1 表示未开始。
        /// </summary>
        public int lastDialogueId = -1;

        /// <summary>
        /// 上次更新时间。
        /// </summary>
        public DateTime lastUpdated;

        /// <summary>
        /// 对话进度数据字典（paragraphId → DialogueProgress），运行时使用，方便快速查询。
        /// </summary>
        public Dictionary<int, DialogueProgress> progressDic = new Dictionary<int, DialogueProgress>();

        public Dictionary<int, OptionChoiceRecord> optionHisDic = new Dictionary<int, OptionChoiceRecord>();

        public NPCDialogueHistory() { }

        public NPCDialogueHistory(int npcId, List<DialogueProgress> dialogueProgressList, List<OptionChoiceRecord> optionChoices, DateTime lastUpdated)
        {
            this.npcId = npcId;
            this.dialogueProgressList = dialogueProgressList;
            this.optionChoices = optionChoices;
            this.lastUpdated = lastUpdated;

            foreach (var progress in dialogueProgressList)
                progressDic[progress.paragraphId] = progress;
            foreach (var option in optionChoices)
                optionHisDic[option.DialogueId] = option;
        }

        /// <summary>
        /// 解锁指定段落。
        /// </summary>
        public bool UnlockParagraph(int paragraphId)
        {
            if (progressDic.TryGetValue(paragraphId, out var progress))
            {
                progress.isUnlocked = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 标记指定段落为已完成。
        /// </summary>
        public bool FinishParagraph(int paragraphId)
        {
            if (progressDic.TryGetValue(paragraphId, out var progress))
            {
                progress.isFinished = true;
                lastDialogueId = -1;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 记录当前播放到的对话 id。
        /// </summary>
        public void RecordProgress(int dialogueId)
        {
            lastDialogueId = dialogueId;
        }

        /// <summary>
        /// 记录玩家在某个选项对话中的选择。
        /// </summary>
        public void AddChoiceRecord(int dialogueId, int choice)
        {
            optionChoices ??= new List<OptionChoiceRecord>();
            var existing = optionChoices.Find(r => r.DialogueId == dialogueId);
            if (existing != null)
                existing.choiceIndex = choice;
            else
                optionChoices.Add(new OptionChoiceRecord(dialogueId, choice));
        }

        /// <summary>
        /// 查询历史选项记录。
        /// </summary>
        public bool TryGetChoiceRecord(int dialogueId, out int choice)
        {
            choice = 0;
            if (optionChoices != null)
            {
                var record = optionChoices.Find(r => r.DialogueId == dialogueId);
                if (record != null)
                {
                    choice = record.choiceIndex;
                    return true;
                }
            }
            return false;
        }
    }
}
