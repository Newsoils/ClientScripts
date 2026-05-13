using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.UI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

public class ScrollerController_DispatchItem : MonoBehaviour, IEnhancedScrollerDelegate
{
    public CellView_DispatchItem gameItemCellView;
    private List<ScrollData_GameItem> gameItemDataList = new List<ScrollData_GameItem>();
    public Button jumpToShop;
    private Item_Type currentType;
    private bool hasLoadedType;

    /// <summary>
    /// This is our scroller we will be a delegate for
    /// </summary>
    public EnhancedScroller scroller;


    void Start()
    {
        scroller.Delegate = this;
        jumpToShop.onClick.AddListener(JumpToShop);
        EvtDsp.AddEvt(EvtNames.RefreshUI, OnRefreshUI);
        //RefreshUI(Item_Type.Food);
    }

    private void OnDestroy()
    {
        jumpToShop.onClick.RemoveListener(JumpToShop);
        EvtDsp.RemoveEvt(EvtNames.RefreshUI, OnRefreshUI);
    }

    private void OnRefreshUI()
    {
        // 仅在已初始化过类型后刷新，避免首次未选择类型时用默认值覆盖。
        if (!hasLoadedType)
        {
            return;
        }
        ReloadData(currentType);
    }


    public void ReloadData(Item_Type type)
    {
        ClearData();

        currentType = type;
        hasLoadedType = true;

        var items = Global_Inventory_Manager.Get_Items_By_Type(type);
        if(items.Count == 0)
        {
            jumpToShop.gameObject.SetActive(true);
        }
        else
        {
            jumpToShop.gameObject.SetActive(false);
        }
        foreach (var item in items)
        {
            gameItemDataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity));
        }

        scroller.ReloadData();
    }

    /// <summary>
    /// 当前已选中的物品类型（食物 / 零食）下刷新列表，用于背包槽变更后更新占用显示。
    /// </summary>
    public void ReloadCurrentIfLoaded()
    {
        if (hasLoadedType)
        {
            ReloadData(currentType);
        }
    }


    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(gameItemCellView) as CellView_DispatchItem;

        var data = gameItemDataList[dataIndex];

        // pass in a reference to our data set with the offset for this cell
        cellView.SetData(data);

        var panel = UIManager.Instance != null ? UIManager.Instance.GetPanel<DispatchPanel>() : null;
        var mgr = Dispatch_Manager._instance;
        if (panel != null && mgr != null && mgr.dispatch_Bags != null && panel.CurrentBagIndex >= 0
            && panel.CurrentBagIndex < mgr.dispatch_Bags.Count)
        {
            var bagsView = panel.GetBagsViewForWarehouse();
            var curBag = bagsView[panel.CurrentBagIndex];
            var state = DispatchWarehouseVisual.GetCellState(data.count, currentType, data.name, curBag, bagsView);
            int displayCount = DispatchWarehouseVisual.GetDisplayCount(data.count, currentType, data.name, bagsView);
            cellView.ApplyDispatchWarehouseVisual(state, displayCount);
            cellView.SetClickEvent(() => OnDispatchWarehouseCellClicked(data));
        }
        else
        {
            cellView.ApplyDispatchWarehouseVisual(DispatchWarehouseCellState.Normal, data.count);
            cellView.SetClickEvent(() => UIManager.Instance.GetPanel<DispatchPanel>().SetDispatchItem(currentType, data.name));
        }

        // return the cell to the scroller
        return cellView;
    }

    private void OnDispatchWarehouseCellClicked(ScrollData_GameItem data)
    {
        var panel = UIManager.Instance.GetPanel<DispatchPanel>();
        var mgr = Dispatch_Manager._instance;
        if (panel == null || mgr == null || mgr.dispatch_Bags == null)
        {
            panel?.SetDispatchItem(currentType, data.name);
            return;
        }

        int bi = panel.CurrentBagIndex;
        if (bi < 0 || bi >= mgr.dispatch_Bags.Count)
        {
            panel.SetDispatchItem(currentType, data.name);
            return;
        }

        var bagsView = panel.GetBagsViewForWarehouse();
        var curBag = bagsView[bi];
        var state = DispatchWarehouseVisual.GetCellState(data.count, currentType, data.name, curBag, bagsView);

        // 占用态 = 当前背包已选该物品；再点同一格 → 收回槽位（不写真实库存）。
        if (state == DispatchWarehouseCellState.OccupiedByCurrentBag)
        {
            panel.ClearDispatchSlot(currentType);
        }
        else if (state == DispatchWarehouseCellState.InUseElsewhere)
        {
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, "库存不足，该物品已在其他背包中使用");
            return;
        }
        else
        {
            panel.SetDispatchItem(currentType, data.name);
        }
    }
    public void JumpToShop()
    {
        StartCoroutine(JumpToShopCoroutine());
    }

    private IEnumerator JumpToShopCoroutine()
    {
        var shoppingPanel = UIManager.Instance.OpenPanel<ShoppingPanel>();
        shoppingPanel.OpenOrChooseDailyMagazine();

        // 首次打开时 DailyMagazinePanel.Start() 会默认切到 Clothes，等其执行后再强制切到携带物。
        yield return null;
        shoppingPanel.dailyMagazinePanel.OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory.Belonging);
        switch(currentType)
        {
            case Item_Type.Food:
                shoppingPanel.dailyMagazinePanel.OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Food);
                break;
            case Item_Type.Snack:
                shoppingPanel.dailyMagazinePanel.OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Snack);
                break;
        }

    }

    public void ClearData()
    {
        gameItemDataList.Clear();
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 200f;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return gameItemDataList.Count;
    }

    /// <summary>
    /// 尝试拿一下第一个Cell（这个应该是当前显示的第一个）
    /// </summary>
    /// <returns></returns>
    public CellView_DispatchItem GetFirstCell()
    {
        var cell = scroller.GetCellViewAtDataIndex(scroller.StartCellViewIndex);
        return cell as CellView_DispatchItem;
    }

    public Button GetFirstButton()
    {
        return GetFirstCell()?.button;
    }

}
