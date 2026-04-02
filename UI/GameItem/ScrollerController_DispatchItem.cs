using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

public class ScrollerController_DispatchItem : MonoBehaviour, IEnhancedScrollerDelegate
{
    public CellView_DispatchItem gameItemCellView;
    private List<ScrollData_GameItem> gameItemDataList = new List<ScrollData_GameItem>();

    private Item_Type currentType;

    /// <summary>
    /// This is our scroller we will be a delegate for
    /// </summary>
    public EnhancedScroller scroller;


    void Start()
    {
        scroller.Delegate = this;
        //RefreshUI(Item_Type.Food);
    }


    public void ReloadData(Item_Type type)
    {
        ClearData();

        currentType = type;

        var items = Global_Inventory_Manager.Get_Items_By_Type(type);

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


    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(gameItemCellView) as CellView_DispatchItem;

        var data = gameItemDataList[dataIndex];

        // pass in a reference to our data set with the offset for this cell
        cellView.SetData(data);

        cellView.SetClickEvent(()=> UIManager.Instance.GetPanel<DispatchPanel>().SetDispatchItem(currentType, data.name));

        // return the cell to the scroller
        return cellView;
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
