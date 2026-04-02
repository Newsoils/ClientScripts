using System;
using System.Collections.Generic;
using System.IO;
using GameCoreResourceLoad;
using UnityEngine;

public static class GameServiceMgr
{
    // 模拟数据库中的文件信息列表 (文件名|md5)
    public static List<string> DbInfos = new List<string>();
    public static List<string> DbOldInfos = new List<string>();

    public static string HotComparePath => Path.Combine(HttpDownloadManager.DownloadPath, "HotCompare.db");
    public static string LocalHotComparePath => Path.Combine(ResourceMgr.BUNDLE_ABS_PATH, "HotCompare.db");

    /// <summary>
    /// 获取当前安装包版本
    /// </summary>
    public static string GetAppVersion()
    {
        string versionFile = Path.Combine(ResourceMgr.BUNDLE_ABS_PATH, "version.txt");
        return File.Exists(versionFile) ? File.ReadAllText(versionFile) : Application.version;
    }

    /// <summary>
    /// 获取新版本号（来自刚下载的HotCompare.db）
    /// </summary>
    public static string GetNewAppVersion()
    {
        if (!File.Exists(HotComparePath))
            return string.Empty;

        foreach (var line in File.ReadAllLines(HotComparePath))
        {
            if (line.StartsWith("Version="))
                return line.Split('=')[1].Trim();
        }

        return string.Empty;
    }

    /// <summary>
    /// 从HotCompare.db解析出文件列表
    /// </summary>
    public static string[] DbNewUpdateFiles
    {
        get
        {
            if (!File.Exists(HotComparePath)) return Array.Empty<string>();
            var lines = File.ReadAllLines(HotComparePath);
            var files = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains("|")) files.Add(line.Trim());
            }

            DbInfos = files;
            return files.ToArray();
        }
    }

    /// <summary>
    /// 从旧的HotCompare.db中解析文件列表
    /// </summary>
    public static string[] DbAppUpdateFiles
    {
        get
        {
            if (!File.Exists(LocalHotComparePath)) return Array.Empty<string>();
            var lines = File.ReadAllLines(LocalHotComparePath);
            var files = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains("|")) files.Add(line.Trim());
            }

            DbOldInfos = files;
            return files.ToArray();
        }
    }

    /// <summary>
    /// 获取下载服务器地址
    /// </summary>
    public static string GetDownloadAddress(string platform, string type)
    {
        // 实际上可能从配置文件或服务器接口拿到
        // 这里模拟返回
        return $"https://cdn.game.com/{platform}/{type}";
    }
}
