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
                /// <summary>与 <see cref="Common.MailInfo.MailID"/> 一致（uint64）。</summary>
                public ulong mail_id;
                public string mail_title;
                public string mail_text;
                public List<item_in_mail> item_list=new List<item_in_mail>();
                public DateTime mail_date;
                /// <summary>邮件过期时间（Unix 秒/毫秒，与 <see cref="Common.MailInfo.ValidTime"/> 一致）。</summary>
                public long mail_valid_time;
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