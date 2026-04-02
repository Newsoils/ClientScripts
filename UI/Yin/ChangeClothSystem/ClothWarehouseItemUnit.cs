using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ClothWarehouseItemUnit : MonoBehaviour
            {
                public Kernel.Game_Item_In_Inventory _game_item_in_inventory;

                public ClothWarehouseInteractManager _parent_control;

                public Image Image_Icon;

                public TMP_Text Item_Name;
                public TMP_Text Item_Count;

                public Button BTN_Show_Detail;


                [Header("For_Collection")]
                public Image _BG_collection;
                public Button BTN_Favorite;
                public TMP_Text _collection_text;
                public Color _color_collected;
                public Color _color_not_collected;
                [Header("For_Selection")]
                public Button _btn_all;
                public Image BG_all_image;
                public Color _color_selected;
                public Color _color_no_selected;
                public Image selectBg;

                public void _init_game_item_unit(
                    Kernel.Game_Item_In_Inventory game_item_in_inventory,
                    ClothWarehouseInteractManager parent_control,
                    bool _enable_show_detail = true)
                {
                    _game_item_in_inventory = game_item_in_inventory;
                    _parent_control = parent_control;
                    Item_Name.text = game_item_in_inventory.item_name;
                    Item_Count.text = game_item_in_inventory._item_count.ToString();

                    BTN_Show_Detail.onClick.RemoveAllListeners();
                    BTN_Favorite.onClick.RemoveAllListeners();
                    _btn_all.onClick.RemoveAllListeners();

                    if (game_item_in_inventory.is_favorite == true)
                    {
                        _on_enter_collected();
                    }
                    else
                    {
                        _on_exit_collected();
                    }
                    _on_exit_selected();

                    //if (parent_control._current_selected_item_ui == this)
                    //{
                    //    _on_enter_selected();
                    //}
                    if (parent_control._selected_item_units.Contains(this))
                    {
                        _on_enter_selected();
                    }

                    if (_enable_show_detail == true)
                    {
                        BTN_Show_Detail.gameObject.SetActive(true);

                        BTN_Show_Detail.onClick.AddListener(()=> ItemDescriptionPanel.Instance.OpenPanel(game_item_in_inventory));
                    }
                    else
                    {
                        BTN_Show_Detail.gameObject.SetActive(false);
                    }


                    BTN_Favorite.onClick.AddListener(toggle_collection);

                    _btn_all.onClick.AddListener(toggle_selection);
                    if (game_item_in_inventory.item_info.res_url != null && game_item_in_inventory.item_info.res_url.Length != 0)
                    {
                        var _image_url_data = game_item_in_inventory.item_info.res_url.Split("#");
                        if (_image_url_data.Length == 2)
                        {
                            PM_RM.load_sub_sprite(_image_url_data[0], _image_url_data[1], _set_sprite);
                            return;
                        }
                        else 
                        {
                            PM_RM.load_sprite_async(_image_url_data[0], _set_sprite);
                        }
                    }


                }

                public void _show_detial()
                {
                    _parent_control.show_detail(this);
                }
                public void toggle_selection()
                {
                    _parent_control.toggle_selection(this);
                }
                public void toggle_collection()
                {
                    _parent_control.toggle_collection(this);
                }

                public void _set_sprite(Sprite sp)
                {
                    Image_Icon.sprite = sp;
                }
                public void _on_enter_collected()
                {
                    _BG_collection.color = _color_collected;
                    _collection_text.text = "取消\n收藏";
                }
                public void _on_exit_collected()
                {
                    _BG_collection.color = _color_not_collected;
                    _collection_text.text = "收藏";
                }

                public void _on_enter_selected()
                {
                    //BG_all_image.color = _color_selected;
                    Color c = selectBg.color;
                    c.a = 1f;
                    selectBg.color = c;
                }
                public void _on_exit_selected()
                {
                    //BG_all_image.color = _color_no_selected;
                    Color c = selectBg.color;
                    c.a = 0f;
                    selectBg.color = c;
                }
            }
        }
    }
}

