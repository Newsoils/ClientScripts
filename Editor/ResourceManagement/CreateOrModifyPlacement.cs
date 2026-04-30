using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// 加上这个特性，方便 JsonUtility 序列化
[System.Serializable]
public class PlacementPathSetting
{
    public string OldModelPath = "";
    public string NewModelPath = "";
    public string MaterialPath = "";
    public string TexturePath = "";
    public string OutPrefabPathOld = "";
    public string OutPrefabPathNew = "";
}

public class CreateOrModifyPlacement : EditorWindow
{
    private static string PREFS_KEY = "PlacementPathSetting";
    private static string PLACEMENT_SO_KEY = "PLACEMENT_SO_KEY";
    private static string PlacementGeometry_SO_KEY = "PlacementGeometry_SO_KEY";
    private static string PlacementGeometry_Json_KEY = "PlacementGeometry_Json_Key";
    public PlacementPathSetting pathData = new PlacementPathSetting();
    public Placement_SO db;
    public PlacementGeometry_SO geoDb;
    public TextAsset geometryJson;



    [MenuItem("Tools/生成\\修正家具 Prefab")]
    public static void ShowWindow()
    {
        GetWindow<CreateOrModifyPlacement>("生成/修正家具Prefab");
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnGUI()
    {
        // 使用 EditorGUI.BeginChangeCheck 来更精确地捕获变动
        EditorGUI.BeginChangeCheck();

        db = (Placement_SO)EditorGUILayout.ObjectField(nameof(Placement_SO), db, typeof(Placement_SO), false);

        geoDb = (PlacementGeometry_SO)EditorGUILayout.ObjectField(nameof(PlacementGeometry_SO), geoDb, typeof(PlacementGeometry_SO), false);

        geometryJson = (TextAsset)EditorGUILayout.ObjectField("家具尺寸Json", geometryJson, typeof(TextAsset), false);

        // 核心修改：将返回值赋回给 pathData 成员
        pathData.OldModelPath = DrawPathFolder("模型文件路径（老）", pathData.OldModelPath);
        pathData.NewModelPath = DrawPathFolder("模型文件路径（新）", pathData.NewModelPath);
        //pathData.MaterialPath = DrawPathFolder("材质文件路径", pathData.MaterialPath);
        //pathData.TexturePath = DrawPathFolder("贴图文件路径", pathData.TexturePath);
        pathData.OutPrefabPathOld = DrawPathFolder("输出Prefab路径（老）", pathData.OutPrefabPathOld);
        pathData.OutPrefabPathNew = DrawPathFolder("输出Prefab路径（新）",pathData.OutPrefabPathNew);



        GUILayout.Space(10);

        if (GUILayout.Button("手动保存路径配置"))
        {
            SaveData();
            ShowNotification(new GUIContent("配置已保存"));
        }

        // 如果界面上有任何输入改动，执行保存
        if (EditorGUI.EndChangeCheck())
        {
            SaveData();
            // 强制重绘，确保路径显示即时更新
            Repaint();
        }


        GUILayout.Space(10);
        if (GUILayout.Button("生成家具（新）"))
        {
            CreatePlacementPrefabs();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("修正家具（老）"))
        {
            ModifyFurniture();
        }

        GUILayout.Space(10);
        if(GUILayout.Button("测量家具尺寸"))
        {
            MeasurePlacementGeo();
        }

    }

    private string DrawPathFolder(string label, string path)
    {
        EditorGUILayout.BeginVertical("box");

        // 1. 标题
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // 2. 文件夹对象槽
        DefaultAsset folderObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
        DefaultAsset newObj = (DefaultAsset)EditorGUILayout.ObjectField(folderObj, typeof(DefaultAsset), false);

        if (newObj != folderObj)
        {
            string newPath = AssetDatabase.GetAssetPath(newObj);
            if (AssetDatabase.IsValidFolder(newPath))
            {
                path = newPath;
            }
            else if (newObj == null)
            {
                path = "";
            }
        }

        // 3. 快速清空按钮 (可选，提升体验)
        if (GUILayout.Button("重置", GUILayout.Width(40)))
        {
            path = "";
        }

        EditorGUILayout.EndHorizontal();

        // 4. 【核心修改】显示具体路径
        if (!string.IsNullOrEmpty(path))
        {
            // 使用 HelpBox 或者 SelectableLabel，这样用户还可以选中并复制路径
            EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(18));
        }
        else
        {
            EditorGUILayout.HelpBox("请拖入文件夹或手动输入路径", MessageType.None);
        }

        EditorGUILayout.EndVertical();

        return path;
    }


