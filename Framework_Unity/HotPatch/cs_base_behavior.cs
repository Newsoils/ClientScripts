using UnityEngine;

/// <summary>
/// 游戏启动前的通用设置行为
/// </summary>
public static class cs_base_behaviour
{
    /// <summary>
    /// 初始化游戏运行时基础设置
    /// </summary>
    public static void InitGameSettings()
    {
        // ✅ 基础系统设置
        Application.runInBackground = true;  // 后台继续运行（如热更新下载）
        //QualitySettings.vSyncCount = 0;      // 不依赖 VSync
        Application.targetFrameRate = cs_app_const.GameFrameRate;

        // ✅ 屏幕设置
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_ANDROID || UNITY_IOS
        // 移动端全屏模式
        Screen.fullScreen = true;
#endif

        // ✅ 调试输出设置
#if UNITY_EDITOR
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;  // 发布版本禁用日志
#endif

        // ✅ 初始化本地化系统
        //InitLocalization();

        // ✅ 其他可扩展系统（音频、存档路径、输入系统等）
        // e.g. AudioListener.volume = PlayerPrefs.GetFloat("volume", 1.0f);

        Debug.Log("<color=green>[Game Init]</color> 基础游戏设置初始化完成。");
    }

    private static void InitLocalization()
    {
        // 举例：读取当前系统语言或配置
        var lang = Application.systemLanguage.ToString();
        PlayerPrefs.SetString("SystemLanguage", lang);
    }
}
