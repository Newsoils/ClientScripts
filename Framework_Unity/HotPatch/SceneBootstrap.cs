using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using GameCoreResourceLoad;
using UnityEngine.Rendering;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif


namespace GameCoreSceneAndWorld
{
    public class SceneBootstrap : MonoBehaviour
    {
        static private SceneBootstrap boot = null;

        public SubSceneLoadInfo this[string sceneName] { get { return GetSomeLoadedSubScene(sceneName); } }

        //当前激活的主场景-
        static public Scene mainScene;


        //查询某个已经加载子场景-
        static public SubSceneLoadInfo GetSomeLoadedSubScene(string sceneName) { if (boot != null) { boot._GetSomeLoadedSubScene(sceneName); } return null; }


        //-
        public static void LoadScene(string sceneName,
            Action<string> sceneLoadedCallBackFunc = null,
            Action<string> oldSceneUnloadBeforeCallBackFunc = null)
        {
            boot.LoadSceneInner(sceneName, oldSceneUnloadBeforeCallBackFunc, sceneLoadedCallBackFunc);
        }

        //获得场景加载状态-
        public static int LoadSceneState => boot != null ? boot.loadSceneState : 0;


        //-
        static public void LoadSubScene(string sceneName,
            Action<string> sceneLoadedCallBackFunc = null,
            Action<string> oldSceneUnloadBeforeCallBackFunc = null)
        {
            boot.LoadSubSceneInner(sceneName, oldSceneUnloadBeforeCallBackFunc, sceneLoadedCallBackFunc);
        }


        static public void UnloadSubScene(string sceneName)
        {
            boot.UnloadSubSceneInner(sceneName);
        }


        //-
        static public void UnloadAllSubScenes(string[] sideSceneName, Action<string> unloadSubSceneBefore = null, Action unloadAllSubScenesComplete = null)
        {
            if (boot != null) { boot._UnloadAllSubScenes(sideSceneName, unloadSubSceneBefore, unloadAllSubScenesComplete); }
        }


        static public void LoadAllSubScenes(string[] allSceneNames, Action<string> loadSubSceneAfter = null, Action loadAllSubScenesComplete = null)
        {
            if (boot != null) { boot._LoadAllSubScenes(allSceneNames, loadSubSceneAfter, loadAllSubScenesComplete); }
        }


        //-
        static public void CancelAllSubSceneDoing(Action doneFunc)
        {
            if (boot != null) { boot._CancelAllSubSceneDoing(doneFunc); }
        }



        //-
        private void Awake()
        {
            boot = this;

            mainScene = SceneManager.GetActiveScene();
        }

        //流程-
        private void Update()
        {
            _LoadAllSubScenesUpdate();
            _UnloadAllSubScenesUpdate();
            _CancelAllSubSceneDoingUpdate();
        }


        private void OnDestroy()
        {
            boot = null;
        }



        //关于场景加载-
        private int                 loadSceneState = 0;
        private AsyncOperation      sceneOpr = null;
        private string              loadSceneName = "";
        private string              prevLoadSceneName = "";
        private Action<string>      loadSceneOverFunc = null;
        private Action<string>      oldSceneUnloadBeforeFunc = null;

        private void LoadSceneInner(string sceneName, Action<string> oldSceneUnloadBeforeCallBackFunc, Action<string> sceneLoadedCallBackFunc)
        {
            oldSceneUnloadBeforeFunc = oldSceneUnloadBeforeCallBackFunc;
            loadSceneOverFunc = sceneLoadedCallBackFunc;
            prevLoadSceneName = loadSceneName;
            loadSceneName = sceneName;
            loadSceneState = 1;

            //sceneName += ".scene";
            ResourceMgr.LoadSceneFromFileAnsy(sceneName,
                (AssetBundle ab) =>
                {
                    //-
                    mSubSceneLoadInfos.Clear();
                    StartCoroutine("AsyncLoadScene");
                });
        }


