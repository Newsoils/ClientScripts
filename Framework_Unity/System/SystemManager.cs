using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Framework_Unity
{
    public class SystemManager : SingletonMono<SystemManager>
    {
        private readonly List<ISystem> _systems = new();
        private readonly Dictionary<string, ISystem> _systemMap = new();

        public void RegisterSystem<T>() where T : class, ISystem, new()
        {
            // 检查是否是 Mono 类型
            if (typeof(MonoBehaviour).IsAssignableFrom(typeof(T)))
            {
                // 创建 GameObject 并挂载
                var go = new GameObject(typeof(T).Name);
                DontDestroyOnLoad(go);

                var component = go.AddComponent(typeof(T)) as ISystem;
                InternalRegister(component);
            }
            else
            {
                var instance = new T();
                InternalRegister(instance);
            }
        }

        private void InternalRegister(ISystem system)
        {
            if (system == null) return;
            if (_systemMap.ContainsKey(system.Name))
            {
                Debug.LogWarning($"[SystemManager] 系统已存在: {system.Name}");
                return;
            }

            _systemMap.Add(system.Name, system);
            _systems.Add(system);
            system.OnInit();
        }

        private void Start()
        {
            foreach (var sys in _systems)
                sys.OnStart();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var sys in _systems)
                sys.OnUpdate(dt);
        }

        private void OnDestroy()
        {
            for (int i = _systems.Count - 1; i >= 0; i--)
                _systems[i].OnDestroy();
            _systems.Clear();
            _systemMap.Clear();
        }
    }
}
