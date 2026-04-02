using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Framework_Unity
{
    /// <summary>
    /// 管理游戏运行时不继承Mono的单例
    /// 当管理器需要销毁时，将这些单例释放
    /// </summary>
    public class SingletonManager : SingletonMono<SingletonManager>
    {
        private readonly List<ISingleton> _singletonList = new();
        public IReadOnlyList<ISingleton> GetAllSingletons() => _singletonList;


        /// <summary>
        /// 按类型获取单例
        /// </summary>
        public T GetSingleton<T>() where T : class, ISingleton
        {
            foreach (var s in _singletonList)
                if (s is T t) return t;
            return null;
        }

        /// <summary>
        /// 注册单例引用
        /// </summary>
        public void RegisterSingleton(ISingleton singleton)
        {
            if (!_singletonList.Contains(singleton))
            {
                _singletonList.Add(singleton);
                //Log.Info($"[SingletonManager] 注册单例: {singleton.Name}");
            }
        }


        protected override void OnDestroy()
        {
            //Log.Info("<color=orange>[SingletonManager] 销毁所有单例...</color>");
            foreach (var s in _singletonList)
            {
                try { s.Dispose(); }
                catch (Exception e) { Log.Error($"销毁 {s.Name} 时出错: {e}"); }
            }
            _singletonList.Clear();
            base.OnDestroy();
        }

    }
}