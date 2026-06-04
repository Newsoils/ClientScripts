using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.EditorTools.ResourceImporter
{
    public class SearchProvider
    {
        static readonly Dictionary<string, (string json, string xlsx)> TableMapping =
            new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["game_item"] = ("project_mouse_tb_game_item.json", "game_item.xlsx"),
            ["cloth_info"] = ("project_mouse_tb_cloth_info.json", "cloth_info.xlsx"),
            ["room_placement_info"] = ("project_mouse_tb_room_placement_info.json", "room_placement_info.xlsx"),
            ["pot_info"] = ("project_mouse_tb_pot_info.json", "pot_info.xlsx"),
            ["interact_info"] = ("project_mouse_tb_interact_info.json", "interact_info.xlsx"),
            ["plant_info"] = ("project_mouse_tb_plant_info.json", "plant_info.xlsx"),
            ["flower_pot_info"] = ("project_mouse_tb_flower_pot_info.json", "flower_pot_info.xlsx"),
            ["food_info"] = ("project_mouse_tb_food_info.json", "food_info.xlsx"),
            ["snack_info"] = ("project_mouse_tb_snack_info.json", "snack_info.xlsx"),
            ["tape_info"] = ("project_mouse_tb_tape_info.json", "tape_info.xlsx"),
            ["handheld_info"] = ("project_mouse_tb_handheld_info.json", "handheld_info.xlsx"),
            ["fertilizer_info"] = ("project_mouse_tb_fertilizer_info.json", "fertilizer_info.xlsx"),
            ["music_info"] = ("project_mouse_tb_music_info.json", "music_info.xlsx"),
            ["npc_info"] = ("project_mouse_tb_npc_info.json", "npc_info.xlsx"),
            ["gacha_pool"] = ("project_mouse_tb_gacha_pool.json", "gacha_pool.xlsx"),
            ["shopitem"] = ("project_mouse_tb_shopitem.json", "shop_item.xlsx"),
            ["item_group"] = ("project_mouse_tb_item_group.json", "item_group.xlsx"),
        };

        static readonly Dictionary<string, string[]> NameFieldMap =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["game_item"] = new[] { "item_name", "name" },
            ["cloth_info"] = new[] { "clothName", "cloth_name" },
            ["plant_info"] = new[] { "plantName", "plant_name" },
            ["flower_pot_info"] = new[] { "pot_name", "name" },
            ["room_placement_info"] = new[] { "room_placement_name", "name" },
            ["pot_info"] = new[] { "pot_name", "name" },
            ["interact_info"] = new[] { "interact_name", "name" },
            ["food_info"] = new[] { "food_name", "name" },
            ["snack_info"] = new[] { "snack_name", "name" },
            ["tape_info"] = new[] { "tape_name", "name" },
            ["handheld_info"] = new[] { "handheld_name", "name" },
            ["fertilizer_info"] = new[] { "fertilizer_name", "name" },
            ["music_info"] = new[] { "music_name", "name" },
            ["npc_info"] = new[] { "npc_name", "name" },
            ["gacha_pool"] = new[] { "pool_name", "name" },
            ["shopitem"] = new[] { "shopitem_name", "item_name", "name" },
            ["item_group"] = new[] { "group_name", "name" },
        };

        // ========== 状态 ==========
        public string SearchQuery = "";
        public List<FuzzyResult> FuzzyResults = new List<FuzzyResult>();
        public bool FuzzySearchPerformed;
        public string SelectedExactQuery = "";

        public List<SearchTableResult> TableResults = new List<SearchTableResult>();
        public List<SearchAssetResult> AssetResults = new List<SearchAssetResult>();
        public bool SearchPerformed;

        public Action<string> OnLog;
        Vector2 _fuzzyScroll;
        Vector2 _detailScroll;

        public SearchProvider(ResourceImporterWindow window) { }

        void Log(string msg) => OnLog?.Invoke(msg);

        public void ProcessPendingSearch() { }

        // ==================== UI ====================

        public void DrawPanel()
        {
            EditorGUILayout.LabelField("资源查找", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            SearchQuery = EditorGUILayout.TextField("物品名或 ID", SearchQuery);
            if (GUILayout.Button("搜索", GUILayout.Width(80)))
            {
                PerformFuzzySearch();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            if (!FuzzySearchPerformed)
            {
                EditorGUILayout.HelpBox("输入搜索词后点击搜索", MessageType.Info);
                return;
            }

            if (FuzzyResults.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到匹配", MessageType.Warning);
                return;
            }

            // 左右布局
            EditorGUILayout.BeginHorizontal();

            // 左侧：模糊结果
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(280), GUILayout.ExpandHeight(true));
            DrawFuzzyResults();
            EditorGUILayout.EndVertical();

            // 右侧：详情
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            DrawDetailPanel();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        void DrawFuzzyResults()
        {
            EditorGUILayout.LabelField($"搜索结果 ({FuzzyResults.Count})", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _fuzzyScroll = EditorGUILayout.BeginScrollView(_fuzzyScroll);
            foreach (var fr in FuzzyResults)
            {
                var isSelected = SelectedExactQuery == fr.QueryForExact;
                var bgStyle = isSelected ? "SelectionRect" : "box";

                EditorGUILayout.BeginVertical(bgStyle);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(fr.DisplayName, EditorStyles.boldLabel, GUILayout.MinWidth(120));
                EditorGUILayout.LabelField($"[{fr.TableName}] ID:{fr.IdValue}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("查看", GUILayout.Width(50), GUILayout.Height(36)))
                {
                    SelectedExactQuery = fr.QueryForExact;
                    PerformExactSearch(fr.QueryForExact);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawDetailPanel()
        {
            if (string.IsNullOrEmpty(SelectedExactQuery))
            {
                EditorGUILayout.HelpBox("点击左侧条目查看详情", MessageType.Info);
                return;
            }

            if (!SearchPerformed)
            {
                EditorGUILayout.HelpBox("正在加载...", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"[{SelectedExactQuery}] 详情", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // 配置表结果
            EditorGUILayout.LabelField($"配置表匹配（共 {TableResults.Count} 个表）", EditorStyles.boldLabel);
            if (TableResults.Count == 0)
            {
                EditorGUILayout.HelpBox("未在任何配置表中找到匹配", MessageType.Warning);
            }
            else
            {
                foreach (var table in TableResults)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{table.TableName}  —  {table.Rows.Count} 行", EditorStyles.boldLabel);
                    if (GUILayout.Button("打开表", GUILayout.Width(60)))
                        OpenTableFile(table.TableName);
                    EditorGUILayout.EndHorizontal();

                    foreach (var row in table.Rows)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(row.Summary, GUILayout.MinWidth(200));
                        if (!string.IsNullOrEmpty(row.JsonPath) && GUILayout.Button("打开Json", GUILayout.Width(70)))
                            OpenExternalFile(row.JsonPath);
                        EditorGUILayout.EndHorizontal();
                    }

                    // 关联物品
                    if (table.RelatedItems.Count > 0)
                    {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.LabelField("关联物品:", EditorStyles.miniLabel);
                        int perRow = 3;
                        for (int i = 0; i < table.RelatedItems.Count; i += perRow)
                        {
                            EditorGUILayout.BeginHorizontal();
                            for (int j = i; j < i + perRow && j < table.RelatedItems.Count; j++)
                            {
                                var rel = table.RelatedItems[j];
                                if (GUILayout.Button($"[{rel.RelationType}]{rel.DisplayName}", EditorStyles.miniButton))
                                {
                                    SelectedExactQuery = rel.TargetQuery;
                                    PerformExactSearch(rel.TargetQuery);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.Space(8);

            // 资源结果
            EditorGUILayout.LabelField($"引用资源（共 {AssetResults.Count} 个）", EditorStyles.boldLabel);
            if (AssetResults.Count == 0)
            {
                EditorGUILayout.HelpBox("未找到相关资源文件", MessageType.Info);
            }
            else
            {
                foreach (var asset in AssetResults)
                {
                    EditorGUILayout.BeginHorizontal("box");
                    EditorGUILayout.LabelField(asset.AssetType, GUILayout.Width(60));
                    EditorGUILayout.LabelField(asset.AssetPath, GUILayout.MinWidth(200));
                    if (GUILayout.Button("跳转", GUILayout.Width(50)))
                        PingAsset(asset.AssetPath);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        // ==================== 模糊搜索 ====================

        void PerformFuzzySearch()
        {
            FuzzyResults.Clear();
            SelectedExactQuery = "";
            FuzzySearchPerformed = true;
            SearchPerformed = false;
            TableResults.Clear();
            AssetResults.Clear();

            var query = SearchQuery.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                Log("请输入搜索词");
                return;
            }

            var queryLower = query.ToLowerInvariant();
            var jsonDir = Path.Combine(Application.dataPath, "Resources", "Json");
            if (!Directory.Exists(jsonDir))
            {
                Log("Json 目录不存在");
                return;
            }

            foreach (var kv in TableMapping)
            {
                var path = Path.Combine(jsonDir, kv.Value.json);
                if (!File.Exists(path)) continue;

                try
                {
                    var text = File.ReadAllText(path);
                    var arr = JArray.Parse(text);

                    NameFieldMap.TryGetValue(kv.Key, out var nameFields);
                    if (nameFields == null) nameFields = new[] { "name" };

                    foreach (var token in arr)
                    {
                        if (!(token is JObject obj)) continue;

                        // 提取名称
                        string displayName = "";
                        foreach (var nf in nameFields)
                        {
                            if (obj.TryGetValue(nf, StringComparison.OrdinalIgnoreCase, out var nameToken))
                            {
                                displayName = nameToken?.ToString() ?? "";
                                break;
                            }
                        }

                        // 提取ID（任意含id字段）
                        string idValue = "";
                        foreach (var prop in obj.Properties())
                        {
                            if (prop.Name.ToLowerInvariant().Contains("id"))
                            {
                                idValue = prop.Value.ToString();
                                break;
                            }
                        }

                        // 模糊匹配
                        if ((displayName ?? "").ToLowerInvariant().Contains(queryLower) ||
                            (idValue ?? "").ToLowerInvariant().Contains(queryLower))
                        {
                            FuzzyResults.Add(new FuzzyResult
                            {
                                TableName = kv.Key,
                                DisplayName = string.IsNullOrEmpty(displayName) ? "(无名称)" : displayName,
                                IdValue = idValue,
                                QueryForExact = displayName,
                                JsonPath = path
                            });
                        }
                    }
                }
                catch { }
            }

            Log($"模糊搜索完成，共 {FuzzyResults.Count} 条匹配");
        }

        // ==================== 精准搜索（复用原有逻辑）====================

        void PerformExactSearch(string exactQuery)
        {
            TableResults.Clear();
            AssetResults.Clear();
            SearchPerformed = true;

            if (string.IsNullOrWhiteSpace(exactQuery))
            {
                Log("搜索词为空");
                return;
            }

            var query = exactQuery.Trim();
            bool isIdQuery = int.TryParse(query, out var queryId);
            var jsonDir = Path.Combine(Application.dataPath, "Resources", "Json");
            if (!Directory.Exists(jsonDir))
            {
                Log("Json 目录不存在");
                return;
            }

            var matchedRows = new List<(string tableName, JObject obj)>();

            foreach (var kv in TableMapping)
            {
                var path = Path.Combine(jsonDir, kv.Value.json);
                if (!File.Exists(path)) continue;

                try
                {
                    var jsonText = File.ReadAllText(path);
                    var jArray = JArray.Parse(jsonText);
                    var tableResult = new SearchTableResult
                    {
                        TableName = kv.Key,
                        JsonPath = path,
                        XlsxPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..")), "External_Tools", "Luban", "Project_Mouse_Template", "Datas", kv.Value.xlsx)
                    };

                    foreach (var token in jArray)
                    {
                        if (!(token is JObject obj)) continue;
                        bool matched = false;

                        foreach (var prop in obj.Properties())
                        {
                            var val = prop.Value.ToString();
                            if (isIdQuery && int.TryParse(val, out var intVal) && intVal == queryId)
                            {
                                matched = true;
                                break;
                            }
                            if (!isIdQuery && string.Equals(val, query, StringComparison.OrdinalIgnoreCase))
                            {
                                matched = true;
                                break;
                            }
                        }

                        if (matched)
                        {
                            matchedRows.Add((kv.Key, obj));
                            tableResult.Rows.Add(new SearchRowResult
                            {
                                Summary = FormatRowSummary(obj),
                                JsonPath = path
                            });
                        }
                    }

                    if (tableResult.Rows.Count > 0)
                        TableResults.Add(tableResult);
                }
                catch { }
            }

            TraceResourcesFromRows(matchedRows);
            FindRelatedItems(matchedRows);
            Log($"精准搜索完成，表 {TableResults.Sum(t => t.Rows.Count)} 行，资源 {AssetResults.Count} 个，关联 {TableResults.Sum(t => t.RelatedItems.Count)} 个");
        }

        static string FormatRowSummary(JObject obj)
        {
            var idVal = "";
            var nameVal = "";
            foreach (var prop in obj.Properties())
            {
                var pn = prop.Name.ToLowerInvariant();
                var pv = prop.Value.ToString();
                if (pn.Contains("id") && string.IsNullOrEmpty(idVal))
                    idVal = pv;
                if (pn.Contains("name") && string.IsNullOrEmpty(nameVal))
                    nameVal = pv;
            }
            return $"ID:{idVal}  Name:{nameVal}";
        }

        // ==================== 资源追踪 ====================

        void TraceResourcesFromRows(List<(string tableName, JObject obj)> rows)
        {
            foreach (var (tableName, obj) in rows)
            {
                switch (tableName.ToLowerInvariant())
                {
                    case "cloth_info":
                        TraceClothResources(obj);
                        break;
                    case "room_placement_info":
                        TracePlacementResources(obj);
                        break;
                    case "pot_info":
                        TracePotResources(obj);
                        break;
                    case "game_item":
                        TraceGameItemResources(obj);
                        break;
                    case "plant_info":
                        TracePlantResources(obj);
                        break;
                }
            }
        }

        void TraceClothResources(JObject obj)
        {
            var suitName = GetJValue(obj, "suitName", "suit_name");
            var colorKey = GetJValue(obj, "colorKey", "color_key", "color");
            var resUrl = GetJValue(obj, "resUrl", "res_url");

            if (!string.IsNullOrEmpty(suitName))
            {
                var modelsDir = Path.Combine(Application.dataPath, "Resources", "Models", "Clothes");
                if (Directory.Exists(modelsDir))
                {
                    foreach (var f in Directory.GetFiles(modelsDir, "*.fbx", SearchOption.TopDirectoryOnly))
                    {
                        var assetPath = ResourceImporterWindow.ToAssetsPath(f);
                        if (string.IsNullOrEmpty(assetPath)) continue;
                        var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        if (fbxAsset == null) continue;

                        foreach (Transform t in fbxAsset.GetComponentsInChildren<Transform>(true))
                        {
                            if (t == fbxAsset.transform) continue;
                            var parsed = ClothImportProvider.ParseClothFileName(t.gameObject.name);
                            if (parsed.IsValid && string.Equals(parsed.SuitName, suitName, StringComparison.OrdinalIgnoreCase))
                            {
                                AddAssetUnique("模型", assetPath);
                                break;
                            }
                        }
                    }
                }

                var matsDir = Path.Combine(Application.dataPath, "Resources", "Materials", "Clothes");
                if (Directory.Exists(matsDir))
                {
                    foreach (var f in Directory.GetFiles(matsDir, "*.mat", SearchOption.TopDirectoryOnly))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(f);
                        if (fileName.StartsWith(suitName + "_", StringComparison.OrdinalIgnoreCase) &&
                            fileName.IndexOf(colorKey, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            AddAssetUnique("材质", ResourceImporterWindow.ToAssetsPath(f));
                        }
                    }
                }

                var texDir = Path.Combine(Application.dataPath, "Resources", "Textures", "Texture_character");
                if (Directory.Exists(texDir))
                {
                    foreach (var ext in new[] { "*.png", "*.jpg", "*.jpeg", "*.tga" })
                    {
                        foreach (var f in Directory.GetFiles(texDir, ext, SearchOption.TopDirectoryOnly))
                        {
                            var fileName = Path.GetFileNameWithoutExtension(f);
                            if (fileName.StartsWith(suitName + "_", StringComparison.OrdinalIgnoreCase) &&
                                fileName.IndexOf(colorKey, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                AddAssetUnique("贴图", ResourceImporterWindow.ToAssetsPath(f));
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(resUrl))
            {
                FindAndAddIcon(resUrl);
            }
            else
            {
                var clothName = GetJValue(obj, "clothName", "cloth_name");
                if (!string.IsNullOrEmpty(clothName))
                {
                    var giResUrl = FindGameItemResUrlByName(clothName);
                    if (!string.IsNullOrEmpty(giResUrl))
                        FindAndAddIcon(giResUrl);
                }
            }
        }

        void TracePlacementResources(JObject obj)
        {
            var resUrl = GetJValue(obj, "res_url", "resUrl");
            if (!string.IsNullOrEmpty(resUrl))
            {
                var prefabPath = Path.Combine(Application.dataPath, "Resources", resUrl.Replace('\\', Path.DirectorySeparatorChar) + ".prefab");
                if (File.Exists(prefabPath))
                {
                    AddAssetUnique("Prefab", ResourceImporterWindow.ToAssetsPath(prefabPath));
                }
            }
        }

        void TracePotResources(JObject obj)
        {
            var resUrl = GetJValue(obj, "resUrl", "res_url");
            if (!string.IsNullOrEmpty(resUrl))
            {
                var prefabPath = Path.Combine(Application.dataPath, "Resources", resUrl.Replace('\\', Path.DirectorySeparatorChar) + ".prefab");
                if (File.Exists(prefabPath))
                {
                    AddAssetUnique("Prefab", ResourceImporterWindow.ToAssetsPath(prefabPath));
                }
            }
        }

        void TraceGameItemResources(JObject obj)
        {
            var resUrl = GetJValue(obj, "res_url", "resUrl");
            if (!string.IsNullOrEmpty(resUrl))
            {
                FindAndAddIcon(resUrl);
            }
        }

        void TracePlantResources(JObject obj)
        {
            var modelKey = GetJValue(obj, "modelKey", "model_key");
            if (string.IsNullOrEmpty(modelKey)) return;

            var keyLower = modelKey.ToLowerInvariant();

            var plantModelDir = Path.Combine(Application.dataPath, "Resources", "Models", "Plant");
            if (Directory.Exists(plantModelDir))
            {
                foreach (var ext in new[] { "*.fbx", "*.FBX" })
                {
                    foreach (var f in Directory.GetFiles(plantModelDir, ext, SearchOption.TopDirectoryOnly))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                        if (fileName.StartsWith(keyLower + "_") || fileName == keyLower)
                        {
                            AddAssetUnique("植物模型", ResourceImporterWindow.ToAssetsPath(f));
                        }
                    }
                }
            }

            var plantMatDir = Path.Combine(Application.dataPath, "Resources", "Materials", "Plant");
            if (Directory.Exists(plantMatDir))
            {
                foreach (var f in Directory.GetFiles(plantMatDir, "*.mat", SearchOption.TopDirectoryOnly))
                {
                    var fileName = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                    if (fileName == keyLower || fileName.StartsWith(keyLower + "_"))
                    {
                        AddAssetUnique("植物材质", ResourceImporterWindow.ToAssetsPath(f));
                    }
                }
            }
        }

        // ==================== 关联物品 ====================

        void FindRelatedItems(List<(string tableName, JObject obj)> matchedRows)
        {
            foreach (var (tableName, obj) in matchedRows)
            {
                var table = TableResults.FirstOrDefault(t => t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                if (table == null) continue;

                switch (tableName.ToLowerInvariant())
                {
                    case "plant_info":
                        ExtractPlantRelatedItems(table, obj);
                        break;
                    case "cloth_info":
                        ExtractClothRelatedItems(table, obj);
                        break;
                }
            }
        }

        void ExtractPlantRelatedItems(SearchTableResult table, JObject obj)
        {
            var plantName = GetJValue(obj, "plantName", "plant_name");
            var plantSeed = GetJValue(obj, "plantSeed", "plant_seed");
            var harvestItems = obj["harvestItem"] as JArray;
            var plantVariants = obj["plantVariant"] as JArray;

            if (!string.IsNullOrEmpty(plantSeed))
                AddRelatedItemUnique(table, plantSeed, "种子", plantSeed);

            if (harvestItems != null)
            {
                foreach (var item in harvestItems)
                {
                    var name = item?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        AddRelatedItemUnique(table, name, "收获", name);
                }
            }

            if (plantVariants != null)
            {
                foreach (var item in plantVariants)
                {
                    var name = item?.ToString();
                    if (!string.IsNullOrEmpty(name) && !string.Equals(name, plantName, StringComparison.OrdinalIgnoreCase))
                        AddRelatedItemUnique(table, name, "变种", name);
                }
            }
        }

        void ExtractClothRelatedItems(SearchTableResult table, JObject obj)
        {
            var suitName = GetJValue(obj, "suitName", "suit_name");
            var clothName = GetJValue(obj, "clothName", "cloth_name");

            if (string.IsNullOrEmpty(suitName)) return;

            var jsonPath = Path.Combine(Application.dataPath, "Resources", "Json", "project_mouse_tb_cloth_info.json");
            if (!File.Exists(jsonPath)) return;

            try
            {
                var text = File.ReadAllText(jsonPath);
                var arr = JArray.Parse(text);
                foreach (var token in arr)
                {
                    if (!(token is JObject o)) continue;
                    var otherSuit = GetJValue(o, "suitName", "suit_name");
                    var otherName = GetJValue(o, "clothName", "cloth_name");
                    if (string.Equals(otherSuit, suitName, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(otherName, clothName, StringComparison.OrdinalIgnoreCase))
                    {
                        AddRelatedItemUnique(table, otherName, "同套装", otherName);
                    }
                }
            }
            catch { }
        }

        void AddRelatedItemUnique(SearchTableResult table, string displayName, string relationType, string targetQuery)
        {
            if (table.RelatedItems.Any(r => string.Equals(r.TargetQuery, targetQuery, StringComparison.OrdinalIgnoreCase)))
                return;
            table.RelatedItems.Add(new RelatedItemResult
            {
                DisplayName = displayName,
                RelationType = relationType,
                TargetQuery = targetQuery
            });
        }

        // ==================== 工具方法 ====================

        void FindAndAddIcon(string resUrl)
        {
            if (string.IsNullOrEmpty(resUrl)) return;
            var iconPath = Path.Combine(Application.dataPath, "Resources", resUrl.Replace('\\', Path.DirectorySeparatorChar));
            foreach (var ext in new[] { ".png", ".jpg", ".jpeg" })
            {
                var fullPath = iconPath + ext;
                if (File.Exists(fullPath))
                {
                    AddAssetUnique("UI图标", ResourceImporterWindow.ToAssetsPath(fullPath));
                    break;
                }
            }
        }

        string FindGameItemResUrlByName(string itemName)
        {
            var jsonPath = Path.Combine(Application.dataPath, "Resources", "Json", "project_mouse_tb_game_item.json");
            if (!File.Exists(jsonPath)) return "";
            try
            {
                var text = File.ReadAllText(jsonPath);
                var arr = JArray.Parse(text);
                foreach (var token in arr)
                {
                    if (!(token is JObject o)) continue;
                    var name = GetJValue(o, "item_name", "itemName");
                    if (string.Equals(name, itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        return GetJValue(o, "res_url", "resUrl");
                    }
                }
            }
            catch { }
            return "";
        }

        static string GetJValue(JObject obj, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (obj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token))
                    return token?.ToString() ?? "";
            }
            return "";
        }

        void AddAssetUnique(string assetType, string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            if (AssetResults.Any(a => string.Equals(a.AssetPath, assetPath, StringComparison.OrdinalIgnoreCase)))
                return;
            AssetResults.Add(new SearchAssetResult { AssetType = assetType, AssetPath = assetPath });
        }

        void OpenTableFile(string tableName)
        {
            if (!TableMapping.TryGetValue(tableName, out var files)) return;
            var xlsxPath = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..")), "External_Tools", "Luban", "Project_Mouse_Template", "Datas", files.xlsx);
            if (File.Exists(xlsxPath))
                OpenExternalFile(xlsxPath);
            else
                Log($"找不到表文件: {xlsxPath}");
        }

        static void OpenExternalFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            var uri = new System.Uri(path).AbsoluteUri;
            Application.OpenURL(uri);
        }

        void PingAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
            else
            {
                Log($"无法加载资源: {assetPath}");
            }
        }
    }
}
