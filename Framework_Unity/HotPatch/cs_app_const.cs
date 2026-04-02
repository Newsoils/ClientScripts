using UnityEngine;

public static class cs_app_const
{
    public const bool OpenPlayMoreDownload = true; // 是否开启边玩边下功能
    public const int GameFrameRate = 60;
    public const string GameVersion = "1.0.0";
    public const string HotfixURL = "https://yourcdn.com/hotupdate/";
    public const string DefaultLanguage = "CN";

    public static readonly string PersistentPath = Application.persistentDataPath;
    public static readonly string StreamingAssetsPath = Application.streamingAssetsPath;
}