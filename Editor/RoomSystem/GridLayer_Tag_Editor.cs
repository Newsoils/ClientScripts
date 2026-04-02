using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridLayerTag))]
public class GridLayerTagEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        GridLayerTag tag = (GridLayerTag)target;

        if (GUILayout.Button("生成网格原点Origin"))
        {
            BuildSurface(tag);
        }
    }

    void BuildSurface(GridLayerTag tag)
    {
        Transform root = tag.transform;

        // 防止重复生成
        if (root.Find("GridOrigin") != null)
        {
           DestroyImmediate(root.Find("GridOrigin").gameObject);
            return;
        }
        MeshFilter mf = root.GetComponentInChildren<MeshFilter>();
        var mesh = mf.sharedMesh;
        if (mesh == null)
        {
            Debug.LogError("没有找到 MeshFilter");
            return;
        }
       
        Vector3[] vertices = mesh.vertices;
        Vector3 min = vertices[0];

        foreach (var v in vertices)
        {
            min = Vector3.Min(min, v);
        }

        Vector3 worldMin = mf.transform.TransformPoint(min);


        // 创建Origin
        GameObject origin = new GameObject("GridOrigin");
        origin.transform.SetParent(root);
        origin.transform.rotation = mf.transform.rotation;
        origin.transform.position = worldMin;

        // 自动赋值
        tag.girdOrginalPoint = origin.transform;

        EditorUtility.SetDirty(tag);

        Debug.Log("GridOrigin生成完成");
    }
}