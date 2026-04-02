using System.Collections;
using CLIP.Project_Mouse.UI;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using CLIP.Framework_Core.Event;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UIElements;

public class GuideStepHelper : MonoBehaviour
{

    public void GetMapBalconyButton(GuideStep step)
    {
        var mapPanel = UIManager.Instance.GetPanel<MapPanel>();
        if(mapPanel!=null)
        {
            step.SetListenButton(mapPanel.balcony);
        }
    }
    public void GetMapButton(GuideStep step)
    {
        var mainPanel = UIManager.Instance.GetPanel<MainPanel>();
        if (mainPanel != null)
        {
            step.SetListenButton(mainPanel.mainFunctionPanel.HouseButton);
        }
    }

    public void GetConfirmModiButton(GuideStep step)
    {
        var plPanel = UIManager.Instance.GetPanel<Placement_Panel>();
        if(plPanel !=null)
        {
            step.SetListenButton(plPanel.ConfirmModification);
        }
    }

    public void GetOpenPhoneButton(GuideStep step)
    {
        var mainPanel = UIManager.Instance.GetPanel<MainPanel>();
        if (mainPanel != null)
        {
            step.SetListenButton(mainPanel.openPhoneButton.phoneButton);
        }
    }

    public void GetPhotoButton(GuideStep step)
    {
        var phonePanel = UIManager.Instance.GetPanel<PhonePanel>();
        if(phonePanel != null)
        {
            step.SetListenButton(phonePanel.PhotoButton);
        }
    }

    public void GetCloseShowPhotoButton(GuideStep step)
    {
        var showPhonePanel = UIManager.Instance.GetPanel<ShowPhotoPanel>();
        if (showPhonePanel != null)
        {
            step.SetListenButton(showPhonePanel.btnExit);
        }
    }

    public void GetPhotoSaveButton(GuideStep step)
    {
        var showPhotoPanel = UIManager.Instance.GetPanel<ShowPhotoPanel>();
        if(showPhotoPanel!=null)
        {
            step.SetListenButton(showPhotoPanel.save);
        }
    }

    public void GetRewardButton(GuideStep step)
    {
        var rewardPanel = UIManager.Instance.GetPanel<RewardPanel>();
        if (rewardPanel != null )
        {
            step.SetListenButton(rewardPanel.exitButton);
        }
    }

    public void GetUseTicketButton(GuideStep step)
    {
        var mainPanel = UIManager.Instance.GetPanel<MainPanel>();
        if(mainPanel != null )
        {
            step.SetListenButton(mainPanel.mainFunctionPanel.TicketButton);
        }
    }

    public void GetTicketConfirmButton(GuideStep step)
    {
        var mainPanel = UIManager.Instance.GetPanel<MainPanel>();
        if (mainPanel != null)
        {
            step.SetListenButton(mainPanel.mainFunctionPanel.TicketConfimButton);
        }
    }

    public void GetDispatchButton(GuideStep step)
    {
        var mainPanel = UIManager.Instance.GetPanel<MainPanel>();
        if (mainPanel != null)
        {
            step.SetListenButton(mainPanel.dispatchButton);
        }
    }
    public void GetDispatchBagButton(GuideStep step)
    {
        var dispatchPanel = UIManager.Instance.GetPanel<DispatchPanel>();

        if (dispatchPanel != null)
        {
            step.SetListenButton(dispatchPanel.Bag1Button);
        }
    }

    public void GetDispatchSelectSnackButton(GuideStep step)
    {
        var dispatchPanel = UIManager.Instance.GetPanel<DispatchPanel>();
        if (dispatchPanel != null)
        {
            step.SetListenButton(dispatchPanel.selectSnackButton);
        }
    }

    public void GetDispathcFoodFirstButton(GuideStep step)
    {
        var dispatchPanel = UIManager.Instance.GetPanel<DispatchPanel>();
        if (dispatchPanel != null)
        {
            step.SetListenButton(dispatchPanel.scrollerController_Dispatch_Food.GetFirstButton());
        }
    }
    public void GetPlacementFirstButton(GuideStep step)
    {
        var placementPanel = UIManager.Instance.GetPanel<Placement_Panel>();
        if(placementPanel != null)
        {
            step.SetListenButton(placementPanel.mScroller.GetFirstCell()?.button);
        }
    }
    private GuideStep step;
    public void OnPutPlacement(GuideStep step)
    {
        this.step = step;
        EvtDsp.AddEvt<GridObject>(EvtNames.OnPutPlacement, OnPutPlacement);
    }
    private void OnPutPlacement(GridObject obj)
    {
        if (GuideManager.Instance.currentStep != step) return;
        GuideManager.Instance.NextStep();
        EvtDsp.RemoveEvt<GridObject>(EvtNames.OnPutPlacement, OnPutPlacement);
    }
    
    public void OnClickPlacement(GuideStep step)
    {
        this.step = step;
        EvtDsp.AddEvt<GridObject, string, Vector3>(EvtNames.Open_Edit_Placement_Panel, OnClickPlacement);
    }
    private void OnClickPlacement(GridObject placement, string text, Vector3 pos)
    {
        if (GuideManager.Instance.currentStep != step) return;
        GuideManager.Instance.NextStep();
        EvtDsp.RemoveEvt<GridObject, string, Vector3>(EvtNames.Open_Edit_Placement_Panel, OnClickPlacement);
    }
    public void GetDispatchSelectFoodButton(GuideStep step)
    {
        var DispatchPanel = UIManager.Instance.GetPanel<DispatchPanel>();
        if(DispatchPanel != null)
        {
            step.SetListenButton(DispatchPanel.selectFoodButton);
        }
    }

    public void GetDispatchSelectCDButton(GuideStep step)
    {
        var DispatchPanel = UIManager.Instance.GetPanel<DispatchPanel>();
        if (DispatchPanel != null)
        {
            step.SetListenButton(DispatchPanel.selectCDButton);
        }
    }
    
    public void GetDispatchSelectCDFirstButton(GuideStep step)
    {
        var DispatchPanel = UIManager.Instance.GetPanel<DispatchPanel>();
        if (DispatchPanel != null)
        {
            step.SetListenButton(DispatchPanel.scrollerController_CD.GetFirstButton());
        }
    }


}
