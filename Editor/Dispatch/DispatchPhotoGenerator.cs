using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using CLIP.Framework_Unity;

public class DispatchPhotoGenerator : EditorWindow
{
    private const string SceneFolder = "Assets/Scenes/Dispatch";
    private const string ScenePrefix = "DispatchScene_";
    private const int GenerateCount = 20;

    [MenuItem("Tools/Photo Generator/Generate For All Dispatch Scenes")]
    public static void GenerateForAllDispatchScenes()
    {
        string[] allScenes = Directory.GetFiles(SceneFolder, ScenePrefix + "*.unity", SearchOption.AllDirectories);

        if (allScenes.Length == 0)
        {
            Debug.LogWarning("未找到任何 DispatchScene_ 开头的场景！");
            return;
        }

        foreach (string scenePath in allScenes)
        {
            Debug.Log("处理场景：" + scenePath);

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            GeneratePhotoPointsInScene(scene);

            EditorSceneManager.SaveScene(scene);
        }

        Log.Info("所有场景处理完成！");
    }

    private static void GeneratePhotoPointsInScene(Scene scene)
    {
        // 已存在就跳过
        GameObject existingRoot = FindInScene(scene, "PhotoRoot");
        if (existingRoot != null)
        {
            Log.Info($"场景 {scene.name} 已存在 PhotoRoot，跳过生成。");
            return;
        }


        // 完全新建
        GameObject root = new GameObject("PhotoRoot");

        for (int i = 1; i <= GenerateCount; i++)
        {
            string groupName = $"PhotoGroup_{i:00}";
            GameObject group = GetOrCreate(groupName, root.transform);

            // Camera
            GameObject camObj = GetOrCreate("Camera", group.transform);
            if (!camObj.TryGetComponent<Camera>(out _))
            {
                camObj.AddComponent<Camera>();
                camObj.transform.localPosition = new Vector3(0, 1.6f, -2);
                camObj.transform.localRotation = Quaternion.identity;
            }

            // MainPoint
            GameObject mainPoint = GetOrCreate("MainPoint", group.transform);
            mainPoint.transform.localPosition = Vector3.zero;

            // 在 MainPoint 下生成角色占位
            CreateCharacterPlaceholder(mainPoint.transform, "MainCharacterHolder", "Main_Character_photo");

            // NPCPoint
            GameObject npcPoint = GetOrCreate("NPCPoint", group.transform);
            npcPoint.transform.localPosition = Vector3.zero;

            // 在 NPCPoint 下生成 NPC 占位
            CreateCharacterPlaceholder(npcPoint.transform, "NPCHolder", "NPC_Photo_Default");
        }

        Log.Info($"场景 {scene.name} 生成完成");
    }

    private static void CreateCharacterPlaceholder(Transform parent, string childName, string defaultPrefabName)
    {
        GameObject holder = GetOrCreate(childName, parent);

        // 若已经有 Model_Placeholder 则不重复挂载，只更新默认名字
        if (!holder.TryGetComponent<Model_Placeholder>(out var placeholder))
        {
            placeholder = holder.AddComponent<Model_Placeholder>();
        }

        // **仅当 prefabName 为空时才填默认值，避免覆盖手动修改的内容**
        if (string.IsNullOrEmpty(placeholder.prefabName))
        {
            placeholder.prefabName = defaultPrefabName;
        }

        // transform reset
        holder.transform.localPosition = Vector3.zero;
    }

    private static GameObject GetOrCreate(string name, Transform parent)
    {
        Transform child = parent != null ? parent.Find(name) : null;

        if (child != null)
            return child.gameObject;

        GameObject obj = new GameObject(name);
        if (parent != null)
            obj.transform.SetParent(parent);

        return obj;
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;

            // 递归查找子物体
            Transform found = root.transform.Find(name);
            if (found != null)
                return found.gameObject;
        }
        return null;
    }
}
