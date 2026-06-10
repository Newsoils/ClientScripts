using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

enum PlacementScrollerMode
{
    Items,
    Groups,
    GroupItems
}

public class ScrollerController_Placement : MonoBehaviour, IEnhancedScrollerDelegate
{
    public EnhancedScroller scroller;
    public FirstCellView_GameItem rowCellPrefab;
    public FirstCellView_Group groupRowCellPrefab;
    public int numberOfCellsPerRow = 5;

    private readonly List<Game_Item_In_Inventory> _placementItemList = new List<Game_Item_In_Inventory>();
    private readonly List<ScrollData_GameItem> _itemDataList = new List<ScrollData_GameItem>();
    private readonly List<ScrollData_ItemGroup> _groupDataList = new List<ScrollData_ItemGroup>();

    private PlacementScrollerMode _mode = PlacementScrollerMode.Items;
    private Placement_First_Category _currentFirstCategory = Placement_First_Category.None;
    private InventorySortType _currentSortType = InventorySortType.ObtainDate;
    private int _selectedGroupId = -1;

    private void Start()
    {
        if (scroller != null)
            scroller.Delegate = this;
    }

    private void OnDestroy()
    {
        ClearData();
    }

    public void ReloadData()
    {
        if (_currentFirstCategory == Placement_First_Category.Set)
        {
            if (_mode == PlacementScrollerMode.GroupItems && _selectedGroupId > 0)
                ShowGroupItems(_selectedGroupId, string.Empty, _currentSortType, false, false);
            else
                ShowGroups(string.Empty, _currentSortType, false);

            return;
        }

        ShowPlacementItems(Placement_First_Category.None, Placement_Second_Category.None, string.Empty, _currentSortType, false, false);
    }

    public void RefreshPanel(Placement_First_Category firstCategory = Placement_First_Category.None,
        Placement_Second_Category secondCategory = Placement_Second_Category.None,
        string filterStr = "", InventorySortType sortType = InventorySortType.Rarity, bool isFavorite = false, bool isAscending = false)
    {
        _currentSortType = sortType;
        _currentFirstCategory = firstCategory;

        if (firstCategory == Placement_First_Category.Set)
        {
            if (_mode == PlacementScrollerMode.GroupItems && _selectedGroupId > 0)
                ShowGroupItems(_selectedGroupId, filterStr, sortType, isFavorite, isAscending);
            else
                ShowGroups(filterStr, sortType, isAscending);

            return;
        }

        _selectedGroupId = -1;
        ShowPlacementItems(firstCategory, secondCategory, filterStr, sortType, isFavorite, isAscending);
    }

    public void BackToGroupList(string filterStr = "", InventorySortType sortType = InventorySortType.Rarity, bool isAscending = false)
    {
        _currentFirstCategory = Placement_First_Category.Set;
        _selectedGroupId = -1;
        ShowGroups(filterStr, sortType, isAscending);
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        var count = _mode == PlacementScrollerMode.Groups ? _groupDataList.Count : _itemDataList.Count;
        if (count == 0) return 0;
        return Mathf.CeilToInt((float)count / numberOfCellsPerRow);
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        var cell = scroller.GetCellViewAtDataIndex(dataIndex);
        if (cell is FirstCellView_GameItem) return 205f;
        else return 268f;
    }
   

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var startIndex = dataIndex * numberOfCellsPerRow;

        if (_mode == PlacementScrollerMode.Groups)
        {
            var groupRow = scroller.GetCellView(groupRowCellPrefab) as FirstCellView_Group;
            groupRow.name = $"Group Row {dataIndex}";
            groupRow.SetData(_groupDataList, startIndex, OnGroupClick);
            return groupRow;
        }

