using CLIP.Project_Mouse.ENUM;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CLIP.Project_Mouse.Kernel.Dispatch
{
    [System.Serializable]
    public class Map_Info
    {
        /// <summary>
        /// id
        /// </summary>
        public int map_id;
        /// <summary>
        /// 地图名
        /// </summary>
        public string map_name;

        /// <summary>
        /// 对应的地图场景名
        /// </summary>
        public string map_Scene_Name;
        /// <summary>
        /// 地图等级
        /// </summary>
        public int map_level;
        /// <summary>
        /// tag
        /// </summary>
        public List<Mood_Tag> mood_tag_list;
        /// <summary>
        /// 权重
        /// </summary>
        public float sampling_weight;
        /// <summary>
        /// 奖励钱
        /// </summary>
        public int reward_currency;
        /// <summary>
        /// 奖励经验
        /// </summary>
        public int reward_exp;
        /// <summary>
        /// 默认照片luck
        /// </summary>
        public float default_photo_luck;

        /// <summary>
        /// 获得某个稀有度需要的幸运值
        /// item1 : 稀有度id  item2: 幸运值要求
        /// </summary>
        public List<(int,float)> luck_list_for_photo;

        /// <summary>
        /// 这张地图可获取照片id列表
        /// </summary>
        public List<int> photo_Ids;

        /// <summary>
        /// 伙伴遇见概率
        /// </summary>
        public float friend_meeting_probability;
        /// <summary>
        /// 可遇见伙伴列表
        /// </summary>
        public List<string> friend_list;

        /// <summary>
        /// 额外奖励出现概率
        /// </summary>
        public float additional_item_plus;

        /// <summary>
        /// 工牌图鉴描述
        /// </summary>
        public string card_allery_desc;
        /// <summary>
        /// 图鉴资源路径
        /// </summary>
        public List<string> card_gallery_res_url;

        public override string ToString()
        {
            return "{ "
            + "map_id:" + map_id + ","
            + "map_name:" + map_name + ","
            + "map_level:" + map_level + ","
            + "sampling_weight:" + sampling_weight + ","
            + "reward_currency:" + reward_currency + ","
            + "reward_exp:" + reward_exp + ","
            + "default_photo_luck:" + default_photo_luck + ","
            + "friend_meeting_probability:" + friend_meeting_probability + ","
            + "friend_list:" + friend_list.ToString() + ","
              + "additional_item_plus:" + additional_item_plus + ","
                 + "card_allery_desc:" + card_allery_desc + ","
        + "card_gallery_res_url:" + JsonConvert.SerializeObject(card_gallery_res_url) + ","
            + "}";
        }

    }
}