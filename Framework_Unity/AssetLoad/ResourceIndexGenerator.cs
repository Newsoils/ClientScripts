#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CLIP.Framework_Core.Tools;
using CLIP.Framework_Unity;
using UnityEditor;
using UnityEngine;

public  class ResourceIndexGenerator : EditorWindow
{
    private string scanRoot = "Assets/Resources/RuntimeAssets";
    private string outputSOPath = "Assets/GameConfig/Generated/ResourceIndex.asset";
    private string outputJsonPath = "Assets/Resources/Config/resource_index.json";
    private string resKeysPath = "Assets/Scripts/Game_Play_Systems/ResKeys.cs";


    [MenuItem("Tools/生成资源索引")]
    public static void ShowWindow()
    {
        GetWindow<ResourceIndexGenerator>("资源索引配置");
    }

    private void OnEnable()
    {
        // 加载之前保存的路径
        scanRoot = EditorPrefs.GetString("RIG_ScanRoot", scanRoot);
        outputJsonPath = EditorPrefs.GetString("RIG_JsonPath", outputJsonPath);
        resKeysPath = EditorPrefs.GetString("RIG_ResKeysPath", resKeysPath);
        outputSOPath = EditorPrefs.GetString("RIG_SOPath", outputSOPath);
    }

    private void OnGUI()
    {
        GUILayout.Label("路径设置", EditorStyles.boldLabel);

        scanRoot = EditorGUILayout.TextField("资源扫描根目录", scanRoot);
        outputJsonPath = EditorGUILayout.TextField("JSON 输出路径", outputJsonPath);
        resKeysPath = EditorGUILayout.TextField("ResKeys 脚本路径", resKeysPath);
        outputSOPath = EditorGUILayout.TextField("ScriptableObject 输出路径", outputSOPath);

        if (GUILayout.Button("保存配置", GUILayout.Height(30)))
        {
            EditorPrefs.SetString("RIG_ScanRoot", scanRoot);
            EditorPrefs.SetString("RIG_JsonPath", outputJsonPath);
            EditorPrefs.SetString("RIG_ResKeysPath", resKeysPath);
            EditorPrefs.SetString("RIG_SOPath", outputSOPath);
            Debug.Log("配置已保存");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("立即生成索引", GUILayout.Height(40)))
        {
            Generate();
        }
    }

    public void Generate()
    {
        var data = new ResourceIndexData();

        var files = Directory.GetFiles(scanRoot, "*.*", SearchOption.AllDirectories);
        var so = ScriptableObject.CreateInstance<ResourceIndexSO>();

        // 优化 1: 用于检测 Key 冲突的字典
        Dictionary<string, string> duplicateCheck = new Dictionary<string, string>();

        foreach (var file in files)
        {
            if (file.EndsWith(".meta")) continue;
            string ext = Path.GetExtension(file).ToLower();
            if (!IsSupported(ext)) continue;

            // 核心逻辑：普通资源处理
            AddEntry(data, file, ext, so, duplicateCheck);

            // 优化 2: 如果是 FBX，自动拆分子资源 (AnimationClip)
            //if (ext == ".fbx")
            //{
            //    ProcessFbxSubAssets(data, file, duplicateCheck);
            //}
        }

        // 写入 JSON
        Directory.CreateDirectory(Path.GetDirectoryName(outputJsonPath));
        File.WriteAllText(outputJsonPath, JsonUtility.ToJson(data, true));

        // 优化 3: 写入 C# ResKeys
        GenerateResKeysCode(data.Items);

        Directory.CreateDirectory(Path.GetDirectoryName(outputSOPath));
        AssetDatabase.CreateAsset(so, outputSOPath);

        AssetDatabase.Refresh();
        Log.Sucess($"[RIG] 索引生成成功！共 {data.Items.Count} 个资源项。");
    }

