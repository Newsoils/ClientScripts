// BorderMeshGeneratorEditor.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BorderMeshGeneratorEditor : EditorWindow
{
    [MenuItem("Tools/生成边框Mesh")]
    static void Init()
    {
        BorderMeshGeneratorEditor window = GetWindow<BorderMeshGeneratorEditor>();
        window.titleContent = new GUIContent("边框Mesh生成器");
        window.Show();
    }

    // 参数
    private int width = 2;
    private int length = 3;
    private float cellSize = 1f;
    private float borderThickness = 0.4f;
    private float cornerRadius = 0.3f;
    private int cornerSegments = 6;
    private string meshName = "BorderMesh";

    void OnGUI()
    {
        GUILayout.Label("边框设置", EditorStyles.boldLabel);

        width = EditorGUILayout.IntField("宽度 (格子)", width);
        length = EditorGUILayout.IntField("长度 (格子)", length);
        cellSize = EditorGUILayout.FloatField("格子大小", cellSize);
        borderThickness = EditorGUILayout.FloatField("边框厚度", borderThickness);
        cornerRadius = EditorGUILayout.FloatField("圆角半径", cornerRadius);
        cornerSegments = EditorGUILayout.IntField("圆角分段", cornerSegments);
        meshName = EditorGUILayout.TextField("Mesh名称", meshName);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("生成并保存Mesh", GUILayout.Height(40)))
        {
            GenerateAndSaveMesh();
        }
    }

    private void GenerateAndSaveMesh()
    {
        // 计算尺寸
        float worldW = width * cellSize;
        float worldH = length * cellSize;
        float halfW = worldW * 0.5f;
        float halfH = worldH * 0.5f;

        // 创建Mesh
        Mesh mesh = new Mesh();
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var normals = new List<Vector3>();
        var tris = new List<int>();

        // 生成顶点数据（这里简化，用你的逻辑）
        BuildRoundedBorderWithUV(verts, uvs, normals, tris, halfW, halfH);

        // Pivot偏移
        Vector3 pivotOffset = new Vector3(halfW, 0.01f, halfH);
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] += pivotOffset;
        }

        // 设置Mesh
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateBounds();
        mesh.name = meshName;

        // 保存Mesh为Asset
        string path = "Assets/Art/GeneratedMeshes/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string fullPath = path + meshName + ".asset";
        AssetDatabase.CreateAsset(mesh, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Mesh已保存到: {fullPath}");

        // 选中生成的Mesh
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Mesh>(fullPath);
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

}
#endif