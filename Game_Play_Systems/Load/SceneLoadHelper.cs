using System;
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
        get { return SceneManager.GetActiveScene().name ==LoginScene; }
    }
    public static bool IsMainScene
    {
        get { return SceneManager.GetActiveScene().name == MainSceneName; }
    }

    public static bool IsDispatchScene
    {
        get { return SceneManager.GetActiveScene().name == DispatchSceneName; }
    }

    public static void Load_MainScene( Action<Scene> callback = null)
    {
        if (SceneManager.GetActiveScene().name != MainSceneName)
        {
            Project_Mouse_Resource_Management.load_scene_async(MainSceneName, (loadedScene) =>
            {
                callback?.Invoke(loadedScene);
            });
        }
    }

    public static void Load_DispatchScene(Action<Scene> callback = null)
    {
        if(SceneManager.GetActiveScene().name != DispatchSceneName)
        {
            Project_Mouse_Resource_Management.load_scene_async(DispatchSceneName, (loadedScene) =>
            {
                callback?.Invoke(loadedScene);
            } );
        }
    }

    public static void LoadLoginScene(Action<Scene> callback = null)
    {

        if (SceneManager.GetActiveScene().name != LoginScene)
        {
            Project_Mouse_Resource_Management.load_scene_async(LoginScene, (loadedScene) =>
            {
                callback?.Invoke(loadedScene);
            });
        }
    }

}
