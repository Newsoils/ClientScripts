using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Framework_Unity;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 场景加载助手，纯业务逻辑层，不持有任何 UI 程序集引用。
    /// 通过 EvtDsp 事件驱动 Loading UI 面板的显示/进度/隐藏。
    /// </summary>
    public class SceneLoadingHelper : SingletonMono<SceneLoadingHelper>
    {
        private Action _onSceneLoaded;

        public void LoadScene(string sceneName, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                onLoaded?.Invoke();
                return;
            }

            _onSceneLoaded = onLoaded;
            EvtDsp.TriggerEvt(EvtNames.SceneLoading_Open);
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            yield return new WaitForSeconds(0.05f);

            var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (asyncOp == null)
            {
                Debug.LogError($"[SceneLoadingHelper] Failed to load scene: {sceneName}");
                yield break;
            }

            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                float progress = asyncOp.progress;
                EvtDsp.TriggerEvt<float>(EvtNames.SceneLoading_Progress, progress);
                yield return null;
            }

            EvtDsp.TriggerEvt<float>(EvtNames.SceneLoading_Progress, 1f);
            yield return new WaitForSeconds(0.2f);

            asyncOp.allowSceneActivation = true;
            yield return new WaitUntil(() => asyncOp.isDone);

            yield return new WaitForSeconds(0.3f);

            EvtDsp.TriggerEvt(EvtNames.SceneLoading_Close);

            _onSceneLoaded?.Invoke();
            _onSceneLoaded = null;
        }

        // ================================================================
        // 兼容旧 SceneLoadHelper 的静态入口
        // ================================================================

        public static void Load_MainScene(Action callback = null)
        {
            Instance.LoadScene(SceneLoadHelper.MainSceneName, callback);
        }

        public static void Load_DispatchScene(Action callback = null)
        {
            Instance.LoadScene(SceneLoadHelper.DispatchSceneName, callback);
        }

        public static void LoadLoginScene(Action callback = null)
        {
            Instance.LoadScene(SceneLoadHelper.LoginScene, callback);
        }
    }
}
