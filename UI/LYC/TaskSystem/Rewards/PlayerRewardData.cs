using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    // 这个类用来存储角色（玩家）可以从任务系统中获取的相关数据，用于作为数据从 任务系统传输到外部的实际数据中（比如服务器上的角色相关数据）的过渡
    public class PlayerRewardData
    {
        // ==================== 基础奖励 ====================
        // 经验值
        public int ex_value;
        // 小鱼币
        //public int fishCoin;
        // 罐罐
        //public int guanguan;

        // ==================== 特殊奖励 ====================
        // 道具 List<(ItemReward, count)>
        public List<ItemReward> ItemRewards = new List<ItemReward>();
    }

    // 道具类型奖励
    public struct ItemReward
    {
        // 道具 id
        public int itemId;
        // 获取数量
        public int amount;

        public ItemReward(int itemId, int amount)
        {
            this.itemId = itemId;
            this.amount = amount;
        }
    }
}
