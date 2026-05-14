using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            public class Weather_State
            {
                public string _current_weather = "Normal";
                //public bool _have_try_change_weather = false;
                public DateTime _last_update_weather_time = DateTime.MinValue;
                public DateTime _return_to_normal_time = DateTime.MinValue;
                [JsonIgnore]
                public Action _on_weather_change = delegate { };
                public bool update_weather(DateTime _now,global_const _config)
                {
                    
                    int hour = _now.Hour;
                    if( //_last_update_weather_time == DateTime.MinValue||
                        _now> _last_update_weather_time.AddHours(12)
                        )
                    {
                        // if (_have_try_change_weather == true) return;
                        if (_current_weather == "Rain") {
                            if (_now > _return_to_normal_time)
                            {
                                _current_weather = "Normal";
                                _return_to_normal_time=DateTime.MinValue;
                                _on_weather_change.Invoke();
                                // _have_try_change_weather = false;
                                return true;
                            }else return false;
                        }
                    }
                        int seed = (int)_now.Ticks;
                        var rd= new System.Random(seed);
                        double _p= rd.NextDouble();
                        if (_p < _config.rain_p)
                        {
                            _current_weather = "Rain";
                            _return_to_normal_time=_now.AddHours(rd.Next(_config.rain_len_min, _config.rain_len_max + 1));
                            _on_weather_change.Invoke();
                        }
                        //_have_try_change_weather = true;
                        _last_update_weather_time = _now;
                        return true;
                    }
 
                    
                }
            }
        }
    }
 