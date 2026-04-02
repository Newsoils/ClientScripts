using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Linq;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif



namespace GameCoreResourceLoad
{
    //资源加载器-
    public class ResourceMgr : MonoBehaviour
    {
        //从Bundle里加载-
        static private ResourceMgr boot = null;

        //Bundle包访问途径方式-
        public enum BundleAccessType
        {
            EditorAssets = 0,               //访问在工程外围打包完成的资源缓冲存放地-
            StreamAssets = 1,               //在发布包时访问程序Assets/StreamAssets下的资源缓冲-
            HotpatchAssets = 2,             //也是正式访问形式,针对每个操作系统下工程安装的读写缓存中-
            EditorAssetDatabase = 3,        //纯粹在编辑器下跑编辑器缓冲镜像数据-
        }

        //加载途径类型-
        public enum LoadAccessType
        {
            FromResource = 0,               //用Unity特有的Resource目录内容进行加载访问-
            FromAssetBundle = 1,            //发布包加载访问-
            FromAssetDatabase = 2,          //Unity编辑器环境下加载Assets工程目录中的资源内容-

            FromDefault,
        }


        //通用字体名-
        public const string GFontName = "font";

        // 打包出的信息自定义格式-
        public const string AppDirName = "Bin";                         //发布的App目录-
        public const string BundleActionName = "BundlePag";             //包体输出到目录名-
        public const string BundleResExtName = ".bbs";                  //包格文件式-
        //public const string BundleResKeyExtName = ".bbn";               //包迭代更新验证文件格式-
        public const string BundleSceneExtName = ".scene";              //场景文件格式-
        public const string BundleRelativeExtName = ".rltif";
        public const string SceneDependenceInfoFileName = "mainland.rltif";
        public const int ocd = 0;

        // 打包源目录路径自定义-
        public const string BundleDestDir = "Assets/../";
        public const string BundleSrcDir = "Assets/Art/";                                 //热更总目录-
        public const string BundleSrcPlayMoreDir = "Assets/ArtPlayMore/";                 //边玩边下总目录-
        public const string BundleSrcScript = "Assets/JS/Resources";                           //热更新脚本目录-
        public const string BundleSrcScenes = "Assets/TestScenes/";                           //热更新场景，子场景目录-


        // 支持的资源文件格式
        private static readonly string[] ResourceExts = {
                                 ".prefab",
                                 ".mat", ".shader", ".hlsl", ".shadervariants",
                                 ".png", ".jpg", ".dds", ".gif", ".psd", ".tga", ".bmp", "exr",
                                 ".fbx", ".asset", ".anim", ".controller", ".mesh",
                                 ".wav", ".mp3", ".ogg", ".ttf", ".otf",
                                 ".txt", ".bytes", ".xml", ".csv", ".json", ".lua", ".js",
                                 ".unity",
                                 ".scene", ".bbs", ".rltif"};

        private static readonly string[] ResourceTexExts = { ".png", ".jpg", ".tga", ".bmp", ".exr", ".dds", ".gif", ".psd", };
        private static readonly string[] ResourceShaderExts = { ".shader", ".hlsl", ".shadervariants", };
        private static readonly string[] ResourceSoundExts = { ".wav", ".mp3", ".ogg", };
        private static readonly string[] ResourceFontExts = { ".ttf", ".otf", };


        //Assets下资源访问的一些目录(最后一定要加'/'作为解析路径识别)-
        //编辑器模式下访问的搜索目录集-
        public static string[] RES_FIND_PATHS_ASSETDATABASE_SCRIPT = new string[] { "Assets/Export/", "Assets/Art/DB/", "Assets/ArtPlayMore/DB/", };
        public static string[] RES_FIND_PATHS_ASSETDATABASE_SCENE = new string[] { "Assets/Scenes/", "Assets/ArtPlayMore/Scenes/" };
        public static string[] RES_FIND_PATHS_ASSETDATABASE_PREFAB = new string[] { "Assets/Art/Prefab/", "Assets/ArtPlayMore/Prefab/", };
        public static string[] RES_FIND_PATHS_ASSETDATABASE_RES = new string[] { "Assets/Art/Res/", "Assets/ArtPlayMore/Res/", "Assets/Art/DB/", "Assets/ArtPlayMore/DB/", };
        public static string[] RES_FIND_PATHS_ASSETDATABASE_READY = new string[] {
                                                                                    "Assets/Export/",
                                                                                    "Assets/Scenes/",
                                                                                    "Assets/Art/Prefab/",
                                                                                    "Assets/ArtPlayMore/Prefab/",
                                                                                    "Assets/Art/Res/",
                                                                                    "Assets/ArtPlayMore/Res/",
                                                                                    "Assets/Art/DB/",
                                                                                    "Assets/ArtPlayMore/DB/", 
                                                                                  };


        //编辑器Resource访问模式下的搜索目录集-
        static public string[] RES_FIND_PATHS = new string[] {
                                                    "ReadyBundle/Prefab/",
                                                    "ReadyBundle/Art/",
                                                    "ReadyBundle/DB/|chinese",                //一种分支格式,|后半部分描述在资源名相同的情况下默认采样哪个分支-
                                                    "ReadyBundle/LuaScript/",
                                                    "ReadyBundle/SubScene/",
                                                    "Setting/",
                                                };




        //语言文件读写-
        public static string Language
        {
            get {
                string pld = PlayerPrefs.GetString("PackLanguageDir", "");

                if (string.IsNullOrEmpty(pld))
                {
                    //如果当时配置不存在, 新建加载默认配置里的配置-
                    pld = CreateLoadPrefile_Language("chinese");
                    PlayerPrefs.SetString("PackLanguageDir", pld);
                }

                return pld;
            }
        }


        //加载Bundle合集来源-
        //注意!:这里是整个ResourceMgr模块配置的主入口-
        private static BundleAccessType mBUNDLE_ACCESS = BundleAccessType.EditorAssets;
        public static BundleAccessType BUNDLE_ACCESS
        {
            get
            {
                int ba = CreateLoadPrefile_BundleAccess(PlayerPrefs.GetInt("RMBundleAccess", -1));
                //Debug.Log("<color=blue>ResourceMgr:资源加载模式为</color>::::::" + ba);
                if (ba < 0)
                {
                    //如果当时配置不存在, 新建加载默认配置里的配置-
                    ba = CreateLoadPrefile_BundleAccess(1);
                    PlayerPrefs.SetInt("RMBundleAccess", ba);

                    Debug.Log("<color=red>ResourceMgr:BUNDLE_ACCESS配置加载失败，重置为默认</color>::::::" + ba);
                }

                //-
                if (Application.isPlaying)
                {
                    mBUNDLE_ACCESS = (BundleAccessType)ba;
                    switch ((BundleAccessType)ba)
                    {
                        case BundleAccessType.StreamAssets:
                            Debug.Log("<color=blue>ResourceMgr:资源加载模式为StreamAssets</color>");
                            LATMode = LoadAccessType.FromAssetBundle;
                            if (Application.platform == RuntimePlatform.Android)
                            {
                                //两种路径拼凑方式其实是一样的-
                                //BUNDLE_PATH = "jar:file://" + Application.dataPath + "!/assets/" + BundleActionName + "/";
                                BUNDLE_PATH = Application.streamingAssetsPath + "/" + BundleActionName + "/";
                            } else {
                                if (Application.platform == RuntimePlatform.IPhonePlayer)
                                {
                                    BUNDLE_PATH = Application.dataPath + "/Raw/" + BundleActionName + "/";
                                } else {
                                    //默认平台-
                                    BUNDLE_PATH = Application.streamingAssetsPath + "/" + BundleActionName + "/";
                                }
                            }

                            //纯测试-
                            //LoadIOFileAPK(BUNDLE_PATH + "logicmodule.rltif",
                            //    (byte[] data) => {
                            //        if (data != null) { Debug.LogError("11111111111111155555555555555554444sssss:" + data.Length); }
                            //        else {
                            //            Debug.LogError("1111111111111117777777777777777755555555555555554444:" + BUNDLE_PATH + "logicmodule.rltif");
                            //        }
                            //    });

                            break;

                        case BundleAccessType.HotpatchAssets:
                            Debug.Log("<color=blue>ResourceMgr:资源加载模式为HotpatchAssets</color>");
                            LATMode = LoadAccessType.FromAssetBundle; BUNDLE_PATH = Application.persistentDataPath + "/" + BundleActionName + "/";
                            ////纯测试-
                            //byte[] data = LoadIOFile(BUNDLE_PATH + "logicmodule.rltif");
                            //if (data != null && data.Length >0) { Debug.LogError("11111111111111155555555555555554444sssss:" + data.Length); }
                            //else
                            //{
                            //    Debug.LogError("1111111111111117777777777777777755555555555555554444:" + BUNDLE_PATH + "logicmodule.rltif");
                            //}

                            break;

                        case BundleAccessType.EditorAssets:
                            Debug.Log("<color=blue>ResourceMgr:资源加载模式为EditorAssets</color>");
                            LATMode = LoadAccessType.FromAssetBundle; BUNDLE_PATH = Application.dataPath + "/../" + BundleActionName + "/"; break;

                        default: case BundleAccessType.EditorAssetDatabase:
                            Debug.Log("<color=blue>ResourceMgr:资源加载模式为EditorAssetDatabase</color>");
                            LATMode = LoadAccessType.FromAssetDatabase; BUNDLE_PATH = ""; break;
                    }
                }

                return (BundleAccessType)ba;
            }
        }

        public static void BUNDLE_ACCESS_READ(Action func)
        {
            bundleLoadTable.Clear();
            resourceLoadTable.Clear();
            if (mBUNDLE_ACCESS != BundleAccessType.EditorAssetDatabase)
            {
                GetBundleFilesDesc((string[] pathBuffer) =>
                {
                    LoadBundleInfoHead(pathBuffer, () => {
                        LoadMainlandInfoHead(() => {
                            LoadLanguageBundleHead(ResourceMgr.Language, () => {
                                func?.Invoke();
                            });
                        });
                    });
                });
            } else {
                //缓存入Resources加载方式信息头-
                LoadResourcesInfoTxtFile();
                func?.Invoke();
            }
        }


        //加载途径设置-
        public static LoadAccessType LATMode = LoadAccessType.FromAssetDatabase;


        //BUNDLE包读取目录-
        //正式会拼接上设备所在的绝对路径比方说预定义的 Application.persistentDataPath 等-
        static private string mBundleAtPath = string.Empty;
        static public string BUNDLE_PATH
        {
            get { return mBundleAtPath; }
            set
            {
                mBundleAtPath = value;
                BUNDLE_ABS_PATH = Application.persistentDataPath + "/" + BundleActionName + "/";
                if(!Directory.Exists(BUNDLE_ABS_PATH))
                {
                    Debug.Log("<color=blue>ResourceMgr:BUNDLE_ABS_PATH文件源目录为:" + BUNDLE_ABS_PATH + " </color>");
                    Directory.CreateDirectory(BUNDLE_ABS_PATH);
                }

                //-
                bundleLoadTable.Clear();
                
                if (!string.IsNullOrEmpty(mBundleAtPath) &&
                    mBUNDLE_ACCESS != BundleAccessType.EditorAssetDatabase)
                {
                    if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
                    {
                        if(mBUNDLE_ACCESS == BundleAccessType.HotpatchAssets)
                        {
                            if (!Directory.Exists(BUNDLE_PATH))
                            {
                                Directory.CreateDirectory(BUNDLE_PATH);
                                Debug.Log("<color=red>ResourceMgr:资源Bundle文件源目录" + BUNDLE_PATH + "读取失败!自动创建热更目录 </color>");
                            }
                            else
                            {
                                if (!Directory.Exists(BUNDLE_PATH)) { Debug.Log("<color=red>ResourceMgr:资源Bundle文件源目录" + BUNDLE_PATH + "读取失败! </color>"); return; }
                            }
                        }
                    }
                }
            }
        }

