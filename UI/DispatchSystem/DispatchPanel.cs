using System.Collections;
using System.Collections.Generic;
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
using CLIP.Project_Mouse.Kernel;

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
    [Header("确认打包三态：空背包 / 未满三格 / 满三格")]
    [Tooltip("派遣新增_不可点击")]
    public Sprite confirmButtonDisabledSprite;
    [Tooltip("派遣新增_确认打包_非高亮（白突起，1～2 格有物）")]
    public Sprite confirmButtonPartialSprite;
    [Tooltip("派遣新增_确认打包（绿突起，三格齐）")]
    public Sprite confirmButtonFullSprite;
    private int curBagIndex;
    /// <summary>进入「选物」时从 dispatch_Bags 复制；确认打包前不改动 Manager 里对应槽位。</summary>
    private DispatchBagInfo curBagInfo;
    private readonly List<DispatchBagInfo> _bagsViewForWarehouse = new List<DispatchBagInfo>(8);

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
        EvtDsp.AddEvt<int>(EvtNames.Dispatch_Package_Bag_Res, OnPackageBagRes);
        SwitchProcedure(Procedure_Dispatch.SelectBag, false);

    }

    public override void OnDestroy()
    {
        if (Dispatch_Manager._instance != null)
        {
            Dispatch_Manager._instance.ClearDispatchModelPreview();
        }
        Bag1Button.onClick.RemoveAllListeners();
        Bag2Button.onClick.RemoveAllListeners();
        Bag3Button.onClick.RemoveAllListeners();

        exitDispatchButton.onClick.RemoveAllListeners();

        selectFoodButton.onClick.RemoveAllListeners();
        selectSnackButton.onClick.RemoveAllListeners();
        selectCDButton.onClick.RemoveAllListeners();
        UIManager.Instance.UnregisterPanel(typeof(DispatchPanel).Name);
        EvtDsp.RemoveEvt<int>(EvtNames.Dispatch_Package_Bag_Res, OnPackageBagRes);
        EvtDsp.TriggerEvt(EvtNames.OnDispatchPanelClose);
    }

    private void SwitchBag(int bagIndex)
    {
        curBagIndex = bagIndex;
        CopyBagInfoFromManager(bagIndex);
        Dispatch_Manager._instance.SetDispatchModelPreview(curBagIndex, curBagInfo);
        SwitchProcedure(Procedure_Dispatch.SelectFood);
    }

    private void CopyBagInfoFromManager(int bagIndex)
    {
        var bags = Dispatch_Manager._instance.dispatch_Bags;
        if (bags == null || bagIndex < 0 || bagIndex >= bags.Count)
        {
            curBagInfo = new DispatchBagInfo();
            return;
        }

        var src = bags[bagIndex];
        curBagInfo = src == null
            ? new DispatchBagInfo()
            : new DispatchBagInfo
            {
                foodName = src.foodName,
                snackName = src.snackName,
                tapeName = src.tapeName,
                isPacked = src.isPacked
            };
        curBagInfo.SyncPackedFlag();
    }

    /// <summary><see cref="PackageBagRes"/> 回包后同步列表与详情界面。</summary>
    private void OnPackageBagRes(int bagIndex)
    {
        var mgr = Dispatch_Manager._instance;
        if (mgr?.dispatch_Bags == null || bagIndex < 0 || bagIndex >= mgr.dispatch_Bags.Count)
            return;

        if (bagIndex == curBagIndex)
        {
            CopyBagInfoFromManager(bagIndex);
            mgr.SetDispatchModelPreview(curBagIndex, curBagInfo);
        }

        RefreshAllBagListStates();

        if (procedureSelectFood.activeSelf)
        {
            RefreshPanel();
            RefreshDispatchModel();
            scrollerController_Dispatch_Food?.ReloadCurrentIfLoaded();
        }
        else if (procedureSelectCD.activeSelf)
        {
            RefreshPanel();
            RefreshDispatchModel();
            scrollerController_CD?.ReloadData();
        }
    }

    private void RefreshAllBagListStates()
    {
        GetBagState(0);
        GetBagState(1);
        GetBagState(2);
    }

    /// <summary>选物界面预览 / CD 高亮：与 <see cref="Dispatch_Manager.GetBagForModelPreview"/> 一致。</summary>
    public DispatchBagInfo GetPreviewBagInfo(int bagIndex)
    {
        return Dispatch_Manager._instance.GetBagForModelPreview(bagIndex);
    }

    /// <summary>仓库格占用数：全列表里当前背包项用 curBagInfo，其余用 dispatch_Bags。</summary>
    public IReadOnlyList<DispatchBagInfo> GetBagsViewForWarehouse()
    {
        _bagsViewForWarehouse.Clear();
        var bags = Dispatch_Manager._instance.dispatch_Bags;
        for (int i = 0; i < bags.Count; i++)
        {
            if (i == curBagIndex && curBagInfo != null)
            {
                _bagsViewForWarehouse.Add(curBagInfo);
            }
            else
            {
                _bagsViewForWarehouse.Add(bags[i]);
            }
        }
        return _bagsViewForWarehouse;
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
                Dispatch_Manager._instance.ClearDispatchModelPreview();
                procedureSelectBag.SetActive(true);
                exitDispatchButton.onClick.RemoveAllListeners();
                exitDispatchButton.onClick.AddListener(() =>
                {
                    MainPanel.SetPhoneBTNEnable(true);
                    //SceneLoadHelper.Load_MainScene((s) =>EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active));
                    SceneLoadingHelper.Load_MainScene(() => EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active));
                });
                lastBagButton.gameObject.SetActive(false);
                nextBagButton.gameObject.SetActive(false);
                RefreshAllBagListStates();
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
        if (scrollerController_Dispatch_Food != null && scrollerController_Dispatch_Food.gameObject.activeInHierarchy)
        {
            scrollerController_Dispatch_Food.ReloadCurrentIfLoaded();
        }
    }

    /// <summary>
    /// 从当前编辑背包撤回该槽位物品（仅本地派遣数据，不立刻改仓库库存）。
    /// </summary>
    public void ClearDispatchSlot(Item_Type type)
    {
        if (curBagInfo == null)
        {
            return;
        }

        switch (type)
        {
            case Item_Type.Food:
                curBagInfo.foodName = null;
                break;
            case Item_Type.Snack:
                curBagInfo.snackName = null;
                break;
            case Item_Type.Tape:
                curBagInfo.tapeName = null;
                break;
            default:
                return;
        }

        RefreshPanel();
        RefreshDispatchModel();
        if (scrollerController_Dispatch_Food != null && scrollerController_Dispatch_Food.gameObject.activeInHierarchy)
        {
            scrollerController_Dispatch_Food.ReloadCurrentIfLoaded();
        }
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
        var current_info = curBagInfo;
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

        RefreshConfirmButtonState(current_info);
    }

    private static int CountFilledDispatchSlots(DispatchBagInfo info)
    {
        if (info == null)
        {
            return 0;
        }

        int n = 0;
        if (!string.IsNullOrEmpty(info.foodName))
        {
            n++;
        }

        if (!string.IsNullOrEmpty(info.snackName))
        {
            n++;
        }

        if (!string.IsNullOrEmpty(info.tapeName))
        {
            n++;
        }

        return n;
    }

    /// <summary>0 格扁平不可点；1～2 格白突起；3 格绿突起。均可点态下点击即写入 Manager，仍可在本页替换物品。</summary>
    private void RefreshConfirmButtonState(DispatchBagInfo info)
    {
        if (confirmButton == null || confirmButton.image == null)
        {
            return;
        }

        int filled = CountFilledDispatchSlots(info);
        if (filled <= 0)
        {
            confirmButton.interactable = false;
            confirmButton.image.sprite = confirmButtonDisabledSprite;
        }
        else if (filled < 3)
        {
            confirmButton.interactable = true;
            confirmButton.image.sprite = confirmButtonPartialSprite;
        }
        else
        {
            confirmButton.interactable = true;
            confirmButton.image.sprite = confirmButtonFullSprite;
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
        var bags = Dispatch_Manager._instance.dispatch_Bags;
        if (bags == null || index < 0 || index >= bags.Count)
        {
            SetBagState(index, false);
            return "";
        }

        var info = bags[index];
        SetBagState(index, info != null && info.HasAnyFilledSlot());
        return "";
    }
    private void SetPackState()
    {
        if (CountFilledDispatchSlots(curBagInfo) <= 0)
        {
            return;
        }

        PromptManager.ShowUpPrompt(PromptId.DispatchPacked);
        Dispatch_Manager._instance.SendPackageBagRequest(curBagIndex, curBagInfo);

        if (CountFilledDispatchSlots(curBagInfo) >= 3)
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
