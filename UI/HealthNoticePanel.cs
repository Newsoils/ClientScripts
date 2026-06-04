using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HealthNoticePanel : UIPanelBase
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public TMP_Text contentText;

    [Header("Timing")]
    public float fadeDuration = 1.5f;
    public float stayDuration = 2f;

    [Header("Skip")]
    public float skipEnableDelay = 1f;

    [Header("Callback")]
    public UnityEvent onFinish;


    private TaskCompletionSource<bool> tcs;

    public async Task PlayAsync()
    {
        tcs = new TaskCompletionSource<bool>();

        Play(); // 原来的 DOTween 播放逻辑

        await tcs.Task;
    }

    private void OnComplete()
    {
        tcs?.TrySetResult(true);

        gameObject.SetActive(false);
    }


    public override void Awake()
    {
        base.Awake();
        canvasGroup.alpha = 0;
    }

    private void Start()
    {
        Play();
    }

    public void Play()
    {
         DOTween.Sequence().Append(canvasGroup.DOFade(1, fadeDuration))   // 淡入
                .AppendInterval(stayDuration)                  // 停留
                .Append(canvasGroup.DOFade(0, fadeDuration))   // 淡出
                .OnComplete(OnComplete);

    }

    public override void OpenPanel(params object[] data)
    {
    }

    public override void ClosePanel()
    {
    }
}