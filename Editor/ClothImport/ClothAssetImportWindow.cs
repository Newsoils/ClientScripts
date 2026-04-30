using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CLIP.Project_Mouse.ENUM;
using UnityEditor;
using UnityEngine;

[Serializable]
public sealed class ClothImportRowPayload
{
    public int clothId;
    public string clothName;
    public int firstCategory;
    public int secondCategory;
    public string slotsOccupiedCsv;
    public string suitName;
    public string colorKey;
    public string gameItemDesc;
    public string resUrl;
    public int sellPrice;
    public int rarity;
    public string currencyUnit;
    public string galleryDesc;
    public bool canBePresent;
}

[Serializable]
public sealed class ClothImportJsonEnvelope
{
    public string clothInfoXlsx;
    public string gameItemXlsx;
    public ClothImportRowPayload[] rows;
}

[Serializable]
public sealed class ClothImportRowDraft
{
    public string clothName = "新服装";
    public int clothId = 20100;
    public Cloth_First_Category firstCategory = Cloth_First_Category.Top;
    public Cloth_Second_Category secondCategory = Cloth_Second_Category.None;
    public string slotsOccupiedCsv = "body";
    public string suitName = "newsuit01";
    public string colorKey = "default";
    public string gameItemDesc = "";
    public string resUrl = @"Sprites\Cloth\默认";
    public int sellPrice = -1;
    public int rarity = 1;
    public string currencyUnit = "";
    public string galleryDesc = "";
    public bool canBePresent;
}

