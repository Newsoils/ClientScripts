using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Core.Tools;
using UnityEngine;

namespace CLIP.Framework_Unity.Asset
{
    public class ResourceIndexRuntime
    {
        public static Dictionary<string, ResourceItem> table;

        public static void Init()
        {
            //var json = File.ReadAllText(
            //    Path.Combine(Application.streamingAssetsPath, "resource_index.json"));

            var json = Resources.Load<TextAsset>("Config/resource_index");

            var data = Serialization_Provider.DeserializeObject<ResourceIndexData>(json.text);

            table = new Dictionary<string, ResourceItem>();
            foreach (var e in data.Items)
                table[e.key] = e;
        }

        public static string GetPath(string key)
        {
            return table.TryGetValue(key, out var e) ? e.path : null;
        }
    }

    /// <summary>
    /// 游戏全局资源中心（Prefab、Icon、Material等）
    /// 所有资源路径集中管理，方便热更或路径调整
    /// </summary>
    public class GameAssets : SingletonMono<GameAssets>
    {
        // === Prefabs ===
        public GameObject mainCharacter_Dispatch;

        // === 图标 / 材质 / 其他资源 ===
        public Material funiture_Border_Mat;


        public readonly List<string> assetKeys = new List<string>();

        /// <summary>
        /// key -> asset
        /// </summary>
        public readonly Dictionary<string, UnityEngine.Object> allAssets = new();

        /// <summary>
        /// key -> resource path
        /// </summary>
        public readonly Dictionary<string, string> keyToPath = new();


        /// <summary>
        /// 当前加载进度（0 ~ 100）
        /// </summary>
        private int currentProgress = 0;


        public async Task InitAsync()
        {
            Log.Info("[GameAssets] 开始加载全局资源...");

            ResourceIndexRuntime.Init();

            funiture_Border_Mat = await LoadAsyncByPath<Material>("Materials/Funiture_Border_BarberPoleBorder");

            mainCharacter_Dispatch = await LoadAsyncByPath<GameObject>("Prefabs/Character/MainCharacter_Dispatch");

            // 1️⃣ 建立 Key -> Path 映射
            foreach (var kv in ResourceIndexRuntime.table)
            {
                assetKeys.Add(kv.Key);
                string key = kv.Key;
                string path = kv.Value.path;

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(path))
                {
                    Log.Warn($"[GameAssets] Invalid entry: key={key}, path={path}");
                    continue;
                }

                keyToPath[key] = path;
            }

            Log.Info("[GameAssets] 全局资源加载完成，共 " + allAssets.Count + " 个。");
        }


        /// <summary>
        /// 通过逻辑 Key 加载（同步）
        /// </summary>
        public T Load<T>(string key) where T : UnityEngine.Object
        {
            if (allAssets.TryGetValue(key, out var asset))
            {
                return asset as T;
            }

            if (!keyToPath.TryGetValue(key, out var path))
            {
                Log.Error($"[GameAssets] Key not found: {key}");
                return null;
            }

            asset = AssetLoader.Instance.Load<T>(path);
            allAssets[key] = asset;
            return asset as T;
        }

        /// <summary>
        /// 通过逻辑 Key 加载（异步）
        /// </summary>
        private async Task<T> LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
        {
            if (allAssets.TryGetValue(key, out var asset))
            {
                callback?.Invoke(asset as T);
                return asset as T;
            }

            if (!keyToPath.TryGetValue(key, out var path))
            {
                Log.Warn($"[GameAssets] Key not found: {key}");
                return null;
            }

            asset = await AssetLoader.Instance.LoadAsync<T>(path);

            // 只有加载成功，且类型匹配时才存入缓存
            if (asset is T typedAsset)
            {
                allAssets[key] = typedAsset;
                callback?.Invoke(typedAsset);
                return typedAsset;
            }
            else
            {
                Log.Warn($"[GameAssets] 资源类型不匹配或加载失败: Key={key}, Expected={typeof(T).Name}");
                callback?.Invoke(null);
                return null;
            }
        }



