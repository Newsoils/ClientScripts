using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.Client_Event_Systems;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ClothWarehouseInteractManager : MonoBehaviour
            {
                public GameItem_DB_SO _inventory;
                public List<Item_Type> _show_item_type_list;
                public RectTransform _first_level_root;
                public List<Button> _first_buttons = new List<Button>();
                public List<RectTransform> _secondary_panels = new List<RectTransform>();
                public TMP_Dropdown _sort_mode_dd;
                public GameObject normalSortMode;
                public Button _btn_close_all;
                public string _sort_mode = "获取时间";
                public ClothWarehouseSortModeUnit currentSortMode;


                public GameObject Game_Item_Unit_Prefab;
                public RectTransform Game_Item_Unit_Root;
                public List<ClothWarehouseItemUnit> _game_item_units = new List<ClothWarehouseItemUnit>();

                //public Kernel.Game_Item_In_Inventory _current_selected_game_item = null;
                //public ClothWarehouseItemUnit _current_selected_item_ui = null;

                public List<Game_Item_In_Inventory> _selected_items = new List<Game_Item_In_Inventory>();
                public List<ClothWarehouseItemUnit> _selected_item_units = new List<ClothWarehouseItemUnit>();

                public string first_level_search_tag = "";
                public List<string> second_level_search_tag = new List<string>();
                public List<string> detail_search_item_tags = new List<string>();

                private Dictionary<string, string> _secondLevelTagToCategory = new Dictionary<string, string>
                {
                    { "TShirt", "T恤" },
                    { "Gallus", "吊带" },
                    { "Jacket", "外套" },
                    { "Shorts", "短裤" },
                    { "Pants", "长裤" },
                    { "Dress", "连衣裙" },
                    { "Skirt", "半裙" },
                    { "Sneaker", "运动鞋" },
                    { "Sandal", "凉鞋" },
                    { "Boot", "靴子" },
                    { "Socks", "袜子" },
                    { "Bag", "背包" },
                    { "Headwear", "头饰" },
                    { "Special", "特殊" },
                };

                [Header("For_Additional_Search")]
                public GameObject Panel_Search;
                public Button _btn_open_search_panel;
                public Button _btn_close_search_panel;
                public TMP_InputField _search_input_field;
                public Button _btn_comfirm_search_button;
                public GameObject Panel_Filter_Pot;
                public GameObject Panel_Filter_Seed;

                public List<RectTransform> _panel_detail_filter;

                public Color _active_color;
                public Color _deactive_color;

                [Header("For_Item_Description")]
                public GameObject Panel_ItemDescription;
                public TMP_Text _item_Name;
                public TMP_Text _item_Description;
                public TMP_Text _sell_price;
                public TMP_Text _obtain_date;
                public TMP_Text quantity;
                public GameObject normalRarity;
                public GameObject rareRarity;
                public GameObject previousRarity;
                public GameObject collected;
                public GameObject uncollected;
                public ClothWarehouseItemUnit detailItem;
                private string filterString = "";
                private float debounceTime = 0.25f;
                Coroutine currentCoroutine;

                [Header("Event_Systems")]
                public Change_Clothes_Warehouse_Event_Hub_SO _warehouse_ui_event_hub;
                void Start()
                {
                    _binding_first_level_buttons();
                    _binding_secondary_level_button();
                    _binding_detail_level_button();
                    if (_sort_mode_dd != null) _sort_mode_dd.onValueChanged.AddListener(_sort_mode_change);
                    init_game_items();

                    _btn_open_search_panel.onClick.AddListener(on_open_search_panel);
                    _btn_close_search_panel.onClick.AddListener(on_close_search_panel);
                    _btn_comfirm_search_button.onClick.AddListener(on_confirm_search);



                    _search_input_field.onValueChanged.AddListener(value =>
                    {
                        filterString = value;

                        if (currentCoroutine != null)
                        {
                            StopCoroutine(currentCoroutine);
                        }
                        currentCoroutine = StartCoroutine(DebouncedValidate(filterString));
                    });


                    if (_btn_close_all != null) _btn_close_all.onClick.AddListener(_invoke_on_close_warehouse_UI);


                    if (_warehouse_ui_event_hub != null)
                    {
                        _warehouse_ui_event_hub._on_refresh_warehouse_UI.AddListener(refresh_filtering_and_sorting);
                        //_warehouse_ui_event_hub._toggle_selection_current_unit.AddListener(ToggleSelectionCurrentUnit);
                    }
                }


                IEnumerator DebouncedValidate(string text)
                {
                    yield return new WaitForSeconds(debounceTime);
                    Debug.Log("DebouncedValidate");
                    // 执行实际校验逻辑
                    filterString = SensitiveWordManager.Instance.FilterText(filterString);
                    _search_input_field.text = filterString;



                    Debug.Log($"Validating: {text}");
                }


                private static readonly Dictionary<string, string> FirstLevelTagToCategory =
                    new Dictionary<string, string>
                    {
                        { "Upper", "上衣" },
                        { "Lower", "裤子" },
                        { "Dress", "裙子" },
                        { "Buttom", "鞋袜" },
                        { "Attachment", "配饰" }
                    };


                public void OnEnable()
                {
                    first_level_search_tag = "Upper";
                    init_game_items();
                }

                public void _sort_mode_change(int value)
                {
                    _sort_mode = _sort_mode_dd.options[value].text;
                    Debug.Log("Search Mode Changed: " + _sort_mode);

                    _warehouse_ui_event_hub._invoke_sort_mode_change_change(_sort_mode);

                    refresh_filtering_and_sorting();
                }

                public void init_game_items()
                {
                    //_inventory._inventory._current_resulting_inventory = _inventory._inventory._current_inventory;
                    _game_item_units = new List<ClothWarehouseItemUnit>(Game_Item_Unit_Root.gameObject.GetComponentsInChildren<ClothWarehouseItemUnit>());
                    refresh_filtering_and_sorting();
                    _update_item_view();
                }
                public void refresh_game_items()
                {

                }

                public void OnDisable()
                {
                    //_current_selected_game_item = null;
                    //_current_selected_item_ui = null;
                    _selected_items.Clear();
                    _selected_item_units.Clear();

                    first_level_search_tag = "";
                    second_level_search_tag.Clear();
                    detail_search_item_tags.Clear();
                    reset_button();
                }
                public void reset_button()
                {
                    foreach (var _sp in _secondary_panels)
                    {
                        var _btn = _sp.GetComponentsInChildren<Button>();
                        foreach (var b in _btn)
                        {
                            var img = b.GetComponent<Image>();
                            if (img != null) img.color = _deactive_color;
                        }
                    }

                }
                public void on_open_search_panel()
                {
                    Panel_Search.SetActive(true);
                    _search_input_field.text = null;
                    if (first_level_search_tag == "Pot")
                    {
                        Panel_Filter_Pot.SetActive(true);
                        Panel_Filter_Seed.SetActive(false);
                    }
                    else if (first_level_search_tag == "Seed")
                    {
                        Panel_Filter_Pot.SetActive(false);
                        Panel_Filter_Seed.SetActive(true);
                    }
                    else
                    {
                        Panel_Filter_Pot.SetActive(false);
                        Panel_Filter_Seed.SetActive(false);
                    }
                }
                public void on_close_search_panel()
                {
                    _search_input_field.text = null;
                    Panel_Search.SetActive(false);

                    detail_search_item_tags.Clear();
                    refresh_filtering_and_sorting();
                    //_update_item_view();
                }

                public void _binding_first_level_buttons()
                {
                    _first_buttons = new List<Button>(_first_level_root.GetComponentsInChildren<Button>());
                    foreach (var button in _first_buttons)
                    {
                        var _name = button.gameObject.name;
                        var _tag = _name.Substring(4, button.gameObject.name.Length - 4);
                        button.onClick.AddListener(() =>
                        {

                            first_level_search_tag = _tag;

                            //_current_selected_game_item = null;
                            //_current_selected_item_ui = null;
                            //_selected_items.Clear();
                            //_selected_item_units.Clear();

                            second_level_search_tag.Clear();
                            detail_search_item_tags.Clear();
                            _switch_secondary_panel(_tag);

                            if (Panel_Search.activeSelf == true)
                            {
                                on_close_search_panel();
                                on_open_search_panel();
                            }
                            UI_Helper.set_dropdown(_sort_mode_dd, "获取时间");
                            _warehouse_ui_event_hub._invoke_first_level_search_tag_change_change(first_level_search_tag);
                            refresh_filtering_and_sorting();
                        }

                       // _update_item_view();

                       );
                    }
                }
                public void on_confirm_search()
                {
                    refresh_filtering_and_sorting();
                }
                public void _binding_secondary_level_button()
                {
                    foreach (var _rt in _secondary_panels)
                    {
                        _binding_secondary_level_button_from_root(_rt);
                    }
                }

                public void _binding_secondary_level_button_from_root(RectTransform _root)
                {
                    var _btns = new List<Button>(_root.GetComponentsInChildren<Button>());
                    foreach (var button in _btns)
                    {
                        var _btn = button;
                        var _image = button.GetComponent<Image>();
                        var _name = _btn.gameObject.name;
                        var _tag = _name.Substring(4, _btn.gameObject.name.Length - 4);
                        _btn.onClick.AddListener(() =>
                        {
                            //_current_selected_game_item = null;
                            //_current_selected_item_ui = null;
                            //_selected_items.Clear();
                            //_selected_item_units.Clear();

                            if (second_level_search_tag.Contains(_tag))
                            {
                                second_level_search_tag.Remove(_tag);
                                _image.color = _deactive_color;
                            }
                            else
                            {
                                second_level_search_tag.Add(_tag);
                                _image.color = _active_color;
                            }
                            _warehouse_ui_event_hub._invoke_second_level_search_tag_change(second_level_search_tag);
                            refresh_filtering_and_sorting();

                            // _update_item_view();
                        });
                    }
                }

                public void _binding_detail_level_button()
                {
                    foreach (var _rt in _panel_detail_filter)
                    {
                        _binding_detail_level_button_from_root(_rt);
                    }
                }

                public void _binding_detail_level_button_from_root(RectTransform _root)
                {
                    var _btns = new List<Button>(_root.GetComponentsInChildren<Button>());
                    foreach (var button in _btns)
                    {
                        var _btn = button;
                        var _image = button.GetComponent<Image>();
                        _image.color = _deactive_color;
                        var _name = _btn.gameObject.name;
                        var _tag = _name.Substring(4, _btn.gameObject.name.Length - 4);
                        _btn.onClick.AddListener(() =>
                        {
                            if (detail_search_item_tags.Contains(_tag))
                            {
                                detail_search_item_tags.Remove(_tag);
                                _image.color = _deactive_color;
                            }
                            else
                            {
                                detail_search_item_tags.Add(_tag);
                                _image.color = _active_color;
                            }

                            //_current_selected_game_item = null;
                            //_current_selected_item_ui = null;
                            //_selected_items.Clear();
                            //_selected_item_units.Clear();

                            _warehouse_ui_event_hub._invoke_detail_search_item_tags_tag_change(detail_search_item_tags);

                            refresh_filtering_and_sorting();

                        });
                    }
                }

                public void _switch_secondary_panel(string _tag)
                {
                    foreach (var panel in _secondary_panels)
                    {
                        if (panel.gameObject.name.Contains(_tag) == true)
                        {
                            panel.gameObject.SetActive(true);
                            var btns = panel.GetComponentsInChildren<Button>();
                            foreach (var button in btns)
                            {
                                var img = button.GetComponent<Image>();
                                if (img != null) img.color = _deactive_color;
                            }
                        }
                        else
                        {
                            panel.gameObject.SetActive(false);
                        }
                    }
                }

                public void set_collection_status(string _item_name, bool _flag)
                {
                    //Global_Inventory_Manager.set_collection_status(_item_name, _flag);

                    _update_item_view();
                }

                public void _update_item_view()
                {
                    ////var _update_target_list = _inventory._inventory._current_resulting_inventory;
                    //_update_target_list.RemoveAll((_i) => { return _i._item_count <= 0; });
                    //if (_game_item_units.Count < _update_target_list.Count)
                    //{
                    //    int diff = _update_target_list.Count - _game_item_units.Count;
                    //    for (int i = 0; i < diff; i++)
                    //    {
                    //        var gi_go = Instantiate(Game_Item_Unit_Prefab);
                    //        gi_go.transform.SetParent(Game_Item_Unit_Root, false);
                    //        _game_item_units.Add(gi_go.GetComponent<ClothWarehouseItemUnit>());
                    //    }
                    //}
                    //else
                    //{
                    //    for (int j = _update_target_list.Count; j < _game_item_units.Count; j++)
                    //    {
                    //        _game_item_units[j].gameObject.SetActive(false);

                    //    }
                    //}
                    //for (int k = 0; k < _update_target_list.Count; k++)
                    //{
                    //    _game_item_units[k].gameObject.SetActive(true);
                    //    _game_item_units[k]._init_game_item_unit(_update_target_list[k], this);
                    //    var unit = _game_item_units[k];

                    //    bool isWearing = Character_Cloth_Manager.Instance.targetCloth.IsWearingCloth(unit._game_item_in_inventory.item_name);
                        
                    //    if (isWearing)
                    //    {
                    //        unit._on_enter_selected();
                    //        if (!_selected_item_units.Contains(unit))
                    //            _selected_item_units.Add(unit);
                    //        if (!_selected_items.Contains(unit._game_item_in_inventory))
                    //            _selected_items.Add(unit._game_item_in_inventory);
                    //    }
                    //    else
                    //    {
                    //        unit._on_exit_selected();
                    //        _selected_item_units.Remove(unit);
                    //        _selected_items.Remove(unit._game_item_in_inventory);
                    //    }
                    //}
                }

              

                public void refresh_filtering_and_sorting()
                {
                    //var inventory = _inventory._inventory;
                    //IEnumerable<Game_Item_In_Inventory> result = inventory._current_inventory;

                    // Collectible
                    //if (first_level_search_tag == "Collectible")
                    //{
                    //    result = result.Where(i => i.is_favorite);
                    //}
                    //// Cloth 分类
                    //else if (FirstLevelTagToCategory.TryGetValue(first_level_search_tag, out var firstCategory))
                    //{
                    //    result = result.Where(i => MatchClothCategory(i, firstCategory));
                    //}

                    //result = result.Where(i => !i.item_name.Contains("默认"));

                    //// 文本搜索
                    //if (!string.IsNullOrEmpty(_search_input_field.text))
                    //{
                    //    result = result.Where(i => i.item_name.Contains(_search_input_field.text));
                    //}

                    //// Item Type 过滤
                    //if (_show_item_type_list != null && _show_item_type_list.Count > 0)
                    //{
                    //    result = result.Where(i => _show_item_type_list.Contains(i.item_info.type));
                    //}

                    //inventory._current_resulting_inventory = result.ToList();

                    resorting();
                    _update_item_view();
                }


                //private bool MatchClothCategory(Game_Item_In_Inventory _i, string firstCategory)
                //{
                //    if (_i?.item_info?.type != Project_Mouse.ENUM.Item_Type.Cloth)
                //        return false;

                //    var clothInfo = Character_Cloth_Manager.Instance.clothInfo_db.Find(c=>c.cloth_name == _i.item_name);
                //    if (clothInfo == null)
                //        return false;

                //    if (clothInfo.first_category != firstCategory)
                //        return false;

                //    if (second_level_search_tag == null || second_level_search_tag.Count == 0)
                //        return true;

                //    foreach (var tag in second_level_search_tag)
                //    {
                //        if (_secondLevelTagToCategory.TryGetValue(tag, out var cnCategory) &&
                //            clothInfo.second_category == cnCategory)
                //        {
                //            return true;
                //        }
                //    }

                //    return false;
                //}


                public void _invoke_on_close_warehouse_UI()
                {
                    _warehouse_ui_event_hub._invoke_on_close_warehouse_UI();
                    this.gameObject.SetActive(false);
                }
                public void resorting()
                {
                    //if (_sort_mode == "获取时间")
                    //{
                    //    _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByDate);
                    //}
                    //if (_sort_mode == "稀有度")
                    //{
                    //    _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByRarity);
                    //}
                    //if (_sort_mode == "售价")
                    //{
                    //    _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByPrice);
                    //}
                    //if (_sort_mode == "持有数量")
                    //{
                    //    _inventory._inventory._current_resulting_inventory.Sort(InventorySorter.CompareByCount);
                    //}
                    //if (_sort_mode == "花盆大小")
                    //{
                    //    _inventory._inventory._current_resulting_inventory.Sort(Game_Item_In_Inventory.comp_func_pot_size);
                    //}
                }
                public void toggle_collection(ClothWarehouseItemUnit g_i_u)
                {
                    //if (g_i_u._game_item_in_inventory.is_favorite == true)
                    //{
                    //    _inventory._inventory.set_collection_status(g_i_u._game_item_in_inventory.item_name, false);
                    //    g_i_u._on_exit_collected();
                    //}
                    //else
                    //{
                    //    _inventory._inventory.set_collection_status(g_i_u._game_item_in_inventory.item_name, true);
                    //    g_i_u._on_enter_collected();
                    //}

                    refresh_filtering_and_sorting();
                }
                public void toggle_selection(ClothWarehouseItemUnit g_i_u)
                {
                    if (_selected_items.Contains(g_i_u._game_item_in_inventory))
                    {
                        DeselectItem(g_i_u._game_item_in_inventory.item_name);
                        return;
                    }

                    _selected_items.Add(g_i_u._game_item_in_inventory);
                    _selected_item_units.Add(g_i_u);

                    _warehouse_ui_event_hub._invoke_selected_item_change(g_i_u._game_item_in_inventory.item_name);
                    g_i_u._on_enter_selected();
                    _update_item_view();
                }
                public void show_detail(ClothWarehouseItemUnit g_i_u)
                {
                    detailItem = g_i_u;
                    Panel_ItemDescription.SetActive(true);
                    _item_Name.text = g_i_u._game_item_in_inventory.item_name;
                    _item_Description.text = g_i_u._game_item_in_inventory.item_info.desc;
                    _sell_price.text = "喵币 " + g_i_u._game_item_in_inventory.item_info.sell_price.ToString();
                    quantity.text = "库存 " + g_i_u._game_item_in_inventory._item_count.ToString();
                    //_obtain_date.text = "获取时间 : " + g_i_u._game_item_in_inventory._obtain_date.ToString();
                    switch (g_i_u._game_item_in_inventory.item_info.rarity)
                    {
                        case Enum_RarityType.Common:
                            normalRarity.SetActive(true);
                            rareRarity.SetActive(false);
                            previousRarity.SetActive(false);
                            break;
                        case Enum_RarityType.Rare:
                            normalRarity.SetActive(false);
                            rareRarity.SetActive(true);
                            previousRarity.SetActive(false);
                            break;
                        case Enum_RarityType.Precious:
                            normalRarity.SetActive(false);
                            rareRarity.SetActive(false);
                            previousRarity.SetActive(true);
                            break;
                        default:
                            normalRarity.SetActive(false);
                            rareRarity.SetActive(false);
                            previousRarity.SetActive(false);
                            break;
                    }
                    if (g_i_u._game_item_in_inventory.is_favorite == true)
                    {
                        collected.SetActive(true);
                        uncollected.SetActive(false);
                    }
                    else
                    {
                        collected.SetActive(false);
                        uncollected.SetActive(true);
                    }
                }


                public void DeselectItem(string itemName)
                {
                    _warehouse_ui_event_hub._invoke_deselected_item(itemName);

                    //if (_current_selected_game_item != null && _current_selected_item_ui != null)
                    //{
                    //    _current_selected_item_ui._on_exit_selected();
                    //    _current_selected_game_item = null;
                    //    _current_selected_item_ui = null;
                    //}

                    _selected_items.RemoveAll(item => item.item_name == itemName);
                    for (int i = _selected_item_units.Count - 1; i >= 0; i--)
                    {
                        var unit = _selected_item_units[i];
                        if (unit._game_item_in_inventory.item_name == itemName)
                        {
                            unit._on_exit_selected();
                            _selected_item_units.RemoveAt(i);
                            break;
                        }
                    }

                    Panel_ItemDescription.SetActive(false);
                }

                public void ToggleSelectionCurrentUnit()
                {
                    //if (_current_selected_item_ui != null)
                    //{
                    //    toggle_selection(_current_selected_item_ui);
                    //}
                }

                public void ToggleSortModePanel()
                {
                    if (normalSortMode.activeSelf)
                    {
                        normalSortMode.SetActive(false);
                    }
                    else
                    {
                        normalSortMode.SetActive(true);
                    }
                }

                public void SetNormalSortMode(string mode)
                {
                    foreach (var unit in normalSortMode.GetComponentsInChildren<ClothWarehouseSortModeUnit>(true))
                    {
                        if (unit.sortMode == mode)
                        {
                            unit.SelectSortMode();
                            break;
                        }
                    }
                }

                public void Collect()
                {
                    //_inventory._inventory.set_collection_status(detailItem._game_item_in_inventory.item_name, true);
                    detailItem._on_enter_collected();
                    uncollected.SetActive(false);
                    collected.SetActive(true);

                    refresh_filtering_and_sorting();
                }

                public void Decollect()
                {
                    //_inventory._inventory.set_collection_status(detailItem._game_item_in_inventory.item_name, false);
                    detailItem._on_exit_collected();
                    collected.SetActive(false);
                    uncollected.SetActive(true);

                    refresh_filtering_and_sorting();
                }
            }
        }
    }
}

