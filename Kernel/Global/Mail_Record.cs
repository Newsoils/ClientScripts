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
            public class item_in_mail
            {
                public string item_name;
                public int item_quantity;
            }
            [System.Serializable]
            public class Mail_Record
            {
                public int mail_id;
                public string mail_title;
                public string mail_text;
                public List<item_in_mail> item_list=new List<item_in_mail>();
                public DateTime mail_date;
                public string sender;
                public string receiver;
                // unread, read, deleted
                public string mail_state;
                public int _send_present_msg_id_related=-1;
                public bool isGetReward;
            }
        }
    }
}