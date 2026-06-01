using CLIP.Project_Mouse.ENUM;
using System;
using System.Collections.Generic;

namespace CLIP
{
    namespace Project_Mouse
    {
        [Serializable]
        public class Game_Item_Info
        {
            /// <summary>
            /// 名称
            /// </summary>
            public string name;

            /// <summary>
            /// id
            /// </summary>
            public int item_id;

            /// <summary>
            /// 描述
            /// </summary>
            public string desc;

            /// <summary>
            /// 类型（可以不止一个）
            /// </summary>
            public Item_Type type;
            /// <summary>
            /// UI图标资源路径
            /// </summary>
            public string res_url;

            /// <summary>
            /// 售价(-1 代表不可出售)
            /// </summary>
            public int sell_price;

            /// <summary>
            /// 稀有度（越大越稀有）
            /// </summary>
            public Enum_RarityType rarity;

            /// <summary>
            /// 售价单位物品
            /// </summary>
            public string currency_unit;

            /// <summary>
            /// 图鉴描述
            /// </summary>
            public string gallery_desc;
            /// <summary>
            /// 图鉴资源路径
            /// </summary>
            public List<string> gallery_res_url;

            /// <summary>
            /// 是否可以作为礼物
            /// </summary>
            public bool can_be_present;


            public bool IsPot
            {
                get
                {
                    return type == Item_Type.Pot;
                }
            }

        }
    }
}
