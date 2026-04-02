using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using CLIP.Project_Mouse.Kernel.Dispatch;
using System;

namespace CLIP.Project_Mouse.Game_Play_System.Dispatch_System
{
    [CreateAssetMenu(fileName = "Dispatch_DB_SO", menuName = "Project_Mouse/Dispatch_DB_SO")]
    public class Dispatch_DB_SO : ScriptableObject
    {
        public Dispatch_Configuration _dispatch_config;

        [Header("JSON_File")]
        public TextAsset tape_info_json_file;
        public TextAsset food_info_json_file;
        public TextAsset snack_info_json_file;
        public TextAsset map_info_json_file;
        public TextAsset map_reward_pool_json_file;
        public TextAsset photo_info_json_file;
        public TextAsset photo_rarity_json_file;
        public TextAsset departure_probability_json_file;

        public void OnEnable()
        {
            RefreshData();
        }


        public void OnValidate()
        {
            RefreshData();
        }

        [ContextMenu("加载数据")]
        public void RefreshData()
        {
            if (tape_info_json_file != null) _dispatch_config.tape_info_list = JsonConvert.DeserializeObject<List<Tape_Info>>(tape_info_json_file.text);
            if (food_info_json_file != null) _dispatch_config.food_info_list = JsonConvert.DeserializeObject<List<Food_Info>>(food_info_json_file.text);
            if (snack_info_json_file != null) _dispatch_config.snack_info_list = JsonConvert.DeserializeObject<List<Snack_Info>>(snack_info_json_file.text);
            if (map_info_json_file != null) _dispatch_config.map_info_list = JsonConvert.DeserializeObject<List<Map_Info>>(map_info_json_file.text);
            if (map_reward_pool_json_file != null) _dispatch_config.map_reward_pool_list = JsonConvert.DeserializeObject<List<Map_Reward_Pool>>(map_reward_pool_json_file.text);
            if (photo_info_json_file != null) _dispatch_config.photo_info_list = JsonConvert.DeserializeObject<List<Photo_Info>>(photo_info_json_file.text);
            if (departure_probability_json_file != null) _dispatch_config.departure_probability_list = JsonConvert.DeserializeObject<List<Departure_Probability>>(departure_probability_json_file.text);
            if(photo_rarity_json_file != null)
            {
                _dispatch_config.photo_rarity_list= JsonConvert.DeserializeObject<List<Photo_Rarity>>(photo_rarity_json_file.text);
               _dispatch_config.photo_rarity_Dic = new Dictionary<int, string>();
                foreach (var photo_Rarity in _dispatch_config.photo_rarity_list)
                {
                    _dispatch_config.photo_rarity_Dic[photo_Rarity.id] = photo_Rarity.photo_Rarity_Name;
                }
            }

            if(_dispatch_config.photo_info_list != null)
            {
                _dispatch_config.photo_map_index_Dic = new Dictionary<int, int>();
                for (int i = 0; i < _dispatch_config.photo_info_list.Count; i++)
                {
                    var mapID = _dispatch_config.map_info_list.FindIndex(x => x.photo_Ids.Contains(_dispatch_config.photo_info_list[i].photo_id));
                    _dispatch_config.photo_map_index_Dic[i] = mapID;
                }
            }
        }

        public single_dispatch_info Get_Random_Package()
        {
            single_dispatch_info ans = new single_dispatch_info();
            int seed = (int)DateTime.Now.ToBinary();
            //Debug.Log("get_random_package()_seed_=_"+ seed);
            System.Random rand = new System.Random(seed);
            ans.carried_food_name = _dispatch_config.food_info_list[rand.Next(0, _dispatch_config.food_info_list.Count)].name;

            ans.carried_snack_name = _dispatch_config.snack_info_list[rand.Next(0, _dispatch_config.snack_info_list.Count)].snack_name;

            ans.carried_tape_name = _dispatch_config.tape_info_list[rand.Next(0, _dispatch_config.tape_info_list.Count)].tape_name;
            return ans;
        }
    }


}