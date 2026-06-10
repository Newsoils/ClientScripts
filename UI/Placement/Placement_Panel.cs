using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Serialization;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.UI;
using Cmd;
using DG.Tweening;
using Google.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Placement_Panel : UIPanelBase
{
    public GameObject obj;

    public ScrollerController_Placement mScroller;

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

    public Button cancelModification;
    public Button clearAllPlacement;


    [Tooltip("测试用：循环切换 装修自由 / 俯视 / 平视 三个相机视角；平视时自动朝向视野中心墙。")]
    public Button btn_TestSwitchCamera;

    private static readonly CameraState[] s_placementCameraCycle = new[]
    {
        CameraState.Placement,
        CameraState.PlacementTopDown,
        CameraState.PlacementFrontView,
    };
    private CameraState _currentPlacementState = CameraState.Placement;

    private Placement_First_Label[] firstLabels;
    private Placement_Second_Label[] secondLabels;

    public TMP_InputField searchInputField;

    private Placement_First_Category _currentFirst = Placement_First_Category.None;
    private Placement_Second_Category _currentSecond = Placement_Second_Category.None;
    private InventorySortType _currentSort = InventorySortType.Rarity;
    private bool _isAscending = false;
    private bool _currentIsFavorite = false;
    private string filterString = "";
    public float debounceTime = 0.25f;
    Coroutine currentCoroutine;

    // 新增：控制展开/收起状态与 Tween
    private bool _isInvenExpanded = false;
    private Tween _invenTween = null;
    public float invenToggleDuration = 0.25f;

    private void Start()
    {
        firstLabels = firstCategoryParent.GetComponentsInChildren<Placement_First_Label>();
        secondLabels = secondCategoryParent.GetComponentsInChildren<Placement_Second_Label>();
        InitDropdown();

        btn_Search.onClick.AddListener(() => OpenSearchPanel());
        // 原来是 ClosePanel，这里改为切换 invenArea（展开/收起）
        btn_DrawUp.onClick.AddListener(ToggleInvenArea);
        BtnCloseSearch.onClick.AddListener(CloseSearchPanel);
        ConfirmModification.onClick.AddListener(ConfirmModify);

        cancelModification.onClick.AddListener(CancelModify);

        clearAllPlacement.onClick.AddListener(ClearAllPlacement);

        btn_TestSwitchCamera.onClick.AddListener(SwitchPlacementCamera);

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
        });

        EvtDsp.AddEvt(EvtNames.ReloadPlacementData, RefreshUI);
        EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshUI);

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


        ClosePanel();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // 结束可能存在的 tween
        _invenTween?.Kill();

        btn_Search.onClick.RemoveAllListeners();
        btn_DrawUp.onClick.RemoveAllListeners();
        ConfirmModification.onClick.RemoveAllListeners();
        cancelModification.onClick.RemoveAllListeners();
        clearAllPlacement.onClick.RemoveAllListeners();
        btn_TestSwitchCamera.onClick.RemoveAllListeners();

        searchInputField.onValueChanged.RemoveAllListeners();
        searchInputField.onSubmit.RemoveAllListeners();

        EvtDsp.RemoveEvt(EvtNames.ReloadPlacementData, RefreshUI);
        EvtDsp.RemoveEvt(EvtNames.RefreshUI, RefreshUI);
    }

    IEnumerator DebouncedValidate(string text)
    {
        yield return new WaitForSeconds(debounceTime);
        filterString = SensitiveWordManager.Instance.FilterText(filterString);
        searchInputField.text = filterString;

        cancelModification.onClick.RemoveAllListeners();

        RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);
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
        filterString = "";
        RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _isAscending);
    }

    public override void OpenPanel(params object[] data)
    {
        obj.SetActive(true);
        mScroller.ReloadData();
        ResetAllLabels();
        _currentFirst = Placement_First_Category.None;
        _currentSecond = Placement_Second_Category.None;
        _currentIsFavorite = false;
        RefreshByFirstCategory(Placement_First_Category.None);
        EvtDsp.TriggerEvt(EvtNames.OnPlacementPanelOpen);

        EditManager.Instance.SetMode(new DefaultGridObjectMode());

        var roomSnapshot = new RoomSaveData { rooms = RoomSystem.Instance.RoomDatas };
        Save_Load_Tools.Save("Room.json", roomSnapshot);

        EvtDsp.TriggerEvt<bool>(EvtNames.SetMainCharacterState, false);
        _currentPlacementState = CameraState.Placement;
        CameraManager.Instance.ChangeState(_currentPlacementState);


    }

    /// <summary>
    /// 测试按钮：循环切换三个 placement 相机视角（自由 → 俯视 → 平视 → 自由 …）。
    /// 切到 PlacementFrontView 时，找当前视野中心的墙（wall._wall_front 与水平相机视线点积最小者，即正对玩家那面墙），
    /// 把相机 yaw 设到面向该墙。
    /// </summary>
    private void SwitchPlacementCamera()
    {
        int idx = System.Array.IndexOf(s_placementCameraCycle, _currentPlacementState);
        int nextIdx = (idx + 1) % s_placementCameraCycle.Length;
        _currentPlacementState = s_placementCameraCycle[nextIdx];

        CameraManager.Instance.ChangeState(_currentPlacementState);

        if (_currentPlacementState == CameraState.PlacementFrontView)
        {
            var wall = RoomSystem.currentRoom?.FindClosestFacingWall(Camera.main);
            if (wall != null && CameraManager.Instance.curCtrl != null)
            {
                CameraManager.Instance.curCtrl.SetYawByForward(-wall._wall_front);
            }
        }
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

    public override void ClosePanel()
    {
        EditManager.Instance.ExitCurrentMode();

        mScroller.ClearData();
        obj.SetActive(false);
        UIManager.Instance.GetPanel<JoystickPanel>().SetDown();
        UIManager.Instance.GetPanel<EditPlacementPanel>().ClosePanel();
        CameraManager.Instance.ChangeState(CameraState.Normal);
        EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active);
        EvtDsp.TriggerEvt(EvtNames.OnPlacementPanelClose);
        EvtDsp.TriggerEvt(EvtNames.ReSetMainCharacterRenderer);

        ReadItemBagReq req = new ReadItemBagReq() { BagTag = 6 };
        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);
    }

    public void RefreshByFirstCategory(Placement_First_Category first, bool isFavorite = false)
    {
        _currentFirst = first;
        _currentSecond = Placement_Second_Category.None;
        _currentIsFavorite = isFavorite;

        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);
        UpdateSecondCategoryUI();
    }

    private void UpdateSecondCategoryUI()
    {
        var subsToOpen = Enum_Helper.GetSubCategories(_currentFirst);
        InventoryPanelUIHelper.UpdateSecondCategoryLabels(
            secondLabels,
            subsToOpen,
            Placement_Second_Category.None,
            label => label.second_Category,
            label => label.gameObject);
    }

    public void RefreshBySecondCategory(Placement_Second_Category second)
    {
        _currentSecond = second;
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);
    }

    public void RefreshPanel(
        Placement_First_Category first_Category = Placement_First_Category.None,
        Placement_Second_Category second_Category = Placement_Second_Category.None,
        string filterStr = "",
        InventorySortType sortType = InventorySortType.Rarity,
        bool isAscending = false)
    {
        _currentFirst = first_Category;
        _currentSecond = second_Category;
        _currentSort = sortType;
        _isAscending = isAscending;
        filterString = filterStr;

        mScroller.RefreshPanel(first_Category, second_Category, filterStr, sortType, _currentIsFavorite, isAscending);
    }

    public void Set_FirstLabel_SelectedState(Placement_First_Label selected)
    {
        InventoryPanelUIHelper.ResetOtherLabels(firstLabels, selected, label => label.SetSelectFalse());
    }

    public void Set_AllSecondLabel_SelectState(Placement_Second_Label selected)
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
                ? (_isAscending ? " <size=150%><b>↑</b></size>" : " <size=150%><b>↓</b></size>") 
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
            var template = blocker.parent;
            if (template != null)
            {
                var templateArrow = template.Find("Arrow");
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
        ClosePanel();
    }

    public void ClearAllPlacement()
    {
        RoomSystem.Instance.Clear_All_Placement();
        RoomSystem.Instance.Upload_Data_To_Server();
        ClosePanel();
        UIManager.Instance.GetPanel<EditPlacementPanel>().ClosePanel();
    }

    public async void CancelModify()
    {
        var data = Save_Load_Tools.Load<RoomSaveData>("Room.json");
        if (data == null || data.rooms == null)
        {
            PromptManager.ShowWarning(PromptId.PlacementCancelNoBackup);
            return;
        }

        ClosePanel();
        UIManager.Instance.GetPanel<EditPlacementPanel>().ClosePanel();

        await RoomSystem.Instance.ResetStateFromData(data);
        RoomSystem.Instance.Upload_Data_To_Server();
    }

    public void RefreshUI()
    {
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);
        UpdateSecondCategoryUI();
    }
}
