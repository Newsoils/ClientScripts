using System;
using System.Collections.Generic;
using UnityEngine;
using CLIP.Framework_Unity;


namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 持久化对象管理器
    /// 统一管理在场景切换时不被销毁的 GameObject
    /// 配置通过 ScriptableObject（放在 Resources/PersistentObject/ 目录下）自动加载
    /// </summary>
    public class PersistentObjectManager :SingletonMono<PersistentObjectManager> 
    {

        /// <summary>
        /// 配置文件在 Resources 目录下的路径（不包含扩展名）
        /// </summary>
        private const string CONFIG_PATH = "Config/PersistentObjectConfig";

        private Dictionary<string, GameObject> _persistentObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, PersistentObjectConfig> _configs = new Dictionary<string, PersistentObjectConfig>();
        private bool _isLoaded = false;
        private bool _isInitialized = false;

        private PersistentObjectConfigSO _configSO;

        public IReadOnlyDictionary<string, GameObject> PersistentObjects => _persistentObjects;

     

        /// <summary>
        /// 初始化并加载所有持久化对象
        /// 由 GameBootstrap 在进入 MainScene 前调用
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[PersistentObjectManager] Already initialized.");
                return;
            }

            _isInitialized = true;
            LoadConfigAndInstantiate();
        }

        private void LoadConfigAndInstantiate()
        {
            // 优先使用 Inspector 指定的配置
            if (_configSO != null)
            {
                LoadConfigs(_configSO.PersistentPrefabs);
                return;
            }

            // 从 Resources 加载默认配置
            var configSO = Resources.Load<PersistentObjectConfigSO>(CONFIG_PATH);
            if (configSO != null)
            {
                LoadConfigs(configSO.PersistentPrefabs);
            }
            else
            {
                Debug.LogWarning("[PersistentObjectManager] No config found at Resources/PersistentObject/PersistentObjectConfig.asset");
            }
        }

        private void LoadConfigs(IReadOnlyList<PersistentObjectConfig> configs)
        {
            _configs.Clear();

            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];
                if (config == null || string.IsNullOrEmpty(config.prefabPath))
                    continue;

                if (string.IsNullOrEmpty(config.id))
                    config.id = $"persistent_{i}";

                _configs[config.id] = config;

                if (config.autoLoad)
                {
                    LoadObjectInternal(config);
                }
            }

            _isLoaded = true;
            Debug.Log($"[PersistentObjectManager] Loaded {_persistentObjects.Count} persistent objects.");
        }


        private void LoadObjectInternal(PersistentObjectConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.prefabPath))
                return;

            string id = config.id;
            if (string.IsNullOrEmpty(id))
                id = config.prefabPath;

            if (_persistentObjects.ContainsKey(id))
            {
                Debug.LogWarning($"[PersistentObjectManager] Object '{id}' already loaded.");
                return;
            }

            GameObject prefab = null;

            // Resources 加载
            prefab = Resources.Load<GameObject>(config.prefabPath);

            GameObject instance = Instantiate(prefab);
   
            instance.transform.SetParent(this.transform);
            if (config.overrideLocalPosition)
            {
                instance.transform.localPosition = config.localPosition;
            }

            // 标记不销毁
            if (config.dontDestroyOnLoad)
            {
                DontDestroyOnLoad(instance);
            }

            _persistentObjects[id] = instance;
            Debug.Log($"[PersistentObjectManager] Loaded persistent object: {id}");
        }


        /// <summary>
        /// 根据 ID 获取已加载的持久化对象
        /// </summary>
        public GameObject GetObject(string id)
        {
            if (_persistentObjects.TryGetValue(id, out var obj))
            {
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 根据 ID 获取已加载的持久化对象的组件
        /// </summary>
        public T GetObject<T>(string id) where T : Component
        {
            var obj = GetObject(id);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        /// <summary>
        /// 检查指定 ID 的对象是否已加载
        /// </summary>
        public bool IsLoaded(string id)
        {
            return _persistentObjects.ContainsKey(id);
        }

        /// <summary>
        /// 手动加载指定 ID 的持久化对象
        /// </summary>
        public void LoadObject(string id)
        {
            if (_configs.TryGetValue(id, out var config))
            {
                LoadObjectInternal(config);
            }
            else
            {
                Debug.LogError($"[PersistentObjectManager] Config with id '{id}' not found.");
            }
        }

        /// <summary>
        /// 运行时动态添加并加载对象
        /// </summary>
        public void AddAndLoad(GameObject prefab, string id = null, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[PersistentObjectManager] Prefab is null.");
                return;
            }

            string configId = id ?? prefab.name;

            var config = new PersistentObjectConfig
            {
                id = configId,
                prefabPath = prefab.name,
                autoLoad = true,
                dontDestroyOnLoad = true,
                parentPath = parent != null ? GetTransformPath(parent) : null
            };

            _configs[configId] = config;
            LoadObjectInternal(config);
        }

        private string GetTransformPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        /// <summary>
        /// 手动销毁指定的持久化对象
        /// </summary>
        public void DestroyObject(string id)
        {
            if (_persistentObjects.TryGetValue(id, out var obj) && obj != null)
            {
                _persistentObjects.Remove(id);
                Destroy(obj);
            }
        }

        /// <summary>
        /// 重置所有持久化对象（调试用）
        /// </summary>
        [ContextMenu("Reset All")]
        public void ResetAll()
        {
            foreach (var obj in _persistentObjects.Values)
            {
                if (obj != null)
                    Destroy(obj);
            }
            _persistentObjects.Clear();
            _configs.Clear();
            _isLoaded = false;
            _isInitialized = false;
        }

        /// <summary>
        /// 重新加载所有持久化对象（调试用）
        /// </summary>
        [ContextMenu("Reload All")]
        public void ReloadAll()
        {
            ResetAll();
            LoadConfigAndInstantiate();
        }
    }
}
