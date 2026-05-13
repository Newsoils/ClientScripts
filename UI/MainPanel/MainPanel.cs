using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : UIPanelBase
{
    public Button editRoom;
    public TopPanel topPanel;

    public Button dispatchButton;
    public GameObject dispatchWarningButton;

    public MainFunctionPanel mainFunctionPanel;

    public PhoneButton openPhoneButton;

    private void Start()
    {
        editRoom.onClick.AddListener(OpenPlacementInventory);
        dispatchButton.onClick.AddListener(Dispatch);
        RefreshDispatchWarningVisibility();


        EvtDsp.AddEvt<Room>(EvtNames.SwitchRoom, SwitchPanelByRoomType);

        EvtDsp.AddEvt<bool>(EvtNames.Set_PhoneButton_Enable, SetPhoneButtonEnable);
        EvtDsp.AddEvt<bool>(EvtNames.Set_TopPanel_Active, SetTopPanel_Active);
        EvtDsp.AddEvt<bool>(EvtNames.Set_MainFunction_Active, SetMainFunctionPanel_Active);
        EvtDsp.AddEvt(EvtNames.Show_TopPanel_Close_Other, ShowTopPanelOnly);
        EvtDsp.AddEvt(EvtNames.Set_MainPanel_All_Active, ShowAll);

        EvtDsp.AddEvt(EvtNames.OnPlacementPanelOpen, ShowTopPanelOnly);
        EvtDsp.AddEvt(EvtNames.OnPlacementPanelClose, ShowAll);
        EvtDsp.AddEvt(EvtNames.OnPlantPanelOpen, ShowTopPanelOnly);
        EvtDsp.AddEvt(EvtNames.OnPlantPanelClose, ShowAll);
        EvtDsp.AddEvt(EvtNames.OnClothPanelOpen, ShowTopPanelOnly);
        EvtDsp.AddEvt(EvtNames.OnClothPanelClose, ShowAll);
        EvtDsp.AddEvt(EvtNames.OnShoppingPanelOpen, ShowTopPanelOnly);
        EvtDsp.AddEvt(EvtNames.OnShoppingPanelClose, HandleShoppingPanelClose);
        EvtDsp.AddEvt(EvtNames.OnDispatchPanelOpen, ShowTopPanelOnly);
        EvtDsp.AddEvt(EvtNames.OnDispatchPanelClose, ShowAll);
        EvtDsp.AddEvt(EvtNames.OnPhonePanelOpen, CloseMainFuncP);
        EvtDsp.AddEvt(EvtNames.OnPhonePanelClose, ShowAll);
        EvtDsp.AddEvt(EvtNames.OnPerseonBriefOpen, ShowTopPanelOnly);
        EvtDsp.AddEvt(EvtNames.OnPerseonBriefClose, ShowAll);
        EvtDsp.AddEvt(EvtNames.OnTakePhotoPanelOpen, CloseAllUI);
        EvtDsp.AddEvt(EvtNames.OnTakePhotoPanelClose, ShowAll);
        EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshDispatchWarningVisibility);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        dispatchButton.onClick.RemoveAllListeners();
        editRoom.onClick.RemoveAllListeners();

        EvtDsp.RemoveEvt<Room>(EvtNames.SwitchRoom, SwitchPanelByRoomType);   

        EvtDsp.RemoveEvt<bool>(EvtNames.Set_PhoneButton_Enable, SetPhoneButtonEnable);
        EvtDsp.RemoveEvt<bool>(EvtNames.Set_TopPanel_Active, SetTopPanel_Active);
        EvtDsp.RemoveEvt<bool>(EvtNames.Set_MainFunction_Active, SetMainFunctionPanel_Active);
        EvtDsp.RemoveEvt(EvtNames.Show_TopPanel_Close_Other, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnPlacementPanelOpen, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnPlacementPanelClose, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.OnPlantPanelOpen, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnPlantPanelClose, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.OnClothPanelOpen, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnClothPanelClose, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.OnShoppingPanelOpen, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnShoppingPanelClose, HandleShoppingPanelClose);
        EvtDsp.RemoveEvt(EvtNames.Set_MainPanel_All_Active, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.OnDispatchPanelOpen, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnDispatchPanelClose, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.OnPhonePanelOpen, CloseMainFuncP);
        EvtDsp.RemoveEvt(EvtNames.OnPhonePanelClose, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.OnPerseonBriefOpen, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnPerseonBriefClose, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.OnTakePhotoPanelOpen, ShowTopPanelOnly);
        EvtDsp.RemoveEvt(EvtNames.OnTakePhotoPanelClose, ShowAll);
        EvtDsp.RemoveEvt(EvtNames.RefreshUI, RefreshDispatchWarningVisibility);
    }

    #region 开关面板元素
    public void SetPhoneButtonEnable(bool Enable)
    {
        openPhoneButton.enabled = Enable;
    }

    public void SetTopPanel_Active(bool active)
    {
        if(active) OpenTopPanel();
        else CloseTopPanel();
    }    

    public void OpenTopPanel()
    {
        topPanel.gameObject.SetActive(true);
    }

    public void CloseTopPanel()
    {
        topPanel.gameObject?.SetActive(false);
    }

    public void SetMainFunctionPanel_Active(bool active)
    {
        if(active) OpenMainFunctionPanel();
        else CloseMainFunctionPanel();
    }

    public void OpenMainFunctionPanel()
    {
        mainFunctionPanel.OpenPanel();
    }

    public void CloseMainFunctionPanel()
    {
        mainFunctionPanel.ClosePanel();
    }

    public static void CloseMainFuncP()
    {
        UIManager.Instance.GetPanel<MainPanel>().CloseMainFunctionPanel();
    }
    public static void OpenMainFuncP()
    {
        UIManager.Instance.GetPanel<MainPanel>().OpenMainFunctionPanel();
    }

    public static void SetPhoneBTNEnable(bool Enable)
    {
        UIManager.Instance.GetPanel<MainPanel>().SetPhoneButtonEnable(Enable);
    }

    public static void CloseTopP()
    {
        UIManager.Instance.GetPanel<MainPanel>().CloseTopPanel();
    }

    public static void OpenTopP()
    {
        UIManager.Instance.GetPanel<MainPanel>().OpenTopPanel();
    }

    // 只显示上方状态栏
    public void ShowTopPanelOnly()
    {
        topPanel.gameObject.SetActive(true);

        mainFunctionPanel.ClosePanel();
        dispatchButton.gameObject.SetActive(false);
        PhoneButton.Instance.ClosePanel();
        UIManager.Instance.GetPanel<PlantInteractPanel>().SetRight(false);

        this.transform.parent.GetComponent<Canvas>().sortingOrder = 100;
    }
    public void CloseAllUI()
    {
        ShowTopPanelOnly();
        topPanel.gameObject.SetActive(false);
    }

    private void HandleShoppingPanelClose()
    {
        // 派遣流程中关闭商店应返回派遣界面，不应恢复主界面所有入口。
        if (SceneLoadHelper.IsDispatchScene)
        {
            ShowTopPanelOnly();
            return;
        }
        ShowAll();
    }

    // 恢复原来的所有 UI 显示
    public void ShowAll()
    {
        topPanel.gameObject.SetActive(true);
        mainFunctionPanel.OpenPanel();
        dispatchButton.gameObject.SetActive(true);
        RefreshDispatchWarningVisibility();
        PhoneButton.Instance.OpenPanel();
        UIManager.Instance.GetPanel<PlantInteractPanel>().SetRight(true);
        SwitchPanelByRoomType(RoomSystem.currentRoom);
        this.transform.parent.GetComponent<Canvas>().sortingOrder = 0;
    }
    #endregion

    //这里不会控制面板整体的开关,处于常开状态
    //topPanel和mainFunctionPanel的开关单独控制
    public override void OpenPanel(params object[] data)
    {

    }

    public override void ClosePanel()
    {

    }

    public void Dispatch()
    {
        if (!SceneLoadHelper.IsDispatchScene)
        {
            //SceneLoadHelper.Load_DispatchScene();
            SceneLoadHelper.Load_DispatchScene();
            CloseMainFuncP();
            dispatchButton.gameObject.SetActive(false);
        }
    }
    public void OpenPlacementInventory()
    {
        UIManager.Instance.GetPanel<Placement_Panel>().OpenPanel();
    }

    public void OpenPlantInventory()
    {
        UIManager.Instance.GetPanel<PlantPanel>().OpenPanel();
    }
    public void SwitchPanelByRoomType(Room room)
    {
        switch (room.RoomType)
        {
            case RoomType.Balcony:
                SwitchPlantMode();
                break;
            default:
                SwitchNormalMode();
                break;
        }
    }
    public void SwitchPlantMode()
    {
        UIManager.Instance.OpenPanel<PlantInteractPanel>();
        mainFunctionPanel.SetRight(false);
        dispatchButton.gameObject.SetActive(false);
    }
    public void SwitchNormalMode()
    {
        UIManager.Instance.GetPanel<PlantInteractPanel>().ClosePanel();
        mainFunctionPanel.SetRight(true);
        dispatchButton.gameObject.SetActive(true);
        RefreshDispatchWarningVisibility();
    }

    private void RefreshDispatchWarningVisibility()
    {
        if (dispatchWarningButton == null)
        {
            return;
        }

        dispatchWarningButton.SetActive(ShouldShowDispatchWarning());
    }

    private bool ShouldShowDispatchWarning()
    {
        var mgr = Dispatch_Manager._instance;
        if (mgr == null || mgr.dispatch_Bags == null || mgr.dispatch_Bags.Count == 0)
        {
            return false;
        }

        foreach (var bag in mgr.dispatch_Bags)
        {
            if (bag == null)
            {
                continue;
            }

            bool hasAnyItem =
                !string.IsNullOrEmpty(bag.foodName) ||
                !string.IsNullOrEmpty(bag.snackName) ||
                !string.IsNullOrEmpty(bag.tapeName);
            if (hasAnyItem)
            {
                return false;
            }
        }

        return true;
    }
}
