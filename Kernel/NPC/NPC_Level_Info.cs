using System.Collections;
using System.Collections.Generic;
#if ON_UNITY
using UnityEngine;
#endif

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            public class NPC_Level_Info
            {
                public NPC_Info _npc_Info;
                public bool is_main_character;
                public float[] position=new float[3];
                public string charcter_state;
                public string AA_asset_path;

                public bool is_valid_character()
                {
                    if (_npc_Info._npc_Base.npc_id == null) return false;
                    return true;
                }
            }
        }
    }
}