    // --- 数据保存与读取逻辑 ---
    void LoadData()
    {
        if (EditorPrefs.HasKey(PREFS_KEY))
        {
            string json = EditorPrefs.GetString(PREFS_KEY);
            // 覆盖当前对象
            JsonUtility.FromJsonOverwrite(json, pathData);
        }

        if (EditorPrefs.HasKey(PLACEMENT_SO_KEY))
        {
            string dbPath = EditorPrefs.GetString(PLACEMENT_SO_KEY, "");
            db = AssetDatabase.LoadAssetAtPath<Placement_SO>(dbPath);
        }

        if (EditorPrefs.HasKey(PlacementGeometry_SO_KEY))
        {
            string GeoPath = EditorPrefs.GetString(PlacementGeometry_SO_KEY);
            geoDb = AssetDatabase.LoadAssetAtPath<PlacementGeometry_SO>(GeoPath);
        }

        if(EditorPrefs.HasKey(PlacementGeometry_SO_KEY))
        {
            string geoJson = EditorPrefs.GetString(PlacementGeometry_Json_KEY);
            geometryJson = AssetDatabase.LoadAssetAtPath<TextAsset>(geoJson);
        }

    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(pathData);
        EditorPrefs.SetString(PREFS_KEY, json);

        string dbPath = AssetDatabase.GetAssetPath(db);
        EditorPrefs.SetString(PLACEMENT_SO_KEY, dbPath);

        string dbPath2 = AssetDatabase.GetAssetPath(geoDb);
        EditorPrefs.SetString(PlacementGeometry_SO_KEY, dbPath2);

        string geoJson = AssetDatabase.GetAssetPath(geometryJson);
        EditorPrefs.SetString(PlacementGeometry_Json_KEY, geoJson);
    }


    public void CreatePlacementPrefabs()
    {
        if (string.IsNullOrEmpty(pathData.NewModelPath)) return;

        string[] modelFiles = Directory.GetFiles(pathData.NewModelPath, "*.*", SearchOption.AllDirectories);

        if (geoDb == null)
        {
            Debug.LogError("请指定 PlacementGeometry_SO");
            return;
        }

        geoDb.geometryList.Clear();

        foreach (string file in modelFiles)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext != ".fbx" && ext != ".obj") continue;

            string assetPath = file.Replace('\\', '/');
            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (modelAsset == null) continue;

            // 1. 实例化整个 FBX 资源到内存
            GameObject mainInstance = Instantiate(modelAsset);

            // 2. 找到下面所有的 MeshRenderer
            MeshRenderer[] allRenderers = mainInstance.GetComponentsInChildren<MeshRenderer>(true);

