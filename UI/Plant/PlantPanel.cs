using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using Cmd;
using DG.Tweening;
using Google.Protobuf;
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
    private List<string> _baseSortOptions = new List<string> { "稀有度", "获取时间", "持有数量" };
    public GameObject PanelSecondLevelMenuPanel;

    public Button btn_Search;
    public Button BtnCloseSearch;
    public GameObject searchPanel;

    public Button btn_filter;

    public Button btn_DrawUp;
    public RectTransform invenArea;
    public Sprite up;
    public Sprite down;

    public Button btn_Exit;
    public Button ConfirmModification;


    private PlantFirstLabel[] firstLabels;
    private PlantSecondLabel[] secondLabels;

    public TMP_InputField searchInputField;

    public FilterPanel filterPanel;


    // 记录当前的一级菜单状态
    private Plant_First_Category _currentFirst = Plant_First_Category.None;
    private Plant_Second_Category _currentSecond = Plant_Second_Category.None;
    private InventorySortType _currentSort = InventorySortType.Rarity;
    private bool _isAscending = false;
    private string filterString = "";
    private PlantType _currentSeedPlantTypeFilter = PlantType.None;
    private ObtainSource _currentSeedObtainSourceFilter = ObtainSource.Unknown;
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
        firstLabels = firstCategoryParent.GetComponentsInChildren<PlantFirstLabel>();
        secondLabels = secondCategoryParent.GetComponentsInChildren<PlantSecondLabel>();
        InitDropdown();

        btn_Search.onClick.AddListener(() => OpenSearchPanel());
        // 改为切换展开/收起
        btn_DrawUp.onClick.AddListener(ToggleInvenArea);
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

        btn_filter.onClick.AddListener(() =>
        {
            filterPanel.OpenPanel();
        });


        EvtDsp.AddEvt(EvtNames.ReloadPlantData, RefreshUI);
        EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshUI);

        // 初始化为收起状态 (1,0.5) 且按钮图为 up
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
    IEnumerator DebouncedValidate(string text)
    {
        yield return new WaitForSeconds(debounceTime);
        // 执行实际校验逻辑
        filterString = SensitiveWordManager.Instance.FilterText(filterString);
        searchInputField.text = filterString;


        RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);

        Debug.Log($"Validating: {text}");
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // 结束可能存在的 tween
        _invenTween?.Kill();

        btn_Search.onClick.RemoveAllListeners();
        btn_DrawUp.onClick.RemoveAllListeners();
        ConfirmModification.onClick.RemoveAllListeners();

        btn_filter.onClick.RemoveAllListeners();
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

        ///发送看过消息，清除红点
        ReadItemBagReq req = new ReadItemBagReq() { BagTag = 4 };
        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);

        ReadItemBagReq req1 = new ReadItemBagReq() { BagTag = 5 };
        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req1);

        ReadItemBagReq req2 = new ReadItemBagReq() { BagTag = 6 };
        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req2);
    }


    // 刷新逻辑：由内部统一调度状态
    public void RefreshByFirstCategory(Plant_First_Category first, bool isFavorite = false)
    {
        _currentFirst = first;
        _currentSecond = Plant_Second_Category.None;
        _currentIsFavorite = isFavorite;
        if (first != Plant_First_Category.Seed)
        {
            ClearSeedExtraFilters();
        }

        // 将 isFavorite 传给 scroller（scroller 会根据 isFavorite 只显示收藏项）
        RefreshCurrentPanel();

        // 由面板负责更新二级菜单栏的显示
        UpdateSecondCategoryUI();
    }

    public void RefreshBySecondCategory(Plant_Second_Category second)
    {
        // 点击二级菜单时，直接带上记录好的 _currentFirst 和收藏过滤状态
        _currentSecond = second;
        RefreshCurrentPanel();
    }

    public void ApplySeedFilter(PlantType plantType, ObtainSource obtainSource)
    {
        _currentFirst = Plant_First_Category.Seed;
        _currentSecond = Plant_Second_Category.None;
        _currentSeedPlantTypeFilter = plantType;
        _currentSeedObtainSourceFilter = obtainSource;
        ResetAllLabels();
        UpdateSecondCategoryUI();
        RefreshCurrentPanel();
    }

    public void SetSeedPlantTypeFilter(PlantType plantType)
    {
        ApplySeedFilter(plantType, _currentSeedObtainSourceFilter);
    }

    public void SetSeedObtainSourceFilter(ObtainSource obtainSource)
    {
        ApplySeedFilter(_currentSeedPlantTypeFilter, obtainSource);
    }

    public PlantType CurrentSeedPlantTypeFilter => _currentSeedPlantTypeFilter;
    public ObtainSource CurrentSeedObtainSourceFilter => _currentSeedObtainSourceFilter;

    public void RefreshPanel(Plant_First_Category first_Category = Plant_First_Category.None,
        Plant_Second_Category second_Category = Plant_Second_Category.None, string filterStr = "",
        InventorySortType sortType = InventorySortType.Rarity, bool isFavorite = false, bool isAscending = false)
    {
        _currentFirst = first_Category;
        _currentSecond = second_Category;
        _currentSort = sortType;
        _isAscending = isAscending;
        _currentIsFavorite = isFavorite;
        filterString = filterStr;

        if (first_Category != Plant_First_Category.Seed)
        {
            ClearSeedExtraFilters();
        }

        RefreshCurrentPanel();
    }

    // 新增：切换 invenArea 展开/收起（使用 DOTween 动画）
    public void ToggleInvenArea()
    {
        if (invenArea == null || btn_DrawUp == null) return;

        // 目标值
        Vector2 target = _isInvenExpanded ? new Vector2(1f, 0.5f) : new Vector2(1f, 0.65f);
        Sprite targetSprite = _isInvenExpanded ? up : down;

        // 结束已有 tween
        if (_invenTween != null && _invenTween.IsActive()) _invenTween.Kill();

        // 立刻替换按钮图片（如需在动画完成后替换可改为 OnComplete）
        if (btn_DrawUp.image != null)
        {
            btn_DrawUp.image.sprite = targetSprite;
        }

        // 使用 DOTween 对 anchorMax 做插值
        _invenTween = DOTween.To(() => invenArea.anchorMax, x => invenArea.anchorMax = x, target, invenToggleDuration)
            .SetEase(Ease.OutCubic)
            .SetTarget(invenArea);

        // 切换状态标志
        _isInvenExpanded = !_isInvenExpanded;
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

    public void RefreshUI()
    {
        RefreshCurrentPanel();
        UpdateSecondCategoryUI();
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
            RefreshCurrentPanel();
            _waitingForCloseToggle = false;
            return;
        }

        _currentSort = (InventorySortType)index;
        _isAscending = false;
        _waitingForCloseToggle = false;
        RefreshSortDropdownVisual();
        RefreshCurrentPanel();
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
                    RefreshCurrentPanel();
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
        RefreshPanel(_currentFirst, _currentSecond, "", _currentSort, _currentIsFavorite, _isAscending);
    }

    private void RefreshCurrentPanel()
    {
        mScroller.RefreshPanel(
            _currentFirst,
            _currentSecond,
            filterString,
            _currentSort,
            _currentIsFavorite,
            _isAscending,
            _currentSeedPlantTypeFilter,
            _currentSeedObtainSourceFilter);
    }

    private void ClearSeedExtraFilters()
    {
        _currentSeedPlantTypeFilter = PlantType.None;
        _currentSeedObtainSourceFilter = ObtainSource.Unknown;
    }
}
