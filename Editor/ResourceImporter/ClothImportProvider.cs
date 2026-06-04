using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.EditorTools.ResourceImporter
{
    public enum ClothSubMode
    {
        WizardImport,
        AddVariant,
        FileManager
    }

    public enum ClothWizardStep
    {
        ModelUpload = 0,
        InfoFill = 1,
        MaterialUpload = 2,
        TableGenerate = 3,
        ConfirmExecute = 4
    }

    public class ClothImportProvider
    {
        static readonly HashSet<string> AllowedClothSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "head", "body", "leg", "shoes", "decoration"
        };

        static readonly string[] ClothSlotOrder = { "head", "body", "leg", "shoes", "decoration" };

        static readonly Dictionary<int, string> FirstCategoryMap = new Dictionary<int, string>
        {
            [0] = "无", [1] = "上衣", [2] = "下装", [3] = "连体",
            [4] = "鞋袜", [5] = "配饰", [6] = "头饰", [7] = "套装"
        };

        static readonly Dictionary<int, string> SecondCategoryMap = new Dictionary<int, string>
        {
            [0] = "无", [301] = "裤子", [302] = "半裙",
            [501] = "项链", [502] = "背包", [503] = "特殊配饰",
            [601] = "帽子", [602] = "眼镜", [603] = "特殊头饰"
        };

        // ========== 导航与模式 ==========
        public ClothSubMode SubMode = ClothSubMode.WizardImport;
        public ClothWizardStep WizardStep = ClothWizardStep.ModelUpload;

        // ========== 步骤1：模型上传 ==========
        public List<ClothFbxEntry> ClothFbxEntries = new List<ClothFbxEntry>();
        public bool ClothScanned;
        public string ClothSuitName = "";

        // ========== 步骤2：信息填写 ==========
        public string WizardSuitName = "";
        public List<ClothRowDraft> WizardSlotConfigs = new List<ClothRowDraft>();

        // ========== 步骤3：材质上传 ==========
        public List<ClothMaterialEntry> WizardMaterials = new List<ClothMaterialEntry>();
        public bool MaterialScanned;

        // ========== 步骤4：表格预览 ==========
        public List<ClothRowDraft> WizardPreviewRows = new List<ClothRowDraft>();
        public List<GameItemDraft> WizardGameItemDrafts = new List<GameItemDraft>();
        public bool WizardAddToGameItem;
        public bool WizardGameItemExpanded;
        public int WizardNextClothId = 20100;

        // ========== 添加变体 ==========
        public string[] ExistingSuitNames = Array.Empty<string>();
        public int VariantSelectedSuitIndex;
        public string VariantColorKey = "";
        public string VariantChinesePrefix = "";
        public List<ExistingClothConfig> VariantBaseConfigs = new List<ExistingClothConfig>();
        public List<ClothMaterialEntry> VariantMaterials = new List<ClothMaterialEntry>();
        public bool VariantMaterialScanned;
        public bool VariantConfigsLoaded;
        string _lastVariantSuit = "";

        // ========== 通用 ==========
        public Action<string> OnLog;
        ResourceImporterWindow _window;

        public ClothImportProvider(ResourceImporterWindow window)
        {
            _window = window;
        }

        void Log(string msg) => OnLog?.Invoke(msg);

        // ==================== 入口 ====================

        public void DrawPanel(ImportMode mode)
        {
            EditorGUILayout.BeginHorizontal();

            // 左侧导航
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(140));
            DrawLeftNav();
            EditorGUILayout.EndVertical();

            // 右侧内容
            EditorGUILayout.BeginVertical();
            switch (SubMode)
            {
                case ClothSubMode.WizardImport:
                    DrawWizardImport();
                    break;
                case ClothSubMode.AddVariant:
                    DrawAddVariant();
                    break;
                case ClothSubMode.FileManager:
                    DrawFileManager();
                    break;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        // ==================== 左侧导航 ====================

        void DrawLeftNav()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("服装工具", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            var modes = new[] { ClothSubMode.WizardImport, ClothSubMode.AddVariant, ClothSubMode.FileManager };
            var labels = new[] { "➕ 服装导入", "📋 添加变体", "📁 文件管理" };

            for (int i = 0; i < modes.Length; i++)
            {
                var isActive = SubMode == modes[i];
                var bg = isActive ? "SelectionRect" : "GUI.skin.button";
                var prevBg = GUI.backgroundColor;
                if (isActive) GUI.backgroundColor = new Color(0.3f, 0.5f, 0.9f, 1f);

                if (GUILayout.Button(labels[i], GUILayout.Height(32)))
                {
                    SubMode = modes[i];
                }

                GUI.backgroundColor = prevBg;
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("提示", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(GetNavHint(), EditorStyles.wordWrappedMiniLabel);
        }

        string GetNavHint()
        {
            switch (SubMode)
            {
                case ClothSubMode.WizardImport:
                    return "向导式导入全新服装套装，分5步完成。";
                case ClothSubMode.AddVariant:
                    return "为已有套装新增颜色变体，只需上传新材质。";
                case ClothSubMode.FileManager:
                    return "快速打开常用资源文件夹。";
                default:
                    return "";
            }
        }

        // ==================== 服装导入向导 ====================

        void DrawWizardImport()
        {
            EditorGUILayout.LabelField($"【服装导入向导 - 步骤{(int)WizardStep + 1}：{GetStepName(WizardStep)}】", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);
            DrawStepIndicator();
            EditorGUILayout.Space(8);

            switch (WizardStep)
            {
                case ClothWizardStep.ModelUpload:
                    DrawStep_ModelUpload();
                    break;
                case ClothWizardStep.InfoFill:
                    DrawStep_InfoFill();
                    break;
                case ClothWizardStep.MaterialUpload:
                    DrawStep_MaterialUpload();
                    break;
                case ClothWizardStep.TableGenerate:
                    DrawStep_TableGenerate();
                    break;
                case ClothWizardStep.ConfirmExecute:
                    DrawStep_ConfirmExecute();
                    break;
            }
        }

        string GetStepName(ClothWizardStep step)
        {
            switch (step)
            {
                case ClothWizardStep.ModelUpload: return "模型上传";
                case ClothWizardStep.InfoFill: return "服装信息";
                case ClothWizardStep.MaterialUpload: return "材质上传";
                case ClothWizardStep.TableGenerate: return "表格生成";
                case ClothWizardStep.ConfirmExecute: return "确认执行";
                default: return "";
            }
        }

        void DrawStepIndicator()
        {
            var steps = new[] { "模型", "信息", "材质", "表格", "执行" };
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < steps.Length; i++)
            {
                var isCurrent = i == (int)WizardStep;
                var isPast = i < (int)WizardStep;
                var label = isCurrent ? $"▶ {steps[i]}" : isPast ? $"✓ {steps[i]}" : $"○ {steps[i]}";
                var style = isCurrent ? EditorStyles.boldLabel : EditorStyles.miniLabel;
                EditorGUILayout.LabelField(label, style, GUILayout.Width(58));
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawWizardNavButtons(bool canPrev, bool canNext, string nextLabel = "下一步")
        {
            EditorGUILayout.Space(12);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = canPrev;
            if (GUILayout.Button("上一步", GUILayout.Width(100), GUILayout.Height(28)))
            {
                WizardStep--;
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            GUI.enabled = canNext;
            if (GUILayout.Button(nextLabel, GUILayout.Width(100), GUILayout.Height(28)))
            {
                WizardStep++;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        // ==================== 步骤1：模型上传 ====================

        void DrawStep_ModelUpload()
        {
            EditorGUILayout.HelpBox(
                "步骤说明：\n" +
                "1. 将服装 FBX 文件拖入下方暂存目录\n" +
                "2. 点击「刷新扫描」检测文件\n" +
                "3. 确认 FBX 内部子物体命名正确：{款式名}_{部件名}_{槽位}_{材质ID}",
                MessageType.Info);

            EditorGUILayout.Space(4);
            DrawStagingFolderHint();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新扫描", GUILayout.Width(100), GUILayout.Height(28)))
            {
                ScanStaging();
            }
            if (GUILayout.Button("在文件夹中显示", GUILayout.Width(120), GUILayout.Height(28)))
            {
                OpenFolder(ResourceImporterWindow.GetStagingPath(ResourceType.Cloth));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            if (!ClothScanned)
            {
                EditorGUILayout.HelpBox("请先拖入 FBX 文件并点击扫描", MessageType.Info);
            }
            else if (ClothFbxEntries.Count == 0)
            {
                EditorGUILayout.HelpBox("暂存目录为空，未检测到 FBX 文件", MessageType.Warning);
            }
            else
            {
                DrawFbxList();

                EditorGUILayout.Space(4);
                var validSlots = ClothFbxEntries
                    .SelectMany(e => e.Children)
                    .Where(c => c.IsValid)
                    .Select(c => c.Slot)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"检测到款式名: {ClothSuitName}", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                var missing = ClothSlotOrder.Where(s => !validSlots.Contains(s)).ToList();
                if (missing.Count > 0)
                {
                    EditorGUILayout.HelpBox($"缺失槽位: {string.Join(", ", missing)}", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("所有标准槽位已齐全 ✓", MessageType.Info);
                }
            }

            bool canNext = ClothScanned && ClothFbxEntries.Count > 0 &&
                ClothFbxEntries.SelectMany(e => e.Children).Any(c => c.IsValid);
            DrawWizardNavButtons(false, canNext);
        }

        // ==================== 步骤2：信息填写 ====================

        void DrawStep_InfoFill()
        {
            // 首次进入时只生成一行默认草稿
            if (WizardSlotConfigs.Count == 0 && ClothFbxEntries.Count > 0)
            {
                WizardSuitName = ClothSuitName;
                var allSlots = ClothFbxEntries
                    .SelectMany(e => e.Children)
                    .Where(c => c.IsValid)
                    .Select(c => c.Slot)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => Array.IndexOf(ClothSlotOrder, s));

                WizardSlotConfigs.Add(new ClothRowDraft
                {
                    clothId = WizardNextClothId,
                    clothName = "",
                    suitName = WizardSuitName,
                    slotsOccupied = string.Join(",", allSlots),
                    colorKey = "default",
                    firstCategory = 0,
                    secondCategory = 0
                });
            }

            EditorGUILayout.HelpBox(
                "步骤说明：编辑 cloth_info 配置列表。每行对应一个服装道具，可占用任意槽位组合。",
                MessageType.Info);

            EditorGUILayout.Space(4);
            WizardSuitName = EditorGUILayout.TextField("服装款式名", WizardSuitName);
            EditorGUILayout.Space(4);

            var firstCatIds = GetCategoryIds(FirstCategoryMap);
            var firstCatLabels = firstCatIds.Select(id => FirstCategoryMap[id]).ToArray();
            var secondCatIds = GetCategoryIds(SecondCategoryMap);
            var secondCatLabels = secondCatIds.Select(id => SecondCategoryMap[id]).ToArray();

            for (int i = 0; i < WizardSlotConfigs.Count; i++)
            {
                var row = WizardSlotConfigs[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"配置 {i + 1}", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("删除", GUILayout.Width(50)) && WizardSlotConfigs.Count > 1)
                {
                    WizardSlotConfigs.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    i--;
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                row.clothName = EditorGUILayout.TextField("显示名", row.clothName);

                var fcIndex = GetCategoryIndex(FirstCategoryMap, row.firstCategory);
                var newFcIndex = EditorGUILayout.Popup("一级分类", fcIndex, firstCatLabels);
                if (newFcIndex != fcIndex) row.firstCategory = firstCatIds[newFcIndex];

                var scIndex = GetCategoryIndex(SecondCategoryMap, row.secondCategory);
                var newScIndex = EditorGUILayout.Popup("二级分类", scIndex, secondCatLabels);
                if (newScIndex != scIndex) row.secondCategory = secondCatIds[newScIndex];

                row.slotsOccupied = EditorGUILayout.TextField("占用槽位（逗号分隔）", row.slotsOccupied);

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ 增加一行"))
            {
                var last = WizardSlotConfigs.Count > 0 ? WizardSlotConfigs[WizardSlotConfigs.Count - 1] : null;
                WizardSlotConfigs.Add(new ClothRowDraft
                {
                    clothId = last != null ? last.clothId + 1 : WizardNextClothId,
                    suitName = WizardSuitName,
                    colorKey = "default",
                    firstCategory = last?.firstCategory ?? 0,
                    secondCategory = last?.secondCategory ?? 0
                });
            }
            EditorGUILayout.EndHorizontal();

            bool canNext = !string.IsNullOrWhiteSpace(WizardSuitName) &&
                WizardSlotConfigs.Count > 0 &&
                WizardSlotConfigs.All(r => !string.IsNullOrWhiteSpace(r.clothName));

            if (!canNext && WizardSlotConfigs.Count > 0)
            {
                EditorGUILayout.HelpBox("请为所有配置填写显示名", MessageType.Warning);
            }

            DrawWizardNavButtons(true, canNext);
        }

        // ==================== 步骤3：材质上传 ====================

        void DrawStep_MaterialUpload()
        {
            EditorGUILayout.HelpBox(
                "步骤说明：\n" +
                "1. 将材质(.mat)和贴图(.png/.jpg)拖入暂存目录\n" +
                "2. 材质命名格式：{款式名}_{材质ID}_{颜色}.mat  如 maid01_02_default.mat\n" +
                "3. 不上传材质也可以继续（使用默认材质或后续补充）",
                MessageType.Info);

            EditorGUILayout.Space(4);
            DrawStagingFolderHint();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新扫描", GUILayout.Width(100), GUILayout.Height(28)))
            {
                ScanMaterials();
            }
            if (GUILayout.Button("在文件夹中显示", GUILayout.Width(120), GUILayout.Height(28)))
            {
                OpenFolder(ResourceImporterWindow.GetStagingPath(ResourceType.Cloth));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            if (!MaterialScanned)
            {
                EditorGUILayout.HelpBox("请先上传材质文件并点击扫描", MessageType.Info);
            }
            else if (WizardMaterials.Count == 0)
            {
                EditorGUILayout.HelpBox("暂存目录未检测到材质或贴图文件", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField("扫描结果", EditorStyles.boldLabel);
                var mats = WizardMaterials.Where(m => !m.IsTexture).ToList();
                var texs = WizardMaterials.Where(m => m.IsTexture).ToList();

                if (mats.Count > 0)
                {
                    EditorGUILayout.LabelField("材质文件:", EditorStyles.miniBoldLabel);
                    foreach (var m in mats)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(m.FileName, GUILayout.MinWidth(200));
                        EditorGUILayout.LabelField($"colorKey: {m.ColorKey}", GUILayout.Width(120));
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (texs.Count > 0)
                {
                    EditorGUILayout.LabelField("贴图文件:", EditorStyles.miniBoldLabel);
                    foreach (var t in texs)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(t.FileName, GUILayout.MinWidth(200));
                        EditorGUILayout.LabelField($"colorKey: {t.ColorKey}", GUILayout.Width(120));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            DrawWizardNavButtons(true, true);
        }

        // ==================== 步骤4：表格生成 ====================

        void DrawStep_TableGenerate()
        {
            EditorGUILayout.HelpBox("步骤说明：预览将要生成的 cloth_info 配置。可选择同时添加到 game_item 表。", MessageType.Info);

            // 生成预览
            GeneratePreviewRows();
            SyncGameItemDrafts();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("cloth_info 预览", EditorStyles.boldLabel);

            if (WizardPreviewRows.Count == 0)
            {
                EditorGUILayout.HelpBox("无可预览数据", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                EditorGUILayout.LabelField("ID", EditorStyles.miniBoldLabel, GUILayout.Width(50));
                EditorGUILayout.LabelField("名称", EditorStyles.miniBoldLabel, GUILayout.Width(140));
                EditorGUILayout.LabelField("款式", EditorStyles.miniBoldLabel, GUILayout.Width(80));
                EditorGUILayout.LabelField("颜色", EditorStyles.miniBoldLabel, GUILayout.Width(60));
                EditorGUILayout.LabelField("槽位", EditorStyles.miniBoldLabel, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                foreach (var row in WizardPreviewRows)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(row.clothId.ToString(), GUILayout.Width(50));
                    EditorGUILayout.LabelField(row.clothName, GUILayout.Width(140));
                    EditorGUILayout.LabelField(row.suitName, GUILayout.Width(80));
                    EditorGUILayout.LabelField(row.colorKey, GUILayout.Width(60));
                    EditorGUILayout.LabelField(row.slotsOccupied, GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space(8);
            WizardAddToGameItem = EditorGUILayout.ToggleLeft("同时添加到 game_item 表（供背包/商店使用）", WizardAddToGameItem);

            if (WizardAddToGameItem)
            {
                EditorGUILayout.Space(4);
                WizardGameItemExpanded = EditorGUILayout.Foldout(WizardGameItemExpanded, "game_item 配置（* 为必填）");
                if (WizardGameItemExpanded)
                {
                    var itemTypes = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                    var itemTypeLabels = itemTypes.Select(t => t.ToString()).ToArray();

                    for (int i = 0; i < WizardGameItemDrafts.Count; i++)
                    {
                        var gi = WizardGameItemDrafts[i];
                        var correspondName = i < WizardPreviewRows.Count ? WizardPreviewRows[i].clothName : "";

                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"物品 {i + 1}  (对应: {correspondName})", EditorStyles.miniBoldLabel);

                        EditorGUILayout.BeginHorizontal();
                        gi.itemId = EditorGUILayout.IntField("item_id *", gi.itemId);
                        EditorGUILayout.EndHorizontal();

                        gi.itemName = EditorGUILayout.TextField("name *", gi.itemName);
                        gi.resUrl = EditorGUILayout.TextField("res_url (图标路径)", gi.resUrl);

                        var typeIndex = Array.IndexOf(itemTypes, gi.itemType);
                        if (typeIndex < 0) typeIndex = itemTypes.Length - 1; // default 12
                        var newTypeIndex = EditorGUILayout.Popup("type", typeIndex, itemTypeLabels);
                        gi.itemType = itemTypes[newTypeIndex];

                        gi.sellPrice = EditorGUILayout.IntField("sell_price", gi.sellPrice);
                        gi.rarity = EditorGUILayout.IntField("rarity", gi.rarity);

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(2);
                    }

                    // 必填检查
                    var missing = WizardGameItemDrafts.Where(g => g.itemId <= 0 || string.IsNullOrWhiteSpace(g.itemName)).ToList();
                    if (missing.Count > 0)
                    {
                        EditorGUILayout.HelpBox("game_item 中 item_id 和 name 为必填项", MessageType.Warning);
                    }
                }
            }

            bool canNext = true;
            if (WizardAddToGameItem && WizardGameItemExpanded)
            {
                canNext = WizardGameItemDrafts.All(g => g.itemId > 0 && !string.IsNullOrWhiteSpace(g.itemName));
            }

            DrawWizardNavButtons(true, canNext);
        }

        void SyncGameItemDrafts()
        {
            // 保持 game_item 草稿数量与 cloth_info 预览行数一致
            while (WizardGameItemDrafts.Count < WizardPreviewRows.Count)
            {
                var idx = WizardGameItemDrafts.Count;
                var row = WizardPreviewRows[idx];
                WizardGameItemDrafts.Add(new GameItemDraft
                {
                    itemId = row.clothId,
                    itemName = row.clothName,
                    itemType = 12
                });
            }
            while (WizardGameItemDrafts.Count > WizardPreviewRows.Count)
            {
                WizardGameItemDrafts.RemoveAt(WizardGameItemDrafts.Count - 1);
            }
        }

        // ==================== 步骤5：确认执行 ====================

        void DrawStep_ConfirmExecute()
        {
            EditorGUILayout.HelpBox("步骤说明：确认以下操作清单，点击「确认执行」完成导入。", MessageType.Info);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("执行清单", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("1. 移动 FBX 文件到 Resources/Models/Clothes/");
            EditorGUILayout.LabelField("2. 移动材质文件到 Resources/Materials/Clothes/");
            EditorGUILayout.LabelField("3. 移动贴图文件到 Resources/Textures/Texture_character/");
            EditorGUILayout.LabelField($"4. 写入 cloth_info.xlsx ({WizardPreviewRows.Count} 行)");
            if (WizardAddToGameItem)
                EditorGUILayout.LabelField($"5. 写入 game_item.xlsx ({WizardPreviewRows.Count} 行)");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = true;
            if (GUILayout.Button("上一步", GUILayout.Width(100), GUILayout.Height(28)))
            {
                WizardStep--;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("确认执行", GUILayout.Width(120), GUILayout.Height(36)))
            {
                Log("[服装导入] 执行功能待实现");
            }
            EditorGUILayout.EndHorizontal();
        }

        // ==================== 添加变体 ====================

        void DrawAddVariant()
        {
            EditorGUILayout.LabelField("【添加颜色变体】", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // 步骤1: 选择套装
            EditorGUILayout.LabelField("步骤1：选择套装", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            VariantSelectedSuitIndex = EditorGUILayout.Popup("套装", VariantSelectedSuitIndex, ExistingSuitNames.Length > 0 ? ExistingSuitNames : new[] { "(无)" });
            if (GUILayout.Button("刷新列表", GUILayout.Width(80)))
            {
                RefreshExistingSuits();
                VariantConfigsLoaded = false;
                _lastVariantSuit = "";
            }
            EditorGUILayout.EndHorizontal();

            if (ExistingSuitNames.Length == 0 || VariantSelectedSuitIndex >= ExistingSuitNames.Length)
            {
                EditorGUILayout.HelpBox("请先刷新列表并选择套装", MessageType.Info);
                return;
            }

            var suit = ExistingSuitNames[VariantSelectedSuitIndex];

            if (!VariantConfigsLoaded || _lastVariantSuit != suit)
            {
                var allConfigs = LoadClothConfigs(suit);
                VariantBaseConfigs = allConfigs
                    .Where(c => string.Equals(c.ColorKey, "default", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => Array.IndexOf(ClothSlotOrder, c.SlotsOccupied.Split(',')[0]))
                    .ToList();
                VariantConfigsLoaded = true;
                _lastVariantSuit = suit;
            }

            if (VariantBaseConfigs.Count == 0)
            {
                EditorGUILayout.HelpBox($"套装 [{suit}] 没有 default 变体配置，无法作为复制基础", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(8);

            // 步骤2: 填写信息
            EditorGUILayout.LabelField("步骤2：填写变体信息", EditorStyles.boldLabel);
            VariantColorKey = EditorGUILayout.TextField("新颜色变体 (colorKey)", VariantColorKey);
            VariantChinesePrefix = EditorGUILayout.TextField("中文名前缀", VariantChinesePrefix);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("将基于 [default] 变体复制以下配置:", EditorStyles.miniLabel);
            foreach (var cfg in VariantBaseConfigs)
            {
                var newName = string.IsNullOrEmpty(VariantChinesePrefix) ? cfg.ClothName : $"{VariantChinesePrefix}{cfg.ClothName}";
                EditorGUILayout.LabelField($"  {cfg.SlotsOccupied} → {newName}", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(8);

            // 步骤3: 上传材质
            EditorGUILayout.LabelField("步骤3：上传新材质", EditorStyles.boldLabel);
            DrawStagingFolderHint();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新扫描", GUILayout.Width(100), GUILayout.Height(28)))
            {
                ScanVariantMaterials(suit);
            }
            EditorGUILayout.EndHorizontal();

            if (VariantMaterialScanned)
            {
                if (VariantMaterials.Count == 0)
                {
                    EditorGUILayout.HelpBox("未检测到材质文件", MessageType.Info);
                }
                else
                {
                    foreach (var m in VariantMaterials)
                    {
                        EditorGUILayout.BeginHorizontal("box");
                        EditorGUILayout.LabelField(m.FileName);
                        EditorGUILayout.LabelField($"colorKey: {m.ColorKey}", GUILayout.Width(120));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUILayout.Space(8);

            // 步骤4: 确认执行
            EditorGUILayout.LabelField("步骤4：确认执行", EditorStyles.boldLabel);
            bool canExecute = !string.IsNullOrWhiteSpace(VariantColorKey) && VariantBaseConfigs.Count > 0;
            if (!canExecute)
            {
                EditorGUILayout.HelpBox("请填写颜色变体名称", MessageType.Warning);
            }

            if (GUILayout.Button("确认执行", GUILayout.Width(120), GUILayout.Height(36)))
            {
                Log("[添加变体] 执行功能待实现");
            }
        }

        // ==================== 文件管理 ====================

        void DrawFileManager()
        {
            EditorGUILayout.LabelField("【文件管理】", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox("快速打开常用资源文件夹", MessageType.Info);
            EditorGUILayout.Space(8);

            var folders = new (string label, string path)[]
            {
                ("📁 模型文件夹 (Clothes)", Path.Combine(Application.dataPath, "Resources", "Models", "Clothes")),
                ("📁 材质文件夹 (Clothes)", Path.Combine(Application.dataPath, "Resources", "Materials", "Clothes")),
                ("📁 贴图文件夹 (Texture_character)", Path.Combine(Application.dataPath, "Resources", "Textures", "Texture_character")),
                ("📁 服装暂存目录", ResourceImporterWindow.GetStagingPath(ResourceType.Cloth)),
            };

            foreach (var (label, path) in folders)
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField(label, GUILayout.MinWidth(200));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("打开", GUILayout.Width(60)))
                {
                    OpenFolder(path);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }

        // ==================== 扫描逻辑 ====================

        public void ScanStaging()
        {
            ClothFbxEntries.Clear();
            ClothScanned = true;
            var path = ResourceImporterWindow.GetStagingPath(ResourceType.Cloth);
            if (!Directory.Exists(path))
            {
                Log("暂存目录不存在");
                return;
            }

            var fbxFiles = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(path, "*.obj", SearchOption.AllDirectories))
                .Where(p => !p.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .ToList();

            int validChildrenCount = 0;
            int totalChildrenCount = 0;

            foreach (var abs in fbxFiles)
            {
                var assetPath = ResourceImporterWindow.ToAssetsPath(abs);
                if (string.IsNullOrEmpty(assetPath))
                {
                    ClothFbxEntries.Add(new ClothFbxEntry
                    {
                        FileName = Path.GetFileName(abs),
                        Children = new List<ClothChildEntry>
                        {
                            new ClothChildEntry { ChildName = "(无法转换为 Assets 路径)", IsValid = false, ErrorMessage = "路径不在 Assets 下" }
                        }
                    });
                    continue;
                }

                var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (fbxAsset == null)
                {
                    ClothFbxEntries.Add(new ClothFbxEntry
                    {
                        FileName = Path.GetFileName(abs),
                        Children = new List<ClothChildEntry>
                        {
                            new ClothChildEntry { ChildName = "(加载失败)", IsValid = false, ErrorMessage = "无法加载 FBX" }
                        }
                    });
                    continue;
                }

                var entry = new ClothFbxEntry { FileName = Path.GetFileName(abs) };
                foreach (Transform t in fbxAsset.GetComponentsInChildren<Transform>(true))
                {
                    if (t == fbxAsset.transform) continue;
                    var parsed = ParseClothFileName(t.gameObject.name);
                    parsed.ChildName = t.gameObject.name;
                    entry.Children.Add(parsed);
                    totalChildrenCount++;
                    if (parsed.IsValid) validChildrenCount++;
                }

                if (entry.Children.Count == 0)
                {
                    entry.Children.Add(new ClothChildEntry
                    {
                        ChildName = "(无子物体)",
                        IsValid = false,
                        ErrorMessage = "FBX 内无子物体"
                    });
                }

                ClothFbxEntries.Add(entry);
            }

            var firstValid = ClothFbxEntries
                .SelectMany(e => e.Children)
                .FirstOrDefault(c => c.IsValid && !string.IsNullOrEmpty(c.SuitName));
            if (firstValid != null)
                ClothSuitName = firstValid.SuitName;

            Log($"扫描完成，共 {fbxFiles.Count} 个 FBX，合法部件 {validChildrenCount} 个");
        }

        void ScanMaterials()
        {
            WizardMaterials.Clear();
            MaterialScanned = true;
            var path = ResourceImporterWindow.GetStagingPath(ResourceType.Cloth);
            if (!Directory.Exists(path))
            {
                Log("暂存目录不存在");
                return;
            }

            foreach (var f in Directory.GetFiles(path, "*.mat", SearchOption.AllDirectories))
            {
                if (f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                var fileName = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                var colorKey = ExtractColorKeyFromMaterialName(fileName);
                WizardMaterials.Add(new ClothMaterialEntry
                {
                    FileName = Path.GetFileName(f),
                    AssetPath = ResourceImporterWindow.ToAssetsPath(f),
                    ColorKey = colorKey,
                    IsTexture = false
                });
            }

            foreach (var ext in new[] { "*.png", "*.jpg", "*.jpeg", "*.tga" })
            {
                foreach (var f in Directory.GetFiles(path, ext, SearchOption.AllDirectories))
                {
                    if (f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                    var fileName = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                    var colorKey = ExtractColorKeyFromMaterialName(fileName);
                    WizardMaterials.Add(new ClothMaterialEntry
                    {
                        FileName = Path.GetFileName(f),
                        AssetPath = ResourceImporterWindow.ToAssetsPath(f),
                        ColorKey = colorKey,
                        IsTexture = true
                    });
                }
            }

            Log($"材质扫描完成，共 {WizardMaterials.Count} 个文件");
        }

        void ScanVariantMaterials(string suitName)
        {
            VariantMaterials.Clear();
            VariantMaterialScanned = true;
            var path = ResourceImporterWindow.GetStagingPath(ResourceType.Cloth);
            if (!Directory.Exists(path))
            {
                Log("暂存目录不存在");
                return;
            }

            var suitPrefix = suitName.ToLowerInvariant() + "_";
            foreach (var f in Directory.GetFiles(path, "*.mat", SearchOption.AllDirectories))
            {
                if (f.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                var fileName = Path.GetFileNameWithoutExtension(f).ToLowerInvariant();
                if (!fileName.StartsWith(suitPrefix)) continue;

                var colorKey = ExtractColorKeyFromMaterialName(fileName);
                VariantMaterials.Add(new ClothMaterialEntry
                {
                    FileName = Path.GetFileName(f),
                    AssetPath = ResourceImporterWindow.ToAssetsPath(f),
                    ColorKey = colorKey,
                    IsTexture = false
                });
            }

            Log($"变体材质扫描完成，共 {VariantMaterials.Count} 个文件");
        }

        string ExtractColorKeyFromMaterialName(string fileNameNoExt)
        {
            // 格式: suitName_matId_colorKey  或 suitName_colorKey
            var parts = fileNameNoExt.Split('_');
            if (parts.Length >= 3)
            {
                // 最后一段或最后多段作为 colorKey
                // 例如: maid01_02_default → default
                //       maid01_02_blueLine → blueLine
                return string.Join("_", parts.Skip(2));
            }
            if (parts.Length == 2)
                return parts[1];
            return "default";
        }

        void GeneratePreviewRows()
        {
            WizardPreviewRows.Clear();
            if (string.IsNullOrEmpty(WizardSuitName) || WizardSlotConfigs.Count == 0) return;

            var colorKeys = WizardMaterials.Where(m => !m.IsTexture).Select(m => m.ColorKey).Distinct().ToList();
            if (colorKeys.Count == 0) colorKeys.Add("default");

            int id = WizardNextClothId;
            foreach (var colorKey in colorKeys)
            {
                foreach (var cfg in WizardSlotConfigs)
                {
                    WizardPreviewRows.Add(new ClothRowDraft
                    {
                        clothId = id++,
                        clothName = cfg.clothName,
                        suitName = WizardSuitName,
                        colorKey = colorKey,
                        slotsOccupied = cfg.slotsOccupied,
                        firstCategory = cfg.firstCategory,
                        secondCategory = cfg.secondCategory
                    });
                }
            }
        }

        // ==================== UI 辅助 ====================

        void DrawFbxList()
        {
            foreach (var fbx in ClothFbxEntries)
            {
                var validChildren = fbx.Children.Where(c => c.IsValid).ToList();
                if (validChildren.Count == 0) continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(fbx.FileName, EditorStyles.boldLabel);

                foreach (var child in validChildren)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("  └─ " + child.ChildName, GUILayout.MinWidth(180));
                    EditorGUILayout.LabelField($"suit:{child.SuitName}", GUILayout.Width(80));
                    EditorGUILayout.LabelField($"slot:{child.Slot}", GUILayout.Width(60));
                    EditorGUILayout.LabelField($"mat:{child.MatId}", GUILayout.Width(40));
                    GUILayout.Label("✓", GUILayout.Width(20));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
        }

        public static void ApplyFbxImportSettings(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.useFileUnits)
            {
                importer.useFileUnits = false;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log($"[FBX导入设置] {assetPath}: useFileUnits=false");
            }
        }

        void DrawStagingFolderHint()
        {
            var path = ResourceImporterWindow.GetStagingPath(ResourceType.Cloth);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("暂存目录:", GUILayout.Width(60));
            EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();
        }

        // ==================== 文件解析 ====================

        public static ClothChildEntry ParseClothFileName(string childName)
        {
            var entry = new ClothChildEntry { ChildName = childName };
            var parts = childName.Split('_');
            if (parts.Length < 3)
            {
                entry.IsValid = false;
                entry.ErrorMessage = "命名需含至少3段";
                return entry;
            }

            entry.SuitName = parts[0];
            entry.PartName = parts[1];
            entry.Slot = parts[2];

            if (!AllowedClothSlots.Contains(entry.Slot))
            {
                entry.IsValid = false;
                entry.ErrorMessage = $"非法槽位:{entry.Slot}";
                return entry;
            }

            entry.MatId = parts.Length >= 4 ? parts[3] : "02";
            entry.IsValid = true;
            return entry;
        }

        // ==================== 配置加载 ====================

        public void RefreshExistingSuits()
        {
            var modelsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "Resources", "Models", "Clothes"));
            if (!Directory.Exists(modelsDir))
            {
                ExistingSuitNames = Array.Empty<string>();
                return;
            }

            var suits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var fbxFiles = Directory.GetFiles(modelsDir, "*.fbx", SearchOption.TopDirectoryOnly);

            foreach (var fbxPath in fbxFiles)
            {
                var assetPath = ResourceImporterWindow.ToAssetsPath(fbxPath);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (fbxAsset == null) continue;

                foreach (Transform t in fbxAsset.GetComponentsInChildren<Transform>(true))
                {
                    if (t == fbxAsset.transform) continue;
                    var parsed = ParseClothFileName(t.gameObject.name);
                    if (parsed.IsValid && !string.IsNullOrEmpty(parsed.SuitName))
                    {
                        suits.Add(parsed.SuitName);
                    }
                }
            }

            ExistingSuitNames = suits.OrderBy(s => s).ToArray();
            VariantSelectedSuitIndex = 0;
        }

        List<ExistingClothConfig> LoadClothConfigs(string suitName)
        {
            var result = new List<ExistingClothConfig>();
            var jsonPath = Path.Combine(Application.dataPath, "Resources", "Json", "project_mouse_tb_cloth_info.json");
            if (!File.Exists(jsonPath)) return result;

            try
            {
                var text = File.ReadAllText(jsonPath);
                var arr = JArray.Parse(text);
                foreach (var token in arr)
                {
                    if (!(token is JObject obj)) continue;
                    var sn = GetJString(obj, "suitName");
                    if (!string.Equals(sn, suitName, StringComparison.OrdinalIgnoreCase)) continue;

                    var slotsToken = obj["slotsOccupied"];
                    var slots = "";
                    if (slotsToken is JArray slotArr)
                        slots = string.Join(",", slotArr.Select(t => t.ToString()));
                    else
                        slots = slotsToken?.ToString() ?? "";

                    result.Add(new ExistingClothConfig
                    {
                        ClothId = GetJInt(obj, "clothId"),
                        ClothName = GetJString(obj, "clothName"),
                        SuitName = sn,
                        ColorKey = GetJString(obj, "colorKey"),
                        SlotsOccupied = slots,
                        FirstCategory = GetJInt(obj, "firstCategory"),
                        SecondCategory = GetJInt(obj, "secondCategory")
                    });
                }
            }
            catch (Exception e) { Log($"加载配置失败: {e.Message}"); }
            return result;
        }

        // ==================== 工具方法 ====================

        int[] GetCategoryIds(Dictionary<int, string> map)
        {
            return map.Keys.OrderBy(k => k).ToArray();
        }

        int GetCategoryIndex(Dictionary<int, string> map, int id)
        {
            var ids = GetCategoryIds(map);
            var idx = Array.IndexOf(ids, id);
            return idx >= 0 ? idx : 0;
        }

        static string GetJString(JObject obj, string key)
        {
            if (obj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token))
                return token?.ToString() ?? "";
            return "";
        }

        static int GetJInt(JObject obj, string key)
        {
            if (obj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out var token) && token != null)
            {
                if (int.TryParse(token.ToString(), out var val))
                    return val;
            }
            return 0;
        }

        void OpenFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Log($"目录不存在: {path}");
                return;
            }
            EditorUtility.RevealInFinder(path);
        }
    }
}
