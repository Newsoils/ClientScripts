using CLIP.Project_Mouse.ENUM;
using System;
using System.Collections.Generic;

namespace CLIP
{
    namespace Project_Mouse
    {
        [Serializable]
        public class ItemGroupInfo
        {
            /// <summary>
            /// 组ID
            /// </summary>
            public int group_id;

            /// <summary>
            /// 组名称
            /// </summary>
            public string group_name;

            /// <summary>
            /// 描述
            /// </summary>
            public string desc;

            /// <summary>
            /// 组类型
            /// </summary>
            public GroupType groupType;

            /// <summary>
            /// 风格
            /// </summary>
            public string style;

            /// <summary>
            /// 资源路径/Key
            /// </summary>
            public string res_url;

            /// <summary>
            /// 组内物品列表
            /// </summary>
            public List<int> group_item_list;
        }

        public class ItemGroupData
        {
            public int group_id;

            public ItemGroupInfo itemGroupInfo;
            /// <summary>
            /// 套组中已获得的物品数量
            /// </summary>
            public int obtainCount;

            /// <summary>
            /// 套组中总物品数量
            /// </summary>
            public int totalCount;

            /// <summary>
            /// 已获得的物品ID列表
            /// </summary>
            public List<int> obtained = new List<int>();
        }
    }
}
