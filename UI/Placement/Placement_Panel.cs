using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Serialization;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Scene_View_Control;
using CLIP.Project_Mouse.UI;
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
    public GameObject PanelSecondLevelMenuPanel;

    public Button btn_Search;
    public Button BtnCloseSearch;
    public GameObject searchPanel;

    public Button btn_Close;
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

    private void Start()
    {
        firstLabels = firstCategoryParent.GetComponentsInChildren<Placement_First_Label>();
        secondLabels = secondCategoryParent.GetComponentsInChildren<Placement_Second_Label>();
        InitDropdown();

        btn_Search.onClick.AddListener(() => OpenSearchPanel());
        btn_Close.onClick.AddListener(ClosePanel);
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
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        btn_Search.onClick.RemoveAllListeners();
        btn_Close.onClick.RemoveAllListeners();
        ConfirmModification.onClick.RemoveAllListeners();
        cancelModification.onClick.RemoveAllListeners();
        clearAllPlacement.onClick.RemoveAllListeners();
        btn_TestSwitchCamera.onClick.RemoveAllListeners();

        searchInputField.onValueChanged.RemoveAllListeners();
        searchInputField.onSubmit.RemoveAllListeners();

        EvtDsp.RemoveEvt(EvtNames.ReloadPlacementData, RefreshUI);
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
            var wall = FindClosestFacingWall();
            if (wall != null && CameraManager.Instance.curCtrl != null)
            {
                CameraManager.Instance.curCtrl.SetYawByForward(-wall._wall_front);
            }
        }
    }

    /// <summary>找当前房间里最被相机正对的那面墙：水平投影下 wall._wall_front 与 camDir 点积最小（最接近 -1）。</summary>
    private static Hide_Wall FindClosestFacingWall()
    {
        var room = RoomSystem.currentRoom;
        if (room == null || Camera.main == null) return null;

        Vector3 camDir = Camera.main.transform.forward;
        camDir.y = 0f;
        if (camDir.sqrMagnitude < 1e-6f) return null;
        camDir.Normalize();

        Hide_Wall best = null;
        float minDot = float.PositiveInfinity;
        foreach (var wall in room.wallsMap.Values)
        {
            if (wall == null) continue;
            Vector3 front = wall._wall_front;
            front.y = 0f;
            if (front.sqrMagnitude < 1e-6f) continue;
            front.Normalize();
            float dot = Vector3.Dot(front, camDir);
            if (dot < minDot)
            {
                minDot = dot;
                best = wall;
            }
        }
        return best;
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

        foreach (var label in secondLabels)
        {
            if (subsToOpen.Contains(label.second_Category))
            {
                label.gameObject.SetActive(true);
            }
            else
            {
                label.gameObject.SetActive(false);
            }

            if (label.second_Category == Placement_Second_Category.None)
            {
                label.gameObject.SetActive(true);
            }
        }
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
        foreach (var label in firstLabels)
        {
            if (label != selected)
            {
                label.SetSelectFalse();
            }
        }
    }

    public void Set_AllSecondLabel_SelectState(Placement_Second_Label selected)
    {
        foreach (var label in secondLabels)
        {
            if (label != selected)
            {
                label.SetSelectFalse();
            }
        }
    }

    public void ResetAllLabels()
    {
        foreach (var label in firstLabels)
        {
            label.SetSelectFalse();
        }

        foreach (var label in secondLabels)
        {
            label.SetSelectFalse();
        }
    }

    private void InitDropdown()
    {
        dropdown_SortType.ClearOptions();

        List<string> options = new List<string> { "稀有度", "获取时间", "持有数量" };
        dropdown_SortType.AddOptions(options);

        dropdown_SortType.onValueChanged.RemoveAllListeners();
        dropdown_SortType.onValueChanged.AddListener(OnSortDropdownChanged);

        dropdown_SortType.value = (int)_currentSort;
    }

    private void OnSortDropdownChanged(int index)
    {
        _currentSort = (InventorySortType)index;
        mScroller.RefreshPanel(_currentFirst, _currentSecond, filterString, _currentSort, _currentIsFavorite, _isAscending);
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
            EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel,
                "无法读取进入布置前的房间备份。请先打开一次布置界面再取消，或检查存储权限。");
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