        //忽略加载模式的绝对BUNDLE写入目录-
        public static string BUNDLE_ABS_PATH = "";


        //-
        static private AssetBundle                          mMainBundle = null;                                                         //Bundle包合集信息头部-
        static private AssetBundleManifest                  mManifest = null;

        private static Dictionary<string, string[]>         mainlandLoadTable = new Dictionary<string, string[]>(8);                    //母包场景加载依赖信息-
        private static Dictionary<string, AssetBundle>      loadedBundle = new Dictionary<string, AssetBundle>(0);                      //已经加载的Bundle-
        private static Dictionary<string, RMAssetRequire>   loadedResources = new Dictionary<string, RMAssetRequire>(0);                //已经加载的ResourcesAsset-

        static private Dictionary<string, string>           resourceLoadTable = new Dictionary<string, string>();                       //记录着资源名对应的所在Resources下的目录-
        static private Dictionary<string, string[]>         bundleLoadTable = new Dictionary<string, string[]>();                       //记录着资源名对应的所在bundle包-


        static private Action<AssetBundle>                  collectBundleFunc = null;



        //-
        private void Awake()
        {
            boot = this;

            //BUNDLE_PATH的最终路径定义(临时的)-
            mBundleAtPath = Application.dataPath + "\\../BundlePag/";
        }

        private void OnDestroy()
        {
            StopAllAnsyLoad();
            ClearAllAssets();
            ClearAssetBundle();

            boot = null;
        }


        #region 清理-
        //清理所有
        public static void ClearAll()
        {
            void _var_c()
            {
                mMainBundle = null; mManifest = null;
                collectBundleFunc = null;
                loadedBundle.Clear();
                loadedResources.Clear();
                resourceLoadTable.Clear();
                bundleLoadTable.Clear();
                mainlandLoadTable.Clear();
            }

            if (boot == null) 
            {
                Debug.Log("<color=blue>ResourceMgr:游戏未启动模式清空缓存</color>");

                _var_c();
                return; 
            }
            

            StopAllAnsyLoad();
            ClearAllAssets();
            ClearAssetBundle();
            _var_c();
        }

        //清理正在加载中的数据-
        private static void StopAllAnsyLoad()
        {
            if (boot == null) { return; }

            boot.mResourceNowAnsyLoad = string.Empty;
            boot.mResourceAnsyLoadArray.Clear();

            boot.mBundleNowAnsyLoad = string.Empty;
            boot.mBundleAnsyLoadArray.Clear();

            boot.StopCoroutine("_LoadResourcesAnsyInnerIEnumerator");
            boot.StopCoroutine("_LoadBundleAnsyInnerIEnumerator");
        }


        //清理已经加载的Resource资产-
        //注意:resName这个命名
        //     如果是同步方法加载的loadedResources.keys为"资源名.扩展名",  
        //     如果是异步方法加载的loadedResources.keys为"资源完整路径(含资源名,但无扩展名)" -
        static public void ClearAllAssets()
        {
            if (boot == null) { return; }


            List<RMAssetRequire> resTemp = new List<RMAssetRequire>(loadedResources.Values);
            for (int i = 0; i < resTemp.Count; i++)
            {
                if (resTemp[i].Module == null) { continue; }

                //Debug.Log(string.Format("释放资源:{0}{1}", resTemp[i].name, Application.isEditor ? "(编辑器模式下)" : "") );

                //只能释放非GameObject类型资源-
                UnityEngine.Object  assetObj = resTemp[i].Module;
                GameObject          gameObject = assetObj != null ? assetObj as GameObject : null;
                if (gameObject == null)
                {
                    Resources.UnloadAsset(resTemp[i].Module);
                }
            }

            loadedResources.Clear();


            //释放GameObject类型的资源只能用这个接口-
            Resources.UnloadUnusedAssets();
        }


        static public void DisposeOneAsset(string resName)
        {
            string lowResName = resName.ToLower();

            if (loadedResources.ContainsKey(lowResName) )
            {
                UnityEngine.Object  assetObj = loadedResources[lowResName].Module;
                GameObject          gameObject = assetObj != null ? assetObj as GameObject : null;
                    
                loadedResources.Remove(lowResName);
                if (gameObject == null)
                {
                    Resources.UnloadAsset(assetObj);
                    assetObj = null;
                } else {
                    //释放GameObject类型的Asset只能用这个接口-
                    Resources.UnloadUnusedAssets();
                }
            }
        }



        //清理已经加载的bundle-
        public static void ClearAssetBundle(string bundleName, bool clearClone = true)
        {
            if (loadedBundle.ContainsKey(bundleName))
            {
                loadedBundle[bundleName].Unload(clearClone);
                loadedBundle.Remove(bundleName);

                return;
            }

            string useBundleName;
            if (!bundleName.Contains(BundleResExtName)) { useBundleName = bundleName + BundleResExtName; } else { useBundleName = bundleName; }
            if (loadedBundle.ContainsKey(useBundleName))
            {
                loadedBundle[useBundleName].Unload(clearClone);
                loadedBundle.Remove(useBundleName);

                return;
            }

            if (!bundleName.Contains(BundleSceneExtName)) { useBundleName = bundleName + BundleSceneExtName; } else { useBundleName = bundleName; }
            if (loadedBundle.ContainsKey(useBundleName))
            {
                loadedBundle[useBundleName].Unload(clearClone);
                loadedBundle.Remove(useBundleName);

                return;
            }
        }

        public static void ClearAssetBundle()
        {
            if(mMainBundle != null)
            {
                mMainBundle.Unload(true); mMainBundle = null;
            }

            //-
            List<AssetBundle> resTemp = new List<AssetBundle>(loadedBundle.Values);
            for (int i = 0; i < resTemp.Count; i++)
            {
                if (resTemp[i] == null) { continue; }
                resTemp[i].Unload(true);
            }

            loadedBundle.Clear();
        }

        #endregion


        #region 加载资源过程中的记录-
        //设置加载bundle时的回调-
        public static void SetCollectBundleFunc(Action<AssetBundle> func)
        {
            collectBundleFunc = func;
        }

        //记录已经加载的资源-
        //记录的名字resName都是原始输入的-
        private static void StoreLoadedAsset(string resName, string bdName, string assetPath, UnityEngine.Object resObject)
        {
            resName = resName.ToLower();
            if (!loadedResources.ContainsKey(resName))
            {
                loadedResources.Add(resName, new RMAssetRequire
                {
                    Name = resName,
                    BundleName = bdName,
                    AssetPath = assetPath,
                    Module = resObject,
                    ItorCount = 1,
                    loadState = resObject == null ? -1 : 1,
                });
            } /*else {
                RMAssetRequire rMAssetRequire = loadedResources[resName];
                rMAssetRequire.ItorCount++;
                loadedResources[resName] = rMAssetRequire;
            }*/
        }

        private static T LoadStoreAsset<T>(string resName) where T : UnityEngine.Object
        {
            resName = resName.ToLower();
            if (loadedResources.ContainsKey(resName))
            {
                RMAssetRequire rMAssetRequire = loadedResources[resName];
                rMAssetRequire.ItorCount++;
                loadedResources[resName] = rMAssetRequire;
                return rMAssetRequire.Module == null ? null : rMAssetRequire.Module as T;
            }

            return null;
        }

        public static RMAssetRequire GetAssetRequire(string resName)
        {
            if (loadedResources.ContainsKey(resName)) { return loadedResources[resName]; }
            return default;
        }

        //清理bundle加载的Asset-
        public static void DestroyAsset(string assetName)
        {
            if (loadedResources.ContainsKey(assetName))
            {
                RMAssetRequire rMAssetRequire = loadedResources[assetName];

                //这里:因为Asset镜像在bundle区域中仅此一份，所以无需卸载，只需要卸载与之对应的bundle即可-
                //if(rMAssetRequire.Module != null) { UnityEngine.Object.Destroy(rMAssetRequire.Module); }

                loadedResources.Remove(assetName);
            }
        }


        /// <summary>
        /// 给包文件放入加密头-
        /// 当 ResourceMgr.ocd = 0 时，不会对 AB 包进行任何加密处理。
        /// BundleEncode 逻辑将原文件原封不动写回，因此等同于未加密状态。
        /// </summary>
        /// <param name="filePath"></param>
        public static void BundleEncode(string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] oldBytes = File.ReadAllBytes(filePath);
                long newByteLen = ResourceMgr.ocd + oldBytes.Length;

                byte[] newBytes = new byte[newByteLen];
                for (int i = 0; i < oldBytes.Length; i++) { newBytes[ResourceMgr.ocd + i] = oldBytes[i]; }
                for (int i = 0; i < ResourceMgr.ocd; i++) { newBytes[i] = 123; }

                FileStream fs = File.OpenWrite(filePath);

                fs.Write(newBytes, 0, newBytes.Length);

