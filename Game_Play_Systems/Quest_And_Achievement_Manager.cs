using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.Events;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Quest_And_Achievement_Manager : MonoBehaviour
    {
        public static Quest_And_Achievement_Manager instance;

        public List<achievement_record> _obtained_achievement_list = new List<achievement_record>();
        public List<quest_info> _finished_quest_list = new List<quest_info>();

        public List<achievement_record> pending_finished_achievement_list = new List<achievement_record>();

        public UnityEvent on_achievement_finished = new UnityEvent();
        [Header("Data")]
        public Achievement_Design_Info_DB_SO _config_data_so;
        [Header("Event")]
        public UnityEvent _update_achievement_info_from_server;
        public UnityEvent _on_pending_finished_achievement_changed;
        public UnityEvent _on_get_achievement_reward;
        public UnityEvent _upload_player_action_to_server;
        [Header("Temp_Data")]

        public string current_player_action_name;
        public string current_player_action_detial_json;
        public string current_get_reward_achievement_name;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                // tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
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

        // Triggers _update_achievement_info_from_server
        public void update_achievement_info_from_server()
        {
            if (_update_achievement_info_from_server != null) _update_achievement_info_from_server.Invoke();
        }

        // Triggers _on_pending_finished_achievement_changed
        public void on_pending_finished_achievement_changed()
        {
            if (_on_pending_finished_achievement_changed != null) _on_pending_finished_achievement_changed.Invoke();
        }

        // Triggers _on_get_achievement_reward
        public void on_get_achievement_reward(string _achievement_name)
        {
            current_get_reward_achievement_name = _achievement_name;
            if (_on_get_achievement_reward != null) _on_get_achievement_reward.Invoke();
        }

        // Triggers _upload_player_action_to_server
        public void upload_player_action_to_server(string _action_detail_type, string _action_detail_json)
        {
            current_player_action_name = _action_detail_type;
            current_player_action_detial_json = _action_detail_json;
            if (_upload_player_action_to_server != null) _upload_player_action_to_server.Invoke();
        }
        #endregion
        public void load_obtained_achievement_info(string _json)
        {
            var _data = GF_SP.DeserializeObject<List<achievement_record>>(_json);
            pending_finished_achievement_list.Clear();
            _obtained_achievement_list.Clear();
            foreach (var _item in _data)
            {
                if (_item.current_state == "Unlock_And_No_Obtain_Reward")
                {
                    pending_finished_achievement_list.Add(_item);
                }
                if (_item.current_state == "Unlock_And_Obtain_Reward")
                {
                    _obtained_achievement_list.Add(_item);
                }
            }
        }
        public void load_finished_achievement_info(string _json)
        {
            pending_finished_achievement_list = GF_SP.DeserializeObject<List<achievement_record>>(_json);
        }
        public void add_achievement_to_pending_list(string _json)
        {
            pending_finished_achievement_list.Add(
                GF_SP.DeserializeObject<achievement_record>(_json));
        }
        public void add_achievement_list_to_pending_list(string _json)
        {
            var _data_list = GF_SP.DeserializeObject<List<achievement_record>>(_json);
            if (_data_list == null) return;
            foreach (var item in _data_list)
            {
                pending_finished_achievement_list.Add(item);
            }
        }
    }
}