using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class TextureSpriteExtractorWindow : EditorWindow
{
    [System.Serializable]
    public class TextureSpritePathSetting
    {
        public string inputFolder = "Assets";
        public string outputFolder = "Assets/ExtractedSprites";
    }

    private const string PREFS_KEY = "TextureSpriteExtractorWindow.PathSetting";

    private TextureSpritePathSetting pathData = new TextureSpritePathSetting();

    [MenuItem("Tools/Sprite/Extract Sprites From Texture")]
    public static void Open()
    {
        GetWindow<TextureSpriteExtractorWindow>("Sprite Extractor");
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void OnGUI()
    {
        GUILayout.Label("Extract Sprites From Texture2D", EditorStyles.boldLabel);
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

        if (GUILayout.Button("Extract Sprites", GUILayout.Height(30)))
        {
            ExtractSprites();
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

    private void ExtractSprites()
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

        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { pathData.inputFolder });

        int extractedCount = 0;

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            var sprites = assets.OfType<Sprite>().ToList();
            if (sprites.Count == 0)
                continue;

            foreach (Sprite sprite in sprites)
            {
                string spritePath = Path.Combine(
                    pathData.outputFolder,
                    sprite.name + ".asset"
                ).Replace("\\", "/");

                if (AssetDatabase.LoadAssetAtPath<Sprite>(spritePath) != null)
                {
                    Debug.LogWarning($"Sprite already exists, skipped: {spritePath}");
                    continue;
                }

                Sprite spriteCopy = Object.Instantiate(sprite);
                AssetDatabase.CreateAsset(spriteCopy, spritePath);
                extractedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Sprite extraction completed. Total extracted: {extractedCount}");
    }
}
