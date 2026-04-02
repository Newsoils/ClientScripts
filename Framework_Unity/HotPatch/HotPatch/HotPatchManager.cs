#define BRING_HOT_PATCH

using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using GameCoreResourceLoad;
using System.Threading.Tasks;
using CLIP.Framework_Core.Tools;
using CLIP.Framework_Core.Event;




//热更新流程-
public class HotPatchManager : MonoBehaviour
{
    //-
    static public HotPatchManager   boot = null;

    static public string                URL_ADDRESS_DOWNLOAD = "file:///" + Application.dataPath + "/../TestServer/";         //从服务器的下载地址-
    static public string                DownloadToLocalPath =string.Empty;          //下载的资源所在的磁盘位置-
    static public string                Version = string.Empty;                     //版本-
    public static int                   PlayMoreState => boot == null ? 0 : boot.PlayMoreDone;


    //热更-
    public static void Do() { if (boot != null) { boot._Do(); } }

    //边玩边下-
    public static void DoPlayMore(Action func) { if (boot != null) { boot._DoPlayMore(func); } }
    public static void CancelDoPlayMore() 
    {
        boot?._CancelDoPlayMore();
    }


    //string[0]:文件名-
    //string[1]:md5-
    //string[2]:文件最终存放路径-
    private List<string[]>              mHotPatchList =new List<string[]>(10);
    private int                         mHotPatchListFinalCount =0;

    private string[]                    mInHotPatching = null;

    private int                         mDownloadState =0;              //下载阶段0:未开始下载  1:下载进程中-

    private int                         mCheckFileStart =0;
    public int                          TolCheckFiles = 0;
    public int                          CurCheckFiles = 0;


    public int                          PlayMoreDone = 0;               //边玩边下状态 0:未开始 1:进入边玩边下流程 6:已经完成-

    private Action                      mPlayMoreCompleteFunc = null;
    private Task                        mTask = null;
    private int                         mTaskQueueCount = 0;

    //-
    private void Awake()
    {
        boot = this;
        EvtDsp.AddEvt<int>(GameEventDefine.CS_RESTART_GAME, OnGameRestart);
    }

    private void OnDestroy()
    {
        boot = null;
        EvtDsp.RemoveEvt<int>(GameEventDefine.CS_RESTART_GAME, OnGameRestart);
    }


    private void OnGameRestart(int restartCount)
    {
        if (restartCount < 1) { return; }
        _CancelDoPlayMore();
    }


    // Update is called once per frame
    void Update()
    {
        UpdateHotPatch();
    }


    //热更新检查-
    private void _Do()
    {
        //资源管理器中销毁全部已经加载的资源-
        ResourceMgr.ClearAll();

        //-
        _SetUp();
    }



