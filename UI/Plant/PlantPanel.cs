using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantPanel : UIPanelBase
{
    public GameObject obj;

    public ScrollerController_Plant mScroller;

    public Transform firstCategoryParent;
    public Transform secondCategoryParent;

    public TMP_Dropdown dropdown_SortType;
    public GameObject PanelSecondLevelMenuPanel;

    public Button btn_Search;
    public Button BtnCloseSearch;
    public GameObject searchPanel;

    public Button btn_Close;
    public Button btn_Exit;
    public Button ConfirmModification;


    private PlantFirstLabel[] firstLabels;
    private PlantSecondLabel[] secondLabels;

    public TMP_InputField searchInputField;

    // 记录当前的一级菜单状态
    private Plant_First_Category _currentFirst = Plant_First_Category.None;
    private Plant_Second_Category _currentSecond = Plant_Second_Category.None;
    private InventorySortType _currentSort = InventorySortType.Rarity;
    private bool _isAscending = false;
    private string filterString = "";
    public float debounceTime = 0.25f;
    Coroutine currentCoroutine;

    private void Start()
    {
        firstLabels = firstCategoryParent.GetComponentsInChildren<PlantFirstLabel>();
        secondLabels = secondCategoryParent.GetComponentsInChildren<PlantSecondLabel>();
        InitDropdown();

        btn_Search.onClick.AddListener(() => OpenSearchPanel());
        btn_Close.onClick.AddListener(ClosePanel);
        btn_Exit.onClick.AddListener(ClosePanel);
        BtnCloseSearch.onClick.AddListener(CloseSearchPanel);
        ConfirmModification.onClick.AddListener(ConfirmModify);

        searchInputField.onValueChanged.AddListener(value =>
        {
            filterString = value;

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(DebouncedValidate(filterString));

        });

        searchInputField.onSubmit.AddListener(value =>
        {
            filterString = value;

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(DebouncedValidate(filterString));
            //RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);
        });

        EvtDsp.AddEvt(EvtNames.ReloadPlantData, RefreshUI);
        EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshUI);
    }
    IEnumerator DebouncedValidate(string text)
    {
        yield return new WaitForSeconds(debounceTime);
        // 执行实际校验逻辑
        filterString = SensitiveWordManager.Instance.FilterText(filterString);
        searchInputField.text = filterString;


        RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);

        Debug.Log($"Validating: {text}");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        btn_Search.onClick.RemoveAllListeners();
        btn_Close.onClick.RemoveAllListeners();
        ConfirmModification.onClick.RemoveAllListeners();

        searchInputField.onSubmit.RemoveAllListeners();

        EvtDsp.RemoveEvt(EvtNames.ReloadPlantData, RefreshUI);
        EvtDsp.RemoveEvt(EvtNames.RefreshUI, RefreshUI);
    }
    public override void ClosePanel()
    {
        ClickManager.NotifyUiConsumedPick();
        EditManager.Instance.ExitCurrentMode();
        mScroller.ClearData();
        obj.SetActive(false);
        EvtDsp.TriggerEvt(EvtNames.OnPlantPanelClose);
    }

    public override void OpenPanel(params object[] data)
    {
        EditManager.Instance.SetMode(new DefaultGridObjectMode());
        obj.SetActive(true);
        mScroller.ReloadData();
        ResetAllLabels();
        RefreshByFirstCategory(Plant_First_Category.None);
        EvtDsp.TriggerEvt(EvtNames.OnPlantPanelOpen);
    }


    // 刷新逻辑：由内部统一调度状态
    public void RefreshByFirstCategory(Plant_First_Category first)
    {
        _currentFirst = first;

        // 1. 刷新列表（二级菜单默认为 None）
        mScroller.RefreshPanel(_currentFirst, Plant_Second_Category.None, filterString, _currentSort, _isAscending);

        // 2. 核心：由面板负责更新二级菜单栏的显示
        UpdateSecondCategoryUI();
    }

    private void UpdateSecondCategoryUI()
    {
        // 从 Helper 获取映射
        var subsToOpen = Enum_Helper.GetSubCategories(_currentFirst);


        foreach (var label in secondLabels)
        {
            if (subsToOpen.Contains(label.second_Category))
            {
                label.gameObject.SetActive(true); // 激活已有的
            }
            else
            {
                label.gameObject.SetActive(false); // 不需要的就隐藏
            }
            if (label.second_Category == Plant_Second_Category.None)
            {
                label.gameObject.SetActive(true);
            }
        }
    }

    public void RefreshBySecondCategory(Plant_Second_Category second)
    {
        // 点击二级菜单时，直接带上记录好的 _currentFirst
        mScroller.RefreshPanel(_currentFirst, second, filterString, _currentSort, _isAscending);
    }


    public void RefreshPanel(Plant_First_Category first_Category = Plant_First_Category.None,
        Plant_Second_Category second_Category = Plant_Second_Category.None, string filterStr = "",
        InventorySortType sortType = InventorySortType.Rarity, bool isAscending = false)
    {
        mScroller.RefreshPanel(first_Category, second_Category, filterStr, sortType, isAscending);
    }

    public void RefreshUI()
    {
        RefreshByFirstCategory(_currentFirst);
    }
    /// <summary>
    /// 重置其他一级标签的选中状态
    /// </summary>
    /// <param name="selected">谁是被选中的</param>
    public void Set_FirstLabel_SelectedState(PlantFirstLabel selected)
    {
        foreach (var label in firstLabels)
        {
            if (label != selected)
            {
                label.SetSelectFalse();
            }
        }
    }

    /// <summary>
    /// 重置其他二级标签的选中状态
    /// </summary>
    /// <param name="selected">谁是被选中的</param>
    public void Set_AllSecondLabel_SelectState(PlantSecondLabel selected)
    {
        foreach (var label in secondLabels)
        {
            if (label != selected)
                label.SetSelectFalse();
        }
    }

    public void ResetAllLabels()
    {
        // 重置一级标签
        foreach (var label in firstLabels)
        {
            label.SetSelectFalse();
        }
        // 重置二级标签
        foreach (var label in secondLabels)
        {
            label.SetSelectFalse();
        }
    }


    private void InitDropdown()
    {
        dropdown_SortType.ClearOptions();

        // 只添加前三个：Rarity, ObtainDate, Count
        List<string> options = new List<string> { "稀有度", "获取时间", "持有数量" };
        dropdown_SortType.AddOptions(options);

        // 注册监听
        dropdown_SortType.onValueChanged.RemoveAllListeners();
        dropdown_SortType.onValueChanged.AddListener(OnSortDropdownChanged);

        // 设置初始值
        dropdown_SortType.value = (int)_currentSort;
    }

    private void OnSortDropdownChanged(int index)
    {
        // index 刚好对应枚举：0:Rarity, 1:ObtainDate, 2:Count
        _currentSort = (InventorySortType)index;

        // 刷新面板，保留当前的一、二级分类
        // 假设你还需要一个变量记录当前的二级分类
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);
    }

    public void ConfirmModify()
    {
        RoomSystem.Instance.Upload_Data_To_Server();
        CameraManager.Instance.ChangeState(CameraState.Normal);
        ClosePanel();
        EvtDsp.TriggerEvt(EvtNames.Close_Edit_Placement_Panel);
    }
    public void OpenSearchPanel()
    {
        searchPanel.gameObject.SetActive(true);
        dropdown_SortType.gameObject.SetActive(false);
        btn_Search.gameObject.SetActive(false);
        PanelSecondLevelMenuPanel.SetActive(false);
    }



    public void CloseSearchPanel()
    {
        PanelSecondLevelMenuPanel.SetActive(true);
        dropdown_SortType.gameObject.SetActive(true);
        btn_Search.gameObject.SetActive(true);
        searchPanel.gameObject.SetActive(false);
        RefreshPanel(_currentFirst, _currentSecond, "", _currentSort, _isAscending);
    }
}
