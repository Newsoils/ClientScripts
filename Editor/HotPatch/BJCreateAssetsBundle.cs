using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Reflection;
using GameCoreResourceLoad;


namespace GameCoreBuild
{
    //编辑器打包逻辑-
    public class BJCreateAssetsBundle : Editor
    {
        public static void CodeScript(string pathName, string createCode = "")
        {
            byte[] data = ResourceMgr.LoadIOFile(pathName);
            System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
            string readContent = encoding_utf8.GetString(data);

            //string res = StringEncodeMgr.Encrypt(readContent, code, iv);
            //string res = CodelizationSprite.CodeScript(readContent);
            ResourceMgr.WriteIOFile(pathName, readContent);
        }

        //-
        public static void ClearConsole()
        { 
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntries = assembly.GetType("UnityEditor.LogEntries");
            MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
            clearConsoleMethod.Invoke(new object(), null);
        }


        //-
        //[MenuItem("Assets/查找依赖显示", false, 10)]
        //private static void FindDependencies()
        //{
        //    Debug.Log("查找依赖开始");
        //    string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        //    string[] files = AssetDatabase.GetDependencies(path);
        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        if (files[i].Equals(path)) { continue; }
        //        Debug.Log(files[i], AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(files[i]));
        //    }
        //    Debug.Log("查找依赖结束");
        //}


        #region 语言选项-
        public const string PackTolMenuName = "HotPatch/打包与版本发布/";
        public const string PackLanguageMenu = PackTolMenuName + "版本语言/";
        public static string[] PackLanguage = {
            "简体中文" ,
            "繁體中文" ,
            "日本語" ,
            "English(US)" ,
            "En français",
            "Das ist Deutsch",
            "español",
        };

        public static string[] PackLanguageDir = {
            "chinese",
            "chinese_fan",
            "japanese",
            "english(US)" ,
            "french" ,
            "german" ,
            "Spanish" ,
        };


        [MenuItem(PackLanguageMenu + "简体中文")] public static void SwitchTo1() { SwitchToTargetResource(PackLanguage[0], PackLanguageDir[0]); }
        [MenuItem(PackLanguageMenu + "繁體中文")] public static void SwitchTo2() { SwitchToTargetResource(PackLanguage[1], PackLanguageDir[1]); }
        [MenuItem(PackLanguageMenu + "日本語")] public static void SwitchTo3() { SwitchToTargetResource(PackLanguage[2], PackLanguageDir[2]); }
        [MenuItem(PackLanguageMenu + "English(US)")] public static void SwitchTo4() { SwitchToTargetResource(PackLanguage[3], PackLanguageDir[3]); }
        [MenuItem(PackLanguageMenu + "En français")] public static void SwitchTo5() { SwitchToTargetResource(PackLanguage[4], PackLanguageDir[4]); }
        [MenuItem(PackLanguageMenu + "Das ist Deutsch")] public static void SwitchTo6() { SwitchToTargetResource(PackLanguage[5], PackLanguageDir[5]); }
        [MenuItem(PackLanguageMenu + "español")] public static void SwitchTo7() { SwitchToTargetResource(PackLanguage[6], PackLanguageDir[6]); }



        [MenuItem(PackTolMenuName + "版本语言/简体中文", true, -19)]
        public static bool CheckPackLanguage()
        {
            string language = EditorPrefs.GetString("PackLanguage", PackLanguage[0]);

            bool matched = false;
            for (int i = 0; i < PackLanguage.Length; i++)
            {
                string lanName = PackLanguage[i];
                string menuName = string.Format(PackLanguageMenu + "{0}", lanName);
                Menu.SetChecked(menuName, language == lanName);
                if (language == lanName)
                {
                    string inFileLang = GetCurrentLanguageMenu(ResourceMgr.CreateLoadPrefile_Language(GetCurrentLanguageDir(language))); //PlayerPrefs.GetString("PackLanguage", PackLanguage[0]);
                    if (inFileLang != language)
                    {
                        SwitchToTargetResource(lanName, PackLanguageDir[i]);
                    }
                }

                //当找不到任何选项时默认赋值第一个选项-
                if ((i + 1) == PackLanguage.Length && matched)
                {
                    EditorPrefs.SetString("PackLanguage", PackLanguage[0]);
                    language = EditorPrefs.GetString("PackLanguage", PackLanguage[0]);
                    SwitchToTargetResource(language, PackLanguageDir[0]);
                }
            }

            return true;
        }


        private static void SwitchToTargetResource(string targetKeyword, string dirName)
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("错误", string.Format("当前正在进行游戏试运行，请先关闭"), "确定");
                return;
            }

            EditorPrefs.SetString("PackLanguage", targetKeyword);
            PlayerPrefs.SetString("PackLanguageDir", dirName);

            //这里先注释掉，暂时不需要进行多语言处理
            //ResourceMgr.CreateLoadPrefile_Language(dirName, true);


            //HotPatchManager.LanguageDir = dirName;
            ChangeJsonTableCopyDest(dirName);

            //？？？
            //刷新语言UI相关显示-
            //UILanguageLinker.ResetAllSetting(dirName);

