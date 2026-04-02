//using System.Collections.Generic;
//using System.Linq;
//using CLIP.Project_Mouse.ENUM;
//using CLIP.Project_Mouse.Game_Play_System;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(Room_SO))]
//class Room_SO_Editor : Editor
//{
//    Room_SO _obj;

//    Transform roomRoot;

//    private float cellSize = 1f;

//    void OnEnable()
//    {
//        _obj = (Room_SO)target;
//    }
//    public override void OnInspectorGUI()
//    {
//        base.OnInspectorGUI();

//        // 分隔线，区分原生面板和自定义面板
//        GUILayout.Space(5);
//        //EditorGUILayout.LabelField("房间数据配置", EditorStyles.boldLabel);
//        // 定义一个大字体样式
//        GUIStyle bigLabelStyle = new GUIStyle(EditorStyles.boldLabel);
//        bigLabelStyle.fontSize = 16; // 设置你想要的字号
//        bigLabelStyle.normal.textColor = Color.white; // 也可以顺便改个颜色

//        // 使用自定义样式
//        EditorGUILayout.LabelField("房间数据配置", bigLabelStyle);

//        GUILayout.Space(5);
//        EditorGUILayout.LabelField("需要扫描的数据的根节点，扫描下面的Room.cs和GridLayerTag.cs来生成数据", EditorStyles.whiteLabel);
//        roomRoot = (Transform)EditorGUILayout.ObjectField("Room Root", roomRoot, typeof(Transform), true);

//        EditorGUILayout.LabelField("Grid大小", EditorStyles.whiteLabel);
//        cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);

//        GUILayout.Space(10);

//        EditorGUILayout.LabelField("扫描房间数据（根据Room.cs）", EditorStyles.whiteLabel);
//        if (GUILayout.Button("GetRoomData"))
//        {
//            GetRoomData();
//        }

//        EditorGUILayout.LabelField("测量房间长度（根据GridLayerTag.cs）", EditorStyles.whiteLabel);
//        if (GUILayout.Button("MeasureAndAddData"))
//        {
//            MeasureAndSaveData();
//        }


//        GUILayout.Space(10);
//        EditorGUILayout.LabelField("自动解析物体名并分配ID/UID（根据GridLayerTag.cs）", EditorStyles.whiteLabel);
//        if (GUILayout.Button("Auto Configure Tags"))
//        {
//            AutoConfigureTagsAndMeasure();
//        }

//        GUILayout.Space(10);
//        EditorGUILayout.LabelField("一键配置：自动解析物体名并分配ID/UID", EditorStyles.boldLabel);
//        if (GUILayout.Button("Auto Configure Tags & Measure"))
//        {
//            AutoConfigureTagsAndMeasure();
//        }

//        GUILayout.Space(10);
//        EditorGUILayout.LabelField("三合一按钮：一键处理房间信息", EditorStyles.boldLabel);
//        if (GUILayout.Button("GetRoomInfo& Auto Configure Tags & Measure"))
//        {
//            RunAll();
//        }

//        GUILayout.Space(30);
//        EditorGUILayout.LabelField("清除数据", bigLabelStyle);
//        EditorGUILayout.LabelField("清除房间数据", EditorStyles.whiteLabel);
//        if (GUILayout.Button("ClearRoomConfig"))
//        {
//            ClearRoomData();
//        }
//        EditorGUILayout.LabelField("清除长度数据", EditorStyles.whiteLabel);
//        if (GUILayout.Button("ClearSizeData"))
//        {
//            ClearSizeData();
//        }
//    }

//    void GetRoomData()
//    {
//        if (roomRoot == null)
//        {
//            Debug.LogError("请挂载roomRoot");
//            return;
//        }
//        var rooms = roomRoot.GetComponentsInChildren<Room>();
//        foreach (var room in rooms)
//        {
//            Debug.Log($"找到Room: {room.RoomName}，类型: {room.RoomType}");
//            _obj.roomConfigs.Add(new Room_SO.RoomConfig()
//            {
//                RoomName = room.RoomName,
//                RoomType = room.RoomType
//            });
//        }
//        EditorUtility.SetDirty(_obj);
//        AssetDatabase.SaveAssets();
//    }
//    private void AutoConfigureTags()
//    {
//        var allTags = roomRoot.GetComponentsInChildren<GridLayerTag>(true);

//        Dictionary<(Room, GridLayerType), int> idCounter = new();

//        var sortedTags = allTags.OrderBy(t => t.gameObject.name).ToList();

//        foreach (var tag in sortedTags)
//        {
//            Undo.RecordObject(tag, "Auto Configure GridLayerTag");

//            var parentRoom = tag.GetComponentInParent<Room>();
//            if (parentRoom == null)
//            {
//                Debug.LogError($"Tag {tag.name} 没有找到父 Room");
//                continue;
//            }

//            var layerType = ParseLayerType(tag.name);
//            if (layerType == GridLayerType.None)
//            {
//                Debug.LogWarning($"无法解析图层类型: {tag.name}");
//                continue;
//            }

