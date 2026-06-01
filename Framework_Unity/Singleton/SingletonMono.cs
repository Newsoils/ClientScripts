using Sirenix.OdinInspector;
using UnityEngine;

namespace CLIP.Framework_Unity
{
    /// <summary>
    /// Mono 单例基类：唯一静态 <see cref="Instance"/>、Awake 内判重销毁副本、<see cref="OnDestroy"/> 仅在本体销毁时清空引用。
    /// <para/>不在 getter 内自动 new 物体（避免与场景真实实例抢注册）；需要时用 <see cref="ForceReplaceInstance"/> 把静态引用指回场景权威组件。
    /// <para/>跨场景常驻：子类重写 <see cref="PersistAcrossScenes"/> 为 true，会在 <see cref="Awake"/> 内对本物体调用 <c>DontDestroyOnLoad</c>。
    /// </summary>
    /// <typeparam name="T">子类类型</typeparam>
    public class SingletonMono<T> : SerializedMonoBehaviour, ISingleton where T : SerializedMonoBehaviour
    {
        private static T instance;

        /// <summary>
        /// 为 true 时在本物体成为唯一实例后执行 <see cref="UnityEngine.Object.DontDestroyOnLoad(UnityEngine.Object)"/>，切换场景不卸载。
        /// 默认 false，避免无意让所有单例常驻；需要常驻的子类重写为 true。
        /// </summary>
        protected virtual bool PersistAcrossScenes => false;

        /// <summary>
        /// 将静态单例引用指向指定实例（场景里「权威」物体上的组件）。用于纠正过早访问 <see cref="Instance"/> 或其它顺序问题导致的错位。
        /// </summary>
        public static void ForceReplaceInstance(T authoritative)
        {
            if (authoritative == null)
                return;
            instance = authoritative;
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<T>(true);
                return instance;
            }
        }

        public string Name => typeof(T).Name;



        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                // 本实例是第二份及以后：本帧末卸载体，Unity 不会对其执行 Start；断点若打在这里会进，但 Start 永不到。
                Debug.LogWarning(
                    $"[SingletonMono] 重复 {typeof(T).Name} 已忽略并销毁本物体：「{gameObject.name}」。已存在实例在「{instance.name}」上，仅那一份会跑 Start。");
                Destroy(gameObject);
                return;
            }

            instance = this as T;

            if (PersistAcrossScenes)
                DontDestroyOnLoad(gameObject);

            SingletonManager.Instance?.RegisterSingleton(this);
        }
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            // 仅当销毁的是「当前登记的单例本体」时才清空；否则会误伤：Awake 里 Destroy(duplicate) 时若无条件 instance=null，
            // 会把仍存在的真正单例从静态引用摘掉 → 下次 Instance 可能 Find 失败并 new 空对象（例如 _current_cat_info 丢引用）。
            if (instance != null && ReferenceEquals(instance, this))
                instance = null;
        }
    }
}