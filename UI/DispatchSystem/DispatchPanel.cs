using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DispatchPanel : UIPanelBase
{
    public Button bg;

    //public Button random_Button;
    //public Button prepare_Button;
    //public Button force_Start_Button;
    //public Button force_Return_Button;
    //public TMP_Text noticeText;


    public GameObject procedureSelectBag;
    public GameObject procedurePlayAnimation;
    private UIFrameAnimation animation_Player;
    public Animator animator;
    public GameObject procedureSelectFood;
    public GameObject procedureSelectCD;

    public GameObject AddFoodImage;
    public GameObject AddSnackImage;
    public GameObject AddCDImage;

    public Button Bag1Button;
    public TMP_Text Bag1State;
    public GameObject Bag1TextStateIcon1;
    public GameObject Bag1TextStateIcon2;
    public GameObject Bag1StateIcon1;
    public GameObject Bag1StateIcon2;
    public Button Bag2Button;
    public TMP_Text Bag2State;
    public GameObject Bag2TextStateIcon1;
    public GameObject Bag2TextStateIcon2;
    public GameObject Bag2StateIcon1;
    public GameObject Bag2StateIcon2;
    public Button Bag3Button;
    public TMP_Text Bag3State;
    public GameObject Bag3TextStateIcon1;
    public GameObject Bag3TextStateIcon2;
    public GameObject Bag3StateIcon1;
    public GameObject Bag3StateIcon2;
    public Button confirmButton;
    [Header("确认打包按钮高亮图 派遣新增_确认打包")]
    public Sprite confirmButtonHighlightSprite;
    private Sprite confirmButtonNormalSprite;
    private int curBagIndex;
    private DispatchBagInfo curBagInfo;

    public int CurrentBagIndex => curBagIndex;

    public Button selectFoodButton;
    public Button selectCDButton;
    public Button selectSnackButton;

    public Button exitDispatchButton;
    public Button lastBagButton;
    public TMP_Text lastBagText;
    public Button nextBagButton;
    public TMP_Text nextBagText;

    public RectTransform confirmAndScroller;
    public ScrollerController_DispatchItem scrollerController_Dispatch_Food;
    public Image scrollerName;
    public Sprite foodImage;
    public Sprite luckImage;
    public Button hideScrollerButton;
    private Vector2 scrollerOriginPosition;

    public ScrollerController_CD scrollerController_CD;
    public Button confirmCD;

    public GameObject popUpObj;
    public TMP_Text popUpText;
    public CanvasGroup popUpCanvas;
    private Vector3 popOriginPosition;
    private Sequence popTween;
    public void Start()
    {
        popUpObj.SetActive(false);
        popOriginPosition = popUpObj.GetComponent<RectTransform>().anchoredPosition;
        scrollerOriginPosition = confirmAndScroller.anchoredPosition;

        if (confirmButton != null && confirmButton.image != null)
        {
            confirmButtonNormalSprite = confirmButton.image.sprite;
        }

        animation_Player = procedurePlayAnimation.GetComponentInChildren<UIFrameAnimation>();


        MainPanel mainPanel = UIManager.Instance.GetPanel<MainPanel>();

        if(animation_Player == null )
        {
            Log.Error("DispatchPanel: UIFrameAnimation not found in procudurePlayAnimation");
        }


        Bag1Button.onClick.AddListener(() =>
        {
            SwitchBag(0);
        });
        Bag2Button.onClick.AddListener(() =>
        {
            SwitchBag(1);
        });
        Bag3Button.onClick.AddListener(() =>
        {
            SwitchBag(2);
        });
        confirmButton.onClick.AddListener(SetPackState);

        selectFoodButton.onClick.AddListener(() =>
        {
            ShowScroller();
            scrollerName.sprite = foodImage;
            mainPanel.SetPhoneButtonEnable(false);
            scrollerController_Dispatch_Food.ReloadData(Item_Type.Food);
        });

        selectSnackButton.onClick.AddListener(() =>
        {
            ShowScroller();
            scrollerName.sprite = luckImage;
            mainPanel.SetPhoneButtonEnable(false);
            scrollerController_Dispatch_Food.ReloadData(Item_Type.Snack);
        });

        selectCDButton.onClick.AddListener(() =>
        {
           SwitchProcedure(Procedure_Dispatch.SelectCD);
        });

        bg.onClick.AddListener(() =>
        {
            HideScroller();
            mainPanel.SetPhoneButtonEnable(true);
        });
        hideScrollerButton.onClick.AddListener(() =>
        {
            HideScroller();
            mainPanel.SetPhoneButtonEnable(true);
        });

        EvtDsp.TriggerEvt(EvtNames.OnDispatchPanelOpen);
        SwitchProcedure(Procedure_Dispatch.SelectBag, false);

    }

    public override void OnDestroy()
    {
        Bag1Button.onClick.RemoveAllListeners();
        Bag2Button.onClick.RemoveAllListeners();
        Bag3Button.onClick.RemoveAllListeners();

        exitDispatchButton.onClick.RemoveAllListeners();

        selectFoodButton.onClick.RemoveAllListeners();
        selectSnackButton.onClick.RemoveAllListeners();
        selectCDButton.onClick.RemoveAllListeners();
        UIManager.Instance.UnregisterPanel(typeof(DispatchPanel).Name);
        EvtDsp.TriggerEvt(EvtNames.OnDispatchPanelClose);
    }

    private void SwitchBag(int bagIndex)
    {
        //Dispatch_Manager.Instance.SwitchBag(bagIndex);
        curBagInfo = Dispatch_Manager._instance.dispatch_Bags[bagIndex];
        SwitchProcedure(Procedure_Dispatch.SelectFood);
        curBagIndex = bagIndex;
    }

    public void SwitchProcedure(Procedure_Dispatch procedure, bool playAnimation = true)
    {
        StartCoroutine(SwitchProcedureCoroutine(procedure, playAnimation));
    }
    private IEnumerator SwitchProcedureCoroutine(Procedure_Dispatch procedure, bool playAnimation = true)
    {
        
        if(playAnimation)
        {
            procedurePlayAnimation.SetActive(true);
            switch(procedure)
            {
                case Procedure_Dispatch.SelectBag:
                case Procedure_Dispatch.SelectFood:
                    yield return PlayAnimation("start", "start");
                    break;
                case Procedure_Dispatch.SelectCD:
                    yield return PlayAnimation("startCD", "startCD");
                    break;
            }

        }
        procedureSelectBag.SetActive(false);
        procedureSelectCD.SetActive(false);
        procedureSelectFood.SetActive(false);
        scrollerController_CD.gameObject.SetActive(false);
        scrollerController_Dispatch_Food.gameObject.SetActive(false);
        lastBagButton.gameObject.SetActive(false);
        nextBagButton.gameObject.SetActive(false);

        EvtDsp.TriggerEvt<Procedure_Dispatch>(EvtNames.Dispatch_Switch_Procudure, procedure);
        switch (procedure)
        {
            case Procedure_Dispatch.SelectBag:
                procedureSelectBag.SetActive(true);
                exitDispatchButton.onClick.RemoveAllListeners();
                exitDispatchButton.onClick.AddListener(() =>
                {
                    MainPanel.SetPhoneBTNEnable(true);
                    //SceneLoadHelper.Load_MainScene((s) =>EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active));
                    SceneLoadingHelper.Load_MainScene(() => EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active));
                });
                lastBagButton.gameObject.SetActive(true);
                nextBagButton.gameObject.SetActive(true);
                Bag1State.text = GetBagState(0);
                Bag2State.text = GetBagState(1);
                Bag3State.text = GetBagState(2);
                break;
            case Procedure_Dispatch.SelectFood:
                procedureSelectFood.SetActive(true);
                lastBagText.text = "背包" + (curBagIndex);
                nextBagText.text = "背包" + (curBagIndex + 2);
                lastBagButton.gameObject.SetActive(true);
                nextBagButton.gameObject.SetActive(true);
                lastBagButton.onClick.RemoveAllListeners();
                nextBagButton.onClick.RemoveAllListeners();
                lastBagButton.onClick.AddListener(() =>
                {
                    SwitchBag(curBagIndex - 1);
                });
                nextBagButton.onClick.AddListener(() =>
                {
                    SwitchBag(curBagIndex + 1);
                });
                if(curBagIndex == 0)
                {
                    lastBagButton.gameObject.SetActive(false);
                }
                if(curBagIndex == 2)
                {
                    nextBagButton.gameObject.SetActive(false);
                }
                //这里刷新一下显示的物品模型
                RefreshDispatchModel();

                RefreshPanel();
                scrollerController_Dispatch_Food.ReloadData(Item_Type.Food);
                exitDispatchButton.onClick.RemoveAllListeners();
                exitDispatchButton.onClick.AddListener(() =>
                {
                    SwitchProcedure(Procedure_Dispatch.SelectBag);
                });
                RectTransform rectTransform = scrollerController_Dispatch_Food.GetComponent<RectTransform>();
                confirmAndScroller.anchoredPosition = scrollerOriginPosition - new Vector2(0, rectTransform.rect.height);
                break;
            case Procedure_Dispatch.SelectCD:
                procedureSelectCD.SetActive(true);
                scrollerController_CD.gameObject.SetActive(true);
                scrollerController_CD.ReloadData();
                RefreshDispatchModel();
                RefreshPanel();
                exitDispatchButton.onClick.RemoveAllListeners();
                confirmCD.onClick.RemoveAllListeners();
                exitDispatchButton.onClick.AddListener(() =>
                {
                    SwitchProcedure(Procedure_Dispatch.SelectFood);
                });
                confirmCD.onClick.AddListener(() =>
                {
                    SwitchProcedure(Procedure_Dispatch.SelectFood);
                });
                break;
        }
        if (playAnimation)
        {
            switch (procedure)
            {
                case Procedure_Dispatch.SelectBag:
                case Procedure_Dispatch.SelectFood:
                    yield return PlayAnimation("end", "end");
                    break;
                case Procedure_Dispatch.SelectCD:
                    yield return PlayAnimation("endCD", "endCD");
                    break;
            }
            procedurePlayAnimation.SetActive(false);
        }
    }
    public void SetDispatchItem(Item_Type type, string name)
    {
        switch (type)
        {
            case Item_Type.Food:
                curBagInfo.foodName = name;
                break;
            case Item_Type.Snack:
                curBagInfo.snackName = name;
                break;
            case Item_Type.Tape:
                curBagInfo.tapeName = name;
                break;
        }
        RefreshPanel();
        RefreshDispatchModel();
    }
    private IEnumerator PlayAnimation(string triggerName, string animationName)
    {
        animator.SetBool(triggerName, true);
        yield return null;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        while (!info.IsName(animationName) || info.normalizedTime < 1.0)
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        animator.SetBool(triggerName, false);
        Debug.Log("动画播放完毕");
    }
    public void RefreshPanel()
    {
        var current_info = Dispatch_Manager._instance.dispatch_Bags[curBagIndex];
        if (string.IsNullOrEmpty(current_info.foodName))
        {
            AddFoodImage.SetActive(true);
        }
        else
        {
            AddFoodImage.SetActive(false);
        }
        if(string.IsNullOrEmpty(current_info.snackName))
        {
            AddSnackImage.SetActive(true);
        }
        else
        {
            AddSnackImage.SetActive(false);
        }
        if(string.IsNullOrEmpty(current_info.tapeName))
        {
            AddCDImage.SetActive(true);
        }
        else
        {
            AddCDImage.SetActive(false);
        }

        RefreshConfirmButtonHighlight(current_info);
    }

    // 三个物品（food / snack / tape）都装填后，确认按钮才切到高亮 sprite
    private void RefreshConfirmButtonHighlight(DispatchBagInfo info)
    {
        if (confirmButton == null || confirmButton.image == null)
        {
            return;
        }

        bool isFull = info != null
                      && !string.IsNullOrEmpty(info.foodName)
                      && !string.IsNullOrEmpty(info.snackName)
                      && !string.IsNullOrEmpty(info.tapeName);

        if (isFull && confirmButtonHighlightSprite != null)
        {
            confirmButton.image.sprite = confirmButtonHighlightSprite;
        }
        else if (confirmButtonNormalSprite != null)
        {
            confirmButton.image.sprite = confirmButtonNormalSprite;
        }
    }
    private void ShowScroller()
    {
        scrollerController_Dispatch_Food.gameObject.SetActive(true);
        confirmAndScroller.anchoredPosition = scrollerOriginPosition - new Vector2(0, 150);
        confirmAndScroller.DOAnchorPos(scrollerOriginPosition, 0.4f).SetEase(Ease.OutBack);
    }
    private void HideScroller()
    {
        RectTransform rectTransform = scrollerController_Dispatch_Food.GetComponent<RectTransform>();
        confirmAndScroller.DOAnchorPos(scrollerOriginPosition - new Vector2(0, rectTransform.rect.height), 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            scrollerController_Dispatch_Food.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// 通知Dispatch_Procudure_Controller
    /// </summary>
    private void RefreshDispatchModel()
    {
        EvtDsp.TriggerEvt<int>(EvtNames.Dispatch_Refresh_Model, curBagIndex);
    }

    public override void OpenPanel(params object[] data)
    {
       
    }

    public override void ClosePanel()
    {
        
    }

    public override void UpdatePanel(params object[] data)
    {
        
    }


    private void SetBagState(int index,bool isPacked)
    {
        switch (index)
        {
            case 0:
                Bag1TextStateIcon1.SetActive(isPacked);
                Bag1TextStateIcon2.SetActive(!isPacked);
                Bag1StateIcon1.SetActive(isPacked);
                Bag1StateIcon2.SetActive(!isPacked);
                break;
            case 1:
                Bag2TextStateIcon1.SetActive(isPacked);
                Bag2TextStateIcon2.SetActive(!isPacked);
                Bag2StateIcon1.SetActive(isPacked);
                Bag2StateIcon2.SetActive(!isPacked);
                break;
            case 2:
                Bag3TextStateIcon1.SetActive(isPacked);
                Bag3TextStateIcon2.SetActive(!isPacked);
                Bag3StateIcon1.SetActive(isPacked);
                Bag3StateIcon2.SetActive(!isPacked);
                break;
            default:
                break;
        }

    }

    private string GetBagState(int index)
    {
        var info = Dispatch_Manager._instance.dispatch_Bags[index];
        SetBagState(index, info.isPacked);
        if (info.isPacked)
        {
            return "";
        }
        else
        {
            return "";
        }
    }
    private void SetPackState()
    {
        if(string.IsNullOrEmpty(curBagInfo.foodName)||string.IsNullOrEmpty(curBagInfo.snackName))
        {
            ShowPopUp("食物或幸运小物未装填，打包失败！");
            return;
        }

        ShowPopUp("打包​完毕，​小苔随时​可能​出门​哦！");
        Dispatch_Manager._instance.SetBagContent(curBagIndex, curBagInfo);

        // 三样齐全（= 确认按钮处于高亮态）时，直接跳回选择背包界面，不播放过渡动画
        bool isFullyPacked = !string.IsNullOrEmpty(curBagInfo.foodName)
                             && !string.IsNullOrEmpty(curBagInfo.snackName)
                             && !string.IsNullOrEmpty(curBagInfo.tapeName);
        if (isFullyPacked)
        {
            SwitchProcedure(Procedure_Dispatch.SelectBag, false);
        }
    }
    private void ShowPopUp(string text)
    {
        popTween?.Kill();
        popUpObj.SetActive(true);
        popUpText.text = text;
        popUpObj.GetComponent<RectTransform>().anchoredPosition = popOriginPosition + new Vector3(0, 30, 0);
        popUpCanvas.alpha = 0;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(popUpObj.GetComponent<RectTransform>().DOAnchorPos(popOriginPosition, 0.3f));
        sequence.Join(popUpCanvas.DOFade(1, 0.3f));
        sequence.AppendInterval(2f);
        sequence.Append(popUpCanvas.DOFade(0, 0.3f).OnComplete(() =>
        {
            popUpObj.gameObject.SetActive(false);
        }));
        popTween = sequence;
    }
}