    //热更新流程开启-
    private void _SetUp()
    {
        //Resource模式下直接跳过更新检查流程-
        if (ResourceMgr.LATMode == ResourceMgr.LoadAccessType.FromResource)
        {
            _clear_hotpatch_buffer("ResourceMgr:LoadAccessType.FromResource加载途径不支持");
            return;
        }

        //-
        if(mDownloadState > 0 || mCheckFileStart >0)
        {
            Debug.LogError("已经在进行更新流程了");
            return;
        }


        mHotPatchList.Clear();
        mInHotPatching =null;
        mHotPatchListFinalCount = 0;
        TolCheckFiles = CurCheckFiles = mCheckFileStart = 0;
        Version = Application.version;

        //提示通知更新检查-
        EvtDsp.TriggerEvt(GameEventDefine.GAME_VERSION_CHECK_START);

        //检查配置是否进入热更模式加载中-
        if (ResourceMgr.BUNDLE_ACCESS != ResourceMgr.BundleAccessType.HotpatchAssets) 
        {
            EvtDsp.TriggerEvt<string>(GameEventDefine.GAME_VERSION_CHECK_END, Version);         //注意这里是有意识这么写的，不能走报错通知，不然不能继续流程了-
            return; 
        }

        //当前环境下下载流程开启-
        string osName = GetDownloadOsDir();


        //-
        if (!string.IsNullOrEmpty(osName) )
        {
            //获取对应的服务区捕获的url地址-
            URL_ADDRESS_DOWNLOAD = GameServiceMgr.GetDownloadAddress(osName, "download");

            if (string.IsNullOrEmpty(URL_ADDRESS_DOWNLOAD))
            {
                _clear_hotpatch_buffer("下载源地址为空!");
            } else {
                //新建下载目录-
                if (!Directory.Exists(HttpDownloadManager.DownloadPath) ) { Directory.CreateDirectory(HttpDownloadManager.DownloadPath); }

                //下载地址捕获成功-
                //从服务器下载版本配置文件-
                mCheckFileStart = 1;
                File.Delete(HttpDownloadManager.DownloadPath + "HotCompare.db.spwi");
                HttpDownloadManager.StartDownloadFile(URL_ADDRESS_DOWNLOAD, "HotCompare.db", 1, "",
                    (string s, int n) => {
                        if (n > 0)
                        {
                            //下载成功-
                            //如果没有就直接用下载的文件并且全部更新下载包，-
                            //如果有本地配置文件过时需要跟新到新配置文件的情况，那么先和本地的下载文件包比对md5码，过时的重新下载，等全部包下载校对完毕跟新到新的配置文件-
                            DownloadToLocalPath = HttpDownloadManager.DownloadPath;

                            //清理上一次内存中的老数据-
                            GameServiceMgr.DbInfos.Clear();
                            GameServiceMgr.DbOldInfos.Clear();

                            //读取本地配置文件-
                            string OldVersion = GameServiceMgr.GetAppVersion();
                            string NewVersion = GameServiceMgr.GetNewAppVersion();

                            if (string.IsNullOrEmpty(NewVersion)) { _clear_hotpatch_buffer("无效的下载配置头文件!"); return; }



                            //如果大版本号相同，就不再更新，要求去商店重新下载-
                            string AppVersion = Application.version;

                            if (AppVersion.Split('.')[0] != NewVersion.Split('.')[0])
                            {
                                EvtDsp.TriggerEvt(GameEventDefine.GAME_VERSION_BIG_NEED_APPSTORE_DOWNLOAD);
                                Debug.Log("<color=red>大版本更新需要重新,请从商店更新下载包!</color>");
                                return;
                            }


                            string      LanguageDirName = string.Format("lg_{0}", ResourceMgr.Language);
                            string[]    downLoadInfoFiles = GameServiceMgr.DbNewUpdateFiles;


                            mTask = new Task(() => {

                                TolCheckFiles = downLoadInfoFiles.Length;
                                CurCheckFiles = 0;

                                //语言包的检查和下载-
                                //只要对比从服务器上下载文件中md5码不同，就重新下载-
                                for (int idNew = 0; idNew < downLoadInfoFiles.Length; idNew++)
                                {
                                    if (downLoadInfoFiles[idNew].ToLower().Contains("playmore-")) { CurCheckFiles++; continue; }          //边玩边下类型直接撇开-
                                    if (downLoadInfoFiles[idNew].Contains("lg_")) { CurCheckFiles++; }
                                    if (string.IsNullOrEmpty(downLoadInfoFiles[idNew])) { continue; }

                                    string[] downloadFileName = downLoadInfoFiles[idNew].Split('|');

                                    //这里只下检查载语言包-
                                    string lagName = Path.GetFileNameWithoutExtension(downloadFileName[0]);
                                    if (lagName != LanguageDirName)
                                    {
                                        if (!downloadFileName[0].Contains(LanguageDirName)) { continue; }

                                        //因为组合的格式关系,去掉最后一位数字位,再比较一次-
                                        lagName = StringTools.GetNoNumberName(lagName, '_');
                                        if (lagName != LanguageDirName) { continue; }
                                    }

                                    string md5 = downloadFileName[1];
                                    string tmpPath = ResourceMgr.BUNDLE_PATH + downloadFileName[0];
                                    if (File.Exists(tmpPath))
                                    {
                                        string fileMd5 = StringEncodeMgr.GetMd5(ResourceMgr.LoadIOFileAnsy(tmpPath).Result);
                                        if (fileMd5 == md5) { continue; }
                                    }

                                    Debug.Log("<color=yellow>本地语言包文件{0},需要更新下载</color>" + downloadFileName[0]);
                                    mHotPatchList.Add(new string[3] { downloadFileName[0], md5, tmpPath });
                                }


                                //除语言包外的其他文件-
                                for (int idNew = 0; idNew < downLoadInfoFiles.Length; idNew++)
                                {
                                    if (downLoadInfoFiles[idNew].ToLower().Contains("playmore-")) { continue; }
                                    if (string.IsNullOrEmpty(downLoadInfoFiles[idNew])) { CurCheckFiles++; continue; }

                                    string[] downloadFileName = downLoadInfoFiles[idNew].Split('|');

                                    //语言条件选择,语言包不在这里下载-
                                    if (downloadFileName[0].Contains("lg_")) { continue; } else { CurCheckFiles++; }


                                    string md5 = downloadFileName[1];
                                    string fileAtPath = string.Empty;
                                    string tmpPath = ResourceMgr.BUNDLE_PATH + downloadFileName[0];
                                    if (File.Exists(tmpPath)) 
                                    { 
                                        fileAtPath = tmpPath;

                                        string fileMd5 = StringEncodeMgr.GetMd5(ResourceMgr.LoadIOFileAnsy(tmpPath).Result);
                                        if (fileMd5 == md5) { continue; }
                                    }

                                    mHotPatchList.Add(new string[3] { downloadFileName[0], md5, fileAtPath });
                                }


                                //把版本号成功导入到游戏运行时中-
                                mCheckFileStart = 0;
                                Version = NewVersion;

                                //开始启动更新-
                                mDownloadState = 1;
                                mHotPatchListFinalCount = mHotPatchList.Count;

                                return;
                            });

                            mTask.Start();
                        } else {

                            //-
                            _clear_hotpatch_buffer("下载服务器链接失败,code:" + n +
                                "\n下载目标地址:" + s);
                        }
                    });
            }
        }
    }



