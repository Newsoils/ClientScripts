using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;

public class FirstCellView_GameItem : EnhancedScrollerCellView
{
    public CellView_GameItem[] secondCellViews;
    public void SetData( List<ScrollData_GameItem> data, int startingIndex,Action<ScrollData_GameItem> clickEvent = null, Action<ScrollData_GameItem> pointDownEvent = null)
    {
        for (var i = 0; i < secondCellViews.Length; i++)
        {
            var dataIndex = startingIndex + i;

            // if the sub cell is outside the bounds of the data, we pass null to the sub cell
            secondCellViews[i].SetData( dataIndex < data.Count ? data[dataIndex] : null, clickEvent, pointDownEvent);
        }
    }
    public CellView_GameItem GetFirestElement()
    {
        if (secondCellViews.Length > 0)
        {
            return secondCellViews[0];
        }
        return null;
    }

}