        private IEnumerator AsyncLoadScene()
        {
            //加载完成回调-
            OnOldSceneUnloadBefore(prevLoadSceneName);
            
            //WorldBootstrap.LoadSceneAction(loadSceneName);
            #if UNITY_EDITOR
                if(ResourceMgr.LATMode == ResourceMgr.LoadAccessType.FromAssetDatabase)
                {
                    var parameters = new LoadSceneParameters(LoadSceneMode.Single);
                    sceneOpr = EditorSceneManager.LoadSceneAsyncInPlayMode(ResourceMgr.RES_FIND_PATHS_ASSETDATABASE_SCENE[0] + loadSceneName + ".unity", parameters);
                } else{
                    sceneOpr = SceneManager.LoadSceneAsync(loadSceneName);
                }
            #else
                sceneOpr = SceneManager.LoadSceneAsync(loadSceneName);
            #endif
            
            if (sceneOpr == null) { yield break; }

            //这里设置自动跳转版本先-
            sceneOpr.allowSceneActivation = true;
            while (!sceneOpr.isDone)         //异步知道场景加载完成-
            {
                yield return sceneOpr;
            }

            ////这是非自动跳转的版本-
            //sceneOpr.allowSceneActivation = false;
            //while (!sceneOpr.isDone)
            //{
            //    if (sceneOpr.progress < 0.9f) { progressValue = sceneOpr.progress; }
            //    else { progressValue = 1.0f; }

            //    slider.value = progressValue;
            //    progress.text = (int)(slider.value * 100) + " %";

            //    if (progressValue >= 0.9)
            //    {
            //        progress.text = "按任意键继续";

            //        if (Input.anyKeyDown) { sceneOpr.allowSceneActivation = true; }
            //    }

            //    yield return sceneOpr;
            //}

            mainScene = SceneManager.GetActiveScene();

            //加载完成回调-
            OnSceneLoaded(loadSceneName);

            ////删除旧的entityworld-
            //WorldBootstrap.DestroySceneAction();

            //销毁assetbundle-
            ResourceMgr.ClearAssetBundle(loadSceneName, false);

            loadSceneState = 2;
        }


        //-
        protected void OnOldSceneUnloadBefore(string sceneName)
        {
            if (oldSceneUnloadBeforeFunc != null)
            {
                oldSceneUnloadBeforeFunc(sceneName);
                oldSceneUnloadBeforeFunc = null;
            }
        }


        //-       
        protected void OnSceneLoaded(string sceneName)
        {
            if (loadSceneOverFunc != null)
            {
                loadSceneOverFunc(sceneName);
                loadSceneOverFunc = null;
            }
        }




        //关于子场景的加载-
        public enum SubSceneLoadState
        {
            None = 0,
            Loading = 1,
            Unloading = 2,
        }

        public class SubSceneLoadInfo
        {
            public string loadSceneName;

            public UnityEngine.Object ab_scene;
            public AsyncOperation sceneOpr;

            public Action<string> loadSceneOverFunc;
            public Action<string> oldSceneUnloadBeforeFunc;

            public SubSceneLoadState state = SubSceneLoadState.None;
        }

        protected Dictionary<string, SubSceneLoadInfo> mSubSceneLoadInfos = new Dictionary<string, SubSceneLoadInfo>(0);



        //查询某个已经加载子场景-
        private SubSceneLoadInfo _GetSomeLoadedSubScene(string sceneName)
        {
            SubSceneLoadInfo sceneLoadInfo = null;

            if (mSubSceneLoadInfos.ContainsKey(sceneName))
            {
                sceneLoadInfo = mSubSceneLoadInfos[sceneName];
                if (sceneLoadInfo.state != SubSceneLoadState.None) { sceneLoadInfo = null; }
            }

            return sceneLoadInfo;
        }


        //取消所有的子场景加载和卸载(包括正在进行中的)-
        private int mFlagApplyCancelAllSubScene = 0;
        private Action mCancelAllSubSceneFunc = null;
        private void _CancelAllSubSceneDoing(Action doneFunc)
        {
            mFlagApplyCancelAllSubScene = 1;
            mCancelAllSubSceneFunc = doneFunc;
        }

