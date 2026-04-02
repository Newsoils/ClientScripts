using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

// 1. 数据结构类：用于序列化保存路径列表
[System.Serializable]
public class FbxConfigData
{
    public List<string> targetPaths = new List<string>();
}

// 2. 编辑器窗口类：提供可视化配置界面
public class FbxImportConfigWindow : EditorWindow
{
    private static string PREFS_KEY = "FbxAutoConvertUnits_Config";
    private FbxConfigData data;
    private Vector2 scrollPos;

    // 在 Unity 顶部菜单栏创建入口
    [MenuItem("Tools/FBX 导入设置 (Auto Convert Units)")]
    public static void ShowWindow()
    {
        GetWindow<FbxImportConfigWindow>("FBX Config");
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnGUI()
    {
        GUILayout.Label("自动取消 Convert Units 的文件夹", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("在下列文件夹及其子文件夹内导入 FBX 时，将自动取消勾选 'Convert Units'。", MessageType.Info);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < data.targetPaths.Count; i++)
        {
            EditorGUILayout.LabelField(data.targetPaths[i]);
            EditorGUILayout.BeginHorizontal("box");

            // 显示当前路径
            string currentPath = data.targetPaths[i];

            // 为了方便，我们提供一个对象槽，让你可以直接把文件夹拖进去
            Object folderObj = AssetDatabase.LoadAssetAtPath<Object>(currentPath);
            Object newObj = EditorGUILayout.ObjectField(folderObj, typeof(DefaultAsset), false);

            // 如果拖入了新的文件夹，更新路径
            if (newObj != folderObj)
            {
                string newPath = AssetDatabase.GetAssetPath(newObj);
                // 简单的校验，确保是文件夹而不是普通文件
                if (Directory.Exists(newPath))
                {
                    data.targetPaths[i] = newPath;
                }
                else if (string.IsNullOrEmpty(newPath))
                {
                    // 如果被清空了
                    data.targetPaths[i] = "";
                }
            }

            // 如果路径为空（或者手动输入），显示文本框备用
            if (string.IsNullOrEmpty(data.targetPaths[i]))
            {
                data.targetPaths[i] = EditorGUILayout.TextField(data.targetPaths[i]);
            }

            // 删除按钮
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                data.targetPaths.RemoveAt(i);
                SaveData();
                i--; // 调整索引
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("添加新文件夹路径", GUILayout.Height(30)))
        {
            data.targetPaths.Add("");
            SaveData();
        }

        // 只要界面有变动（例如拖拽了物体），就保存
        if (GUI.changed)
        {
            SaveData();
        }
    }

    // --- 数据保存与读取逻辑 ---
    void LoadData()
    {
        if (EditorPrefs.HasKey(PREFS_KEY))
        {
            string json = EditorPrefs.GetString(PREFS_KEY);
            data = JsonUtility.FromJson<FbxConfigData>(json);
        }
        else
        {
            data = new FbxConfigData();
        }
    }

    void SaveData()
    {
        string json = JsonUtility.ToJson(data);
        EditorPrefs.SetString(PREFS_KEY, json);
    }
}

// 3. 资源导入处理器：实际干活的部分
public class FbxImportConfig : AssetPostprocessor
{
    private static string PREFS_KEY = "FbxAutoConvertUnits_Config";

    void OnPreprocessModel()
    {
        // 1. 基础检查：只处理 FBX
        if (Path.GetExtension(assetPath).ToLower() != ".fbx") return;

        // 2. 读取配置数据
        if (!EditorPrefs.HasKey(PREFS_KEY)) return; // 如果没配置过，直接跳过

        string json = EditorPrefs.GetString(PREFS_KEY);
        FbxConfigData data = JsonUtility.FromJson<FbxConfigData>(json);

        if (data == null || data.targetPaths == null) return;

        // 3. 检查当前文件路径是否包含在配置的文件夹中
        bool shouldProcess = false;
        foreach (var targetPath in data.targetPaths)
        {
            // 忽略空配置
            if (string.IsNullOrEmpty(targetPath)) continue;

            // 使用 StartsWith 确保是该文件夹下的文件 (例如: Assets/Art/Characters/Hero.fbx 匹配 Assets/Art/Characters)
            if (assetPath.StartsWith(targetPath))
            {
                shouldProcess = true;
                break;
            }
        }

        // 4. 执行修改逻辑
        if (shouldProcess)
        {
            ModelImporter importer = assetImporter as ModelImporter;
            if (importer != null)
            {
                // 如果当前已经是 false，就不需要重复设置（避免不必要的脏标记）
                if (importer.useFileScale == true)
                {
                    importer.useFileScale = false;
                    Debug.Log($"[Auto-Import] 已自动取消 Convert Units: <color=cyan>{assetPath}</color>");
                }
            }
        }
    }
}
