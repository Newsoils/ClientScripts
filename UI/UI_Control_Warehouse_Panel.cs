//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Framework_Core.Event;
//using CLIP.Framework_Unity;
//using CLIP.Project_Mouse.Client_Event_Systems;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using CLIP.Project_Mouse.Kernel;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {

//            public class UI_Control_Warehouse_Panel : MonoBehaviour
//            {
//                public Game_Play_System.GameItem_DB_SO _inventory;
//                public List<ENUM.Item_Type> _show_item_type_list;
//                public RectTransform _first_level_root;
//                public List<Button> _first_buttons = new List<Button>();
//                public List<RectTransform> _secondary_panels = new List<RectTransform>();
//                public TMP_Dropdown _sort_mode_dd;
//                public GameObject normalSortMode;
//                public GameObject potSortMode;
//                public Button _btn_close_all;
//                public string _sort_mode = "获取时间";
//                public NormalWarehouseSortModeUnit currentSortMode;


//                public GameObject Game_Item_Unit_Prefab;
//                public RectTransform Game_Item_Unit_Root;
//                public List<UI_Control_Game_Item_Unit> _game_item_units = new List<UI_Control_Game_Item_Unit>();

//                public Kernel.Game_Item_In_Inventory _current_selected_game_item = null;
//                public UI_Control_Game_Item_Unit _current_selected_item_ui = null;

//                public string first_level_search_tag = "";
//                public List<string> second_level_search_tag = new List<string>();
//                public List<string> detail_search_item_tags = new List<string>();



//                [Header("For_Additional_Search")]
//                public GameObject Panel_Search;
//                public Button _btn_open_search_panel;
//                public Button _btn_close_search_panel;
//                public TMP_InputField _search_input_field;
//                public Button _btn_comfirm_search_button;
//                public GameObject Panel_Filter_Pot;
//                public GameObject Panel_Filter_Seed;

//                public List<RectTransform> _panel_detail_filter;

//                public Color _active_color;
//                public Color _deactive_color;

//                public List<string> _pot_color_tags = new List<string>() { "黑色", "蓝色", "绿色", "黄色", "红色", "粉色", "紫色", "棕色", "灰色", "原色" };

//                [Header("For_Item_Description")]
//                public GameObject Panel_ItemDescription;
//                public TMP_Text _item_Name;
//                public TMP_Text _item_Description;
//                public TMP_Text _sell_price;
//                public TMP_Text _obtain_date;
//                public TMP_Text quantity;
//                public GameObject normalRarity;
//                public GameObject rareRarity;
//                public GameObject previousRarity;
//                public GameObject collected;
//                public GameObject uncollected;
//                public UI_Control_Game_Item_Unit detailItem;

//                [Header("Event_Systems")]
//                public Warehouse_UI_Event_Hub_SO _warehouse_ui_event_hub;

//                List<Game_Item_In_Inventory> _current_resulting_inventory;
//                void Start()
//                {
//                    _binding_first_level_buttons();
//                    _binding_secondary_level_button();
//                    _binding_detail_level_button();
//                    if (_sort_mode_dd != null) _sort_mode_dd.onValueChanged.AddListener(_sort_mode_change);
//                    init_game_items();

//                    _btn_open_search_panel.onClick.AddListener(on_open_search_panel);
//                    _btn_close_search_panel.onClick.AddListener(on_close_search_panel);
//                    _btn_comfirm_search_button.onClick.AddListener(on_confirm_search);

//                    //_inventory.load_current_inventory_from_json();

//                    //if (_btn_close_all != null) _btn_close_all.onClick.AddListener(_invoke_on_close_warehouse_UI);


//                    if (_warehouse_ui_event_hub != null)
//                    {
//                        _warehouse_ui_event_hub._on_refresh_warehouse_UI.AddListener(refresh_filtering_and_sorting);
//                        _warehouse_ui_event_hub._deselected_item.AddListener(DeselectItem);
//                        _warehouse_ui_event_hub._toggle_selection_current_unit.AddListener(ToggleSelectionCurrentUnit);
//                    }
//                }

//                public void OnEnable()
//                {
//                    var _indoor = this.GetComponentInParent<Indoor_Room_Game_Manager>();
//                    if (_indoor != null)
//                    {
//                        if (_indoor._level_info._room_name == "阳台")
//                        {
//                            first_level_search_tag = "Pot";
//                            SetPotSortMode("花盆大小");
//                        }
//                        else
//                        {
//                            first_level_search_tag = "Room_Placement";
//                            SetNormalSortMode("获取时间");
//                        }

