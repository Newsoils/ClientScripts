//using System;
//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Framework_Core.Event;
//using CLIP.Framework_Core.LYC.TaskSystem;
//using CLIP.Framework_Unity;
//using CLIP.Project_Mouse.Client_Event_Systems;
//using CLIP.Project_Mouse.ENUM;
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
//            public class FurnitureWarehouseInteractManager : MonoBehaviour
//            {
//                public Game_Play_System.GameItem_DB_SO _inventory;
//                public List<Item_Type> _show_item_type_list;
//                public RectTransform _first_level_root;
//                public List<Button> _first_buttons = new List<Button>();
//                public List<RectTransform> _secondary_panels = new List<RectTransform>();
//                public List<RectTransform> _secondary_buttons_root = new List<RectTransform>();
//                public TMP_Dropdown _sort_mode_dd;
//                public GameObject normalSortMode;
//                public Button _btn_close_all;
//                public string _sort_mode = "获取时间";
//                public NormalWarehouseSortModeUnit currentSortMode;


//                public GameObject Game_Item_Unit_Prefab;
//                public RectTransform Game_Item_Unit_Root;
//                public List<FurnitureWarehouseItemUnit> _game_item_units = new List<FurnitureWarehouseItemUnit>();

//                [HideInInspector]
//                public Game_Item_In_Inventory _current_selected_game_item = null;
//                [HideInInspector]
//                public FurnitureWarehouseItemUnit _current_selected_item_ui = null;

//                public string first_level_search_tag = "";
//                public List<string> second_level_search_tag = new List<string>();
//                public List<string> detail_search_item_tags = new List<string>();

//                private Dictionary<string, string> _secondLevelTagToCategory = new Dictionary<string, string>
//                {
//                    { "Table", "桌子" },
//                    { "Chair", "椅子" },
//                    { "Bed", "床" },
//                    { "Cabinet", "柜子" },
//                    { "Sofa", "沙发" },
//                    { "Light", "灯" },
//                    { "Pots", "盆栽" },
//                    { "Amusement", "娱乐设施" },
//                    { "Electricity", "电器" },
//                    { "Bath", "卫浴" },
//                    { "Carpet", "地毯" },
//                    { "HangPhoto", "挂画" },
//                    { "Shelf", "架子" },
//                    { "Floor", "地板" },
//                    { "Wall", "墙纸" },
//                    { "Door", "门" },
//                    { "Decoration", "装饰物" },
//                };

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


//                [Header("Event_Systems")]
//                public Warehouse_UI_Event_Hub_SO _warehouse_ui_event_hub;
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

//                    if (_btn_close_all != null) _btn_close_all.onClick.AddListener(_invoke_on_close_warehouse_UI);
//                }

//                public void OnEnable()
//                {

//                    first_level_search_tag = "Floor_Furniture";
//                    init_game_items();

//                    if (_warehouse_ui_event_hub != null)
//                    {
//                        _warehouse_ui_event_hub._on_refresh_warehouse_UI.AddListener(refresh_filtering_and_sorting);
//                        //_warehouse_ui_event_hub._toggle_selection_current_unit.AddListener(ToggleSelectionCurrentUnit);
//                    }
//                }


//                public void OnDisable()
//                {
//                    _current_selected_game_item = null;
//                    _current_selected_item_ui = null;

//                    first_level_search_tag = "";
//                    second_level_search_tag.Clear();
//                    detail_search_item_tags.Clear();
//                    reset_button();
//                    if (_warehouse_ui_event_hub != null)
//                    {
//                        _warehouse_ui_event_hub._on_refresh_warehouse_UI.RemoveListener(refresh_filtering_and_sorting);
//                        //_warehouse_ui_event_hub._toggle_selection_current_unit.AddListener(ToggleSelectionCurrentUnit);
//                    }
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
//                    _inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory;
//                    _game_item_units = new List<FurnitureWarehouseItemUnit>(Game_Item_Unit_Root.gameObject.GetComponentsInChildren<FurnitureWarehouseItemUnit>());
//                    refresh_filtering_and_sorting();
//                    _update_item_view();
//                }

//                public void reset_button()
//                {
//                    foreach (var _sp in _secondary_buttons_root)
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

//                            //_current_selected_game_item = null;
//                            //_current_selected_item_ui = null;
//                            //_selected_items.Clear();
//                            //_selected_item_units.Clear();

