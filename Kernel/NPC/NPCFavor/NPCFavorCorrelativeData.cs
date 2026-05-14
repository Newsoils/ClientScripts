using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 单个好感度等级下的对话进度记录。
    /// </summary>
    [System.Serializable]
    public class DialogueProgress
    {
        [JsonProperty("favorLevel")]
        public int FavorLevel { get; set; }

        /// <summary>
        /// 该等级解锁的段落 id（静态配置，由 ToDialogueProgress 注入）。
        /// </summary>
        [JsonProperty("paragraphId")]
        public int ParagraphId { get; set; }

        /// <summary>
        /// 该等级下玩家最后一句播放到的对话 id。-1 表示未开始。
        /// </summary>
        [JsonProperty("lastDialogueId")]
        public int LastDialogueId { get; set; } = -1;

        [JsonProperty("isUnlocked")]
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// 该段落是否已完成。
        /// </summary>
        [JsonProperty("isFinished")]
        public bool IsFinished { get; set; }

        public DialogueProgress() { }

        public DialogueProgress(int favorLevel, int paragraphId)
        {
            FavorLevel = favorLevel;
            ParagraphId = paragraphId;
            LastDialogueId = -1;
            IsUnlocked = false;
            IsFinished = false;
        }

        /// <summary>
        /// 标记该段落为已完成。
        /// </summary>
        public void MarkFinished() => IsFinished = true;

        /// <summary>
        /// 标记该段落为未完成（恢复为中间进度状态）。
        /// </summary>
        public void MarkIncomplete()
        {
            IsFinished = false;
        }
    }

    /// <summary>
    /// 好感度等级 → 段落 id 的映射条目（List 格式，方便服务器解析）。
    /// </summary>
    [System.Serializable]
    public class FavorLevelParagraphPair
    {
        public int FavorLevel;

        public int ParagraphId;

        public FavorLevelParagraphPair() { }

        public FavorLevelParagraphPair(int favorLevel, int paragraphId)
        {
            FavorLevel = favorLevel;
            ParagraphId = paragraphId;
        }
    }

    /// <summary>
    /// 好感度等级 → 段落 id 映射配置，从 Resources JSON 加载。
    /// </summary>
    [System.Serializable]
    public class NPCFavorCorrelativeData
    {
        public int npcId;

        /// <summary>
        /// 好感度等级与段落 id 的对应关系
        /// </summary>
         public List<FavorLevelParagraphPair> FavorLevelParagraphPairs = new List<FavorLevelParagraphPair>();

        /// <summary>
        /// 运行时将 List 转换为 Dictionary 供快速查找。初始化后由 NPCDialogueHistory 使用。
        /// </summary>
        public Dictionary<int, int> ToDictionary()
        {
            var dict = new Dictionary<int, int>();
            if (FavorLevelParagraphPairs != null)
            {
                foreach (var pair in FavorLevelParagraphPairs)
                    dict[pair.FavorLevel] = pair.ParagraphId;
            }
            return dict;
        }
    }
}