//                    }

//                    init_game_items();
//                }


//                public void _sort_mode_change(int value)
//                {
//                    _sort_mode = _sort_mode_dd.options[value].text;
//                    Debug.Log("Search Mode Changed: " + _sort_mode);

//                    _warehouse_ui_event_hub._invoke_sort_mode_change_change(_sort_mode);

//                    refresh_filtering_and_sorting();
//                }

//                public void init_game_items()
//                {
//                    _current_resulting_inventory = _inventory._inventory._current_inventory;
//                    _game_item_units = new List<UI_Control_Game_Item_Unit>(Game_Item_Unit_Root.gameObject.GetComponentsInChildren<UI_Control_Game_Item_Unit>());
//                    refresh_filtering_and_sorting();
//                    _update_item_view();
//                }
//                public void refresh_game_items()
//                {

//                }

//                public void OnDisable()
//                {
//                    _current_selected_game_item = null;
//                    _current_selected_item_ui = null;

//                    first_level_search_tag = "";
//                    second_level_search_tag.Clear();
//                    detail_search_item_tags.Clear();
//                    reset_button();
//                }
//                public void reset_button()
//                {
//                    foreach (var _sp in _secondary_panels)
//                    {
//                        var _btn = _sp.GetComponentsInChildren<Button>();
//                        foreach (var b in _btn)
//                        {
//                            var img = b.GetComponent<Image>();
//                            if (img != null) img.color = _deactive_color;
//                        }
//                    }

//                }
//                public void on_open_search_panel()
//                {
//                    Panel_Search.SetActive(true);
//                    _search_input_field.text = null;
//                    if (first_level_search_tag == "Pot")
//                    {
//                        Panel_Filter_Pot.SetActive(true);
//                        Panel_Filter_Seed.SetActive(false);
//                    }
//                    else if (first_level_search_tag == "Seed")
//                    {
//                        Panel_Filter_Pot.SetActive(false);
//                        Panel_Filter_Seed.SetActive(true);
//                    }
//                    else
//                    {
//                        Panel_Filter_Pot.SetActive(false);
//                        Panel_Filter_Seed.SetActive(false);
//                    }
//                }
//                public void on_close_search_panel()
//                {
//                    _search_input_field.text = null;
//                    Panel_Search.SetActive(false);

//                    detail_search_item_tags.Clear();
//                    refresh_filtering_and_sorting();
//                    //_update_item_view();
//                }

//                public void _binding_first_level_buttons()
//                {
//                    _first_buttons = new List<Button>(_first_level_root.GetComponentsInChildren<Button>());
//                    foreach (var button in _first_buttons)
//                    {
//                        var _name = button.gameObject.name;
//                        var _tag = _name.Substring(4, button.gameObject.name.Length - 4);
//                        button.onClick.AddListener(() =>
//                        {

//                            first_level_search_tag = _tag;
//                            _current_selected_game_item = null;
//                            _current_selected_item_ui = null;
//                            second_level_search_tag.Clear();
//                            detail_search_item_tags.Clear();
//                            _switch_secondary_panel(_tag);

//                            if (Panel_Search.activeSelf == true)
//                            {
//                                on_close_search_panel();
//                                on_open_search_panel();
//                            }
//                            if (first_level_search_tag == "Pot")
//                            {
//                                //UI_Helper.add_option_to_dropdown(_sort_mode_dd, "花盆大小");
//                                //UI_Helper.set_dropdown(_sort_mode_dd, "花盆大小");
//                                SetPotSortMode("花盆大小");
//                            }
//                            else
//                            {
//                                //UI_Helper.remove_option_to_dropdown(_sort_mode_dd, "花盆大小");
//                                if (first_level_search_tag == "Seed")
//                                {
//                                    //UI_Helper.set_dropdown(_sort_mode_dd, "获取时间");
//                                    SetNormalSortMode("获取时间");
//                                }
//                                else if (first_level_search_tag == "Fertilizer")
//                                {
//                                    //UI_Helper.set_dropdown(_sort_mode_dd, "持有数量");
//                                    SetNormalSortMode("持有数量");
//                                }
//                                else
//                                {
//                                    SetNormalSortMode("获取时间");
//                                }

