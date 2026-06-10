using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

/// <summary>
/// 回收箱物品列表滚动控制器。物品来源为仓库中 <see cref="Item_Type.Harvest"/>，
/// 以及表类型为 <see cref="Item_Type.Food_Material"/> 但在 <see cref="PlantData.harvestItem"/> 中出现过的收获物
///（例：草莓/蓝莓果实在 game_item 里为食材类，与盆栽收获物一并在此出售）。
/// 二级分类通过 <see cref="PlantManager.plantSeedNameDic"/> 反查 <see cref="PlantData.plantType"/>
/// 并映射到 <see cref="Recycle_Second_Category"/>。
/// </summary>
public class ScrollerController_Recycle : ScrollerController_GameItem<ScrollData_GameItem, FirstCellView_GameItem>
{
    private List<Game_Item_In_Inventory> _harvestItemList = new List<Game_Item_In_Inventory>();

    private InventorySortType _currentSortType = InventorySortType.ObtainDate;

    /// <summary>回调回 <see cref="RecyclePanel"/>，Inspector 必挂。</summary>
    public RecyclePanel recyclePanel;

    private void OnDestroy()
    {
        _harvestItemList.Clear();
    }

    /// <summary>打开面板时调用，预加载收获相关物品并刷 scroller。</summary>
    public void ReloadData()
    {
        ClearData();

        var plantSeedDic = PlantManager.Instance != null ? PlantManager.Instance.plantSeedNameDic : null;
        _harvestItemList = BuildRecycleItemList(plantSeedDic);

        foreach (var item in _harvestItemList)
        {
            if (item == null || item.item_info == null) continue;
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

    /// <summary>按一级 / 二级分类 + 关键字 + 排序规则刷新列表。</summary>
    public void RefreshPanel(Recycle_First_Category first_category = Recycle_First_Category.None,
        Recycle_Second_Category second_category = Recycle_Second_Category.None,
        string filterStr = "", InventorySortType sortType = InventorySortType.Rarity, bool isAscending = false)
    {
        ClearData();
        _currentSortType = sortType;

        var plantSeedDic = PlantManager.Instance != null ? PlantManager.Instance.plantSeedNameDic : null;
        _harvestItemList = BuildRecycleItemList(plantSeedDic);

        if (plantSeedDic == null)
        {
            // PlantManager 未就绪的降级分支：跳过二级分类反查，仍必须 ReloadScroller，
            // 否则 ClearData 后 scroller 未刷新会留旧 cell、数量错位。
            Log.Custom("ScrollerController_Recycle", "PlantManager.plantSeedNameDic 为空，无法判定作物种类，降级为只按搜索 / 排序显示。", Color.red);
            List<Game_Item_In_Inventory> fallback = new List<Game_Item_In_Inventory>(_harvestItemList);
            if (!string.IsNullOrEmpty(filterStr)) fallback = fallback.SelectByName(filterStr);
            fallback.SortByType(sortType, isAscending);
            foreach (var item in fallback)
            {
                if (item?.item_info == null) continue;
                dataList.Add(new ScrollData_GameItem(
                    item.item_info.name,
                    item.item_info.item_id,
                    item._item_count,
                    item.item_info.rarity,
                    item.uid,
                    Global_Inventory_Manager.IsNewObtainItem(item.uid)));
            }
            ReloadScroller();
            return;
        }

        var harvestNameToPlant = new Dictionary<string, PlantData>();
        foreach (var kv in plantSeedDic)
        {
            var pd = kv.Value;
            if (pd == null || pd.harvestItem == null) continue;
            foreach (var harvestName in pd.harvestItem)
            {
                if (string.IsNullOrEmpty(harvestName)) continue;
                if (!harvestNameToPlant.ContainsKey(harvestName))
                    harvestNameToPlant.Add(harvestName, pd);
            }
        }

        List<Game_Item_In_Inventory> showData = new List<Game_Item_In_Inventory>(_harvestItemList);

        if (!string.IsNullOrEmpty(filterStr))
        {
            showData = showData.SelectByName(filterStr);
        }

        if (first_category == Recycle_First_Category.Crop)
        {
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                return harvestNameToPlant.ContainsKey(item.item_info.name);
            }).ToList();
        }

        if (second_category != Recycle_Second_Category.None)
        {
            Plant_Second_Category expected = MapToPlantSecond(second_category);
            showData = showData.Where(item =>
            {
                if (item?.item_info == null) return false;
                if (!harvestNameToPlant.TryGetValue(item.item_info.name, out var pd) || pd == null) return false;
                return pd.plantType == expected;
            }).ToList();
        }

        showData.SortByType(sortType, isAscending);

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

    /// <summary>
    /// Harvest 类型 + 在任意 <see cref="PlantData.harvestItem"/> 中登记过的 Food_Material（如果 Plant 表未加载则仅 Harvest）。
    /// </summary>
    private static List<Game_Item_In_Inventory> BuildRecycleItemList(Dictionary<string, PlantData> plantSeedDic)
    {
        var byHarvestType = Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Harvest);
        if (plantSeedDic == null)
            return byHarvestType;

        var harvestNames = new HashSet<string>();
        foreach (var kv in plantSeedDic)
        {
            var pd = kv.Value;
            if (pd?.harvestItem == null) continue;
            foreach (var name in pd.harvestItem)
            {
                if (!string.IsNullOrEmpty(name)) harvestNames.Add(name);
            }
        }

        var byId = new Dictionary<int, Game_Item_In_Inventory>();
        foreach (var it in byHarvestType)
        {
            if (it == null) continue;
            byId[it.item_id] = it;
        }

        foreach (var it in Global_Inventory_Manager.Get_Items_By_Type(Item_Type.Food_Material))
        {
            if (it?.item_info == null) continue;
            if (!harvestNames.Contains(it.item_info.name)) continue;
            if (!byId.ContainsKey(it.item_id)) byId[it.item_id] = it;
        }

        return byId.Values.ToList();
    }

    public override void ClearData()
    {
        base.ClearData();
        _harvestItemList.Clear();
    }

    protected override void BindRow(FirstCellView_GameItem rowCell, int startIndex)
    {
        rowCell.SetData(dataList, startIndex, OnItemClick);
    }

    private void OnItemClick(ScrollData_GameItem data)
    {
        if (data == null) return;

        Game_Item_In_Inventory target = null;
        for (int i = 0; i < _harvestItemList.Count; i++)
        {
            var it = _harvestItemList[i];
            if (it == null || it.item_info == null) continue;
            if (it.item_info.item_id == data.id || it.item_name == data.name)
            {
                target = it;
                break;
            }
        }
        if (target == null)
        {
            target = Global_Inventory_Manager.GetItem(data.id);
        }
        if (target == null)
        {
            Log.Custom("ScrollerController_Recycle", "点击物品但未找到对应 Inventory 条目: " + data.name, Color.yellow);
            return;
        }

        recyclePanel.ShowSellPopup(target);
    }

    private static Plant_Second_Category MapToPlantSecond(Recycle_Second_Category s)
    {
        switch (s)
        {
            case Recycle_Second_Category.NormalPlant: return Plant_Second_Category.NormalPlant;
            case Recycle_Second_Category.VinePlant: return Plant_Second_Category.VinePlant;
            default: return Plant_Second_Category.None;
        }
    }
}
