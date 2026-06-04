using System.Threading.Tasks;
using UnityEngine;

namespace CLIP.Framework_Unity.Asset
{
    public class ResourceAssetProvider : IAssetProvider
    {
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            path = NormalizeResourcesPath(path);
            return Resources.Load<T>(path);
        }

        public async Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object
        {
            path = NormalizeResourcesPath(path);
            ResourceRequest request = Resources.LoadAsync<T>(path);
            while (!request.isDone)
                await Task.Yield();
            return request.asset as T;
        }

        public async Task<T[]> LoadAllAsync<T>(string path) where T : UnityEngine.Object
        {
            path = NormalizeResourcesPath(path);
            var result = Resources.LoadAll<T>(path);
                await Task.Yield();
            return result;
        }

        private static string NormalizeResourcesPath(string path)
        {
            return string.IsNullOrEmpty(path) ? path : path.Replace('\\', '/');
        }

        public void Unload(Object asset)
        {
            if (asset != null)
                Resources.UnloadAsset(asset);
        }
    }
}
