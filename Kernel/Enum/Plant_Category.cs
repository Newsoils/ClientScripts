using System.Collections;
using System.Collections.Generic;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace ENUM
        {
            [System.Serializable]
            public enum Plant_Category
            {
                  cycle,
                  flower,
                  normal
            }

            /// <summary>
            /// 种植一级分类
            /// </summary>
            public enum Plant_First_Category
            {
                None = 0,
                /// <summary> 花盆 </summary>
                Pot = 1,
                /// <summary> 种子 </summary>
                Seed = 2,
                /// <summary> 肥料 </summary>
                Fertilizer = 3
            }

            /// <summary>
            /// 种植二级分类
            /// </summary>
            public enum Plant_Second_Category
            {
                /// <summary> 无 </summary>
                None = 0,

                // --- 花盆类 ---
                /// <summary> 大花盆 </summary>
                BigPot = 101,
                /// <summary> 中花盆 </summary>
                MediumPot = 102,
                /// <summary> 小花盆 </summary>
                SmallPot = 103,
                /// <summary> 吊顶花盆 </summary>
                HangingPot = 104,

                // --- 种子类 ---
                /// <summary> 普通植物 </summary>
                NormalPlant = 201,
                /// <summary> 爬藤（吊顶）植物 </summary>
                VinePlant = 202,

                // --- 肥料类 ---
                /// <summary> 生长肥料 </summary>
                GrowthFert = 301,
                /// <summary> 有机肥料 </summary>
                OrganicFert = 302
            }

        }
    }
}