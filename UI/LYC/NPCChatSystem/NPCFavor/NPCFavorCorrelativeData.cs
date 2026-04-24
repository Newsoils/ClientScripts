using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.NPCDialogueSystem
{
    // 与 NPC 好感度相关的数据（包括：要触发的对话，会收到的礼物......）
    [System.Serializable]
    public class NPCFavorCorrelativeData
    {
        // NPC Id
        [JsonProperty("npcId")]
        [SerializeField] public int npcId;

        // 各个好感度等级对应的 礼物Id ----- Dictionary<favorLevel, giftId>
        //[JsonProperty("giftOfEachFavorLevel")]
        //[JsonConverter(typeof(IntIntDictConverter))]
        //[SerializeField] public Dictionary<int, int> giftOfEachFavorLevel;

        // 各个好感度等级对应的 段落Id ----- Dictionary<favorLevel, paraId>
        [JsonProperty("paraIdOfEachFavorLevel")]
        [JsonConverter(typeof(IntIntDictConverter))]
        [SerializeField] public Dictionary<int, int> paraIdOfEachFavorLevel;

        [JsonConstructor]
        public NPCFavorCorrelativeData()
        {

        }
    }
}
