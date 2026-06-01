using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using UnityEngine.SceneManagement;

public class SceneLoadHelper : SingletonMono<SceneLoadHelper>
{
    public static string DispatchSceneName = "Dispatch_Demo";
    public static string MainSceneName = "MainScene";
    public static string LoginScene = "LoginScene";

    public static bool IsLoginScene
    {
        get { return SceneManager.GetActiveScene().name == LoginScene; }
    }
    public static bool IsMainScene
    {
        get { return SceneManager.GetActiveScene().name == MainSceneName; }
    }

    public static bool IsDispatchScene
    {
        get { return SceneManager.GetActiveScene().name == DispatchSceneName; }
    }

    public static void Load_MainScene(Action<Scene> callback = null)
    {
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            LoadSceneAsync(MainSceneName, (loadedScene) =>
            {
                callback?.Invoke(loadedScene);
            });
        }
    }

    public static void Load_DispatchScene(Action<Scene> callback = null)
    {
        if (SceneManager.GetActiveScene().name != DispatchSceneName)
        {
            LoadSceneAsync(DispatchSceneName, (loadedScene) =>
            {
                callback?.Invoke(loadedScene);
            });
        }
    }

    public static void LoadLoginScene(Action<Scene> callback = null)
    {

        if (SceneManager.GetActiveScene().name != LoginScene)
        {
            LoadSceneAsync(LoginScene, (loadedScene) =>
            {
                callback?.Invoke(loadedScene);
            });
        }
    }

    public static UnityEngine.AsyncOperation LoadSceneAsync(string name)
    {
        return SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
    }

    public static void LoadSceneAsync(string path, Action<Scene> _callback)
    {
        var async_loader = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
        async_loader.completed += (async_operation) =>
        {
            if (async_loader.isDone)
            {
                Scene loadedScene = SceneManager.GetSceneByName(path);
                _callback(loadedScene);
            }
        };
    }


}