                fs.Flush();
                fs.Close();
            }
        }
        #endregion



        #region io方式加载文件-
        //-
        public static void WriteIOFile(string filePath, string content)
        {
            FileStream writeStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            if (writeStream != null)
            {
                System.Text.UTF8Encoding encoding_utf8_withOutBOM = new System.Text.UTF8Encoding(false);
                byte[] fileBytes = encoding_utf8_withOutBOM.GetBytes(content);
                writeStream.Write(fileBytes, 0, fileBytes.Length);

                writeStream.Flush();
                writeStream.Close();
                writeStream.Dispose();
            }
        }

        //io方式的文件加载-
        public static byte[] LoadIOFile(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    //打开这个文件-
                    FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    byte[] content = new byte[fs.Length];
                    fs.Read(content, 0, (int)fs.Length);

                    fs.Flush();
                    fs.Close();

                    return content;
                }
                else
                {
                    throw (new System.Exception());
                }
            }
            catch (System.Exception)
            {
                Debug.LogError(string.Format("读取文件{0}错误", fullPath));
            }

            return new byte[0];
        }

        public static async Task<byte[]> LoadIOFileAnsy(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    //打开这个文件-
                    FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    byte[] content = new byte[fs.Length];

                    //这里注意: Result使用方法会造成当前线程阻塞,等待直到任务完成，所以在主线程里使用是危险的！-
                    var res = await fs.ReadAsync(content, 0, (int)fs.Length);

                    fs.Flush();
                    fs.Close();

                    return content;
                }
            }
            catch (System.Exception) {  Debug.LogError(string.Format("读取文件{0}错误", fullPath)); }

            return default;
        }


        public static void LoadIOFileAPK(string fullPath, Action<byte[],string> func)
        {
            boot?._LoadIOFileAPK(fullPath, func);
        }

        public void _LoadIOFileAPK(string fullPath, Action<byte[],string> func)
        {
            StartCoroutine(_LoadIOFileAPKProcess(fullPath, func));
        }

        IEnumerator _LoadIOFileAPKProcess(string fullPath, Action<byte[],string> func)
        {
            //Uri uri = new System.Uri(fullPath);
            var request = UnityWebRequest.Get(fullPath);
            //Debug.LogError("Apk打包目录:" + fullPath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("APK包文件加载失败!");
                yield break;
            }

            byte[] bytes = request.downloadHandler.data;

            func?.Invoke(bytes, fullPath);
        }
        #endregion



        #region Resources加载方式下编辑器菜单操作-
        public static void WriteTxtFile(string content, string pathName)
        {
            //写入到文件中-
            string save_path = pathName;

            FileStream file = new FileStream(save_path, FileMode.Create);

            if (file != null && file.CanWrite)
            {
                //得到字符串的UTF8 数据流-
                System.Text.UTF8Encoding encoding_utf8_withoutBOM = new System.Text.UTF8Encoding(false);
                byte[] bts = encoding_utf8_withoutBOM.GetBytes(content);

                file.Write(bts, 0, bts.Length);

                if (file != null)
                {
                    file.Flush();
                    file.Close();
                    file.Dispose();
                }
            }
        }


        //罗列在Resources目录下的结构关系-并且保存成信息文件到Resources跟目录下-
        public static void CreateResourcesInfoTxtFile()
        {
            Dictionary<string, string> infos = new Dictionary<string, string>();
            foreach (string path in RES_FIND_PATHS)
            {
                string[] pathSetting = path.Split('|');

                string banchName = pathSetting.Length > 1 ? pathSetting[1] : "";
                banchName = banchName.ToLower();

                string fullPath = pathSetting[0];
                fullPath = "Assets/Resources/" + fullPath;

                if (Directory.Exists(fullPath))
                {
                    foreach (string fileExt in ResourceExts)
                    {
                        var files = Directory.GetFiles(fullPath, "*" + fileExt, SearchOption.AllDirectories).ToList();
                        foreach (string fileFullPath in files)
                        {
                            string assetFileFullPath = fileFullPath.Replace("\\", "/");
                            int index = assetFileFullPath.LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase);

                            //找到后返回-
                            string retFileName = assetFileFullPath.Substring(index + 1);
                            retFileName = retFileName.Substring(0, retFileName.LastIndexOf("."));

                            string retFileResourcePath = assetFileFullPath.Substring(assetFileFullPath.LastIndexOf("Resources", StringComparison.CurrentCultureIgnoreCase));
                            retFileResourcePath = retFileResourcePath.Substring(retFileResourcePath.IndexOf("/", StringComparison.CurrentCultureIgnoreCase) + 1);
                            retFileResourcePath = retFileResourcePath.Substring(0, retFileResourcePath.LastIndexOf("."));

                            //存入信息池-
                            if (!infos.ContainsKey(retFileName))
                            {
                                infos.Add(retFileName, retFileResourcePath);
                            } else {
                                //产生资源名相同时，指定到目录名的那个采样-
                                if (pathSetting.Length > 1 &&
                                    retFileResourcePath.Contains(banchName))
                                {
                                    infos[retFileName] = retFileResourcePath;
                                }
                            }
                        }

                    }
                }
            }


            //建立文件内容-
            string content = "";
            foreach (string key in infos.Keys)
            {
                content += (key + "|" + infos[key] + "\n");
            }



            //写入到文件中-
            string save_path = "Assets/Resources/Setting/ResourcesIndexMark.txt";

            FileStream file = new FileStream(save_path, FileMode.Create);

            //得到字符串的UTF8 数据流-
            System.Text.UTF8Encoding encoding_utf8_withoutBOM = new System.Text.UTF8Encoding(false);
            byte[] bts = encoding_utf8_withoutBOM.GetBytes(content);

            file.Write(bts, 0, bts.Length);

            if (file != null)
            {
                file.Flush();
                file.Close();
                file.Dispose();
            }

            //-
            infos.Clear();
        }

        //读取Resources加载相关的信息配置文件到缓存-
        public static void LoadResourcesInfoTxtFile()
        {
            resourceLoadTable.Clear();

            //-
            if (mBUNDLE_ACCESS == BundleAccessType.EditorAssetDatabase &&
                LATMode == LoadAccessType.FromAssetDatabase)
            {
                foreach (string path in RES_FIND_PATHS_ASSETDATABASE_READY)
                {
                    string fullPath = path;

                    //拼凑出语言包路径-
                    if (fullPath.Contains("DB/"))
                    {
                        fullPath += ResourceMgr.Language + "/";
                    }

                    if (!File.Exists(fullPath)) { continue; }

                    //-
                    var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories).ToList();
                    foreach(string fp in files)
                    {
                        if (fp.Contains(".meta")) { continue; }
                        string fn = Path.GetFileName(fp).ToLower();
                        if(!resourceLoadTable.ContainsKey(fn))
                        {
                            string finalPath = GetAssetRelativePath(fp, "Assets/");
                            resourceLoadTable.Add(fn, finalPath);
                        }
                    }
                }
            } else { 
                //-
                string save_path = "Setting/ResourcesIndexMark";

                var ta = Resources.Load<TextAsset>(save_path);
                if (ta != null)
                {
                    string[] lines = ta.text.Split('\n');
                    foreach (string line in lines)
                    {
                        string[] words = line.Split('|');
                        if (words.Length > 1 && !string.IsNullOrEmpty(words[0]) && !resourceLoadTable.ContainsKey(words[0]))
                        {
                            resourceLoadTable.Add(words[0], words[1]);
                        }
                    }
                } else {
                    Debug.LogWarning("Resources目录访问方式使用的资源关系文件读取失败!");
                }
            }
        }

        //-
        public static string GetResourcesDictionaryInfo(string fileName) { if (resourceLoadTable.ContainsKey(fileName)) { return resourceLoadTable[fileName]; } return ""; }
        #endregion


        #region 语言,本地版本号-  
        //读取本地母包的版本号-
        public static string LoadLocalVersion()
        {
            string version = "0.0.0.0";
            string save_path = "Setting/ReleaseFileVersion";
            var ta = Resources.Load<TextAsset>(save_path);
            if (ta != null)
            {
                string[] lines = ta.text.Split('\n');
                if (lines.Length > 3)
                {
                    string[] versionTags = new string[4];

                    version = string.Format("{1}.{2}.{3}.{0}",
                        lines[0].Substring(lines[0].LastIndexOf("]") + 1),
                        lines[1].Substring(lines[1].LastIndexOf("]") + 1),
                        lines[2].Substring(lines[2].LastIndexOf("]") + 1),
                        lines[3].Substring(lines[3].LastIndexOf("]") + 1));

                    version = version.Replace("\r", "");
                }
            }

            return version;
        }


        //读取记录语言模式-
        //write:强制作为写入开关-
        public static string CreateLoadPrefile_Language(string defauleValue, bool write = false)
        {
            string save_path = "Assets/Resources/RMSetting/LanguageFrom.txt";
            string load_path = "RMSetting/LanguageFrom";
            string save_dir = "Assets/Resources/RMSetting/";

            var ta = Resources.Load<TextAsset>(load_path);
            if (ta != null && !write)
            {
                return ta.text.Split('\n')[0];
            }

            if (!Directory.Exists(save_dir)) { Directory.CreateDirectory(save_dir); }

            FileStream file = new FileStream(save_path, FileMode.Create);
            string content = "" + defauleValue;

            //得到字符串的UTF8 数据流-
            System.Text.UTF8Encoding encoding_utf8_withoutBOM = new System.Text.UTF8Encoding(false);
            byte[] bts = encoding_utf8_withoutBOM.GetBytes(content);

            file.Write(bts, 0, bts.Length);

            if (file != null)
            {
                file.Flush();
                file.Close();
                file.Dispose();
            }

            return defauleValue;

        }


        //读取以及记录BundleAccess模式-
        //write:强制作为写入开关-
        public static int CreateLoadPrefile_BundleAccess(int defauleValue, bool write = false)
        {
            string save_path = "Assets/Resources/RMSetting/BundleAccessFrom.txt";
            string load_path = "RMSetting/BundleAccessFrom";
            string save_dir = "Assets/Resources/RMSetting/";

            var ta = Resources.Load<TextAsset>(load_path);
            if (ta != null && !write)
            {
                return System.Convert.ToInt32(ta.text.Split('\n')[0]);
            }

            if (!Directory.Exists(save_dir)) { Directory.CreateDirectory(save_dir); }

            FileStream file = new FileStream(save_path, FileMode.Create);
            string content = "" + defauleValue;

            //得到字符串的UTF8 数据流-
            System.Text.UTF8Encoding encoding_utf8_withoutBOM = new System.Text.UTF8Encoding(false);
            byte[] bts = encoding_utf8_withoutBOM.GetBytes(content);

            file.Write(bts, 0, bts.Length);

            if (file != null)
            {
                file.Flush();
                file.Close();
                file.Dispose();
            }

            return defauleValue;
        }


        #endregion


        #region 声音剪辑加载
        public static void LoadAnsyAudioClip(string fileName, Action<AudioClip> func, out string writeName)
        {
            fileName = fileName.ToLower();
            if (LATMode == LoadAccessType.FromResource)
            {
                string nameTemp = fileName;
                if (!nameTemp.Contains(".ogg")) { nameTemp += ".ogg"; }
                string assetPath = FindResourcePath(nameTemp, RES_FIND_PATHS);

                if (string.IsNullOrEmpty(assetPath))
                {
                    nameTemp = fileName;
                    if (!nameTemp.Contains(".wav")) { nameTemp += ".wav"; }
                    assetPath = FindResourcePath(nameTemp, RES_FIND_PATHS);
                }

                if (string.IsNullOrEmpty(assetPath))
                {
                    nameTemp = fileName;
                    if (!nameTemp.Contains(".mp3")) { nameTemp += ".mp3"; }
                    assetPath = FindResourcePath(nameTemp, RES_FIND_PATHS);
                }

                writeName = nameTemp;
                LoadFromResourcesAnsy(nameTemp, assetPath, (AudioClip clip, GameObject _shell) =>
                {
                    if (func != null) { func(clip); }
                }, null);
            }

            //从bundle中实现加载-
            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;

                string nameTemp = fileName;
                if (!nameTemp.Contains(".wav")) { nameTemp += ".wav"; }
                if (bundleLoadTable.ContainsKey(nameTemp))
                {
                    bundleName = bundleLoadTable[nameTemp][0];
                    fileName = nameTemp;
                } else {
                    nameTemp = fileName;
                    if (!nameTemp.Contains(".ogg")) { nameTemp += ".ogg"; }
                    if (bundleLoadTable.ContainsKey(nameTemp))
                    {
                        bundleName = bundleLoadTable[nameTemp][0];
                        fileName = nameTemp;
                    } else {
                        nameTemp = fileName;
                        if (!nameTemp.Contains(".mp3")) { nameTemp += ".mp3"; }
                        if (bundleLoadTable.ContainsKey(nameTemp))
                        {
                            bundleName = bundleLoadTable[nameTemp][0];
                            fileName = nameTemp;
                        }
                    }
                }

                writeName = nameTemp;
                LoadFromBundleAnsy<AudioClip>(fileName, bundleName, (AudioClip clip, AssetBundle atBundle) =>
                {
                    if (func != null) { func(clip); }
                });
            } else {
#if UNITY_EDITOR
                string nameTemp = fileName;
                if (!nameTemp.Contains(".ogg")) { nameTemp += ".ogg"; }
                string assetPath = FindResourcePath(nameTemp, RES_FIND_PATHS_ASSETDATABASE_RES);

                if (string.IsNullOrEmpty(assetPath))
                {
                    nameTemp = fileName;
                    if (!nameTemp.Contains(".wav")) { nameTemp += ".wav"; }
                    assetPath = FindResourcePath(nameTemp, RES_FIND_PATHS_ASSETDATABASE_RES);
                }

                if (string.IsNullOrEmpty(assetPath))
                {
                    nameTemp = fileName;
                    if (!nameTemp.Contains(".mp3")) { nameTemp += ".mp3"; }
                    assetPath = FindResourcePath(nameTemp, RES_FIND_PATHS_ASSETDATABASE_RES);
                }

                writeName = nameTemp;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (func != null) { func(clip); }
#else
                    writeName = "";
#endif
            }
        }
        #endregion


        #region 动画剪辑加载
        //从动画FBX文件中加载动画剪辑-
        public static AnimationClip LoadAnimationClipFromFbxFile(string fbxName, string clipName, out string writeName)
        {
            if (!fbxName.Contains(".fbx")) { fbxName += ".fbx"; }
            string fbxClipName = fbxName + "%" + clipName;
            fbxClipName = fbxClipName.ToLower();
            writeName = fbxClipName;

            if (LATMode == LoadAccessType.FromResource)
            {
                string assetPath = FindResourcePath(fbxName, RES_FIND_PATHS);
                //assetPath = assetPath.Substring(0, assetPath.LastIndexOf("."));         //Resources.Load加载方法是不需要扩展名的-

                var first = LoadStoreAsset<AnimationClip>(fbxClipName);
                if (first != null) { return first; }

                var clips_obj = Resources.LoadAll(assetPath);
                var anim_clip = (AnimationClip)clips_obj.FirstOrDefault((clip) => { return clip.name == clipName; });

                StoreLoadedAsset(fbxClipName, "", assetPath, anim_clip);

                return anim_clip;
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                var first = LoadStoreAsset<AnimationClip>(fbxClipName);
                if (first != null) { return first; }

                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fbxName)) { bundleName = bundleLoadTable[fbxName][0]; } else { return null; }

                var clips_obj = LoadSubAssetFromBundle(fbxName, bundleName);
                AnimationClip anim_clip = clips_obj.FirstOrDefault((clip) =>
                                                {
                                                    AnimationClip tmp = clip as AnimationClip;
                                                    if (tmp != null) { return tmp.name == clipName; }
                                                    return false;
                                                }) as AnimationClip;

                StoreLoadedAsset(fbxClipName, "", !bundleLoadTable.ContainsKey(fbxName) ? "" : bundleLoadTable[fbxName][1], anim_clip);           //这里暂时先这样，等启动时测试-

                return anim_clip;
            } else {
#if UNITY_EDITOR
                var first = LoadStoreAsset<AnimationClip>(fbxClipName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(fbxName, RES_FIND_PATHS_ASSETDATABASE_RES);
                var clips_obj = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                var anim_clip = (AnimationClip)clips_obj.FirstOrDefault((clip) => { return clip.name == clipName; });

                StoreLoadedAsset(fbxClipName, "", assetPath, anim_clip);

                return anim_clip;
#else
                    writeName ="";
                    return null;
#endif
            }
        }

        //从动画文件中加载动画剪辑--
        public static AnimationClip LoadAnimationClipFromClipFile(string clipFileName, out string writeName)
        {
            if (!clipFileName.Contains(".anim")) { clipFileName += ".anim"; }
            clipFileName = clipFileName.ToLower();
            writeName = clipFileName;

            if (LATMode == LoadAccessType.FromResource)
            {
                string assetPath = FindResourcePath(clipFileName, RES_FIND_PATHS);

                var first = LoadStoreAsset<AnimationClip>(clipFileName);
                if (first != null) { return first; }

                var clip = Resources.Load<AnimationClip>(assetPath);

                StoreLoadedAsset(clipFileName, "", assetPath, clip);
                return clip;
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(clipFileName)) { bundleName = bundleLoadTable[clipFileName][0]; } else { return null; }

                var clip = LoadFromBundle<AnimationClip>(clipFileName, bundleName);

                return clip;
            } else {
#if UNITY_EDITOR
                var first = LoadStoreAsset<AnimationClip>(clipFileName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(clipFileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

                StoreLoadedAsset(clipFileName, "", assetPath, clip);

                return clip;
#else
                    writeName ="";
                    return null;
#endif
            }
        }
        #endregion


        #region Prefab加载
        //异步方法-
        //返回:一层空的Prefab-
        public static GameObject LoadPrefabInstFromFileAnsy(string fileName, Transform parentDir,
                                                            Action<GameObject, GameObject> func,
                                                            out string writeName)
        {
            //GameObject prefab = LoadPrefabFromFile(fileName, accessType);
            GameObject newGameObject = new GameObject();
            LoadPrefabFromFileAnsy(fileName, (GameObject prefab, GameObject shell) => {

                if (shell != null)
                {
                    GameObject obj = GameObject.Instantiate(prefab);

                    Vector3 scale = obj.transform.localScale;

                    obj.transform.parent = shell.transform;

                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.transform.localScale = scale;

                    if (func != null) { func(obj, shell); }
                }
            }, newGameObject, out writeName);

            if (newGameObject != null)
            {
                var obj = newGameObject;
                if (parentDir != null)
                {
                    obj.transform.parent = parentDir;
                }

                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                return obj;
            }

            return newGameObject;
        }


        public static void LoadPrefabInstFromFileAnsyNoBox(string fileName, Transform parentDir,
                                                            Action<GameObject, GameObject> func,
                                                            out string writeName)
        {
            LoadPrefabFromFileAnsy(fileName, (GameObject prefab, GameObject shell) => {
                GameObject obj = GameObject.Instantiate(prefab, parentDir);

                if (obj != null)
                {
                    Vector3 scale = obj.transform.localScale;

                    //obj.transform.parent = parentDir;

                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;
                    obj.transform.localScale = scale;
                }

                if (func != null) { func(obj, null); }
            }, null, out writeName);
        }


        public static void LoadPrefabFromFileAnsy(string fileName,
                                                    Action<GameObject, GameObject> func,
                                                    GameObject shell,
                                                    out string writeName)
        {
            if (!fileName.Contains(".prefab")) { fileName += ".prefab"; }
            fileName = fileName.ToLower();
            writeName = fileName;

            if (LATMode == LoadAccessType.FromResource)
            {
                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);
                LoadFromResourcesAnsy(fileName, assetPath, (GameObject prefab, GameObject _shell) =>
                {
                    if (func != null) { func(prefab, _shell); }
                }, shell);

                return;
            }


            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }

                LoadFromBundleAnsy(fileName, bundleName, (GameObject resGameObject, AssetBundle atBundle) =>
                {
                    if (func != null) { func(resGameObject, shell); }
                });
            } else {
                #if UNITY_EDITOR
                GameObject prefab = LoadStoreAsset<GameObject>(fileName);
                string assetPath = "";
                if (prefab == null)
                {
                    assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_PREFAB);
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }

                StoreLoadedAsset(fileName, "", assetPath, prefab);

                if (func != null) { func(prefab, null); }
                #else
                    writeName = "";
                    return ;
                #endif
            }
        }

        #endregion


        #region Mesh加载
        //从Fbx中加载Mesh对象-
        public static Mesh LoadMeshFromFbxFile(string fileName, string meshName, out string writeName)
        {
            if (!fileName.Contains(".fbx")) { fileName += ".fbx"; }
            string fbxMeshName = fileName + "%" + meshName;
            fbxMeshName = fbxMeshName.ToLower();
            writeName = fbxMeshName;

            if (LATMode == LoadAccessType.FromResource)
            {
                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);

                var first = LoadStoreAsset<Mesh>(fbxMeshName);
                if (first != null) { return first; }

                //assetPath = assetPath.Substring(0, assetPath.LastIndexOf("."));         //Resources.Load加载方法是不需要扩展名的-
                var mesh = Resources.Load<Mesh>(assetPath);

                StoreLoadedAsset(fbxMeshName, "", assetPath, mesh);
                return mesh;
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                var first = LoadStoreAsset<Mesh>(fbxMeshName);
                if (first != null) { return first; }

                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; } else { return null; }

                var meshs_obj = LoadSubAssetFromBundle(fileName, bundleName);
                Mesh mesh = meshs_obj.FirstOrDefault((_mesh) =>
                                                        {
                                                            Mesh tmp = _mesh as Mesh;
                                                            if (tmp != null) { return tmp.name == meshName; }
                                                            return false;
                                                        }) as Mesh;

                StoreLoadedAsset(bundleName, "", !bundleLoadTable.ContainsKey(fileName) ? "" : bundleLoadTable[fileName][1], mesh);

                return mesh;
            } else {
#if UNITY_EDITOR
                var first = LoadStoreAsset<Mesh>(fbxMeshName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

                StoreLoadedAsset(fbxMeshName, "", assetPath, mesh);

                return mesh;
#else
                    writeName = "";
                    return null;
#endif
            }
        }

        //从bundle中加载mesh-
        public static void LoadMeshFromFile(string fileName, Action<Mesh> func, out string writeName)
        {
            if (!fileName.Contains(".mesh")) { fileName += ".mesh"; }
            fileName = fileName.ToLower();
            writeName = fileName;

            if (LATMode == LoadAccessType.FromResource)
            { /*先不实现*/}

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }

                LoadFromBundleAnsy(fileName, bundleName, (Mesh mesh, AssetBundle atBundle) =>
                {
                    if (func != null) { func(mesh); }
                });
            } else {
#if UNITY_EDITOR
                Mesh obj = LoadStoreAsset<Mesh>(fileName);
                string assetPath = "";
                if (obj == null)
                {
                    assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_PREFAB);
                    obj = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                }

                StoreLoadedAsset(fileName, "", assetPath, obj);

                if (func != null) { func(obj); }
