using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

public class ScrollerController_Plant : ScrollerController_GameItem<ScrollData_GameItem, FirstCellView_GameItem>
{
    //本地缓存：保存从全局获取的原始物品数据
    private List<Game_Item_In_Inventory> _potItemList = new List<Game_Item_In_Inventory>();
    private List<Game_Item_In_Inventory> _plantItemList = new List<Game_Item_In_Inventory>();
    private List<Game_Item_In_Inventory> _fertilizerItemList = new List<Game_Item_In_Inventory>();

    //默认按照品级
    private InventorySortType _currentSortType = InventorySortType.ObtainDate;

    private void OnDestroy()
    {
        _potItemList.Clear();
    }

    /// <summary>
    /// 载入数据（再开始使用面板之前载入再调用RefreshPanel
    /// ，否则_placementItemList为空不起作用）
    /// </summary>
    public void ReloadData()
    {
        ClearData();

        _potItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Pot);
        _plantItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Seed);
        _fertilizerItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Fertilizer);

        foreach (var item in _potItemList)
        {
            dataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity,
                item.uid,
                Global_Inventory_Manager.IsNewObtainItem(item.uid)));
        }

        ReloadScroller();
    }

    /// <summary>
    /// 根据给定的排序刷新面板
    /// </summary>
    /// <param name="first_category"></param>
    /// <param name="second_category"></param>
    /// <param name="sortType"></param>
    /// <param name="isAscending"></param>
    public void RefreshPanel(Plant_First_Category first_category = Plant_First_Category.None,
        Plant_Second_Category second_category = Plant_Second_Category.None,
        string filterStr = "", InventorySortType sortType = InventorySortType.Rarity, bool isFavorite = false, bool isAscending = false,
        PlantType seedPlantTypeFilter = PlantType.None, ObtainSource seedObtainSourceFilter = ObtainSource.Unknown)
    {
        //清理UI数据
        ClearData();
        _currentSortType = sortType;

        //（打开面板时）更新数据（游戏中数据可能会发生变化）
        _potItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Pot);
        _plantItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Seed);
        _fertilizerItemList = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Fertilizer);

        // 1. 基础防护：检查源数据是否存在
        if (_potItemList == null)
        {
            Log.Custom("ScrollerController_Placement", "源列表 _placementItemList 为空，请先加载数据！", Color.red);
            return;
        }

        var _plantNameDic = PlantManager.Instance.plantSeedNameDic;
        var _potNameDic = PlantManager.Instance.potDatas;
        var _fertilizerNameDic = PlantManager.Instance.fertilizerDatas;
        if (_plantNameDic == null || _potNameDic == null || _fertilizerNameDic == null)
        {
            Log.Custom("字典为空，无法刷新面板", "ScrollerController_Placement", Color.red);
            return;
        }

        // 2. 初始化用于显示的列表 (showData)
        List<Game_Item_In_Inventory> showData = new List<Game_Item_In_Inventory>(_potItemList);
        showData.AddRange(new List<Game_Item_In_Inventory>(_plantItemList));
        showData.AddRange(new List<Game_Item_In_Inventory>(_fertilizerItemList));

        // 3. 名称过滤 
        if (!string.IsNullOrEmpty(filterStr))
        {
            showData = showData.SelectByName(filterStr);
        }

        // 4. 一级分类过滤
        if (first_category != Plant_First_Category.None)
        {
            // 这里直接覆盖 showData，调试时你可以在这里打断点，查看 showData 的 Count
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                if (_plantNameDic.TryGetValue(item.item_info.name, out var meta))
                {
                    return first_category == Plant_First_Category.Seed;
                }
                if(_fertilizerNameDic.TryGetValue(item.item_info.name, out var data))
                {
                    return first_category == Plant_First_Category.Fertilizer;
                }
                if(_potNameDic.TryGetValue(item.item_info.name, out var gata))
                {
                    return first_category == Plant_First_Category.Pot;
                }
                return false;
            }).ToList();
        }

        // 5. 二级分类过滤
        if (second_category != Plant_Second_Category.None)
        {
            showData = showData.Where(item =>
            {
                if (_plantNameDic.TryGetValue(item.item_info.name, out var meta))
                {
                    return meta.plantType == second_category;
                }
                if (_fertilizerNameDic.TryGetValue(item.item_info.name, out var data))
                {
                    return data.category == second_category;
                }
                if (_potNameDic.TryGetValue(item.item_info.name, out var gata))
                {
                    return gata.category == second_category;
                }
                return false;
            }).ToList();
        }

        bool hasSeedPlantTypeFilter = seedPlantTypeFilter != PlantType.None;
        bool hasSeedObtainSourceFilter = seedObtainSourceFilter != ObtainSource.Unknown;
        if (hasSeedPlantTypeFilter || hasSeedObtainSourceFilter)
        {
            var seedInfoDic = PlantManager.Instance.seedInfoDic;
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                if (!_plantNameDic.ContainsKey(item.item_info.name)) return false;

                if (hasSeedObtainSourceFilter && item.item_info.obtain_source != seedObtainSourceFilter)
                {
                    return false;
                }

                if (hasSeedPlantTypeFilter)
                {
                    if (seedInfoDic == null || !seedInfoDic.TryGetValue(item.item_info.item_id, out var seedInfo) || seedInfo == null)
                    {
                        return false;
                    }

                    return seedInfo.plantType == seedPlantTypeFilter;
                }

                return true;
            }).ToList();
        }

        // 6. 如果面板要求只显示收藏项，则过滤
        if (isFavorite)
        {
            showData = showData.Where(item => item.is_favorite).ToList();
        }

        // 7. 排序（此时 showData 已经是过滤后的结果）
        showData.SortByType(sortType, isAscending);


        // 8. 填充显示数据
        foreach (var item in showData)
        {
            dataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity,
                item.uid,
                Global_Inventory_Manager.IsNewObtainItem(item.uid)));
        }

        ReloadScroller();
    }


    public override void ClearData()
    {
        base.ClearData();
        _plantItemList.Clear();
        _potItemList.Clear();
        _fertilizerItemList.Clear();
    }

    protected override void BindRow(FirstCellView_GameItem rowCell, int startIndex)
    {
        rowCell.SetData(dataList, startIndex, OnItemClick);
    }

    private async void OnItemClick(ScrollData_GameItem data)
    {
        if(_potItemList.Find(x=>x.item_name == data.name)!=null)
        {
            var potData = PlantManager.Instance.potDatas[data.name];
            //加载prefab
            var pot = await GridObjectSystem.CreatePot(data.name);
            //通知编辑Manager进入放置家具模式
            if (pot != null)
            {
                var currentRoom = RoomSystem.currentRoom;
                EditManager.Instance.SetMode(new CreateGridObjectMode( pot));
                var placingType = potData.category == Plant_Second_Category.HangingPot ? Room_Placing_Type.Ceiling_Furniture : Room_Placing_Type.Surface_Furniture;

                Enum_Helper.GridLayerMap.TryGetValue(placingType, out var gridLayerTypes);

                if (currentRoom != null && gridLayerTypes != null && gridLayerTypes.Count > 0)
                {
                    var pos = currentRoom.GetRandomPosition(gridLayerTypes[0]);

                    pot.transform.position = pos;
                }
            }
            EvtDsp.TriggerEvt(EvtNames.ReloadPlantData);
            return;
        }
        if(_fertilizerItemList.Find(x => x.item_name == data.name) != null)
        {
            var fertItem = Global_Inventory_Manager.GetItem(data.name);
            int fertCount = fertItem != null ? fertItem._item_count : 0;
            if (fertCount <= 0)
            {
                PromptManager.ShowUpPrompt(PromptId.FertilizerNotEnough);
                return;
            }
            EditManager.Instance.SetMode(new FertilizePlantMode(data.name));
        }
        if(_plantItemList.Find(x => x.item_name == data.name) != null)
        {
            var seedItem = Global_Inventory_Manager.GetItem(data.name);
            int seedCount = seedItem != null ? seedItem._item_count : 0;
            if (seedCount <= 0)
            {
                PromptManager.ShowUpPrompt(PromptId.SeedNotEnough);
                return;
            }
            EditManager.Instance.SetMode(new PlantPlantMode(data.name));
        }
    }
    
}
