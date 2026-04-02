#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class Model_Placeholder_Menu
{
    // 1. 在场景中直接创建一个空的 Scene Placeholder
    [MenuItem("GameObject/创建 场景占位物体", false, 10)]
    public static void CreateScenePlaceholder(MenuCommand menuCommand)
    {
        CreatePlaceholderInternal(menuCommand.context as GameObject, null);
    }

    // ------------------ 新功能 ------------------ //
    // 2. 在 Project 中右键 Prefab 创建 Placeholder
    [MenuItem("Assets/创建 场景占位物体", false, 2000)]
    public static void CreatePlaceholderFromPrefab()
    {
        var obj = Selection.activeObject;

        if (!(obj is GameObject))
        {
            Debug.LogWarning("选择的不是 Prefab！");
            return;
        }

        string prefabName = obj.name;

        // 创建占位物体
        GameObject go = CreatePlaceholderInternal(null, prefabName);

        // 自动把 placeholder 放到 Scene 里的中心
        go.transform.position = Vector3.zero;
        go.gameObject.name = $"ScenePlaceholder_{prefabName}";

        // 高亮一下
        EditorGUIUtility.PingObject(go);
    }

    // Project 右键菜单仅当选中 prefab 时才出现
    [MenuItem("Assets/创建 场景占位物体", true)]
    private static bool ValidateCreatePlaceholderFromPrefab()
    {
        return Selection.activeObject is GameObject;
    }

    // ------------------ 私有函数 ------------------ //
    private static GameObject CreatePlaceholderInternal(GameObject parent, string prefabName)
    {
        GameObject go = new GameObject("ScenePlaceholder");

        var comp = go.AddComponent<Model_Placeholder>();

        if (!string.IsNullOrEmpty(prefabName))
            comp.prefabName = prefabName;

        // 对齐到层级
        if (parent != null)
            GameObjectUtility.SetParentAndAlign(go, parent);

        // 注册 Undo
        Undo.RegisterCreatedObjectUndo(go, "创建 场景占位物体");

        // 自动选中
        Selection.activeObject = go;

        return go;
    }
}
#endif
