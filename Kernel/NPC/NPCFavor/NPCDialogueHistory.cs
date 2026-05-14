using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 单个 NPC 的对话进度与选项记录，在本地持久化。
    /// </summary>
    [System.Serializable]
    public class OptionChoiceRecord
    {
        [JsonProperty("dialogueId")]
        public int DialogueId;

        [JsonProperty("selectedOption")]
        public string SelectedOption;

        public OptionChoiceRecord(int dialogueId, string selectedOption)
        {
            DialogueId = dialogueId;
            SelectedOption = selectedOption;
        }
    }

    /// <summary>
    /// 单个 NPC 的对话进度与选项记录，在本地持久化。
    /// </summary>
    [System.Serializable]
    public class NPCDialogueHistory
    {
        [JsonProperty("npcId")]
        public int NPCId { get; private set; }

        /// <summary>
        /// 各好感度等级的对话进度记录。
        /// </summary>
        [JsonProperty("dialogueProgressList")]
        public List<DialogueProgress> DialogueProgressList { get; private set; }

        /// <summary>
        /// 已触发过的选项对话记录（List 格式，方便服务器解析）。
        /// </summary>
        [JsonProperty("optionChoices")]
        public List<OptionChoiceRecord> OptionChoices { get; private set; }

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; private set; }

        [JsonConstructor]
        public NPCDialogueHistory() { }

        public NPCDialogueHistory(int npcId)
        {
            NPCId = npcId;
            LastUpdated = DateTime.Now;
            DialogueProgressList = new List<DialogueProgress>();
            OptionChoices = new List<OptionChoiceRecord>();
        }

        /// <summary>
        /// 从好感度等级 → 段落 id 映射配置初始化进度列表。若已有条目则跳过。
        /// </summary>
        public void InitDialogueProgress(Dictionary<int, int> favorLevelToParaIdDic)
        {
            if (favorLevelToParaIdDic == null) return;

            foreach (var kvp in favorLevelToParaIdDic)
            {
                int favorLevel = kvp.Key;
                if (DialogueProgressList.Find(p => p.FavorLevel == favorLevel) == null)
                    DialogueProgressList.Add(new DialogueProgress(favorLevel, kvp.Value));
            }
            DialogueProgressList.Sort((a, b) => a.FavorLevel.CompareTo(b.FavorLevel));
        }

        /// <summary>
        /// 根据当前好感度等级解锁满足条件的进度条目。
        /// </summary>
        public void UpdateUnlockStatus(int newFavorLevel)
        {
            foreach (var progress in DialogueProgressList)
            {
                if (progress.FavorLevel <= newFavorLevel && !progress.IsUnlocked)
                    progress.IsUnlocked = true;
            }
        }

        /// <summary>
        /// 获取下一个"未完成"的已解锁进度条目（FavorLevel 最低的）。
        /// 返回值：null 表示没有未完成的段落。
        /// </summary>
        public DialogueProgress GetNextPendingProgress()
        {
            foreach (var progress in DialogueProgressList)
            {
                if (!progress.IsUnlocked || progress.IsFinished)
                    continue;
                return progress;
            }
            return null;
        }

        /// <summary>
        /// 返回历史上所有"已完成"的段落 id 列表（在 atFavorLevel 及以下）。
        /// </summary>
        public List<int> GetFinishedParagraphIds(int atFavorLevel)
        {
            var result = new List<int>();
            foreach (var progress in DialogueProgressList)
            {
                if (progress.FavorLevel <= atFavorLevel && progress.IsFinished)
                    result.Add(progress.FavorLevel);
            }
            result.Sort();
            return result;
        }

        /// <summary>
        /// 标记指定好感度等级的段落为"已完成"状态。
        /// </summary>
        public void MarkParagraphFinished(int favorLevel)
        {
            var progress = DialogueProgressList.Find(p => p.FavorLevel == favorLevel);
            if (progress != null && !progress.IsFinished)
                progress.MarkFinished();
        }

        /// <summary>
        /// 记录当前播放到的对话 id。
        /// </summary>
        public void RecordProgress(int favorLevel, int dialogueId)
        {
            var progress = DialogueProgressList.Find(p => p.FavorLevel == favorLevel);
            if (progress != null)
                progress.LastDialogueId = dialogueId;
        }

        /// <summary>
        /// 记录玩家在某个选项对话中的选择。
        /// </summary>
        public void AddChoiceRecord(int dialogueId, string optionText)
        {
            OptionChoices ??= new List<OptionChoiceRecord>();
            var existing = OptionChoices.Find(r => r.DialogueId == dialogueId);
            if (existing != null)
                existing.SelectedOption = optionText;
            else
                OptionChoices.Add(new OptionChoiceRecord(dialogueId, optionText));
        }

        /// <summary>
        /// 查询历史选项记录。
        /// </summary>
        public bool TryGetChoiceRecord(int dialogueId, out string choice)
        {
            if (OptionChoices != null)
            {
                var record = OptionChoices.Find(r => r.DialogueId == dialogueId);
                if (record != null)
                {
                    choice = record.SelectedOption;
                    return true;
                }
            }
            choice = null;
            return false;
        }

        public void ResetAllProgress()
        {
            foreach (var progress in DialogueProgressList)
            {
                progress.IsUnlocked = true;
                progress.MarkIncomplete();
            }
        }
    }
}
