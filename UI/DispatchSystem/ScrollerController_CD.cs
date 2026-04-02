using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using CLIP.Project_Mouse.ENUM;
using System.Linq;
using UnityEngine.UI;

public class ScrollerController_CD : MonoBehaviour, IEnhancedScrollerDelegate
{
    public FirstCellView_CD firstCellView_CD_Prefab;
    public EnhancedScroller scroller;
    private List<ScrollData_GameItem> gameItemDataList = new List<ScrollData_GameItem>();
    public int numberOfCellsPerRow = 3;

    // Start is called before the first frame update
    void Start()
    {
        scroller.Delegate = this;
        ReloadData();
    }

    public void ReloadData()
    {
        ClearData();

        var inventory = Global_Inventory_Manager.Items.ToList();
        if(inventory == null)
        {
            Log.Error(" ScrollerController_CD:Inventory is null!");
            return;

        }

        var cd_Data = inventory.FindAll(item => item?.item_info?.type == Item_Type.Tape);
     
        foreach (var item in cd_Data)
        {
            gameItemDataList.Add(new ScrollData_GameItem(
                item.item_info.name,
                item.item_info.item_id,
                item._item_count,
                item.item_info.rarity));
        }

        scroller.ReloadData();
    }

    
    public void ClearData()
    {
        gameItemDataList.Clear();
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        FirstCellView_CD cellView = scroller.GetCellView(firstCellView_CD_Prefab) as FirstCellView_CD;

        // data index of the first sub cell
        var di = dataIndex * numberOfCellsPerRow;

        cellView.name = "CD " + (di).ToString() + " to " + ((di) + numberOfCellsPerRow - 1).ToString();

        // pass in a reference to our data set with the offset for this cell
        cellView.SetData(ref gameItemDataList, di);

        return cellView;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 350f;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return gameItemDataList.Count;
    }

    /// <summary>
    /// 尝试拿一下第一个Cell（这个应该是当前显示的第一个）
    /// </summary>
    /// <returns></returns>
    public SecondCellView_CD GetFirstCell()
    {
        var cell = scroller.GetCellViewAtDataIndex(scroller.StartCellViewIndex) as FirstCellView_CD;
        return cell?.GetFirstCell();
    }

    public Button GetFirstButton()
    {
        return GetFirstCell()?.zButton as Button;
    }

}