            for (int i = 0; i < allRenderers.Length; i++)
            {
                MeshRenderer targetRenderer = allRenderers[i];
                string meshName = targetRenderer.gameObject.name;

                EditorUtility.DisplayProgressBar("拆分导出中", $"正在处理 Mesh: {meshName}", (float)i / allRenderers.Length);

                // 3. 尺寸计算：使用原始 Mesh 的 Local Bounds 并结合当前物体的 LossyScale
                MeshFilter mf = targetRenderer.GetComponent<MeshFilter>();

                if (mf == null || mf.sharedMesh == null) continue;


                Bounds localBounds = mf.sharedMesh.bounds;
                Vector3 worldScale = targetRenderer.transform.lossyScale;

                Measure(mf, out var meshSize, out var intSize);
        
                Vector3 meshCenter = Vector3.Scale(localBounds.center, worldScale);

                // 4. 构建新 Prefab 结构
                GameObject newPrefabRoot = new GameObject(meshName);
                var placement = newPrefabRoot.AddComponent<PlacementRuntime>();


                Transform rootT = GetOrCreateChild(newPrefabRoot.transform, "Root");
                Transform meshContainer = GetOrCreateChild(rootT, "Mesh");
                Transform colliderContainer = GetOrCreateChild(rootT, "Collider");

                // 5. 复制 Mesh 物体：直接 Instantiate 目标 Renderer 所在的 GameObject
                // 这样它会自带原模型上的 MeshFilter, MeshRenderer 以及已经挂好的 Material
                GameObject singleMeshGO = Instantiate(targetRenderer.gameObject);
                singleMeshGO.name = meshName;
                singleMeshGO.transform.SetParent(meshContainer);

                // 将 Mesh 几何中心归零
                singleMeshGO.transform.localPosition = new Vector3(0, 0, 0);
                singleMeshGO.transform.localRotation = Quaternion.identity;
                singleMeshGO.transform.localScale = Vector3.one;

                // 6. 处理 Collider
                BoxCollider box = colliderContainer.gameObject.AddComponent<BoxCollider>();
                box.size = meshSize;
                box.center = new Vector3(0, meshSize.y / 2f, 0); // 底部对齐逻辑

                var navMeshObs = colliderContainer.gameObject.AddComponent<NavMeshObstacle>();
                navMeshObs.shape = NavMeshObstacleShape.Box;
                navMeshObs.size = box.size;
                navMeshObs.center = box.center;
                navMeshObs.carving = true; // 开启挖空，确保导航网格正确更新

                placement.gridData.length = intSize.x;
                placement.gridData.width = intSize.y;
                placement.gridData.height = intSize.z;

                // 7. 处理 Pivot (Root 偏移)
                rootT.localPosition = new Vector3(meshSize.x / 2f, 0, meshSize.z / 2f);

                // 整棵 Prefab 层级设为 Placement（含 Collider，便于射线/触发检测）
                SetLayerRecursively(newPrefabRoot.transform, GetPlacementLayer());

                // 8. 保存 Prefab (这会成为一个独立的、不依赖原始 FBX 层级的 Prefab)
                string saveName = meshName + ".prefab";
                string outPath = Path.Combine(pathData.OutPrefabPathNew, saveName).Replace('\\', '/');

                PrefabUtility.SaveAsPrefabAsset(newPrefabRoot, outPath);

                // 9. 记录到 SO
                geoDb.geometryList.Add(new PlacementGeometryData()
                {
                    modelName = meshName,
                    size = intSize,
                    prefabPath = outPath
                });

                DestroyImmediate(newPrefabRoot);
            }