        private void _CancelAllSubSceneDoingUpdate()
        {
            if (mFlagApplyCancelAllSubScene > 0)
            {
                //父场景没有在切换时候执行-
                if (sceneOpr == null || sceneOpr.isDone)
                {
                    mLoadAllSubScenes = 0;
                    mSubReadyLoadSceneList.Clear();
                    mLoadSubSceneAfter = null;
                    mLoadAllSubScenesComplete = null;

                    mReleaseScene = 0;
                    mLoadedSceneList.Clear();
                    mSideSceneName.Clear();
                    mUnloadSubSceneBefore = null;
                    mUnloadAllSubScenesComplete = null;

                    StopCoroutine("AsyncLoadSubScene");
                    StopCoroutine("AsyncUnloadSubScene");

                    mFlagApplyCancelAllSubScene = 0;

                    //校对一下目前存储的已经加载的场景是否还存在-
                    List<string> mTempName = new List<string>(4);

                    foreach (var si in mSubSceneLoadInfos.Values) {  if (si.state == SubSceneLoadState.Unloading) { mTempName.Add(si.loadSceneName); } }
                    foreach (var name in mTempName) { mSubSceneLoadInfos.Remove(name); }

                    if (mCancelAllSubSceneFunc != null) { mCancelAllSubSceneFunc(); }
                }
            }
        }


        #region 加载子场景-
        //加载子场景-
        private void LoadSubSceneInner(string sceneName, Action<string> oldSceneUnloadBeforeCallBackFunc, Action<string> sceneLoadedCallBackFunc)
        {
            if (string.IsNullOrEmpty(sceneName)) { return; }
            if (mSubSceneLoadInfos.ContainsKey(sceneName)) { if (sceneLoadedCallBackFunc != null) { sceneLoadedCallBackFunc(sceneName); } return; }



            //sceneName += ".scene";
            ResourceMgr.LoadSceneFromFileAnsy(sceneName,
            (AssetBundle ab) =>
            {
                SubSceneLoadInfo ssli = new SubSceneLoadInfo();
                ssli.loadSceneName = sceneName;
                ssli.oldSceneUnloadBeforeFunc = oldSceneUnloadBeforeCallBackFunc;
                ssli.loadSceneOverFunc = sceneLoadedCallBackFunc;

                ssli.state = SubSceneLoadState.Loading;
                mSubSceneLoadInfos.Add(ssli.loadSceneName, ssli);

                StartCoroutine("AsyncLoadSubScene", ssli);
            });
        }

        private IEnumerator AsyncLoadSubScene(SubSceneLoadInfo info)
        {
            //加载场景前先加载关于他们的依赖(尝试失败)-
            if (ResourceMgr.LATMode == ResourceMgr.LoadAccessType.FromAssetBundle)
            {
                AssetBundle[] bb;
                bool loadDependenceComplete = false;
                ResourceMgr.LoadMainlandDependenceBundleAnsy(info.loadSceneName, (AssetBundle[] abs) =>
                {
                    loadDependenceComplete = true;
                    bb = abs;
                });
                while (!loadDependenceComplete) { yield return null; }
            }


            //加载完成回调-
            OnOldSubSceneUnloadBefore(info);

            #if UNITY_EDITOR
                if(ResourceMgr.LATMode == ResourceMgr.LoadAccessType.FromAssetDatabase)
                {
                    var parameters = new LoadSceneParameters(LoadSceneMode.Additive);
                    info.sceneOpr = EditorSceneManager.LoadSceneAsyncInPlayMode(ResourceMgr.RES_FIND_PATHS_ASSETDATABASE_SCENE[0] + info.loadSceneName + ".unity", parameters);
                } else{
                    info.sceneOpr = SceneManager.LoadSceneAsync(info.loadSceneName, LoadSceneMode.Additive);
                }
            #else
                info.sceneOpr = SceneManager.LoadSceneAsync(info.loadSceneName,LoadSceneMode.Additive);
            #endif


            //info.sceneOpr = SceneManager.LoadSceneAsync(info.loadSceneName, LoadSceneMode.Additive);
            if (info.sceneOpr == null)
            {
                OnSubSceneLoaded(info);
                mSubSceneLoadInfos.Remove(info.loadSceneName);
                yield break;
            }

            //异步直到场景加载完成-
            info.sceneOpr.allowSceneActivation = true;
            while (!info.sceneOpr.isDone) { yield return info.sceneOpr; }

            info.state = SubSceneLoadState.None;

            //加载完成回调-
            OnSubSceneLoaded(info);

            //加载场景完成后销毁原始assetbundle-
            ResourceMgr.ClearAssetBundle(info.loadSceneName, false);
        }


