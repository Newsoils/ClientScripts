using System.Collections;
using System.IO;
using GameCoreResourceLoad;
using UnityEngine;
using UnityEngine.Networking;

public class TestBundleLoader : MonoBehaviour
{
    // 转成绝对路径
    private readonly string bundleRootPath = "D:\\UnityProject\\Project_Mouse\\Project_Mouse_3D_URP\\BundlePag";

    IEnumerator Start()
    {
        Debug.Log("== 开始测试 AssetBundle 加载 ==");

        if (string.IsNullOrEmpty(bundleRootPath) || !Directory.Exists(bundleRootPath))
        {
            Debug.LogError("Bundle 路径不存在: " + bundleRootPath);
            yield break;
        }

        // 1. 加载主 manifest
        string manifestPath = Path.Combine(bundleRootPath, "BundlePag.manifest");

        string bundlePagPath = Path.Combine(bundleRootPath, "BundlePag");
        var mainBundle = AssetBundle.LoadFromFile(bundlePagPath);

        if (mainBundle == null)
        {
            Debug.LogError("加载主 BundlePag 失败！");
            yield break;
        }

        var manifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        if (manifest == null)
        {
            Debug.LogError("加载 AssetBundleManifest 失败！");
            yield break;
        }

        Debug.Log("Manifest 加载成功，列出所有 bundles：");

        foreach (string bn in manifest.GetAllAssetBundles())
            Debug.Log("Bundle: " + bn);

        // 2. 按依赖顺序加载你的三个模块包
        yield return LoadBundleAndPrintAssets(bundleRootPath, "texturemodule_1.bbs");
        yield return LoadBundleAndPrintAssets(bundleRootPath, "urpmodule_1.bbs");
        yield return LoadBundleAndPrintAssets(bundleRootPath, "logicmodule_1.bbs");

        Debug.Log("== 测试完成 ==");
    }

    IEnumerator LoadBundleAndPrintAssets(string root, string bundleName)
    {
        string fullPath = Path.Combine(root, bundleName);
        Debug.Log($"加载 Bundle: {fullPath}");

        var bundle = AssetBundle.LoadFromFile(fullPath);

        if (bundle == null)
        {
            Debug.LogError("加载失败: " + fullPath);
            yield break;
        }

        Debug.Log($"加载成功: {bundleName}");

        // 列出所有 Asset
        string[] assetNames = bundle.GetAllAssetNames();
        foreach (var a in assetNames)
            Debug.Log($"   Asset: {a}");

        // 尝试加载 prefab
        foreach (string a in assetNames)
        {
            if (a.EndsWith(".prefab"))
            {
                var prefab = bundle.LoadAsset<GameObject>(a);
                if (prefab != null)
                {
                    Debug.Log($"实例化 prefab: {a}");
                    Instantiate(prefab);
                }
                else
                {
                    Debug.LogError($"Prefab 加载失败: {a}");
                }
            }
        }

        // 尝试加载贴图
        foreach (string a in assetNames)
        {
            if (a.EndsWith(".png") || a.EndsWith(".jpg") || a.Contains("texture"))
            {
                var tex = bundle.LoadAsset<Texture>(a);
                if (tex != null)
                    Debug.Log($"加载 Texture 成功: {a}");
                else
                    Debug.LogError($"Texture 加载失败: {a}");
            }
        }

        yield return null;
    }
}
