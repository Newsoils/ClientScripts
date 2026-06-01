//using CLIP.Project_Mouse.ENUM;
//using CLIP.Project_Mouse.Game_Play_System;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class NpcChatGiftUnit : MonoBehaviour
//            {
//                public Kernel.Game_Item_In_Inventory _game_item_in_inventory;
//                public Item_Type type;

//                public NpcChatInteractManager _parent_control;

//                public Image Image_Icon;

//                public TMP_Text Item_Name;
//                public TMP_Text Item_Count;

//                public Image selectBg;

//                public void InitGiftUnit(Kernel.Game_Item_In_Inventory game_item_in_inventory, NpcChatInteractManager parent_control)
//                {
//                    _game_item_in_inventory = game_item_in_inventory;
//                    _parent_control = parent_control;
//                    Item_Name.text = game_item_in_inventory.item_name;
//                    Item_Count.text = game_item_in_inventory._item_count.ToString();

//                    _on_exit_selected();

//                    if (game_item_in_inventory.item_info.res_url != null && game_item_in_inventory.item_info.res_url.Length != 0)
//                    {
//                        var _image_url_data = game_item_in_inventory.item_info.res_url.Split("#");
//                        if (_image_url_data.Length != 2) return;
//                        PM_RM.load_sub_sprite(_image_url_data[0], _image_url_data[1], _set_sprite);
//                    }

//                    if (_parent_control != null )
//                    {
//                        var inventoryList = Global_Inventory_Manager.GameItem_DB;
//                        foreach (var item in inventoryList)
//                        {
//                            if (item.name == game_item_in_inventory.item_name)
//                            {
//                                type = item.type;
//                                break;
//                            }
//                        }
//                    }
//                }

//                public void ShowDetail()
//                {
//                    _parent_control.ShowGiftDetail(this);
//                }
//                public void toggle_selection()
//                {
//                    _parent_control.SelectGift(this);
//                }

//                public void _set_sprite(Sprite sp)
//                {
//                    Image_Icon.sprite = sp;
//                }

//                public void _on_enter_selected()
//                {
//                    Color c = selectBg.color;
//                    c.a = 1f;
//                    selectBg.color = c;
//                }

//                public void _on_exit_selected()
//                {
//                    Color c = selectBg.color;
//                    c.a = 0f;
//                    selectBg.color = c;
//                }
//            }
//        }
//    }
//}