using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using Newtonsoft.Json;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            public class InteractInfo
            {
                public string interactName;
                public string animationBoolName;
                public List<string> roomLimit;
                public List<Placement_Second_Category> placementLimit = new List<Placement_Second_Category>();
                public float minInteractTime;
                public float maxInteractTime;
                public string itemRequire;
                public string stateName;
                /// <summary>非空时在主角进入/退出交互状态时分派特殊逻辑（如临时套装，不写入服装存档字典）。</summary>
                public string specialBehavior;
            }
        }
    }
}
