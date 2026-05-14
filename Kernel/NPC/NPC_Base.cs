using System;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            /// <summary>
            /// NPC相关静态数据，从策划填表获取
            /// </summary>
            public class NPC_Base
            {
                /// <summary>
                /// ID
                /// </summary>
                public int npc_id;
                /// <summary>
                /// NPC姓名
                /// </summary>
                public string npc_name;
                /// <summary>
                /// 性格
                /// </summary>
                public string npc_personality;

                public string icon_resource_name;
            }
        }
    }
}