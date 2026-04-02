using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 用于生成一个带圆角的矩形边框 Mesh（例如放置系统预览）
/// - 不依赖 targetPlacement
/// - 直接在当前物体下生成网格
/// - 支持圆角半径、边框厚度、格子宽高
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlacementBorderGenerator : MonoBehaviour
{
    [Header("尺寸设置")]
    public int width = 2;       // 格子宽度
    public int length = 3;      // 格子高度
    public float cellSize = 1f; // 单格单位大小

    [Header("视觉设置")]
    public float borderThickness = 0.4f; // 边框厚度
    public float cornerRadius = 0.3f;     // 圆角半径
    public int cornerSegments = 6;        // 圆角细分段数

    private MeshFilter meshFilter;


    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void GenerateBorderMesh_DS()
    {
        if (meshFilter == null) return;
        Mesh mesh = new Mesh();
        mesh.name = "Animated Placement Border";

        float worldW = length * cellSize;
        float worldH = width * cellSize;
        float halfW = worldW * 0.5f;
        float halfH = worldH * 0.5f;

        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var tris = new List<int>();

        // 生成更细致的UV用于螺旋效果
        BuildRoundedBorderWithUV(verts, uvs, normals, tris, halfW, halfH);

        Vector3 pivotOffset = new Vector3(0, 0.01f, 0); // 轻微上浮避免Z-fighting

        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] += pivotOffset;
        }

        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();

        // 设置Mesh
        meshFilter.sharedMesh = mesh;
    }

    private void BuildRoundedBorderWithUV(
        List<Vector3> verts,
        List<Vector2> uvs,
        List<Vector3> normals,
        List<int> tris,
        float halfW, float halfH)
    {
        // 四个圆角圆心
        Vector2[] cornerCenters =
        {
            new Vector2( halfW - cornerRadius,  halfH - cornerRadius),
            new Vector2(-halfW + cornerRadius,  halfH - cornerRadius),
            new Vector2(-halfW + cornerRadius, -halfH + cornerRadius),
            new Vector2( halfW - cornerRadius, -halfH + cornerRadius)
        };

        // 计算总顶点数用于UV映射
        int totalSegments = 4 * (cornerSegments + 1);
        int segmentIndex = 0;

        int previousOuter = -1;
        int previousInner = -1;

        for (int c = 0; c < 4; c++)
        {
            float startAngle = c * 90f * Mathf.Deg2Rad;

            for (int s = 0; s <= cornerSegments; s++)
            {
                float angle = startAngle + s * (Mathf.PI * 0.5f) / cornerSegments;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                // 内圈顶点
                Vector3 innerVert = new Vector3(
                    cornerCenters[c].x + dir.x * cornerRadius,
                    0,
                    cornerCenters[c].y + dir.y * cornerRadius
                );

                // 外圈顶点
                Vector3 outerVert = innerVert + new Vector3(dir.x, 0, dir.y) * borderThickness;

                // 计算UV（沿着边框的连续UV）
                float uvX = (float)segmentIndex / totalSegments;

                int outerIndex = verts.Count;
                verts.Add(outerVert);
                uvs.Add(new Vector2(uvX, 1)); // V=1 表示外边缘
                normals.Add(Vector3.up);

                int innerIndex = verts.Count;
                verts.Add(innerVert);
                uvs.Add(new Vector2(uvX, 0)); // V=0 表示内边缘
                normals.Add(Vector3.up);

                // 生成三角形
                if (previousOuter >= 0)
                {
                    tris.Add(previousOuter);
                    tris.Add(previousInner);
                    tris.Add(outerIndex);

                    tris.Add(previousInner);
                    tris.Add(innerIndex);
                    tris.Add(outerIndex);
                }

                previousOuter = outerIndex;
                previousInner = innerIndex;
                segmentIndex++;
            }
        }

        // 闭合边框
        int firstOuter = 0;
        int firstInner = 1;

        tris.Add(previousOuter);
        tris.Add(previousInner);
        tris.Add(firstOuter);

        tris.Add(previousInner);
        tris.Add(firstInner);
        tris.Add(firstOuter);
    }


    private void OnDrawGizmosSelected()
    {
        // 绘制边框范围（调试用）
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Vector3 center = transform.position + new Vector3(width * cellSize * 0.5f, 0, length * cellSize * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(width * cellSize, 0.1f, length * cellSize));
    }
  
}