//                            second_level_search_tag.Clear();
//                            detail_search_item_tags.Clear();
//                            _switch_secondary_panel(_tag);

//                            if (Panel_Search.activeSelf == true)
//                            {
//                                on_close_search_panel();
//                                on_open_search_panel();
//                            }
//                            UI_Helper.set_dropdown(_sort_mode_dd, "获取时间");
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
//                    foreach (var _rt in _secondary_buttons_root)
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
//                        var _name = _btn.gameObject.name;
//                        var _tag = _name.Substring(4, _btn.gameObject.name.Length - 4);
//                        _btn.onClick.AddListener(() =>
//                        {
//                            //_current_selected_game_item = null;
//                            //_current_selected_item_ui = null;
//                            //_selected_items.Clear();
//                            //_selected_item_units.Clear();

//                            if (second_level_search_tag.Contains(_tag))
//                            {
//                                second_level_search_tag.Remove(_tag);
//                                _image.color = _deactive_color;
//                            }
//                            else
//                            {
//                                second_level_search_tag.Add(_tag);
//                                _image.color = _active_color;
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
//                    var _btns = new List<Button>(_root.GetComponentsInChildren<Button>());
//                    foreach (var button in _btns)
//                    {
//                        var _btn = button;
//                        var _image = button.GetComponent<Image>();
//                        _image.color = _deactive_color;
//                        var _name = _btn.gameObject.name;
//                        var _tag = _name.Substring(4, _btn.gameObject.name.Length - 4);
//                        _btn.onClick.AddListener(() =>
//                        {
//                            if (detail_search_item_tags.Contains(_tag))
//                            {
//                                detail_search_item_tags.Remove(_tag);
//                                _image.color = _deactive_color;
//                            }
//                            else
//                            {
//                                detail_search_item_tags.Add(_tag);
//                                _image.color = _active_color;
//                            }

//                            //_current_selected_game_item = null;
//                            //_current_selected_item_ui = null;
//                            //_selected_items.Clear();
//                            //_selected_item_units.Clear();

//                            _warehouse_ui_event_hub._invoke_detail_search_item_tags_tag_change(detail_search_item_tags);

//                            refresh_filtering_and_sorting();


//                        });
//                    }
//                }

//                public void _switch_secondary_panel(string _tag)
//                {
//                    foreach (var panel in _secondary_panels)
//                    {
//                        if (panel.gameObject.name.Contains(_tag))
//                        {
//                            panel.gameObject.SetActive(true);
//                        }
//                        else
//                        {
//                            panel.gameObject.SetActive(false);
//                        }
//                    }

//                    foreach (var btnRoot in _secondary_buttons_root)
//                    {
//                        btnRoot.gameObject.SetActive(true);
//                        var btns = btnRoot.GetComponentsInChildren<Button>();
//                        foreach (var button in btns)
//                        {
//                            var img = button.GetComponent<Image>();
//                            if (img != null) img.color = _deactive_color;
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
//                    var _update_target_list = _inventory._inventory._current_resulting_inventory;

//                    //foreach (var item in _update_target_list)
//                    //{
//                    //    //Log.Info( $"Item in DB: {item.name}");
//                    //    if (item.item_info.desc == "粉色套装床")
//                    //    {
//                    //        Log.Custom($"Found item: {item.item_info.name}", "itemFounded", Color.yellow);
//                    //    }
//                    //}
//                    _update_target_list.RemoveAll((_i) => { return _i._item_count <= 0; });
//                    if (_game_item_units.Count < _update_target_list.Count)
//                    {
//                        int diff = _update_target_list.Count - _game_item_units.Count;
//                        for (int i = 0; i < diff; i++)
//                        {
//                            var gi_go = Instantiate(Game_Item_Unit_Prefab);
//                            gi_go.transform.SetParent(Game_Item_Unit_Root, false);
//                            _game_item_units.Add(gi_go.GetComponent<FurnitureWarehouseItemUnit>());
//                        }
//                    }
//                    else
//                    {
//                        for (int j = _update_target_list.Count; j < _game_item_units.Count; j++)
//                        {
//                            _game_item_units[j].gameObject.SetActive(false);
//                        }
//                    }
//                    for (int k = 0; k < _update_target_list.Count; k++)
//                    {
//                        if (_game_item_units[k] == null) continue;
//                        if (_game_item_units[k].gameObject == null) continue;
//                        _game_item_units[k].gameObject.SetActive(true);
//                        _game_item_units[k]._init_game_item_unit(_update_target_list[k], this);
//                    }

