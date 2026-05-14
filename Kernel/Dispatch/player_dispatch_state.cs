using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.ENUM;
using DataStructures.RandomSelector;
using Newtonsoft.Json;
//using UnityEngine;

namespace CLIP.Project_Mouse.Kernel.Dispatch
{

    [System.Serializable]
    public class player_dispatch_state
    {
        [JsonIgnore]
        public Dispatch_Configuration _dispatch_config;
        public List<string> _map_visited = new List<string>();
        public List<string> _map_cleared = new List<string>();
        public List<string> _photo_obtained = new List<string>();
        public single_dispatch_info _current_dispatch_info;

        public List<single_dispatch_info> dispatch_Bags;


        public string player_state = "At_Home";
        public int player_at_home_length_in_minute = 0;
        public int player_on_dispatch_length_in_minute = 0;

        //[Header("Reward_Info")]
        public string last_visited_map;
        public string last_reward_info;
        public int last_reward_currency;
        public int last_reward_exp;
        public string last_reward_photo_name;
        public List<string> last_reward_item_list = new List<string>();
        public string last_friend_event_name;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> _new_visited_map = new List<string>();


        //[Header("Event_System")]
        [JsonIgnore]
        public Action _on_finishing_preparation;
        [JsonIgnore]
        public Action _on_get_mid_way_photo;



        public player_dispatch_state()
        {
            _current_dispatch_info = new single_dispatch_info();
            dispatch_Bags = new List<single_dispatch_info>()
                        {
                            new single_dispatch_info(),
                            new single_dispatch_info(),
                            new single_dispatch_info()
                        };
        }

        public void load_dispatch_info_from_json(string _data_json)
        {
            var _data = JsonConvert.DeserializeObject<player_dispatch_state>(_data_json);

            if (_data != null)
            {
                this._map_visited = _data._map_visited;
                this._map_cleared = _data._map_cleared;
                this._photo_obtained = _data._photo_obtained;
                this._current_dispatch_info = _data._current_dispatch_info;
                this.player_state = _data.player_state;
                this.player_at_home_length_in_minute = _data.player_at_home_length_in_minute;
                this.player_on_dispatch_length_in_minute = _data.player_on_dispatch_length_in_minute;
                this.last_reward_info = _data.last_reward_info;
                this.last_visited_map = _data.last_visited_map;
                this.last_reward_currency = _data.last_reward_currency;
                this.last_reward_exp = _data.last_reward_exp;
                this.last_reward_photo_name = _data.last_reward_photo_name;
                this.last_reward_item_list = _data.last_reward_item_list;
                this.last_friend_event_name = _data.last_friend_event_name;
            }
        }

        public void Set_Dispatch_Item(Item_Type type, string name)
        {
            switch (type)
            {
                case Item_Type.Food:
                    _current_dispatch_info.carried_food_name = name;
                    break;
                case Item_Type.Snack:
                    _current_dispatch_info.carried_snack_name = name;
                    break;
                case Item_Type.Tape:
                    _current_dispatch_info.carried_tape_name = name;
                    break;
            }
        }



        public string dispatch_system_tick()
        {
            if (player_state == "At_Home")
            {
                player_at_home_length_in_minute++;
                if (try_leave_home() == true)
                {
                    on_start_dispatch();
                    return "On_Dispatch_Start";
                }
                return "At_Home";
            }
            if (player_state == "On_Dispatch")
            {
                player_on_dispatch_length_in_minute++;
                if (DateTime.Now >= _current_dispatch_info.end_time)
                {
                    on_end_dispatch();
                    return "On_Dispatch_End";
                }
                string _mw_report = check_middle_way_event();
                if (_mw_report == "Get_Middle_Way_Photo")
                {
                    _on_get_mid_way_photo.Invoke();
                }
                return "On_Dispatch";
            }
            return "Nothing_Happen";
        }