#else
                    writeName = "";
                    return ;
#endif
            }
        }
        #endregion


        #region Material加载
        //从Fbx中加载Material对象-
        public static Material LoadMaterialFromFbxFile(string fileName, string matName, out string writeName)
        {
            if (!fileName.Contains(".fbx")) { fileName += ".fbx"; }
            string fbxMatName = fileName + "%" + matName;
            fbxMatName = fbxMatName.ToLower();
            writeName = fbxMatName;

            if (LATMode == LoadAccessType.FromResource)
            {
                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);

                var first = LoadStoreAsset<Material>(fbxMatName);
                if (first != null) { return first; }

                var mat = Resources.Load<Material>(assetPath);

                StoreLoadedAsset(fbxMatName, "", assetPath, mat);
                return mat;
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                var first = LoadStoreAsset<Material>(fbxMatName);
                if (first != null) { return first; }

                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; } else { return null; }

                var mats_obj = LoadSubAssetFromBundle(fileName, bundleName);
                Material mat = mats_obj.FirstOrDefault((_mat) =>
                                                            {
                                                                Material tmp = _mat as Material;
                                                                if (tmp != null) { return tmp.name == matName; }
                                                                return false;
                                                            }) as Material;

                StoreLoadedAsset(bundleName, "", !bundleLoadTable.ContainsKey(fileName) ? "" : bundleLoadTable[fileName][1], mat);

                return mat;
            } else {
#if UNITY_EDITOR
                var first = LoadStoreAsset<Material>(fbxMatName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

                StoreLoadedAsset(fbxMatName, "", assetPath, mat);

                return mat;
#else
                    writeName = "";
                    return null;
#endif
            }
        }

        //从mat文件中加载Material对象-
        public static Material LoadMaterialFromFile(string fileName, out string writeName)
        {
            if (!fileName.Contains(".mat")) { fileName += ".mat"; }
            fileName = fileName.ToLower();
            writeName = fileName;

            if (LATMode == LoadAccessType.FromResource)
            {
                var first = LoadStoreAsset<Material>(fileName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);
                var mat = Resources.Load<Material>(assetPath);

                //记录到loadedResources表中-
                StoreLoadedAsset(fileName, "", assetPath, mat);

                return mat;
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }

                var mat = LoadFromBundle<Material>(fileName, bundleName);
                return mat;
            } else {
#if UNITY_EDITOR
                var first = LoadStoreAsset<Material>(fileName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

                StoreLoadedAsset(fileName, "", assetPath, mat);
                return mat;
#else
                    writeName = "";
                    return null;
#endif
            }
        }
        #endregion


        #region AssetData加载-
        public static T LoadAssetDataFromFile<T>(string fileName, out string writeName)
            where T : ScriptableObject
        {
            if (!fileName.Contains(".asset")) { fileName += ".asset"; }
            fileName = fileName.ToLower();
            writeName = fileName;

            if (LATMode == LoadAccessType.FromResource)
            {
                var first = LoadStoreAsset<ScriptableObject>(fileName);
                if (first != null) { return (T)first; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);
                var asset = Resources.Load<ScriptableObject>(assetPath);

                StoreLoadedAsset(fileName, "", assetPath, asset);
                return asset as T;
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }

                var asset = LoadFromBundle<ScriptableObject>(fileName, bundleName);

                return asset as T;
            } else {
#if UNITY_EDITOR
                var first = LoadStoreAsset<ScriptableObject>(fileName);
                if (first != null) { return first as T; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                StoreLoadedAsset(fileName, "", assetPath, asset);

                return asset as T;
#else
                    writeName = "";
                    return null;
#endif
            }
        }
        #endregion


        #region Texture2D加载(fileName:文件名必须要求带有扩展名的,否则默认为png格式)
        public static void LoadTexture2DFromFile(string fileName, Action<Texture2D> func, out string writeName)
        {
            fileName = fileName.ToLower();

            if (LATMode == LoadAccessType.FromResource)
            {
                if (!fileName.Contains(".")) { fileName += ".tex"; }
                writeName = fileName;

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);

                LoadFromResourcesAnsy(fileName, assetPath, (Texture2D obj, GameObject _shell) =>
                {
                    if (func != null) { func(obj); }
                });
            }


            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (!fileName.Contains("."))
                {
                    foreach (var ext in ResourceTexExts)
                    {
                        string fullFileName = fileName + ext;
                        if (bundleLoadTable.ContainsKey(fullFileName)) { fileName = fullFileName; bundleName = bundleLoadTable[fullFileName][0]; break; }
                    }
                } else {
                    if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }
                }

                writeName = fileName;
                LoadFromBundleAnsy(fileName, bundleName, (Texture2D obj, AssetBundle atBundle) =>
                {
                    if (func != null) { func(obj); }
                });
            } else {
#if UNITY_EDITOR
                if (!fileName.Contains(".")) { fileName += ".png"; }
                writeName = fileName;

                var first = LoadStoreAsset<Texture2D>(fileName);
                string assetPath = "";
                if (first == null)
                {
                    assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                    first = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                }

                StoreLoadedAsset(fileName, "", assetPath, first);

                if (func != null) { func(first); }
#else
                    writeName = "";
                    return ;
#endif
            }
        }

        public static void LoadSpriteFromFile(string fileName, Action<Sprite> func, out string writeName)
        {
            fileName = fileName.ToLower();
            if (!fileName.Contains(".")) { fileName += ".png"; }
            string spriteFileName = fileName + "_sprite";
            writeName = spriteFileName;

            if (LATMode == LoadAccessType.FromResource)
            {
                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);

                LoadFromResourcesAnsy(spriteFileName, assetPath, (Sprite obj, GameObject _shell) =>
                {
                    if (func != null) { func(obj); }
                });
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                var first = LoadStoreAsset<Sprite>(spriteFileName);
                if (first != null) { if (func != null) { func(first); } return; }

                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }

                LoadFromBundleAnsy(fileName, bundleName, (Sprite obj, AssetBundle atBundle) =>
                {
                    StoreLoadedAsset(spriteFileName, "", !bundleLoadTable.ContainsKey(fileName) ? "" : bundleLoadTable[fileName][1], obj);
                    if (func != null) { func(obj); }
                }, true);
            } else {

#if UNITY_EDITOR
                var first = LoadStoreAsset<Sprite>(spriteFileName);
                string assetPath = "";
                if (first == null)
                {
                    assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                    first = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                }

                StoreLoadedAsset(spriteFileName, "", assetPath, first);

                if (func != null) { func(first); }
#else
                    writeName = "";
                    return ;
#endif
            }

        }
        #endregion


        #region 热更新脚本,表格等文本加载(fileName:必须待扩展名,否则默认为json格式)-
        public static TextAsset LoadScriptFromFile(string fileName, out string writeName, string extName = ".json")
        {
            if (!fileName.Contains(".")) { fileName += extName; }
            fileName = fileName.ToLower();
            writeName = fileName;

            if (LATMode == LoadAccessType.FromResource)
            {
                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);

                var first = LoadStoreAsset<TextAsset>(fileName);
                if (first != null) { return first; }

                var content = Resources.Load<TextAsset>(assetPath);

                //记录到loadedResources表中-
                StoreLoadedAsset(fileName, "", assetPath, content);

                //byte[] bs = Encoding.UTF8.GetBytes(content.text);
                return content;
            }

            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }

                var content = LoadFromBundle<TextAsset>(fileName, bundleName);

                //byte[] bs = Encoding.UTF8.GetBytes(content.text);
                return content;
            } else {
#if UNITY_EDITOR
                var first = LoadStoreAsset<TextAsset>(fileName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_SCRIPT);
                var content = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);

                StoreLoadedAsset(fileName, "", assetPath, content);

                //byte[] bs = Encoding.UTF8.GetBytes(content.text);
                return content;
