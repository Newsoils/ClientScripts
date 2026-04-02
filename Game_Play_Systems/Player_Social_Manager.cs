using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel.Social;
using UnityEngine;
using UnityEngine.Events;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Player_Social_Manager : MonoBehaviour
    {
        public static Player_Social_Manager _instance;
        public Player_Social_Setting _social_setting;
        public Current_Player_Social_Info _current_social_info;
        [Header("Temp_Data")]
        public List<Friend_Social_Record> _temp_search_result;
        public string _current_chat_friend_name = "";
        public Social_Chat_Msg _current_chat_msg;
        [Header("For_Visit_Room")]
        public bool _on_visit_friend_room = false;
        public string _next_visit_room_friend_name = "";
        //public CLIP.Project_Mouse.Kernel.Cloth_Suit _friend_main_character_suit;
        [Header("Data_SO")]
        public Social_Chat_Msg_SO _social_chat_msg_so;

        [Space(16)]
        [Header("Event")]
        public string _temp_cache_input_str;
        public UnityEvent _on_refresh_social_state = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_try_find_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_try_add_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_remove_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent<string> _on_confirm_friend = new UnityEvent<string>();
        [HideInInspector]
        public UnityEvent _on_refuse_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_enter_friend_room = new UnityEvent();


        [HideInInspector]
        public UnityEvent _on_take_photo_with_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_send_photo = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_pin_achievement = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_select_photo_in_brief = new UnityEvent();

        [HideInInspector]
        public UnityEvent _on_update_social_setting_from_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_send_social_setting_to_server = new UnityEvent();

        [HideInInspector]
        public UnityEvent _on_update_social_info_from_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_send_social_info_to_server = new UnityEvent();
        /// <summary>
        ///  For Chat
        /// </summary>
        [Header("For_Chat")]

        [HideInInspector]
        public UnityEvent _on_send_social_chat_msg = new UnityEvent();
        [HideInInspector]
        public UnityEvent<string> _update_social_chat_msg_from_server = new UnityEvent<string>();

        public UnityEvent _on_refresh_social_chat_msg = new UnityEvent();

        [Header("For_Present")]
        public string temp_present_receiver_name;
        public string temp_present_name;
        [HideInInspector]
        public UnityEvent _on_send_present_to_friend = new UnityEvent();
        [HideInInspector]
        public UnityEvent _update_present_records_from_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _upload_present_records_to_server = new UnityEvent();
        [HideInInspector]
        public UnityEvent _on_refresh_present_view = new UnityEvent();
        void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                // tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                DontDestroyOnLoad(this.gameObject);
                _social_chat_msg_so.load_chat_msg_from_json();
            }
            else
            {
                if (_instance != this)
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

        public void on_try_find_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_try_find_friend != null) _on_try_find_friend.Invoke();
        }

        public void on_try_add_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_try_add_friend != null) _on_try_add_friend.Invoke();
        }
        public void on_remove_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_remove_friend != null) _on_remove_friend.Invoke();
        }

        public void on_confirm_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_confirm_friend != null) _on_confirm_friend.Invoke(_friend_id);
        }
        public void on_refuse_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_refuse_friend != null) _on_refuse_friend.Invoke();
        }

        public void on_enter_friend_room(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_enter_friend_room != null) _on_enter_friend_room.Invoke();
        }


        public void on_take_photo_with_friend(string _friend_id)
        {
            _temp_cache_input_str = _friend_id;
            if (_on_take_photo_with_friend != null) _on_take_photo_with_friend.Invoke();
        }

        public void on_send_photo(string _photo_info_json)
        {
            if (_on_send_photo != null) _on_send_photo.Invoke();
        }

        public void on_pin_achievement(string achievement_name)
        {
            _temp_cache_input_str = achievement_name;
            if (_on_pin_achievement != null) _on_pin_achievement.Invoke();
        }

        public void on_select_photo_in_brief(string photo_name)
        {
            _temp_cache_input_str = photo_name;
            if (_on_select_photo_in_brief != null) _on_select_photo_in_brief.Invoke();
        }


        public void on_send_present_to_friend(string _friend_name, string present_name)
        {
            temp_present_name = present_name;
            temp_present_receiver_name = _friend_name;

            if (_on_send_present_to_friend != null) _on_send_present_to_friend.Invoke();
        }

        public void update_present_records_from_server()
        {
            if (_update_present_records_from_server != null) _update_present_records_from_server.Invoke();
        }

        public void upload_present_records_to_server()
        {
            if (_upload_present_records_to_server != null) _upload_present_records_to_server.Invoke();
        }
        public void on_refresh_present_view()
        {
            Debug.Log("on_refresh_present_view_Triggered");
            if (_on_refresh_present_view != null) _on_refresh_present_view.Invoke();
        }
        public void on_update_social_setting_from_server()
        {
            if (_on_update_social_setting_from_server != null) _on_update_social_setting_from_server.Invoke();
        }

        public void on_send_social_setting_to_server()
        {
            if (_on_send_social_setting_to_server != null) _on_send_social_setting_to_server.Invoke();
        }
        public void on_update_social_info_from_server()
        {
            if (_on_update_social_info_from_server != null) _on_update_social_info_from_server.Invoke();
        }

        public void on_send_social_info_to_server()
        {
            if (_on_send_social_info_to_server != null) _on_send_social_info_to_server.Invoke();
        }
        public void on_refresh_social_state()
        {
            Debug.Log("on_refresh_social_state_Triggered");
            if (_on_refresh_social_state != null) _on_refresh_social_state.Invoke();
        }

        #endregion

        public void load_find_friend_from_json(string _json)
        {
            _temp_search_result = GF_SP.DeserializeObject<List<Friend_Social_Record>>(_json);

            // on_refresh_social_state();

        }
        public void on_send_social_chat_msg(string _friend_name, int _chat_msg_id)
        {
            _current_chat_friend_name = _friend_name;
            _current_chat_msg = _social_chat_msg_so.chat_msg_db.Find(_m => _m.msg_id == _chat_msg_id);
            _on_send_social_chat_msg.Invoke();
        }

        public void update_social_chat_msg_from_server(string friend_name)
        {
            _update_social_chat_msg_from_server.Invoke(friend_name);
        }

        public void on_refresh_social_chat_msg()
        {
            Debug.Log("on_refresh_social_chat_msg_Triggered");
            _on_refresh_social_chat_msg.Invoke();
        }


        public void load_chat_msg_from_json(string _json)
        {
            var _data = GF_SP.DeserializeObject<List<string>>(_json);
            var _friend_info = _current_social_info.
                _friend_accepted_info_record.Find(_f => _f.friend_name == _data[0]);
            if (_friend_info != null)
            {
                _friend_info._chat_msg = GF_SP.DeserializeObject<List<Social_Chat_Msg_Record>>(_data[1]);
            }

        }
        public void load_social_info_from_json(string _json)
        {
            Debug.Log("Player_Social_Manager_load_social_info_from_json");
            var _temp_record_accepted = _current_social_info._friend_accepted_info_record;
            var _temp_record_pending = _current_social_info._friend_pending_info_record;
            _current_social_info = GF_SP.DeserializeObject<Current_Player_Social_Info>(_json);
            if (_temp_record_accepted != null)
            {
                foreach (var item in _current_social_info._friend_accepted_info_record)
                {
                    var _record = _temp_record_accepted.Find(_r => _r.friend_id == item.friend_id);
                    if (_record != null)
                    {
                        item._behavior_record = _record._behavior_record;
                        item._chat_msg = _record._chat_msg;
                    }
                }
            }
            if (_temp_record_pending != null)
            {
                foreach (var item in _current_social_info._friend_pending_info_record)
                {
                    var _record = _temp_record_pending.Find(_r => _r.friend_id == item.friend_id);
                    if (_record != null)
                    {
                        item._behavior_record = _record._behavior_record;
                        item._chat_msg = _record._chat_msg;
                    }
                }
            }

        }
        public void set_up_visit_room(string _friend_name)
        {
            _on_visit_friend_room = true;
            _next_visit_room_friend_name = _friend_name;
        }

        public void clear_visit_room()
        {
            _on_visit_friend_room = false;
            _next_visit_room_friend_name = "";
        }


    }
}
