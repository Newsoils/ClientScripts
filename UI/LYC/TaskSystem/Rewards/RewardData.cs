using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    [System.Serializable]
    public class RewardData
    {
        // 奖励类型
        [JsonProperty("rewardType")]
        public Enum_RewardType rewardType;

        // 目标（实际奖励，比如某个道具）id
        [JsonProperty("targetId")]
        public int targetId;

        // 奖励数量
        [JsonProperty("amount")]
        public int amount;

        [JsonConstructor]
        public RewardData()
        {
            rewardType = Enum_RewardType.Exp;
        }

        // 供外部获取 用于处理奖励发放的句柄
        public IRewardHandler CreateRewardHandler()
        {
            return new RewardHandler(this);
        }
    }
}
