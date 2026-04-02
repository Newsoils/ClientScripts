using UnityEngine;

namespace CLIP.Framework_Unity
{
    /// <summary>
    /// 继承Mono的单例基类
    /// </summary>
    /// <typeparam name="T">子类</typeparam>
    public class SingletonMono<T> : MonoBehaviour, ISingleton where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        public string Name => typeof(T).Name;



        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this as T;

            // ✅ 自动注册
            SingletonManager.Instance?.RegisterSingleton(this);
        }
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            instance = null;
        }
    }
}