using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

public class ScrollerController_Placement : ScrollerController_GameItem<ScrollData_GameItem, FirstCellView_GameItem>
{
    //本地缓存：保存从全局获取的原始物品数据
    private List<Game_Item_In_Inventory> _placementItemList = new List<Game_Item_In_Inventory>();

    //默认按照品级
    private InventorySortType _currentSortType = InventorySortType.ObtainDate;

    private void OnDestroy()
    {
        _placementItemList.Clear();
    }

    /// <summary>
    /// 载入数据（再开始使用面板之前载入再调用RefreshPanel
    /// ，否则_placementItemList为空不起作用）
    /// </summary>
    public void ReloadData()
    {
        ClearData();

        _placementItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Room_Placement);

        foreach (var item in _placementItemList)
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
    public void RefreshPanel(Placement_First_Category first_Category = Placement_First_Category.None,
        Placement_Second_Category second_Category = Placement_Second_Category.None,
        string filterStr = "", InventorySortType sortType = InventorySortType.Rarity, bool isAscending = false)
    {
        //清理UI数据
        ClearData();

        _currentSortType = sortType;

        //（打开面板时）更新数据（游戏中数据可能会发生变化）
        _placementItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Room_Placement);

        // 1. 基础防护：检查源数据是否存在
        if (_placementItemList == null)
        {
            Log.Custom("ScrollerController_Placement", "源列表 _placementItemList 为空，请先加载数据！", Color.red);
            return;
        }

        var _placementNameDic = GridObjectSystem.InfoNameDic;
        if (_placementNameDic == null)
        {
            Log.Custom("家具字典为空，无法刷新面板", "ScrollerController_Placement", Color.red);
            return;
        }

        // 2. 初始化用于显示的列表 (showData)
        List<Game_Item_In_Inventory> showData = new List<Game_Item_In_Inventory>(_placementItemList);

        // 3. 名称过滤 
        if (!string.IsNullOrEmpty(filterStr))
        {
            showData = showData.SelectByName(filterStr);
        }

        // 4. 一级分类过滤
        if (first_Category != Placement_First_Category.None)
        {
            // 这里直接覆盖 showData，调试时你可以在这里打断点，查看 showData 的 Count
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                if (_placementNameDic.TryGetValue(item.item_info.name, out var meta))
                {
                    return meta.first_Category == first_Category;
                }
                return false;
            }).ToList();
        }

        // 5. 二级分类过滤
        if (second_Category != Placement_Second_Category.None)
        {
            showData = showData.Where(item =>
            {
                if (_placementNameDic.TryGetValue(item.item_info.name, out var meta))
                {
                    return meta.second_Category == second_Category;
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

    public CellView_GameItem GetFirstCell()
    {
        var row = scroller.GetCellViewAtDataIndex(scroller.StartCellViewIndex);
        var cell = (row as FirstCellView_GameItem)?.GetFirestElement();
        return cell;
    }
    public override void ClearData()
    {
        base.ClearData();
        _placementItemList.Clear();
    }

    protected override void BindRow(FirstCellView_GameItem rowCell, int startIndex)
    {
        rowCell.SetData(dataList, startIndex, null, OnItemClick);
    }

    private async void OnItemClick(ScrollData_GameItem data)
    {
        var currentRoom = RoomSystem.currentRoom;

        var placementInfo = GridObjectSystem.GetPlacementInfo(data.id);
        var roomTypes = placementInfo.relating_rooms;
        if (roomTypes.Contains(currentRoom.RoomType))
        {
            //加载prefab
            var placement = await GridObjectSystem.CreatePlacement(data.id);
            //通知编辑Manager进入放置家具模式
            if (placement != null)
            {
                EditManager.Instance.SetMode(new CreateGridObjectMode(placement));
                var placingType = placementInfo.placing_type;

                Enum_Helper.GridLayerMap.TryGetValue(placingType, out var gridLayerTypes);

                if (currentRoom != null && gridLayerTypes != null && gridLayerTypes.Count > 0)
                {
                   var pos = currentRoom.GetRandomPosition(gridLayerTypes[0]);

                    placement.transform.position =pos;
                }
            }

            //Global_Inventory_Manager.Change_Item_Count(data.id, -1);
            EvtDsp.TriggerEvt(EvtNames.ReloadPlacementData);

        }
        else
        {
            //TODO：UI 弹出提示，无法放置在当前房间
            EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, "该家具无法放置在当前房间");
            return;
        }
    }





}