/// <summary>傻瓜式服装资源导入：移动暂存目录内模型/材质/贴图，写 cloth_info + game_item 表，可选跑 Luban。</summary>
public sealed class ClothAssetImportWindow : EditorWindow
{
    const string HubFolder = "Assets/导入素材点这里";
    const string StagingFolderName = "把需要导入的素材放在这个文件夹（包括模型，材质，贴图）";
    static readonly string[] TextureExt = { ".png", ".jpg", ".jpeg", ".tga", ".tif", ".tiff", ".exr", ".bmp" };
    static readonly HashSet<string> AllowedSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "head", "body", "leg", "shoes", "decoration"
    };

    readonly List<ClothImportRowDraft> _rows = new List<ClothImportRowDraft> { new ClothImportRowDraft() };
    Vector2 _scroll;
    bool _moveFiles = true;
    bool _runLuban = true;
    string _log = "";

    [MenuItem("Tools/小苔屋/服装素材导入…")]
    public static void OpenFromMenu()
    {
        Open();
    }

    public static void Open()
    {
        var w = GetWindow<ClothAssetImportWindow>(true, "服装素材导入", true);
        w.minSize = new Vector2(520, 420);
    }

    void OnEnable()
    {
        EnsureHubFolders();
    }

    static void EnsureHubFolders()
    {
        if (!AssetDatabase.IsValidFolder(HubFolder))
            AssetDatabase.CreateFolder("Assets", "导入素材点这里");
        var staging = $"{HubFolder}/{StagingFolderName}";
        if (!AssetDatabase.IsValidFolder(staging))
            AssetDatabase.CreateFolder(HubFolder, StagingFolderName);
        AssetDatabase.Refresh();
    }

    static string StagingPhysicalPath =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "导入素材点这里", StagingFolderName));

    static string ProjectMouseRoot =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));

    static string LubanTemplateDir =>
        Path.Combine(ProjectMouseRoot, "External_Tools", "Luban", "Project_Mouse_Template");

    static string ClothImportPythonScript =>
        Path.Combine(LubanTemplateDir, "cloth_import_xlsx.py");

    static string ClothInfoXlsx =>
        Path.Combine(LubanTemplateDir, "Datas", "cloth_info.xlsx");

    static string GameItemXlsx =>
        Path.Combine(LubanTemplateDir, "Datas", "game_item.xlsx");

    static string LubanDllPath =>
        Path.Combine(ProjectMouseRoot, "External_Tools", "Luban", "Tools", "Luban", "Luban.dll");

    void OnGUI()
    {
        EditorGUILayout.LabelField("暂存目录（拖入模型 / 材质 / 贴图）", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(StagingPhysicalPath, GUILayout.Height(18));
        EditorGUILayout.Space(4);
        _moveFiles = EditorGUILayout.ToggleLeft("导入时移动文件到 Resources（无模型时仅材质+贴图亦可）", _moveFiles);
        _runLuban = EditorGUILayout.ToggleLeft("完成后尝试执行 Luban 导出并复制 Json（需已安装 dotnet 且存在 Luban.dll）", _runLuban);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("表格行（clothId 与 game_item.item_id 一致；可多行一次写入）", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        for (var i = 0; i < _rows.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"条目 {i + 1}", EditorStyles.miniBoldLabel);
            var r = _rows[i];
            r.clothName = EditorGUILayout.TextField("显示名 clothName", r.clothName);
            r.clothId = EditorGUILayout.IntField("ID（clothId = item_id）", r.clothId);
            r.firstCategory = (Cloth_First_Category)EditorGUILayout.EnumPopup("一级分类", r.firstCategory);
            r.secondCategory = (Cloth_Second_Category)EditorGUILayout.EnumPopup("二级分类", r.secondCategory);
            r.slotsOccupiedCsv = EditorGUILayout.TextField("占用槽位（逗号分隔）", r.slotsOccupiedCsv);
            r.suitName = EditorGUILayout.TextField("款式名 suitName（材质/模型前缀）", r.suitName);
            r.colorKey = EditorGUILayout.TextField("颜色 colorKey", r.colorKey);
            EditorGUILayout.LabelField("game_item 扩展", EditorStyles.miniLabel);
            r.gameItemDesc = EditorGUILayout.TextField("  desc", r.gameItemDesc);
            r.resUrl = EditorGUILayout.TextField("  res_url（商店/背包图标）", r.resUrl);
            r.sellPrice = EditorGUILayout.IntField("  sell_price（-1 不可售）", r.sellPrice);
            r.rarity = EditorGUILayout.IntField("  rarity", r.rarity);
            r.currencyUnit = EditorGUILayout.TextField("  currency_unit", r.currencyUnit);
            r.galleryDesc = EditorGUILayout.TextField("  gallery_desc", r.galleryDesc);
            r.canBePresent = EditorGUILayout.Toggle("  can_be_present", r.canBePresent);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("删除本行", GUILayout.Width(80)) && _rows.Count > 1)
            {
                _rows.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
                i--;
                continue;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ 增加一行"))
            _rows.Add(new ClothImportRowDraft());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);
        if (GUILayout.Button("确认导入", GUILayout.Height(36)))
            RunImport();

        if (!string.IsNullOrEmpty(_log))
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(_log, MessageType.Info);
        }
    }

    void RunImport()
    {
        _log = "";
        EnsureHubFolders();

        var ids = new HashSet<int>();
        foreach (var r in _rows)
        {
            if (r.clothId <= 0)
            {
                EditorUtility.DisplayDialog("服装导入", "clothId 必须为正整数。", "确定");
                return;
            }
            if (!ids.Add(r.clothId))
            {
                EditorUtility.DisplayDialog("服装导入", $"重复的 clothId：{r.clothId}", "确定");
                return;
            }
            if (string.IsNullOrWhiteSpace(r.clothName) || string.IsNullOrWhiteSpace(r.suitName) || string.IsNullOrWhiteSpace(r.colorKey))
            {
                EditorUtility.DisplayDialog("服装导入", "clothName、suitName、colorKey 不能为空。", "确定");
                return;
            }
            if (r.suitName.Contains("_"))
            {
                EditorUtility.DisplayDialog("服装导入", "suitName 内不要包含下划线（与部件命名规则冲突）。", "确定");
                return;
            }
            var slots = r.slotsOccupiedCsv.Replace("，", ",").Split(',').Select(s => s.Trim().ToLowerInvariant()).Where(s => s.Length > 0).ToList();
            if (slots.Count == 0)
            {
                EditorUtility.DisplayDialog("服装导入", "请至少填写一个槽位。", "确定");
                return;
            }
            foreach (var s in slots)
            {
                if (!AllowedSlots.Contains(s))
                {
                    EditorUtility.DisplayDialog("服装导入", $"非法槽位「{s}」，允许：head, body, leg, shoes, decoration", "确定");
                    return;
                }
            }
        }

        if (_moveFiles)
        {
            var staging = StagingPhysicalPath;
            if (!Directory.Exists(staging))
            {
                EditorUtility.DisplayDialog("服装导入", "暂存目录不存在。", "确定");
                return;
            }
            var moved = MoveStagingAssets(staging);
            if (moved < 0)
                return;
            if (moved == 0)
                EditorUtility.DisplayDialog("服装导入", "暂存目录内没有可识别的 .fbx / .mat / 贴图文件；仍将写入表格。", "确定");
        }

        if (!File.Exists(ClothImportPythonScript) || !File.Exists(ClothInfoXlsx) || !File.Exists(GameItemXlsx))
        {
            EditorUtility.DisplayDialog("服装导入", "未找到 cloth_import_xlsx.py 或 Luban Datas 下 xlsx，请检查仓库路径。", "确定");
            return;
        }

        var envelope = new ClothImportJsonEnvelope
        {
            clothInfoXlsx = ClothInfoXlsx.Replace('\\', '/'),
            gameItemXlsx = GameItemXlsx.Replace('\\', '/'),
            rows = _rows.Select(r => new ClothImportRowPayload
            {
                clothId = r.clothId,
                clothName = r.clothName.Trim(),
                firstCategory = (int)r.firstCategory,
                secondCategory = (int)r.secondCategory,
                slotsOccupiedCsv = r.slotsOccupiedCsv,
                suitName = r.suitName.Trim(),
                colorKey = r.colorKey.Trim(),
                gameItemDesc = string.IsNullOrEmpty(r.gameItemDesc) ? r.clothName.Trim() : r.gameItemDesc.Trim(),
                resUrl = string.IsNullOrEmpty(r.resUrl) ? @"Sprites\Cloth\默认" : r.resUrl.Trim(),
                sellPrice = r.sellPrice,
                rarity = r.rarity,
                currencyUnit = r.currencyUnit ?? "",
                galleryDesc = r.galleryDesc ?? "",
                canBePresent = r.canBePresent
            }).ToArray()
        };

        var jsonPath = Path.Combine(Application.temporaryCachePath, "cloth_import_payload.json");
        var json = JsonUtility.ToJson(envelope);
        File.WriteAllText(jsonPath, json, new UTF8Encoding(false));

        var py = FindPythonExecutable();
        if (py == null)
        {
            EditorUtility.DisplayDialog("服装导入", "未在 PATH 中找到 python，请先安装 Python 并勾选「Add to PATH」，或配置好 py 启动器。", "确定");
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = py,
            Arguments = $"\"{ClothImportPythonScript}\" \"{jsonPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = LubanTemplateDir
        };
        using (var p = Process.Start(psi))
        {
            if (p == null)
            {
                EditorUtility.DisplayDialog("服装导入", "无法启动 Python。", "确定");
                return;
            }
            var outp = p.StandardOutput.ReadToEnd();
            var err = p.StandardError.ReadToEnd();
            p.WaitForExit(120000);
            if (p.ExitCode != 0)
            {
                EditorUtility.DisplayDialog("服装导入", "写入 Excel 失败：\n" + err + outp, "确定");
                return;
            }
            _log = "Excel 已更新。\n" + outp.Trim();
        }

        if (_runLuban)
        {
            var lubanLog = TryRunLubanExport();
            _log += "\n" + lubanLog;
        }
        else
            _log += "\n已跳过 Luban；请自行双击 External_Tools/Luban/Project_Mouse_Template/双击点我运行.bat";

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("服装导入", "完成。请查看窗口底部日志；若未跑 Luban 请手动导出 Json。", "确定");
    }

    static string FindPythonExecutable()
    {
        foreach (var cmd in new[] { "python", "py" })
        {
            try
            {
                var args = cmd == "py" ? "-3 -c \"import sys; print(sys.executable)\"" : "-c \"import sys; print(sys.executable)\"";
                var psi = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p == null) continue;
                    var o = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(15000);
                    o = o.Trim();
                    if (p.ExitCode == 0 && !string.IsNullOrEmpty(o) && File.Exists(o))
                        return o;
                }
            }
            catch
            {
                // ignored
            }
        }
        return null;
    }

    string TryRunLubanExport()
    {
        if (!File.Exists(LubanDllPath))
            return "未找到 Luban.dll（External_Tools/Luban/Tools/Luban/Luban.dll），已跳过导出。请在本机放置 Luban 工具后双击 双击点我运行.bat。";

        var tpl = LubanTemplateDir;
        var jsonOut = Path.Combine(tpl, "Json");
        var args = new StringBuilder();
        args.Append('"').Append(LubanDllPath).Append('"');
        args.Append(" -t client -c cs-newtonsoft-json -d json --conf \"").Append(Path.Combine(tpl, "luban.conf")).Append('"');
        args.Append(" -x outputDataDir=Json -x outputCodeDir=output -x output:enumType=name");

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = args.ToString(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = tpl
        };
        try
        {
            using (var p = Process.Start(psi))
            {
                if (p == null) return "无法启动 dotnet。";
                var so = p.StandardOutput.ReadToEnd();
                var se = p.StandardError.ReadToEnd();
                p.WaitForExit(300000);
                if (p.ExitCode != 0)
                    return "Luban 导出失败：\n" + se + so;
            }
        }
        catch (Exception ex)
        {
            return "执行 dotnet 失败：" + ex.Message;
        }

        if (!Directory.Exists(jsonOut))
            return "Luban 未生成 Json 目录。";

        var clientJson = Path.Combine(ProjectMouseRoot, "Project_Mouse_3D_URP", "Assets", "Resources", "Json");
        var serverJson = Path.Combine(ProjectMouseRoot, "Server", "Json");
        RobocopyMirror(jsonOut, clientJson);
        if (Directory.Exists(serverJson))
            RobocopyMirror(jsonOut, serverJson);
        return "Luban 已导出并 robocopy 到客户端/服务端 Json。";
    }

    static void RobocopyMirror(string from, string to)
    {
        Directory.CreateDirectory(to);
        var psi = new ProcessStartInfo
        {
            FileName = "robocopy",
            Arguments = $"\"{from}\" \"{to}\" /E /NFL /NDL /NJH /NJS /nc /ns /np",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (var p = Process.Start(psi))
            p?.WaitForExit(300000);
    }

    /// <summary>返回移动文件数；-1 表示用户取消或出错。</summary>
    static int MoveStagingAssets(string stagingPhysical)
    {
        var modelsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "Resources", "Models", "Clothes"));
        var matsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "Resources", "Materials", "Clothes"));
        var texDir = Path.GetFullPath(Path.Combine(Application.dataPath, "Resources", "Textures", "Texture_character"));
        Directory.CreateDirectory(modelsDir);
        Directory.CreateDirectory(matsDir);
        Directory.CreateDirectory(texDir);

        var files = Directory.GetFiles(stagingPhysical, "*", SearchOption.AllDirectories)
            .Where(p => !p.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var count = 0;
        foreach (var abs in files)
        {
            var ext = Path.GetExtension(abs).ToLowerInvariant();
            string destDir;
            if (ext == ".fbx" || ext == ".obj")
                destDir = modelsDir;
            else if (ext == ".mat")
                destDir = matsDir;
            else if (TextureExt.Contains(ext))
                destDir = texDir;
            else
                continue;

            var name = Path.GetFileName(abs);
            var destAbs = Path.Combine(destDir, name);
            if (File.Exists(destAbs))
            {
                if (!EditorUtility.DisplayDialog("服装导入", $"目标已存在文件：{name}\n是否覆盖？", "覆盖", "取消整次导入"))
                    return -1;
            }

            var destAsset = ToAssetsPath(destAbs);
            var srcAsset = ToAssetsPath(abs);
            if (string.IsNullOrEmpty(destAsset) || string.IsNullOrEmpty(srcAsset))
                continue;

            var err = AssetDatabase.MoveAsset(srcAsset, destAsset);
            if (!string.IsNullOrEmpty(err))
            {
                EditorUtility.DisplayDialog("服装导入", $"移动失败：{err}\n{srcAsset} -> {destAsset}", "确定");
                return -1;
            }
            count++;

            if (ext == ".fbx")
            {
                var imp = AssetImporter.GetAtPath(destAsset) as ModelImporter;
                if (imp != null)
                {
                    imp.useFileScale = false;
                    imp.SaveAndReimport();
                }
            }
        }
        return count;
    }

    static string ToAssetsPath(string absolutePath)
    {
        var assetsRoot = Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar;
        var norm = Path.GetFullPath(absolutePath);
        if (!norm.StartsWith(assetsRoot, StringComparison.OrdinalIgnoreCase))
            return null;
        var tail = norm.Substring(assetsRoot.Length).Replace('\\', '/');
        return "Assets/" + tail;
    }
}
