using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.EditorTools
{
    /// <summary>
    /// 批量调整花盆预制体，统一为 furniture convention：
    /// - Root 位于网格中心（X/Z 偏移到子物体中心），Y = 0
    /// - Model_Mesh 位于 Root 本地原点（X/Z = 0），Y 使模型最低点/最高点贴 Root.y = 0
    /// - Plant_Root 跟随 Model_Mesh 同步移动，保留相对位置
    /// - 其他子物体（Grid_Cube 等）仅在 X/Z 转换时移动，Y 不变
    /// </summary>
    public class PotPrefabNormalizer : EditorWindow
    {
        [MenuItem("Tools/Pot Prefab Normalizer")]
        public static void ShowWindow()
        {
            GetWindow<PotPrefabNormalizer>("Pot Prefab Normalizer");
        }

        private void OnGUI()
        {
            GUILayout.Label("批量调整花盆预制体", EditorStyles.boldLabel);
            GUILayout.Label(
                "将所有 pot prefab 统一为 furniture convention：\n" +
                "  • Root 在中心，Y = 0\n" +
                "  • Model_Mesh 在原点，Y 使模型贴地/贴顶\n" +
                "  • Plant_Root / Grid_Cube 跟随 Model_Mesh 同步移动，保留相对位置\n" +
                "  • 其他子物体不动 Y",
                EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("调整所有花盆预制体", GUILayout.Height(40)))
            {
                NormalizeAllPotPrefabs();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("统一碰撞体 Y 为 0.5", GUILayout.Height(40)))
            {
                SetAllColliderYToHalf();
            }
        }

        private static void NormalizeAllPotPrefabs()
        {
            string potDir = "Assets/Resources/Prefabs/Planting/Pot";
            string prototypePath = "Assets/Resources/Prefabs/Planting/Pot_Prototype.prefab";

            int processed = 0;
            int skipped = 0;

            // 1. 处理原型（只调 Y，不改 X/Z，避免影响未覆盖该属性的 variant）
            if (!string.IsNullOrEmpty(prototypePath))
            {
                GameObject proto = AssetDatabase.LoadAssetAtPath<GameObject>(prototypePath);
                if (proto != null)
                {
                    bool ok = ProcessPrefab(proto, isPrototype: true);
                    if (ok)
                    {
                        PrefabUtility.SavePrefabAsset(proto);
                        processed++;
                        Debug.Log($"[Pot_Prototype] 已调整 Y");
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }

            // 2. 处理所有 variant prefab
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { potDir });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool ok = ProcessPrefab(prefab, isPrototype: false);
                if (ok)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    processed++;
                }
                else
                {
                    skipped++;
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", $"处理 {processed} 个预制体，跳过 {skipped} 个。\n详见 Console 日志。", "确定");
        }

        /// <summary>
        /// 处理单个 prefab，返回 true 表示成功处理。
        /// </summary>
        private static bool ProcessPrefab(GameObject prefab, bool isPrototype)
        {
            Transform root = prefab.transform.Find("Root");
            if (root == null)
            {
                Debug.LogWarning($"[{prefab.name}] 找不到 Root，跳过");
                return false;
            }

            Transform modelMesh = root.Find("Model_Mesh");
            if (modelMesh == null)
            {
                Debug.LogWarning($"[{prefab.name}] 找不到 Model_Mesh，跳过");
                return false;
            }

            bool isHanging = prefab.name.Contains("吊顶") || prefab.name.Contains("Hang");
            bool modified = false;

            // ---------- Step 1: X/Z 转换（pot convention → furniture convention） ----------
            // 仅对 variant 做完整转换；prototype 只调 Y，不改 X/Z，防止破坏未覆盖的 variant
            if (!isPrototype)
            {
                Vector3 childOffsetXZ = new Vector3(modelMesh.localPosition.x, 0, modelMesh.localPosition.z);
                if (childOffsetXZ.magnitude > 0.01f)
                {
                    foreach (Transform child in root.Cast<Transform>().ToArray())
                    {
                        Undo.RecordObject(child, "Pot Prefab Normalize");
                        Vector3 lp = child.localPosition;
                        lp.x -= childOffsetXZ.x;
                        lp.z -= childOffsetXZ.z;
                        child.localPosition = lp;
                    }

                    Undo.RecordObject(root, "Pot Prefab Normalize");
                    Vector3 rp = root.localPosition;
                    rp.x += childOffsetXZ.x;
                    rp.z += childOffsetXZ.z;
                    root.localPosition = rp;

                    modified = true;
                }
            }

            // ---------- Step 2: Y 调整 —— 只动 Model_Mesh，同步 Plant_Root / Grid_Cube ----------
            float dy = CalculateYAdjustmentForMesh(modelMesh, isHanging);
            if (Mathf.Abs(dy) > 0.001f)
            {
                Undo.RecordObject(modelMesh, "Pot Prefab Normalize Mesh Y");
                Vector3 meshLp = modelMesh.localPosition;
                meshLp.y += dy;
                modelMesh.localPosition = meshLp;

                Transform plantRoot = root.Find("Plant_Root");
                if (plantRoot != null)
                {
                    Undo.RecordObject(plantRoot, "Pot Prefab Normalize PlantRoot Y");
                    Vector3 prLp = plantRoot.localPosition;
                    prLp.y += dy;
                    plantRoot.localPosition = prLp;
                }

                Transform gridCube = root.Find("Grid_Cube");
                if (gridCube != null)
                {
                    Undo.RecordObject(gridCube, "Pot Prefab Normalize GridCube Y");
                    Vector3 gcLp = gridCube.localPosition;
                    gcLp.y += dy;
                    gridCube.localPosition = gcLp;
                }

                modified = true;
            }

            // ---------- Step 3: 确保 Root.y = 0 ----------
            if (Mathf.Abs(root.localPosition.y) > 0.001f)
            {
                Undo.RecordObject(root, "Pot Prefab Normalize Root Y");
                Vector3 rp = root.localPosition;
                rp.y = 0;
                root.localPosition = rp;
                modified = true;
            }

            if (modified)
            {
                Debug.Log($"[{prefab.name}] 已调整 (hanging={isHanging}, dy={dy:F3})");
            }
            else
            {
                Debug.Log($"[{prefab.name}] 无需调整");
            }

            return true;
        }

        /// <summary>
        /// 计算需要把 Model_Mesh 上下移动多少，才能让模型最低点/最高点落在 Root.y = 0。
        /// </summary>
        private static float CalculateYAdjustmentForMesh(Transform meshTransform, bool isHanging)
        {
            MeshFilter mf = meshTransform.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
            {
                return 0f;
            }

            Bounds bounds = mf.sharedMesh.bounds;
            float lowestY = meshTransform.localPosition.y + bounds.min.y;
            float highestY = meshTransform.localPosition.y + bounds.max.y;

            if (isHanging)
            {
                // 吊顶：最高点紧贴 Root.y = 0
                return -highestY;
            }
            else
            {
                // 地面：最低点紧贴 Root.y = 0
                return -lowestY;
            }
        }

        /// <summary>
        /// 批量将所有花盆 prefab 的 Grid_Cube.localPosition.y 设为 0.5。
        /// </summary>
        private static void SetAllColliderYToHalf()
        {
            string potDir = "Assets/Resources/Prefabs/Planting/Pot";
            string prototypePath = "Assets/Resources/Prefabs/Planting/Pot_Prototype.prefab";

            int processed = 0;
            int skipped = 0;

            // 1. 处理原型
            if (!string.IsNullOrEmpty(prototypePath))
            {
                GameObject proto = AssetDatabase.LoadAssetAtPath<GameObject>(prototypePath);
                if (proto != null)
                {
                    bool ok = SetGridCubeY(proto, 0.5f);
                    if (ok)
                    {
                        PrefabUtility.SavePrefabAsset(proto);
                        processed++;
                        Debug.Log($"[Pot_Prototype] Grid_Cube.y 已设为 0.5");
                    }
                    else
                    {
                        skipped++;
                    }
                }
            }

            // 2. 处理所有 variant prefab
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { potDir });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool ok = SetGridCubeY(prefab, 0.5f);
                if (ok)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                    processed++;
                }
                else
                {
                    skipped++;
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", $"处理 {processed} 个预制体，跳过 {skipped} 个。\n详见 Console 日志。", "确定");
        }

        private static bool SetGridCubeY(GameObject prefab, float targetY)
        {
            Transform root = prefab.transform.Find("Root");
            if (root == null)
            {
                Debug.LogWarning($"[{prefab.name}] 找不到 Root，跳过");
                return false;
            }

            Transform gridCube = root.Find("Grid_Cube");
            if (gridCube == null)
            {
                Debug.LogWarning($"[{prefab.name}] 找不到 Grid_Cube，跳过");
                return false;
            }

            if (Mathf.Abs(gridCube.localPosition.y - targetY) > 0.001f)
            {
                Undo.RecordObject(gridCube, "Pot Set Collider Y");
                Vector3 lp = gridCube.localPosition;
                lp.y = targetY;
                gridCube.localPosition = lp;
                Debug.Log($"[{prefab.name}] Grid_Cube.y 已设为 {targetY:F3}");
                return true;
            }

            Debug.Log($"[{prefab.name}] Grid_Cube.y 已是 {targetY:F3}，无需调整");
            return true;
        }
    }
}