        public bool try_leave_home()
        {
            //测试，10秒出门
            if (player_at_home_length_in_minute >= 10f)
            {
                player_at_home_length_in_minute = 0;
                return true;
            }

            Departure_Probability dp = _dispatch_config.departure_probability_list.Find(
                (_dp) =>
                {
                    return _dp.min_time <= player_at_home_length_in_minute
                      && _dp.max_time >= player_at_home_length_in_minute;
                }
                );
            if (dp == null) return false;
            int seed = (int)DateTime.Now.ToBinary();
            Random rand = new Random(seed);
            float p = (float)rand.NextDouble();
            return p <= dp.probability;
        }

        public void on_start_dispatch()
        {
            player_state = "On_Dispatch";

            //Clear up
            //_current_dispatch_info = new single_dispatch_info();

            Random rand = new Random(DateTime.Now.Millisecond);

            // --- 随机食物 ---
            if (string.IsNullOrEmpty(_current_dispatch_info.carried_food_name) && _dispatch_config.food_info_list.Count > 0)
            {
                // rand.Next(0, count) 返回 [0, count) 之间的整数
                int index = rand.Next(0, _dispatch_config.food_info_list.Count);
                _current_dispatch_info.carried_food_name = _dispatch_config.food_info_list[index].name;
            }

            // --- 随机零食 ---
            if (string.IsNullOrEmpty(_current_dispatch_info.carried_snack_name) && _dispatch_config.snack_info_list.Count > 0)
            {
                int index = rand.Next(0, _dispatch_config.snack_info_list.Count);
                _current_dispatch_info.carried_snack_name = _dispatch_config.snack_info_list[index].snack_name;
            }

            // --- 随机磁带 ---
            if (string.IsNullOrEmpty(_current_dispatch_info.carried_tape_name) && _dispatch_config.tape_info_list.Count > 0)
            {
                int index = rand.Next(0, _dispatch_config.tape_info_list.Count);
                _current_dispatch_info.carried_tape_name = _dispatch_config.tape_info_list[index].tape_name;
            }


            var _food = _dispatch_config.food_info_list.Find((_f) =>
            {
                return _f.name == _current_dispatch_info.carried_food_name;

            });

            //Get dispatch time length
            int time_len = rand.Next(_food.min_travel_time, _food.max_travel_time);
            _current_dispatch_info.dispatch_length_in_minute = time_len;
            TimeSpan ts = new TimeSpan(0, time_len, 0);
            _current_dispatch_info.start_time = DateTime.Now;
            _current_dispatch_info.end_time = _current_dispatch_info.start_time + ts;



            //select_map
            select_map();


            if (_current_dispatch_info.dispatch_length_in_minute >= 24 * 60)
            {
                if (rand.NextDouble() <= 0.95)
                {
                    _current_dispatch_info.can_get_middle_way_photo = true;
                }
            }
            player_at_home_length_in_minute = 0;

            player_on_dispatch_length_in_minute = 0;

        }
        public (int, int, string) on_end_dispatch()
        {
            get_dispatch_reward();
            last_reward_currency = _current_dispatch_info.reward_currency;
            last_reward_exp = _current_dispatch_info.reward_exp;
            last_reward_photo_name = _current_dispatch_info.reward_photo_name;
            last_reward_item_list = new List<string>(_current_dispatch_info.reward_item_list);
            last_friend_event_name = _current_dispatch_info.friend_event_name;
            last_visited_map = _current_dispatch_info.go_to_map_name;
            last_reward_info = _current_dispatch_info.reward_to_str();
            //TODO save to DB

            player_state = "At_Home";

            var res = (_current_dispatch_info.reward_currency, _current_dispatch_info.reward_exp, _current_dispatch_info.reward_photo_name);

            return res;
        }

