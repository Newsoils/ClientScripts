#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GameCoreResourceLoad;

[CustomEditor(typeof(Model_Placeholder))]
public class Model_Placeholder_Editor : Editor
{
    private SerializedProperty prefabObjectProp;
    private SerializedProperty prefabNameProp;

    // 初始化 Inspector 属性
    private void OnEnable()
    {
        prefabObjectProp = serializedObject.FindProperty("prefabObject");
        prefabNameProp = serializedObject.FindProperty("prefabName");
    }

    // Inspector GUI（支持修改即刷新）
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(prefabObjectProp);
        EditorGUILayout.PropertyField(prefabNameProp);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            RefreshPlaceholder((Model_Placeholder)target);
            SceneView.RepaintAll();  // 刷新场景视图
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    // ===============================================
    //       Scene Open 自动刷新（安全版本）
    // ===============================================
    [InitializeOnLoadMethod]
    private static void Init()
    {
        // 防止重复注册
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        foreach (var p in Object.FindObjectsOfType<Model_Placeholder>())
        {
            RefreshPlaceholder(p);
        }
    }

    // ===============================================
    //             公共刷新函数（核心）
    // ===============================================

    public static void RefreshPlaceholder(Model_Placeholder placeholder)
    {
        if (placeholder == null)
            return;

        // 不要在 Prefab 资源上实例化（只允许在场景中使用）
        if (PrefabUtility.IsPartOfPrefabAsset(placeholder))
            return;

        CleanupPreviousInstances(placeholder);

        GameObject prefab = ResolvePrefab(placeholder);
        if (prefab == null)
        {
            Debug.LogWarning($"Model_Placeholder: 无法找到可实例化的 Prefab ({placeholder.name})");
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, placeholder.transform);
        instance.name = prefab.name;
        instance.hideFlags = HideFlags.DontSave;  // 保证不写入场景文件

        Undo.RegisterCreatedObjectUndo(instance, "Instantiate Placeholder Model");
    }

    // ===============================================
    //                 清理旧实例
    // ===============================================
    private static void CleanupPreviousInstances(Model_Placeholder placeholder)
    {
        for (int i = placeholder.transform.childCount - 1; i >= 0; i--)
        {
            var child = placeholder.transform.GetChild(i);

            // 只删除由系统生成的对象
            if (child.hideFlags == HideFlags.DontSave)
                Object.DestroyImmediate(child.gameObject);
        }
    }

    // ===============================================
    //             查找 Prefab（本地引用 / 名称）
    // ===============================================
    private static GameObject ResolvePrefab(Model_Placeholder placeholder)
    {
        // 1) 使用拖入的对象（优先级最高）
        if (placeholder.prefabObject != null)
            return placeholder.prefabObject;

        // 2) 使用名字查找
        if (string.IsNullOrEmpty(placeholder.prefabName))
            return null;

        string searchPath = $"{ResourceMgr.BundleSrcDir}Prefabs";
        string[] guids = AssetDatabase.FindAssets($"t:Prefab {placeholder.prefabName}", new[] { searchPath });

        if (guids.Length == 0)
            return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}
#endif