//                            }
//                            _warehouse_ui_event_hub._invoke_first_level_search_tag_change_change(first_level_search_tag);
//                            refresh_filtering_and_sorting();
//                        }

//                       // _update_item_view();

//                       );
//                    }
//                }
//                public void on_confirm_search()
//                {
//                    refresh_filtering_and_sorting();
//                }
//                public void _binding_secondary_level_button()
//                {
//                    foreach (var _rt in _secondary_panels)
//                    {
//                        _binding_secondary_level_button_from_root(_rt);
//                    }
//                }

//                public void _binding_secondary_level_button_from_root(RectTransform _root)
//                {
//                    var _btns = new List<Button>(_root.GetComponentsInChildren<Button>());
//                    foreach (var button in _btns)
//                    {
//                        var _btn = button;
//                        var _image = button.GetComponent<Image>();
//                        //var _image = button.transform.parent.GetComponent<Image>();
//                        //var color = _image.color;
//                        //color.a = 0f;
//                        //_image.color = color;
//                        var _name = _btn.gameObject.name;
//                        var _tag = _name.Substring(4, _btn.gameObject.name.Length - 4);
//                        _btn.onClick.AddListener(() =>
//                        {
//                            _current_selected_game_item = null;
//                            _current_selected_item_ui = null;
//                            if (second_level_search_tag.Contains(_tag))
//                            {
//                                second_level_search_tag.Remove(_tag);
//                                _image.color = _deactive_color;
//                                //var color = _image.color;
//                                //color.a = 0f;
//                                //_image.color = color;
//                            }
//                            else
//                            {
//                                second_level_search_tag.Add(_tag);
//                                _image.color = _active_color;
//                                //var color = _image.color;
//                                //color.a = 1f;
//                                //_image.color = color;
//                            }
//                            _warehouse_ui_event_hub._invoke_second_level_search_tag_change(second_level_search_tag);
//                            refresh_filtering_and_sorting();

//                            // _update_item_view();
//                        });
//                    }
//                }

//                public void _binding_detail_level_button()
//                {
//                    foreach (var _rt in _panel_detail_filter)
//                    {
//                        _binding_detail_level_button_from_root(_rt);
//                    }
//                }

//                public void _binding_detail_level_button_from_root(RectTransform _root)
//                {
//                    var _btns = new List<Button>(_root.GetComponentsInChildren<Button>(true));
//                    foreach (var button in _btns)
//                    {
//                        var _btn = button;
//                        //var _image = button.GetComponent<Image>();
//                        var _image = button.transform.parent.GetComponent<Image>();
//                        //_image.color = _deactive_color;
//                        var color = _image.color;
//                        color.a = 0f;
//                        _image.color = color;
//                        var _name = _btn.gameObject.name;
//                        var _tag = _name.Substring(4, _btn.gameObject.name.Length - 4);
//                        _btn.onClick.AddListener(() =>
//                        {
//                            if (detail_search_item_tags.Contains(_tag))
//                            {
//                                detail_search_item_tags.Remove(_tag);
//                                //_image.color = _deactive_color;
//                                var color = _image.color;
//                                color.a = 0f;
//                                _image.color = color;
//                            }
//                            else
//                            {
//                                detail_search_item_tags.Add(_tag);
//                                //_image.color = _active_color;
//                                var color = _image.color;
//                                color.a = 1f;
//                                _image.color = color;
//                            }
//                            _current_selected_game_item = null;
//                            _current_selected_item_ui = null;

//                            _warehouse_ui_event_hub._invoke_detail_search_item_tags_tag_change(detail_search_item_tags);

//                            refresh_filtering_and_sorting();
//                        });
//                    }
//                }

//                public void _switch_secondary_panel(string _tag)
//                {
//                    foreach (var panel in _secondary_panels)
//                    {
//                        if (panel.gameObject.name.Contains(_tag) == true)
//                        {
//                            panel.gameObject.SetActive(true);
//                            var btns = panel.GetComponentsInChildren<Button>();
//                            foreach (var button in btns)
//                            {
//                                var img = button.GetComponent<Image>();
//                                if (img != null) img.color = _deactive_color;
//                            }
//                        }
//                        else
//                        {
//                            panel.gameObject.SetActive(false);
//                        }
//                    }
//                }

