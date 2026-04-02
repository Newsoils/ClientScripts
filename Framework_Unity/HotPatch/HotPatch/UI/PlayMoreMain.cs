using UnityEngine;
using UnityEngine.UI;
using GameCoreResourceLoad;
using System;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Core.Tools;



public class PlayMoreMain : MonoBehaviour
{
    public static PlayMoreMain boot = null;

    private static string       UICanvasPosName = "UITopTopTop";


    //调出边玩边下界面并开启流程-
    public static void Presage(Action<GameObject> func)
    {
        if (!cs_app_const.OpenPlayMoreDownload) 
        {
            return;
        }

        //-
        GameObject uiRoot = GameObject.Find("UIRoot");
        if(uiRoot != null )
        {
            Transform trans = uiRoot.transform.Find("UICanvas/" + UICanvasPosName);
            Transform transPlayMorePanel = trans.transform.Find("PlayMoreUpdate(HotPatch)");
            if(transPlayMorePanel == null)
            {
                string loadRes;
                ResourceMgr.LoadPrefabFromFileAnsy("PlayMoreUpdate",
                    (GameObject obj, GameObject shell) => {
                        //加载面板-
                        GameObject go = GameObject.Instantiate(obj);
                        go.name = "PlayMoreUpdate(HotPatch)";
                        Vector3 pos = go.transform.localPosition;

                        Transform hotPatchUIParentTrans = uiRoot.transform.Find("UICanvas/" + UICanvasPosName);
                        go.transform.parent = hotPatchUIParentTrans;

                        Unity_Tools.IdentityGameObject(go);
                        go.transform.localPosition = pos;
                        //var rectTran = go.GetComponent<RectTransform>();
                        //rectTran.offsetMin = rectTran.offsetMax = Vector2.zero;

                        var pmr = go.GetComponent<PlayMoreMain>();
                        if (pmr == null) { go.AddComponent<PlayMoreMain>(); }

                        func?.Invoke(go);
                    }, null, out loadRes);
            } else {
                var pmr = transPlayMorePanel.gameObject.GetComponent<PlayMoreMain>();
                if (pmr == null) { pmr = transPlayMorePanel.gameObject.AddComponent<PlayMoreMain>(); }

                func?.Invoke(transPlayMorePanel.gameObject);
            }


            Debug.Log("登录主界面卸载");
            HotPatchMain.DestroyLoadingPanel();
        }
    }


    //关闭热更加载界面-
    public static void CloseUnloadPanel()
    {
        HotPatchMain.DestroyLoadingPanel();
    }



    //-
    private Text            uiDownloadTotalInfo = null;
    private float           mTryRestartAge = 0f; 


    //-
    private void Awake()
    {
        EvtDsp.AddEvt(GameEventDefine.GAME_VERSION_CHECK_START, OnGameVersionCheckStart);
        EvtDsp.AddEvt<int, int, string>(GameEventDefine.GAME_VERSION_CHECK_FILE, OnDownloadCheckFiles);
        EvtDsp.AddEvt<float>(GameEventDefine.GAME_VERSION_DOWNLOAD_FILE_PROCESS, OnDownloadFileProcess);
        EvtDsp.AddEvt<string>(GameEventDefine.GAME_VERSION_CHECK_END, OnGameVersionCheckEnd);
        EvtDsp.AddEvt<int, int>(GameEventDefine.GAME_VERSION_CHECK_FILE_PROCESS, OnGameVersionCheckProcess);
        EvtDsp.AddEvt(GameEventDefine.GAME_VERSION_CHECK_INFO_GET_FAILED, OnHotpatchFailed);
        EvtDsp.AddEvt<int>(GameEventDefine.CS_RESTART_GAME, OnGameRestart);


        boot = this;

        UIRootInit();
    }


