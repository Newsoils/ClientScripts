using UnityEngine;
using UnityEditor;
using System.IO;

public class ExtractFbxAnimations : EditorWindow
{
    [System.Serializable]
    public class FbxAnimationPathSetting
    {
        public string inputFolder = "Assets/Art/Animation";
        public string outputFolder = "Assets/Art/Animation";
    }

    private const string PREFS_KEY = "ExtractFbxAnimations.PathSetting";

    private FbxAnimationPathSetting pathData = new FbxAnimationPathSetting();

    [MenuItem("Tools/Animation/Extract FBX Animations")]
    public static void Open()
    {
        GetWindow<ExtractFbxAnimations>("FBX Animations");
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnGUI()
    {
        GUILayout.Label("Extract FBX Animations", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        pathData.inputFolder = EditorFolderPathField.Draw("Input Folder", pathData.inputFolder);
        pathData.outputFolder = EditorFolderPathField.Draw("Output Folder", pathData.outputFolder);
        if (EditorGUI.EndChangeCheck())
        {
            SaveData();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Save Settings"))
        {
            SaveData();
            ShowNotification(new GUIContent("Settings saved"));
        }

        if (GUILayout.Button("Extract Animations", GUILayout.Height(30)))
        {
            Extract();
        }
    }

    private void LoadData()
    {
        if (!EditorPrefs.HasKey(PREFS_KEY))
            return;

        string json = EditorPrefs.GetString(PREFS_KEY);
        JsonUtility.FromJsonOverwrite(json, pathData);
    }

    private void SaveData()
    {
        EditorPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(pathData));
    }

    private void Extract()
    {
        if (string.IsNullOrEmpty(pathData.inputFolder) || !AssetDatabase.IsValidFolder(pathData.inputFolder))
        {
            Debug.LogError("Input folder is invalid: " + pathData.inputFolder);
            return;
        }

        if (string.IsNullOrEmpty(pathData.outputFolder))
        {
            Debug.LogError("Output folder is empty.");
            return;
        }

        EditorFolderPathField.EnsureFolderExists(pathData.outputFolder);

        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { pathData.inputFolder });

        int extractedCount = 0;

        foreach (string guid in fbxGuids)
        {
            string fbxPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!fbxPath.EndsWith(".fbx"))
                continue;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

            foreach (Object asset in assets)
            {
                AnimationClip clip = asset as AnimationClip;

                if (clip == null)
                    continue;

                if (clip.name.StartsWith("__preview__"))
                    continue;

                string fbxName = Path.GetFileNameWithoutExtension(fbxPath);
                string animPath = Path.Combine(pathData.outputFolder, fbxName + ".anim").Replace("\\", "/");

                if (File.Exists(animPath))
                {
                    Debug.Log($"[Skip] Animation already exists: {animPath}");
                    continue;
                }

                AnimationClip newClip = Object.Instantiate(clip);
                newClip.name = fbxName;

                AssetDatabase.CreateAsset(newClip, animPath);
                extractedCount++;
                Debug.Log($"[Create] {animPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"FBX animation extraction finished. Total created: {extractedCount}");
    }
}
