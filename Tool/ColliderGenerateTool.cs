using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CLIP.Project_Mouse.Custom_Tool
{

    /// <summary>
    /// 碰撞体/单元格生成工具
    /// 根据目标物体的包围盒大小，自动生成网格状的碰撞体或单元格预制体
    /// </summary>
    public enum AlignmentMode
    {
        Center,          // 居中对齐
        BottomLeft,      // 左下对齐
        BottomRight,     // 右下对齐
        TopLeft,         // 左上对齐
        TopRight,        // 右上对齐
        Bottom,          // 底部对齐
        Top,             // 顶部对齐
        Left,            // 左侧对齐
        Right,           // 右侧对齐
        Custom           // 自定义偏移
    }

    public class ColliderGenerateTool : MonoBehaviour
    {
        [Tooltip("需要生成碰撞体/单元格的目标游戏对象列表")]
        public List<GameObject> gameObjectList;

        [Tooltip("生成的单元格对齐方式")]
        public AlignmentMode alignmentMode = AlignmentMode.Center;

        [Tooltip("自定义对齐方式时的偏移量")]
        public Vector3 customOffset = Vector3.zero;

        [Tooltip("单元格预制体（用于实例化的模板）")]
        public GameObject _cell_prefab;

        /// <summary>
        /// 生成碰撞体/单元格的主方法
        /// 遍历目标列表，为每个对象生成网格状的单元格
        /// </summary>
        public void GenerateCollider()
        {
            // 检查目标列表是否为空
            if (gameObjectList == null || gameObjectList.Count == 0) return;

            // 遍历每个目标游戏对象
            foreach (GameObject go in gameObjectList)
            {
                if (go == null) continue;

                // 获取或添加MeshCollider组件（用于获取物体的包围盒）
                MeshCollider collider = go.GetComponent<MeshCollider>();
                if (collider == null)
                {
                    collider = go.AddComponent<MeshCollider>();
                }

                // 获取物体的包围盒信息
                Bounds bounds = collider.bounds;

                // 将包围盒尺寸四舍五入为整数，用于确定网格数量
                int boundX = Mathf.RoundToInt(bounds.size.x);
                int boundY = Mathf.RoundToInt(bounds.size.y);
                int boundZ = Mathf.RoundToInt(bounds.size.z);

                // 计算碰撞体/单元格的整体尺寸
                Vector3 colliderSize = new Vector3(boundX, boundY, boundZ);
                Vector3 startPos;
                float baseStartY = bounds.min.y;

                // 提取包围盒的关键位置信息
                float boundsMinX = bounds.min.x;
                float boundsMaxX = bounds.max.x;
                float boundsMinZ = bounds.min.z;
                float boundsMaxZ = bounds.max.z;
                float boundsCenterX = bounds.center.x;
                float boundsCenterZ = bounds.center.z;

                // 根据选择的对齐方式计算起始位置
                switch (alignmentMode)
                {
                    case AlignmentMode.Center:
                        startPos = new Vector3(boundsCenterX - colliderSize.x / 2f, baseStartY, boundsCenterZ - colliderSize.z / 2f);
                        break;
                    case AlignmentMode.BottomLeft:
                        startPos = new Vector3(boundsMinX, baseStartY, boundsMinZ);
                        break;
                    case AlignmentMode.BottomRight:
                        startPos = new Vector3(boundsMaxX - colliderSize.x, baseStartY, boundsMinZ);
                        break;
                    case AlignmentMode.TopLeft:
                        startPos = new Vector3(boundsMinX, baseStartY, boundsMaxZ - colliderSize.z);
                        break;
                    case AlignmentMode.TopRight:
                        startPos = new Vector3(boundsMaxX - colliderSize.x, baseStartY, boundsMaxZ - colliderSize.z);
                        break;
                    case AlignmentMode.Bottom:
                        startPos = new Vector3(boundsCenterX - colliderSize.x / 2f, baseStartY, boundsMinZ);
                        break;
                    case AlignmentMode.Top:
                        startPos = new Vector3(boundsCenterX - colliderSize.x / 2f, baseStartY, boundsMaxZ - colliderSize.z);
                        break;
                    case AlignmentMode.Left:
                        startPos = new Vector3(boundsMinX, baseStartY, boundsCenterZ - colliderSize.z / 2f);
                        break;
                    case AlignmentMode.Right:
                        startPos = new Vector3(boundsMaxX - colliderSize.x, baseStartY, boundsCenterZ - colliderSize.z / 2f);
                        break;
                    case AlignmentMode.Custom:
                        startPos = new Vector3(bounds.min.x + customOffset.x, baseStartY + customOffset.y, bounds.min.z + customOffset.z);
                        break;
                    default:
                        startPos = new Vector3(bounds.min.x, baseStartY, bounds.min.z);
                        break;
                }

                // 创建父物体用于管理生成的单元格
                GameObject parentGO = new GameObject(go.name + "_CubeColliders");
                parentGO.transform.parent = go.transform;
                parentGO.transform.localPosition = Vector3.zero;
                parentGO.transform.localRotation = Quaternion.identity;
                parentGO.transform.localScale = Vector3.one;

                // 三维循环生成单元格网格
                for (int x = 0; x < boundX; x++)
                {
                    for (int y = 0; y < boundY; y++)
                    {
                        for (int z = 0; z < boundZ; z++)
                        {
                            // 在编辑器模式下使用PrefabUtility实例化预制体
#if UNITY_EDITOR
                            GameObject cube = (GameObject)PrefabUtility.InstantiatePrefab(_cell_prefab);
#else
                        GameObject cube = Instantiate(_cell_prefab);
#endif
                            cube.name = $"Cube_{x}_{y}_{z}";

                            // 计算每个单元格的位置
                            Vector3 center = new Vector3(startPos.x + x, startPos.y + y, startPos.z + z);
                            cube.transform.position = center;
                            cube.transform.parent = parentGO.transform;

                            // 调整单元格的缩放以匹配父物体的缩放
                            Vector3 parentLossyScale = parentGO.transform.lossyScale;
                            cube.transform.localScale = new Vector3(
                                1f / parentLossyScale.x,
                                1f / parentLossyScale.y,
                                1f / parentLossyScale.z
                            );
                        }
                    }
                }
            }
        }
    }
}