            // 刷新，可以直接在Unity工程中看见打包后的文件-
            AssetDatabase.Refresh();
        }

        private static string GetCurrentLanguageDir(string language)
        {
            string languageDir = string.Empty;

            for (int i = 0; i < PackLanguage.Length; i++)
            {
                if (language == PackLanguage[i])
                {
                    languageDir = PackLanguageDir[i];
                    break;
                }
            }

            return languageDir;
        }

        private static string GetCurrentLanguageMenu(string languageDir)
        {
            string language = string.Empty;

            for (int i = 0; i < PackLanguageDir.Length; i++)
            {
                if (languageDir == PackLanguageDir[i])
                {
                    language = PackLanguage[i];
                    break;
                }
            }

            return language;
        }


        //改写一个导表的批处理文件使其生成到当前语言包下-
        //这里他们是使用bat文件处理多语言的导出问题，以后可能用luban导出 多语言数据结构 未来如果需要多语言可以研究一下
        private static void ChangeJsonTableCopyDest(string languageDir)
        {
            //string batPath = Application.dataPath + "/../tools/__config_convert.bat";
            //if (File.Exists(batPath))
            //{
            //    byte[] data = ResourceMgr.LoadIOFile(batPath);
            //    System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
            //    string readContent = encoding_utf8.GetString(data);
            //    if (!string.IsNullOrEmpty(readContent))
            //    {
            //        string[] lines = readContent.Split('\n');
            //        for(int i = 0;i < lines.Length;i++) 
            //        {
            //            string line = lines[i];
            //            string lc = line.ToLower();
            //            if (lc.Contains("set dst") && lc.Contains("db") )
            //            {
            //                for(int l = 0; l < PackLanguageDir.Length; l++) 
            //                {
            //                    if(line.Contains(PackLanguageDir[l]))
            //                    {
            //                        if (PackLanguageDir[l] == languageDir)
            //                        {
            //                            lines[i] = "set dst=..\\assets\\art\\db\\" + languageDir + "\\json\\";

            //                            string content = "";
            //                            for (int i2 = 0; i2 < lines.Length; i2++) { content += lines[i2] + "\n"; }

            //                            ResourceMgr.WriteIOFile(batPath, content);
            //                        }

            //                        return ;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }


        //通过文件字符集与数据表中的数据内容字符进行比较获取生僻字然后加入到字符集中-
        [MenuItem("HotPatch/杂项/字符集补充", false, -19)]
        private static void CharsetCompareAndBuild()
        {
            string filePath = EditorUtility.OpenFilePanel("选择字符集文件", Application.dataPath, "txt");

            //-
            if (!File.Exists(filePath)) { EditorUtility.DisplayDialog("字符集更新完毕", "字符集比对错误:字符集文件" + filePath +"不存在", "确定");  return; }


            //-
            byte[] data = ResourceMgr.LoadIOFile(filePath);

            System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
            string readContent = encoding_utf8.GetString(data);

            Dictionary<string, string> inFileCharset = new Dictionary<string, string>(1024);
            char aa = readContent[0];
            for (int i = 0; i < readContent.Length;i++)
            {
                string s = "" + readContent[i];
                if (!inFileCharset.ContainsKey(s))
                {
                    inFileCharset.Add(s,s);
                }
                
            }

            int oldCharsetCount = inFileCharset.Count;

            // hjTableAgent 和 hjTable 是项目自定义的表格数据管理类，用于读取语言表。
            // 暂时不处理具体实现，直接注释掉：
            /*
            hjTableAgent ta = new hjTableAgent();
            string[] strLabelKeys = ta.AllKeys("language_cfg");
            for(int i = 0; i < strLabelKeys.Length; i++)
            {
                hjTable tb = ta["language_cfg", strLabelKeys[i]];
                string tCNContext = tb["CN", ""];

                for (int n = 0; n < tCNContext.Length; n++)
                {
                    string s = "" + tCNContext[n];
                    if (!inFileCharset.ContainsKey(s)) { inFileCharset.Add(s,s); }
                }
            }
            */


            //重新记录新录陆的字符集-
            if (inFileCharset.Count > oldCharsetCount)
            {
                string newCharset = "";
                foreach (string s in inFileCharset.Keys) { newCharset += s; }

                ResourceMgr.WriteTxtFile(newCharset, filePath);
            }

            EditorUtility.DisplayDialog("字符集更新完毕", 
                "旧字符集数量为:" + oldCharsetCount + " 新字符集数量为:" + inFileCharset.Count, "完成");
        }
        #endregion


        #region BundleAccess选项-
        public const string PackBundleAccessMenu = PackTolMenuName + "Bundle访问模式/";
        public static string[] PackBundleAccess = { "工程Asset外围缓存包内(只用于编辑器模式)", "游戏包体内", "热更目录内", "编辑器镜像数据(只用于编辑器模式)" };

        [MenuItem(PackBundleAccessMenu + "工程Asset外围缓存包内(只用于编辑器模式)")] public static void SwitchBa0() { SwitchToTargetBundleAccess(PackBundleAccess[0], 0); }
        [MenuItem(PackBundleAccessMenu + "游戏包体内")] public static void SwitchBa1() { SwitchToTargetBundleAccess(PackBundleAccess[1], 1); }
        [MenuItem(PackBundleAccessMenu + "热更目录内")] public static void SwitchBa2() { SwitchToTargetBundleAccess(PackBundleAccess[2], 2); }
        [MenuItem(PackBundleAccessMenu + "编辑器镜像数据(只用于编辑器模式)")] public static void SwitchBa3() { SwitchToTargetBundleAccess(PackBundleAccess[3], 3); }


        [MenuItem(PackTolMenuName + "Bundle访问模式/工程Asset外围缓存包内(只用于编辑器模式)", true, -20)]
        public static bool CheckPackBundleAccess()
        {
            string modeName = EditorPrefs.GetString("RMBundleAccessKey", PackBundleAccess[0]);

            bool matched = false;
            for (int i = 0; i < PackBundleAccess.Length; i++)
            {
                string lanName = PackBundleAccess[i];
                string menuName = string.Format(PackBundleAccessMenu + "{0}", lanName);
                Menu.SetChecked(menuName, modeName == lanName);
                if (modeName == lanName)
                {
                    string inFileLang = PackBundleAccess[ResourceMgr.CreateLoadPrefile_BundleAccess(GetCurrentBundleAccess(modeName))];
                    if (inFileLang != modeName)
                    {
                        matched = true;
                        SwitchToTargetBundleAccess(lanName, i);
                    }
                }

                //当找不到任何选项时默认赋值第一个选项-
                if ((i + 1) == PackBundleAccess.Length && matched)
                {
                    EditorPrefs.SetString("RMBundleAccessKey", PackBundleAccess[0]);
                    modeName = EditorPrefs.GetString("RMBundleAccessKey", PackBundleAccess[0]);
                    SwitchToTargetBundleAccess(modeName, 0);
                }
            }


            return true;
        }

        private static void SwitchToTargetBundleAccess(string targetKeyword, int mode)
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("错误", string.Format("当前正在进行游戏试运行，请先关闭"), "确定");
                return;
            }

            EditorPrefs.SetString("RMBundleAccessKey", targetKeyword);
            PlayerPrefs.SetInt("RMBundleAccess", mode);

            ResourceMgr.CreateLoadPrefile_BundleAccess(mode, true);
            //HotPatchManager.LanguageDir = dirName;

            // 刷新，可以直接在Unity工程中看见打包后的文件-
            AssetDatabase.Refresh();
        }

        private static int GetCurrentBundleAccess(string language)
        {
            int rtn = 0;

            for (int i = 0; i < PackBundleAccess.Length; i++)
            {
                if (language == PackBundleAccess[i])
                {
                    rtn = i;
                    break;
                }
            }

            return rtn;
        }

        public static int GetCurrentBundleAccess()
        {
            return GetCurrentBundleAccess(EditorPrefs.GetString("RMBundleAccessKey", PackBundleAccess[0]));
        }
        #endregion





        #region 打包资源文件-
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

        private const long BUNDLE_SIZE = 50000000;                 //50mb- //平均一个包体占据大小(字节数)-


        //[MenuItem(PackTolMenuName + "打包应用资源仅脚本Bundle文件(ActivitPlatform)", false, -11)] static void PackScriptModuleBundlePC(){ _PackBundleOnlyScript(); }
        [MenuItem(PackTolMenuName + "打包应用资源Bundle文件(PC)", false, -10)] static void PackBundlePC() { _PackBundle(BuildTarget.StandaloneWindows64); }
        [MenuItem(PackTolMenuName + "打包应用资源Bundle文件(iOS)", false, -9)] static void PackBundleIOS() { _PackBundle(BuildTarget.iOS); }
        [MenuItem(PackTolMenuName + "打包应用资源Bundle文件(Android)", false, -8)] static void PackBundleAndroid() { _PackBundle(BuildTarget.Android); }
        

        private static void _PackBundleOnlyScript(bool overDialog = true)
        {
            int         lastVersion = ServiceCompareHeadCount;
            BuildTarget bundlePlatform = ServiceCompareHeadBundlePlatform;

            //-
            List<AssetBundleBuild> assetBundleInfo = new List<AssetBundleBuild>(32);
            List<AssetBundleOutInfo> assetBundleOutInfo = new List<AssetBundleOutInfo>(32);
            List<string> doneMsgs = new List<string>();

            //-
            string bundleDirPathBuff = ResourceMgr.BundleDestDir + ResourceMgr.BundleActionName + "/Temp";
            string bundleDirPath = ResourceMgr.BundleDestDir + ResourceMgr.BundleActionName;

            if (!Directory.Exists(bundleDirPathBuff)) { Directory.CreateDirectory(bundleDirPathBuff); }

            string JSModule = "JSModule";
            string tempStr = bundleDirPath + "/" + JSModule.ToLower() + ResourceMgr.BundleRelativeExtName;
            if (File.Exists(tempStr))
            {
                string tempStr2 = bundleDirPathBuff + "/" + JSModule.ToLower() + ResourceMgr.BundleRelativeExtName;
                File.Move(tempStr, tempStr2);
            }

            //打包信息收集-
            assetBundleInfo.AddRange(
                BundleAssetsDir(ResourceMgr.BundleSrcScript, JSModule, bundleDirPathBuff, "*.mjs", assetBundleOutInfo, doneMsgs));


            //开始打包-
            if (assetBundleInfo.Count > 0)
            {
                Caching.ClearCache();

                BuildPipeline.BuildAssetBundles(bundleDirPathBuff, assetBundleInfo.ToArray(),
                    BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle/*| BuildAssetBundleOptions.DisableWriteTypeTree*/,
                    bundlePlatform);

                //删除不怎么需要的包的.manifest文件(看后面情况吧，目前只保留带有包关联信息的BundlePag.manifest文件)-
                //.manifest文件进行unity内部的增量打包机制-
                //其实我们自创的".rltif"从某种意义上替代了它的作用(信息关联)
                string src = "";
                string dest = "";
                foreach (var info in assetBundleOutInfo.ToArray())
                {
                    Debug.Log(
                        string.Format("<color=yellow>生成包:{0}  压缩前大小为:{1:N2}MB  包含文件数:{2}</color>",
                        info.bundleName, info.fileSizeMB, info.nFiles) +
                        (info.nFiles > 0 ? "" : "<color=red>(删除原始Bundle文件)</color>")
                        );

                    if (info.nFiles > 0)
                    {
                        string bundleName = Path.GetFileNameWithoutExtension(info.bundleName);
                        src = bundleDirPathBuff + "/" + info.bundleName;
                        dest = bundleDirPath + "/" + info.bundleName;

                        ResourceMgr.BundleEncode(src);
                        File.Delete(dest);
                        File.Move(src, dest);

                        //src = bundleDirPathBuff + "/" + bundleName + ResourceMgr.BundleResKeyExtName;
                        //dest = bundleDirPath + "/" + bundleName + ResourceMgr.BundleResKeyExtName;

                        //File.Delete(dest);
                        //File.Move(src, dest);
                    }
                }

                src = bundleDirPathBuff + "/" + JSModule.ToLower() + ResourceMgr.BundleRelativeExtName;
                dest = bundleDirPath + "/" + JSModule.ToLower() + ResourceMgr.BundleRelativeExtName;
                File.Delete(dest);
                File.Move(src, dest);

                FileUtil.DeleteFileOrDirectory(bundleDirPathBuff);

                

                //写入这个版本的文件md5和文件号-
                CreateServiceCompareHead(lastVersion, bundlePlatform);

                // 刷新，可以直接在Unity工程中看见打包后的文件-
                if(overDialog)
                {
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("资源打Bundle包", "打包完成", "确定");
                }


                //清理缓存-
                assetBundleInfo.Clear();
                assetBundleOutInfo.Clear();
                doneMsgs.Clear();
            }
        }


        private static void _PackBundle(BuildTarget forSystemPlatform, bool overDialog = true)
        {
            int         lastVersion = ServiceCompareHeadCount;
            BuildTarget bundlePlatform = ServiceCompareHeadBundlePlatform;

            //清理信息显示框-
            ClearConsole();

            //清理旧的打包文件,*****仅仅在打包之前存在平台切换时才做彻底根除清理*****,除此都是迭代更新-
            //注意!发完包unity会根据包平台格式自己转换发布平台-
            string bundleDirPath = ResourceMgr.BundleDestDir + ResourceMgr.BundleActionName;
            if(ServiceCompareHeadBundlePlatform != forSystemPlatform)
            {
                bundlePlatform = forSystemPlatform;

                FileUtil.DeleteFileOrDirectory(bundleDirPath);
                FileUtil.DeleteFileOrDirectory(bundleDirPath + ".meta");
            }

            //-
            if (!Directory.Exists(bundleDirPath)) { Directory.CreateDirectory(bundleDirPath); }

            //-
            List<AssetBundleBuild> assetBundleInfo = new List<AssetBundleBuild>(32);
            List<AssetBundleOutInfo> assetBundleOutInfo = new List<AssetBundleOutInfo>(32);
            List<string> doneMsgs = new List<string>();

            //Asset/DB目录-
            //先把语言标识文件打进各个语言包中-
            //for (int i = 0; i < PackLanguageDir.Length; i++)
            //{
            //    string dirName = PackLanguageDir[i].ToLower();
            //    string bundleDBLangPath = ResourceMgr.BundleSrcDir + "DB/" + dirName;
            //    if (!Directory.Exists(bundleDBLangPath)) { Directory.CreateDirectory(bundleDBLangPath); }

            //    assetBundleInfo.AddRange(
            //        BundleAssetsDir(string.Format("{0}{1}{2}", ResourceMgr.BundleSrcDir, "DB/", dirName),
            //                        string.Format("lg_{0}", dirName),
            //                        bundleDirPath,
            //                        "*.*",                                  //语言包中的所有内容都打入-
            //                        assetBundleOutInfo,
            //                        doneMsgs));
            //}


            //Asset/Art目录-
            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcDir, "Res"),
                                "TextureModule", bundleDirPath, "*.png|*.jpg|*.dds|*.gif|*.psd|*.tga|*.bmp|*.exr", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcDir, "Res"),
                                "ChunkModule", bundleDirPath, "*.fbx|*.asset|*.anim|*.controller|*.mesh|*.txt|*.json", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcDir, "Res"),
                                    "SoundModule", bundleDirPath, "*.wav|*.mp3|*.ogg|*.ttf|*.otf", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcDir, "Res"),
                                "URPModule", bundleDirPath, "*.mat|*.shader|*.hlsl|*.shadervariants", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcDir, "Prefabs"),
                                "LogicModule", bundleDirPath, "*.prefab", assetBundleOutInfo, doneMsgs));



            //边玩边下模块-
            for (int i = 0; i < PackLanguageDir.Length; i++)
            {
                string dirName = PackLanguageDir[i].ToLower();
                string bundleDBLangPath = ResourceMgr.BundleSrcPlayMoreDir + "DB/" + dirName;
                if (!Directory.Exists(bundleDBLangPath)) { Directory.CreateDirectory(bundleDBLangPath); }

                assetBundleInfo.AddRange(
                    BundleAssetsDir(string.Format("{0}{1}{2}", ResourceMgr.BundleSrcPlayMoreDir, "DB/", dirName),
                                    string.Format("PlayMore-lg_{0}", dirName),
                                    bundleDirPath,
                                    "*.*",                                  //语言包中的所有内容都打入-
                                    assetBundleOutInfo,
                                    doneMsgs));
            }

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcPlayMoreDir, "Res"),
                    "PlayMore-TextureModule", bundleDirPath, "*.png|*.jpg|*.dds|*.gif|*.psd|*.tga|*.bmp|*.exr", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcPlayMoreDir, "Res"),
                                "PlayMore-ChunkModule", bundleDirPath, "*.fbx|*.asset|*.anim|*.controller|*.mesh", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcPlayMoreDir, "Res"),
                                    "PlayMore-SoundModule", bundleDirPath, "*.wav|*.mp3|*.ogg|*.ttf|*.otf", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcPlayMoreDir, "Res"),
                                "PlayMore-URPModule", bundleDirPath, "*.mat|*.shader|*.hlsl|*.shadervariants", assetBundleOutInfo, doneMsgs));

            assetBundleInfo.AddRange(
                BundleAssetsDir(string.Format("{0}{1}", ResourceMgr.BundleSrcPlayMoreDir, "Prefab"),
                                "PlayMore-LogicModule", bundleDirPath, "*.prefab", assetBundleOutInfo, doneMsgs));
            



            //收集母包中对资源的依赖关系-
            MotherSceneDependenceBuild(bundleDirPath, assetBundleInfo);


            // 开始打包-
            if (assetBundleInfo.Count > 0)
            {
                Caching.ClearCache();

                // 添加调试信息
                Debug.Log($"准备打包 {assetBundleInfo.Count} 个AssetBundle");
                foreach (var build in assetBundleInfo)
                {
                    Debug.Log($"Bundle: {build.assetBundleName}, 包含 {build.assetNames.Length} 个资源");
                }

                // 执行打包并检查结果
                var manifest = BuildPipeline.BuildAssetBundles(bundleDirPath, assetBundleInfo.ToArray(),
                    BuildAssetBundleOptions.ChunkBasedCompression,
                    forSystemPlatform);

                // 检查打包结果
                if (manifest == null)
                {
                    Debug.LogError("AssetBundle打包失败！manifest为null");
                }
                else
                {
                    string[] allBundles = manifest.GetAllAssetBundles();
                    Debug.Log($"打包成功！生成了 {allBundles.Length} 个Bundle:");
                    foreach (string bundleName in allBundles)
                    {
                        Debug.Log($" - {bundleName}");
                    }
                }
            }

            // TypeTree：Unity 在 AssetBundle 中写入的类型结构描述信息。
            // 包含各资源对象的字段布局、类型名称、序列化结构等。
            // 作用：让不同 Unity 版本、不同平台在加载 AB 时正确反序列化资源。
            // 若禁用（DisableWriteTypeTree）：
            //    - AB 包体会更小，加载更快
            //    - 但跨版本、跨平台加载极易失败，不利于热更新
            // 通常建议保留 TypeTree 以确保兼容性与稳定性。

            ////开始打包-
            //if (assetBundleInfo.Count > 0)
            //{
            //    Caching.ClearCache();
            //    BuildPipeline.BuildAssetBundles(bundleDirPath, assetBundleInfo.ToArray(),
            //        BuildAssetBundleOptions.ChunkBasedCompression /*| BuildAssetBundleOptions.DisableWriteTypeTree*/,
            //        forSystemPlatform);
            //}


            //打包场景文件-
            string[] invaildScenes = BundleScene(ResourceMgr.BundleSrcScenes, bundleDirPath, forSystemPlatform);
            //string[] invaildScenes = new string[0];


            //删除不怎么需要的包的.manifest文件(看后面情况吧，目前只保留带有包关联信息的BundlePag.manifest文件)-
            //.manifest文件进行unity内部的增量打包机制-
            //其实我们自创的".rltif"从某种意义上替代了它的作用(信息关联)-

            //这里只留下了一个“BundlePag.manifest”的文件，其余的manifest都被删除掉了
            string[] extfileNames = Directory.GetFiles(bundleDirPath, "*.manifest", SearchOption.AllDirectories);
            string BundleActionFileName = ResourceMgr.BundleActionName + ".manifest";
            foreach (var fileNamePath in extfileNames)
            {
                if (fileNamePath.Contains(BundleActionFileName) ) { continue; }

                File.Delete(fileNamePath);
            }


            // 补充打印整理包信息消息-
            foreach (var msg in doneMsgs) { Debug.Log(msg); }

            // 整合最后的bundle包信息并显示,删除不用的空的bundle包-
            List<string> checkBundle = new List<string>(Directory.GetFiles(bundleDirPath, "*" + ResourceMgr.BundleResExtName, SearchOption.AllDirectories));

            foreach (var info in assetBundleOutInfo.ToArray())
            {
                Debug.Log(
                    string.Format("<color=yellow>生成包:{0}  压缩前大小为:{1:N2}MB  包含文件数:{2}</color>",
                    info.bundleName, info.fileSizeMB, info.nFiles) +
                    (info.nFiles > 0 ? "" : "<color=red>(删除原始Bundle文件)</color>")
                    );

                //删除上个打包版本多余无效的ResourceMgr.BundleResExtName文件-
                if (info.nFiles < 1)
                {
                    bool existFileInfo = checkBundle.Exists((string str) =>
                    {
                        string fileName = Path.GetFileName(str);
                        return fileName == info.bundleName;
                    });

                    if (existFileInfo) 
                    {
                        File.Delete(bundleDirPath + "/" + info.bundleName);

                        //删除对应 ResourceMgr.BundleResKeyExtName文件-
                        //string fileName = Path.GetFileNameWithoutExtension(info.bundleName);
                        //File.Delete(bundleDirPath + "/" + fileName + ResourceMgr.BundleResKeyExtName);
                    }
                }
            }

            //删除上个打包版本多余无效的ResourceMgr.BundleResExtName文件-
            foreach (string checkBundleFile in checkBundle)
            {
                string fileName = Path.GetFileName(checkBundleFile);
                bool existFileInfo = assetBundleOutInfo.Exists((AssetBundleOutInfo _oi) => { return _oi.nFiles > 0 && _oi.bundleName == fileName; });

                //文件不再属于包文件,删除掉它-
                if (!existFileInfo) 
                { 
                    File.Delete(checkBundleFile);

                    ////删除对应ResourceMgr.BundleResKeyExtName文件-
                    //fileName = Path.GetFileNameWithoutExtension(checkBundleFile);
                    //File.Delete(bundleDirPath + "/" + fileName + ResourceMgr.BundleResKeyExtName);
                }
            }


            //对这些包进行加密-
            checkBundle.Clear();
            checkBundle.AddRange(Directory.GetFiles(bundleDirPath, "*" + ResourceMgr.BundleResExtName, SearchOption.AllDirectories) );
            foreach (string checkBundleFile in checkBundle) { ResourceMgr.BundleEncode(checkBundleFile); }


            //删除上个打包版本多余无效的ResourceMgr.BundleRelativeExtName文件-
            checkBundle.Clear();
            checkBundle.AddRange(Directory.GetFiles(bundleDirPath, "*" + ResourceMgr.BundleRelativeExtName, SearchOption.AllDirectories));
            foreach (string checkBundleFile in checkBundle)
            {
                if (File.Exists(checkBundleFile))
                {
                    byte[] bt = ResourceMgr.LoadIOFile(checkBundleFile);
                    System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
                    string readContent = encoding_utf8.GetString(bt);
                    if (string.IsNullOrEmpty(readContent))
                    {
                        //无效头部文件删除-
                        File.Delete(checkBundleFile);
                    }
                }
            }


            //删除上个打包版本过时的场景-
            //foreach (string sceneName in invaildScenes)
            //{
            //    Debug.Log("<color=red>删除上个打包版本中过时的场景文件:" + sceneName + "</color>");
            //    File.Delete(bundleDirPath + "/" + sceneName);
            //}


            //对场景文件进行加密-
            checkBundle.Clear();
            checkBundle.AddRange(Directory.GetFiles(bundleDirPath, "*" + ResourceMgr.BundleSceneExtName, SearchOption.AllDirectories));
            foreach (string checkBundleFile in checkBundle) { ResourceMgr.BundleEncode(checkBundleFile); }

            //写入这个版本的文件md5和文件号-
            CreateServiceCompareHead(lastVersion, bundlePlatform);



            // 刷新，可以直接在Unity工程中看见打包后的文件-
            if (overDialog)
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("资源打Bundle包", "打包完成", "确定");
            }



            //清理缓存-
            checkBundle.Clear();
            assetBundleInfo.Clear();
            assetBundleOutInfo.Clear();
            doneMsgs.Clear();
        }


        //创建服务器比对头- 

        /// <summary>
        /// 每次打包packageVer都会+1,用于热更比对使用-
        /// </summary>
        private static int ServiceCompareHeadCount{ 
            get{
                int packageVer = 0;
                string filePath = ResourceMgr.BundleDestDir + ResourceMgr.BundleActionName + "/" + "HotCompare.db";
                if (File.Exists(filePath))
                {
                    byte[] bt = ResourceMgr.LoadIOFile(filePath);
                    System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
                    string readContent = encoding_utf8.GetString(bt);
                    string[] lines = readContent.Split('\n');

                    if (lines.Length > 0) { int.TryParse(lines[0], out packageVer); }
                }

                return packageVer;
            } }

        /// <summary>
        /// 拿取目标平台，这个拼台和上面的packagerVer一样存在HotCompare.db文件中-
        /// </summary>
        private static BuildTarget ServiceCompareHeadBundlePlatform
        {
            get
            {
                int btg = (int)BuildTarget.NoTarget;
                string filePath = ResourceMgr.BundleDestDir + ResourceMgr.BundleActionName + "/" + "HotCompare.db";
                if (File.Exists(filePath))
                {
                    byte[] bt = ResourceMgr.LoadIOFile(filePath);
                    System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
                    string readContent = encoding_utf8.GetString(bt);
                    string[] lines = readContent.Split('\n');

                    if (lines.Length > 0) { int.TryParse(lines[2], out btg); }
                }

                return (BuildTarget)btg;
            }
        }


        private static void CreateServiceCompareHead(int lastValue,BuildTarget bundlePlatform)
        {
            string bundleDirPath = ResourceMgr.BundleDestDir + ResourceMgr.BundleActionName;

            int     packageVer = lastValue;
            string  filePath = bundleDirPath + "/" + "HotCompare.db";
            if(File.Exists(filePath)) 
            {
                byte[] bt = ResourceMgr.LoadIOFile(filePath);
                System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
                string readContent = encoding_utf8.GetString(bt);
                string[] lines = readContent.Split('\n');
            
                if(lines.Length > 0) { int.TryParse(lines[0], out packageVer); }
            }

            packageVer++;

            string content = packageVer + "\n";
            content += Application.version/*PlayerSettings.bundleVersion*/ + "\n";
            content += (int)bundlePlatform + "\n";

            string[] filePaths = Directory.GetFiles(bundleDirPath, "*.*", SearchOption.AllDirectories);
            foreach (string path in filePaths)
            {
                string fileName = Path.GetFileName(path);
                if (fileName.Contains("HotCompare.db")) { continue; }

                string fileMd5 = StringEncodeMgr.GetMd5(ResourceMgr.LoadIOFile(path));
                content += fileName + "|" + fileMd5 + "|" + path + "\n";
            }

            ResourceMgr.WriteIOFile(filePath,content);
        }


        //打一个小开发目录的包-
        private static AssetBundleBuild[] BundleAssetsDir(
                                                            string readyDir, 
                                                            string bundleName, 
                                                            string bundleDirPath,
                                                            string searchFileCondition,
                                                            List<AssetBundleOutInfo> assetBundleOutInfo,
                                                            List<string>    msgs
                                                          )
        {
            string  content = "";
            bool    isLangBundle  = bundleName.Contains("lg_");                 //语言包的第一号包体永远是装有且只有数据表的包-

            bundleName = bundleName.ToLower();

            //先在指定目录下找到上一次的资源关系文件-
            Dictionary<string, string>          saveTxtList = new Dictionary<string, string>(128);
            Dictionary<string, AssetRaltive>    oldAssetsRaltive = new Dictionary<string, AssetRaltive>(128);      //前一次资源关系文件的记录-
            string                              oldAssetsRaltiveFilePath = string.Format("{0}/{1}{2}", bundleDirPath, bundleName, ResourceMgr.BundleRelativeExtName);

            if (File.Exists(oldAssetsRaltiveFilePath))
            {
                byte[] bt = ResourceMgr.LoadIOFile(oldAssetsRaltiveFilePath);
                System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
                string readContent = encoding_utf8.GetString(bt);

                string[] lines = readContent.Split('\n');
                foreach (string line in lines)
                {
                    string[] words = line.Split('|');
                    if (words.Length > 3 && !string.IsNullOrEmpty(words[0]) && !oldAssetsRaltive.ContainsKey(words[0]))
                    {
                        oldAssetsRaltive.Add(words[0].ToLower(), new AssetRaltive()
                        {
                            bundleName = words[1],
                            md5 = words[2],
                            fileSize = System.Convert.ToInt64(words[3]),
                            assetsFilePathName = words[4],
                        });

                        //记录到信息文件中-
                        if (!saveTxtList.ContainsKey(words[0])) { saveTxtList.Add(words[0], words[0]); }
                    }
                }
            }


            Dictionary<string,string>           fileSameNameSave =new Dictionary<string, string>(32);
            Dictionary<string, AssetRaltive>    assets = new Dictionary<string, AssetRaltive>(128);          //需要打包的资源们-  
            List<string>                        oldAssetsKeyList = new List<string>(oldAssetsRaltive.Keys);
            List<string>                        fileNames =new List<string>(128);
            //当前打包的是哪些格式的文件-
            string[]                            fileExts = searchFileCondition.Split('|');
            foreach (var ext in fileExts)
            {
                string[] extfileNames = Directory.GetFiles(readyDir, ext, SearchOption.AllDirectories);
                fileNames.AddRange(extfileNames);
            }
        


            foreach (string filePathName in fileNames)
            {
                string filePathNameTmp = filePathName;
                string fileName = Path.GetFileName(filePathName);
                string assetsFilePathName = ResourceMgr.GetAssetRelativePath(filePathName);
                string fileMd5 = StringEncodeMgr.GetMd5(ResourceMgr.LoadIOFile(filePathName));

                if (fileName.Contains("__")) { continue; }          //包含此符号的文件忽略不打包-
                if (fileName.Contains(".meta")) { continue; }       //meta文件不包含其中-

                //对于相同的文件产生警告-
                string lowFileName = fileName.ToLower();
                if (!fileSameNameSave.ContainsKey(lowFileName))
                {
                    fileSameNameSave.Add(lowFileName, filePathNameTmp);
                } else {
                    msgs.Add(
                        string.Format("<color=yellow> 警告:两个打包文件名字相同在\"{0}\"和\"{1}\"</color>,重复的后者会被忽略", 
                        fileSameNameSave[lowFileName] ,
                        filePathNameTmp) );
                    continue;
                }

                //先查看一下这个文件有没有在旧的信息中,
                //如果有那么看一下文件是否发生了内容的改变-
                bool needUpdate = true;
                string parentBundle = string.Empty;
                for (int i = 0; i < oldAssetsKeyList.Count; i++)
                {
                    string keyName = oldAssetsKeyList[i].ToLower();             //降为小字母比较-

                    if (lowFileName == keyName)
                    {
                        AssetRaltive ar = oldAssetsRaltive[keyName];

                        if (ar.md5 == fileMd5)
                        {
                            needUpdate = false;
                        } else {
                            //旧的记载文件的实质内容有更改的情况产生-
                            //记载归属bundle名-
                            parentBundle = ar.bundleName;

                            //重新确定这个更改文件后的大小-
                            FileInfo fi = new FileInfo(filePathNameTmp);
                            ar.fileSize = fi.Length;
                            ar.assetsFilePathName = assetsFilePathName;
                            ar.md5 = fileMd5;
                            oldAssetsRaltive[keyName] = ar;

                            msgs.Add("<color=green>文件:" + ar.assetsFilePathName + "发生了变化</color>");
                        }

                        break;
                    }
                }

                //加入打包缓冲-
                if (needUpdate)
                {
                    if (!assets.ContainsKey(lowFileName))
                    {
                        FileInfo fi = new FileInfo(filePathNameTmp);
                        assets.Add(lowFileName, new AssetRaltive()
                        {
                            assetsFilePathName = assetsFilePathName,
                            bundleName = parentBundle,
                            md5 = fileMd5,                       //临时先放一下路径，方便后续打包，本应该是指向的Md5码-
                            fileSize = fi.Length,
                        });
                    }
                }
            }


            //得出打包缓冲后开始进行打包-
            //如果这个文件已经和包产生了关系，那么会不顾大小上限规定直接对这个包进行重新打包
            //得到文件对应的包关系集合大小，如果大小集合超过了限制规定，则重新开辟新包

            //得出资源需要牵扯到的bundle包
            Dictionary<string, long>    saveBundleSize = new Dictionary<string, long>(30);
            Dictionary<string, string>  noFileBundle = new Dictionary<string, string>(10);
            List<AssetBundleBuild>      assetBundleInfo = new List<AssetBundleBuild>(32);                     //要存储的包名字-

            //对已经存在的bundle包进行统计-
            foreach (string keyName in oldAssetsRaltive.Keys)
            {
                AssetRaltive ar = oldAssetsRaltive[keyName];
                if (!saveBundleSize.ContainsKey(ar.bundleName)) { saveBundleSize.Add(ar.bundleName, 0); }
                saveBundleSize[ar.bundleName] += ar.fileSize;


                //如果这个包文件在磁盘被删除了,那么也需要重新打包-
                string relativeBundleName = ar.bundleName;
                string bundlePath = string.Format("{0}/{1}{2}", bundleDirPath, relativeBundleName, ResourceMgr.BundleResExtName);
                string bundleManiPath = string.Format("{0}/{1}{2}.manifest", bundleDirPath, relativeBundleName, ResourceMgr.BundleResExtName);

                if (!File.Exists(bundlePath) )  //可能bundle包文件被删除了-
                {
                    if (!noFileBundle.ContainsKey(relativeBundleName))
                    {
                        //删除半个残废的文件-
                        File.Delete(bundlePath);
                        File.Delete(bundleManiPath);

                        noFileBundle.Add(relativeBundleName, relativeBundleName);
                        Debug.Log(string.Format("<color=yellow>警告:曾经生成的包\"{0}\"被删除了,对此包的内容会进行重新打包</color>", relativeBundleName));
                    }

                    //这里根据unity特性需要每次对所有资源进行重新打包(因为BundlePag.manifest的依赖关系需要重新建立)，但原先我们的设计思路是只重新打包变更的文件-
                }

                //如果准备打包的文件实际不存在的情况(实际文件被删除了)-
                if (!File.Exists(ar.assetsFilePathName))
                {
                    msgs.Add(string.Format("<color=yellow>警告:曾经在\"{0}\"包中的文件源头\"{1}\"被删除了</color>", ar.bundleName, ar.assetsFilePathName));

                    //从原来包信息中剔除-
                    saveTxtList.Remove(keyName);
                    if (saveBundleSize.ContainsKey(ar.bundleName))
                    {
                        saveBundleSize[ar.bundleName] -= ar.fileSize;
                        if (saveBundleSize[ar.bundleName] < 0) { saveBundleSize[ar.bundleName] = 0; }
                    }
                
                    continue;
                }

                //这里根据unity特性需要每次对所有资源进行重新打包(因为BundlePag.manifest的依赖关系需要重新建立)，但原先我们的设计思路是只重新打包变更的文件-
                //对上一次打包的文件重新打进包里-
                if (!assets.ContainsKey(keyName) )
                {
                    assets.Add(keyName, ar);
                }
            }


            //对已经进入重新修改的资源文件所牵扯到的bundle包文件都要重新加入打包-
            List<string> assetsKeyList = new List<string>(assets.Keys);
            foreach (var key in assetsKeyList)
            {
                AssetRaltive ar = assets[key];
                string       relativeBundleName = ar.bundleName;

                //还没有进行bundle关联的文件强制进行关联检索手续-
                if (string.IsNullOrEmpty(ar.bundleName))
                {
                    bool isCsvFile =false;

                    //得出最后一个存储bundle-
                    if (isLangBundle)
                    {
                        //对于语言包的硬性规定做特殊处理-
                        //语言包的第一号包体永远是装有且只有数据表的包-
                        string keyLow = key.ToLower();
                        if(keyLow.Contains(".csv") ||
                            keyLow.Contains(".json") ||
                            keyLow.Contains("releaselanguage.txt") )
                        {
                            relativeBundleName = bundleName + "_1";
                            isCsvFile = true;
                        } else {
                            string[] num = GetLastBundleInTheirSort(saveBundleSize, bundleName).Split('_');
                            int nnum;
                            int.TryParse(num[num.Length - 1],out nnum);
                            nnum = Mathf.Max(nnum, 2);
                            relativeBundleName = bundleName + "_" + nnum;
                        }
                    } else {
                        relativeBundleName = GetLastBundleInTheirSort(saveBundleSize, bundleName); 
                    }

                    //保证始终有一个原始的存在-
                    if (!saveBundleSize.ContainsKey(relativeBundleName)) { saveBundleSize.Add(relativeBundleName, 0); } 

                    //这个包的存储量到达规定上限了，换一个新包存储-
                    if ( saveBundleSize[relativeBundleName] > BUNDLE_SIZE &&
                        !isCsvFile )
                    {
                        _next_bundle:
                        string[] bundleNameInfo = relativeBundleName.Split('_');
                        int number = 0;
                        int.TryParse(bundleNameInfo[bundleNameInfo.Length - 1], out number);
                        number++;
                        relativeBundleName = bundleNameInfo.Length > 2 ?
                            string.Format("{0}_{1}_{2}", bundleNameInfo[0], bundleNameInfo[1], number):
                            string.Format("{0}_{1}", bundleNameInfo[0], number);
                        if (!saveBundleSize.ContainsKey(relativeBundleName)) { saveBundleSize.Add(relativeBundleName, 0); }
                        else {
                            //如果下一个包的容量还是过大，继续再下一个-
                            if(saveBundleSize[relativeBundleName] > BUNDLE_SIZE)
                            { goto _next_bundle; }
                        }
                    }

                    saveBundleSize[relativeBundleName] +=ar.fileSize;

                    ar.bundleName = relativeBundleName;
                    assets[key] = ar;
                }

                //这里根据unity特性需要每次对所有资源进行重新打包(因为BundlePag.manifest的依赖关系需要重新建立)，但原先我们的设计思路是只重新打包变更的文件-
                if (saveBundleSize.ContainsKey(relativeBundleName))
                {
                    foreach (string keyName in oldAssetsRaltive.Keys)
                    {
                        AssetRaltive ar2 = oldAssetsRaltive[keyName];
                        if (relativeBundleName == ar2.bundleName &&               //都扯到了一样的bundle-
                            saveTxtList.ContainsKey(keyName) &&                   //文件必须存在-
                           !assets.ContainsKey(keyName))                          //还没有到重更新列表中的-
                        {
                            assets.Add(keyName, ar2);
                        }
                    }
                }
            }


            //对整理后的资源准备重新打包-
            foreach (var key in assets.Keys)
            {
                AssetRaltive ar = assets[key];

                if (!string.IsNullOrEmpty(ar.bundleName))
                {
                    if (ar.fileSize <= 0) { continue; }         //文件大小为0的文件打不进包里，所以过滤掉-

                    assetBundleInfo.Add(new AssetBundleBuild
                    {
                        assetBundleName = ar.bundleName,
                        assetNames = new string[] { ar.assetsFilePathName },
                        assetBundleVariant = ResourceMgr.BundleResExtName.Replace(".",""),
                    });

                    //记录到信息文件中-
                    if (!saveTxtList.ContainsKey(key)) { saveTxtList.Add(key, key); }
                }
            }



            //写入信息文件-
            Dictionary<string,List<string>> bbns = new Dictionary<string, List<string>>();
            foreach (var fileName in saveTxtList.Keys)
            {
                AssetRaltive ar = new AssetRaltive { fileSize = 0, assetsFilePathName = string.Empty, bundleName = string.Empty, md5 = string.Empty, };
                if (assets.ContainsKey(fileName)) { ar = assets[fileName]; }
                else if (oldAssetsRaltive.ContainsKey(fileName)) { ar = oldAssetsRaltive[fileName]; }

                if (ar.fileSize > 0)
                {
                    content += ar.TransToTxt(fileName);
                }

                ////下载迭代标识文件整理-
                //if (!bbns.ContainsKey(ar.bundleName) )
                //{ List<string> tl = new List<string>(); bbns.Add(ar.bundleName, tl); }

                //bbns[ar.bundleName].Add(ar.md5);
            }


            ////写入迭代标识文件-
            ////a电脑打出来的bundle，b电脑读不出来，unity内部crc码关联无效了, 所以这个平凑无效
            ///*所以我们想到把Unity工程中的Library文件夹也加入到版本管理中，将脚本编译后的文件也进行缓存。
            ///需要打渠道包时，将Unity工程拉下来，再把Library缓存和脚本缓存也拉下来放到对应的位置中，
            ///让Unity不执行资源导入。经测试，此方法可行，打出来的AssetBundle文件是一致的*/
            //foreach(var key in bbns.Keys)
            //{
            //    List<string> tl = bbns[key];
            //    tl.Sort((string a, string b)=> { return a.CompareTo(b); });

            //    string ct = "";
            //    for (int m = 0; m < tl.Count; m++) 
            //    {
            //        ct += tl[m] + "\n";
            //    }

            //    FileStream ws = File.Open(bundleDirPath + "/" + key + ResourceMgr.BundleResKeyExtName, FileMode.Create, FileAccess.Write);
            //    if (ws != null)
            //    {
            //        System.Text.UTF8Encoding encoding_utf8_withOutBOM = new System.Text.UTF8Encoding(false);
            //        byte[] fileBytes = encoding_utf8_withOutBOM.GetBytes(ct);
            //        ws.Write(fileBytes, 0, fileBytes.Length);

            //        ws.Flush(); ws.Close(); ws.Dispose();
            //    }
            //}

            //bbns.Clear();


            //开始进入unity打包阶段信息-
            AssetBundleBuild[] assetBundleBuilds = saveBundleSize.Count > 0 ? new AssetBundleBuild[saveBundleSize.Count] : new AssetBundleBuild[0];

            List<string>    inBundleFileName = new List<string>(64);
            string          bundleVarName = ResourceMgr.BundleResExtName.Replace(".", "");

            int     idx = 0;
            long    toSizeKbMb = 1024 * 1024;
            foreach (var bn in saveBundleSize.Keys)
            {
                inBundleFileName.Clear();

                foreach (AssetBundleBuild abb in assetBundleInfo)
                {
                    if(bn == abb.assetBundleName) { inBundleFileName.Add(abb.assetNames[0]); }
                }

                assetBundleBuilds[idx] = new AssetBundleBuild
                { assetBundleName = bn, assetBundleVariant = bundleVarName, assetNames = inBundleFileName.ToArray(), };

                //补充打印每个包体的原始大小-
                long    size = saveBundleSize[bn];
                double  mbSize = (double)size / toSizeKbMb;

                if (inBundleFileName.Count > 0 && mbSize < 0.01) { mbSize = 0.01; }
                assetBundleOutInfo.Add(new AssetBundleOutInfo {
                    infoHeadFileName = bundleName + ResourceMgr.BundleRelativeExtName, 
                    bundleName = bn + ResourceMgr.BundleResExtName, 
                    fileSizeMB = mbSize, nFileSize = size, nFiles = inBundleFileName.Count, });

                //-
                idx++;
            }

            inBundleFileName.Clear();


            //-
            assets.Clear();
            saveBundleSize.Clear();
            assetBundleInfo.Clear();
            oldAssetsRaltive.Clear();
            noFileBundle.Clear();



            //重新写入信息文件-
            FileStream writeStream = File.Open(oldAssetsRaltiveFilePath, FileMode.Create, FileAccess.Write);
            if (writeStream != null)
            {
                System.Text.UTF8Encoding encoding_utf8_withOutBOM = new System.Text.UTF8Encoding(false);
                byte[] fileBytes = encoding_utf8_withOutBOM.GetBytes(content);
                writeStream.Write(fileBytes, 0, fileBytes.Length);

                writeStream.Flush(); writeStream.Close(); writeStream.Dispose();
            }


            return assetBundleBuilds;
        }


        //从旧信息中提取一类bundle包中,最近一个产生的bundle包名-
        //如果有断序号的，就接在这个断掉的序号这里-
        private static string GetLastBundleInTheirSort(Dictionary<string, long> oldAssetsRaltive, string sortName)
        {
            int maxNumber = 1;
            int idx = 1;
            foreach(var bundleName in oldAssetsRaltive.Keys)
            {
                string[] bundleNameInfo = bundleName.Split('_');

                //找到断档的序号并给出-
                if (bundleNameInfo.Length > 1)
                {
                    int nextId = int.Parse(bundleNameInfo.Length > 2 ? bundleNameInfo[bundleNameInfo.Length - 1] : bundleNameInfo[1]);
                    if(nextId != idx)
                    {
                        maxNumber = idx;
                        break;
                    }
                }

                //直接找最后的尾巴序号-
                if (bundleName.Contains(sortName))
                {
                    if (bundleNameInfo.Length > 1)
                    {
                        int number = 0;
                        int.TryParse(bundleNameInfo[bundleNameInfo.Length -1], out number);
                        if (number > maxNumber) { maxNumber = number; }

                        if (oldAssetsRaltive[bundleName] <= BUNDLE_SIZE) { break; }
                    }
                }

                idx++;
            }

            return sortName +"_" + maxNumber;
        }



        //打包字场景目录内容-
        private static string[] BundleScene(string readyDir,
                                            string bundleDirPath,
                                            BuildTarget forSystemPlatform)
        {
            string[]        extfileNames = Directory.GetFiles(readyDir, "*.unity", SearchOption.AllDirectories);
            string[]        existFileInPag = Directory.GetFiles(bundleDirPath + "/", "*" + ResourceMgr.BundleSceneExtName, SearchOption.AllDirectories);
            List<string>    invaildSceneList = new List<string>();
            Dictionary<string, string>   buildedSceneNames = new Dictionary<string, string>();

            foreach (var filePathName in extfileNames)
            {
                string sceneName = Path.GetFileName(filePathName);

                //跳过已经在buildsetting中添加过的场景不打包-
                bool needBag = true;
                foreach(var scene in EditorBuildSettings.scenes)
                { if (scene.path == filePathName) { needBag = false; } }

                if (!needBag) { continue; }

                //-
                string fileName = Path.GetFileNameWithoutExtension(filePathName);
                string outPath = string.Format("{0}/{1}{2}", bundleDirPath, fileName, ResourceMgr.BundleSceneExtName);

                buildedSceneNames.Add(fileName, filePathName);
                BuildPipeline.BuildPlayer(new string[] { filePathName }, outPath, forSystemPlatform, BuildOptions.BuildAdditionalStreamedScenes);
            }

            //找出
            foreach (var pagScenePath in existFileInPag)
            {
                string pagSceneName = Path.GetFileNameWithoutExtension(pagScenePath);
                if (!buildedSceneNames.ContainsKey(pagSceneName)) { invaildSceneList.Add(pagSceneName + ResourceMgr.BundleSceneExtName); }
            }

            //-
            string[] invaildSceneNames = invaildSceneList.ToArray();
            invaildSceneList.Clear();
            buildedSceneNames.Clear();

            return invaildSceneNames;
        }


        /// <summary>
        /// 这个方法主要是写入场景文件的依赖信息，来让加载场景之前能够将依赖的包先行加载-
        /// </summary>
        /// <param name="bundleDirPath"></param>
        /// <param name="assetBundleInfo"></param>
        static void MotherSceneDependenceBuild( string bundleDirPath,
                                                List<AssetBundleBuild> assetBundleInfo)
        {
            //整理出在母包场景文件的依赖-
            Dictionary<string, string> saveDependenceBundleName = new Dictionary<string, string>(0);
            EditorBuildSettingsScene[] sceneSettings = EditorBuildSettings.scenes;
            string infoContent = "";
            foreach (var set in sceneSettings)
            {
                saveDependenceBundleName.Clear();

                string[] files = AssetDatabase.GetDependencies(set.path);
                string sceneFileName = Path.GetFileNameWithoutExtension(set.path);

                infoContent += sceneFileName + ":\n";

                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains(".cs")) { continue; }
                    if (files[i].Equals(set.path)) { continue; }

                    //这里比对文件名和bundle包的第一个资源名来确定依赖关系-
                    string fileName = Path.GetFileName(files[i]);
                    foreach (AssetBundleBuild abb in assetBundleInfo)
                    {
                        if (abb.assetNames.Length < 1) { continue; }

                        string inFileName = Path.GetFileName(abb.assetNames[0]);
                        if (inFileName == fileName)
                        {
                            string bundleName = abb.assetBundleName + ResourceMgr.BundleResExtName;
                            if (!saveDependenceBundleName.ContainsKey(bundleName))
                            {
                                saveDependenceBundleName.Add(bundleName, bundleName);
                                infoContent += bundleName + "\n";
                            }
                            break;
                        }
                    }
                }
            }


            //重新写入信息文件-
            string sceneDepdFilePathName = bundleDirPath + "/" + ResourceMgr.SceneDependenceInfoFileName;
            FileStream writeStream = File.Open(sceneDepdFilePathName, FileMode.Create);
            if (writeStream != null)
            {
                System.Text.UTF8Encoding encoding_utf8_withOutBOM = new System.Text.UTF8Encoding(false);
                byte[] fileBytes = encoding_utf8_withOutBOM.GetBytes(infoContent);
                writeStream.Write(fileBytes, 0, fileBytes.Length);

                writeStream.Flush();
                writeStream.Close();
                writeStream.Dispose();
            }
        }

        #endregion


        #region 测试打包功能-
        //[MenuItem("打包与版本发布/TestBundle")]
        static void MyBundleTest()
        {
            string outPath = Application.dataPath + "\\../BundlePag/";

            string[] assetBundleNames = { //"RoleRes",                      
                                          //"RolePrefab",
                                          //"BattleScene",
                                          "TempDataPrefab" ,
                                          //"TempDataOther",
                                          //"TempDataMat",
                                          //"TempDataShader",
                                          //"Table",
                                          /*"Scenes" ,*/};
            string[] assetNames = { //"Assets/Data/Role",              //注意!:这里子目录(Assets/Data/Role)打完bundle后，主目录(Assets/Data)再覆盖打bundle就不会包含已经打完的子目录了-
                                    //"Assets/Data/RolePrefab",
                                    //"Assets/BattleScene",
                                    "Assets/TempData/prefab/Npc37_t1.prefab",
                                    //"Assets/TempData/other",
                                    //"Assets/TempData/mat",
                                    //"Assets/TempData/shader",
                                    //"Assets/Table",
                                    /*"Assets/Scenes" ,*/};

            AssetBundleBuild[] bbs = new AssetBundleBuild[assetBundleNames.Length];

            for (int i = 0; i < bbs.Length; i++)
            {
                bbs[i].assetBundleName = assetBundleNames[i];
                bbs[i].assetBundleVariant = "bbs";
                bbs[i].assetNames = new string[] { assetNames[i], };
            }

            BuildPipeline.BuildAssetBundles(outPath, bbs, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree, BuildTarget.StandaloneWindows64);
        }

        //打包unity场景文件
        //[MenuItem("打包与版本发布/打包场景Bundle文件")]
        static void MyBuild()
        {
            string outPathRoot = Application.dataPath + "\\../BundlePag/";

            // 需要打包的场景名字-
            string outPath = outPathRoot + "TestAA1.scene";
            var files = Directory.GetFiles("Assets/Scenes/", "TestAA1.unity", SearchOption.AllDirectories);
        
            var error =BuildPipeline.BuildPlayer(files, outPath, BuildTarget.StandaloneWindows64, BuildOptions.BuildAdditionalStreamedScenes);
            BuildSummary summary = error.summary;
            if (summary.result == BuildResult.Succeeded)
            { Debug.Log("Build succeeded: " + summary.totalSize + " bytes"); }
            else
            { Debug.Log("Build failed"); }


            outPath = outPathRoot + "SampleScene.scene";
            files = Directory.GetFiles("Assets/Scenes/", "SampleScene.unity", SearchOption.AllDirectories);

            error = BuildPipeline.BuildPlayer(files, outPath, BuildTarget.StandaloneWindows64, BuildOptions.BuildAdditionalStreamedScenes);
            summary = error.summary;
            if (summary.result == BuildResult.Succeeded)
            { Debug.Log("Build succeeded: " + summary.totalSize + " bytes"); }
            else
            { Debug.Log("Build failed"); }

            outPath = outPathRoot + "TestAA1_Sub_1.scene";
            files = Directory.GetFiles("Assets/Scenes/TestAA1/", "TestAA1_Sub_1.unity", SearchOption.AllDirectories);
            BuildPipeline.BuildPlayer(files, outPath, BuildTarget.StandaloneWindows64, BuildOptions.BuildAdditionalStreamedScenes);

            outPath = outPathRoot + "TestAA1_Sub_2.scene";
            files = Directory.GetFiles("Assets/Scenes/TestAA1/", "TestAA1_Sub_2.unity", SearchOption.AllDirectories);
            BuildPipeline.BuildPlayer(files, outPath, BuildTarget.StandaloneWindows64, BuildOptions.BuildAdditionalStreamedScenes);

            outPath = outPathRoot + "TestAA1_Sub_3.scene";
            files = Directory.GetFiles("Assets/Scenes/TestAA1/", "TestAA1_Sub_3.unity", SearchOption.AllDirectories);
            BuildPipeline.BuildPlayer(files, outPath, BuildTarget.StandaloneWindows64, BuildOptions.BuildAdditionalStreamedScenes);

            // 刷新，可以直接在Unity工程中看见打包后的文件-
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
