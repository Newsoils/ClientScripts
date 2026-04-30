using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 持久化对象配置数据
    /// </summary>
    [Serializable]
    public class PersistentObjectConfig
    {
        [Tooltip("唯一标识符，用于运行时查找")]
        public string id;

        [Tooltip("Prefab 资源路径（相对于 Resources 目录，或使用 Addressable Address）")]
        public string prefabPath;

        [Tooltip("是否在首次进入 MainScene 时自动加载")]
        public bool autoLoad = true;

        [Tooltip("加载成功后是否设置为不销毁")]
        public bool dontDestroyOnLoad = true;

        [Tooltip("可选：设置父物体Transform路径（为空则使用自身）")]
        public string parentPath;

        public PersistentObjectConfig()
        {
            id = Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }

    /// <summary>
    /// 持久化对象配置表（ScriptableObject）
    /// 放在 Resources/PersistentObject/ 目录下
    /// </summary>
    [CreateAssetMenu(fileName = "PersistentObjectConfig", menuName = "Project_Mouse/Persistent Object Config", order = 1)]
    public class PersistentObjectConfigSO : ScriptableObject
    {
        [Header("持久化 Prefab 配置")]
        [SerializeField] private List<PersistentObjectConfig> _persistentPrefabs = new List<PersistentObjectConfig>();

        public IReadOnlyList<PersistentObjectConfig> PersistentPrefabs => _persistentPrefabs;

        /// <summary>
        /// 根据 ID 查找配置
        /// </summary>
        public PersistentObjectConfig GetConfig(string id)
        {
            return _persistentPrefabs.Find(c => c.id == id);
        }

#if UNITY_EDITOR
        [ContextMenu("Sort By Id")]
        private void SortById()
        {
            _persistentPrefabs.Sort((a, b) => string.Compare(a.id, b.id, StringComparison.Ordinal));
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
