using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
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