    private void AddEntry(ResourceIndexData data, string file, string ext, ResourceIndexSO so, Dictionary<string, string> check, string subName = "")
    {
        string rawKey = Path.GetFileNameWithoutExtension(file).ToLower();

        string safeKey = StringTools.ToSafeString(rawKey);

        //如果字符串不合法，跳过
        if (safeKey == "invalid" )
        {
            Log.Error( "文件名不合法，跳过：" + file);
            return;
        }
        // 如果是子资源，Key 格式为: 文件名_子资源名
        string finalKey = string.IsNullOrEmpty(subName) ? safeKey : $"{safeKey}_{subName}".ToLower();
        string assetPath = ToResourcePath(file);

        if (check.ContainsKey(finalKey))
        {
            Log.Error($"[ResourceIndexGenerator] Key 冲突！Key: {finalKey}\n路径A: {check[finalKey]}\n路径B: {file}");
            return;
        }

        check.Add(finalKey, file);
        var entry = new ResourceItem
        {
            key = $"{ext.TrimStart('.').ToUpper()}_{finalKey}",
            path = assetPath,
            //subAssetName = subName, // 新增字段，标记子资源名
            type = ext,
            bundleName = GuessBundle(file) // 这里根据你的旧脚本逻辑分配包名
        };
        data.Items.Add(entry);

        so.entries.Add(entry);
    }

    //private void ProcessFbxSubAssets(ResourceIndexData data, string file, Dictionary<string, string> check)
    //{
    //    // 只有在 Editor 下能这么干，获取 FBX 里的动画片段
    //    var assets = AssetDatabase.LoadAllAssetsAtPath(file.Replace("\\", "/"));
    //    foreach (var asset in assets)
    //    {
    //        if (asset is AnimationClip && !asset.name.StartsWith("__")) // 过滤 Unity 自动生成的无用动画
    //        {
    //            AddEntry(data, file, ".fbx", check, asset.name);
    //        }
    //    }
    //}

    // 优化 4: 抽离代码生成逻辑，让类名更整洁
    private void GenerateResKeysCode(List<ResourceItem> entries)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// 自动生成，请勿手动修改");
        sb.AppendLine("public static class ResKeys {");
        foreach (var entry in entries)
        {
            string varName = entry.key.ToUpper().Replace("-", "_").Replace(" ", "_").Replace(".", "_");
            sb.AppendLine($"public const string {varName} = \"{entry.key}\";");
        }
        sb.AppendLine("}");

        Directory.CreateDirectory(Path.GetDirectoryName(resKeysPath));
        File.WriteAllText(resKeysPath, sb.ToString());
    }

    private string ToResourcePath(string file)
    {
        string path = file.Replace("\\", "/");
        int idx = path.IndexOf("Resources/");
        if (idx == -1) return path; // 兼容非 Resources 路径
        return path.Substring(idx + "Resources/".Length).Replace(Path.GetExtension(path), "");
    }

    private string GuessBundle(string file)
    {
        // 这里的逻辑可以对应你 BJCreateAssetsBundle 里的 Module 分类
        if (file.Contains("/Texture/")) return "texturemodule";
        if (file.Contains("/Prefabs/")) return "logicmodule";
        return "default";
    }

    static bool IsSupported(string ext)
    {
        return ext == ".prefab" || ext == ".mat" || ext == ".png" || ext == ".asset" ||
                ext == ".prefab" || ext == ".mat" || ext == ".shader" || ext == ".hlsl" || ext == ".shadervariants" ||
                ext == ".png" || ext == ".jpg" || ext == ".dds" || ext == ".gif" || ext == ".psd" || ext == ".tga" ||
                ext == ".bmp" || ext == "exr" || ext == ".fbx" || ext == ".asset" || ext == ".anim" || ext == ".controller" ||
                ext == ".mesh" || ext == ".wav" || ext == ".mp3" || ext == ".ogg" || ext == ".ttf" || ext == ".otf" ||
                ext == ".txt" || ext == ".bytes" || ext == ".xml" || ext == ".csv" || ext == ".json" || ext == ".lua" ||
                ext == ".js" || ext == ".unity" || ext == ".scene" || ext == ".bbs" || ext == ".rltif";
    }
    
   
}
#endif
