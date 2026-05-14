using Newtonsoft.Json;
using System.Collections.Generic;
namespace CLIP.Project_Mouse.Kernel.Dispatch
{
    [System.Serializable]
    public class Map_Reward_Pool
    {
        /// <summary>
        /// 地图名
        /// </summary>
        public string map_name;
        /// <summary>
        /// 获得物品数量权重
        /// </summary>
        public List<int> reward_count_weight_list;
        /// <summary>
        /// 可获得奖励物品名
        /// </summary>
        public List<string> reward_item_name_list;

        public override string ToString()
        {
            return "{ "
            + "map_name:" + map_name + ","
            + "reward_count_weight_list:" + reward_count_weight_list.ToString() + ","
            + "reward_item_name_list:" + reward_item_name_list.ToString() + ","
            + "}";
        }
    }
}