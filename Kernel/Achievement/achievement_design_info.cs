using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            namespace Achievement
            {
                [System.Serializable]
                public class achievement_design_info
                {
                    /// <summary>
                    /// 成就Id
                    /// </summary>
                    public int achievement_id;
                    /// <summary>
                    /// 成就名称
                    /// </summary>
                    public string achievement_name;
                    /// <summary>
                    /// 成就类型
                    /// </summary>
                    public string achievement_type;
                    /// <summary>
                    /// 成就描述
                    /// </summary>
                    public string achievement_desc;
                    /// <summary>
                    /// 成就解锁条件
                    /// </summary>
                    public string achievement_unlock_condition;
                    /// <summary>
                    /// 成就解锁判定SQL
                    /// </summary>
                    public string achievement_unlock_condition_SQL;
                    /// <summary>
                    /// 成就级别
                    /// </summary>
                    public string achievement_rank;
                    /// <summary>
                    /// 物品奖励列表
                    /// </summary>
                    public List<Inventory.shop_item> item_reward = new List<Inventory.shop_item>();
                    /// <summary>
                    /// 好感度奖励对象
                    /// </summary>
                    public string affinity_reaward_target;
                    /// <summary>
                    /// 好感度奖励数值
                    /// </summary>
                    public int affinity_reaward_value;
                    /// <summary>
                    /// 成就图标美术资源路径
                    /// </summary>
                    public string achievement_icon_res_url;

                    public override string ToString()
                    {
                        return "{ "
                            + "achievement_id:" + achievement_id + ","
                            + "achievement_name:" + achievement_name + ","
                            + "achievement_type:" + achievement_type + ","
                            + "achievement_desc:" + achievement_desc + ","
                            + "achievement_unlock_condition:" + achievement_unlock_condition + ","
                            + "achievement_unlock_condition_SQL:" + achievement_unlock_condition_SQL + ","
                            + "achievement_rank:" + achievement_rank + ","
                            + "item_reward:" + JsonConvert.SerializeObject(item_reward) + ","
                            + "affinity_reaward_target:" + affinity_reaward_target + ","
                            + "affinity_reaward_value:" + affinity_reaward_value + ","
                            + "achievement_icon_res_url:" + achievement_icon_res_url + ","
                            + "}";
                    }

                 
                }
            }
        }
    }
}