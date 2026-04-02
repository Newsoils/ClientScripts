using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
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
              
                public List<int> _temp_mail_id_list;
                public UnityEvent _update_mail_from_server;
                public UnityEvent _update_annoncement_from_server;
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

                public void update_announcement_from_server()
                {
                    if (_update_annoncement_from_server != null) _update_annoncement_from_server.Invoke();
                }

                
                public void load_mail_list_from_json(string _json)
                {
                    Debug.Log("Loading mail list from JSON.");
                    _mail_record =GF_SP.DeserializeObject<List<Mail_Record>>(_json);
                }

                public void load_announcement_list_from_json(string _json)
                {
                    Debug.Log("Loading announcement list from JSON.");
                    _announcement_record = GF_SP.DeserializeObject<List<Announcement_Record>>(_json);
                }
                public void on_delete_mail(List<int> deleted_mail_id_list)
                {
                    _temp_mail_id_list = deleted_mail_id_list;
                    if (_on_delete_mail != null) _on_delete_mail.Invoke();
                }

                public void on_read_mail(List<int> read_mail_id_list)
                {
                    _temp_mail_id_list = read_mail_id_list;
                    if (_on_read_mail != null) _on_read_mail.Invoke();
                }


                public void on_get_mail_reward(List<int> mail_list_get_reward)
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