//                public void set_collection_status(string _item_name, bool _flag)
//                {
//                    _inventory._inventory.set_collection_status(_item_name, _flag);

//                    _update_item_view();
//                }

//                public void _update_item_view()
//                {
//                    StartCoroutine(_update_item_view_co());
//                    Log.Info("refresh");
//                }

//                public IEnumerator _update_item_view_co()
//                {
//                    var _update_target_list = _current_resulting_inventory;
//                    _update_target_list.RemoveAll((_i) => { return _i._item_count <= 0; });
//                    if (_game_item_units.Count < _update_target_list.Count)
//                    {
//                        int diff = _update_target_list.Count - _game_item_units.Count;
//                        for (int i = 0; i < diff; i++)
//                        {
//                            var gi_go = Instantiate(Game_Item_Unit_Prefab);
//                            gi_go.transform.SetParent(Game_Item_Unit_Root, false);
//                            _game_item_units.Add(gi_go.GetComponent<UI_Control_Game_Item_Unit>());
//                        }
//                    }
//                    else
//                    {
//                        for (int j = _update_target_list.Count; j < _game_item_units.Count; j++)
//                        {
//                            _game_item_units[j].gameObject.SetActive(false);

//                        }
//                    }

//                    yield return new WaitForSecondsRealtime(0.25f);
//                    for (int k = 0; k < _update_target_list.Count; k++)
//                    {
//                        _game_item_units[k].gameObject.SetActive(true);
//                        _game_item_units[k]._init_game_item_unit(_update_target_list[k], this);
//                    }

//                    yield return new WaitForSecondsRealtime(0.25f);
//                    _warehouse_ui_event_hub._invoke_first_level_search_tag_change_change_in_dispatch();
//                }
//                public void refresh_filtering_and_sorting()
//                {
//                    //_inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory;
//                    //if (first_level_search_tag == "Collectible")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {

//                    //        return _i.is_favorite == true;
//                    //    });

//                    //}

//                    //if (first_level_search_tag == "Pot")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll(filter_pot);

//                    //}
//                    //if (first_level_search_tag == "Seed")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {
//                    //        if(_i == null )
//                    //        {
//                    //            Log.Error("Inventory里有数据为空！" + _i);
//                    //            return false; 
//                    //        }
//                    //        if(_i.item_info == null)
//                    //        {
//                    //            Log.Error($"{_i.item_name}ItemInfo为空！！");
//                    //            return false;
//                    //        }
//                    //        if (_i.item_info.type != ENUM.Item_Type.Seed) return false;
//                    //        if (second_level_search_tag != null && second_level_search_tag.Count != 0)
//                    //        {
//                    //            string _tag = _i.plant_info.plant_Type.ToString();
//                    //            return second_level_search_tag.Contains(_tag);
//                    //        }
//                    //        if (detail_search_item_tags != null && detail_search_item_tags.Count != 0)
//                    //        {
//                    //            bool _type_flag = false;
//                    //            bool _source_tag = false;
//                    //            if (detail_search_item_tags.Contains(_i.plant_info.harvest_type) == true)
//                    //            {
//                    //                _type_flag = true;

//                    //            }
//                    //            if (detail_search_item_tags.Contains(_i.plant_info.seed_source) == true)
//                    //            {
//                    //                _source_tag = true;
//                    //            }
//                    //            return _type_flag && _source_tag;
//                    //        }
//                    //        return true;


//                    //    });

//                    //}
//                    //if (first_level_search_tag == "Fertilizer")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {
//                    //        if (_i?.item_info?.type != ENUM.Item_Type.Fertilizer) return false;
//                    //        if (second_level_search_tag != null && second_level_search_tag.Count != 0)
//                    //        {
//                    //            if (_i._fertilizer_info.is_organic == true)
//                    //            {
//                    //                if (second_level_search_tag.Contains("Organic") == true) return true;
//                    //            }
//                    //            if (_i._fertilizer_info.is_speed_up == true)
//                    //            {
//                    //                if (second_level_search_tag.Contains("Grow") == true) return true;
//                    //            }
//                    //            return false;
//                    //        }
//                    //        return true;


//                    //    });

