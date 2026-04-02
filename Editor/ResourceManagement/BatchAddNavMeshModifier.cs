using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class BatchAddNavMeshModifier : Editor
{
    [MenuItem("Tools/NavMesh/批量添加NavMeshModifier到Grid_Cube")]
    public static void AddModifierToGridCubes()
    {
        string targetPath = "Assets/Resources/Prefabs/Planting/Pot";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { targetPath });

        int count = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool prefabModified = false;

            // 打开prefab编辑上下文
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            var gridCubes = prefabInstance.GetComponentsInChildren<Transform>(true);

            foreach (var t in gridCubes)
            {
                if (t.name == "Grid_Cube")
                {
                    if (t.GetComponent<BoxCollider>() == null)
                        continue;
                    var modifier = t.GetComponent<NavMeshObstacle>();
                    if (modifier == null)
                    {
                        modifier = Undo.AddComponent<NavMeshObstacle>(t.gameObject);
                    }
                    modifier.size = t.GetComponent<BoxCollider>().size;
                    modifier.carving = true;
                    prefabModified = true;
                }
            }

            if (prefabModified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                count++;
            }

            GameObject.DestroyImmediate(prefabInstance);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✔ 完成：共更新 {count} 个 Prefab");
    }
}
