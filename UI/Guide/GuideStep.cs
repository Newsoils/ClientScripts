using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class GuideStep : MonoBehaviour
{
    private RectTransform highlightTarget; // 高亮UI
    public GameObject guideImage;         // 教程图片

    public Button listenButton; // 监听的按钮

    public bool autoTrackButton = false; // 是否自动追踪按钮位置（如果listenButton不为空）

    private Button extraListenButton = null;
    public UnityEvent<GuideStep> ResolveListenButton;

    private Action onClickEvent;

    public event Action OnStepCompleted;
    public string StepId => name;

    //// 引导超时时间（秒，可Inspector配置）
    //public float waitTimeout = 10f;
    //private Coroutine _timeoutCoroutine;

    public bool IsReady => listenButton != null || extraListenButton != null;


    private void Start()
    {
        highlightTarget = guideImage.GetComponent<RectTransform>();
    }
    public void Show(GuideMask mask)
    {
        if (autoTrackButton)
        {
                highlightTarget ??= (listenButton != null ? listenButton.GetComponent<RectTransform>() : null)
                                ?? (extraListenButton != null ? extraListenButton.GetComponent<RectTransform>() : null)
                                ?? guideImage.GetComponent<RectTransform>();
        
        }
        else
        {
            highlightTarget = guideImage.GetComponent<RectTransform>();
        }



        if (guideImage != null)
            guideImage.SetActive(true);

        if (highlightTarget != null)
            mask?.Show(highlightTarget);

        // 1. 绑定空洞点击事件（核心：点击即下一步）
        //GuideManager.Instance.mask.OnHoleClicked.AddListener(OnClick);

        // 2. 启动超时兜底（防止玩家不操作）
        //_timeoutCoroutine = StartCoroutine(TimeoutNextStep());

        ResolveListenButton?.Invoke(this);

        if (extraListenButton != null)
        {
            //extraListenButton.onClick.RemoveListener(OnClick);
            if (extraListenButton is ZButton zButton)
            {
                zButton.onPointDown.AddListener(OnClick);
            }
            extraListenButton.onClick.AddListener(OnClick);
        }

        if (listenButton != null)
        {
            listenButton.onClick.RemoveListener(OnClick);
            listenButton.onClick.AddListener(OnClick);
        }

    }



    public void SetListenButton(Button button)
    {
        if (button != null)
        {
            extraListenButton = button;
        }
    }

    public void SetClickEvent(Action action)
    {
        onClickEvent = action;
    }



    // 超时自动下一步（兜底）
    //private IEnumerator TimeoutNextStep()
    //{
    //    yield return new WaitForSeconds(waitTimeout);
    //    if (extraListenButton != null)
    //    {
    //        extraListenButton.onClick.Invoke();
    //    }
    //    if (listenButton != null)
    //    {
    //        listenButton.onClick.Invoke();
    //    }
    //    if(listenButton == null && extraListenButton == null)
    //    {
    //        OnClick();
    //    }
    //}

    public void Hide()
    {
        if (guideImage != null)
            guideImage.SetActive(false);


        //GuideManager.Instance.mask.OnHoleClicked.RemoveListener(()=> onClickEvent?.Invoke());
        //OnHide?.Invoke();
        if (listenButton != null)
        {
            listenButton.onClick.RemoveListener(OnClick);
        }
        if (extraListenButton != null)
        {
            //extraListenButton.onClick.RemoveListener(OnClick);
            if (extraListenButton is ZButton zButton)
            {
                zButton.onPointDown.RemoveListener(OnClick);
            }
            extraListenButton.onClick.RemoveListener(OnClick);
            extraListenButton = null;
        }
    }

    void OnClick()
    {
        OnStepCompleted?.Invoke();
    }
}
