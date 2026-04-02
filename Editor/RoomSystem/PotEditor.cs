using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Newtonsoft.Json;
using CLIP.Project_Mouse.Game_Play_System;


public class PotProcessor : EditorWindow
{
    private List<PotData> potDataList = new List<PotData>();

    [MenuItem("Tools/Pot Processor")]
    public static void ShowWindow()
    {
        GetWindow<PotProcessor>("Pot Processor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Pot Data Processor", EditorStyles.boldLabel);

        if (GUILayout.Button("Process All Pots"))
        {
            ProcessAllPots();
        }
    }

    public void ProcessAllPots()
    {
        string data = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_pot_info");
        potDataList = JsonConvert.DeserializeObject<List<PotData>>(data);
        if (potDataList == null || potDataList.Count == 0)
        {
            Debug.LogWarning("No pot data to process!");
            return;
        }

        foreach (var potData in potDataList)
        {
            ProcessSinglePot(potData);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Processed {potDataList.Count} pots successfully!");
    }

    private void ProcessSinglePot(PotData potData)
    {
        // 1. 根据resUrl加载预制体
        string prefabPath = potData.resUrl;
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        string fullPrefabPath = AssetDatabase.GetAssetPath(prefab);
        if (prefab == null)
        {
            Debug.LogError($"Cannot load prefab at path: {prefabPath}");
            return;
        }

        // 实例化预制体以便修改
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            Debug.LogError($"Failed to instantiate prefab: {prefab.name}");
            return;
        }

        // 开始记录修改
        Undo.RecordObject(instance, "Process Pot");

        // 2. 找到名为"Root"的组件（应该是Transform组件）
        Transform rootTransform = instance.transform.Find("Root");
        if (rootTransform == null)
        {
            Debug.LogError($"Cannot find 'Root' transform in prefab: {prefab.name}");
            DestroyImmediate(instance);
            return;
        }

        // 3. 计算中心点位置
        // length对应X轴，width对应Z轴
        float centerX = potData.length / 2f;  // X轴中心
        float centerY = potData.height / 2f;
        float centerZ = potData.width / 2f;   // Z轴中心
        Vector3 centerPosition = new Vector3(centerX, centerY, centerZ);

        // 4. 移动Root到中心
        rootTransform.localPosition = centerPosition;
        // 5. 处理Root下的所有子物体
        foreach (Transform child in rootTransform)
        {
            child.localPosition = Vector3.zero;
        }
        AlignMeshesToWorldZero(rootTransform);



        // 6. 处理BoxCollider
        BoxCollider[] boxColliders = rootTransform.GetComponentsInChildren<BoxCollider>();
        foreach (var boxCollider in boxColliders)
        {
            boxCollider.size = new Vector3(potData.length, potData.height, potData.width);
        }

        // 保存修改到预制体
        PrefabUtility.SaveAsPrefabAsset(instance, fullPrefabPath);
        DestroyImmediate(instance);

        Debug.Log($"Successfully processed: {prefab.name}");
    }
    private void AlignMeshesToWorldZero(Transform rootTransform)
    {
        // 获取所有MeshFilter组件（包括子物体）
        MeshFilter[] meshFilters = rootTransform.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
        {
            Debug.LogWarning($"No MeshFilter found in {rootTransform.name}");
            return;
        }

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"MeshFilter on {meshFilter.gameObject.name} has no mesh");
                continue;
            }

            // 获取mesh的顶点数组（在世界空间中计算）
            Vector3[] vertices = meshFilter.sharedMesh.vertices;

            if (vertices.Length == 0)
            {
                continue;
            }

            // 将顶点转换到世界空间
            List<Vector3> worldVertices = new List<Vector3>();
            foreach (Vector3 vertex in vertices)
            {
                worldVertices.Add(meshFilter.transform.TransformPoint(vertex));
            }

            // 计算在世界空间中的最低Y值
            float minY = float.MaxValue;
            foreach (Vector3 worldVertex in worldVertices)
            {
                if (worldVertex.y < minY)
                {
                    minY = worldVertex.y;
                }
            }

            // 如果最低点已经是0，不需要调整
            if (Mathf.Approximately(minY, 0f))
            {
                continue;
            }

            // 计算需要向上移动的距离（在世界空间中）
            float offsetY = -minY;

            // 移动整个物体（在世界空间中向上移动）
            meshFilter.transform.position += new Vector3(0, offsetY, 0);

            Debug.Log($"Moved {meshFilter.gameObject.name} up by {offsetY} to set lowest point to 0");
        }
    }
    // 这个方法供你传入数据
    public void SetPotDataList(List<PotData> dataList)
    {
        potDataList = dataList;
    }
}