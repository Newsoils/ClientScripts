using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 奖励分发器：将任务奖励分发到玩家的道具仓库，并触发 UI 展示。
    /// </summary>
    public static class RewardDistributor
    {
        /// <param name="rewardItems">道具奖励列表：(itemId, amount)</param>
        /// <param name="rewardData">读表原始奖励数据，用于提取经验值</param>
        /// <param name="rewardNames">用于 UI 展示的奖励名称列表</param>
        public static void Distribute(
            List<(int itemId, int amount)> rewardItems,
            List<RewardData> rewardData,
            List<string> rewardNames)
        {
            int expValue = 0;
            foreach (var r in rewardData)
            {
                if (r.rewardType == Enum_RewardType.Exp)
                    expValue += r.amount;
            }

            List<(string, int)> itemsToAdd = new List<(string, int)>();

            if (expValue > 0)
                itemsToAdd.Add(("亲密度", expValue));

            foreach (var (itemId, amount) in rewardItems)
            {
                var name = Global_Inventory_Manager.GetItemInfo(itemId).name;
                itemsToAdd.Add((name, amount));
            }
            // TODO zhaorui
            // Global_Inventory_Manager.Change_Items_Count(itemsToAdd, "任务奖励");

            EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, itemsToAdd);
        }
    }
}
