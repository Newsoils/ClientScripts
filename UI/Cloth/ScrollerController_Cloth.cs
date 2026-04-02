using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

public class ScrollerController_Cloth : ScrollerController_GameItem<ScrollData_GameItem, FirstCellView_GameItem>
{
    //本地缓存：保存从全局获取的原始物品数据
    private List<Game_Item_In_Inventory> _clothItemList = new List<Game_Item_In_Inventory>();

    //默认按照品级
    private InventorySortType _currentSortType = InventorySortType.ObtainDate;

    private void OnDestroy()
    {
        _clothItemList.Clear();
    }

    /// <summary>
    /// 载入数据（再开始使用面板之前载入再调用RefreshPanel
    /// ，否则_placementItemList为空不起作用）
    /// </summary>
    public void ReloadData()
    {
        ClearData();

        _clothItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Cloth);

        foreach (var item in _clothItemList)
        {
            dataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity));
        }

        ReloadScroller();
    }

    /// <summary>
    /// 根据给定的排序刷新面板
    /// </summary>
    /// <param name="first_Category"></param>
    /// <param name="second_Category"></param>
    /// <param name="sortType"></param>
    /// <param name="isAscending"></param>
    public void RefreshPanel(Cloth_First_Category first_Category = Cloth_First_Category.None,
        Cloth_Second_Category second_Category = Cloth_Second_Category.None,
        string filterStr = "", InventorySortType sortType = InventorySortType.Rarity, bool isAscending = false)
    {
        //清理UI数据
        ClearData();

        _currentSortType = sortType;

        //（打开面板时）更新数据（游戏中数据可能会发生变化）
        _clothItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Cloth);

        // 1. 基础防护：检查源数据是否存在
        if (_clothItemList == null)
        {
            Log.Custom("ScrollerController_cloth", "源列表 _clothItemList 为空，请先加载数据！", Color.red);
            return;
        }

        var _clothNameDic = Character_Cloth_Manager.Instance.clothNameDic;
        if (_clothNameDic == null)
        {
            Log.Custom("衣服字典为空，无法刷新面板", "ScrollerController_cloth", Color.red);
            return;
        }

        // 2. 初始化用于显示的列表 (showData)
        List<Game_Item_In_Inventory> showData = new List<Game_Item_In_Inventory>(_clothItemList);

        // 3. 名称过滤 
        if (!string.IsNullOrEmpty(filterStr))
        {
            showData = showData.SelectByName(filterStr);
        }

        // 4. 一级分类过滤
        if (first_Category != Cloth_First_Category.None)
        {
            // 这里直接覆盖 showData，调试时你可以在这里打断点，查看 showData 的 Count
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                if (_clothNameDic.TryGetValue(item.item_info.name, out var meta))
                {
                    return meta.first_category == first_Category;
                }
                return false;
            }).ToList();
        }

        // 5. 二级分类过滤
        if (second_Category != Cloth_Second_Category.None)
        {
            showData = showData.Where(item =>
            {
                if (_clothNameDic.TryGetValue(item.item_info.name, out var meta))
                {
                    return meta.second_category == second_Category;
                }
                return false;
            }).ToList();
        }

        // 6. 排序（此时 showData 已经是过滤后的结果）
        showData.SortByType(sortType, isAscending);


        // 7. 填充显示数据
        foreach (var item in showData)
        {
            dataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity));
        }

        ReloadScroller();
    }


    public override void ClearData()
    {
        base.ClearData();
        _clothItemList.Clear();
    }

    protected override void BindRow(FirstCellView_GameItem rowCell, int startIndex)
    {
        rowCell.SetData(dataList, startIndex, OnItemClick);
    }

    private void OnItemClick(ScrollData_GameItem data)
    {
        Character_Cloth_Manager.Instance.Add_Cloth(Character_Type.Target_Character, data.name);
    }

}
