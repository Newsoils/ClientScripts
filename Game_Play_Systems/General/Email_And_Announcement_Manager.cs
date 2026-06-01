using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {
            public class Email_And_Announcement_Manager : MonoBehaviour
            {
                public static Email_And_Announcement_Manager instance;

                public List<Mail_Record> _mail_record=new List<Mail_Record>();
                public List<Announcement_Record> _announcement_record=new List<Announcement_Record>();

                public Mail_Record _temp_mail_record;
                public Announcement_Record _temp_announcement_record;
              
                public List<ulong> _temp_mail_id_list;
                public UnityEvent _update_mail_from_server;
                public UnityEvent _on_refresh_mail;
                public UnityEvent _on_refresh_announcement;
                public UnityEvent _on_delete_mail;
                public UnityEvent _on_read_mail;
                public UnityEvent _on_get_mail_reward;
                void Start()
                {
                    if (instance == null)
                    {
                        instance = this;
                        DontDestroyOnLoad(this.gameObject);
                    }
                    else
                    {
                        if (instance != this)
                        {
#if UNITY_EDITOR
                            DestroyImmediate(this.gameObject);
#else
           Destroy(this.gameObject);
#endif
                        }
                    }
                }


                #region Event Triggers
                // Trigger methods (names without leading underscore, similar style to Player_Social_Manager)

                public void update_mail_from_server()
                {
                    if (_update_mail_from_server != null) _update_mail_from_server.Invoke();
                }

                
                public void load_mail_list_from_json(string _json)
                {
                    Debug.Log("Loading mail list from JSON.");
                    _mail_record =GF_SP.DeserializeObject<List<Mail_Record>>(_json);
                }

                public void load_mail_list_from_json(Cmd.MailListRes res)
                {
                    Debug.Log("Loading mail list from List<Common.MailInfo>.");
                    _mail_record.Clear();
                    if (res?.Mails == null || res.Mails.Count == 0)
                        return;

                    foreach (var mail in res.Mails)
                    {
                        Mail_Record mail_record = new Mail_Record();
                        mail_record.mail_id = mail.MailID;
                        mail_record.mail_title = mail.Title?.ParamString ?? string.Empty;
                        mail_record.mail_text = mail.Content?.ParamString ?? string.Empty;
                        mail_record.mail_date = UnixTimeToLocalDateTime(mail.CreateTime);
                        mail_record.mail_valid_time = mail.ValidTime;
                        mail_record.mail_state = mail.IsRead == 2 ? "read" : "unread";
                        if (mail.IsRead == 3)
                            mail_record.mail_state = "deleted";
                        mail_record.isGetReward = mail.IsReceive == 3;
                        mail_record.item_list = new List<item_in_mail>();
                        foreach (var item in mail.ItemAwards)
                        {
                            var itemInfo = Global_Inventory_Manager.Instance._itemDB_SO.GetItemInfo((int)item.ConfigID);
                            if (itemInfo != null)
                            {
                                item_in_mail item_in_mail = new item_in_mail();
                                item_in_mail.item_name = itemInfo.name;
                                item_in_mail.item_quantity = (int)item.Count;
                                mail_record.item_list.Add(item_in_mail);
                            }
                        }
                        _mail_record.Add(mail_record);
                    }
                }

                static DateTime UnixTimeToLocalDateTime(long unixTime)
                {
                    if (unixTime <= 0)
                        return DateTime.Now;

                    // 大于 1e12 视为毫秒，否则为秒（与常见服务端约定一致）
                    if (unixTime > 1_000_000_000_000L)
                        return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).LocalDateTime;
                    return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
                }

                public void load_announcement_list_from_json(string _json)
                {
                    Debug.Log("Loading announcement list from JSON.");
                    _announcement_record = GF_SP.DeserializeObject<List<Announcement_Record>>(_json);
                }

                /// <summary>从登录服 HTTP 回包 <c>Payload.Syss</c> 写入公告列表。</summary>
                public void load_announcement_list_from_syss(JArray syss)
                {
                    _announcement_record = new List<Announcement_Record>();
                    if (syss == null || syss.Count == 0)
                        return;

                    foreach (var token in syss)
                    {
                        if (token is not JObject item)
                            continue;

                        var noticePic = item.Value<string>("NoticePic");
                        var record = new Announcement_Record
                        {
                            anno_id = item.Value<int?>("NoticeID") ?? 0,
                            anno_title = item.Value<string>("NoticeTitle") ?? string.Empty,
                            anno_content = item.Value<string>("NoticeContent") ?? string.Empty,
                            anno_bg_url = NormalizeNoticePic(noticePic),
                            anno_date = DateTime.Now,
                        };
                        if (record.anno_id > 0)
                            _announcement_record.Add(record);
                    }

                    Debug.Log($"[Email_And_Announcement_Manager] 公告已加载 {_announcement_record.Count} 条（Syss）");
                }

                /// <summary>NoticePic 需为 path#sprite；纯数字等占位符不当作资源路径。</summary>
                static string NormalizeNoticePic(string noticePic)
                {
                    if (string.IsNullOrWhiteSpace(noticePic))
                        return string.Empty;
                    var trimmed = noticePic.Trim();
                    if (!trimmed.Contains("#"))
                        return string.Empty;
                    return trimmed;
                }
                public void on_delete_mail(List<ulong> deleted_mail_id_list)
                {
                    _temp_mail_id_list = deleted_mail_id_list;
                    if (_on_delete_mail != null) _on_delete_mail.Invoke();
                }

                public void on_read_mail(List<ulong> read_mail_id_list)
                {
                    _temp_mail_id_list = read_mail_id_list;
                    if (_on_read_mail != null) _on_read_mail.Invoke();
                }


                public void on_get_mail_reward(List<ulong> mail_list_get_reward)
                {
                    _temp_mail_id_list = mail_list_get_reward;
                    if (_on_get_mail_reward != null) _on_get_mail_reward.Invoke();
                }
                public void on_refresh_mail()
                {
                    Debug.Log("on_refresh_mail.");
                    if (_on_refresh_mail != null) _on_refresh_mail.Invoke();
                }

                public void on_refresh_announcement()
                {
                    if (_on_refresh_announcement != null) _on_refresh_announcement.Invoke();
                }
                #endregion
            }
        }
    }
}