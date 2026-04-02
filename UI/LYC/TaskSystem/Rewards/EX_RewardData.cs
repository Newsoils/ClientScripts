using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    [System.Serializable]
    public class EX_RewardData : RewardData
    {

        [JsonConstructor]
        public EX_RewardData()
        {
            rewardType = Enum_RewardType.Exp;
        }

        public EX_RewardData(int amount)
        {
            rewardType = Enum_RewardType.Exp;
            targetId = 1;
            this.amount = amount;
        }

        public new IRewardHandler CreateRewardHandler()
        {
            return new EX_RewardHandler(this);
        }
    }
}