        //顺序集合加载子场景-
        private int mLoadAllSubScenes = 0;
        private List<string> mSubReadyLoadSceneList = new List<string>(4);
        private Action<string> mLoadSubSceneAfter = null;
        private Action mLoadAllSubScenesComplete = null;

        private void _LoadAllSubScenes(string[] allSceneNames, Action<string> loadSubSceneAfter = null, Action loadAllSubScenesComplete = null)
        {
            if (allSceneNames == null) { return; }
            if (mLoadAllSubScenes > 0) { if (loadAllSubScenesComplete != null) { loadAllSubScenesComplete(); } return; }        //已经有任务在身，强制调用结束-

            mSubReadyLoadSceneList.Clear();
            mSubReadyLoadSceneList.AddRange(allSceneNames);

            mLoadAllSubScenes = 1;
            mLoadSubSceneAfter = loadSubSceneAfter;
            mLoadAllSubScenesComplete = loadAllSubScenesComplete;
        }

        private void _LoadAllSubScenesUpdate()
        {
            if (mLoadAllSubScenes < 1) { return; }

            if (mLoadAllSubScenes == 1)
            {
                for (int i = 0; i < mSubReadyLoadSceneList.Count; i++)
                {
                    mLoadAllSubScenes = 2;

                    string loadSceneName = mSubReadyLoadSceneList[i];
                    LoadSubSceneInner(loadSceneName, null,
                        (string sceneName) =>
                        {
                            if (mLoadSubSceneAfter != null) { mLoadSubSceneAfter(sceneName); }

                            //等待这个加载完了，继续加载下一个-
                            mLoadAllSubScenes = 1;
                        });

                    mSubReadyLoadSceneList.RemoveAt(i);
                    break;
                }

                //已经没有场景可以卸载了-
                if (mLoadAllSubScenes == 1)
                {
                    mLoadAllSubScenes = 0;
                    if (mLoadAllSubScenesComplete != null) { mLoadAllSubScenesComplete(); }
                }
            }
        }

        #endregion

        #region 卸载子场景-
        //卸载子场景-
        private void UnloadSubSceneInner(string sceneName, Action<string> oldSceneUnloadBeforeCallBackFunc = null, Action<string> sceneLoadedCallBackFunc = null)
        {
            if (string.IsNullOrEmpty(sceneName)) { return; }
            if (!mSubSceneLoadInfos.ContainsKey(sceneName)) { return; }
            if (mSubSceneLoadInfos[sceneName].state == SubSceneLoadState.Unloading)
            {
                if (sceneLoadedCallBackFunc != null) { sceneLoadedCallBackFunc(sceneName); } 
                return;
            }

            SubSceneLoadInfo ssli = mSubSceneLoadInfos[sceneName];

            ssli.state = SubSceneLoadState.Unloading;
            ssli.oldSceneUnloadBeforeFunc = oldSceneUnloadBeforeCallBackFunc;
            ssli.loadSceneOverFunc = sceneLoadedCallBackFunc;

            StartCoroutine("AsyncUnloadSubScene", ssli);
        }

        private IEnumerator AsyncUnloadSubScene(SubSceneLoadInfo info)
        {
            //加载完成回调(这里先暂时不支持根据返回参量执行阻断机制)-
            OnOldSubSceneUnloadBefore(info);

            //-
            string[] snum = info.loadSceneName.Split('_');
            int id = -1;
            int.TryParse(snum[snum.Length - 1], out id);
            //WorldBootstrap.DestroyAddtionalNewSubSceneEntity(id);

            info.sceneOpr = SceneManager.UnloadSceneAsync(info.loadSceneName);
            while (info.sceneOpr == null)
            {
                yield return 0;

                info.sceneOpr = SceneManager.UnloadSceneAsync(info.loadSceneName);
                if (info.sceneOpr != null)
                {
                    break;
                }
            }
            
            info.sceneOpr.allowSceneActivation = true;
            while (!info.sceneOpr.isDone)         //异步知道场景加载完成-
            { yield return info.sceneOpr; }


            //卸载完成回调-
            OnSubSceneLoaded(info);

            //销毁assetbundle-
            mSubSceneLoadInfos.Remove(info.loadSceneName);
        }



