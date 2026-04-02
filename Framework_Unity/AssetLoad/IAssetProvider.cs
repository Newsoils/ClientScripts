using System.Threading.Tasks;

namespace CLIP.Framework_Unity.Asset
{
    public interface IAssetProvider
    {
        T Load<T>(string path) where T : UnityEngine.Object;
        Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object;

        Task<T[]> LoadAllAsync<T>(string path) where T : UnityEngine.Object;
        void Unload(UnityEngine.Object asset);
    }
}
