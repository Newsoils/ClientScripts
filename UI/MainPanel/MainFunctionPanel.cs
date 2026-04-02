using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Project_Mouse.LYC.TaskSystem;
using CLIP.Project_Mouse.NewFrame.UI;
using CLIP.Project_Mouse.Game_Play_System;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Framework_Core.Event;
using TMPro;

public class MainFunctionPanel : MonoBehaviour
{
    public GameObject objLeft;
    public GameObject objRight;


    public Button AnnoucementButton;
    public Button TaskButton;
    public Button TakePhotoButton;
    public Button HouseButton;

    public Button ShoppingButton;

    public Button TicketButton;
    public TMP_Text TicketNum;
    public GameObject TicketConfirmObj;
    public Button TicketConfimButton;

    public void Start()
    {
        AnnoucementButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenPanel<AnnouncementPanel>();
        });
        HouseButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenPanel<MapPanel>();
        });

        TaskButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OpenPanel<TaskPanel>();
        });
        TakePhotoButton.onClick.AddListener(() =>
        {
            //PhotoInteractManager.Instance.OpenTakePhoto();
            UIManager.Instance.OpenPanel<PhotoCapturePanel>();
        });

        ShoppingButton.onClick.AddListener(() =>
        {
            UIManager.Instance.GetPanel<ShoppingPanel>().OpenPanel();
        });

        TicketButton.onClick.AddListener(() =>
        {
            TicketConfirmObj.SetActive(true);
        });

        TicketConfimButton.onClick.AddListener(() =>
        { 
            if (Dispatch_Manager.IsDispatching)
            {
                TicketConfirmObj.SetActive(false);
                if(Global_Inventory_Manager.ReduceItemCount("爱心车票", 1, "让小苔回家"))
                {
                    Dispatch_Manager._instance.force_dispatch_end();
                    RefreshTicketNum();
                }
                else
                {
                    PromptMessage.Instance.ShowUpPrompt("爱心车票不足");
                }
            }
            else
            {
                EvtDsp.TriggerEvt(EvtNames.Show_Warning_Panel, "小苔没有外出!");
            }
        });
        RefreshTicketNum();
        EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshTicketNum);
    }

    public void OnDestroy()
    {
        AnnoucementButton.onClick.RemoveAllListeners();
        HouseButton.onClick.RemoveAllListeners();
        TaskButton.onClick.RemoveAllListeners();
        TakePhotoButton.onClick.RemoveAllListeners();
        TicketButton.onClick.RemoveAllListeners();
        TicketConfimButton.onClick.RemoveAllListeners();
        EvtDsp.RemoveEvt(EvtNames.RefreshUI, RefreshTicketNum);
    }
    public void SetRight(bool value)
    {
        objRight.SetActive(value);
    }
    public void SetLeft(bool value)
    {
        objLeft.SetActive(value);
    }
    public  void ClosePanel()
    {
        objLeft.SetActive(false);
        objRight.SetActive(false);
    }

    public  void OpenPanel()
    {
        objLeft.SetActive(true);
        objRight.SetActive(true);
    }
    private void RefreshTicketNum()
    {
        TicketNum.text = Global_Inventory_Manager._instance.GetItemNum("爱心车票").ToString();
    }

}
