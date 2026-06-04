using System;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TransitionVideoPanel : UIPanelBase
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("UI")]
    public GameObject obj;
    public CanvasGroup canvasGroup;
    public RawImage videoImage;
    public Button btnSkip;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public float skipEnableDelay = 0.5f;

    private TaskCompletionSource<bool> _tcs;
    private bool _isSkipped;
    private bool _isPlaying;

    public  bool IsPlaying => _isPlaying;

    private void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        videoImage = GetComponentInChildren<RawImage>(true);
        if (videoImage != null && videoPlayer.targetTexture != null)
            videoImage.texture = videoPlayer.targetTexture;

        if (btnSkip != null)
        {
            btnSkip.onClick.RemoveAllListeners();
            btnSkip.onClick.AddListener(OnSkipClicked);
        }

        EvtDsp.AddEvt<VideoClip>(EvtNames.PlayTransitionVideo, PlayVideo);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        videoPlayer.loopPointReached -= OnLoopPointReached;
        EvtDsp.RemoveEvt<VideoClip>(EvtNames.PlayTransitionVideo, PlayVideo);
    }

    public void PlayVideo(VideoClip clip)
    {
        OpenPanel();
        EvtDsp.TriggerEvt(EvtNames.Audio_Stop);
        _ = PlayAsync(clip);
    }

    public async Task PlayAsync(VideoClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[TransitionVideoPanel] VideoClip is null, skip playback.");
            return;
        }

        _isSkipped = false;
        _isPlaying = false;
        _tcs = new TaskCompletionSource<bool>();

        obj.SetActive(true);
        canvasGroup.alpha = 0f;
        videoImage.gameObject.SetActive(false);
        videoImage.texture = videoPlayer.targetTexture;
        if (btnSkip != null)
        {
            btnSkip.gameObject.SetActive(false);
            btnSkip.transform.localScale = Vector3.zero;
        }

        // 先取消旧回调，防止 Start() 时 Prepare() 已完成导致 prepareCompleted 立即同步触发
        videoPlayer.prepareCompleted -= OnPrepareCompleted;

        videoPlayer.clip = clip;
        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.Prepare();

        await _tcs.Task;

        canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
        {
            videoPlayer.Stop();
            videoImage.gameObject.SetActive(false);
            obj.SetActive(false);
        });
    }

    private void OnPrepareCompleted(VideoPlayer vp)
    {
        if (_isSkipped) return;

        _isPlaying = true;

        videoImage.gameObject.SetActive(true);
        canvasGroup.DOFade(1f, fadeInDuration).OnComplete(() =>
        {
            if (_isSkipped) return;

            videoPlayer.loopPointReached += OnLoopPointReached;
            videoPlayer.Play();

            if (btnSkip != null)
            {
                btnSkip.gameObject.SetActive(true);
                btnSkip.transform.localScale = Vector3.zero;
                btnSkip.transform.DOScale(1f, 0.15f).SetDelay(skipEnableDelay);
            }
        });
    }

    private void OnLoopPointReached(VideoPlayer vp)
    {
        Finish();
    }

    private void OnSkipClicked()
    {
        if (_isSkipped) return;
        _isSkipped = true;

        if (btnSkip != null)
            btnSkip.transform.DOScale(0f, 0.1f);

        Finish();
    }

    private void Finish()
    {
        videoPlayer.loopPointReached -= OnLoopPointReached;
        _tcs?.TrySetResult(true);
        EvtDsp.TriggerEvt(EvtNames.OnTransitionVideoFinished);
    }

    public override void OpenPanel(params object[] data)
    {
        obj.SetActive(true);
    }

    public override void ClosePanel()
    {
        obj.SetActive(false);
    }

    public override void UpdatePanel(params object[] data)
    {
    }
}