//                    StartCoroutine(DelayRebuildLayout());
//                }

//                private IEnumerator DelayRebuildLayout()
//                {
//                    yield return null;
//                    LayoutRebuilder.ForceRebuildLayoutImmediate(Game_Item_Unit_Root);
//                }

//                /// <summary>
//                /// 刷新物品筛选、排序与显示逻辑。
//                /// </summary>
//                public void refresh_filtering_and_sorting()
//                {
//                    // 安全检查：外层 inventory 是否为空
//                    var invRoot = _inventory?._inventory;
//                    if (invRoot == null || invRoot._current_inventory == null)
//                    {
//                        Debug.LogWarning("[Inventory] refresh_filtering_and_sorting(): 无有效的库存数据。");
//                        return;
//                    }

//                    // 初始化：从当前背包拷贝一份作为工作集合
//                    List<Game_Item_In_Inventory> source = invRoot._current_inventory;
//                    List<Game_Item_In_Inventory> result = new List<Game_Item_In_Inventory>(source);

//                    //var item = source.Find(i => i.item_info.desc == "粉色套装床");
//                    //if(item == null)
//                    //{
//                    //    Log.Custom("No 粉色套装床 found in current inventory", "itemNotFound", Color.red);
//                    //}
//                    //else
//                    //{
//                    //    Log.Sucess("Found 粉色套装床 in current inventory"+ item.item_info.type);
//                    //}

//                    // ========== 一级筛选：根据 first_level_search_tag 分类 ==========
//                    switch (first_level_search_tag)
//                    {
//                        case "Collectible":
//                            // 筛选可收藏物品
//                            result = result.FindAll(_i => _i != null && _i.is_favorite);
//                            break;

//                        case "Floor_Furniture":
//                            result = FilterByPlacement(result, Room_Placing_Type.Floor_Furniture);
//                            break;

//                        case "Wall_Furniture":
//                            result = FilterByPlacement(result, Room_Placing_Type.Wall_Furniture);
//                            break;

//                        case "Floor_Finishes":
//                            result = FilterByPlacement(result, Room_Placing_Type.Floor_Finishes);
//                            break;

//                        case "Wall_Finishes":
//                            result = FilterByPlacement(result, Room_Placing_Type.Wall_Finishes);
//                            break;

//                        case "Ceiling_Furniture":
//                            result = FilterByPlacement(result, Room_Placing_Type.Ceiling_Furniture);
//                            break;
//                        default:
//                            // 无匹配分类则不过滤
//                            break;
//                    }

//                    // ========== 搜索过滤：根据输入框文本 ==========
//                    if (!string.IsNullOrEmpty(_search_input_field?.text))
//                    {
//                        string keyword = _search_input_field.text;
//                        result = result.FindAll(_i =>
//                            _i != null &&
//                            !string.IsNullOrEmpty(_i.item_name) &&
//                            _i.item_name.Contains(keyword)
//                        );
//                    }

//                    // ========== 限定物品类型过滤（例如只显示某几种 item_type）==========
//                    if (_show_item_type_list != null && _show_item_type_list.Count > 0)
//                    {
//                        result = result.FindAll(_i =>
//                            _i != null &&
//                            _i.item_info != null &&
//                            _show_item_type_list.Contains(_i.item_info.type)
//                        );
//                    }

//                    // ========== 结果写回并更新显示 ==========
//                    invRoot._current_resulting_inventory = result;

//                    resorting();        // 排序
//                    _update_item_view(); // 刷新 UI
//                }


//                /// <summary>
//                /// 根据 Room_Placing_Type（地面家具、墙面饰品等）进行过滤。
//                /// 内部自动处理空引用与二级标签过滤。
//                /// </summary>
//                private List<Game_Item_In_Inventory> FilterByPlacement(List<Game_Item_In_Inventory> source, Room_Placing_Type targetType)
//                {
//                    if (source == null)
//                        return new List<Game_Item_In_Inventory>();

//                    return source.FindAll(_i =>
//                    {
//                        // 1️⃣ 空引用保护
//                        if (_i == null) return false;
//                        if (_i.item_info == null) return false;

//                        //if(_i.item_info.desc == "粉色套装床")
//                        //{
//                        //    Log.Custom("Filtering 粉色套装床", "filteringItem", Color.yellow);
//                        //}

