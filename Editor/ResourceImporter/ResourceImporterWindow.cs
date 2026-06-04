using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CLIP.Project_Mouse.EditorTools.ResourceImporter
{
    public class ResourceImporterWindow : EditorWindow
    {
        const string HubFolder = "Assets/导入素材点这里";

        static readonly string[] ResourceTypeNames = { "服装", "家具", "花盆", "交互", "查找" };
        static readonly string[] ModeNames = { "导入资源", "修改资源" };

        ResourceType _currentType = ResourceType.Cloth;
        ImportMode _currentMode = ImportMode.Import;

        Vector2 _scroll;
        string _log = "";

        ClothImportProvider _clothProvider;
        SearchProvider _searchProvider;

        [MenuItem("Tools/小苔屋/万能资源导入器")]
        public static void Open()
        {
            var w = GetWindow<ResourceImporterWindow>(false, "万能资源导入器", true);
            w.minSize = new Vector2(640, 480);
        }

        void OnEnable()
        {
            _clothProvider = new ClothImportProvider(this) { OnLog = msg => _log = msg };
            _searchProvider = new SearchProvider(this) { OnLog = msg => _log = msg };

            EnsureHubFolders();
            _clothProvider.RefreshExistingSuits();
        }

        void OnGUI()
        {
            DrawTabs();
            if (_currentType != ResourceType.Search && _currentType != ResourceType.Cloth)
                DrawModeToggle();
            EditorGUILayout.Space(4);

            // 在 BeginScrollView 之前处理 pending search，避免 GUILayout 布局不匹配
            if (_currentType == ResourceType.Search)
                _searchProvider.ProcessPendingSearch();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            switch (_currentType)
            {
                case ResourceType.Cloth:
                    _clothProvider.DrawPanel(_currentMode);
                    break;
                case ResourceType.Furniture:
                    DrawFurniturePanel();
                    break;
                case ResourceType.Pot:
                    DrawPotPanel();
                    break;
                case ResourceType.Interact:
                    DrawInteractPanel();
                    break;
                case ResourceType.Search:
                    _searchProvider.DrawPanel();
                    break;
            }
            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(_log))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(_log, MessageType.Info);
            }
        }

        // ==================== 通用 UI ====================

        void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < ResourceTypeNames.Length; i++)
            {
                var style = _currentType == (ResourceType)i
                    ? EditorStyles.toolbarButton
                    : EditorStyles.miniButton;
                if (GUILayout.Button(ResourceTypeNames[i], style, GUILayout.Height(28)))
                {
                    _currentType = (ResourceType)i;
                    _clothProvider.ClothScanned = false;
                    _log = "";
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawModeToggle()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _currentMode = (ImportMode)GUILayout.Toolbar((int)_currentMode, ModeNames, GUILayout.Width(240), GUILayout.Height(24));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public void DrawStagingFolder(ResourceType type)
        {
            var path = GetStagingPath(type);
            EditorGUILayout.LabelField("暂存目录（拖入素材到以下文件夹）", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(20));
            if (GUILayout.Button("打开文件夹", GUILayout.Width(90)))
            {
                EditorUtility.RevealInFinder(path);
            }
            if (GUILayout.Button("刷新扫描", GUILayout.Width(80)))
            {
                ScanCurrentType();
            }
            EditorGUILayout.EndHorizontal();
        }

        void ScanCurrentType()
        {
            switch (_currentType)
            {
                case ResourceType.Cloth:
                    _clothProvider.ScanStaging();
                    break;
                case ResourceType.Furniture:
                    _log = "[家具] 扫描功能待实现";
                    break;
                case ResourceType.Pot:
                    _log = "[花盆] 扫描功能待实现";
                    break;
                case ResourceType.Interact:
                    _log = "[交互] 无文件扫描（纯配置）";
                    break;
            }
        }

        // ==================== 通用工具 ====================

        public static void EnsureHubFolders()
        {
            if (!AssetDatabase.IsValidFolder(HubFolder))
                AssetDatabase.CreateFolder("Assets", "导入素材点这里");

            foreach (var sub in new[] { "服装", "家具", "花盆", "交互" })
            {
                var path = $"{HubFolder}/{sub}";
                if (!AssetDatabase.IsValidFolder(path))
                    AssetDatabase.CreateFolder(HubFolder, sub);
            }
            AssetDatabase.Refresh();
        }

        public static string GetStagingPath(ResourceType type)
        {
            var sub = type switch
            {
                ResourceType.Cloth => "服装",
                ResourceType.Furniture => "家具",
                ResourceType.Pot => "花盆",
                ResourceType.Interact => "交互",
                _ => "Other"
            };
            return Path.GetFullPath(Path.Combine(Application.dataPath, "导入素材点这里", sub));
        }

        public static string ToAssetsPath(string absolutePath)
        {
            var assetsRoot = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
            var norm = Path.GetFullPath(absolutePath);
            if (!norm.StartsWith(assetsRoot, StringComparison.OrdinalIgnoreCase))
                return null;
            var tail = norm.Substring(assetsRoot.Length).Replace('\\', '/');
            return "Assets/" + tail;
        }

        // ==================== 其他页签占位 ====================

        void DrawFurniturePanel()
        {
            DrawStagingFolder(ResourceType.Furniture);
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("家具导入界面开发中...\n\n预计功能：\n1. 拖入 FBX 自动拆分为 Prefab\n2. 自动测量 bounds 计算长/宽/高\n3. 添加 PlacementRuntime + Collider + NavMeshObstacle", MessageType.Info);
        }

        void DrawPotPanel()
        {
            DrawStagingFolder(ResourceType.Pot);
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox("花盆导入界面开发中...\n\n预计功能：\n1. 拖入 FBX 生成标准 Pot Prefab\n2. 自动调整 Root Pivot（最低点贴 y=0）\n3. 添加 Pot 组件 + Grid_Cube 碰撞体", MessageType.Info);
        }

        void DrawInteractPanel()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox("交互配置无文件资源，直接在下方表单填写", MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("interact_info 配置", EditorStyles.boldLabel);
            EditorGUILayout.TextField("交互名称 (interactName)", "");
            EditorGUILayout.TextField("动画 Bool 名 (animationBoolName)", "");
            EditorGUILayout.TextField("房间限制 (roomLimit)", "");
            EditorGUILayout.TextField("家具限制 (placementLimit)", "");
            EditorGUILayout.FloatField("最短交互时间", 10f);
            EditorGUILayout.FloatField("最长交互时间", 20f);
            EditorGUILayout.TextField("所需物品 (itemRequire)", "");
            EditorGUILayout.TextField("动画片段名 (stateName)", "");
            EditorGUILayout.Toggle("特殊触发", false);
            EditorGUILayout.TextField("特殊逻辑 (specialBehavior)", "");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
            if (GUILayout.Button("写入配置表", GUILayout.Height(36)))
            {
                _log = "[交互配置] 写入功能待实现";
            }
        }
    }
}
