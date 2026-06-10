using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using Cmd;
using DG.Tweening;
using Google.Protobuf;
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
    private List<string> _baseSortOptions = new List<string> { "稀有度", "获取时间", "持有数量" };
    public GameObject PanelSecondLevelMenuPanel;

    public Button btn_Search;
    public Button BtnCloseSearch;
    public GameObject searchPanel;

    public Button btn_DrawUp;
    public RectTransform invenArea;
    public Sprite up;
    public Sprite down;

    public Button ConfirmModification;

    private ClothFirstLabel[] firstLabels;
    private ClothSecondLabel[] secondLabels;

    public TMP_InputField searchInputField;

    public Button exitButton;

    // 记录当前的一级菜单状态
    private Cloth_First_Category _currentFirst = Cloth_First_Category.None;
    private Cloth_Second_Category _currentSecond = Cloth_Second_Category.None;
    private InventorySortType _currentSort = InventorySortType.Rarity;
    private bool _isAscending = false;
    private string filterString = "";
    // 新增：记录当前是否为只显示收藏项
    private bool _currentIsFavorite = false;
    public float debounceTime = 0.25f;
    Coroutine currentCoroutine;

    // 新增：控制展开/收起状态与 Tween
    private bool _isInvenExpanded = false;
    private Tween _invenTween = null;
    public float invenToggleDuration = 0.25f;

    private void Start()
    {
        firstLabels = firstCategoryParent.GetComponentsInChildren<ClothFirstLabel>();
        secondLabels = secondCategoryParent.GetComponentsInChildren<ClothSecondLabel>();
        InitDropdown();

        ConfirmModification.onClick.AddListener(CloseChangeCloth);

        btn_Search.onClick.AddListener(() => OpenSearchPanel());
        // 将按钮行为改为切换展开/收起
        btn_DrawUp.onClick.AddListener(ToggleInvenArea);
        BtnCloseSearch.onClick.AddListener(CloseSearchPanel);

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

        exitButton.onClick.AddListener(ClosePanel);

        // 确保初始状态为收起（anchorMax = (1,0.5)，按钮图为 up）
        _isInvenExpanded = false;
        if (invenArea != null)
        {
            invenArea.anchorMax = new Vector2(1f, 0.5f);
        }
        if (btn_DrawUp != null && btn_DrawUp.image != null)
        {
            btn_DrawUp.image.sprite = up;
        }
    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        // 结束可能存在的 tween
        _invenTween?.Kill();

        btn_Search.onClick.RemoveAllListeners();
        btn_DrawUp.onClick.RemoveAllListeners();
        ConfirmModification.onClick.RemoveAllListeners();

        searchInputField.onSubmit.RemoveAllListeners();
        exitButton.onClick.RemoveListener(ClosePanel);
    }
    public override void ClosePanel()
    {
        mScroller.ClearData();
        obj.SetActive(false);

        // 关闭 wood 身体预览
        CharacterClothesManager.Instance.SetWoodBodyPreview(CharacterType.Target, false);

        EvtDsp.TriggerEvt(EvtNames.OnClothPanelClose);

        MainPanel.OpenMainFuncP();

        // 恢复 MainPanel 所有 UI 的正常显示
        UIManager.Instance.GetPanel<MainPanel>().ShowAll();

        ReadItemBagReq req = new ReadItemBagReq() { BagTag = 7 };
        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);
    }

    public override void OpenPanel(params object[] data)
    {
        obj.SetActive(true);

        MainPanel.CloseMainFuncP();

        // 显示上方状态栏 TopPanel，并使其优先级为最高
        UIManager.Instance.GetPanel<MainPanel>().ShowTopPanelOnly();

        mScroller.ReloadData();
        ResetAllLabels();
        // 默认打开时不带收藏筛选
        RefreshByFirstCategory(Cloth_First_Category.None);

        // 开启 wood 身体预览（只对 Target 预览角色）
        CharacterClothesManager.Instance.SetWoodBodyPreview(CharacterType.Target, true);

        EvtDsp.TriggerEvt(EvtNames.OnClothPanelOpen);
    }

    // 新增：切换 invenArea 展开/收起（使用 DOTween 动画）
    public void ToggleInvenArea()
    {
        _isInvenExpanded = InventoryPanelUIHelper.ToggleInventoryArea(
            invenArea,
            btn_DrawUp,
            up,
            down,
            _isInvenExpanded,
            ref _invenTween,
            invenToggleDuration,
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0.65f));
    }

    // 刷新逻辑：由面板负责调度状态
    public void RefreshByFirstCategory(Cloth_First_Category first, bool isFavorite = false)
    {
        _currentFirst = first;
        _currentSecond = Cloth_Second_Category.None;
        _currentIsFavorite = isFavorite;

        // 将 isFavorite 传给 scroller（scroller 会根据 isFavorite 只显示收藏项）
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);

        // 由面板负责更新二级菜单栏的显示
        UpdateSecondCategoryUI();
    }


    public void CloseChangeCloth()
    {
        CharacterClothesManager.Instance.SyncClothes(CharacterType.Target, CharacterType.Main);
        CharacterClothesManager.Instance.SaveClothesData();
        ClosePanel();
    }

    private void UpdateSecondCategoryUI()
    {
        var subsToOpen = Enum_Helper.GetSubCategories(_currentFirst);
        InventoryPanelUIHelper.UpdateSecondCategoryLabels(
            secondLabels,
            subsToOpen,
            Cloth_Second_Category.None,
            label => label.second_Category,
            label => label.gameObject);
    }

    public void RefreshBySecondCategory(Cloth_Second_Category second)
    {
        // 点击二级菜单时，直接带上记录好的 _currentFirst 和收藏过滤状态
        mScroller.RefreshPanel(_currentFirst, second, filterString, _currentSort, _currentIsFavorite, _isAscending);
    }


    public void RefreshPanel(Cloth_First_Category first_Category = Cloth_First_Category.None,
        Cloth_Second_Category second_Category = Cloth_Second_Category.None, string filterStr = "",
        InventorySortType sortType = InventorySortType.Rarity, bool isFavorite = false, bool isAscending = false)
    {
        _currentFirst = first_Category;
        _currentSecond = second_Category;
        _currentSort = sortType;
        _isAscending = isAscending;
        _currentIsFavorite = isFavorite;
        filterString = filterStr;

        mScroller.RefreshPanel(first_Category, second_Category, filterStr, sortType, _currentIsFavorite, isAscending);
    }

    /// <summary>
    /// 重置其他一级标签的选中状态
    /// </summary>
    /// <param name="selected">谁是被选中的</param>
    public void Set_FirstLabel_SelectedState(ClothFirstLabel selected)
    {
        InventoryPanelUIHelper.ResetOtherLabels(firstLabels, selected, label => label.SetSelectFalse());
    }

    /// <summary>
    /// 重置其他二级标签的选中状态
    /// </summary>
    /// <param name="selected">谁是被选中的</param>
    public void Set_AllSecondLabel_SelectState(ClothSecondLabel selected)
    {
        InventoryPanelUIHelper.ResetOtherLabels(secondLabels, selected, label => label.SetSelectFalse());
    }

    public void ResetAllLabels()
    {
        InventoryPanelUIHelper.ResetAllLabels(firstLabels, label => label.SetSelectFalse());
        InventoryPanelUIHelper.ResetAllLabels(secondLabels, label => label.SetSelectFalse());
    }


    private void InitDropdown()
    {
        dropdown_SortType.ClearOptions();
        dropdown_SortType.AddOptions(_baseSortOptions);

        dropdown_SortType.onValueChanged.RemoveAllListeners();
        dropdown_SortType.onValueChanged.AddListener(OnSortDropdownChanged);

        dropdown_SortType.value = (int)_currentSort;

        SetupDropdownClickDetection();

        RefreshSortDropdownVisual();
    }

    private int _valueBeforeOpen = -1;
    private bool _waitingForCloseToggle = false;

    private void SetupDropdownClickDetection()
    {
        var arrow = dropdown_SortType.transform.Find("Arrow");
        GameObject target = arrow != null ? arrow.gameObject : dropdown_SortType.gameObject;

        var trigger = target.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        var entry = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick
        };
        entry.callback.AddListener((data) =>
        {
            _valueBeforeOpen = dropdown_SortType.value;
            _waitingForCloseToggle = true;
        });
        trigger.triggers.Clear();
        trigger.triggers.Add(entry);
    }

    private void OnSortDropdownChanged(int index)
    {
        if (index == (int)_currentSort)
        {
            _isAscending = !_isAscending;
            RefreshSortDropdownVisual();
            mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);
            _waitingForCloseToggle = false;
            return;
        }

        _currentSort = (InventorySortType)index;
        _isAscending = false;
        _waitingForCloseToggle = false;
        RefreshSortDropdownVisual();
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);
    }

    private void LateUpdate()
    {
        var template = dropdown_SortType.transform.Find("Template");
        if (template == null) return;

        if (_waitingForCloseToggle)
        {
            if (!template.gameObject.activeSelf)
            {
                _waitingForCloseToggle = false;

                if (dropdown_SortType.value == _valueBeforeOpen && _valueBeforeOpen >= 0)
                {
                    _isAscending = !_isAscending;
                    RefreshSortDropdownVisual();
                    mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);
                }
            }
        }
    }

    private void RefreshSortDropdownVisual()
    {
        UpdateDropdownOptionsText();
        UpdateDropdownArrowRotation();
    }

    private void UpdateDropdownOptionsText()
    {
        var options = dropdown_SortType.options;
        for (int i = 0; i < options.Count; i++)
        {
            string arrow = i == (int)_currentSort
                ? (_isAscending ? " <size=120%><b>↑</b></size>" : " <size=120%><b>↓</b></size>")
                : "";
            options[i].text = _baseSortOptions[i] + arrow;
        }
        dropdown_SortType.options = options;
    }

    private void UpdateDropdownArrowRotation()
    {
        var arrow = dropdown_SortType.transform.Find("Arrow");
        if (arrow != null)
        {
            var rect = arrow.GetComponent<RectTransform>();
            if (rect != null)
            {
                float targetAngle = _isAscending ? 180f : 0f;
                rect.localRotation = Quaternion.Euler(0, 0, targetAngle);
            }
        }

        var blocker = dropdown_SortType.transform.Find("Blocker");
        if (blocker != null)
        {
            var templateParent = blocker.parent;
            if (templateParent != null)
            {
                var templateArrow = templateParent.Find("Arrow");
                if (templateArrow != null)
                {
                    var rect = templateArrow.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, _isAscending ? 180f : 0f);
                    }
                }
            }
        }
    }
    IEnumerator DebouncedValidate(string text)
    {
        yield return new WaitForSeconds(debounceTime);
        // 执行实际校验逻辑
        filterString = SensitiveWordManager.Instance.FilterText(filterString);
        searchInputField.text = filterString;


        RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);

        Debug.Log($"Validating: {text}");
    }
    public void OpenSearchPanel()
    {
        InventoryPanelUIHelper.SetSearchPanelVisible(
            searchPanel,
            dropdown_SortType,
            btn_Search,
            PanelSecondLevelMenuPanel,
            true);
    }


    public void CloseSearchPanel()
    {
        InventoryPanelUIHelper.SetSearchPanelVisible(
            searchPanel,
            dropdown_SortType,
            btn_Search,
            PanelSecondLevelMenuPanel,
            false);
        RefreshPanel(_currentFirst, _currentSecond, "", _currentSort, _currentIsFavorite, _isAscending);
    }
}
