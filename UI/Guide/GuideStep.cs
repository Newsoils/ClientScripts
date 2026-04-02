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

    private Button extraListenButton = null;
    public UnityEvent<GuideStep> ResolveListenButton;

    public bool IsReady => listenButton != null || extraListenButton != null;

    private void Start()
    {
        highlightTarget = guideImage.GetComponent<RectTransform>();
        //ResolveListenButton?.Invoke(this);
    }
    public void Show()
    {
        if (highlightTarget == null)
            highlightTarget = guideImage.GetComponent<RectTransform>();

        if (guideImage != null)
            guideImage.SetActive(true);

        if (highlightTarget != null)
            GuideManager.Instance.mask.Show(highlightTarget);

        if (listenButton != null)
        {
            listenButton.onClick.RemoveListener(OnClick);
            listenButton.onClick.AddListener(OnClick);
        }

        if (extraListenButton != null)
        {
            extraListenButton.onClick.AddListener(OnClick);
        }
    }

    public void SetListenButton(Button button)
    {
        if (button != null)
        {
            extraListenButton = button;
        }
    }

    public void Hide()
    {
        if (guideImage != null)
            guideImage.SetActive(false);

        //OnHide?.Invoke();
        if (listenButton != null)
        {
            listenButton.onClick.RemoveListener(OnClick);
        }
        if (extraListenButton != null)
        {
            extraListenButton.onClick.RemoveListener(OnClick);
            extraListenButton = null;
        }
    }

    void OnClick()
    {
        GuideManager.Instance.NextStep();
    }
}