#else
                    writeName = "";
                    return null;
#endif
            }
        }
        #endregion


        #region Shader加载(编辑器数据模式访问也是必须加上扩展名，否则默认扩展名为shader文件)-
        public static Shader LoadShaderFromFile(string fileName, out string writeName)
        {
            fileName = fileName.ToLower();
            if (LATMode == LoadAccessType.FromResource)
            {
                if (!fileName.Contains(".")) { fileName += ".shader"; }
                writeName = fileName;

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS);

                var first = LoadStoreAsset<Shader>(fileName);
                if (first != null) { return first; }

                var shader = Resources.Load<Shader>(assetPath);

                StoreLoadedAsset(fileName, "", assetPath, shader);
                return shader;
            }


            if (LATMode == LoadAccessType.FromAssetBundle)
            {
                string bundleName = string.Empty;
                if (!fileName.Contains("."))
                {
                    foreach (var ext in ResourceShaderExts)
                    {
                        string fullFileName = fileName + ext;
                        if (bundleLoadTable.ContainsKey(fullFileName)) { fileName = fullFileName; bundleName = bundleLoadTable[fullFileName][0]; break; }
                    }
                } else {
                    if (bundleLoadTable.ContainsKey(fileName)) { bundleName = bundleLoadTable[fileName][0]; }
                }

                writeName = fileName;

                var shader = LoadFromBundle<Shader>(fileName, bundleName);
                return shader;
            } else {
#if UNITY_EDITOR
                if (!fileName.Contains(".")) { fileName += ".shader"; }
                writeName = fileName;

                var first = LoadStoreAsset<Shader>(fileName);
                if (first != null) { return first; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_RES);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

                StoreLoadedAsset(fileName, "", assetPath, shader);

                return shader;
#else
                    writeName = "";
                    return null;
#endif
            }
        }
        #endregion


        #region scene加载(仅需填场景名)-
        //加载一个场景:场景文件加载到内存镜像本身格式就是一个Assetbundle-
        //fileName:对动态场景要求Bundle文件来说fileName等于bundleName
        public static void LoadSceneFromFileAnsy(string fileName, AssetBundleAnsyCallBack func)
        {
            //经证实:Resources.Load无法动态加载场景(不能加载AssetBundle类型)-
            if (LATMode == LoadAccessType.FromResource)
            {
                if (func != null) { func(null); }
                return;
            }

            //目前Unity只有这一种方法可以动态加载场景
            if (LATMode == LoadAccessType.FromAssetBundle /*||
                LATMode == LoadAccessType.FromAssetDatabase*/)
            {
                //这里有些特殊，场景文件在AssetBundle是自定义格式, 参考"BundleSceneExtName"配置-
                if (boot == null) { if (func != null) { func(null); } }
                boot._LoadBundleAnsyInner(fileName, BundleSceneExtName, func);

                return;
            }


            //经证实:AssetDatabase.LoadAssetAtPath无法动态加载场景(不能加载AssetBundle类型)-
            if (LATMode == LoadAccessType.FromAssetDatabase)
            {
                #if UNITY_EDITOR
                if (!fileName.Contains(".unity")) { fileName += ".unity"; }

                string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_SCENE);
                SceneAsset sceneBundle = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);

                if (func != null) { func(null); }
                #endif
            }

            return;
        }


        public static AssetBundle LoadSceneAssetFromFile(string fileName)
        {
            #if UNITY_EDITOR
            string assetPath = FindResourcePath(fileName, RES_FIND_PATHS_ASSETDATABASE_SCENE);
            var sceneAsset = AssetDatabase.LoadAssetAtPath<AssetBundle>(assetPath);
            return sceneAsset;
            #else
                return null;
            #endif
        }
        #endregion



        //----------------------------------------------------------------------------

        #region Bundle信息头加载-
        //检查初始化主文件-
        private static void _CheckMainBundle()
        {
            if (mMainBundle == null)
            {
                string mainInfoBundleFullPath = BUNDLE_PATH + BundleActionName;      //string.Format("{0}BundlePag", BUNDLE_PATH);
                if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
                {
                    if (File.Exists(mainInfoBundleFullPath))
                    {
                        mMainBundle = AssetBundle.LoadFromFile(mainInfoBundleFullPath);
                        if (mMainBundle) { mManifest = mMainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest"); }
                    }
                } else {
                    mMainBundle = AssetBundle.LoadFromFile(mainInfoBundleFullPath);
                    if (mMainBundle != null) 
                    {
                        mManifest = mMainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest"); 
                    }
                    else { Debug.LogError("加载mMainBundle失败！"); }

                }

            }
        }


        //加载信息头-
        public static string[] GetMainlandDependence(string sceneName)
        { if (mainlandLoadTable.ContainsKey(sceneName)) { return mainlandLoadTable[sceneName]; } return new string[0]; }


        //加载包体内的文件列表-
        private static void GetBundleFilesDesc(Action<string[]> func)
        {
            string[] _read_data(string ta)
            {
                List<string> sList = new List<string>();
                if (!string.IsNullOrEmpty(ta))
                {
                    string[] lines = ta.Split('\n');
                    foreach (string line in lines)
                    {
                        string[] words = line.Split('|');
                        if (words.Length > 2)
                        {
                            sList.Add(BUNDLE_PATH + words[0]);
                        }
                    }
                }

                string[] s = sList.ToArray();
                sList.Clear();
                return s;
            }

            System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
            string filePath = BUNDLE_PATH + "HotCompare.db";

            if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
            {
                var ta = encoding_utf8.GetString(LoadIOFile(filePath));
                func?.Invoke(_read_data(ta));
            } else {
                LoadIOFileAPK(filePath, (byte[] data,string  _filePath) =>
                {
                    var ta = encoding_utf8.GetString(data/*LoadIOFile(filePath)*/);
                    func?.Invoke(_read_data(ta));
                });
            }
        }

        //-
        private static void LoadBundleInfoHead(string[] pathBuffer, Action func)
        {
            void _read_data(string ta)
            {
                if (!string.IsNullOrEmpty(ta))
                {
                    string[] lines = ta.Split('\n');
                    foreach (string line in lines)
                    {
                        string[] words = line.Split('|');
                        if (words.Length > 3 && !string.IsNullOrEmpty(words[0]))
                        {
                            if(!bundleLoadTable.ContainsKey(words[0]))
                            {
                                bundleLoadTable.Add(words[0],
                                    new string[] {
                                            words[1],       //bundle名-
                                            words[4],       //在bundle内的镜像路径-
                                            words[3],       //bundle大小-
                                    });
                            }
                        }
                    }
                }
            }

            bundleLoadTable.Clear();
            //Debug.Log("<color=blue>ResourceMgr:LoadBundleInfoHead开始</color>");
            string[] infoHeadsFilePath = null;
            if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
            {
                if (!Directory.Exists(BUNDLE_PATH)) { return; }
                infoHeadsFilePath = Directory.GetFiles(BUNDLE_PATH, "*" + BundleRelativeExtName);
            }

            infoHeadsFilePath = pathBuffer;

            System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);

            int     count = 0;
            foreach (string filePath in infoHeadsFilePath)
            {
                count++;
                if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets) 
                {
                    if (!File.Exists(filePath)) { continue; }
                }
                
                string fileName = Path.GetFileName(filePath);
                if (!fileName.Contains(BundleRelativeExtName)) { continue; }
                if (fileName.Contains("lg_")) { continue; }             //语言包的信息头不在这里加载-
                if (fileName.Contains("mainland")) { continue; }        //母包场景依赖信息-

                if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
                {
                    var ta = encoding_utf8.GetString(LoadIOFile(filePath));
                    if (!string.IsNullOrEmpty(ta))
                    {
                        _read_data(ta);
                    }
                } else {
                    LoadIOFileAPK(filePath, (byte[] data, string _filePath) => {
                        if (data == null) { Debug.LogError("信息头文件为空:" + filePath); return; }

                        var ta = encoding_utf8.GetString(data/*LoadIOFile(filePath)*/);
                        if (!string.IsNullOrEmpty(ta))
                        {
                            _read_data(ta);
                        }

                        //Debug.Log("<color=blue>ResourceMgr:LoadBundleInfoHead结束</color>" + bundleLoadTable.Count +
                        //            "  infoHeadsFilePath.Length:" + infoHeadsFilePath.Length +
                        //            " _filePath:" + _filePath);
                        if (count == infoHeadsFilePath.Length) 
                        { 
                            func?.Invoke(); 
                        }
                    });
                }
            }

            //-
            if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
            {
                func?.Invoke();
            }
        }



        private static void LoadMainlandInfoHead(Action func)
        {
            void _read_data(string ta)
            {
                if (!string.IsNullOrEmpty(ta))
                {
                    string dependssName = "";
                    List<string> dependss = new List<string>(0);
                    string[] lines = ta.Split('\n');
                    int idx = 0;
                    foreach (string line in lines)
                    {
                        idx++;

                        if (line.Contains(":"))
                        {
                            string[] words = line.Split(':');
                            if (words.Length > 1)
                            {
                                //清算前一个累计信息-
                                if (mainlandLoadTable.ContainsKey(dependssName)) { mainlandLoadTable[dependssName] = dependss.ToArray(); }

                                //-
                                if (!mainlandLoadTable.ContainsKey(words[0]))
                                { dependssName = words[0]; dependss.Clear(); mainlandLoadTable.Add(words[0], null); }
                            }
                        } else {
                            if (!string.IsNullOrEmpty(line)) { dependss.Add(line); }

                            //清算最后一个累计信息-
                            if (idx == lines.Length && mainlandLoadTable.ContainsKey(dependssName)) { mainlandLoadTable[dependssName] = dependss.ToArray(); }
                        }
                    }

                    dependss.Clear();
                }
            }

            mainlandLoadTable.Clear();


                

            System.Text.UTF8Encoding    encoding_utf8 = new System.Text.UTF8Encoding(true);
            string                      filePath = BUNDLE_PATH +SceneDependenceInfoFileName;

            if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
            {
                if (!Directory.Exists(BUNDLE_PATH)) { return; }

                var ta = encoding_utf8.GetString(LoadIOFile(filePath));
                _read_data(ta);

                func?.Invoke();
            } else {
                LoadIOFileAPK(filePath, (byte[] data, string _filePath) =>
                {
                    var ta = encoding_utf8.GetString(data/*LoadIOFile(filePath)*/);
                    _read_data(ta);

                    func?.Invoke();
                });
            }

        }


        private static void LoadLanguageBundleHead(string lanHeadFileName, Action func)
        {
            void _read_data(string ta)
            {
                if (!string.IsNullOrEmpty(ta))
                {
                    string[] lines = ta.Split('\n');
                    foreach (string line in lines)
                    {
                        string[] words = line.Split('|');
                        if (words.Length > 3 && !string.IsNullOrEmpty(words[0]))
                        {
                            string[] assetInfo = new string[] {
                                    words[1],       //bundle名-
                                    words[4],       //在bundle内的镜像路径-
                                    words[3],       //bundle大小-
                                };

                            if (!bundleLoadTable.ContainsKey(words[0]))
                            {
                                bundleLoadTable.Add(words[0], assetInfo);
                            }
                            else { bundleLoadTable[words[0]] = assetInfo; }
                        }
                    }
                }
            }


            if (!lanHeadFileName.Contains("lg_")) { lanHeadFileName = "lg_" + lanHeadFileName; }

            System.Text.UTF8Encoding    encoding_utf8 = new System.Text.UTF8Encoding(true);
            string                      filePath = BUNDLE_PATH + lanHeadFileName + BundleRelativeExtName;

            if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
            {
                if (!Directory.Exists(BUNDLE_PATH)) { return; }

                var ta = encoding_utf8.GetString(LoadIOFile(filePath) );
                _read_data(ta);

                func?.Invoke();
            } else {
                LoadIOFileAPK(filePath, (byte[] data, string _filePath) =>
                {
                    var ta = encoding_utf8.GetString(data/*LoadIOFile(filePath)*/);
                    _read_data(ta);

                    func?.Invoke();
                });
            }

        }
        #endregion


        //-
        #region 异步加载Bundle相关方法-
        //加载一个bundle-
        //这里没有释放载入的AssetBundle内存映射，临时方便后面使用-

        //异步加载Bundle方法-
        public delegate void LoadFromBundleAnsyCallBack<T>(T res, AssetBundle atBundle);
        public delegate void LoadAllFromBundleAnsyCallBack<T>(T[] res, AssetBundle atBundle);
        public delegate void AssetBundleAnsyCallBack(AssetBundle bundle);
        struct LoadBundleAnsyInnerIEnumeratorParam      //异步调用使用到的参数合集-
        {
            public string bundleName;
            public string bundleExtName;
            public AssetBundleAnsyCallBack func;
        }

        //排队加载每一个异步对象-
        List<LoadBundleAnsyInnerIEnumeratorParam>   mBundleAnsyLoadArray = new List<LoadBundleAnsyInnerIEnumeratorParam>(0);
        string                                      mBundleNowAnsyLoad = string.Empty;

        //加载依赖收集缓存池-
        Dictionary<string, string>                  mDependenciesBuffer = new Dictionary<string, string>(0);


        //加载母包场景依赖-
        public static void LoadMainlandDependenceBundleAnsy(string sceneName,Action<AssetBundle[]> func)
        {
            if (boot == null) { return; }

            _CheckMainBundle();

            string[]        needLoadBundleNames = ResourceMgr.GetMainlandDependence(sceneName);
            AssetBundle[]   bundles = new AssetBundle[needLoadBundleNames.Length];
            int             idx =0;

            if(needLoadBundleNames.Length >0)
            {
                foreach (string bundleName in needLoadBundleNames)
                {
                    boot._LoadBundleAnsyInner(bundleName, BundleSceneExtName, (AssetBundle ab) =>
                    {
                        bundles[idx] = ab; idx++;
                        if (idx >= needLoadBundleNames.Length) { if (func != null) func(bundles); }
                    });
                }
            } else {
                func( new AssetBundle[0]);
            }
        }

        //加载bundle并加载其中的Asset-
        private static void LoadFromBundleAnsy<Tk>(string resName, string bundleName, LoadFromBundleAnsyCallBack<Tk> _func, bool ignoreStoreLoad = false)
        where Tk : UnityEngine.Object       //从bundle中提取资源-
        {
            if (string.IsNullOrEmpty(bundleName)) 
            {
                Debug.LogError("文件:" + resName +",没有在包库内!");
                //Debug.LogError("bundleLoadTablebundleLoadTablebundleLoadTable:" + bundleLoadTable["loading"]);
                if (!ignoreStoreLoad) { StoreLoadedAsset(resName, bundleName, !bundleLoadTable.ContainsKey(resName) ? "" : bundleLoadTable[resName][1], null); }
                _func(null, null); 
                return; 
            }

            if (!ignoreStoreLoad)
            { 
                Tk obj = LoadStoreAsset<Tk>(resName);
                if (obj != null) 
                {
                    RMAssetRequire rMAssetRequire = loadedResources[resName];
                    _func(obj, loadedBundle[rMAssetRequire.BundleName]);
                    return ; 
                }
            }

            _LoadBundleAnsy(bundleName, (AssetBundle bundle) => {
                if(bundle != null)
                {
                    AssetBundleRequest abr = bundle.LoadAssetWithSubAssetsAsync<Tk>(resName);
                    abr.completed += (AsyncOperation ao) =>
                    {
                        if (!ignoreStoreLoad) { StoreLoadedAsset(resName, bundle.name, !bundleLoadTable.ContainsKey(resName) ? "" : bundleLoadTable[resName][1], abr.asset); }

                        if (_func != null) { _func((Tk)abr.asset, bundle); }
                    };
                } else {
                    if (!ignoreStoreLoad) { StoreLoadedAsset(resName, bundleName, !bundleLoadTable.ContainsKey(resName) ? "" : bundleLoadTable[resName][1], null); }
                    if (_func != null) { _func(null, bundle); }
                }  
            });
        }

        //这个方法把一个bundle里所有的asset都读出来，但没啥大作用处-
        static private void LoadAllFromBundleAnsy<Tk>(string bundleName, LoadAllFromBundleAnsyCallBack<Tk> resAllFunc)
            where Tk : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                Debug.LogError("访问了空的资源包!");
                resAllFunc(null, null); return;
            }

            //-
            _LoadBundleAnsy(bundleName, (AssetBundle bundle) => {
                if (resAllFunc != null)
                {
                    if(bundle != null)
                    {
                        AssetBundleRequest abr = bundle.LoadAllAssetsAsync<Tk>();
                        abr.completed += (AsyncOperation ao) =>
                        {
                            resAllFunc((Tk[])abr.allAssets, bundle);
                        };
                    } else {
                        resAllFunc(null, bundle);
                    }  
                }
            });
        }

        //加载bundle内部方法-
        static private void _LoadBundleAnsy(string bundleName, AssetBundleAnsyCallBack func)        
        {
            _CheckMainBundle();

            if (boot != null) { boot._LoadBundleAnsyInner(bundleName, BundleResExtName, func); }
        }

        private void _LoadBundleAnsyInner(string bundleName, string bundleExtName ,AssetBundleAnsyCallBack func) 
        {
            _CheckMainBundle();
            string exName = Path.GetExtension(bundleName);
            string bundleFullName = string.IsNullOrEmpty(exName) ? bundleName + bundleExtName : bundleName;

            if (loadedBundle.ContainsKey(bundleFullName))
            {
                UnityEngine.Object obj = loadedBundle[bundleFullName];
                if (obj != null)
                {
                    //成功加载了依赖bundle-
                    if (collectBundleFunc != null) { collectBundleFunc((AssetBundle)obj); }

                    if (func != null) { func((AssetBundle)obj); }

                    return;
                }
            }


            //改为队列异步加载-
            //开始进入加载队列-
            mBundleAnsyLoadArray.Add(
                new LoadBundleAnsyInnerIEnumeratorParam
                {
                    bundleName = bundleFullName,
                    bundleExtName = bundleExtName,
                    func = func,
                });


            //开始加载bundle-
            _reload:
            if (mBundleAnsyLoadArray.Count >0 && mBundleNowAnsyLoad == string.Empty)
            {
                LoadBundleAnsyInnerIEnumeratorParam raiip = mBundleAnsyLoadArray[0];
                mBundleAnsyLoadArray.RemoveAt(0);

                if (raiip.bundleName != string.Empty)
                {
                    mBundleNowAnsyLoad = raiip.bundleName;
                    StartCoroutine("_LoadBundleAnsyInnerIEnumerator", raiip);
                } else {
                    goto _reload;
                }
            }
        }

       
        private IEnumerator _LoadBundleAnsyInnerIEnumerator(LoadBundleAnsyInnerIEnumeratorParam funcParam) 
        {
            //收集依赖方法-
            void DependeniesCollect(string fullBundleName)
            {
                if (!mDependenciesBuffer.ContainsKey(fullBundleName))
                {
                    mDependenciesBuffer.Add(fullBundleName, fullBundleName);

                    string[] dependencies = mManifest.GetAllDependencies(fullBundleName);
                    foreach (string str in dependencies) { DependeniesCollect(fullBundleName); }
                }
            }

            //-
            string exName = Path.GetExtension(funcParam.bundleName);
            string bundleFullName = string.IsNullOrEmpty(exName) ? funcParam.bundleName + funcParam.bundleExtName : funcParam.bundleName;
            string bundleFullPath =  BUNDLE_PATH + bundleFullName;

            funcParam.bundleName = bundleFullName;

            //加载这个bundle的依赖bundle-
            if ( mManifest !=null )
            {
                //收集依赖(unity已经帮你把俄罗斯套娃式的依赖收集完成了)-
                mDependenciesBuffer.Clear();
                string[] dependencies = mManifest.GetAllDependencies(bundleFullName);
                //foreach (string str in dependencies) { DependeniesCollect(str); }

                //加载这些依赖-
                foreach (string str in dependencies)
                {
                    if (!loadedBundle.ContainsKey(str))
                    {
                        loadedBundle.Add(str, null);

                        string                      abcrPath = BUNDLE_PATH + str;
                        AssetBundleCreateRequest    abcr = null;
                        try
                        {
                            if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
                            {
                                if (File.Exists(abcrPath))
                                {
                                    abcr = AssetBundle.LoadFromFileAsync(abcrPath, 0, ResourceMgr.ocd);
                                    abcr.allowSceneActivation = true;
                                }
                            } else {
                                abcr = AssetBundle.LoadFromFileAsync(abcrPath, 0, ResourceMgr.ocd);
                                abcr.allowSceneActivation = true;
                            }

                        }
                        catch (Exception e) { if (funcParam.func != null) { funcParam.func(null); } goto _reload; }
                        while (abcr != null && !abcr.isDone) { yield return null; }                 //异步直到场景加载完成-

                        //AssetBundle db = AssetBundle.LoadFromFile(abcrPath);
                        //if (db != null) { loadedBundle[str] = db; }
                        if (abcr !=null && abcr.assetBundle != null) { loadedBundle[str] = abcr.assetBundle; }
                        else {
                            Debug.LogError(string.Format("包{0}加载出错!", str));
                            loadedBundle.Remove(str);

                            if (funcParam.func != null) { funcParam.func(null); }  goto _reload;
                        }

                    } else {
                        //其他也想获取该bundle数据的对象，经行等待检测-
                        AssetBundle ab = loadedBundle[str];
                        while (ab == null)
                        {
                            if (!loadedBundle.ContainsKey(str)) { yield break; }       //主加载对象宣布加载失败了-
                            ab = loadedBundle[str];        //继续等待-
                            yield return 0;
                        }
                    }


                    //成功加载了依赖bundle-
                    if (collectBundleFunc != null) { collectBundleFunc(loadedBundle[str]); }
                }
            }



            //加载这个bundle-
            if (!loadedBundle.ContainsKey(bundleFullName))
            {
                loadedBundle.Add(bundleFullName, null);

                string pathTemp = BUNDLE_PATH + bundleFullName;
                Debug.Log("加载:" + pathTemp);

                AssetBundleCreateRequest abcr = null;
                try
                {
                    if (Application.platform != RuntimePlatform.Android || mBUNDLE_ACCESS != BundleAccessType.StreamAssets)
                    {
                        if (File.Exists(pathTemp))
                        {
                            abcr = AssetBundle.LoadFromFileAsync(pathTemp, 0, ResourceMgr.ocd);
                            abcr.allowSceneActivation = true;
                        }
                    } else {
                        abcr = AssetBundle.LoadFromFileAsync(pathTemp, 0, ResourceMgr.ocd);
                        abcr.allowSceneActivation = true;
                    }
                }
                catch (Exception e) { if (funcParam.func != null) { funcParam.func(null); } goto _reload; }
                while (abcr != null && !abcr.isDone) { yield return null; }                 //异步知道场景加载完成-

                //AssetBundle db = AssetBundle.LoadFromFile(pathTemp);
                //if (db != null) { loadedBundle[bundleFullName] = db; }

                if (abcr != null && abcr.assetBundle != null) { loadedBundle[bundleFullName] = abcr.assetBundle; }
                else {
                    Debug.LogError(string.Format("包{0}加载出错!", bundleFullName));
                    loadedBundle.Remove(bundleFullName);

                    if (funcParam.func != null) { funcParam.func(null); }

                    goto _reload;
                }
            } else {
                //其他也想获取该bundle数据的对象，经行等待检测-
                AssetBundle ab = loadedBundle[bundleFullName];
                while (ab == null) {
                    if (!loadedBundle.ContainsKey(bundleFullName)) { yield break; }       //主加载对象宣布加载失败了-
                    ab = loadedBundle[bundleFullName];        //继续等待-
                    yield return 0;
                }
            }


            //成功加载了bundle-
            if (collectBundleFunc !=null) { collectBundleFunc(loadedBundle[bundleFullName]); }
            if (funcParam.func != null) { funcParam.func( loadedBundle[bundleFullName]); }


            //加载下一个Bundle-
            _reload:
            mBundleNowAnsyLoad = string.Empty;
            if (mBundleAnsyLoadArray.Count >0)
            {
                LoadBundleAnsyInnerIEnumeratorParam raiip = mBundleAnsyLoadArray[0];
                mBundleAnsyLoadArray.RemoveAt(0);

                if (loadedBundle.ContainsKey(raiip.bundleName))
                {
                    UnityEngine.Object obj = loadedBundle[raiip.bundleName];
                    if (obj != null)
                    {
                        //成功加载了依赖bundle-
                        if (collectBundleFunc != null) { collectBundleFunc((AssetBundle)obj); }

                        //-
                        if (raiip.func != null) { raiip.func((AssetBundle)obj); }

                        goto _reload;
                    }
                }


                if (raiip.bundleName != string.Empty)
                {
                    mBundleNowAnsyLoad = raiip.bundleName;
                    StartCoroutine("_LoadBundleAnsyInnerIEnumerator", raiip);
                } else {
                    goto _reload;
                }
            }
        }

        #endregion



        #region 同步加载Bundle方法-
        static private AssetBundle _LoadBundle(string bundleName)               
        {
            _CheckMainBundle();

            //开始加载bundle-
            string exName = Path.GetExtension(bundleName);
            string bundleFullName = string.IsNullOrEmpty(exName) ? bundleName + BundleResExtName : bundleName;
            string bundleFullPath =  BUNDLE_PATH + bundleFullName;

            string[] dependencies = mManifest.GetAllDependencies(bundleFullName);
            foreach (string str in dependencies)
            {
                if (!loadedBundle.ContainsKey(str))
                {
                    AssetBundle resBd = AssetBundle.LoadFromFile(BUNDLE_PATH + str, 0, ResourceMgr.ocd);
                    if (resBd != null) { loadedBundle.Add(str, resBd); }
                    else { Debug.LogError(string.Format("包{0}加载出错!", str)); continue; }
                }

                //成功加载了bundle-
                if (collectBundleFunc != null) { collectBundleFunc(loadedBundle[str]); }
            }

            if (!loadedBundle.ContainsKey(bundleFullName))
            {
                AssetBundle resBd = AssetBundle.LoadFromFile(BUNDLE_PATH + bundleFullName, 0, ResourceMgr.ocd);
                if (resBd != null)
                {
                    loadedBundle.Add(bundleFullName, resBd);
                }
                else { Debug.LogError(string.Format("包{0}加载出错!", bundleFullName)); return null; }
            }

            //成功加载了bundle-
            if (collectBundleFunc != null) { collectBundleFunc(loadedBundle[bundleFullName]); }

            return loadedBundle[bundleFullName];
        }


        //从bundle里加载资源文件-
        //inFbxName:因为fbx文件的特殊性，它自身也是个小bundle里面包含着动画模型等，所以记录资源踪迹要记录到这个fbx文件所在的bundle-
        static private Tk LoadFromBundle<Tk>(string resName, string bundleName, bool ignoreStoreLoad = false) where Tk : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(bundleName)) 
            {
                Debug.LogError("文件:" + resName + ",没有在包库内!");
                if (!ignoreStoreLoad) { StoreLoadedAsset(resName, bundleName, !bundleLoadTable.ContainsKey(resName) ? "" : bundleLoadTable[resName][1], null); }
                return default(Tk); 
            }

            if (!ignoreStoreLoad)
            {
                Tk obj = LoadStoreAsset<Tk>(resName);
                if (obj != null) { return obj; }
            }

            AssetBundle bd = _LoadBundle(bundleName);
            if (bd == null) 
            {
                if (!ignoreStoreLoad) { StoreLoadedAsset(resName, bundleName, !bundleLoadTable.ContainsKey(resName) ? "" : bundleLoadTable[resName][1], null); }
                return default(Tk); 
            }

            UnityEngine.Object asset = bd.LoadAsset<Tk>(resName);
            if (!ignoreStoreLoad) { StoreLoadedAsset(resName, bd.name, !bundleLoadTable.ContainsKey(resName) ? "" : bundleLoadTable[resName][1], asset); }

            return (Tk)asset;
        }


        //这个方法主要用来加载fbx中的asset对象集-
        public static UnityEngine.Object[] LoadSubAssetFromBundle(string resName, string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName)) { Debug.LogError("文件:" + resName + ",没有在包库内!"); return default; }

            AssetBundle bd = _LoadBundle(bundleName);
            if (bd == null) { return default; }

            UnityEngine.Object[] assets = bd.LoadAssetWithSubAssets(resName);

            return assets;
        }


        //这个方法把一个bundle里所有的asset都读出来，但没啥大作用处-
        static private Tk[] LoadAllFromBundle<Tk>(string bundleName) where Tk : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(bundleName)) { Debug.LogError("访问了空的资源包!"); return default(Tk[]); }

            AssetBundle bd = _LoadBundle(bundleName);
            if (bd == null) { return default(Tk[]); }

            return bd.LoadAllAssets<Tk>();
        }
        #endregion



        #region 异步加载Resources方法-
        public delegate void LoadFromResourceAnsyCallBack<T>(T res, GameObject shell);
        public delegate void ResourceAnsyCallBack(UnityEngine.Object @object,GameObject shell);
        struct ResourceAnsyInnerIEnumeratorParam      //异步调用使用到的参数合集-
        {
            public string resName;
            public GameObject shell;
            public ResourceAnsyCallBack func;
        }

        //排队加载每一个异步对象-
        List<ResourceAnsyInnerIEnumeratorParam>               mResourceAnsyLoadArray = new List<ResourceAnsyInnerIEnumeratorParam>(0);
        string                                                mResourceNowAnsyLoad =string.Empty;


        //-
        public static void LoadFromResourcesAnsy<Tk>(string fileName, string assetPath, LoadFromResourceAnsyCallBack<Tk> func ,GameObject shell = null)
            where Tk : UnityEngine.Object
        {
            if (boot == null) { return; }

            Tk obj = LoadStoreAsset<Tk>(fileName);
            if (obj != null)
            {
                func(obj, shell);
                return;
            }

            boot._LoadResourcesAnsyInner(fileName, (UnityEngine.Object @object, GameObject _shell) => {
                if(@object !=null)
                {
                    StoreLoadedAsset(fileName, "", assetPath, @object);
                    func((Tk)@object, _shell);
                }
            }, shell);
        }



        private void _LoadResourcesAnsyInner(string resName, ResourceAnsyCallBack func, GameObject shell)
        {
            if (loadedResources.ContainsKey(resName))
            {
                RMAssetRequire rMAssetRequire = loadedResources[resName];
                if(rMAssetRequire.Module != null)
                {
                    if (func != null)
                    {
                        func(rMAssetRequire.Module, shell);
                    }

                    return;
                }  
            }


            //改为队列异步加载-
            //开始进入加载队列-
            mResourceAnsyLoadArray.Add(
                new ResourceAnsyInnerIEnumeratorParam
                {
                    resName = resName,
                    shell = shell,
                    func = func,
                });



            //开始加载Resources-
            _reload:
            if (mResourceAnsyLoadArray.Count >0 && mResourceNowAnsyLoad ==string.Empty)
            {
                ResourceAnsyInnerIEnumeratorParam raii = mResourceAnsyLoadArray[0];
                mResourceAnsyLoadArray.RemoveAt(0);

                if (raii.resName != string.Empty)
                {
                    mResourceNowAnsyLoad = raii.resName;
                    StartCoroutine("_LoadResourcesAnsyInnerIEnumerator", raii);
                } else {
                    goto _reload;
                }
            }

            //StartCoroutine("_LoadResourcesAnsyInnerIEnumerator",
            //    new ResourceAnsyInnerIEnumeratorParam
            //    {
            //        resName = resName,
            //        shell = shell,
            //        func = func,
            //    });
        }


        private IEnumerator _LoadResourcesAnsyInnerIEnumerator(ResourceAnsyInnerIEnumeratorParam funcParam)
        {
            string prefabName = funcParam.resName;

            if (!loadedResources.ContainsKey(prefabName))
            {
                loadedResources.Add(prefabName, default);

                ResourceRequest rr = Resources.LoadAsync(prefabName);
                rr.allowSceneActivation = true;

                while (!rr.isDone)         //异步直到加载完成-
                { yield return rr; }

                if (rr.asset != null) { loadedResources[prefabName] = new RMAssetRequire { Name = prefabName, Module = rr.asset }; }
                else
                {
                    Debug.LogError(string.Format("资源内容{0}加载出错!", prefabName));
                    loadedResources.Remove(prefabName);
                    goto _reload;
                }
            } else {
                //其他也想获取该Resources数据的对象，经行等待检测-
                UnityEngine.Object obj = loadedResources[prefabName].Module;
                while (obj == null)
                {
                    if (!loadedResources.ContainsKey(prefabName)) { yield break; }       //主加载对象宣布加载失败了-
                    obj = loadedResources[prefabName].Module;        //继续等待-

                    yield return 0;
                }
            }

            if (funcParam.func != null)
            {
                funcParam.func(loadedResources[prefabName].Module, funcParam.shell);
            }


            //加载下一个Resources-
            _reload:
            mResourceNowAnsyLoad = string.Empty;
            if (mResourceAnsyLoadArray.Count >0)
            {
                ResourceAnsyInnerIEnumeratorParam raii = mResourceAnsyLoadArray[0];
                mResourceAnsyLoadArray.RemoveAt(0);

                if (loadedResources.ContainsKey(raii.resName))
                {
                    UnityEngine.Object obj = loadedResources[raii.resName].Module;
                    if (obj != null)
                    {
                        if (raii.func != null)
                        {
                            raii.func(obj, raii.shell);
                        }

                        goto _reload;
                    }
                }


                if (raii.resName != string.Empty)
                {
                    mResourceNowAnsyLoad = raii.resName;
                    StartCoroutine("_LoadResourcesAnsyInnerIEnumerator", raii);
                } else {
                    goto _reload;
                }
            }
  
        }

        #endregion



        //-----------------------------------------------------------------------------
        #region 系统文件操作
        //找出一个指定名字的资源，返回资源的AssetDatabase使用的相对路径-
        //findPath:暂时还没有用-
        public static string FindResourcePath(string findName,string[] resFindPaths)
        {
            //编辑器启动环境"AssetDataBase"和"Resource目录"模式下启用该表-
            if (mBUNDLE_ACCESS == BundleAccessType.EditorAssetDatabase &&
                LATMode == LoadAccessType.FromAssetDatabase)
            {
                if (resourceLoadTable.ContainsKey(findName))
                {
                    return resourceLoadTable[findName];
                }
            }


            //-
            if (resFindPaths == RES_FIND_PATHS)
            {
                findName = findName.Substring(0, findName.LastIndexOf("."));         //Resources.Load加载方法是不需要扩展名的-
            }  else { 
                foreach(string path in resFindPaths)
                {
                    string fullPath = path;

                    //拼凑出语言包路径-
                    if (fullPath.Contains("DB/") ) 
                    {
                        fullPath += ResourceMgr.Language + "/";
                    }

                    //-
                    if(Directory.Exists(fullPath) )
                    {
                        var files =Directory.GetFiles(fullPath, findName, SearchOption.AllDirectories).ToList();
                        if (files.Count > 0) { return GetAssetRelativePath(files[0], resFindPaths != RES_FIND_PATHS ? "Assets/": path); }
                    }
                }
            }

            return string.Empty;
        }



        //转换全路径到AssetDatabase使用的相对路径中(相对路径也可以转换到相对路径)-
        public static string GetAssetRelativePath(string fullPath,string relativeDirName = "Assets/")
        {
            if (string.IsNullOrEmpty(fullPath))

                return string.Empty;

            fullPath = fullPath.Replace("\\", "/");

            int index = fullPath.IndexOf(relativeDirName, StringComparison.CurrentCultureIgnoreCase);

            //没找到局部路径头名字-
            if (index < 0) { return fullPath; }

            //找到后返回-
            string ret = fullPath.Substring(index);

            return ret;
        }



        // 根据目录判断是否有资源文件-
        //path:是一个AssetDatabase使用的相对路径
        public static bool DirExistResource(string path)
        {
            if (string.IsNullOrEmpty(path)) { return false; }

            //转换到全局路径-
            string fullPath = Path.GetFullPath(path);

            if (string.IsNullOrEmpty(fullPath)) { return false; }


            string[] files = System.IO.Directory.GetFiles(fullPath);

            if ((files == null) || (files.Length <= 0)) { return false; }

            for (int i = 0; i < files.Length; ++i)
            {
                string ext = System.IO.Path.GetExtension(files[i]);

                if (string.IsNullOrEmpty(ext)) { continue; }

                for (int j = 0; j < ResourceExts.Length; ++j)
                {
                    if (string.Compare(ext, ResourceExts[j], true) == 0)
                    {
                        //if ((ResourceExts[j] == ".fbx") || (ResourceExts[j] == ".controller"))
                        //{
                        //    // ingore xxx@idle.fbx
                        //    string name = Path.GetFileNameWithoutExtension(files[i]);

                        //    if (name.IndexOf('@') >= 0) { return false; }
                        //} else{
                            if (ResourceExts[j] == ".unity")
                            {
                                if (!IsVaildSceneResource(files[i])) { return false; }
                            }
                        //}

                        return true;
                    }
                } //for (int j = 0; j < ResourceExts.Length; ++j)-
            }//for (int i = 0; i < files.Length; ++i)-

            return false;
        }



        //检查场景文件是否配置有效-
        private static bool IsVaildSceneResource(string fileName)
        {

            bool ret = false;
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(fileName)) { return ret; }


            string localFileName = GetAssetRelativePath(fileName);

            if (string.IsNullOrEmpty(localFileName)) { return ret; }


            var scenes = EditorBuildSettings.scenes;

            if (scenes == null)

                return ret;



            var iter = scenes.GetEnumerator();

            while (iter.MoveNext())
            {
                EditorBuildSettingsScene scene = iter.Current as EditorBuildSettingsScene;

                if ((scene != null) && scene.enabled)
                {
                    if (string.Compare(scene.path, localFileName, true) == 0)
                    {
                        ret = true;
                        break;
                    }
                }
            }
#endif
            return ret;
        }
#endregion
    }

}

