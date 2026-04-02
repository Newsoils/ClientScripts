using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

public abstract class ScrollerController_GameItem<TData, TRowCell>
    : MonoBehaviour, IEnhancedScrollerDelegate
    where TRowCell : FirstCellView_GameItem
{
    public EnhancedScroller scroller;
    public TRowCell rowCellPrefab; // 明确这是 Prefab

    protected List<TData> dataList = new List<TData>();
    public int numberOfCellsPerRow = 5; // 建议改为 public，方便不同界面调整

    protected virtual void Start()
    {
        if (scroller != null) scroller.Delegate = this;
    }

    public virtual void ClearData() => dataList.Clear();
    protected virtual void ReloadScroller() => scroller.ReloadData();

    #region EnhancedScroller Delegate

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        if (dataList.Count == 0) return 0;
        return Mathf.CeilToInt((float)dataList.Count / numberOfCellsPerRow);
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => GetCellSize();

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cellView = scroller.GetCellView(rowCellPrefab) as TRowCell;

        // 计算这一行起始的数据索引
        var startIndex = dataIndex * numberOfCellsPerRow;

        cellView.name = $"Row {dataIndex} (Data {startIndex} to {startIndex + numberOfCellsPerRow - 1})";

        // 修正：将实例化的 cellView 传进去绑定数据
        BindRow(cellView, startIndex);

        return cellView;
    }

    #endregion

    protected virtual float GetCellSize() => 205f;
    protected abstract void BindRow(TRowCell rowCellInstance, int startIndex);
}