    //边玩边下流程开启-
    private void _DoPlayMore(Action func)
    {
        //Resource模式下直接跳过更新检查流程-
        if (ResourceMgr.LATMode == ResourceMgr.LoadAccessType.FromResource)
        {
            _clear_hotpatch_buffer("ResourceMgr:LoadAccessType.FromResource加载途径不支持");
            return;
        }

        //-
        if (mDownloadState > 0 || mCheckFileStart > 0 || PlayMoreDone == 1)
        {
            Debug.LogError("已经在进行边玩边下流程了");
            return;
        }

        //-
        PlayMoreDone = 1;
        mPlayMoreCompleteFunc = func;
        mHotPatchList.Clear();
        mInHotPatching = null;
        mHotPatchListFinalCount = 0;
        TolCheckFiles = CurCheckFiles = mCheckFileStart = 0;

        //string localFilePath = ResourceMgr.BUNDLE_ABS_PATH;
        //string newFilePath = HttpDownloadManager.DownloadPath;

        //提示通知更新检查-
        EvtDsp.TriggerEvt(GameEventDefine.GAME_VERSION_CHECK_START);

        ////检查配置是否进入热更模式加载中-
        //if (ResourceMgr.BUNDLE_ACCESS != ResourceMgr.BundleAccessType.HotpatchAssets)
        //{
        //    EvtDsp.TriggerEvt<string>(GameEventDefine.GAME_VERSION_CHECK_END, Version);         //注意这里是有意识这么写的，不能走报错通知，不然不能继续流程了-
        //    return;
        //}


        //当前环境下下载流程开启-
        string osName = GetDownloadOsDir();

        //-
        if (!string.IsNullOrEmpty(osName))
        {
            //获取对应的服务区捕获的url地址-
            URL_ADDRESS_DOWNLOAD = GameServiceMgr.GetDownloadAddress(osName, "download");

            if (string.IsNullOrEmpty(URL_ADDRESS_DOWNLOAD))
            {
                _clear_hotpatch_buffer("下载源地址为空!");
            } else {
                //新建下载目录-
                if (!Directory.Exists(HttpDownloadManager.DownloadPath)) { Directory.CreateDirectory(HttpDownloadManager.DownloadPath); }

                //下载地址捕获成功-
                //从服务器下载版本配置文件-
                mCheckFileStart = 1;
                File.Delete(HttpDownloadManager.DownloadPath + "HotCompare.db.spwi");
                HttpDownloadManager.StartDownloadFile(URL_ADDRESS_DOWNLOAD, "HotCompare.db", 1, "",
                    (string s, int n) =>{

                        if(n < 1){  _clear_hotpatch_buffer("下载服务器链接失败,code:" + n + "\n下载目标地址:" + s); return;  }

                        //-
                        DownloadToLocalPath = HttpDownloadManager.DownloadPath;

                        string      newVersion = GameServiceMgr.GetNewAppVersion();
                        string      LanguageDirName = string.Format("playmore-lg_{0}", ResourceMgr.Language);
                        string[]    downLoadInfoFilesBuff = GameServiceMgr.DbNewUpdateFiles;

                        mTask = new Task((object _queueCount) =>
                        {
                            int queueCount = (int)_queueCount;


                            //统计出边玩边下的文件-
                            List<string> downLoadInfoFiles = new List<string>(16);
                            for (int idNew = 0; idNew < downLoadInfoFilesBuff.Length; idNew++)
                            {
                                if (!downLoadInfoFilesBuff[idNew].ToLower().Contains("playmore-")) { continue; } else 
                                { 
                                    downLoadInfoFiles.Add(downLoadInfoFilesBuff[idNew]); 
                                } 
                            }

                            TolCheckFiles = downLoadInfoFiles.Count;

                            //语言包的检查和下载-
                            //只要对比从服务器上下载文件中md5码不同，就重新下载-
                            for (int idNew = 0; idNew < downLoadInfoFiles.Count; idNew++)
                            {
                                //线程强制中断处理-
                                if (queueCount != mTaskQueueCount) { _CancelDoPlayMore(); return; }

                                //-
                                if (downLoadInfoFiles[idNew].Contains("playmore-lg_")) { CurCheckFiles++; }

                                string[] downloadFileName = downLoadInfoFiles[idNew].Split('|');

                                //这里只下检查载语言包-
                                string lagName = Path.GetFileNameWithoutExtension(downloadFileName[0]);
                                if (lagName != LanguageDirName)
                                {
                                    if (!downloadFileName[0].Contains(LanguageDirName)) { continue; }

                                    //因为组合的格式关系,去掉最后一位数字位,再比较一次-
                                    lagName = StringTools.GetNoNumberName(lagName, '_');
                                    if (lagName != LanguageDirName) { continue; }
                                }

                                string md5 = downloadFileName[1];
                                string tmpPath = ResourceMgr.BUNDLE_ABS_PATH + downloadFileName[0];
                                if (File.Exists(tmpPath))
                                {
                                    string fileMd5 = StringEncodeMgr.GetMd5(ResourceMgr.LoadIOFileAnsy(tmpPath).Result);
                                    if (fileMd5 == md5) { continue; }
                                }

                                Debug.Log("<color=yellow>本地语言包文件{0},需要更新下载</color>" + downloadFileName[0]);
                                mHotPatchList.Add(new string[3] { downloadFileName[0], md5, tmpPath });
                            }


                            //-
                            //除语言包外的其他文件-
                            for (int idNew = 0; idNew < downLoadInfoFiles.Count; idNew++)
                            {
                                //线程强制中断处理-
                                if (queueCount != mTaskQueueCount) { _CancelDoPlayMore(); return; }

                                //-
                                string[] downloadFileName = downLoadInfoFiles[idNew].Split('|');

                                //语言条件选择,语言包不在这里下载-
                                if (downloadFileName[0].Contains("playmore-lg_")) { continue; } else { CurCheckFiles++; }

                                string md5 = downloadFileName[1];
                                string fileAtPath = string.Empty;
                                string tmpPath = ResourceMgr.BUNDLE_ABS_PATH + downloadFileName[0];
                                if (File.Exists(tmpPath))
                                {
                                    fileAtPath = tmpPath;

                                    string fileMd5 = StringEncodeMgr.GetMd5(ResourceMgr.LoadIOFileAnsy(tmpPath).Result);
                                    if (fileMd5 == md5) { continue; }
                                }

                                mHotPatchList.Add(new string[3] { downloadFileName[0], md5, fileAtPath });
                            }

                            //线程强制中断处理-
                            if (queueCount != mTaskQueueCount) { _CancelDoPlayMore(); return; }


                            //把版本号成功导入到游戏运行时中-
                            mCheckFileStart = 0;

                            //开始启动更新-
                            mDownloadState = 1;
                            mHotPatchListFinalCount = mHotPatchList.Count;

                            return;

                        }, mTaskQueueCount);
                        mTask.Start();
                    });
            }
        }
    }


