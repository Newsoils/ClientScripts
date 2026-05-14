using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
namespace CLIP
{
    namespace Project_Mouse
    {
        [System.Serializable]
        public class planting_level_info
        {
            [JsonIgnore]
            public Planting_Configuration _config = null;
            public List<flower_pot_in_level_info> _pots_in_level = new List<flower_pot_in_level_info>();

            [JsonIgnore]
            public Action<string, int> change_item_count;
            [JsonIgnore]
            public Action<string, int,int> change_pot_count;

            public void tick(in DateTime _now,out string output_result)
            {
                for(int i=0;i< _pots_in_level.Count; i++)
                {
                    if (_pots_in_level[i].is_wet == true)
                    {
                        var _water_time_diff = (_now - _pots_in_level[i]._last_add_water_time).TotalMinutes;
                        _pots_in_level[i].current_water_volume-= (float)_water_time_diff * _config._planting_const.pot_wet_decrease_per_minute;
                        if (_pots_in_level[i].current_water_volume < 0)
                        {
                            _pots_in_level[i].current_water_volume = 0;
                            _pots_in_level[i].is_wet = false;
                        }
                    }
                    var _pot = _pots_in_level[i];
                    tick_plant_in_pot(in _now,ref _pot);
                    check_weed(in _now,_pots_in_level[i]);

                }
                output_result = "OK";
            }

            public void check_weed(in DateTime _now,  flower_pot_in_level_info _pot_info)
            {
                if (_pot_info._plant_level_info != null&& _pot_info._plant_level_info.is_valid()==true) return;
                var _diff_t = _now - _pot_info._last_harvest_time;
                if (_diff_t.TotalMinutes >= _config._planting_const.weed_time_in_minute)
                {
                    _pot_info.have_weed = true;
                }
            }
            public void add_pot_to_level(string pot_name,float_3 position,out string output_result) {

                output_result = "OK";
            }
            public void tick_plant_in_pot(in DateTime _now, ref flower_pot_in_level_info _pot)
            {
                var _plant = _pot._plant_level_info;
            
                if (_plant == null) return;
                if (_plant.is_valid() == false) return;
                if (_plant.current_stage == 4||_plant.current_stage == 5) return;
                var _plant_time_diff = (_now - _plant._last_tick_time).TotalMinutes;
                _plant._last_tick_time = _now;
                if (_pot.is_wet == false)
                {
                    _plant_time_diff = _plant_time_diff * _config._planting_const.no_water_growing_rate;
                }
                
                _plant_time_diff= _plant_time_diff* (1.0f + _plant._growing_speed_plus);

                while (true)
                {
                    _plant.remaining_minute_to_next_stage -= (float)_plant_time_diff;
                    if (_plant.remaining_minute_to_next_stage <= 0)
                    {
                        _plant_time_diff -= (double)_plant.remaining_minute_to_next_stage;
                        int next_valid_stage = _plant.current_stage + 1;
                        while (true)
                        {
                            if (_plant._plant_info.life_cycle_table[next_valid_stage] != 0) break;
                            next_valid_stage++;
                        }

                        _plant.current_stage = next_valid_stage;
                        _plant.remaining_minute_to_next_stage =(float) _plant._plant_info.life_cycle_table[next_valid_stage];

                        if (_plant.current_stage == 4 &&
                            _plant._plant_info.have_hided == true)
                        {
                            check_rare_plant(ref _plant);
                            if (_plant._will_become_rare == true)
                            {
                                change_to_rare_plant(ref _plant);
                            }

                        }
                        if (_plant.current_stage == 4) break;
                    }
                    else
                    {
                        break;
                    }
                }
              
            }

            public void add_pot_to_level(
                string pot_name, 
                float_3 position,
               // float_3 rotation,
                float_3 offset,
                flower_pot_info _pot_info_,
                out flower_pot_in_level_info _pot_info,
                out string output_result)
            {

                  _pot_info = new flower_pot_in_level_info();
           
                _pot_info._pot_id_in_level = pot_name + "_#_" + Guid.NewGuid();
                _pot_info.position = position;
                //_pot_info.rotation = rotation;
                _pot_info._rotation = 0;
                _pot_info._level_obj_offset = offset;
                _pot_info._pot_prototype_name = pot_name;
                _pot_info._pot_info = _pot_info_;


                _pot_info._placing_time = DateTime.Now;
                _pot_info._last_add_water_time = DateTime.Now;
                _pot_info._last_harvest_time = DateTime.Now;
                _pots_in_level.Add(_pot_info);

                if(change_pot_count!=null) change_pot_count(pot_name,-1,-1);
                output_result = "OK";
            }


