#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System.Editor
{
    /// <summary>
    /// 菜单工具：创建持久化对象配置
    /// </summary>
    public static class PersistentObjectMenu
    {
        [MenuItem("Assets/Create/CLIP/Persistent Object Config", false, 1)]
        public static void CreatePersistentObjectConfig()
        {
            var config = ScriptableObject.CreateInstance<PersistentObjectConfigSO>();

            // 获取选中的路径
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
            {
                path = "Assets/Resources/PersistentObject";
            }
            else if (!path.StartsWith("Assets/Resources/"))
            {
                path = "Assets/Resources/PersistentObject";
            }

            // 确保目录存在
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = "Assets/Resources";
                if (!AssetDatabase.IsValidFolder(parentPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder(parentPath, "PersistentObject");
            }

            // 保存资源
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/PersistentObjectConfig.asset");
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = config;
            EditorUtility.FocusProjectWindow();
        }
    }
}
#endif
