using System;
using System.IO;
using System.Net;
using UnityEngine;

public static class HttpDownloadManager
{
    public static string DownloadPath = Path.Combine(Application.persistentDataPath, "HotPatch/") + "/";
    public static float DownloadProcessAt = 0f;
    private static WebClient currentClient;

    /// <summary>
    /// 开始下载文件
    /// </summary>
    /// <param name="baseUrl">服务器地址</param>
    /// <param name="fileName">文件名</param>
    /// <param name="speedMode">下载模式 (2=边玩边下, 6=全速下载)</param>
    /// <param name="md5">文件md5校验值</param>
    /// <param name="callback">(文件路径, 状态码)</param>
    public static void StartDownloadFile(string baseUrl, string fileName, int speedMode, string md5, Action<string, int> callback)
    {
        string fileUrl = $"{baseUrl}/{fileName}";
        string localPath = DownloadPath + fileName;

        try
        {
            if (!Directory.Exists(DownloadPath)) Directory.CreateDirectory(DownloadPath);
            if (File.Exists(localPath)) File.Delete(localPath);

            currentClient = new WebClient();
            currentClient.DownloadProgressChanged += (sender, e) =>
            {
                DownloadProcessAt = e.ProgressPercentage / 100f;
            };

            currentClient.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Error != null || e.Cancelled)
                {
                    callback?.Invoke(fileUrl, -1);
                }
                else
                {
                    callback?.Invoke(fileUrl, 1);
                }
                currentClient?.Dispose();
                currentClient = null;
            };

            currentClient.DownloadFileAsync(new Uri(fileUrl), localPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"下载文件失败: {ex}");
            callback?.Invoke(fileUrl, -1);
        }
    }

    /// <summary>
    /// 取消当前下载
    /// </summary>
    public static void CancelDownloadFile()
    {
        try
        {
            currentClient?.CancelAsync();
            currentClient?.Dispose();
            currentClient = null;
        }
        catch { }
    }
}
