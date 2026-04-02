using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class EX_RewardHandler : IRewardHandler
    {
        private EX_RewardData _data;

        public EX_RewardHandler(EX_RewardData data)
        {
            this._data = data;
        }

        public void GrantReward(PlayerRewardData data)
        {
            if(_data.rewardType == Enum_RewardType.Exp)
            {
                data.ex_value += _data.amount;
            }
        }
    }
}
