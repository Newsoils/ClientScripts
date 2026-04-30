using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CLIP.NewSoil
{
    /// <summary>
    /// 游戏启动器（统一入口）
    /// 自动创建核心系统，并初始化非Mono系统。
    /// 放在任意Assembly中都会在BeforeSceneLoad时执行。
    /// </summary>
    public class GameBootstrap :MonoBehaviour
    {
        /// <summary>
        /// 编辑器摸索下是从哪个场景启动，1login，2main
        /// </summary>
        public static int activeSceneIndex = 1;

        /// <summary>
        /// 游戏启动时执行
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StartGame()
        {
#if UNITY_EDITOR
            //这里是为了让编辑器模式下 运行任意场景正确的加载资源
            //如果当前不是Boot场景，记录下之前场景的buildIndex， 先加载到Boot场景，加载完资源之后再切到之前的场景
            //如果是不考虑这些，发布版本直接在这里Initialize()即可
            //这里是为了让编辑器模式下 运行任意场景正确的加载资源
            //如果当前不是Boot场景，记录下之前场景的buildIndex， 先加载到Boot场景，加载完资源之后再切到之前的场景
            //如果是不考虑这些，发布版本直接在这里Initialize()即可

            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());


            if (SceneManager.GetActiveScene().name != "Boot")
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                var currentScene = SceneManager.GetActiveScene();
                activeSceneIndex = currentScene.buildIndex;
                SceneManager.LoadScene("Boot");
            }
            else
            {
                activeSceneIndex = 1;
            }
#endif
            //Initialize();
        }

        //需要在Boot场景中挂载这个脚本，执行初始化
        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "Boot")
                Initialize();
        }

        private static async void Initialize()
        {
            Log.Custom("=== [GameBootstrap] Initializing ===","GameBoot", Color.cyan);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 根节点
            GameObject root = new GameObject("[GameBootstrap]");
            DontDestroyOnLoad(root);

            // -------------------------------
            // 1️⃣ 自动创建所有核心Mono系统
            // -------------------------------
            CreateOrFindSystem<AssetLoader>(root);
            CreateOrFindSystem<GameAssets>(root);
            CreateOrFindSystem<UIManager>(root);
            CreateOrFindSystem<ObjectPool>(root);
            CreateOrFindSystem<SceneLoadingHelper>(root);
            //CreateOrFindSystem<PersistentObjectManager>(root);

            // 初始化 AssetManager
            AssetLoader.Instance.Init();

            //加载资源,播放健康游戏忠告
            var loadTask = GameAssets.Instance.InitAsync();

            var noticeTask = ShowHealthNotice(); // 👈 新增

            // ✅ 等两个都完成
            await Task.WhenAll(loadTask, noticeTask);

            await Task.Delay(100); // 单位是毫秒，100ms = 0.1秒


            // 所有资源加载完毕后再进入游戏主场景
            var loadOp = SceneManager.LoadSceneAsync(activeSceneIndex);
            while (!loadOp.isDone)
                await Task.Yield();
            CreateOrFindSystem<SceneLoadHelper>(root);
            CreateOrFindSystem<CharacterClothesManager>(root);
            CreateOrFindSystem<CharacterHandHeldController>(root);

            // 初始化持久化对象管理器（在进入 MainScene 后）
            CreateOrFindSystem<PersistentObjectManager>(root);
            PersistentObjectManager.Instance.Initialize();


            Debug.Log("<color=green>=== [GameBootstrap] Done ===</color>");
        }


        private static async Task ShowHealthNotice()
        {
            // 通过 UIManager 创建
            var notice = UIManager.Instance.GetPanel<HealthNoticePanel>();

            await notice.PlayAsync();
        }

        /// <summary>
        /// 如果场景中没有，就在root下创建一个Mono单例
        /// </summary>
        private static T CreateOrFindSystem<T>(GameObject root) where T : MonoBehaviour
        {
            T instance =FindObjectOfType<T>();
            if (instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                go.transform.SetParent(root.transform);
                instance = go.AddComponent<T>();
                Log.Info($"[GameBootstrap] Created system: {typeof(T).Name}");
            }
            else
            {
                Log.Info($"[GameBootstrap] Found existing system: {typeof(T).Name}");
            }
            return instance;
        }

        /// <summary>
        /// 初始化非Mono系统
        /// </summary>
        private static void InitNonMonoSystems(IEnumerable<string> sysNames)
        {
            foreach (var sysName in sysNames)
            {
                string fullName = $"CLIP.NewSoil.{sysName}";
                Type type = Type.GetType(fullName);

                if (type == null)
                {
                    Log.Error($"[GameBootstrap] ❌ 未找到系统类型: {fullName}");
                    continue;
                }

                // 获取 Instance 属性
                PropertyInfo instanceProp = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                var instance = instanceProp?.GetValue(null);
                if (instance == null)
                {
                    Log.Error($"[GameBootstrap] ❌ 无法获取单例实例: {sysName}");
                    continue;
                }

                // 调用 Init() 方法（如果存在）
                MethodInfo initMethod = type.GetMethod("Init", BindingFlags.Public | BindingFlags.Instance);
                if (initMethod != null)
                {
                    initMethod.Invoke(instance, null);
                    Log.Info($"[GameBootstrap] Initialized Non-Mono system: {sysName}");
                }
                else
                {
                    Log.Error($"[GameBootstrap] ⚠️ {sysName} 没有 Init() 方法");
                }
            }
        }


        public static async Task LoadSceneAsyncAwait(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
            if (op == null)
            {
                Log.Error($"Failed to load scene: {sceneName}");
                return;
            }

            while (!op.isDone)
                await Task.Yield();

            Log.Info($"Scene '{sceneName}' loaded successfully!");
        }
    }
}