    //获得当前操作系统下的下载相对目录-
    private string GetDownloadOsDir()
    {
        string osName;
        switch (Application.platform)
        {
            case RuntimePlatform.Android: osName = "android"; break;
            case RuntimePlatform.IPhonePlayer: osName = "ios"; break;

            default: osName = "win"; break;
        }

        return osName;
    }

    //-
    private void _clear_hotpatch_buffer(string errMsg = "")
    {
        Debug.LogError(string.IsNullOrEmpty(errMsg) ? "下载文件" + mInHotPatching[0] + "出错!" : errMsg);
        mInHotPatching = null;
        mHotPatchList.Clear();

        mDownloadState = 0;
        mCheckFileStart = 0;
        PlayMoreDone = 0;

        EvtDsp.TriggerEvt(GameEventDefine.GAME_VERSION_CHECK_INFO_GET_FAILED);
    }

    private void _CancelDoPlayMore()
    {
        HttpDownloadManager.CancelDownloadFile();

        mInHotPatching = null;
        mHotPatchList.Clear();

        mDownloadState = 0;
        mCheckFileStart = CurCheckFiles = TolCheckFiles = 0;
        PlayMoreDone = 0;
        mPlayMoreCompleteFunc = null;
        mTaskQueueCount++;

        if (mTask != null) 
        {
            Debug.Log("<color=red>热更任务断开..</color>");

            //mTask.Wait(); 
            //mTask.Dispose();

            mTask = null; 
        }
    }