            public void add_plant_to_level_pot(
                string plant_name,
                string pot_id_in_level,
                out string output_result
                )
            {
                var _pot= _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "Pot not found";
                    return;
                }
                if (_pot._plant_level_info != null
                    &&_pot._plant_level_info.is_valid()==true)
                {
                    output_result = "Pot already has a plant";
                    return;
                }
                _pot._plant_level_info = new plant_in_level_info();
                _pot._plant_level_info._plant_id_in_level = plant_name + "_#_" + Guid.NewGuid();
                var _plant_info = _config.plant_info_list.Find(p => p.plant_name == plant_name);
                _pot._plant_level_info._plant_info = _plant_info;
                _pot._plant_level_info._plant_prototype_name = plant_name;
                
                _pot._plant_level_info.current_stage = 0;
                _pot._plant_level_info.remaining_minute_to_next_stage = _plant_info.life_cycle_table[0];
                _pot._plant_level_info._planting_time=DateTime.Now;
                _pot._plant_level_info._last_tick_time = DateTime.Now;


                _pot._plant_level_info._remain_harvest_count = _plant_info.max_harvest_count;
                _pot._plant_level_info._level_pot_id = _pot._pot_id_in_level;

                _pot._plant_level_info._pot_info = _pot;
                //_pot._plant_level_info._pot_id_in_level=_pot._pot_id_in_level;

                _pot._plant_level_info._final_rate_to_become_rare = _pot._plant_level_info._plant_info.hided_variant_rate;
                _pot.remain_use_count--;

                //TODO update player pot remain_use_count
                if (change_item_count != null) change_item_count(_plant_info.seed_name,-1);
                output_result = "OK";
            }

            public bool check_rare_plant(ref plant_in_level_info _plant_info)
            {
                int seed=System.DateTime.Now.Millisecond;
                var rd=new Random(seed);
                if((float )rd.NextDouble()<= _plant_info._final_rate_to_become_rare)
                {
                    _plant_info._will_become_rare = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public void change_to_rare_plant(ref plant_in_level_info _plant_info)
            {
                var hided_name = _plant_info._plant_info.hided_variant_name;
                _plant_info._plant_prototype_name= hided_name;
                _plant_info._plant_info = _config.plant_info_list.Find(_p => _p.plant_name == hided_name);
                _plant_info._plant_id_in_level = hided_name + "_#_" + Guid.NewGuid();
                _plant_info._will_become_rare = true;
                _plant_info.current_stage = 4;
            }
            public void remove_pot_from_level(
                string pot_id_in_level,
                out string output_result
                )
            {

                var _pot = _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "_pot == null";
                    return;
                }
                int _actual_remain_use_count = _pot.remain_use_count;
                _pot.remain_use_count = -1;

                _pots_in_level.Remove(_pot);
                //TODO return pot to inventory
                if (change_pot_count != null)
                {
                    change_pot_count(_pot._pot_info.pot_name, 1, _actual_remain_use_count);
                }
                output_result = "OK";
            }

            public void rotate_pot_in_level(
                  string pot_id_in_level,
                  int _rotate_offset,
                out string output_result
                )
            {
                var _pot = _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "_pot == null";
                    return;
                }
                _pot._rotation =  (_pot._rotation + 4 + _rotate_offset) % 4;
                output_result = "OK";
            }

            public void move_pot_in_level(
                   string pot_id_in_level,
                  float_3 _position,
                   out string output_result
                )
            {
                var _pot = _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "_pot == null";
                    return;
                }
                _pot.position = _position;
                output_result = "OK";
            }

            public void color_pot_color_in_level(
                string pot_id_in_level,
               string color
             )
            {

            }

            public void remove_plant_from_level_pot(
                string pot_id_in_level,
                out string output_result
                )
            {

                var _pot = _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "Pot not found";
                    return;
                }
                if (_pot._plant_level_info == null
                    || _pot._plant_level_info.is_valid() == false)
                {
                    output_result = "Pot not  plant";
                    return;
                }
                _pot._plant_level_info = null;

                //TODO handle remove pot