//                        // 2️⃣ 检查房间摆件信息
//                        Indoor_Room_Game_Manager._activc_instance.placementDic.TryGetValue(_i.item_id, out var placementInfo);
//                        if (placementInfo == null) return false;
//                        if (placementInfo.placing_type != targetType) return false;

//                        // 3️⃣ 二级筛选标签过滤
//                        if (second_level_search_tag != null && second_level_search_tag.Count > 0 && _secondLevelTagToCategory != null)
//                        {
//                            bool match = false;
//                            foreach (var tag in second_level_search_tag)
//                            {
//                                if (string.IsNullOrEmpty(tag)) continue;
//                                if (_secondLevelTagToCategory.TryGetValue(tag, out string cnCategory))
//                                {
//                                    if (placementInfo.first_Category == cnCategory)
//                                    {
//                                        match = true;
//                                        break;
//                                    }
//                                }
//                            }
//                            if (!match) return false;
//                        }

//                        return true;
//                    });
//                }


//                public void _invoke_on_close_warehouse_UI()
//                {
//                    _warehouse_ui_event_hub._invoke_on_close_warehouse_UI();
//                    this.gameObject.SetActive(false);
//                }
//                public void resorting()
//                {
//                    if (_sort_mode == "获取时间")
//                    {
//                        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByDate);
//                    }
//                    if (_sort_mode == "稀有度")
//                    {
//                        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByRarity);
//                    }
//                    if (_sort_mode == "售价")
//                    {
//                        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByPrice);
//                    }
//                    if (_sort_mode == "持有数量")
//                    {
//                        _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByCount);
//                    }
//                    //if (_sort_mode == "花盆大小")
//                    //{
//                    //    _inventory._inventory._current_resulting_inventory.Sort(Game_Item_In_Inventory.comp_func_pot_size);
//                    //}
//                }
//                public void toggle_collection(FurnitureWarehouseItemUnit g_i_u)
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
//                public void toggle_selection(FurnitureWarehouseItemUnit g_i_u)
//                {
//                    if (g_i_u._game_item_in_inventory == _current_selected_game_item)
//                    {
//                        DeselectItem();
//                        return;
//                    }

//                    if (_current_selected_game_item != null && _current_selected_item_ui != null)
//                    {
//                        _current_selected_item_ui._on_exit_selected();
//                    }
//                    _current_selected_game_item = g_i_u._game_item_in_inventory;
//                    _current_selected_item_ui = g_i_u;

//                    //if (_selected_items.Contains(g_i_u._game_item_in_inventory))
//                    //{
//                    //    DeselectItem(g_i_u._game_item_in_inventory.item_name);
//                    //    return;
//                    //}

//                    //_selected_items.Add(g_i_u._game_item_in_inventory);
//                    //_selected_item_units.Add(g_i_u);


//                    //EvtDsp.TriggerEvt<string>(EvtNames.On_Select_Item_Change, g_i_u._game_item_in_inventory.item_name);
//                    _warehouse_ui_event_hub._invoke_selected_item_change(g_i_u._game_item_in_inventory.item_name);
//                    g_i_u._on_enter_selected();
//                    _update_item_view();

//                }


//                public void DeselectItem()
//                {
//                    _warehouse_ui_event_hub._invoke_deselected_item();

//                    if (_current_selected_game_item != null && _current_selected_item_ui != null)
//                    {
//                        _current_selected_item_ui._on_exit_selected();
//                        _current_selected_game_item = null;
//                        _current_selected_item_ui = null;
//                    }

//                    //_selected_items.RemoveAll(item => item.item_name == itemName);
//                    //for (int i = _selected_item_units.Count - 1; i >= 0; i--)
//                    //{
//                    //    var unit = _selected_item_units[i];
//                    //    if (unit._game_item_in_inventory.item_name == itemName)
//                    //    {
//                    //        unit._on_exit_selected();
//                    //        _selected_item_units.RemoveAt(i);
//                    //        break;
//                    //    }
//                    //}
//                }

//                public void ToggleSelectionCurrentUnit()
//                {
//                    //if (_current_selected_item_ui != null)
//                    //{
//                    //    toggle_selection(_current_selected_item_ui);
//                    //}
//                }

//                public void ToggleSortModePanel()
//                {
//                    if (normalSortMode.activeSelf)
//                    {
//                        normalSortMode.SetActive(false);
//                    }
//                    else
//                    {
//                        normalSortMode.SetActive(true);
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
//            }
//        }
//    }
//}