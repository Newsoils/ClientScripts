using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
//using UnityEngine;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            namespace Social
            {
                [System.Serializable]
                public class Social_Chat_Msg_Record  
                {
                    public int _msg_id;
                    public string _msg_sender="";
                    [JsonProperty(DefaultValueHandling=DefaultValueHandling.Ignore,NullValueHandling =NullValueHandling.Ignore)]
                    public Social_Chat_Msg _msg_content=null;
                    public string _msg_good_item_name = "";
                    public string _msg_res_url = "";
                    [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, NullValueHandling = NullValueHandling.Ignore)]
                    public DateTime _msg_date;
                }
            }
        }
    }
}
