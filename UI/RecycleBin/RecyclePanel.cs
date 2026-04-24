using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 回收箱仓库面板：复用种植仓库交互结构（一级 / 二级菜单 / 过滤 / 排序 / 搜索），
/// 物品来源见 <see cref="ScrollerController_Recycle"/>（Harvest + 植物表登记的果实类食材），一级菜单仅"作物收获物"，
/// 二级菜单为"普通植物 / 爬藤植物"。
/// </summary>
public class RecyclePanel : UIPanelBase
{
    public GameObject obj;

    public ScrollerController_Recycle mScroller;

    public Transform firstCategoryParent;
    public Transform secondCategoryParent;

    public TMP_Dropdown dropdown_SortType;
    public GameObject PanelSecondLevelMenuPanel;

    public Button btn_Search;
    public Button BtnCloseSearch;
    public GameObject searchPanel;

    public Button btn_Close;
    public Button btn_Exit;
    /// <summary>外层"确认回收"按钮，预留给后续批量回收用，当前未接 onClick。</summary>
    public Button ConfirmRecycle;

    /// <summary>点击物品时打开的出售弹窗（走 <see cref="ShowSellPopup"/>）。</summary>
    public RecycleSellPopup sellPopup;

    private RecycleFirstLabel[] firstLabels;
    private RecycleSecondLabel[] secondLabels;

    public TMP_InputField searchInputField;

    private Recycle_First_Category _currentFirst = Recycle_First_Category.None;
    private Recycle_Second_Category _currentSecond = Recycle_Second_Category.None;
    private InventorySortType _currentSort = InventorySortType.Rarity;
    private bool _isAscending = false;
    private string filterString = "";
    public float debounceTime = 0.25f;
    Coroutine currentCoroutine;

    private void Start()
    {
        // 先订阅，避免下面 Inspector 漏挂 NRE 导致 AddEvt 被跳过。
        EvtDsp.AddEvt(EvtNames.ReloadRecycleData, RefreshUI);

        firstLabels = firstCategoryParent.GetComponentsInChildren<RecycleFirstLabel>();
        secondLabels = secondCategoryParent.GetComponentsInChildren<RecycleSecondLabel>();
        InitDropdown();

        btn_Search.onClick.AddListener(() => OpenSearchPanel());
        btn_Close.onClick.AddListener(ClosePanel);
        btn_Exit.onClick.AddListener(ClosePanel);
        BtnCloseSearch.onClick.AddListener(CloseSearchPanel);

        searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
        searchInputField.onSubmit.AddListener(OnSearchValueChanged);
    }

    private void OnSearchValueChanged(string value)
    {
        filterString = value;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(DebouncedValidate(filterString));
    }

    IEnumerator DebouncedValidate(string text)
    {
        yield return new WaitForSeconds(debounceTime);
        filterString = SensitiveWordManager.Instance.FilterText(filterString);
        searchInputField.text = filterString;

        RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);

        Debug.Log($"RecyclePanel Validating: {text}");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        btn_Search.onClick.RemoveAllListeners();
        btn_Close.onClick.RemoveAllListeners();
        btn_Exit.onClick.RemoveAllListeners();
        BtnCloseSearch.onClick.RemoveAllListeners();
        searchInputField.onValueChanged.RemoveAllListeners();
        searchInputField.onSubmit.RemoveAllListeners();

        EvtDsp.RemoveEvt(EvtNames.ReloadRecycleData, RefreshUI);
    }

    public override void ClosePanel()
    {
        mScroller.ClearData();
        obj.SetActive(false);
        EvtDsp.TriggerEvt(EvtNames.OnRecyclePanelClose);
    }

    public override void OpenPanel(params object[] data)
    {
        obj.SetActive(true);
        mScroller.ReloadData();
        ResetAllLabels();
        RefreshByFirstCategory(Recycle_First_Category.None);
        sellPopup.ClosePanel();
        EvtDsp.TriggerEvt(EvtNames.OnRecyclePanelOpen);
    }

    public void RefreshByFirstCategory(Recycle_First_Category first)
    {
        _currentFirst = first;
        _currentSecond = Recycle_Second_Category.None;
        mScroller.RefreshPanel(_currentFirst, Recycle_Second_Category.None, filterString, _currentSort, _isAscending);
        UpdateSecondCategoryUI();
    }

    private void UpdateSecondCategoryUI()
    {
        var subsToOpen = Enum_Helper.GetSubCategories(_currentFirst);
        foreach (var label in secondLabels)
        {
            bool open = label.second_Category == Recycle_Second_Category.None
                        || subsToOpen.Contains(label.second_Category);
            label.gameObject.SetActive(open);
        }
    }

    public void RefreshBySecondCategory(Recycle_Second_Category second)
    {
        _currentSecond = second;
        mScroller.RefreshPanel(_currentFirst, second, filterString, _currentSort, _isAscending);
    }

    public void RefreshPanel(Recycle_First_Category first_Category = Recycle_First_Category.None,
        Recycle_Second_Category second_Category = Recycle_Second_Category.None, string filterStr = "",
        InventorySortType sortType = InventorySortType.Rarity, bool isAscending = false)
    {
        mScroller.RefreshPanel(first_Category, second_Category, filterStr, sortType, isAscending);
    }

    public void RefreshUI()
    {
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);
        UpdateSecondCategoryUI();
    }

    public void Set_FirstLabel_SelectedState(RecycleFirstLabel selected)
    {
        foreach (var label in firstLabels)
        {
            if (label != selected) label.SetSelectFalse();
        }
    }

    public void Set_AllSecondLabel_SelectState(RecycleSecondLabel selected)
    {
        foreach (var label in secondLabels)
        {
            if (label != selected) label.SetSelectFalse();
        }
    }

    public void ResetAllLabels()
    {
        foreach (var label in firstLabels) label.SetSelectFalse();
        foreach (var label in secondLabels) label.SetSelectFalse();
    }

    private void InitDropdown()
    {
        dropdown_SortType.ClearOptions();
        dropdown_SortType.AddOptions(new List<string> { "稀有度", "获取时间", "持有数量" });

        dropdown_SortType.onValueChanged.RemoveAllListeners();
        dropdown_SortType.onValueChanged.AddListener(OnSortDropdownChanged);

        dropdown_SortType.value = (int)_currentSort;
    }

    private void OnSortDropdownChanged(int index)
    {
        _currentSort = (InventorySortType)index;
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);
    }

    public void OpenSearchPanel()
    {
        searchPanel.SetActive(true);
        dropdown_SortType.gameObject.SetActive(false);
        btn_Search.gameObject.SetActive(false);
        PanelSecondLevelMenuPanel.SetActive(false);
    }

    public void CloseSearchPanel()
    {
        PanelSecondLevelMenuPanel.SetActive(true);
        dropdown_SortType.gameObject.SetActive(true);
        btn_Search.gameObject.SetActive(true);
        searchPanel.SetActive(false);
        RefreshPanel(_currentFirst, _currentSecond, "", _currentSort, _isAscending);
    }

    /// <summary>打开指定物品的出售弹窗。</summary>
    public void ShowSellPopup(Game_Item_In_Inventory item)
    {
        sellPopup.Open(item);
    }
}
