using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

public class FirstCellView_CD : EnhancedScrollerCellView
{
    public SecondCellView_CD[] secondCellViews;
   

    public void SetData( List<ScrollData_GameItem> data, int startingIndex,Action<ScrollData_GameItem> action)
    {
        for (var i = 0; i < secondCellViews.Length; i++)
        {
            var dataIndex = startingIndex + i;
            secondCellViews[i].gameObject.name = "cd" + i.ToString();
            secondCellViews[i].zButton.name = "cd" + i.ToString() +"Button";
            // if the sub cell is outside the bounds of the data, we pass null to the sub cell
            secondCellViews[i].SetData(dataIndex, dataIndex < data.Count ? data[dataIndex] : null, ()=> action?.Invoke( data[dataIndex]));
        }
    }

    public SecondCellView_CD GetFirstCell()
    {
        return secondCellViews == null || secondCellViews.Length <= 0 ? null : secondCellViews[0];
    }

}
