using System;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            public class Announcement_Record
            {
                public int anno_id;
                public DateTime anno_date;
                public string anno_title;
                public string anno_content;
                public string anno_bg_url;
                //public string anno_state; // active, inactive
            }
        }
    }
}