using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 资源/打包路径以及后缀名定义
/// </summary>
public static class AssetPathDefine 
{

    // 打包出的信息自定义格式-
    public const string AppDirName = "Bin";                         //发布的App目录-
    public const string BundleActionName = "BundlePag";             //包体输出到目录名-
    public const string BundleResExtName = ".CLS";                  //包格文件式-

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
                                                                                  };


    //编辑器Resource访问模式下的搜索目录集-
    static public string[] RES_FIND_PATHS = new string[] {
                                                    "ReadyBundle/Prefab/",
                                                    "ReadyBundle/Art/",
                                                    "ReadyBundle/DB/|chinese",                //一种分支格式,|后半部分描述在资源名相同的情况下默认采样哪个分支-
                                                    "Setting/",
                                                };

}
