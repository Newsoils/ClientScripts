using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Framework_Unity
{
    /// <summary>
    /// 对象池
    /// 仅提供对象的获取与释放方法
    /// 需要自己维护生成和释放的生命周期
    /// </summary>
    /// <summary>
    /// 简单对象池
    /// 提供获取、释放、预热功能
    /// </summary>
    public class ObjectPool : SingletonMono<ObjectPool>
    {
        private Dictionary<GameObject, Stack<GameObject>> pools = new();

        public GameObject GetObj(GameObject prefab, Transform parent = null)
        {
            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<GameObject>();
                pools[prefab] = pool;
            }

            GameObject obj;

            if (pool.Count > 0)
            {
                obj = pool.Pop();
            }
            else
            {
                obj = Instantiate(prefab);

                // 自动挂 PoolItem
                var item = obj.GetComponent<PoolItem>();
                if (item == null)
                    item = obj.AddComponent<PoolItem>();

                item.prefab = prefab;
            }

            // 设置父节点再激活（更安全）
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);

            return obj;
        }

        public void ReleaseObj(GameObject obj)
        {
            if (obj == null) return;

            var item = obj.GetComponent<PoolItem>();
            if (item == null || item.prefab == null)
            {
                Debug.LogError($"对象 {obj.name} 没有PoolItem或prefab引用，无法回收到对象池");
                return;
            }

            GameObject prefab = item.prefab;

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<GameObject>();
                pools[prefab] = pool;
            }

            obj.SetActive(false);
            obj.transform.SetParent(transform, false);

            pool.Push(obj);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Stack<GameObject>();
                pools[prefab] = pool;
            }

            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab);

                var item = obj.GetComponent<PoolItem>();
                if (item == null)
                    item = obj.AddComponent<PoolItem>();

                item.prefab = prefab;

                obj.SetActive(false);
                obj.transform.SetParent(transform, false);

                pool.Push(obj);
            }
        }

        public void ClearPool(GameObject prefab)
        {
            if (pools.TryGetValue(prefab, out var pool))
            {
                while (pool.Count > 0)
                {
                    Destroy(pool.Pop());
                }
            }
        }

        public void ClearAllPool()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    Destroy(pool.Pop());
                }
            }

            pools.Clear();
        }
    }
}