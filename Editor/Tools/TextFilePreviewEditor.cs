using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[CustomEditor(typeof(DefaultAsset))]
public class UniversalTextFilePreviewEditor : Editor
{
    // 支持的扩展名（可自行增减）
    private readonly string[] _supportedExtensions = { ".txt", ".json", ".csv", ".md", ".xml", ".lua", ".ini" };
    private string _fileContent;
    private bool _isSupported;
    private string _formattedJson;
    private bool _isJson;

    private void OnEnable()
    {
        string assetPath = AssetDatabase.GetAssetPath(target);
        string ext = Path.GetExtension(assetPath).ToLower();
        _isSupported = System.Array.Exists(_supportedExtensions, e => e == ext);
        _isJson = ext == ".json";

        if (_isSupported && !string.IsNullOrEmpty(assetPath))
        {
            // 获取文件完整路径
            string fullPath = GetFullPath(assetPath);
            if (File.Exists(fullPath))
            {
                try
                {
                    _fileContent = File.ReadAllText(fullPath, Encoding.UTF8);
                    if (_isJson)
                    {
                        // 格式化 JSON 以便阅读
                        var parsed = JToken.Parse(_fileContent);
                        _formattedJson = parsed.ToString(Formatting.Indented);
                    }
                }
                catch (System.Exception e)
                {
                    _fileContent = $"读取文件失败: {e.Message}";
                    _formattedJson = _fileContent;
                }
            }
        }
    }

    private string GetFullPath(string assetPath)
    {
        // assetPath 形如 "Assets/Data/sample.txt"
        // 转为系统绝对路径
        return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
    }

    // 决定 Inspector 主区域如何绘制
    public override void OnInspectorGUI()
    {
        if (!_isSupported)
        {
            // 不是文本文件，使用默认绘制
            DrawDefaultInspector();
            return;
        }

        EditorGUILayout.LabelField("📄 文件内容预览", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (_isJson && _formattedJson != null)
        {
            // JSON 显示格式化的文本，并允许编辑保存
            EditorGUILayout.LabelField("JSON (可编辑)", EditorStyles.miniLabel);
            string newJson = EditorGUILayout.TextArea(_formattedJson, GUILayout.ExpandHeight(true));
            if (newJson != _formattedJson && Event.current.type == EventType.Repaint)
            {
                // 简单防抖：实际项目中可以用 Delayed 字段或手动保存按钮
                if (GUILayout.Button("保存修改"))
                {
                    SaveJsonContent(newJson);
                }
            }
            _formattedJson = newJson;
        }
        else
        {
            // 普通文本文件：只读预览（如需编辑可添加类似按钮）
            EditorGUILayout.LabelField("纯文本 (只读)", EditorStyles.miniLabel);
            EditorGUILayout.TextArea(_fileContent, GUILayout.ExpandHeight(true));
        }
    }

    private void SaveJsonContent(string formattedJson)
    {
        string assetPath = AssetDatabase.GetAssetPath(target);
        string fullPath = GetFullPath(assetPath);
        try
        {
            // 将美化后的 JSON 压缩回一行保存（保持原始格式可选）
            var parsed = JToken.Parse(formattedJson);
            string compactJson = parsed.ToString(Formatting.None);
            File.WriteAllText(fullPath, compactJson, Encoding.UTF8);
            AssetDatabase.Refresh();
            _fileContent = compactJson;
            _formattedJson = parsed.ToString(Formatting.Indented);
            Debug.Log($"已保存: {assetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存失败: {e.Message}");
        }
    }

    // 控制下方预览区是否显示
    public override bool HasPreviewGUI()
    {
        return _isSupported;
    }

    // 绘制预览区（下方区域）
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (!_isSupported) return;
        // 简单显示文件内容前 2000 字符
        string previewText = _isJson ? _formattedJson : _fileContent;
        if (previewText.Length > 2000)
            previewText = previewText.Substring(0, 2000) + "...\n(预览截断)";
        GUI.Label(r, previewText, EditorStyles.wordWrappedLabel);
    }
}