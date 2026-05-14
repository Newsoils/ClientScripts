using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;

//using System.Runtime.InteropServices.WindowsRuntime;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            public class Cloth_Suit
            {
                public string _top_name = "NULL";
                public string _upper_name = "NULL";

                public string _dress_name = "NULL";
                public string _lower_name = "NULL";
                public string _bottom_name = "NULL";
                public string _attachment_name = "NULL";
            }

            [System.Serializable]
            public class Character_Clothes_Info
            {
                [JsonIgnore]

                public string _top_name = "NULL";
                public string _upper_name = "NULL";

                public string _dress_name = "NULL";
                public string _lower_name = "NULL";
                public string _bottom_name = "NULL";
                public string _attachment_name = "NULL";

                public List<Cloth_Suit> _cloth_preset = new List<Cloth_Suit>();

                public bool is_auto_change_cloth = false;

                public DateTime _last_time_auto_change_cloth = DateTime.MinValue;

                public string _cached_top = "NULL";
                public string _cached_Lower = "NULL";

                public Character_Clothes_Info()
                {
                    _top_name = "NULL";
                    _upper_name = "NULL";

                    _dress_name = "NULL";
                    _lower_name = "NULL";
                    _bottom_name = "NULL";
                    _attachment_name = "NULL";
                    _cloth_preset = new List<Cloth_Suit>(3);
                }

                public bool IsWearingCloth(string clothName)
                {
                    if (string.IsNullOrEmpty(clothName))
                        return false;

                    if (_dress_name != "NULL")
                    {
                        return
                            _top_name == clothName ||
                            _dress_name == clothName ||
                            _bottom_name == clothName ||
                            _attachment_name == clothName;
                    }
                    else
                    {
                        return
                            _top_name == clothName ||
                            _upper_name == clothName ||
                            _lower_name == clothName ||
                           _bottom_name == clothName ||
                            _attachment_name == clothName;
                    }
                }

                public Character_Clothes_Info(Character_Clothes_Info clothes_Info)
                {
                    this._top_name = clothes_Info._top_name;
                    this._upper_name = clothes_Info._upper_name;
                    this._dress_name = clothes_Info._dress_name;
                    this._lower_name = clothes_Info._lower_name;
                    this._bottom_name = clothes_Info._bottom_name;
                    this._attachment_name = clothes_Info._attachment_name;
                    this._cloth_preset = new List<Cloth_Suit>(clothes_Info._cloth_preset);
                    this.is_auto_change_cloth = clothes_Info.is_auto_change_cloth;
                    this._last_time_auto_change_cloth = clothes_Info._last_time_auto_change_cloth;
                    _cached_Lower = this._lower_name;
                    _cached_top = this._top_name;
                }

                public bool SameAs(Character_Clothes_Info other)
                {
                    if(other == null) return false;

                    return _top_name == other._top_name
                        && _upper_name == other._upper_name
                        && _dress_name == other._dress_name
                        && _lower_name == other._lower_name
                        && _bottom_name == other._bottom_name
                        && _attachment_name == other._attachment_name;
                }


                public void Refresh_Cloth_Data(Character_Clothes_Info _ref)
                {
                    _top_name = _ref._top_name;
                    _upper_name = _ref._upper_name;
                    _dress_name = _ref._dress_name;
                    _lower_name = _ref._lower_name;
                    _bottom_name = _ref._bottom_name;
                    _attachment_name = _ref._attachment_name;
                    _cloth_preset = _ref._cloth_preset;
                    is_auto_change_cloth = _ref.is_auto_change_cloth;
                    _last_time_auto_change_cloth = _ref._last_time_auto_change_cloth;
                }

                public List<cloth_info> Get_All_Cloth(List<cloth_info> cloth_infos)
                {
                    var result = new List<cloth_info>();
                    cloth_infos.Find(cloth_infos =>
                    {
                        if (cloth_infos.cloth_name == _top_name ||
                            cloth_infos.cloth_name == _upper_name ||
                            cloth_infos.cloth_name == _dress_name ||
                            cloth_infos.cloth_name == _lower_name ||
                            cloth_infos.cloth_name == _bottom_name ||
                            cloth_infos.cloth_name == _attachment_name)
                        {
                            result.Add(cloth_infos);
                        }
                        return false;
                    });

                    return result;
                }


                public void Set_Default(Cloth_Default_Config cloth_Default_Config)
                {
                    _top_name = cloth_Default_Config.default_top;
                    _upper_name= cloth_Default_Config.default_upper;
                    _lower_name= cloth_Default_Config.default_lower;
                    _bottom_name= cloth_Default_Config.default_buttom;
                    _attachment_name   = "NULL";
                    _dress_name = "NULL";
                }

                public bool Add_Cloth( string _cloth_name,List<cloth_info> cloth_Infos ,Cloth_Default_Config _config)
                {
                    var _cloth_info = cloth_Infos.Find(x => x.cloth_name == _cloth_name);

                    if (_cloth_info.first_category == Cloth_First_Category.Accessory)
                    {
                        if (_cloth_info.second_category == Cloth_Second_Category.SpecialAcc)
                        {
                            _attachment_name = _cloth_name;
                            return true;
                        }
                        else if (_cloth_info.second_category== Cloth_Second_Category.SpecialHead)
                        {
                            _top_name = _cloth_name;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (_cloth_info.first_category == Cloth_First_Category.OnePiece)
                    {
                        _dress_name = _cloth_name;
                        _upper_name = _config.empty_upper;
                        _lower_name = _config.empty_lower;
                        return true;
                    }

                    else if (_cloth_info.first_category == Cloth_First_Category.Top)
                    {
                        _upper_name = _cloth_name;
                        _dress_name = "NULL";
                        return true;
                    }

                    else if (_cloth_info.first_category == Cloth_First_Category.Bottom)
                    {
                        _lower_name = _cloth_name;
                        _dress_name = "NULL";
                        return true;
                    }

                    else if (_cloth_info.first_category == Cloth_First_Category.Footwear)
                    {
                        _bottom_name = _cloth_name;
                        return true;
                    }
                    else if (_cloth_info.first_category == Cloth_First_Category.Headwear)
                    {
                        _top_name = _cloth_name;
                        return true;
                    }
                    return false;
                }


                public bool Remove_cloth(string clothName,Cloth_Default_Config default_Config)
                {
                    if (_top_name == clothName)
                    {
                        _top_name = default_Config.default_top;
                    }
                    else if (_upper_name == clothName)
                    {
                        _upper_name = default_Config.default_upper;
                    }
                    else if (_lower_name == clothName)
                    {
                       _lower_name = default_Config.default_lower;
                    }
                    else if (_bottom_name == clothName)
                    {
                        _bottom_name = default_Config.default_buttom;
                    }
                    else if ( _dress_name == clothName)
                    {
                        _dress_name = "NULL";
                        _upper_name = default_Config.default_upper;
                        _lower_name = default_Config.default_lower;
                    }
                    else if ( _attachment_name == clothName)
                    {
                         _attachment_name = "NULL";
                    }
                    else
                    {
                        return false;
                    }
                    return true;
                }
                public void change_preset(Cloth_Suit _preset, int index)
                {
                    if (index >= _cloth_preset.Count)
                    {
                        _cloth_preset.Add(_preset);
                    }
                    else
                    {
                        _cloth_preset[index] = _preset;
                    }
                }
                public Cloth_Suit get_current()
                {
                    return new Cloth_Suit()
                    {
                        _top_name = _top_name,
                        _upper_name = _upper_name,
                        _dress_name = _dress_name,
                        _lower_name = _lower_name,
                        _bottom_name = _bottom_name,
                        _attachment_name = _attachment_name
                    };
                }
                public void wear_preset(int index)
                {
                    if (index < _cloth_preset.Count)
                    {
                        var _preset = _cloth_preset[index];
                        _top_name = _preset._top_name;
                        _upper_name = _preset._upper_name;
                        _dress_name = _preset._dress_name;
                        _lower_name = _preset._lower_name;
                        _bottom_name = _preset._bottom_name;
                        _attachment_name = _preset._attachment_name;
                    }
                }

                public bool WearPreset(int index)
                {
                    if (index < 0 || index >= _cloth_preset.Count)
                    {
                        return false;
                    }

                    var _preset = _cloth_preset[index];
                    if (!IsPresetValid(_preset))
                    {
                        return false;
                    }

                    _top_name = _preset._top_name;
                    _upper_name = _preset._upper_name;
                    _dress_name = _preset._dress_name;
                    _lower_name = _preset._lower_name;
                    _bottom_name = _preset._bottom_name;
                    _attachment_name = _preset._attachment_name;
                    return true;
                }

                private bool IsPresetValid(Cloth_Suit preset)
                {
                    if (preset == null) return false;
                    return
                        preset._top_name != "NULL" ||
                        preset._upper_name != "NULL" ||
                        preset._dress_name != "NULL" ||
                        preset._lower_name != "NULL" ||
                        preset._bottom_name != "NULL" ||
                        preset._attachment_name != "NULL";
                }
                //public bool Try_auto_change_cloth(Game_Inventory _inventory, List<cloth_info> clothInfo_db, Cloth_Default_Config cloth_Default_Config, DateTime _now)
                //{
                //    if ((DateTime.Now - _last_time_auto_change_cloth).TotalHours < 24) return false;
                //    return Try_auto_change_cloth(_inventory, clothInfo_db, cloth_Default_Config);
                //}


                //public bool Try_auto_change_cloth(Game_Inventory _inventory,List<cloth_info> cloth_db, Cloth_Default_Config cloth_Default_Config)
                //{
                    //if (is_auto_change_cloth == false) return false;

                    //Set_Default(cloth_Default_Config);
                    ////return;
                    //var rand = new Random((int)DateTime.Now.Ticks);

                    //var _dress_list = _inventory._current_inventory.FindAll(_item => {
                    //    if (_item.item_info.type != ENUM.Item_Type.Cloth) return false;
                    //    if (_item._cloth_info == null) return false;
                    //    if (_item._cloth_info.first_Category != "裙子") return false;
                    //    else return true;

                    //}); 
                    //bool is_dress= rand.Next(0, 2) == 0 ? true : false;
                    //if (_dress_list.Count == 0) is_dress = false;

                    //if (is_dress == true)
                    //{
                    //    _dress_name= _dress_list[rand.Next(0, _dress_list.Count)].item_info.name;

                    //}
                    //else
                    //{
                    //    var _upper_list = _inventory._current_inventory.FindAll(_item => {
                    //        if (_item.item_info.type != ENUM.Item_Type.Cloth) return false;
                    //        if (_item._cloth_info == null) return false;
                    //        if (_item._cloth_info.first_Category != "上衣") return false;
                    //        else return true;

                    //    });
                    //    if (_upper_list.Count != 0)
                    //    {
                    //       _upper_name = _upper_list[rand.Next(0, _upper_list.Count)].item_info.name;
                    //    }
                    //    var _lower_list = _inventory._current_inventory.FindAll(_item => {
                    //        if (_item.item_info.type != ENUM.Item_Type.Cloth) return false;
                    //        if (_item._cloth_info == null) return false;
                    //        if (_item._cloth_info.first_Category != "裤子") return false;
                    //        else return true;
                    //    });
                    //    if (_lower_list.Count != 0)
                    //    {
                    //        _lower_name = _lower_list[rand.Next(0, _lower_list.Count)].item_info.name;
                    //    }
                    //}
                    //var _bottom_list = _inventory._current_inventory.FindAll(_item =>
                    //{
                    //    if (_item.item_info.type != ENUM.Item_Type.Cloth) return false;
                    //    if (_item._cloth_info == null) return false;
                    //    if (_item._cloth_info.first_Category != "鞋袜") return false;
                    //    else return true;
                    //});
                    //if (_bottom_list.Count != 0)
                    //{
                    //    _bottom_name = _bottom_list[rand.Next(0, _bottom_list.Count)].item_info.name;
                    //}
                    //var _top_list = _inventory._current_inventory.FindAll(_item => {
                    //    if (_item.item_info.type != ENUM.Item_Type.Cloth) return false;
                    //    if (_item._cloth_info == null) return false;
                    //    if (_item._cloth_info.first_Category != "配饰") return false;
                    //    if (_item._cloth_info.slots_occupied.Contains("头") == false) return false;
                    //      return true;
                        

                    //});
                    //if (_top_list.Count != 0)
                    //{
                    //    _top_name = _top_list[rand.Next(0, _top_list.Count)].item_info.name;
                    //}
                    //var _attachment_list = _inventory._current_inventory.FindAll(_item =>
                    //{
                    //    if (_item.item_info.type != ENUM.Item_Type.Cloth) return false;
                    //    if (_item._cloth_info == null) return false;
                    //    if (_item._cloth_info.first_Category != "配饰") return false;
                    //    if (_item._cloth_info.slots_occupied.Contains("小配饰") == false) return false;
                    //    return true;

                    //});

                    //if (_attachment_list.Count != 0)
                    //{
                    //    _attachment_name = _attachment_list[rand.Next(0, _attachment_list.Count)].item_info.name;
                    //}
             
                    //_last_time_auto_change_cloth = DateTime.Today.AddHours(6);
                    //return true;
                //}
            
            }


        }
    }
}
 