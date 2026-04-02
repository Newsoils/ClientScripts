using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CLIP.Framework_Unity.Asset
{
    /// <summary>
    /// 资源管理器
    /// 负责加载和缓存游戏资源
    /// </summary>
    public class AssetLoader : SingletonMono<AssetLoader>
    {

        private IAssetProvider provider;
        private readonly Dictionary<string, Object> cache = new();

        public void Init(IAssetProvider customProvider = null)
        {
            provider = customProvider ?? new ResourceAssetProvider();
            Log.Sucess("[AssetManager] 初始化完成 → 当前加载器：" + provider.GetType().Name);
        }

        public T Load<T>(string path, bool cacheIt = true) where T : Object
        {
            if (cache.TryGetValue(path, out var cached))
                return cached as T;

            var asset = provider.Load<T>(path);
            if (cacheIt && asset != null)
                cache[path] = asset;

            return asset;
        }

        public async Task<T> LoadAsync<T>(string path, bool cacheIt = true) where T : Object
        {
            if (cache.TryGetValue(path, out var cached))
                return cached as T;

            var asset = await provider.LoadAsync<T>(path);
            if (cacheIt && asset != null)
                cache[path] = asset;

            return asset;
        }

        public async Task<T[]> LoadAllAsync<T>(string path, bool cacheIt = true) where T : Object
        {
            T[] assets = await provider.LoadAllAsync<T>(path);
            if (cacheIt)
            {
                foreach (var a in assets)
                {
                    string key = $"{path}/{a.name}";
                    if (!cache.ContainsKey(key))
                        cache[key] = a;
                }
            }
            return assets;
        }

        public void Unload(string path)
        {
            if (cache.TryGetValue(path, out var asset))
            {
                provider.Unload(asset);
                cache.Remove(path);
            }
        }

        public void ClearCache()
        {
            foreach (var kv in cache)
            {
                provider.Unload(kv.Value);
            }
            cache.Clear();
        }
    }
}
