using System;
using System.Collections.Generic;
using System.IO;
using CLIP.Framework_Core.Tools;
using GameCoreResourceLoad;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static GameCoreResourceLoad.ResourceMgr;

namespace GameCoreBuild
{
    //发布程序过程事件处理-
    public class BuildPlayerPreprocess : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public static void ClearPostprocess()
        {
            string streamAssetsBundlePath = Application.streamingAssetsPath + "/" + ResourceMgr.BundleActionName;
            if (Directory.Exists(streamAssetsBundlePath))
            {
                FileUtil.DeleteFileOrDirectory(streamAssetsBundlePath);
                FileUtil.DeleteFileOrDirectory(streamAssetsBundlePath + ".meta");
            }

            switch (BJBuildPlayerEditor.CurBundleAccess)
            {
                case BundleAccessType.EditorAssets: BJCreateAssetsBundle.SwitchBa0(); break;
                case BundleAccessType.StreamAssets: BJCreateAssetsBundle.SwitchBa1(); break;
                case BundleAccessType.HotpatchAssets: BJCreateAssetsBundle.SwitchBa2(); break;
                default:
                case BundleAccessType.EditorAssetDatabase: BJCreateAssetsBundle.SwitchBa3(); break;
            }
        }

        //发布完成时-
        public void OnPostprocessBuild(BuildReport report)
        {
            BuildPlayerPreprocess.ClearPostprocess();
        }


        //发布前-
        public void OnPreprocessBuild(BuildReport report)
        {
            new BuildPlayerPreprocess().StartPreprocess();
        }


        //-
        private void StartPreprocess()
        {
            if(BJBuildPlayerEditor.IsBuildPlayerDoing)
            {
                string bundlePath = Application.dataPath + "/../" + ResourceMgr.BundleActionName;
                string streamBundlePath = Application.streamingAssetsPath + "/" + ResourceMgr.BundleActionName;

                if (!Directory.Exists(bundlePath)) { Directory.CreateDirectory(bundlePath); }

                if (Directory.Exists(streamBundlePath)) 
                {
                    FileUtil.DeleteFileOrDirectory(streamBundlePath);
                    FileUtil.DeleteFileOrDirectory(streamBundlePath + ".meta");
                }

                
                if (BJBuildPlayerEditor.BundleAccess == BundleAccessType.StreamAssets)
                {
                    string[] existFile = Directory.GetFiles(bundlePath, "*.*", SearchOption.TopDirectoryOnly);
                    foreach (string pathName2 in existFile)
                    {
                        if (pathName2.ToLower().Contains("playmore-")) { continue; }
                        //if (pathName2.ToLower().Contains(ResourceMgr.BundleResKeyExtName)) { continue; }
                        if (pathName2.Contains(".manifest") && !pathName2.Contains(ResourceMgr.BundleActionName)) { continue; }

                        Core_Tools.CopyFolder(pathName2, streamBundlePath);
                    }

                    BJCreateAssetsBundle.SwitchBa1();
                }
                else if (BJBuildPlayerEditor.BundleAccess == BundleAccessType.HotpatchAssets) { BJCreateAssetsBundle.SwitchBa2(); }
            }
        }
    }


    //-
    public class BJBuildPlayerEditor : Editor
    {
        public static bool  IsBuildPlayerDoing = false;              //这里很坑，加这个标记是为了区分打包和发布应用程序的区别，unity编辑器会统一共用一个回调时间-
        public static ResourceMgr.BundleAccessType BundleAccess = ResourceMgr.BundleAccessType.EditorAssets;
        public static ResourceMgr.BundleAccessType CurBundleAccess = ResourceMgr.BundleAccessType.EditorAssets;

        private static string GetTimeForNow()
        {
            return DateTime.Now.ToString("yyyyMMddHHmm");
        }


        [MenuItem(BJCreateAssetsBundle.PackTolMenuName + "应用程序发布(内含包体)", false, -7)]
        public static void BuildPlayerStreamAssets()
        {
            _BuildPlayerInner(ResourceMgr.BundleAccessType.StreamAssets);
        }

        [MenuItem(BJCreateAssetsBundle.PackTolMenuName + "应用程序发布(受热更)", false, -6)]
        public static void BuildPlayerHotpatchAssets()
        {
            _BuildPlayerInner(ResourceMgr.BundleAccessType.HotpatchAssets);
        }



        private static void _BuildPlayerInner(ResourceMgr.BundleAccessType bpSort)
        {
            BundleAccess = bpSort;
            CurBundleAccess = (BundleAccessType)BJCreateAssetsBundle.GetCurrentBundleAccess();

            //解析发布名字-
            var BPTargetName = PlayerSettings.productName;

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    BPTargetName += (EditorUserBuildSettings.buildAppBundle ? ".aab" : ".APK");
                    break;

                case BuildTarget.StandaloneOSX:
                    BPTargetName += ".app";
                    break;

                default:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    BPTargetName += ".exe";
                    break;
            }


            //发布到的缓存路径-
            string appPath = ResourceMgr.BundleDestDir + ResourceMgr.AppDirName;
            if (!Directory.Exists(appPath)) { Directory.CreateDirectory(appPath); }

            appPath += "/" + PlayerSettings.productName + "_" + GetTimeForNow();

            //加载默认场景-
            var levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes) { if (scene.enabled) { levels.Add(scene.path); } }

            if (levels.Count == 0)
            {
                Debug.Log("<color=red>BuildSettings里缺少一个默认启动的场景</color>");
                return;
            }

            //发布其它配置-
            var options = new BuildPlayerOptions
            {
                scenes = levels.ToArray(),
                locationPathName = $"{appPath}/{BPTargetName}",
                target = EditorUserBuildSettings.activeBuildTarget,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };

            //-
            IsBuildPlayerDoing = true;
            try
            {
                BuildPipeline.BuildPlayer(options);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                BuildPlayerPreprocess.ClearPostprocess();
            }
            
            EditorUtility.OpenWithDefaultApp(appPath);
            IsBuildPlayerDoing = false;
        }
    }
}