        var itemRow = scroller.GetCellView(rowCellPrefab) as FirstCellView_GameItem;
        itemRow.name = $"Item Row {dataIndex}";
        itemRow.SetData(_itemDataList, startIndex, OnItemClick, null,OnItemClick);
        return itemRow;
    }

    public CellView_GameItem GetFirstCell()
    {
        if (_mode == PlacementScrollerMode.Groups)
            return null;

        var row = scroller.GetCellViewAtDataIndex(scroller.StartCellViewIndex);
        return (row as FirstCellView_GameItem)?.GetFirstElement();
    }

    public void ClearData()
    {
        _itemDataList.Clear();
        _groupDataList.Clear();
        _placementItemList.Clear();
    }

    private void ReloadScroller()
    {
        if (scroller != null)
            scroller.ReloadData();
    }

    private void ShowGroups(string filterStr, InventorySortType sortType, bool isAscending)
    {
        ClearData();
        _mode = PlacementScrollerMode.Groups;

        if (groupRowCellPrefab == null)
        {
            Log.Custom("ScrollerController_Placement", "groupRowCellPrefab is not assigned.", Color.red);
            ReloadScroller();
            return;
        }

        var groups = Global_Inventory_Manager.GetObtainedGroupsByType(GroupType.FurnitureSuit);
        if (!string.IsNullOrEmpty(filterStr))
            groups = groups.Where(group => group?.itemGroupInfo != null && group.itemGroupInfo.group_name.Contains(filterStr)).ToList();

        SortGroups(groups, sortType, isAscending);

        foreach (var group in groups)
        {
            if (group == null) continue;
            _groupDataList.Add(new ScrollData_ItemGroup(group));
        }

        ReloadScroller();
    }

    private void ShowGroupItems(int groupId, string filterStr, InventorySortType sortType, bool isFavorite, bool isAscending)
    {
        ClearData();
        _mode = PlacementScrollerMode.GroupItems;
        _selectedGroupId = groupId;

        var showData = Global_Inventory_Manager.GetGroupObtainedItems(groupId) ?? new List<Game_Item_In_Inventory>();
        FillItemData(showData, filterStr, sortType, isFavorite, isAscending);
    }

    private void ShowPlacementItems(Placement_First_Category firstCategory, Placement_Second_Category secondCategory,
        string filterStr, InventorySortType sortType, bool isFavorite, bool isAscending)
    {
        ClearData();
        _mode = PlacementScrollerMode.Items;

        var placementItems = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Room_Placement);
        if (placementItems == null)
        {
            Log.Custom("ScrollerController_Placement", "源列表 _placementItemList 为空，请先加载数据！", Color.red);
            ReloadScroller();
            return;
        }

        _placementItemList.AddRange(placementItems);

        var placementNameDic = GridObjectSystem.InfoNameDic;
        if (placementNameDic == null)
        {
            Log.Custom("家具字典为空，无法刷新面板", "ScrollerController_Placement", Color.red);
            ReloadScroller();
            return;
        }

        List<Game_Item_In_Inventory> showData = new List<Game_Item_In_Inventory>(_placementItemList);

        if (firstCategory != Placement_First_Category.None)
        {
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                if (placementNameDic.TryGetValue(item.item_info.name, out var meta))
                    return meta.first_Category == firstCategory;

                return false;
            }).ToList();
        }

        if (secondCategory != Placement_Second_Category.None)
        {
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                if (placementNameDic.TryGetValue(item.item_info.name, out var meta))
                    return meta.second_Category == secondCategory;

                return false;
            }).ToList();
        }

        FillItemData(showData, filterStr, sortType, isFavorite, isAscending);
    }

    private void FillItemData(List<Game_Item_In_Inventory> showData, string filterStr,
        InventorySortType sortType, bool isFavorite, bool isAscending)
    {
        if (!string.IsNullOrEmpty(filterStr))
            showData = showData.SelectByName(filterStr);

        if (isFavorite)
            showData = showData.Where(i => i.is_favorite).ToList();

        showData.SortByType(sortType, isAscending);

        foreach (var item in showData)
        {
            if (item?.item_info == null) continue;
            _itemDataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity,
                item.uid,
                Global_Inventory_Manager.IsNewObtainItem(item.uid)));
        }

        ReloadScroller();
    }

    private static void SortGroups(List<ItemGroupData> groups, InventorySortType sortType, bool isAscending)
    {
        groups.Sort((a, b) =>
        {
            int result;
            switch (sortType)
            {
                case InventorySortType.Count:
                    result = b.obtainCount.CompareTo(a.obtainCount);
                    break;
                default:
                    result = string.Compare(a.itemGroupInfo?.group_name, b.itemGroupInfo?.group_name, System.StringComparison.Ordinal);
                    break;
            }

            if (result == 0)
                result = a.group_id.CompareTo(b.group_id);

            return isAscending ? -result : result;
        });
    }

    private void OnGroupClick(ScrollData_ItemGroup data)
    {
        if (data == null) return;
        ShowGroupItems(data.groupId, string.Empty, _currentSortType, false, false);
    }

    private async void OnItemClick(ScrollData_GameItem data)
    {
        if (data == null) return;

        var currentRoom = RoomSystem.currentRoom;
        var placementInfo = GridObjectSystem.GetPlacementInfo(data.id);
        if (placementInfo == null || currentRoom == null)
            return;

        var roomTypes = placementInfo.relating_rooms;
        if (roomTypes.Contains(currentRoom.RoomType))
        {
            var placement = await GridObjectSystem.CreatePlacement(data.id);
            if (placement != null)
            {
                EditManager.Instance.SetMode(new CreateGridObjectMode(placement));
                var placingType = placementInfo.placing_type;

                Enum_Helper.GridLayerMap.TryGetValue(placingType, out var gridLayerTypes);

                if (gridLayerTypes != null && gridLayerTypes.Count > 0)
                {
                    var pos = currentRoom.GetRandomPosition(gridLayerTypes[0]);
                    placement.transform.position = pos;
                }
            }

            EvtDsp.TriggerEvt(EvtNames.ReloadPlacementData);
        }
        else
        {
            PromptManager.ShowWarning(PromptId.FurnitureWrongRoom);
        }
    }
}
