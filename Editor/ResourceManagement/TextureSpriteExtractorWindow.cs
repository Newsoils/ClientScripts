using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

public class TextureSpriteExtractorWindow : EditorWindow
{
    private string inputFolder = "Assets";
    private string outputFolder = "Assets/ExtractedSprites";

    [MenuItem("Tools/Sprite/Extract Sprites From Texture")]
    public static void Open()
    {
        GetWindow<TextureSpriteExtractorWindow>("Sprite Extractor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Extract Sprites From Texture2D", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        DrawFolderField("Input Folder", ref inputFolder);
        DrawFolderField("Output Folder", ref outputFolder);

        EditorGUILayout.Space();

        if (GUILayout.Button("Extract Sprites", GUILayout.Height(30)))
        {
            ExtractSprites();
        }
    }

    private void DrawFolderField(string label, ref string path)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(90));
        path = EditorGUILayout.TextField(path);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string selected = EditorUtility.OpenFolderPanel(label, "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                {
                    path = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "Please select a folder inside Assets.",
                        "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ExtractSprites()
    {
        if (!AssetDatabase.IsValidFolder(inputFolder))
        {
            Debug.LogError("Input folder is invalid: " + inputFolder);
            return;
        }

        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            AssetDatabase.Refresh();
        }

        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { inputFolder });

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
                    outputFolder,
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
