using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClothPanel : UIPanelBase
{
    public GameObject obj;

    public ScrollerController_Cloth mScroller;

    public Transform firstCategoryParent;
    public Transform secondCategoryParent;

    public TMP_Dropdown dropdown_SortType;
    public GameObject PanelSecondLevelMenuPanel;

    public Button btn_Search;
    public Button BtnCloseSearch;
    public GameObject searchPanel;

    public Button btn_Close;
    public Button ConfirmModification;

    private ClothFirstLabel[] firstLabels;
    private ClothSecondLabel[] secondLabels;

    public TMP_InputField searchInputField;

    // 记录当前的一级菜单状态
    private Cloth_First_Category _currentFirst = Cloth_First_Category.None;
    private Cloth_Second_Category _currentSecond = Cloth_Second_Category.None;
    private InventorySortType _currentSort = InventorySortType.Rarity;
    private bool _isAscending = false;
    private string filterString = "";
    public float debounceTime = 0.25f;
    Coroutine currentCoroutine;

    private void Start()
    {
        firstLabels = firstCategoryParent.GetComponentsInChildren<ClothFirstLabel>();
        secondLabels = secondCategoryParent.GetComponentsInChildren<ClothSecondLabel>();
        InitDropdown();


        ConfirmModification.onClick.AddListener(CloseChangeCloth);

        btn_Search.onClick.AddListener(() => OpenSearchPanel());
        btn_Close.onClick.AddListener(ClosePanel);
        BtnCloseSearch.onClick.AddListener(CloseSearchPanel);
        //ConfirmModification.onClick.AddListener(ClosePanel);

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
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        btn_Search.onClick.RemoveAllListeners();
        btn_Close.onClick.RemoveAllListeners();
        ConfirmModification.onClick.RemoveAllListeners();

        searchInputField.onSubmit.RemoveAllListeners();
    }
    public override void ClosePanel()
    {
        mScroller.ClearData();
        obj.SetActive(false);
        EvtDsp.TriggerEvt(EvtNames.OnClothPanelClose);
    }

    public override void OpenPanel(params object[] data)
    {
        obj.SetActive(true);
        mScroller.ReloadData();
        ResetAllLabels();
        RefreshByFirstCategory(Cloth_First_Category.None);
        EvtDsp.TriggerEvt(EvtNames.OnClothPanelOpen);
    }


    // 刷新逻辑：由内部统一调度状态
    public void RefreshByFirstCategory(Cloth_First_Category first)
    {
        _currentFirst = first;

        // 1. 刷新列表（二级菜单默认为 None）
        mScroller.RefreshPanel(_currentFirst, Cloth_Second_Category.None, filterString, _currentSort, _isAscending);

        // 2. 核心：由面板负责更新二级菜单栏的显示
        UpdateSecondCategoryUI();
    }


    public void CloseChangeCloth()
    {
        Character_Cloth_Manager.Instance.Sync_Main_Character_Cloth();
        Character_Cloth_Manager.Instance.Upload_Main_Characer_Cloth_Info();
        UIManager.Instance.GetPanel<ChangeClothPanel>().ClosePanel();
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
            if (label.second_Category == Cloth_Second_Category.None)
            {
                label.gameObject.SetActive(true);
            }
        }
    }

    public void RefreshBySecondCategory(Cloth_Second_Category second)
    {
        // 点击二级菜单时，直接带上记录好的 _currentFirst
        mScroller.RefreshPanel(_currentFirst, second, filterString, _currentSort, _isAscending);
    }


    public void RefreshPanel(Cloth_First_Category first_Category = Cloth_First_Category.None,
        Cloth_Second_Category second_Category = Cloth_Second_Category.None, string filterStr = "",
        InventorySortType sortType = InventorySortType.Rarity, bool isAscending = false)
    {
        mScroller.RefreshPanel(first_Category, second_Category, filterStr, sortType, isAscending);
    }

    /// <summary>
    /// 重置其他一级标签的选中状态
    /// </summary>
    /// <param name="selected">谁是被选中的</param>
    public void Set_FirstLabel_SelectedState(ClothFirstLabel selected)
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
    public void Set_AllSecondLabel_SelectState(ClothSecondLabel selected)
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
    IEnumerator DebouncedValidate(string text)
    {
        yield return new WaitForSeconds(debounceTime);
        // 执行实际校验逻辑
        filterString = SensitiveWordManager.Instance.FilterText(filterString);
        searchInputField.text = filterString;


        RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);

        Debug.Log($"Validating: {text}");
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
