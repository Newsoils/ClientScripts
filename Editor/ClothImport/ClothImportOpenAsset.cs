using UnityEditor;
using UnityEditor.Callbacks;

/// <summary>在 Project 窗口双击「打开服装导入面板.clothimport」打开导入窗口。</summary>
public static class ClothImportOpenAsset
{
    const string TriggerLeaf = "打开服装导入面板.clothimport";

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var path = AssetDatabase.GetAssetPath(instanceID);
        if (string.IsNullOrEmpty(path))
            return false;
        var n = path.Replace('\\', '/');
        if (!n.EndsWith(TriggerLeaf, System.StringComparison.Ordinal))
            return false;
        ClothAssetImportWindow.Open();
        return true;
    }
}
