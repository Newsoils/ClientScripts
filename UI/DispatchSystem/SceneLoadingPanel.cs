using System;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Framework_Core.Event;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 场景加载进度条面板，完全通过 EvtDsp 事件驱动，不被任何外部持有引用。
    /// 订阅：SceneLoading_Open / SceneLoading_Progress / SceneLoading_Close
    /// </summary>
    public class SceneLoadingPanel : UIPanelBase
    {
        [Header("UI 组件")]
        public CanvasGroup canvasGroup;
        public Slider progressSlider;

        public GameObject obj;

        [Header("动画参数")]
        [Tooltip("渐入耗时（秒）")]
        public float fadeInDuration = 0.3f;
        [Tooltip("渐出耗时（秒）")]
        public float fadeOutDuration = 0.3f;
        [Tooltip("Slider 平滑插值速度")]
        public float sliderSmoothSpeed = 8f;

        private float _targetProgress = 0f;
        private float _displayProgress = 0f;
        private bool _isFadingOut = false;

        private Action _onOpenHandler;
        private Action _onCloseHandler;


        private void Start()
        {
            DontDestroyOnLoad(canvasGroup.gameObject);
            RegisterEvents();
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
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

        //protected override void OnEnable()
        //{
        //    base.OnEnable();
        //    RegisterEvents();
        //}

        private void OnDisable()
        {
            UnregisterEvents();
        }

        public override void OnDestroy()
        {
            UnregisterEvents();
            StopAllCoroutines();
            base.OnDestroy();
        }

        // ================================================================
        // 事件注册
        // ================================================================

        private void RegisterEvents()
        {
            EvtDsp.AddEvt(EvtNames.SceneLoading_Open, OnSceneLoadingOpen);
            EvtDsp.AddEvt<float>(EvtNames.SceneLoading_Progress, OnSceneLoadingProgress);
            EvtDsp.AddEvt(EvtNames.SceneLoading_Close, OnSceneLoadingClose);
        }

        private void UnregisterEvents()
        {
            EvtDsp.RemoveEvt(EvtNames.SceneLoading_Open, OnSceneLoadingOpen);
            EvtDsp.RemoveEvt<float>(EvtNames.SceneLoading_Progress, OnSceneLoadingProgress);
            EvtDsp.RemoveEvt(EvtNames.SceneLoading_Close, OnSceneLoadingClose);
        }

        // ================================================================
        // 事件处理
        // ================================================================

        private void OnSceneLoadingOpen()
        {
            obj.SetActive(true);
            _isFadingOut = false;
            _targetProgress = 0f;
            _displayProgress = 0f;
            if (progressSlider != null) progressSlider.value = 0f;
            StartCoroutine(FadeInCoroutine());
        }

        private void OnSceneLoadingProgress(float progress)
        {
            _targetProgress = Mathf.Clamp01(progress);
        }

        private void OnSceneLoadingClose()
        {
            if (_isFadingOut) return;
            _isFadingOut = true;
            StartCoroutine(FadeOutCoroutine());
        }

        // ================================================================
        // Update & 动画
        // ================================================================

        private void Update()
        {
            if (progressSlider == null || _isFadingOut) return;

            _displayProgress = Mathf.Lerp(_displayProgress, _targetProgress, Time.deltaTime * sliderSmoothSpeed);
            _displayProgress = Mathf.Clamp01(_displayProgress);
            progressSlider.value = _displayProgress;
        }

        private System.Collections.IEnumerator FadeInCoroutine()
        {
            if (canvasGroup == null) yield break;
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private System.Collections.IEnumerator FadeOutCoroutine()
        {
            yield return new WaitForSeconds(1f); // 等待一段时间，确保玩家能看到满格的进度条
            if (canvasGroup == null)
            {
                obj.SetActive(false);
                yield break;
            }

            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            obj.SetActive(false);
            _isFadingOut = false;
        }
    }
}
