using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.UI;

using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Framework_Core.Event;
using TMPro;
using CLIP.Project_Mouse.Kernel;

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

    [Tooltip("可选。任务按钮上的红点 Image，在 prefab 里挂好组件并拖 `未读红点` 素材；不绑则不做红点显隐。")]
    [SerializeField]
    Image taskClaimableRedDot;

    private void RefreshTaskClaimableRedDot()
    {
        if (taskClaimableRedDot == null)
            return;
        var mgr = MissionManager.Instance;
        bool show = mgr != null && mgr.HasClaimableReward();
        taskClaimableRedDot.gameObject.SetActive(show);
    }

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
                Dispatch_Manager._instance.SendCatBackImmediatelyRequest();
            }
            else
            {
                PromptManager.ShowWarning(PromptId.CatNotAway);
            }
        });
        RefreshTicketNum();
        EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshTicketNum);
        EvtDsp.AddEvt(EvtNames.Task_ClaimableChanged, RefreshTaskClaimableRedDot);
        RefreshTaskClaimableRedDot();
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
        EvtDsp.RemoveEvt(EvtNames.Task_ClaimableChanged, RefreshTaskClaimableRedDot);
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
        TicketNum.text = Global_Inventory_Manager.Instance.GetItemNum("爱心车票").ToString();
    }

}
