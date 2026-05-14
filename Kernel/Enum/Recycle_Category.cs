using System.Collections;
using System.Collections.Generic;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace ENUM
        {
            /// <summary>
            /// 回收箱一级分类
            /// </summary>
            public enum Recycle_First_Category
            {
                None = 0,
                /// <summary> 作物收获物 </summary>
                Crop = 1,
            }

            /// <summary>
            /// 回收箱二级分类
            /// </summary>
            public enum Recycle_Second_Category
            {
                /// <summary> 无 </summary>
                None = 0,

                // --- 作物收获物类 ---
                /// <summary> 普通植物收获物 </summary>
                NormalPlant = 101,
                /// <summary> 爬藤植物收获物 </summary>
                VinePlant = 102,
            }
        }
    }
}
