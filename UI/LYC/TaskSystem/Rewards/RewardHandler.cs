using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public class RewardHandler : IRewardHandler
    {
        private RewardData _data;

        public RewardHandler(RewardData data)
        {
            this._data = data;
        }

        // 这里先暂时用工厂模式对奖励进行分配
        public void GrantReward(PlayerRewardData rewardData)
        {
            if(_data.rewardType == Enum_RewardType.Exp)
            {
                switch (_data.targetId)
                {
                    case 0:
                        rewardData.ex_value += _data.amount;
                        break;
                    default:
                        break;
                }
            }
            else if(_data.rewardType == Enum_RewardType.Item)
            {
                rewardData.ItemRewards.Add(new ItemReward(_data.targetId, _data.amount));
            }
        }
    }
}