        //释放已经加载或者正在加载中的场景-
        //sideSceneName:排除这些场景不卸载-
        private int mReleaseScene = 0;
        private List<SubSceneLoadInfo> mLoadedSceneList = new List<SubSceneLoadInfo>(10);
        private Dictionary<string, string> mSideSceneName = new Dictionary<string, string>(10);
        private Action<string> mUnloadSubSceneBefore = null;
        private Action mUnloadAllSubScenesComplete = null;

        private void _UnloadAllSubScenes(string[] sideSceneName, Action<string> unloadSubSceneBefore = null, Action unloadAllSubScenesComplete = null)
        {
            if (sideSceneName == null) { return; }
            if (mReleaseScene > 0) { if (unloadAllSubScenesComplete != null) { unloadAllSubScenesComplete(); } return; }


            ////校对一下目前存储的已经加载的场景是否还存在-
            //List<string> mTempName = new List<string>(4);

            //foreach(var name in mSubSceneLoadInfos.Keys)
            //{
            //    bool ext = false;
            //    for (int i = 0; i < SceneManager.sceneCount; i++)
            //    { var s = SceneManager.GetSceneAt(i); if (s.name == name) { ext = true; break; } }

            //    if(!ext) { mTempName.Add(name); }
            //}

            //foreach (var name in mTempName) { mSubSceneLoadInfos.Remove(name); }


            //-
            mSideSceneName.Clear();
            foreach (var name in sideSceneName) { mSideSceneName.Add(name, name); }

            mLoadedSceneList.Clear();
            mLoadedSceneList.AddRange(mSubSceneLoadInfos.Values);

            mReleaseScene = 1;
            mUnloadSubSceneBefore = unloadSubSceneBefore;
            mUnloadAllSubScenesComplete = unloadAllSubScenesComplete;
        }


        private void _UnloadAllSubScenesUpdate()
        {
            if (mReleaseScene < 1) { return; }

            //空的时候提取一个场景出来卸载-
            if (mReleaseScene == 1)
            {
                for (int i = 0; i < mLoadedSceneList.Count; i++)
                {
                    SubSceneLoadInfo sceneLoadInfo = mLoadedSceneList[i];
                    if (mSideSceneName.ContainsKey(sceneLoadInfo.loadSceneName) == false)
                    {
                        mReleaseScene = 2;

                        UnloadSubSceneInner(sceneLoadInfo.loadSceneName, mUnloadSubSceneBefore,
                            (string sceneName) =>
                            {
                                //等待这个卸载完了，继续卸载下一个-
                                mReleaseScene = 1;
                            });

                        mLoadedSceneList.RemoveAt(i);
                        
                        break;
                    }
                }

                //已经没有场景可以卸载了-
                if (mReleaseScene == 1)
                {
                    mReleaseScene = 0;
                    if (mUnloadAllSubScenesComplete != null) { mUnloadAllSubScenesComplete(); }
                }
            }
        }

        #endregion


        //-
        protected void OnOldSubSceneUnloadBefore(SubSceneLoadInfo info)
        {
            if (info.oldSceneUnloadBeforeFunc != null)
            {
                info.oldSceneUnloadBeforeFunc(info.loadSceneName);
                info.oldSceneUnloadBeforeFunc = null;
            }
        }


        //-       
        protected void OnSubSceneLoaded(SubSceneLoadInfo info)
        {
            if (info.loadSceneOverFunc != null)
            {
                info.loadSceneOverFunc(info.loadSceneName);
                info.loadSceneOverFunc = null;
            }
        }
    }
}
