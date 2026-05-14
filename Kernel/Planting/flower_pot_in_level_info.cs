using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
 using Newtonsoft.Json.Serialization;
namespace CLIP
{
    namespace Project_Mouse
    {

namespace Kernel
        {
            [System.Serializable]
            public class flower_pot_in_level_info
            {
                public string _pot_id_in_level;
                public float_3 position;
                public float_3 _level_obj_offset;
                //public float_3 rotation;
               
           
                
                public string _pot_prototype_name;
                [JsonIgnore]
                public flower_pot_info _pot_info;
                public int remain_use_count;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
                public plant_in_level_info _plant_level_info = null;

                public bool is_wet;
                public bool have_weed;
                public float current_water_volume = 1.0f;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
                public DateTime _placing_time;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
                public DateTime _last_add_water_time;
                [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
                public DateTime _last_harvest_time;
                
                public int _rotation = 0;
                public flower_pot_in_level_info()
                {
                    is_wet = true;
                    have_weed = false;
                    current_water_volume = 1.0f;
                    _plant_level_info = null;
                }
            }
        }

}

}
