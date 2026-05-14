using System;
using System.Collections.Generic;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            /// <summary>
            /// --- NPC动态变化属性 ---
            /// </summary>
            [System.Serializable]
            public class NPC_RuntimeData
            {
                public int npc_id;

                /// <summary>
                /// 当前好感度等级
                /// </summary>
                public int favor_level;
                /// <summary>
                /// 当前好感度经验
                /// </summary>
                public int favor_Value;

                /// <summary>
                /// 遇到次数
                /// </summary>
                public int encounter_count;
                /// <summary>
                /// 是否遇到过
                /// </summary>
                public bool is_met;
                /// <summary>
                /// 是否“结识”（遇到三次之后结识）
                /// </summary>
                public bool is_acquainted;
           
                /// <summary>
                /// 上次互动时间
                /// </summary>
                public DateTime last_interaction_time;

                public NPC_RuntimeData()
                {
                    favor_level = 0;
                    favor_Value = 0;
                    encounter_count = 0;
                    is_met = false;
                    is_acquainted = false;
                    last_interaction_time = DateTime.MinValue;
                }
                public NPC_RuntimeData(int npc_id, int favorLevel, int favorValue, int next_level_favor_value, int encounter_count, bool is_met, bool is_acquainted, DateTime last_interaction_time)
                {
                    this.npc_id = npc_id;
                    favor_level = favorLevel;
                    favor_Value = favorValue;
                    this.encounter_count = encounter_count;
                    this.is_met = is_met;
                    this.is_acquainted = is_acquainted;
                    this.last_interaction_time = last_interaction_time;
                }
            }

            [System.Serializable]
            public class Player_DailyRuntimeData
            {
                public DateTime lastGiftResetTime;    // 上次送礼次数重置时间
                public int giftsGiven_Daily_Count;           // 今天已送礼次数（最大2）
                public List<int> giftedNPCsToday;  // 今天已送过礼的伙伴ID
            }
        }
    }
}