using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 任务奖励的静态配置数据，由 MissionStaticData 持有，用于读表和 JSON 反序列化。
    /// </summary>
    [System.Serializable]
    public class RewardData
    {
        [JsonProperty("rewardType")]
        public Enum_RewardType rewardType;

        [JsonProperty("targetId")]
        public int targetId;

        [JsonProperty("amount")]
        public int amount;

        [JsonConstructor]
        public RewardData()
        {
            rewardType = Enum_RewardType.Exp;
        }
    }
}