//                    //}
//                    //if (first_level_search_tag == "Food")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {
//                    //        if (_i.item_info.type != ENUM.Item_Type.Food) return false;
//                    //        return true;
//                    //    });
//                    //}
//                    //if (first_level_search_tag == "Snack")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {
//                    //        if (_i.item_info.type != ENUM.Item_Type.Snack) return false;
//                    //        return true;


//                    //    });

//                    //}
//                    //if (first_level_search_tag == "Tape")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {
//                    //        if (_i.item_info.type != ENUM.Item_Type.Tape) return false;
//                    //        return true;


//                    //    });

//                    //}
//                    //if (first_level_search_tag == "Room_Placement")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {
//                    //        if (_i.item_info.type != ENUM.Item_Type.Room_Placement) return false;
//                    //        if (_i._room_placement_info == null) return false;
//                    //        if (_i._room_placement_info.res_url == null) return false;
//                    //        if (_i._room_placement_info.res_url.Length == 0) return false;
//                    //        return true;


//                    //    });

//                    //    if (second_level_search_tag != null && second_level_search_tag.Count != 0)
//                    //    {
//                    //        _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_resulting_inventory.FindAll((_i) =>
//                    //        {
//                    //            if (_i.item_info.type != ENUM.Item_Type.Room_Placement) return false;
//                    //            if (second_level_search_tag.Contains(_i._room_placement_info.placing_type.ToString()) == true) return true;
//                    //            return false;
//                    //        });
//                    //    }
//                    //}
//                    //if (first_level_search_tag == "Cloth")
//                    //{

//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory.FindAll((_i) =>
//                    //    {
//                    //        if (_i.item_info.type != ENUM.Item_Type.Cloth) return false;
//                    //        if (_i._cloth_info_SO == null) return false;

//                    //        return true;
//                    //    });

//                    //    if (second_level_search_tag != null && second_level_search_tag.Count != 0)
//                    //    {
//                    //        _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_resulting_inventory.FindAll((_i) =>
//                    //        {
//                    //            if (_i.item_info.type != ENUM.Item_Type.Cloth) return false;
//                    //            var _type_en = Kernel.cloth_info.cloth_type_cn_to_en(_i._cloth_info_SO.first_Category);
//                    //            if (second_level_search_tag.Contains(_type_en) == true) return true;
//                    //            return false;
//                    //        });
//                    //    }

//                    //}