    //自动化更新下载文件流程-
    //private string mBuffFileToPath = "";
    private void UpdateHotPatch()
    {
        if( mDownloadState >0)
        {
            //从下载缓冲信息中提取并开始下载-
            if (mHotPatchList.Count > 0)
            {
                if(mInHotPatching == null)
                {
                    string fileToPath = HttpDownloadManager.DownloadPath + mHotPatchList[0][0];

                    //确定文件需要下载-
                    if (!string.IsNullOrEmpty(fileToPath) )
                    {
                        //-
                        mInHotPatching = mHotPatchList[0];
                        mHotPatchList.RemoveAt(0);

                        //-
                        EvtDsp.TriggerEvt<int, int, string>(GameEventDefine.GAME_VERSION_CHECK_FILE, mHotPatchList.Count, mHotPatchListFinalCount, mInHotPatching[0]);

                        //开始下载-
                        //这里关于断点续传问题,这里不删除原来已经有的文件，
                        //在下载内部会判断已有文件大小 <需要下载的大小，会自动进行断点续传，否则会删掉重新下载-
                        HttpDownloadManager.StartDownloadFile(URL_ADDRESS_DOWNLOAD, mInHotPatching[0], 
                            PlayMoreDone == 1 ? 2 : 6,   //根据2个模式决定下载速度调节-
                            mInHotPatching[1],
                            (string s, int n) => {

                                if (n > 0)
                                {
                                    //下载完成后校验文件-
                                    string tmpFileToPath = HttpDownloadManager.DownloadPath + mInHotPatching[0];
                                    
                                    mTask = new Task(() =>
                                    {
                                        string fileMd5 = StringEncodeMgr.GetMd5(ResourceMgr.LoadIOFileAnsy(tmpFileToPath).Result);
                                        if (fileMd5 == mInHotPatching[1])
                                        {
                                            //删除原始文件替换到原来位置-
                                            string oldFilePath = ResourceMgr.BUNDLE_ABS_PATH + mInHotPatching[0];
                                            File.Delete(oldFilePath);
                                            File.Move(tmpFileToPath, oldFilePath);

                                            //-
                                            mDownloadState = 1;
                                            mInHotPatching = null;
                                            return;
                                        } else {
                                            //一切形式的文件下载问题，会导致最后重新下载-
                                            _clear_hotpatch_buffer();
                                        }
                                    });

                                    mTask.Start();

                                    return ;
                                }

                                //一切形式的文件下载问题，会导致最后重新下载-
                                _clear_hotpatch_buffer();
                            });
                    } else {
                        //不需要下载操作，已经有本地现成的文件了-
                        mHotPatchList.RemoveAt(0);
                        mInHotPatching = null;
                    }

                }
            }

            //正在下载文件进度通知-
            if(mInHotPatching != null)
            {
                EvtDsp.TriggerEvt<float>(GameEventDefine.GAME_VERSION_DOWNLOAD_FILE_PROCESS, HttpDownloadManager.DownloadProcessAt);
            }


            //检查到此时已经全部都下载校对完毕了，
            //最后写入配置文件-
            if(mHotPatchList.Count <1 && mInHotPatching == null && mDownloadState >0)
            {
                string OldVersion = GameServiceMgr.GetAppVersion();
                string NewVersion = GameServiceMgr.GetNewAppVersion();
                string localFilePath = ResourceMgr.BUNDLE_ABS_PATH + "HotCompare.db";
                string newFilePath = HttpDownloadManager.DownloadPath + "HotCompare.db";

                //检查到更新缓存内容，就复写旧的本地配置-
                if( mHotPatchListFinalCount >0 ||
                    OldVersion != NewVersion )
                {
                    //比较本地文件，看看是否需要重新更新到新文件-
                    File.Delete(localFilePath);
                    File.Move(newFilePath, localFilePath);

                    //重新写入版本-
                    GameServiceMgr.DbOldInfos = GameServiceMgr.DbInfos;

                    //根据新版本内容删除列表里不存在的文件-
                    mTask = new Task(() => { 
                        string[] downLoadInfoFiles = GameServiceMgr.DbAppUpdateFiles;
                        string[] existFile = Directory.GetFiles(ResourceMgr.BUNDLE_ABS_PATH, "*.*", SearchOption.TopDirectoryOnly);
                        foreach (string pathName2 in existFile)
                        {
                            if (string.IsNullOrEmpty(pathName2)) { continue; }

                            bool   isExist = false;
                            string fileName2 = Path.GetFileName(pathName2);
                            if (fileName2.Contains("HotCompare.db")) { continue; }

                            foreach ( string pathName in downLoadInfoFiles )
                            {
                                string[] fileName = pathName.Split('|');
                                if (fileName[0].ToLower() == fileName2.ToLower()) { isExist = true; break; }
                            }

                            if (!isExist) { File.Delete(pathName2); }
                        }
                    });
                    mTask.Start();

                    //清理download缓存目录的临时文件-
                    if (Directory.Exists(HttpDownloadManager.DownloadPath)) { Directory.Delete(HttpDownloadManager.DownloadPath); }
                    Directory.CreateDirectory(HttpDownloadManager.DownloadPath);
                }


                //退出下载流程-
                mDownloadState = 0;

                //如果是边玩边下模式那么设置完成-
                if (PlayMoreDone == 1) { PlayMoreDone = 6; mPlayMoreCompleteFunc?.Invoke(); mPlayMoreCompleteFunc = null; }

                //流程结束通知-
                EvtDsp.TriggerEvt<string>(GameEventDefine.GAME_VERSION_CHECK_END, Version);

                Debug.Log(string.Format("<color=yellow>最新获得的版本为:{0}</color>", Version) );
            }
        }
        else
        {
            if (mCheckFileStart > 0)
            { EvtDsp.TriggerEvt<int, int>(GameEventDefine.GAME_VERSION_CHECK_FILE_PROCESS, CurCheckFiles, Math.Max(TolCheckFiles, 1) ); }
        }

    }
}
