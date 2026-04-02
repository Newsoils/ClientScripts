using UnityEngine;
using UnityEditor;
using System.IO;

public class ExtractFbxAnimations
{
    private const string DefaultFolder = "Assets/Art/Animation";

    [MenuItem("Tools/Animation/Extract FBX Animations")]
    public static void Extract()
    {
        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { DefaultFolder });

        foreach (string guid in fbxGuids)
        {
            string fbxPath = AssetDatabase.GUIDToAssetPath(guid);

            // 只处理 fbx
            if (!fbxPath.EndsWith(".fbx"))
                continue;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

            foreach (Object asset in assets)
            {
                AnimationClip clip = asset as AnimationClip;

                if (clip == null)
                    continue;

                // Unity 内置的预览动画不要
                if (clip.name.StartsWith("__preview__"))
                    continue;

                string folder = Path.GetDirectoryName(fbxPath);
                string fbxName = Path.GetFileNameWithoutExtension(fbxPath);
                string animPath = Path.Combine(folder, fbxName + ".anim").Replace("\\", "/");

                // 已存在则跳过，避免覆盖
                if (File.Exists(animPath))
                {
                    Debug.Log($"[Skip] Animation already exists: {animPath}");
                    continue;
                }

                // 复制动画
                AnimationClip newClip = Object.Instantiate(clip);
                newClip.name = fbxName;

                AssetDatabase.CreateAsset(newClip, animPath);
                Debug.Log($"[Create] {animPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("FBX animation extraction finished.");
    }
}