                check_remove_pot(_pot);
                output_result = "OK";
            }   
            public void remove_weed_from_level_pot(
                  string pot_id_in_level,
                out string output_result
                )
            {
                var _pot = _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "_pot == null";
                    return;
                }
                if (_pot.have_weed == false)
                {
                    output_result = "_pot.have_weed == false";
                    return;
                }
                _pot.have_weed = false;
                _pot._last_harvest_time= DateTime.Now;
                output_result = "OK";
            }
            public void irrigate_all_level_pot(
      
       out string output_result,
       float volume=1.0f
    )
            {
                for (int i = 0; i < _pots_in_level.Count; i++)
                {
                    _pots_in_level[i].current_water_volume = volume;
                    _pots_in_level[i].is_wet = true;
                    _pots_in_level[i]._last_add_water_time = DateTime.Now;
                    /*
                        if (_pots_in_level[i].is_wet == false)
                       {
                           _pots_in_level[i].current_water_volume = 1.0f;
                           _pots_in_level[i].is_wet = true;
                           _pots_in_level[i]._last_add_water_time = DateTime.Now;
                       }

                     */
                }
                output_result = "OK";
            }

            public void irrigate_level_pot(
                  string pot_id_in_level,
                   out string output_result
                )
            {
                output_result = "OK";
            }

            public void harvest_plant_from_level_pot(
                  string pot_id_in_level,
                   out string output_result
                )
            {
                var _pot = _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "_pot == null";
                    return;
                }
                if (_pot._plant_level_info == null|| _pot._plant_level_info.is_valid()==false)
                {
                    output_result = "_pot._plant_level_info == null";
                    return;
                }
                var _plant_info = _pot._plant_level_info._plant_info;
                //TODO add harvest item to player inventory
                var _harvest_item = _pot._plant_level_info._plant_info.harvested_item_name;
                int seed = System.DateTime.Now.Millisecond;
                var rd = new Random(seed);
                int count = rd.Next(_plant_info.min_harvest_count, _plant_info.max_harvest_count+1);

                //
                var _harvest_flag = _plant_info.return_stage_info;
                _pot._last_harvest_time = System.DateTime.Now;
                if (_harvest_flag == -1)
                {
                    _pot._plant_level_info = null;
                    //TODO handle remove pot

                } else
                if (_harvest_flag == 0)
                {
              
                    _pot._plant_level_info.current_stage = 5;

                }
                else
                {
                    _pot._plant_level_info._remain_harvest_count--;
                    if (_pot._plant_level_info._remain_harvest_count == 0)
                    {
                        _pot._plant_level_info.current_stage = 5;

                    }
                    else
                    {
                        _pot._plant_level_info.current_stage = _harvest_flag;
                        _pot._plant_level_info.remaining_minute_to_next_stage = _plant_info.life_cycle_table[_harvest_flag];
                    }
                }
                if (_pot._plant_level_info == null)
                {
                    check_remove_pot(_pot);
                }
            
                    output_result = "OK";
            }

            public void add_fertilizer_to_plant_in_level_pot(
                  string pot_id_in_level,
                  string fertilizer_name,
                   out string output_result
                )
            {
                var _pot = _pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
                if (_pot == null)
                {
                    output_result = "_pot == null";
                    return;
                }
                if (_pot._plant_level_info == null || _pot._plant_level_info.is_valid() == false)
                {
                    output_result = "_pot._plant_level_info == null";
                    return;
                }
                var _fertilizer=_config.fertilizer_info_list.Find(f => f.fertilizer_name == fertilizer_name);

                if (_fertilizer == null)
                {
                    output_result = "_fertilizer == null";
                    return;
                }
                //if (_fertilizer.is_organic == true)
                //{
                //    _pot._plant_level_info._organic_fertilizer_name = fertilizer_name;
                //    _pot._plant_level_info._final_rate_to_become_rare += _fertilizer.hided_variant_plus;

                //}
                //else
                //{
                //    if (_fertilizer.is_speed_up == true)
                //    {
                //        _pot._plant_level_info._speed_up_fertilizer_name= fertilizer_name;
                //        if (_fertilizer.is_to_mature == true)
                //        {
                //            _pot._plant_level_info.current_stage = 4;
                //        }
                //        else
                //        {
                //            _pot._plant_level_info._growing_speed_plus+=_fertilizer.speed_up_rate;
                //        }
                //    }
                //}

                if (change_item_count != null)
                {
                    change_item_count(fertilizer_name,-1);
                }
                    output_result = "OK";


            }
            public void check_remove_pot(flower_pot_in_level_info _pot)
            {
                if (_pot.remain_use_count == 0)
                {
                    _pot.remain_use_count = -1;
                    _pots_in_level.Remove(_pot);
                }
            }
            public void change_pot_color_of_level_pot(
                  string pot_id_in_level,
                   out string output_result
                )
            {
                output_result = "OK";
            }

            public  void tick_pot(ref flower_pot_in_level_info _pot)
            {

            }
            public void tick_pot(ref plant_in_level_info _plant)
            {

            }
        }
    }
}