using UnityEngine;
using UnityEngine.UI;
using GameCoreResourceLoad;
using System;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Tools;



public class HotPatchMain : MonoBehaviour
{
    public static HotPatchMain          boot = null;
    private static bool                 beginDestroyHotpatchLogic = false;
    private static bool                 mLoadingResMgrHeadOver = false;
    private static Action<GameObject>   mLoadingUpFunc = null;


    private static GameObject UIRoot()
    {
        GameObject goRoot = GameObject.Find("UIRoot");
        if (goRoot == null)
        {
            GameObject resObj = Resources.Load<GameObject>("UIRoot");
            if (resObj != null)
            {
                goRoot = GameObject.Instantiate(resObj);
                goRoot.name = "UIRoot";
                Unity_Tools.IdentityGameObject(goRoot);

                GameObject.DontDestroyOnLoad(goRoot);
            }
            else { Debug.LogError("UIRoot界面启动失败!"); }
        }

        return goRoot;
    }


    //-
    private static void _LoadLoadingPanel()
    {
        if (mLoadingResMgrHeadOver)
        {
            string loadRes;
            ResourceMgr.LoadPrefabFromFileAnsy("loading",
                (GameObject obj, GameObject shell) => {
                    Debug.Log("正在加载\"加载面板\"！！！");
                    if (obj == null) { Debug.LogError("结果加载面板为空！！！"); }

                    //加载面板-
                    GameObject go = GameObject.Instantiate(obj);
                    go.name = "view_login_loading(HotPatch)";

                    GameObject uiRoot = GameObject.Find("UIRoot");
                    Transform hotPatchUIParentTrans = uiRoot.transform.Find("UICanvas/UITop");
                    go.transform.parent = hotPatchUIParentTrans;

                    Unity_Tools.IdentityGameObject(go);
                    var rectTran = go.GetComponent<RectTransform>();
                    rectTran.offsetMin = rectTran.offsetMax = Vector2.zero;

                    mLoadingUpFunc?.Invoke(go);
                    mLoadingUpFunc = null;

                }, null, out loadRes);

            mLoadingResMgrHeadOver = false;
        }
    }

    public static void OpenLoadingPanel(Action<GameObject> func, bool forceLoadLoadingPanel = false)
    {
        GameObject goRoot = UIRoot();

        mLoadingUpFunc = func;

        //开始针对加载通道选项进行对应加载-
        ResourceMgr.BUNDLE_ACCESS_READ(() => 
        { 
            mLoadingResMgrHeadOver = true;

            if(forceLoadLoadingPanel)
            {
                //#if UNITY_EDITOR
                _LoadLoadingPanel();
                //#endif
            }
        });
    }

    public static void DestroyLoadingPanel()
    {
        HotPatchMain.CloseUnloadPanel();
    }


    //删除热更后初始化进度面板-
    public static void CloseUnloadPanel()
    {
        beginDestroyHotpatchLogic = true;

        GameObject uiRoot = GameObject.Find("UIRoot");
        if(uiRoot != null) 
        { 
            //这里可能调用删除的时候面板并未创建出来,会删除失败,所以放在更新历程里继续检查直到目标删除-
            Transform loadingTrans = uiRoot.transform.Find("UICanvas/UITop/view_login_loading(HotPatch)");
            if (loadingTrans != null) 
            {
                Debug.Log("<color=red> UIRoot:删除热更进度界面:" + loadingTrans.name + " </color>");
                //loadingTrans.gameObject.SetActive(false);
                GameObject.DestroyImmediate(loadingTrans.gameObject); 
            }
        }
    }

    public static void UpdateCloseUnloadPanel() 
    {
        if (beginDestroyHotpatchLogic)
        {
            //关闭加载界面-
            HotPatchMain.CloseUnloadPanel();

            Transform loadingTrans = HotPatchMain.UIRoot().transform.Find("UICanvas/UITop/view_login_loading(HotPatch)");
            if (loadingTrans == null)
            {
                beginDestroyHotpatchLogic = false;
                if (boot != null && boot.hotPatchUIRoot != null) { boot.hotPatchUIRoot.gameObject.SetActive(false); }
            }
        }
    }


    public static void HiddenLoadingPanel()
    {
        GameObject uiRoot = GameObject.Find("UIRoot");
        if (uiRoot != null)
        {
            Transform loadingTrans = uiRoot.transform.Find("UICanvas/UITop/view_login_loading(HotPatch)");
            if (loadingTrans != null) 
            {
                Debug.Log("<color=red> UIRoot:隐藏热更进度界面 </color>");
                loadingTrans.gameObject.SetActive(false); 
            }
        }
    }


    //-
    private GameObject       uiRoot = null;
    private GameObject       hotPatchUIRoot = null;
    private GameObject       uiClickContinue = null;
    private Text             uiClickToContinueName = null;
    private Text             uiDownloadTotalInfo = null;
    private Text             uiVersion = null;
    