//                    //if (_search_input_field.text != null && _search_input_field.text.Length != 0)
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_resulting_inventory.FindAll((_i) =>
//                    //    {

//                    //        return _i.item_name.Contains(_search_input_field.text);
//                    //    });
//                    //}

//                    //if (_show_item_type_list != null && _show_item_type_list.Count != 0)
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_resulting_inventory.FindAll((_i) =>
//                    //    {
//                    //        return _show_item_type_list.Contains(_i.item_info.type);
//                    //    });
//                    //}
//                    //resorting();
//                    //_update_item_view();
//                }

//                public bool filter_pot(Game_Item_In_Inventory _i)
//                {
//                    return true;
//                    //    if (_i?.item_info?.type != ENUM.Item_Type.Pot) return false;
//                    //    if (_i?._pot_info == null) return false;
//                    //    if (second_level_search_tag != null && second_level_search_tag.Count != 0)
//                    //    {
//                    //        string _tag = _i._pot_info.pot_type.ToString();
//                    //        return second_level_search_tag.Contains(_tag);
//                    //    }
//                    //    if (detail_search_item_tags != null && detail_search_item_tags.Count != 0)
//                    //    {
//                    //        var _color_tags= detail_search_item_tags.FindAll((s) => { return _pot_color_tags.Contains(s)==true; });
//                    //        var _source_tags= detail_search_item_tags.FindAll((s) => { return  _pot_color_tags.Contains(s) == false; });

//                    //        bool _color_flag = false;
//                    //        bool _source_tag = false;
//                    //        if (_color_tags.Count != 0)
//                    //        {
//                    //            if (detail_search_item_tags.Contains(_i._pot_info.color) == true)
//                    //            {
//                    //                _color_flag = true;

//                    //            }
//                    //        }else _color_flag = true;

//                    //        if (_source_tags.Count != 0)
//                    //        {
//                    //            if (detail_search_item_tags.Contains(_i._pot_info.obtain_source) == true)
//                    //            {
//                    //                _source_tag = true;
//                    //            }
//                    //        }
//                    //        else _source_tag = true;

//                    //            return _color_flag && _source_tag;
//                    //    }
//                    //    return true;


//                    //}
//                    //public void _invoke_on_close_warehouse_UI()
//                    //{
//                    //    _warehouse_ui_event_hub._invoke_on_close_warehouse_UI();
//                    //    this.gameObject.SetActive(false);
//                    //}
//                    //public void resorting()
//                    //{
//                    //    if (_sort_mode == "获取时间")
//                    //    {
//                    //        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByDate);
//                    //    }
//                    //    if (_sort_mode == "稀有度")
//                    //    {
//                    //        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByRarity);
//                    //    }
//                    //    if (_sort_mode == "售价")
//                    //    {
//                    //        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByPrice);
//                    //    }
//                    //    if (_sort_mode == "持有数量")
//                    //    {
//                    //        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByCount);
//                    //    }
//                    //    //if (_sort_mode == "花盆大小")
//                    //    //{
//                    //    //    _inventory._inventory._current_resulting_inventory.Sort(Game_Item_In_Inventory.comp_func_pot_size);
//                    //    //}
//                }
//                public void toggle_collection(UI_Control_Game_Item_Unit g_i_u)
//                {
//                    if (g_i_u._game_item_in_inventory.is_favorite == true)
//                    {
//                        _inventory._inventory.set_collection_status(g_i_u._game_item_in_inventory.item_name, false);
//                        g_i_u._on_exit_collected();
//                    }
//                    else
//                    {
//                        _inventory._inventory.set_collection_status(g_i_u._game_item_in_inventory.item_name, true);
//                        g_i_u._on_enter_collected();
//                    }

//                    refresh_filtering_and_sorting();
//                }
//                public void toggle_selection(UI_Control_Game_Item_Unit g_i_u)
//                {
//                    Indoor_Room_Game_Manager._activc_instance.placementDic.TryGetValue(g_i_u._game_item_in_inventory.item_id, out var roomData);
//                    if (roomData != null && roomData.relating_rooms != null && roomData.relating_rooms.Count > 0)
//                    {
//                        var currentRoomName = Indoor_Room_Game_Manager._activc_instance._level_info._room_name;
//                        if (!roomData.relating_rooms.Contains(currentRoomName))
//                        {
//                            PromptMessage.Instance.ShowWarningPanel("该家具无法放置在当前房间");
//                            return;
//                        }
//                    }

//                    //if (g_i_u._game_item_in_inventory == _current_selected_game_item) return;
//                    if (g_i_u._game_item_in_inventory == _current_selected_game_item)
//                    {
//                        //DeselectItem();
//                        _warehouse_ui_event_hub._invoke_deselected_item();
//                        if (Indoor_Room_Game_Manager._activc_instance != null)
//                        {
//                            Indoor_Room_Game_Manager._activc_instance._on_exit_add_placement();
//                            Indoor_Room_Game_Manager._activc_instance._general_interaction_event_hub._invoke_on_set_lean_multi_update(true);
//                        }
//                        return;
//                    }

//                    if (_current_selected_game_item != null && _current_selected_item_ui != null)
//                    {
//                        _current_selected_item_ui._on_exit_selected();
//                    }

//                    _current_selected_game_item = g_i_u._game_item_in_inventory;
//                    _current_selected_item_ui = g_i_u;

//                    //EvtDsp.TriggerEvt<string>(EvtNames.On_Select_Item_Change, g_i_u._game_item_in_inventory.item_name);
//                    _warehouse_ui_event_hub._invoke_selected_item_change(g_i_u._game_item_in_inventory.item_name);
//                    g_i_u._on_enter_selected();
//                }
//                public void show_detail(UI_Control_Game_Item_Unit g_i_u)
//                {
//                    detailItem = g_i_u;
//                    Panel_ItemDescription.SetActive(true);
//                    _item_Name.text = g_i_u._game_item_in_inventory.item_name;
//                    _item_Description.text = g_i_u._game_item_in_inventory.item_info.desc;
//                    _sell_price.text = "喵币 " + g_i_u._game_item_in_inventory.item_info.sell_price.ToString();
//                    quantity.text = "库存 " + g_i_u._game_item_in_inventory._item_count.ToString();
//                    //_obtain_date.text = "获取时间 : " + g_i_u._game_item_in_inventory._obtain_date.ToString();
//                    switch (g_i_u._game_item_in_inventory.item_info.rarity)
//                    {
//                        case Enum_RarityType.Common:
//                            normalRarity.SetActive(true);
//                            rareRarity.SetActive(false);
//                            previousRarity.SetActive(false);
//                            break;
//                        case Enum_RarityType.Rare:
//                            normalRarity.SetActive(false);
//                            rareRarity.SetActive(true);
//                            previousRarity.SetActive(false);
//                            break;
//                        case Enum_RarityType.Precious:
//                            normalRarity.SetActive(false);
//                            rareRarity.SetActive(false);
//                            previousRarity.SetActive(true);
//                            break;
//                        default:
//                            normalRarity.SetActive(false);
//                            rareRarity.SetActive(false);
//                            previousRarity.SetActive(false);
//                            break;
//                    }
//                    if (g_i_u._game_item_in_inventory.is_favorite == true)
//                    {
//                        collected.SetActive(true);
//                        uncollected.SetActive(false);
//                    }
//                    else
//                    {
//                        collected.SetActive(false);
//                        uncollected.SetActive(true);
//                    }
//                }


//                public void DeselectItem()
//                {
//                    if (_current_selected_game_item != null && _current_selected_item_ui != null)
//                    {
//                        _current_selected_item_ui._on_exit_selected();
//                        _current_selected_game_item = null;
//                        _current_selected_item_ui = null;
//                    }
//                    Panel_ItemDescription.SetActive(false);
//                    //Indoor_Room_Game_Manager._activc_instance._general_interaction_event_hub._invoke_on_set_lean_multi_update(true);
//                }

//                public void ToggleSelectionCurrentUnit()
//                {
//                    if (_current_selected_item_ui != null)
//                    {
//                        toggle_selection(_current_selected_item_ui);
//                    }
//                }

//                public void ToggleSortModePanel()
//                {
//                    if (normalSortMode.activeSelf || potSortMode.activeSelf)
//                    {
//                        normalSortMode.SetActive(false);
//                        potSortMode.SetActive(false);
//                    }
//                    else
//                    {
//                        if (first_level_search_tag == "Pot")
//                        {
//                            potSortMode.SetActive(true);
//                            normalSortMode.SetActive(false);
//                        }
//                        else
//                        {
//                            potSortMode.SetActive(false);
//                            normalSortMode.SetActive(true);
//                        }
//                    }
//                }

//                public void SetNormalSortMode(string mode)
//                {
//                    foreach (var unit in normalSortMode.GetComponentsInChildren<NormalWarehouseSortModeUnit>(true))
//                    {
//                        if (unit.sortMode == mode)
//                        {
//                            unit.SelectSortMode();
//                            break;
//                        }
//                    }
//                }
//                public void SetPotSortMode(string mode)
//                {
//                    foreach (var unit in potSortMode.GetComponentsInChildren<NormalWarehouseSortModeUnit>(true))
//                    {
//                        if (unit.sortMode == mode)
//                        {
//                            unit.SelectSortMode();
//                            break;
//                        }
//                    }
//                }

//                public void Collect()
//                {
//                    _inventory._inventory.set_collection_status(detailItem._game_item_in_inventory.item_name, true);
//                    detailItem._on_enter_collected();
//                    uncollected.SetActive(false);
//                    collected.SetActive(true);

//                    refresh_filtering_and_sorting();
//                }

//                public void Decollect()
//                {
//                    _inventory._inventory.set_collection_status(detailItem._game_item_in_inventory.item_name, false);
//                    detailItem._on_exit_collected();
//                    collected.SetActive(false);
//                    uncollected.SetActive(true);

//                    refresh_filtering_and_sorting();
//                }

//                public void SelectItemByName(string itemName)
//                {
//                    foreach (var unit in _game_item_units)
//                    {
//                        if (unit._game_item_in_inventory.item_name == itemName)
//                        {
//                            //toggle_selection(unit);
//                            if (_current_selected_game_item != null && _current_selected_item_ui != null)
//                            {
//                                _current_selected_item_ui._on_exit_selected();
//                            }

//                            _current_selected_game_item = unit._game_item_in_inventory;
//                            _current_selected_item_ui = unit;

//                            unit._on_enter_selected();
//                        }
//                    }
//                }
//            }
//        }
//    }
//}