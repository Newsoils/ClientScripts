using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CLIP.Framework_Core.Tools;

public class ResourcePreprocessWindow : EditorWindow
{
    // 路径配置
    private string spriteInput = "Assets/Art/UI";
    private string spriteOutput = "Assets/Extracted/Sprites";
    private string animInput = "Assets/Art/Animation";
    private string animOutput = "Assets/Extracted/Animations";

    [MenuItem("Tools/Resource/Resource Preprocess Tool")]
    public static void ShowWindow() => GetWindow<ResourcePreprocessWindow>("资源预处理");

    private void OnEnable()
    {
        spriteInput = EditorPrefs.GetString("RP_SpriteInput", spriteInput);
        spriteOutput = EditorPrefs.GetString("RP_SpriteOutput", spriteOutput);
        animInput = EditorPrefs.GetString("RP_AnimInput", animInput);
        animOutput = EditorPrefs.GetString("RP_AnimOutput", animOutput);
    }

    private void OnGUI()
    {
        // --- Sprite 提取部分 ---
        DrawSection("Sprite 提取 (从 Texture2D)", ref spriteInput, ref spriteOutput, ExtractSprites);

        EditorGUILayout.Space(10);
        Handles.DrawLine(new Vector2(0, GUILayoutUtility.GetLastRect().yMax), new Vector2(position.width, GUILayoutUtility.GetLastRect().yMax));
        EditorGUILayout.Space(10);

        // --- Animation 提取部分 ---
        DrawSection("Animation 提取 (从 FBX)", ref animInput, ref animOutput, ExtractAnimations);

        if (GUILayout.Button("保存所有路径配置", GUILayout.Height(30)))
        {
            SaveConfig();
        }
    }

    private void DrawSection(string title, ref string input, ref string output, System.Action action)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);
        input = EditorGUILayout.TextField("输入路径", input);
        output = EditorGUILayout.TextField("输出路径", output);

        if (GUILayout.Button($"立即执行 {title.Split(' ')[0]}"))
        {
            action.Invoke();
        }
    }

    private void SaveConfig()
    {
        EditorPrefs.SetString("RP_SpriteInput", spriteInput);
        EditorPrefs.SetString("RP_SpriteOutput", spriteOutput);
        EditorPrefs.SetString("RP_AnimInput", animInput);
        EditorPrefs.SetString("RP_AnimOutput", animOutput);
        Debug.Log("配置已保存");
    }

    // --- 提取逻辑 ---

    private void ExtractSprites()
    {
        EnsureFolder(spriteOutput);
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { spriteInput });
        int count = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>();
            foreach (var sprite in sprites)
            {
                string safeName = StringTools.ToSafeString(sprite.name); // 使用你之前的清洗工具
                string outPath = $"{spriteOutput}/{safeName}.asset";

                if (File.Exists(outPath)) continue;

                Sprite clone = Object.Instantiate(sprite);
                AssetDatabase.CreateAsset(clone, outPath);
                count++;
            }
        }
        FinalizeAssetDatabase(count, "Sprites");
    }

    private void ExtractAnimations()
    {
        EnsureFolder(animOutput);
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { animInput });
        int count = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.ToLower().EndsWith(".fbx")) continue;

            var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>();
            foreach (var clip in clips)
            {
                if (clip.name.StartsWith("__preview__")) continue;

                string safeName = StringTools.ToSafeString(Path.GetFileNameWithoutExtension(path) + "_" + clip.name);
                string outPath = $"{animOutput}/{safeName}.anim";

                if (File.Exists(outPath)) continue;

                AnimationClip clone = Object.Instantiate(clip);
                AssetDatabase.CreateAsset(clone, outPath);
                count++;
            }
        }
        FinalizeAssetDatabase(count, "Animations");
    }

    private void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }

    private void FinalizeAssetDatabase(int count, string type)
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"提取完成: 成功提取 {count} 个 {type}");
    }
}