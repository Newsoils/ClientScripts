using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Serialization;
using CLIP.Project_Mouse.Kernel.Inventory;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel {


            [System.Serializable]
            public class shop_list_db 
            {
                public List<shop_list> _shop_list;
                public List<item_group> _item_group;
                public static Action<int, int, string> checkPay;
                public void load_json(
                     string _shop_list_json_file,   
                     string _item_group_json_file
                    )
                {
                    if(_shop_list_json_file!=null&& _shop_list_json_file.Length!=0) _shop_list= Serialization_Provider.DeserializeObject<List<shop_list>>(_shop_list_json_file);
                    if (_item_group_json_file != null && _item_group_json_file.Length != 0) _item_group = Serialization_Provider.DeserializeObject<List<item_group>>(_item_group_json_file);
                }

                public void get_daily_shop_list(Cloth_DB cloth_DB, out List<shop_item> output_item_name_list)
                {
                    output_item_name_list=new List<shop_item>();
                    var _seed_list= _shop_list.Find(x => x.shop_list_name=="种子列表");
                    foreach (var item in GetRandom4Item(_seed_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }
                    var _pot_list = _shop_list.Find(x => x.shop_list_name == "花盆列表");
                    foreach (var item in GetRandom4Item(_pot_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }
                    var _fertilizer_list = _shop_list.Find(x => x.shop_list_name == "肥料列表");
                    foreach (var item in GetRandom4Item(_fertilizer_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }
                    var _carried_item_list = _shop_list.Find(x => x.shop_list_name == "便当列表");
                    foreach (var item in GetRandom4Item(_carried_item_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }
                    var _luck_item_list = _shop_list.Find(x => x.shop_list_name == "幸运小物列表");
                    foreach (var item in GetRandom4Item(_luck_item_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }
                    var _CD_item_list = _shop_list.Find(x => x.shop_list_name == "CD列表");
                    foreach (var item in GetRandom4Item(_CD_item_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }
                    var _cloth_item_list = _shop_list.Find(x => x.shop_list_name == "服装商品列表_001");
                    foreach (var item in GetRandom4Item(_cloth_item_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }
                    var _placement_item_list = _shop_list.Find(x => x.shop_list_name == "家具商品列表_001");
                    foreach (var item in GetRandom4Item(_placement_item_list.single_shop_item_list))
                    {
                        output_item_name_list.Add(item);
                    }

                    /*
                    //var _cloth_selected_index = GetKDistinctRandomInts(_cloth_item_list.single_shop_item_list.Count, 4);
                    var _placement_selected_index = GetKDistinctRandomInts(_placement_item_list.single_shop_item_list.Count, 4);


                    //foreach (var index in _cloth_selected_index)
                    //{
                    //    output_item_name_list.Add(_cloth_item_list.single_shop_item_list[index]);
                    //}
                    foreach (var index in _placement_selected_index)
                    {
                        output_item_name_list.Add(_placement_item_list.single_shop_item_list[index]);
                    }
                    */
                    // 随机衣服
                    /*
                    var parentClothGroups = new Dictionary<string, List<shop_item>>();
                    foreach (var shopItem in _cloth_item_list.single_shop_item_list)
                    {
                        var clothInfo = cloth_DB._all_cloth_list.Find(c => c.cloth_name == shopItem.item_name);
                        if (clothInfo == null || string.IsNullOrEmpty(clothInfo.parent_cloth_name))
                            continue;
                        if (!parentClothGroups.ContainsKey(clothInfo.parent_cloth_name))
                            parentClothGroups[clothInfo.parent_cloth_name] = new List<shop_item>();
                        parentClothGroups[clothInfo.parent_cloth_name].Add(shopItem);
                    }

                    var singleShopItemList = new List<shop_item>();
                    var rand = new System.Random((int)DateTime.Now.Ticks);
                    foreach (var kv in parentClothGroups)
                    {
                        var group = kv.Value;
                        var selected = group[rand.Next(group.Count)];
                        singleShopItemList.Add(selected);
                    }

                    if (singleShopItemList.Count >= 1)
                    {
                        int clothCount = Math.Min(4, singleShopItemList.Count);
                        var randomIndexes = GetKDistinctRandomInts(singleShopItemList.Count, clothCount);
                        foreach (var idx in randomIndexes)
                        {
                            output_item_name_list.Add(singleShopItemList[idx]);
                        }
                    }
                    */
                }
                private List<shop_item> GetRandom4Item(List<shop_item> items)
                {
                    int count = Math.Min(items.Count, 4);
                    Random random = new Random();
                    return items.OrderBy(x => random.Next()).Take(count).ToList();
                }

                public void get_limited_shop_list(out List<item_group> output_cloth_item_list,out List<item_group> output_placement_item_list)
                {
                    var clothgroup1 = _shop_list.Find(x => x.shop_list_name == "套装服装商品列表_001").group_shop_item_list[0];
                    var clothgroup2 = _shop_list.Find(x => x.shop_list_name == "套装服装商品列表_001").group_shop_item_list[1];
                    var furniture1 = _shop_list.Find(x => x.shop_list_name == "套组家具商品列表_001").group_shop_item_list[0];
                    var furniture2 = _shop_list.Find(x => x.shop_list_name == "套组家具商品列表_001").group_shop_item_list[1];
                    output_cloth_item_list = new List<item_group> { _item_group.Find(x => x.item_group_name == clothgroup1), _item_group.Find(x => x.item_group_name == clothgroup2) };
                    output_placement_item_list = new List<item_group> { _item_group.Find(x => x.item_group_name == furniture1), _item_group.Find(x => x.item_group_name == furniture2) };
                    
                    /*
                    // ========== 随机服装 ==========
                    var original_color_cloth_groups = new List<item_group>();
                    foreach (var x in _item_group)
                    {
                        if (x.group_type == "服装套装" && x.shop_item_list_color == "原色")
                        {
                            original_color_cloth_groups.Add(x);
                        }
                    }

                    var styleToClothGroup = new Dictionary<string, item_group>();
                    foreach (var group in original_color_cloth_groups)
                    {
                        if (!styleToClothGroup.ContainsKey(group.style))
                        {
                            styleToClothGroup[group.style] = group;
                        }
                    }

                    var distinctStyleClothGroups = new List<item_group>(styleToClothGroup.Values);

                    if (distinctStyleClothGroups.Count == 0)
                    {
                        Log.Error("服装没有套装！");
                        output_cloth_item_list = new List<item_group>();
                    }
                    else
                    {
                        int clothSelectCount = Math.Min(2, distinctStyleClothGroups.Count);
                        var clothSelectedIndexes =  GetKDistinctRandomInts(distinctStyleClothGroups.Count, clothSelectCount);
                        output_cloth_item_list = new List<item_group>();

                        foreach (var idx in clothSelectedIndexes)
                        {
                            output_cloth_item_list.Add(distinctStyleClothGroups[idx]);
                        }
                    }

                    // ========== 随机家具 ==========
                    var original_color_groups = new List<item_group>();
                    foreach (var x in _item_group)
                    {
                        if (x.group_type == "家具套装" && x.shop_item_list_color == "原色")
                        {
                            original_color_groups.Add(x);
                        }
                    }

                    var styleToGroup = new Dictionary<string, item_group>();
                    foreach (var group in original_color_groups)
                    {
                        if (!styleToGroup.ContainsKey(group.style))
                        {
                            styleToGroup[group.style] = group;
                        }
                    }

                    var distinctStyleGroups = new List<item_group>(styleToGroup.Values);

                    if (distinctStyleGroups.Count == 0)
                    {
                        Log.Error("家具没有套装！");
                        output_placement_item_list = new List<item_group>();
                    }
                    else
                    {
                        int selectCount = Math.Min(2, distinctStyleGroups.Count);
                        var selectedIndexes = GetKDistinctRandomInts(distinctStyleGroups.Count, selectCount);

                        output_placement_item_list = new List<item_group>();
                        foreach (var idx in selectedIndexes)
                        {
                            output_placement_item_list.Add(distinctStyleGroups[idx]);
                        }
                    }
                    */
                }

                

                

            }
        }
  
    }
}