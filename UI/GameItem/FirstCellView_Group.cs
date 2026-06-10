using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;

public class FirstCellView_Group : EnhancedScrollerCellView
{
    public CellView_Group[] secondCellViews;

    public void SetData(List<ScrollData_ItemGroup> data, int startingIndex, Action<ScrollData_ItemGroup> clickEvent = null)
    {
        for (var i = 0; i < secondCellViews.Length; i++)
        {
            var dataIndex = startingIndex + i;
            secondCellViews[i].SetData(dataIndex < data.Count ? data[dataIndex] : null, clickEvent);
        }
    }
}
