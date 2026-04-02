namespace CLIP.Framework_Unity
{
    /// <summary>
    /// 不继承Mono的单例基类
    /// 单例生成时会，同步生成管理器，管理器继承Mono
    /// 管理器销毁时，会同步销毁单例
    /// </summary>
    /// <typeparam name="T">子类</typeparam>
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
    {
        private static readonly object lockObj = new();
        private static volatile T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                            // 注册销毁回调
                            SingletonManager.Instance.RegisterSingleton(instance);
                        }
                    }
                }
                return instance;
            }
        }

        public virtual string Name => typeof(T).Name;

        public static bool IsExist => instance != null;

        public virtual void Dispose()
        {
            lock (lockObj)
            {
                if (instance != null)
                {
                    instance = null;
                }
            }
        }
    }
}