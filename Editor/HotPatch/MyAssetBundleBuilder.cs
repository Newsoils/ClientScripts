using System.Collections.Generic;
using System.IO;
using GameCoreResourceLoad;
using UnityEditor;
using UnityEngine;


namespace CLIP.Project_Mouse
{
    public enum BundleAccessMode
    {
        External = 0,    // 工程外围缓存 (AssetBundles/)
        InStreamingAssets = 1, // 游戏包体内 (StreamingAssets/)
        PersistentData = 2,    // 热更目录 (PersistentDataPath/)
    }

    public static class BundleEnvConfig
    {
        // 配置文件的存放路径（位于 Resources 下以便运行时能简单读取）
        private const string SETTING_DIR = "Assets/Resources/RMSetting";
        private const string SETTING_FILE_PATH = SETTING_DIR + "/BundleAccessFrom.txt";
        private const string RESOURCES_LOAD_PATH = "RMSetting/BundleAccessFrom";

        // 模式枚举：提高代码可读性，避免 0, 1, 2 这种魔法数字


        /// <summary>
        /// 设置并保存当前的访问模式
        /// </summary>
        public static void SetMode(BundleAccessMode mode)
        {
            if (!Directory.Exists(SETTING_DIR)) Directory.CreateDirectory(SETTING_DIR);

            // 1. 写入文件
            File.WriteAllText(SETTING_FILE_PATH, ((int)mode).ToString());

            // 2. 核心：必须让 AssetDatabase 知道文件变了，否则运行时 Resources.Load 还是旧内容
            AssetDatabase.ImportAsset(SETTING_FILE_PATH);

            // 3. 记录到 EditorPrefs 方便菜单打勾显示
            EditorPrefs.SetInt("RMBundleAccess_Int", (int)mode);


            Debug.Log($"<color=cyan>[Env] 已切换资源读取模式为: {mode}</color>");
        }

        /// <summary>
        /// 获取当前的访问模式（编辑器与运行时通用）
        /// </summary>
        public static BundleAccessMode GetCurrentMode()
        {
            // 如果在编辑器中，优先读 EditorPrefs
            if (Application.isEditor)
            {
                return (BundleAccessMode)EditorPrefs.GetInt("RMBundleAccess_Int", 0);
            }

            // 运行时加载信号文件
            TextAsset ta = Resources.Load<TextAsset>(RESOURCES_LOAD_PATH);
            if (ta != null && int.TryParse(ta.text.Trim(), out int val))
            {
                return (BundleAccessMode)val;
            }

            return BundleAccessMode.InStreamingAssets; // 默认读取包内
        }
    }



    public class MyAssetBundleBuilder
    {
        public const string MenuBase = "Tools/HotPatch/Bundle访问模式/";

        /// <summary>
        /// 加密位数
        /// </summary>
        public const int ocd = 0;

        #region 编辑器顶部菜单设置Bundle访问模式

        [MenuItem(MenuBase + "1. 工程外围缓存 (AssetBundles)")]
        public static void SetToExternal() 
        {
            BundleEnvConfig.SetMode(BundleAccessMode.External);
            CheckMode();
        }

        [MenuItem(MenuBase + "2. 游戏包体内 (StreamingAssets)")]
        public static void SetToInApp()
        {
            BundleEnvConfig.SetMode(BundleAccessMode.InStreamingAssets);
            CheckMode();
        }

        [MenuItem(MenuBase + "3. 热更目录内 (PersistentDataPath)")]
        public static void SetToPersistent() 
        {
            BundleEnvConfig.SetMode(BundleAccessMode.PersistentData);
            CheckMode();
        }

        public static bool CheckMode() { UpdateMenuChecks(); return true; }

        private static void UpdateMenuChecks()
        {
            var current = BundleEnvConfig.GetCurrentMode();
            Menu.SetChecked(MenuBase + "1. 工程外围缓存 (AssetBundles)", current == BundleAccessMode.External);
            Menu.SetChecked(MenuBase + "2. 游戏包体内 (StreamingAssets)", current == BundleAccessMode.InStreamingAssets);
            Menu.SetChecked(MenuBase + "3. 热更目录内 (PersistentDataPath)", current == BundleAccessMode.PersistentData);
        }

        #endregion


        private struct AssetRaltive
        {
            public string assetsFilePathName;         //文件所在的Asset下的目录-
            public string bundleName;                 //定向到的bundle-
            public string md5;                        //此文件的md5-
            public long fileSize;                   //文件大小(字节单位)-

            public string TransToTxt(string fileName)
            { return string.Format("{0}|{1}|{2}|{3}|{4}\n", fileName, bundleName, md5, fileSize, assetsFilePathName); }
        }

        private struct AssetBundleOutInfo
        {
            public string infoHeadFileName;             //包牵引头文件名-
            public string bundleName;
            public double fileSizeMB;
            public long nFileSize;
            public int nFiles;               //包含的文件数量-
        }


        //[MenuItem(MenuBase + "打包应用资源Bundle文件(PC)", false, -10)] static void PackBundlePC() { _PackBundle(BuildTarget.StandaloneWindows64); }
        //[MenuItem(MenuBase + "打包应用资源Bundle文件(iOS)", false, -9)] static void PackBundleIOS() { _PackBundle(BuildTarget.iOS); }
        //[MenuItem(MenuBase + "打包应用资源Bundle文件(Android)", false, -8)] static void PackBundleAndroid() { _PackBundle(BuildTarget.Android); }

        // 定义打包输出的根目录（Asset同级）
        public static string BundleOutputPath => Path.Combine(Directory.GetCurrentDirectory(), "AssetBundles");

        [MenuItem("Tools/Bundle/Build PC (Windows64)")]
        public static void BuildPC() => BuildABs(BuildTarget.StandaloneWindows64);

        [MenuItem("Tools/Bundle/Build Android")]
        public static void BuildAndroid() => BuildABs(BuildTarget.Android);

        public static void BuildABs(BuildTarget target)
        {
            // 1. 确定输出路径
            string platformFolder = target.ToString();
            string outputPath = Path.Combine(BundleOutputPath, platformFolder);

            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            // 2. 准备打包信息 (这里可以结合你之前的 ResourceIndexGenerator)
            // 建议：在打包前，先运行一次 ResourceIndexGenerator 确保索引是最新的
            AssetBundleBuild[] buildMap = GetBuildMap();

            if (buildMap.Length == 0)
            {
                Debug.LogWarning("没有发现需要打包的资源，请检查 ResourceIndexGenerator 的配置。");
                return;
            }

            // 3. 执行打包
            // BuildAssetBundleOptions.ChunkBasedCompression (LZ4): 支持掉电恢复，且加载速度平衡
            BuildPipeline.BuildAssetBundles(outputPath, buildMap,
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle,
                target);

            // 4. 这里的后续：你可以把生成的 ResourceIndex.json 也拷贝到输出目录，作为总索引
            PostBuild(outputPath);

            AssetDatabase.Refresh();
            Debug.Log($"打包完成！输出路径: {outputPath}");
        }

        private static AssetBundleBuild[] GetBuildMap()
        {
            // 这里逻辑：扫描你的资源目录，根据文件夹或规则分配 BundleName
            // 暂时返回空，等下我们细化这里
            List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();
            return buildMap.ToArray();
        }

        private static void PostBuild(string path)
        {
            // 这里处理 MD5 生成、版本号更新等逻辑
        }
    }


}


