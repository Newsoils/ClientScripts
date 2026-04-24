using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Social;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Global_Game_Manager : SingletonMono<Global_Game_Manager>
    {
        [Header("Global_State")]
        public string _current_player_id = "Default_Player_ID";
        public string _current_player_name = "Default_Player";

        [HideInInspector]
        public Weather_State _weather_state;
        public Player_Social_Setting _player_brief;
        [Header("Config")]
        public global_const _global_const;
        public List<avatar_icon_info> _avatar_icon_list;
        public List<avatar_icon_info> _avatar_icon_frame_list;
        public TextAsset _avatar_icon_json;

        [Header("UI")]
        public TMP_Text _ouput_text;

        [Header("JSON_Data")]
        public bool _global_const_load_from_json = true;
        public TextAsset _global_const_json;

        [Header("Event")]
        public UnityEvent _on_quit_game = new UnityEvent();
        public UnityEvent _update_main_character_cloth_from_server = new UnityEvent();
        public UnityEvent _upload_main_character_cloth_to_server = new UnityEvent();
        public UnityEvent _update_weather_state_from_server = new UnityEvent();
        public UnityEvent _upload_weather_state_to_server = new UnityEvent();

        public UnityEvent _on_update_player_brief_from_server = new UnityEvent();
        public UnityEvent _on_upload_player_brief_to_server = new UnityEvent();
        void Start()
        {
            DontDestroyOnLoad(this.gameObject);

            _weather_state = new Weather_State();
            _weather_state._on_weather_change += upload_weather_state;
            load_from_json();
        }

        public void quit_game(string _reason, float _delay)
        {
            _ouput_text.text = _reason;
            StartCoroutine(quit_game_co(_delay));
        }


        public IEnumerator quit_game_co(float _delay)
        {
            if (_on_quit_game != null) _on_quit_game.Invoke();
            yield return new WaitForSecondsRealtime(_delay);
            Application.Quit();
        }

        public void try_update_weahter()
        {
            if (_global_const != null)
            {
                bool _flag = _weather_state.update_weather(DateTime.Now, _global_const);
                if (_flag == true)
                {
                    upload_weather_state();
                }
            }
        }

        public void load_default_character()
        {
            PM_RM.load_main_character("Default", (go) =>
            {
                var go_in_local = Instantiate(go);
            });
        }

        public void load_from_json()
        {
            if (_global_const_load_from_json == true)
            {
                if (_global_const_json != null)
                {
                    List<global_const> _data = JsonConvert.DeserializeObject<List<global_const>>(_global_const_json.text);
                    if (_data != null)
                    {
                        _global_const = _data[0];
                    }
                }
            }
        }
        public void set_up_player_info(Network_Msg _msg)
        {
            _current_player_name = _msg.player_id;
            var _info = _msg.detail_info.Split("_#_");
            _current_player_id = _info.Last<string>();
        }

        public void update_main_character_cloth()
        {
            _update_main_character_cloth_from_server.Invoke();
        }

        public void upload_main_character_cloth()
        {
            _upload_main_character_cloth_to_server.Invoke();
        }
        public void update_weather_state()
        {
            _update_weather_state_from_server.Invoke();
        }
        public void upload_weather_state()
        {
            _upload_weather_state_to_server.Invoke();
        }

        public void load_weather_state_from_json(string json)
        {
            var _neo_weather = GF_SP.DeserializeObject<Weather_State>(json);
            _weather_state._current_weather = _neo_weather._current_weather;
            _weather_state._last_update_weather_time = _neo_weather._last_update_weather_time;
            _weather_state._return_to_normal_time = _neo_weather._return_to_normal_time;

        }

        public void load_player_brief_from_json(string json)
        {
            var _brief = GF_SP.DeserializeObject<Player_Social_Setting>(json);
            if (_brief != null)
            {
                _player_brief = _brief;
            }
        }
        public void on_update_player_brief_from_server()
        {
            if (_on_update_player_brief_from_server != null) _on_update_player_brief_from_server.Invoke();
        }
        public void on_upload_player_brief_to_server()
        {
            if (_on_upload_player_brief_to_server != null) _on_upload_player_brief_to_server.Invoke();
        }

    }

}