    private void OnDestroy()
    {
        EvtDsp.RemoveEvt(GameEventDefine.GAME_VERSION_CHECK_START, OnGameVersionCheckStart);
        EvtDsp.RemoveEvt<int, int, string>(GameEventDefine.GAME_VERSION_CHECK_FILE, OnDownloadCheckFiles);
        EvtDsp.RemoveEvt<float>(GameEventDefine.GAME_VERSION_DOWNLOAD_FILE_PROCESS, OnDownloadFileProcess);
        EvtDsp.RemoveEvt<string>(GameEventDefine.GAME_VERSION_CHECK_END, OnGameVersionCheckEnd);
        EvtDsp.RemoveEvt<int, int>(GameEventDefine.GAME_VERSION_CHECK_FILE_PROCESS, OnGameVersionCheckProcess);
        EvtDsp.RemoveEvt(GameEventDefine.GAME_VERSION_CHECK_INFO_GET_FAILED, OnHotpatchFailed);
        EvtDsp.RemoveEvt<int>(GameEventDefine.CS_RESTART_GAME, OnGameRestart);

        boot = null;
    }


    private void OnGameRestart(int restartCount)
    {
        if (restartCount < 1) { return; }

        GameObject.DestroyImmediate(this.gameObject);
    }


    //-
    private bool mCheckHotpatch = true;
    private int mThFile = 0;
    private int mTotalFileNumber = 0;
    private string mDownloadingFileName = "";
    private string NetDescript()
    {
        return "\n当前网络:" + Unity_Tools.InternetSourceMode();
    }

    private void OnDownloadCheckFiles(int thFile, int totalFileNumber, string downloadingFileName)
    {
        mThFile = thFile;
        mTotalFileNumber = totalFileNumber;
        mDownloadingFileName = downloadingFileName;
        uiDownloadTotalInfo.text = "正在下载文件:" + mDownloadingFileName + " 0%" + "\n" +
            " 总共:" + (mTotalFileNumber - mThFile) + "/" + mTotalFileNumber + "文件" + NetDescript();
    }

    private void OnDownloadFileProcess(float procF)
    {
        uiDownloadTotalInfo.text = "正在下载文件:" + mDownloadingFileName + string.Format(" {0:N2}", procF * 100.0f) + "%\n" +
            " 总共:" + (mTotalFileNumber - mThFile) + "/" + mTotalFileNumber + "文件" + NetDescript();
    }

    private void OnGameVersionCheckEnd(string obj)
    {
        uiDownloadTotalInfo.text = "边玩边下内容下载完成!" + NetDescript();
    }

    private void OnGameVersionCheckStart()
    {
        uiDownloadTotalInfo.text = "边玩边下检查开始 0%" + NetDescript();
    }

    private void OnGameVersionCheckProcess(int cur, int tol)
    {
        float proc = (float)cur / tol;
        uiDownloadTotalInfo.text = "边玩边下检查开始 " + string.Format(" {0:N2}%", proc * 100.0f) + NetDescript();
    }

    private void OnHotpatchFailed()
    {
        uiDownloadTotalInfo.text = "边玩边下更新失败!等待重启.." + NetDescript();
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void UIRootInit()
    {
        uiDownloadTotalInfo = this.transform.Find("bk1/DownloadInfoShow").GetComponent<Text>();
        uiDownloadTotalInfo.text = "";

        //BJAnimationPlayerMgr.Play(this.gameObject, "playmorepanel_show");
    }




    // Update is called once per frame
    private void Update()
    {
        //自动开启边玩边下-
        if(HotPatchManager.PlayMoreState == 0)
        {
            mTryRestartAge -= Time.deltaTime;
            if(mTryRestartAge < 0f)
            {
                HotPatchManager.DoPlayMore(() =>
                {
                    //GameObject.DestroyImmediate(this.gameObject);
                    Log.Info("PlayMoreMain:: 自动开启边玩边下界面");
                    //BigTimer.In(5, () =>
                    //{
                    //    BJAnimationPlayerMgr.Play(this.gameObject, "playmorepanel_out");
                    //});
                });

                mTryRestartAge = 10f;
            }
        }
    }
}