        /// <summary>
        /// 根据食物权重随机选地图（等级 + 心情）
        /// </summary>
        public void select_map()
        {
            var _map_level_selector = new DynamicRandomSelector<int>();

            var _food = _dispatch_config.food_info_list.Find((_f) =>
            {
                return _f.name == _current_dispatch_info.carried_food_name;

            });

            _map_level_selector.Add(1, _food.Lv_01_map_weight);
            _map_level_selector.Add(2, _food.Lv_02_map_weight);
            _map_level_selector.Add(3, _food.Lv_02_map_weight);

            _map_level_selector.Build();
            int _map_level = _map_level_selector.SelectRandomItem();

            //filter level
            var _map_list_correct_level = _dispatch_config.map_info_list.FindAll((_m) =>
            {

                return _m.map_level == _map_level;
            });
            if (_map_list_correct_level == null || _map_list_correct_level.Count == 0)
            {
                _map_list_correct_level = new List<Map_Info>();
                _dispatch_config.map_info_list.ForEach((_m) => { _map_list_correct_level.Add(_m); });
            }
            //filter clear
            var _map_list_no_clear = _map_list_correct_level.FindAll((_m) =>
            {
                return _map_cleared.Contains(_m.map_name) == false;
            });
            if (_map_list_no_clear == null || _map_list_no_clear.Count == 0)
            {
                _map_list_no_clear = new List<Map_Info>();
                _map_list_correct_level.ForEach((_m) => { _map_list_no_clear.Add(_m); });
            }

            List<Map_Info> map_with_correct_mood = new List<Map_Info>();

            if (!string.IsNullOrEmpty(_current_dispatch_info.carried_tape_name))
            {
                var tape = _dispatch_config.tape_info_list
                    .Find(t => t.tape_name == _current_dispatch_info.carried_tape_name);

                if (tape != null && tape.mood_tag_list != null && tape.mood_tag_list.Count > 0)
                {
                    map_with_correct_mood = _map_list_no_clear.FindAll(map =>
                        map.mood_tag_list != null &&
                        map.mood_tag_list.Any(mapTag =>
                            tape.mood_tag_list.Contains(mapTag))
                    );
                }
            }

            // 兜底：如果没匹配到，使用原始列表
            if (map_with_correct_mood == null || map_with_correct_mood.Count == 0)
            {
                map_with_correct_mood = new List<Map_Info>(_map_list_no_clear);
            }

            var _map_selector = new DynamicRandomSelector<string>();
            foreach (var _m in map_with_correct_mood)
            {
                _map_selector.Add(_m.map_name, _m.sampling_weight);
            }
            _map_selector.Build();
            _current_dispatch_info.go_to_map_name = _map_selector.SelectRandomItem();
        }
        public string check_middle_way_event()
        {
            if (_current_dispatch_info.middle_way_photo_name != null && _current_dispatch_info.middle_way_photo_name != "NULL")
            {
                return "Already_Trigger_Middle_Way_Event";
            }
            if (_current_dispatch_info.dispatch_length_in_minute >= 24 * 60 &&
                player_on_dispatch_length_in_minute >= (int)(_current_dispatch_info.dispatch_length_in_minute * 0.5f))
            {
                int seed = (int)DateTime.Now.ToBinary();
                System.Random rand = new System.Random(seed);
                float _r_val = (float)rand.NextDouble();
                if (_r_val < 0.05)
                {
                    _current_dispatch_info.middle_way_photo_name = "Not_Get_Photo";
                    return "NULL";
                }
                else
                {
                    Map_Info _map = _dispatch_config.map_info_list.Find((_m) =>
                    {

                        return _m.map_name == _current_dispatch_info.go_to_map_name;
                    });

                    _current_dispatch_info.middle_way_photo_name = try_get_photo(in _map);
                    if (_current_dispatch_info.middle_way_photo_name == "NULL")
                    {
                        _current_dispatch_info.middle_way_photo_name = "Not_Get_Photo";
                    }
                    else
                    {
                        bookkeeping_photo_obtained_and_map_cleared(in _map);
                        return "Get_Middle_Way_Photo";
                    }
                }
            }

            return "NULL";
        }

        public void get_dispatch_reward()
        {
            Map_Info _map = _dispatch_config.map_info_list.Find((_m) =>
            {

                return _m.map_name == _current_dispatch_info.go_to_map_name;
            });

            _current_dispatch_info.reward_currency = _map.reward_currency;
            _current_dispatch_info.reward_exp = _map.reward_exp;
            _current_dispatch_info.reward_photo_name = try_get_photo(in _map);


            bookkeeping_photo_obtained_and_map_cleared(in _map);

            get_reward_items(in _map);

            check_friend_event(in _map);
        }