//            tag.gridLayerType = layerType;
//            tag.roomName = parentRoom.RoomName;
//            tag.roomType = parentRoom.RoomType;

//            var key = (parentRoom, layerType);

//            if (!idCounter.ContainsKey(key))
//                idCounter[key] = 1;

//            tag.LayerID = idCounter[key]++;

//            tag.LayerName = $"{tag.roomName}_{layerType}_{tag.LayerID}";

//            if (string.IsNullOrEmpty(tag.LayerUID))
//            {
//                //tag.LayerUID = $"{parentRoom.roomUID}_{layerType}_{tag.LayerID}";
//                tag.LayerUID = System.Guid.NewGuid().ToString();
//            }

//            EditorUtility.SetDirty(tag);
//        }

//        Debug.Log($"自动配置完成，共处理 {allTags.Length} 个 GridLayerTag");
//    }
//    void MeasureAndSaveData()
//    {
//        if (roomRoot == null)
//        {
//            Debug.LogError("请挂载roomRoot");
//            return;
//        }

//        var layers = roomRoot.GetComponentsInChildren<GridLayerTag>();

//        foreach (var layerTag in layers)
//        {
//            Transform t = layerTag.transform;
//            Bounds bounds = CalculateBounds(t);

//            GetWidthAndHeight(out int width, out int height, layerTag);

//            _obj.sizes.Add(new Room_SO.LayerSize()
//            {
//                id = layerTag.LayerID,
//                layer_Name = layerTag.LayerName,
//                layer_UId = layerTag.LayerUID,
//                room_Name = layerTag.roomName,
//                room_Type = layerTag.roomType,
//                layerType = layerTag.gridLayerType,
//                width = width,
//                height = height
//            });
//            Debug.Log($"{layerTag.roomName}{layerTag.roomType}成功生成{layerTag.gridLayerType} -> {width} x {height}");
//        }

//        EditorUtility.SetDirty(_obj);
//        AssetDatabase.SaveAssets();
//    }

//    private GridLayerType ParseLayerType(string name)
//    {
//        name = name.ToLower();

//        if (name.Contains("ceiling")) return GridLayerType.Ceiling;
//        if (name.Contains("floor")) return GridLayerType.Floor;
//        if (name.Contains("surface")) return GridLayerType.Surface;

//        if (name.Contains("wall_n")) return GridLayerType.Wall_N;
//        if (name.Contains("wall_s")) return GridLayerType.Wall_S;
//        if (name.Contains("wall_w")) return GridLayerType.Wall_W;
//        if (name.Contains("wall_e")) return GridLayerType.Wall_E;

//        return GridLayerType.None;
//    }


//    void ClearRoomData()
//    {
//        _obj.roomConfigs.Clear();
//    }

//    void ClearSizeData()
//    {
//        _obj.sizes.Clear();
//    }


//    private void AutoConfigureTagsAndMeasure()
//    {

//        if (roomRoot == null)
//        {
//            Debug.LogError("请指定 Room Root");
//            return;
//        }
//        ClearSizeData();
//        AutoConfigureTags();
//        MeasureAndSaveData();
//    }
//    void RunAll()
//    {
//        if (roomRoot == null)
//        {
//            Debug.LogError("请挂载roomRoot");
//            return;
//        }
//        ClearRoomData();
//        ClearSizeData();
//        GetRoomData();
//        MeasureAndSaveData();
//    }

//    Bounds CalculateBounds(Transform root)
//    {
//        var renderers = root.GetComponentsInChildren<Renderer>();

//        if (renderers.Length == 0)
//            return new Bounds(root.position, Vector3.zero);

//        Bounds bounds = renderers[0].bounds;

//        foreach (var r in renderers)
//            bounds.Encapsulate(r.bounds);

//        return bounds;
//    }


//    public void GetWidthAndHeight(out int width, out int height, GridLayerTag layerTag)
//    {
//        Transform t = layerTag.transform;
//        Bounds bounds = CalculateBounds(t);

//        switch (layerTag.gridLayerType)
//        {
//            case GridLayerType.Floor:
//            case GridLayerType.Ceiling:
//                width = Mathf.RoundToInt(bounds.size.x / cellSize);
//                height = Mathf.RoundToInt(bounds.size.z / cellSize);
//                break;
//            case GridLayerType.Wall_N:
//            case GridLayerType.Wall_S:
//                width = Mathf.RoundToInt(bounds.size.x / cellSize);
//                height = Mathf.RoundToInt(bounds.size.y / cellSize);
//                break;

//            case GridLayerType.Wall_W:
//            case GridLayerType.Wall_E:
//                width = Mathf.RoundToInt(bounds.size.z / cellSize);
//                height = Mathf.RoundToInt(bounds.size.y / cellSize);
//                break;
//            default:
//                width = 0;
//                height = 0;
//                break;
//        }

//    }
//}