using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
 using Newtonsoft.Json.Serialization;
#if ON_UNITY
using UnityEngine;

#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CLIP
{
    namespace Project_Mouse
    {

[System.Serializable]
public class plant_in_level_info 
{
       public string _plant_id_in_level;
       public string _plant_prototype_name;
        [JsonIgnore]
     
        public Plant_Info _plant_info;
        [JsonIgnore]
        [NonSerialized]
         public flower_pot_in_level_info _pot_info;
         
            //public string _pot_id_in_level;
            
          public int current_stage;

         public float remaining_minute_to_next_stage;
         
         public string _speed_up_fertilizer_name = "NULL";

          public string _organic_fertilizer_name = "NULL";

            public bool _will_become_rare = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime _planting_time;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime _last_tick_time;

            public int _remain_harvest_count;
            public string _level_pot_id;

            public float _final_rate_to_become_rare;

            public float _growing_speed_plus = 0.0f;
            public void tick(DateTime _now,out string output_result)
            {
                output_result = "OK";
            }
            public bool is_valid()
            {
                if (_plant_id_in_level == null || _plant_id_in_level == "") return false;
                if (_level_pot_id == null|| _level_pot_id.Length==0) return false;
                return true;
            }
        }
    }
    }
    