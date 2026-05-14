using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            /// <summary>
            /// npc的基础信息和运行时数据
            /// </summary>
            public class NPC_Info
            {
                public NPC_Base _npc_Base;
                public NPC_RuntimeData _npc_RuntimeData;

                /// <summary>
                /// 升到下级好感度需要的经验值,根据实时等级和静态的表里面得来
                /// </summary>
                public int npc_Next_Favor_Level;
            }

            public class NPC_Gift_Favor
            {
                public int npc_id;
                /// <summary>
                /// 礼物（就是物品）
                /// </summary>
                public List<(Item_Type, int)> gift_favor;
            }

            public class NPC_Meeting_Favor
            {
                /// <summary>
                /// 做一个lvel id，不同遇见次数不同的好感度，实际可能用不到
                /// </summary>
                public int lv_id;
                /// <summary>
                /// 遇见次数的区间，例如(1,3)表示遇见1-3次
                /// </summary>
                public (int,int) meet_Time_Range;
                /// <summary>
                /// 遇见的时候的好感度增量
                /// </summary>
                public int favor_Plus;
            }

            /// <summary>
            /// NPC好感度升级可能会给玩家送礼物，存储每个NPC每个等级会送什么礼物
            /// </summary>
            public class NPC_Gift_To_Player
            {
                public int npc_id;
                /// <summary>
                /// 好感对应的等级和礼物ID，对应于GameItem表中的item_id
                /// </summary>
                public List<(int level, int gift_ID)> eachFavorLevel_Gift;
            }

        }
    }
}