    //private bool                mLoadingResMgrHeadOver = false;
    //private Action<GameObject>  mLoadingUpFunc = null;


    private void Awake()
    {
        EvtDsp.AddEvt(GameEventDefine.GAME_VERSION_CHECK_START, OnGameVersionCheckStart);
        EvtDsp.AddEvt(GameEventDefine.GAME_VERSION_BIG_NEED_APPSTORE_DOWNLOAD, OnGameVersionNeedShopDownload);
        EvtDsp.AddEvt<int, int, string>(GameEventDefine.GAME_VERSION_CHECK_FILE, OnDownloadCheckFiles);
        EvtDsp.AddEvt<float>(GameEventDefine.GAME_VERSION_DOWNLOAD_FILE_PROCESS, OnDownloadFileProcess);
        EvtDsp.AddEvt<string>(GameEventDefine.GAME_VERSION_CHECK_END, OnGameVersionCheckEnd);
        EvtDsp.AddEvt<int, int>(GameEventDefine.GAME_VERSION_CHECK_FILE_PROCESS, OnGameVersionCheckProcess);
        EvtDsp.AddEvt(GameEventDefine.GAME_VERSION_CHECK_INFO_GET_FAILED, OnHotpatchFailed);
        EvtDsp.AddEvt<int>(GameEventDefine.CS_RESTART_GAME, OnGameRestart);

        boot = this;

        //热更初始化UI-
        UIRootInit();

        //-
        GameObject.DontDestroyOnLoad(this);
    }


    private void OnDestroy()
    {
        EvtDsp.RemoveEvt(GameEventDefine.GAME_VERSION_CHECK_START, OnGameVersionCheckStart);
        EvtDsp.RemoveEvt(GameEventDefine.GAME_VERSION_BIG_NEED_APPSTORE_DOWNLOAD, OnGameVersionNeedShopDownload);
        EvtDsp.RemoveEvt<int, int, string>(GameEventDefine.GAME_VERSION_CHECK_FILE, OnDownloadCheckFiles);
        EvtDsp.RemoveEvt<float>(GameEventDefine.GAME_VERSION_DOWNLOAD_FILE_PROCESS, OnDownloadFileProcess);
        EvtDsp.RemoveEvt<string>(GameEventDefine.GAME_VERSION_CHECK_END, OnGameVersionCheckEnd);
        EvtDsp.RemoveEvt<int, int>(GameEventDefine.GAME_VERSION_CHECK_FILE_PROCESS, OnGameVersionCheckProcess);
        EvtDsp.RemoveEvt(GameEventDefine.GAME_VERSION_CHECK_INFO_GET_FAILED, OnHotpatchFailed);
        EvtDsp.RemoveEvt<int>(GameEventDefine.CS_RESTART_GAME, OnGameRestart);

        boot = null;

        UIRootClear();
    }

    private void OnGameRestart(int restartCount)
    {
        if (restartCount < 1) { return; }

        //关闭加载界面-
        HotPatchMain.CloseUnloadPanel();
        GameObject.DestroyImmediate(HotPatchMain.boot.gameObject);
    }



    //-
    private bool mCheckHotpatch = true;
    private int mThFile = 0;
    private int mTotalFileNumber = 0;
    private string mDownloadingFileName = "";
    private void OnDownloadCheckFiles(int thFile, int totalFileNumber, string downloadingFileName)
    {
        mThFile = thFile;
        mTotalFileNumber = totalFileNumber;
        mDownloadingFileName = downloadingFileName;
        uiDownloadTotalInfo.text = "正在下载文件:" + mDownloadingFileName + " 0%" + "\n" + 
            " 总共:" + (mTotalFileNumber - mThFile) + "/" + mTotalFileNumber + "文件";
    }

    //-
    private void OnDownloadFileProcess(float procF)
    {
        uiDownloadTotalInfo.text = "正在下载文件:" + mDownloadingFileName + string.Format(" {0:N2}", procF * 100.0f) + "%\n" +
            " 总共:" + (mTotalFileNumber - mThFile) + "/" + mTotalFileNumber + "文件";
    }

    //-
    private void OnGameVersionNeedShopDownload()
    {
        uiDownloadTotalInfo.text = "需要从商店重新下载大版本";
    }

    //-
    private void OnGameVersionCheckStart()
    {
        uiVersion.text = "Ver:" + HotPatchManager.Version + "\n" + "Language:" + ResourceMgr.Language + "\nNet:" + Unity_Tools.InternetSourceMode();
        uiDownloadTotalInfo.text = "版本检查开始 0%";
    }

    private void OnGameVersionCheckProcess(int cur, int tol)
    {
        float proc = (float)cur / tol;
        uiDownloadTotalInfo.text = "版本检查开始 " + string.Format(" {0:N2}%", proc * 100.0f);
    }

