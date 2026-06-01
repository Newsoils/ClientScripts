using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Game_Play_System;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class EmailUnit : MonoBehaviour
            {
                public Mail_Record record;
                public Image isRead;
                public TMP_Text emailTitle;
                public TMP_Text lastDays;
                public Sprite read;
                public Sprite unRead;
                public void InitEmailUnit(Mail_Record mail_Record)
                {
                    record = mail_Record;
                    emailTitle.text = record.mail_title;
                    
                    if (record.mail_state == "unread")
                    {
                        isRead.sprite = unRead;
                    }
                    else
                    {
                        isRead.sprite = read;
                        Color readColor;
                        if (ColorUtility.TryParseHtmlString("#B1B1B1", out readColor))
                        {
                            emailTitle.color = readColor;
                        }
                    }

                    //int daysLimit = record.mail_state == "unread" ? 30 : 14;
                    int daysLeft = GetMailDaysLeft(record);
                    if (daysLeft > 0)
                    {
                        lastDays.text = $"剩{daysLeft}天";
                    }
                    else
                    {
                        DestroyImmediate(gameObject);
                    }
                }

                static int GetMailDaysLeft(Mail_Record record)
                {
                    if (record.mail_valid_time > 0)
                    {
                        var expire = record.mail_valid_time > 1_000_000_000_000L
                            ? DateTimeOffset.FromUnixTimeMilliseconds(record.mail_valid_time).LocalDateTime
                            : DateTimeOffset.FromUnixTimeSeconds(record.mail_valid_time).LocalDateTime;
                        return (int)Math.Ceiling((expire - DateTime.Now).TotalDays);
                    }

                    const int daysLimit = 30;
                    int daysPassed = (int)(DateTime.Now - record.mail_date).TotalDays;
                    return daysLimit - daysPassed;
                }

                public void OpenEmailDetail()
                {
                    //EmailCanvasInteracterManager.Instance.OpenEmailDetail(record);
                    UIManager.Instance.GetPanel<EmailPanel>().OpenEmailDetail(record);
                    List<ulong> readId = new List<ulong>
                    {
                        record.mail_id
                    };
                    Email_And_Announcement_Manager.instance.on_read_mail(readId);
                }
            }
        }
    }
}