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
                public class Current_Player_Social_Info 
                {

                    public List<string> _friend_accepted = new List<string>();
                    public List<Friend_Social_Record>_friend_accepted_info_record = new List<Friend_Social_Record>();
                    [JsonProperty(DefaultValueHandling= DefaultValueHandling.Ignore,NullValueHandling = NullValueHandling.Ignore)]
                    public List<Social_Chat_Msg_Record> _present_records=new List<Social_Chat_Msg_Record>();
                }
            }
        }
    }
}