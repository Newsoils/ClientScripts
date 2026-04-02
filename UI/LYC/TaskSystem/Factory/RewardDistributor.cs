using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    // 奖励分发器
    public static class RewardDistributor
    {
        // 分发玩家奖励
        public static void DistributePlayerRewards(PlayerRewardData rewardData)
        {
            List<(string, int)> data = new List<(string, int)>();

            // 分发经验值 Exp（别称：亲密度）
            data.Add(("亲密度", rewardData.ex_value));

            // 分发小鱼币
            int amount = rewardData.ItemRewards.Find(x => x.itemId == 1).amount;
            data.Add(("鱼币", rewardData.ex_value));

            // 分发罐罐
            amount = rewardData.ItemRewards.Find(x => x.itemId == 2).amount;
            data.Add(("罐罐", rewardData.ex_value));

            if (Global_Inventory_Manager._instance != null)
            {
                // 分发仓库道具（包括：小鱼币，罐罐）
                foreach (var item in rewardData.ItemRewards)
                {
                    var name = Global_Inventory_Manager.GetItemInfo(item.itemId).name;
                    data.Add((name, item.amount));
                }

                Global_Inventory_Manager.Change_Items_Count(data, "任务奖励");
            }
            else
            {
                Debug.LogWarning("Global_Inventory_Manager._instance为空，没有办法添加道具");
            }

        }



    }
}
