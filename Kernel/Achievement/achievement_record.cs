using System;
using System.Collections;
using System.Collections.Generic;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            public class achievement_record  
            {
                public int achievement_record_id=-1;
                public string user_name;
                public string achievement_name;
                public string achievement_type;
                public string current_state;
                public DateTime date_obtained;
                //public int achievement_points = 0;
                public achievement_record() { }
                
            }
        }
    }
}