        public async Task<T> LoadAsycByKey<T>(string key) where T : UnityEngine.Object
        {
            if (!keyToPath.TryGetValue(key, out var path))
            {
                Log.Error($"[GameAssets] Key not found: {key}");
                return null;
            }
            var asset = await AssetLoader.Instance.LoadAsync<T>(path);
            return asset as T;
        }

        public static async Task<T> LoadAsyncByPath<T>(string path, Action<T> callback = null) where T : UnityEngine.Object
        {
            var asset = await AssetLoader.Instance.LoadAsync<T>(path);

            callback?.Invoke(asset);

            return asset as T;
        }

        public static async Task<T[]> LoadAllAsyncByPath<T>(string path) where T : UnityEngine.Object
        {
            var assets = await AssetLoader.Instance.LoadAllAsync<T>(path);
            return assets;
        }

        public void ReleaseAll()
        {
            foreach (var kv in allAssets)
            {
                if (keyToPath.TryGetValue(kv.Key, out var path))
                {
                    AssetLoader.Instance.Unload(path);
                }
            }

            assetKeys.Clear();
            allAssets.Clear();
            keyToPath.Clear();
        }



        public Task<T> GetAssetByKeyword<T>(string key, Action<T> onLoaded = null) where T : UnityEngine.Object
        {
            return GetAssetByKeyword_Inner<T>(onLoaded, new string[] { key });
        }

        public Task<T> GetAssetByKeyword<T>(string key1, string key2, Action<T> onLoaded = null) where T : UnityEngine.Object
        {
            return GetAssetByKeyword_Inner<T>(onLoaded, new string[] { key1, key2 });
        }


        public Task<T> GetAssetByKeyword<T>(params string[] keywords) where T : UnityEngine.Object
        {
            return GetAssetByKeyword_Inner<T>(null, keywords);
        }

        public Task<T> GetAssetByKeyword<T>(Action<T> onLoaded, params string[] keywords) where T : UnityEngine.Object
        {
            return GetAssetByKeyword_Inner<T>(onLoaded, keywords);
        }

        public void LoadAndSet<T>(string key, Action<T> onLoaded) where T : UnityEngine.Object
        {
            _ = LoadAsync<T>(key, onLoaded);
        }

        private async Task<T> GetAssetByKeyword_Inner<T>(Action<T> onLoaded, string[] keywords) where T : UnityEngine.Object
        {
            var key = assetKeys.FirstOrDefault(k =>
                keywords.All(kw => k.Contains(kw))
            );
            if (string.IsNullOrEmpty(key))
            {
                string keywordsStr = string.Join(", ", keywords);
                Log.Info($"未找到资源：{keywordsStr}");
                onLoaded?.Invoke(null);
                return null;
            }

            T asset;

            asset = await LoadAsync<T>(key);
            onLoaded?.Invoke(asset);

            return asset;
        }

        public async Task<List<T>> GetAssetsByKeywordAsync<T>(Action<List<T>> onLoaded = null, params string[] keywords) where T : UnityEngine.Object
        {
            List<T> assets = new();

            var keys = assetKeys
                .Where(k => keywords.All(kw => k.Contains(kw)))
                .ToList();

            var tasks = keys.Select(key => LoadAsync<T>(key)).ToList();
            T[] results = await Task.WhenAll(tasks);
            assets = results.ToList();
            onLoaded?.Invoke(assets);
            return assets;
        }

        public async Task<T[]> GetAssetsByKeywordAsync<T>(params string[] keywords) where T : UnityEngine.Object
        {
            T[] assets;

            var keys = assetKeys
                .Where(k => keywords.All(kw => k.Contains(kw)))
                .ToList();

            var tasks = keys.Select(key => LoadAsync<T>(key)).ToList();
            assets = await Task.WhenAll(tasks);

            return assets;
        }
        public static bool TryConvertFileNameToResKey(string fileName, string fileType, out string resKey)
        {
            string key = StringTools.ToSafeString(fileName);
            string type = StringTools.ToSafeString(fileType);
            resKey = type + "_" + key;
            if(Instance.keyToPath.TryGetValue(resKey, out var path))
            {
                return true;
            }
            return false;
        }
    }
}
