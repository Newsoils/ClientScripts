using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Core.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CLIP.Framework_Unity.Asset
{
    public class ResourceIndexRuntime
    {
        public static Dictionary<string, ResourceItem> table;

        public static void Init()
        {
            var ta = Resources.Load<TextAsset>(GameAssetsPathDefine.ResourceIndexJsonResourcesPath);
            if (ta == null)
            {
                Debug.LogError($"[ResourceIndexRuntime] 找不到资源索引文件: {GameAssetsPathDefine.ResourceIndexJsonResourcesPath}，"
                    + "请确认 ResourceIndexGenerator 已生成索引。");
                return;
            }

            var data = Serialization_Provider.DeserializeObject<ResourceIndexData>(ta.text);
            table = new Dictionary<string, ResourceItem>();
            foreach (var e in data.Items)
                table[e.key] = e;
        }

        public static string GetPath(string key)
        {
            return table != null && table.TryGetValue(key, out var e) ? e.path : null;
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

        public readonly Dictionary<string, UnityEngine.Object> allAssets = new();

        public readonly Dictionary<string, string> keyToPath = new();


        /// 当前加载进度（0 ~ 100）


        public async Task InitAsync()
        {
            Log.Info("[GameAssets] 开始加载全局资源...");

            ResourceIndexRuntime.Init();

            funiture_Border_Mat = await LoadAsync<Material>("Materials/Funiture_Border_BarberPoleBorder");

            mainCharacter_Dispatch = await LoadAsync<GameObject>("Prefabs/Character/MainCharacter_Dispatch");

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
        public T Load<T>(string keyOrPath) where T : UnityEngine.Object
        {
            if (allAssets.TryGetValue(keyOrPath, out var asset))
            {
                return asset as T;
            }

            var path = ResolveKeyOrPath(keyOrPath);
            if (string.IsNullOrEmpty(path))
            {
                Log.Error($"[GameAssets] Invalid key or path: {keyOrPath}");
                return null;
            }

            asset = AssetLoader.Instance.Load<T>(path);
            if (asset != null)
                allAssets[keyOrPath] = asset;
            return asset as T;
        }

        /// <summary>
        /// 通过逻辑 Key 加载（异步）
        /// </summary>
        public async Task<T> LoadAsyncByKey<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
        {
            if (allAssets.TryGetValue(key, out var asset))
            {
                callback?.Invoke(asset as T);
                return asset as T;
            }

            var path = ResolveKey(key);
            if (string.IsNullOrEmpty(path))
            {
                Log.Warn($"[GameAssets] Key not found: {key}");
                callback?.Invoke(null);
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

        [Obsolete("Use LoadAsyncByKey instead.")]
        public Task<T> LoadAsycByKey<T>(string key) where T : UnityEngine.Object
        {
            return LoadAsyncByKey<T>(key);
        }

        public static async Task<T> LoadAsync<T>(string keyOrPath, Action<T> callback = null) where T : UnityEngine.Object
        {
            var path = ResolvePath(keyOrPath);
            var asset = await AssetLoader.Instance.LoadAsync<T>(path);
            callback?.Invoke(asset);
            return asset;
        }

        public static async Task<T[]> LoadAllAsync<T>(string keyOrPath) where T : UnityEngine.Object
        {
            var assets = await AssetLoader.Instance.LoadAllAsync<T>(ResolvePath(keyOrPath));
            return assets;
        }

        public static T LoadByPath<T>(string path) where T : UnityEngine.Object
        {
            return AssetLoader.Instance.Load<T>(path);
        }


        public static void LoadSprite(string keyOrPath, Action<Sprite> callback)
        {
            _ = LoadAsync(keyOrPath, callback);
        }

        public static void LoadSpriteAsync(string keyOrPath, Action<Sprite> callback = null)
        {
            _ = LoadAsync(keyOrPath, callback);
        }

        public static async Task<Sprite> LoadSubSpriteAsync(string imagePath, string spriteName, Action<Sprite> callback = null)
        {
            var sprites = await LoadAllAsync<Sprite>(imagePath);
            var sprite = sprites?.FirstOrDefault(s => s != null && s.name == spriteName);
            callback?.Invoke(sprite);
            return sprite;
        }

        public static void LoadSubSprite(string imagePath, string spriteName, Action<Sprite> callback = null)
        {
            _ = LoadSubSpriteAsync(imagePath, spriteName, callback);
        }

        public static void LoadSpriteByUrl(string url, Action<Sprite> callback = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                callback?.Invoke(null);
                return;
            }

            var parts = url.Split('#');
            if (parts.Length == 2)
                LoadSubSprite(parts[0], parts[1], callback);
            else
                LoadSpriteAsync(parts[0], callback);
        }

        public static async Task<Texture2D> LoadPngAsTextureAsync(string path, Action<Texture2D> callback = null)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("File not found at path: " + path);
                callback?.Invoke(null);
                return null;
            }

            byte[] fileData = await File.ReadAllBytesAsync(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            callback?.Invoke(texture);
            return texture;
        }

        public static void LoadSceneAsync(string path, Action callback = null)
        {
            LoadSceneAsync(path, _ => callback?.Invoke());
        }

        public static void LoadSceneAsync(string path, Action<Scene> callback)
        {
            var asyncLoader = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            asyncLoader.completed += _ =>
            {
                if (asyncLoader.isDone)
                {
                    Debug.Log("Scene loaded successfully: " + path);
                    callback?.Invoke(SceneManager.GetSceneByName(path));
                }
                else
                {
                    Debug.LogError("Failed to load scene: " + path);
                    callback?.Invoke(default);
                }
            };
        }

        public static void LoadMainCharacter(string mainCharacterInfoJson, Action<GameObject> callback)
        {
            _ = LoadAsync("Prefabs/Character/Main_Character", callback);
        }

        public static string ResolvePath(string keyOrPath)
        {
            if (Instance != null)
                return Instance.ResolveKeyOrPath(keyOrPath);

            if (ResourceIndexRuntime.table == null)
                ResourceIndexRuntime.Init();

            return ResourceIndexRuntime.GetPath(keyOrPath) ?? keyOrPath;
        }

        public void LoadAndSetByKey<T>(string key, Action<T> onLoaded) where T : UnityEngine.Object
        {
            _ = LoadAsyncByKey<T>(key, onLoaded);
        }


        public string ResolveKeyOrPath(string keyOrPath)
        {
            if (string.IsNullOrEmpty(keyOrPath))
                return keyOrPath;

            var path = ResolveKey(keyOrPath);
            return string.IsNullOrEmpty(path) ? keyOrPath : path;
        }

        private string ResolveKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (keyToPath.TryGetValue(key, out var path))
                return path;

            return ResourceIndexRuntime.GetPath(key);
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