        public string try_get_photo(in Map_Info map)
        {
            // 1. 计算最终幸运值
            float plus_luck = 0;
            if (!string.IsNullOrEmpty(_current_dispatch_info.carried_snack_name))
            {
                var snack = _dispatch_config.snack_info_list
                    .Find(_s => _s.snack_name == _current_dispatch_info.carried_snack_name);

                if (snack != null)
                    plus_luck = snack.photo_luck_plus;
            }

            float final_luck = map.default_photo_luck + plus_luck;

            var luckList = map.luck_list_for_photo;


            // 按 requiredLuck 升序排序（确保逻辑正确）
            luckList.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            //现在表里面默认最低的稀有度是1
            int current_rarity_index = luckList[0].Item1;


            foreach ((int, int) needLuck in luckList)
            {
                if (final_luck >= needLuck.Item2)
                {
                    current_rarity_index = needLuck.Item1;
                }
            }

            // 4. 从最高稀有度往下尝试抽未获得照片
            bool anyUnobtained = false;

            // 3. 从当前稀有度往下降，寻找未获得的照片
            int rarity = current_rarity_index;
            while (rarity >= luckList[0].Item1)
            {
                var list = GetPhotosByRarity(map, rarity);
                var unobtained = list.FindAll(p => !_photo_obtained.Contains(p.photo_name));

                if (unobtained.Count > 0)
                {
                    // 找到了未获得的照片 → 返回
                    Random r = new Random((int)DateTime.Now.ToBinary());
                    return unobtained[r.Next(0, unobtained.Count)].photo_name;
                }

                // 若此稀有度无未获得照片 → 降一级
                rarity -= 1;
            }

            // 4. 如果是普通稀有度（最低）并且也没有新照片 → 返回一张普通重复照片
            int lowestRarity = luckList[0].Item1;
            var commonList = GetPhotosByRarity(map, lowestRarity);
            if (commonList.Count > 0)
            {
                Random r = new Random((int)DateTime.Now.ToBinary());
                return commonList[r.Next(0, commonList.Count)].photo_name;
            }

            // 5. 如果所有可抽稀有度都已获得 → 返回最高稀有度的任意照片
            if (!anyUnobtained)
            {
                var highestList = GetPhotosByRarity(map, current_rarity_index);
                int seed = (int)DateTime.Now.ToBinary();
                Random rand = new Random(seed);

                if (highestList.Count > 0)
                    return highestList[rand.Next(0, highestList.Count)].photo_name;
            }

            return "NULL";
        }



        private List<Photo_Info> GetPhotosByRarity(Map_Info map, int rarityID)
        {
            List<Photo_Info> result = new List<Photo_Info>();

            foreach (int id in map.photo_Ids)
            {
                var photo = _dispatch_config.photo_info_list.Find(p => p.photo_id == id);
                if (photo != null && photo.rarity == rarityID)
                    result.Add(photo);
            }

            return result;
        }

        public bool is_map_clear(Map_Info map_info)
        {
            foreach (int id in map_info.photo_Ids)
            {
                var photo = _dispatch_config.photo_info_list.Find(p => p.photo_id == id);
                if (photo != null && !_photo_obtained.Contains(photo.photo_name))
                    return false;
            }
            return true;
        }


