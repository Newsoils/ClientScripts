using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏主入口（热更完成后执行）
/// </summary>
public static class cs_main
{
    /// <summary>
    /// 加载主场景（或初始化场景）
    /// </summary>
    public static void LoadSceneInit()
    {
        Debug.Log("<color=yellow>[Game]</color> 开始加载游戏初始化场景...");

        // 如果你用Addressable，可以改成异步加载
        SceneManager.LoadSceneAsync("InitScene");
    }

    /// <summary>
    /// 进入游戏主逻辑入口（例如加载GameManager）
    /// </summary>
    public static void InitializeGame()
    {
        Debug.Log("<color=yellow>[Game]</color> 游戏主系统初始化中...");

        // 示例：初始化主系统
        GameObject root = new GameObject("[GameBootstrap]");
        Object.DontDestroyOnLoad(root);

        // 绑定核心系统
        //root.AddComponent<GameManager>();

        // 继续加载主场景或初始化界面
        SceneManager.LoadSceneAsync("MainScene");
    }
}
