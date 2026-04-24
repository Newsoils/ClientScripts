using System.IO;
using UnityEditor;
using UnityEngine;

public static class EditorFolderPathField
{
    public static string Draw(string label, string path, string emptyMessage = "Drag a folder here or type a valid Assets path")
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();

        DefaultAsset folderObj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
        DefaultAsset newObj = (DefaultAsset)EditorGUILayout.ObjectField(folderObj, typeof(DefaultAsset), false);

        if (newObj != folderObj)
        {
            string newPath = AssetDatabase.GetAssetPath(newObj);
            if (newObj == null)
            {
                path = "";
            }
            else if (AssetDatabase.IsValidFolder(newPath))
            {
                path = newPath;
            }
        }

        if (GUILayout.Button("Reset", GUILayout.Width(50)))
        {
            path = "";
        }

        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(path))
        {
            EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(18));
        }
        else
        {
            EditorGUILayout.HelpBox(emptyMessage, MessageType.None);
        }

        EditorGUILayout.EndVertical();
        return path;
    }

    public static void EnsureFolderExists(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            return;

        Directory.CreateDirectory(folderPath);
        AssetDatabase.Refresh();
    }
}