    private void OnGameVersionCheckEnd(string obj)
    {
        mCheckHotpatch = false;
        uiVersion.text = "Ver:" + HotPatchManager.Version + "\n" + "Language:" + ResourceMgr.Language + "\nNet:" + Unity_Tools.InternetSourceMode();
        uiDownloadTotalInfo.text = "";
        uiClickToContinueName.text = mCheckHotpatch ? "点击热更开始" : "点击游戏开始";
        uiClickContinue.SetActive(true);

        ////开始针对加载通道选项进行对应加载-
        //ResourceMgr.BUNDLE_ACCESS = ResourceMgr.BUNDLE_ACCESS;
        //ResourceMgr.BUNDLE_ACCESS_READ(null);
    }

    private void OnHotpatchFailed()
    {
        mCheckHotpatch = true;
        uiClickToContinueName.text = mCheckHotpatch ? "点击热更开始" : "点击游戏开始";

        uiClickContinue.SetActive(true);
    }


    private void Start()
    {
        //初始游戏设置-
        cs_base_behaviour.InitGameSettings();
        if (!Application.isEditor &&
            Application.platform != RuntimePlatform.WindowsPlayer)
        { Application.targetFrameRate = cs_app_const.GameFrameRate; }

        //-
        //BJGamePhaseDealMgr.PhaseNotify("scene_hotupdate", 0);

        ////热更开始-
        //HotPatchManager.Do();
    }


    //-
    private void UIRootInit()
    {
        uiRoot = HotPatchMain.UIRoot();
        Transform trans = uiRoot.transform.Find("UICanvas/UICenter/HotUpdate");
        hotPatchUIRoot = trans != null ? trans.gameObject : null;
        if (hotPatchUIRoot == null)
        {
            Transform hotPatchUIParentTrans = uiRoot.transform.Find("UICanvas/UICenter");
            GameObject obj = Resources.Load<GameObject>("Hotpatch/HotUpdate");
            if (obj != null)
            {
                hotPatchUIRoot = GameObject.Instantiate(obj);
                hotPatchUIRoot.name = "HotUpdate";
                hotPatchUIRoot.transform.parent = hotPatchUIParentTrans;
                Unity_Tools.IdentityGameObject(hotPatchUIRoot);

                var rectTran = hotPatchUIRoot.GetComponent<RectTransform>();
                rectTran.offsetMin = rectTran.offsetMax = Vector2.zero;
            }
        }

        uiClickContinue = hotPatchUIRoot.transform.Find("ClickToContinue").gameObject;
        uiClickToContinueName = hotPatchUIRoot.transform.Find("ClickToContinue/ClickToContinueName").GetComponent<Text>();
        uiDownloadTotalInfo = hotPatchUIRoot.transform.Find("DownloadInfoShow").GetComponent<Text>();
        uiVersion = hotPatchUIRoot.transform.Find("Version").GetComponent<Text>();

        uiVersion.text = "";
        uiDownloadTotalInfo.text = "";
        uiClickToContinueName.text = mCheckHotpatch ? "点击热更开始" : "点击游戏开始";
        //uiClickContinue.SetActive(false);
        uiClickContinue.SetActive(true);

        Button btnClickContinue = uiClickContinue.GetComponent<Button>();
        btnClickContinue.onClick.AddListener(StartGame);
    }

    private void UIRootClear()
    {
        //HotPatchMain.CloseUnloadPanel();

        if (hotPatchUIRoot != null) { GameObject.Destroy(hotPatchUIRoot); hotPatchUIRoot = null; }
    }

    

    //-
    public void StartGame()
    {
        uiClickContinue.SetActive(false);

        if (mCheckHotpatch)
        {
            mCheckHotpatch = false;

            //热更开始-
            HotPatchManager.Do();
        } else {
            OpenLoadingPanel((GameObject go) => 
            {
                //载入游戏初始化场景-
                cs_main.LoadSceneInit();
            });
        }
    }


    void Update()
    {
        _LoadLoadingPanel();

        //-
        UpdateCloseUnloadPanel();
    }
}


//测试
//using System.Net;
//using System.Net.Security;
//using System.Security.Cryptography.X509Certificates;

//HttpWebRequest request = WebRequest.Create("https://kuyu-1309897295.cos.ap-shanghai.myqcloud.com/dcg_dev/Bundles/Android/Pag/HotCompare.db") as HttpWebRequest;
//ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
//if (request != null) 
//{
//    request.ProtocolVersion = HttpVersion.Version10;
//    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//    if (response != null) { Debug.LogError("测试的response目标大小:" + response.ContentLength); response.Close(); response.Dispose(); }
//    else
//    {
//        Debug.LogError("测试的response为空");
//    }

//    request.Abort();

//} else { Debug.LogError("测试的request为空"); }
