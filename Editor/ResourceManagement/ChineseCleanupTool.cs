using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

public class ChineseCleanupTool : EditorWindow
{
    private string targetFolder = "Assets/Resources/RuntimeAssets";

    // 常用汉字映射表
    private static Dictionary<char, string> pinyinMap = new Dictionary<char, string> {
        {'主', "zhu"}, {'角', "jiao"}, {'攻', "gong"}, {'击', "ji"}, {'特', "te"}, {'效', "xiao"},
        {'玩', "wan"}, {'家', "jia"}, {'纹', "wen"}, {'理', "li"}, {'贴', "tie"}, {'图', "tu"},
        {'人', "ren"}, {'物', "wu"}, {'场', "chang"}, {'景', "jing"}, {'界', "jie"}, {'面', "mian"},
        {'音', "yin"}, {'动', "dong"}, {'画', "hua"}, {'底', "di"}, {'板', "ban"},
        {'星',"Xing" }
        // 你可以在这里继续添加...
    };

    [MenuItem("Tools/Resource/Smart Chinese Cleanup")]
    public static void ShowWindow() => GetWindow<ChineseCleanupTool>("清理中文资源名");

    private void OnGUI()
    {
        targetFolder = EditorGUILayout.TextField("目标目录", targetFolder);

        EditorGUILayout.Space();

        if (GUILayout.Button("1. 扫描遗漏汉字", GUILayout.Height(30)))
        {
            ScanMissingCharacters();
        }

        if (GUILayout.Button("2. 开始重命名 (仅限已知汉字)", GUILayout.Height(30)))
        {
            CleanFolder();
        }
    }

    // 扫描逻辑：找出所有还没加进字典的汉字
    private void ScanMissingCharacters()
    {
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            Debug.LogError($"路径不存在或无效: {targetFolder}");
            return;
        }
        // 修改这一行：使用 t:Object 匹配所有类型的资源
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { targetFolder });
        HashSet<char> missingChars = new HashSet<char>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);

            foreach (char c in fileName)
            {
                // 如果是中文 且 字典里没有
                if (IsChinese(c) && !pinyinMap.ContainsKey(c))
                {
                    missingChars.Add(c);
                }
            }
        }

        if (missingChars.Count > 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"发现 {missingChars.Count} 个未识别汉字，请添加到字典中：\n");
            foreach (var c in missingChars)
            {
                sb.AppendLine($"{{'{c}', \"\"}},");
            }
            Debug.LogWarning(sb.ToString());
            EditorUtility.DisplayDialog("扫描完成", $"发现 {missingChars.Count} 个未识别汉字，请查看 Console 窗口并补充字典。", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("扫描完成", "所有汉字都已在字典中！可以放心重命名。", "太棒了");
        }
    }

    private void CleanFolder()
    {
        // 修改这一行：使用 t:Object 匹配所有类型的资源
        string[] guids = AssetDatabase.FindAssets("t:Object", new[] { targetFolder });

        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            Debug.LogError($"路径不存在或无效: {targetFolder}");
            return;
        }

        int count = 0;
        int skipCount = 0;

        foreach (var guid in guids)
        {
            string oldPath = AssetDatabase.GUIDToAssetPath(guid);
            if (Directory.Exists(oldPath)) continue;

            string fileName = Path.GetFileNameWithoutExtension(oldPath);
            if (!IsChineseInString(fileName)) continue;

            // 检查是否所有汉字都在字典里，如果有不在的，跳过该文件并提示
            if (HasUnrecognizedChinese(fileName))
            {
                Debug.LogError($"[跳过] 文件 \"{fileName}\" 包含未识别汉字，请先补全字典！");
                skipCount++;
                continue;
            }

            string newSafeName = GetSafeName(fileName, oldPath);
            string error = AssetDatabase.RenameAsset(oldPath, newSafeName);

            if (string.IsNullOrEmpty(error)) count++;
            else Debug.LogError($"重命名 {fileName} 失败: {error}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("清理完成", $"成功处理: {count} 个\n跳过(含未知汉字): {skipCount} 个", "确定");
    }

    private string GetSafeName(string originalName, string fullPath)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in originalName)
        {
            if (pinyinMap.TryGetValue(c, out string py)) sb.Append(py);
            else if (IsSafeChar(c)) sb.Append(c);
            else sb.Append("_");
        }

        string baseName = sb.ToString().ToLower();
        baseName = Regex.Replace(baseName, @"_+", "_").Trim('_');

        // 兜底：重名处理
        string directory = Path.GetDirectoryName(fullPath);
        string extension = Path.GetExtension(fullPath);
        string finalName = baseName;
        int counter = 1;
        while (File.Exists(Path.Combine(directory, finalName + extension)))
        {
            finalName = $"{baseName}_{counter}";
            counter++;
        }
        return finalName;
    }

    // 工具方法
    private bool IsChinese(char c) => c >= 0x4E00 && c <= 0x9FA5;
    private bool IsChineseInString(string s) => Regex.IsMatch(s, @"[\u4e00-\u9fa5]");
    private bool IsSafeChar(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';

    private bool HasUnrecognizedChinese(string s)
    {
        foreach (char c in s)
        {
            if (IsChinese(c) && !pinyinMap.ContainsKey(c)) return true;
        }
        return false;
    }
}