            DestroyImmediate(mainInstance);
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.SetDirty(geoDb);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("拆分 Prefab 生成完毕！材质已保留原样。");
    }

    // 自动寻找贴图并创建材质的方法
    private void ApplyAutoMaterial(MeshRenderer renderer, string modelName)
    {
        // 规范：材质球放在 MaterialPath 下，命名为 M_模型名_Mesh名
        string matName = $"M_{modelName}_{renderer.gameObject.name}";
        string matPath = Path.Combine(pathData.MaterialPath, matName + ".mat").Replace('\\', '/');

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); // 假设使用 URP
            AssetDatabase.CreateAsset(mat, matPath);
        }

        // 自动寻找贴图：假设贴图命名包含模型名
        // 搜索 TexturePath 路径下包含 modelName 的贴图
        string[] texGuids = AssetDatabase.FindAssets($"{modelName} t:Texture2D", new[] { pathData.TexturePath });
        if (texGuids.Length > 0)
        {
            string texPath = AssetDatabase.GUIDToAssetPath(texGuids[0]);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            mat.SetTexture("_BaseMap", tex); // URP 默认贴图字段
        }

        renderer.sharedMaterial = mat;
    }

    public void ModifyFurniture()
    {
        if (db == null || db._placement_db == null) return;

        int totalCount = db._placement_db.Count;
        int currentIndex = 0;

        foreach (var info in db._placement_db)
        {
            currentIndex++;
            if (string.IsNullOrEmpty(info.res_url) || info.res_url.Contains("Mat")) continue;

            // 1. 更安全的路径和名称提取
            string normalizedUrl = info.res_url.Replace('\\', '/');
            string prefabName = Path.GetFileNameWithoutExtension(normalizedUrl) + ".prefab";
            string prefabPath = Path.Combine(pathData.OutPrefabPathOld, prefabName).Replace('\\', '/');

            // 显示进度条，防止处理大量数据时 Unity 假死
            EditorUtility.DisplayProgressBar("处理家具预制体", $"正在处理 ({currentIndex}/{totalCount}): {prefabName}", (float)currentIndex / totalCount);

            GameObject instance = null;

            try
            {
                // 2. 加载或创建实例并彻底 Unpack
                if (File.Exists(prefabPath))
                {
                    var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (sourcePrefab != null)
                    {
                        // 实例化到场景中，以便我们能执行 Unpack 动作
                        instance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);

                        // 【核心优化】：彻底解包，断开所有 Prefab Variant 和 嵌套 Prefab 的联系
                        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                    else
                    {
                        // 文件存在但加载失败（预制体已损坏）
                        Debug.LogWarning($"预制体 {prefabName} 已损坏，正在重新创建...");
                        AssetDatabase.DeleteAsset(prefabPath);
                        instance = new GameObject(Path.GetFileNameWithoutExtension(prefabName));
                    }
                }
                else
                {
                    instance = new GameObject(Path.GetFileNameWithoutExtension(prefabName));
                }

                // 3. 处理 PlacementRuntime 组件 (使用 TryGetComponent 更高效)
                if (!instance.TryGetComponent<PlacementRuntime>(out var plMono))
                {
                    plMono = instance.AddComponent<PlacementRuntime>();
                }
                plMono.info = info;

                // 清理无用子节点
                Cleanup(instance.transform);

                // 4. 确保层级结构
                Transform root = GetOrCreateChild(instance.transform, "Root");
                Transform meshT = GetOrCreateChild(root, "Mesh");
                Transform colliderT = GetOrCreateChild(root, "Collider");

                root.localPosition = new Vector3(info.length / 2f, 0, info.width / 2f);

                Unity_Tools.IdentityGameObject(meshT.gameObject);
                Unity_Tools.IdentityGameObject(colliderT.gameObject);

                // 5. 安全处理 Collider
                // 先清理掉该节点上所有的非 BoxCollider 的碰撞器
                var existingColliders = colliderT.GetComponents<Collider>();
                foreach (var col in existingColliders)
                {
                    if (col is not BoxCollider)
                    {
                        UnityEngine.Object.DestroyImmediate(col);
                    }
                }

                // 获取或添加 BoxCollider
                if (!colliderT.TryGetComponent<BoxCollider>(out var boxCol))
                {
                    boxCol = colliderT.gameObject.AddComponent<BoxCollider>();
                }

                boxCol.size = new Vector3(info.length, info.height, info.width);
                boxCol.center = new Vector3(0, info.height / 2f, 0);

                // 6. 处理 NavMeshObstacle
                if (!colliderT.TryGetComponent<NavMeshObstacle>(out var navMeshObs))
                {
                    navMeshObs = colliderT.gameObject.AddComponent<NavMeshObstacle>();
                }
                navMeshObs.shape = NavMeshObstacleShape.Box;
                navMeshObs.size = boxCol.size;
                navMeshObs.center = boxCol.center;

                // 7. 保存为全新的独立预制体（这会覆盖原文件，并作为一个全新的 Base Prefab 保存）
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理 {prefabName} 出错: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                // 8. 清理内存/场景中的临时实例
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("所有家具预制体处理完毕！");
    }

    public void MeasurePlacementGeo()
    {
        if (geoDb == null) return;
        var oldprefabs = LoadAllPrefabs(pathData.OutPrefabPathOld);
        var newprefabs = LoadAllPrefabs(pathData.OutPrefabPathNew);
        geoDb.geometryList.Clear();

        foreach (var prefab in oldprefabs)
        {
            MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            Measure(mf, out var _, out var intSize);
            geoDb.AddOrUpdateGeometryData(new PlacementGeometryData()
            {
                modelName = prefab.name,
                size = intSize,
                prefabPath = AssetDatabase.GetAssetPath(prefab)
            });
        }
        foreach (var prefab in newprefabs)
        {
            MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) continue;
            Measure(mf, out var _, out var intSize);
            geoDb.AddOrUpdateGeometryData(new PlacementGeometryData()
            {
                modelName = prefab.name,
                size = intSize,
                prefabPath = AssetDatabase.GetAssetPath(prefab)
            });
        }
        if (geometryJson != null)
        {
            string path = AssetDatabase.GetAssetPath(geometryJson);
            Save_Load_Tools.WriteIOFile(path, geoDb.ToJson());
        }

        Debug.Log("测量完成，数据已保存到 SO 和 Json 文件！共计 " + (oldprefabs.Length+newprefabs.Length).ToString()+"个");
    }

    public void MeasurePlacementGeoByExcelSO()
    {
        if (db == null || db._placement_db == null || geoDb == null) return;

        int totalCount = db._placement_db.Count;
        //geoDb.geometryList.Clear();

        for (int i = 0; i < totalCount; i++)
        {
            var info = db._placement_db[i];
            if (string.IsNullOrEmpty(info.res_url) || info.res_url.Contains("Mat")) continue;

            string normalizedUrl = info.res_url.Replace('\\', '/');
            string prefabName = Path.GetFileNameWithoutExtension(normalizedUrl) + ".prefab";
            string prefabPath = Path.Combine(pathData.OutPrefabPathOld, prefabName).Replace('\\', '/');

            EditorUtility.DisplayProgressBar("测量家具", $"正在处理: {prefabName}", (float)i / totalCount);

            if (!File.Exists(prefabPath)) continue;

            GameObject rootObj = null;
            try
            {
                // 【修正】必须加载 Prefab 内容才能进行修改和测量
                rootObj = PrefabUtility.LoadPrefabContents(prefabPath);

                // 确保层级
                Transform meshContainer = GetOrCreateChild(rootObj.transform, "Root").Find("Mesh");
                Transform colliderT = GetOrCreateChild(rootObj.transform, "Root").Find("Collider");

                // 【修正】在 MeshContainer 及其子物体中寻找 Mesh
                MeshFilter mf = meshContainer.GetComponentInChildren<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                {
                    Debug.LogWarning($"{prefabName} 找不到 MeshFilter，跳过测量");
                    continue;
                }



                // 尺寸计算
                Vector3 rawSize;
                Vector3Int intSize;
                Measure(mf, out rawSize, out intSize);

                if (rootObj.TryGetComponent<PlacementRuntime>(out var plMono))
                {
                    plMono.info = info;
                    plMono.info.height = intSize.y;
                }
                // 更新 Collider 和 NavMeshObstacle
                if (colliderT.TryGetComponent<BoxCollider>(out var boxCol))
                {
                    // 这里使用测量出的高度，长宽依然沿用 Excel 的 info 数据
                    boxCol.size = new Vector3(info.length, rawSize.y, info.width);
                    boxCol.center = new Vector3(0, rawSize.y / 2f, 0);

                    if (colliderT.TryGetComponent<NavMeshObstacle>(out var nav))
                    {
                        nav.size = boxCol.size;
                        nav.center = boxCol.center;
                    }
                }

                // 记录数据到 SO (转为整数方便策划查看)
                geoDb.geometryList.Add(new PlacementGeometryData()
                {
                    modelName = rootObj.name,
                    size = (Vector3)intSize, // 写入 SO 的是整数
                    prefabPath = prefabPath
                });

                // 保存修改
                PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"处理 {prefabName} 出错: {e.Message}");
            }
            finally
            {
                if (rootObj != null) PrefabUtility.UnloadPrefabContents(rootObj);
            }
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.SetDirty(geoDb);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("测量与高度修正完成！");
    }

    private static void Measure(MeshFilter mf, out Vector3 rawSize, out Vector3Int intSize)
    {
        Bounds localBounds = mf.sharedMesh.bounds;
        Vector3 worldScale = mf.transform.lossyScale; // 注意：取 Mesh 所在物体的缩放
        rawSize = new Vector3(
            localBounds.size.x * worldScale.x,
            localBounds.size.y * worldScale.y,
            localBounds.size.z * worldScale.z
        );

        // 【整数化处理】使用 RoundToInt 比较符合常规家具尺寸
        intSize = new Vector3Int(
            Mathf.RoundToInt(rawSize.x),
            Mathf.RoundToInt(rawSize.y),
            Mathf.RoundToInt(rawSize.z)
        );
    }

    /// <summary>与项目里 Layer 名称 "Placement" 对应，一般为索引 6。</summary>
    private static int GetPlacementLayer()
    {
        int layer = LayerMask.NameToLayer("Placement");
        if (layer < 0)
            layer = 6;
        return layer;
    }

    private static void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i), layer);
    }

    private Transform GetOrCreateChild(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t == null)
        {
            GameObject go = new GameObject(name);
            t = go.transform;
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }
        return t;
    }

    // 辅助：清空一个容器下的所有组件和子物体
    private void Cleanup(Transform TS)
    {
        // 删除所有子物体 (修复了原代码中重复调用 TS.GetChild(i) 的微小安全隐患)
        for (int i = TS.childCount - 1; i >= 0; i--)
        {
            var child = TS.GetChild(i).gameObject;
            if (child.name != "Root")
            {
                UnityEngine.Object.DestroyImmediate(child);
            }
        }
    }

    public static GameObject[] LoadAllPrefabs(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        GameObject[] prefabs = new GameObject[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        return prefabs;
    }

}