        public void bookkeeping_photo_obtained_and_map_cleared(in Map_Info _map)
        {
            string rewardPhoto = _current_dispatch_info.reward_photo_name;

            // 1. 记录新获取的照片
            if (!_photo_obtained.Contains(rewardPhoto))
                _photo_obtained.Add(rewardPhoto);

            string mapName = _map.map_name;

            // 2. 记录“是否第一次访问”
            bool isNewVisited = false;

            if (!_map_visited.Contains(mapName))
            {
                _map_visited.Add(mapName);
                isNewVisited = true;
            }

            // 3. 若是第一次访问 → 加入新访问列表
            if (isNewVisited && !_new_visited_map.Contains(mapName))
            {
                _new_visited_map.Add(mapName);
            }

            // 4. 判断地图是否完成（所有照片都已获得）
            bool isAllCollected = true;

            foreach (int photoId in _map.photo_Ids)
            {
                var photo = _dispatch_config.photo_info_list.Find(p => p.photo_id == photoId);
                if (photo == null) continue;

                if (!_photo_obtained.Contains(photo.photo_name))
                {
                    isAllCollected = false;
                    break;
                }
            }

            if (isAllCollected && !_map_cleared.Contains(mapName))
            {
                _map_cleared.Add(mapName);
            }


            if (_photo_obtained.Find(_p => _p == _current_dispatch_info.reward_photo_name) == null)
            {
                _photo_obtained.Add(_current_dispatch_info.reward_photo_name);
            }

        }
        public void get_reward_items(in Map_Info _map)
        {
            float plus_luck = 0;
            if (_current_dispatch_info.carried_snack_name != null && _current_dispatch_info.carried_snack_name != "")
            {
                var snack = _dispatch_config.snack_info_list.Find((_s) =>
                {

                    return _s.snack_name == _current_dispatch_info.carried_snack_name;
                });
                if (snack != null)
                {
                    plus_luck = snack.more_item_plus_value;
                }
            }

            float _reward_p = _map.additional_item_plus + plus_luck;

            int seed = (int)DateTime.Now.ToBinary();
            Random rand = new Random(seed);
            string _map_name = _map.map_name;
            if (rand.NextDouble() <= _reward_p)
            {
                var _reward_pool = _dispatch_config.map_reward_pool_list.Find((_mp) => { return _mp.map_name == _map_name; });
                if (_reward_pool == null) return;
                var _reward_pool_remain = new List<string>();
                var _count_selector = new DynamicRandomSelector<int>();
                for (int i = 0; i < _reward_pool.reward_count_weight_list.Count; i++)
                {
                    _count_selector.Add(i, _reward_pool.reward_count_weight_list[i]);
                }
                _count_selector.Build();
                int count = _count_selector.SelectRandomItem();


                _reward_pool.reward_item_name_list.ForEach(_item => _reward_pool_remain.Add(_item));

                for (int i = 0; i < count; i++)
                {
                    string item_selected = _reward_pool_remain[rand.Next(0, _reward_pool_remain.Count)];
                    _current_dispatch_info.reward_item_list.Add(item_selected);
                    _reward_pool_remain.Remove(item_selected);
                }

            }
        }

        public void check_friend_event(in Map_Info _map)
        {
            int seed = (int)DateTime.Now.ToBinary();
            System.Random rand = new System.Random(seed);
            float _friend_p = (float)rand.NextDouble();
            if (_friend_p <= _map.friend_meeting_probability)
            {
                _current_dispatch_info.friend_event_name = _map.friend_list[rand.Next(0, _map.friend_list.Count)];
            }
        }


        public bool Try_Finish_Preparation()
        {
            return try_finishing_preparation(_current_dispatch_info.carried_food_name, _current_dispatch_info.carried_snack_name, _current_dispatch_info.carried_tape_name);
        }

        /// <summary>
        /// 完成准备
        /// </summary>
        /// <param name="food_name">食物名字</param>
        /// <param name="snack_name"></param>
        /// <param name="tape_name"></param>
        /// <returns></returns>
        public bool try_finishing_preparation(
            string food_name,
            string snack_name,
            string tape_name)
        {
            var food = _dispatch_config.food_info_list.Find((_f) =>
            {

                return _f.name == food_name;
            });
            if (food == null) return false;
            _current_dispatch_info.carried_food_name = food_name;

            var snack = _dispatch_config.snack_info_list.Find((_s) =>
            {
                return _s.snack_name == snack_name;
            });
            if (snack == null) return false;
            _current_dispatch_info.carried_snack_name = snack_name;

            var tape = _dispatch_config.tape_info_list.Find((_t) =>
            {
                return _t.tape_name == tape_name;
            });
            if (tape == null) return false;
            _current_dispatch_info.carried_tape_name = tape_name;

            var _change_list = new List<(string, int)>()
                            {
                                (food_name, -1),
                                (snack_name, -1),
                                (tape_name, 0)
                            };


            //_inventory.Change_Item_Count(
            //    in _change_list
            //);
            if (_on_finishing_preparation != null) _on_finishing_preparation.Invoke();
            return true;
        }
    }



}