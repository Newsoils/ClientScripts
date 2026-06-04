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

        [SerializeField]
        [Tooltip(
            "唯一常驻栈：建议把 LoginScene 里的 NetWork_Manager / Global_Game_Data_Receiver / Login_Manager / TapTapLoginManager / Dispatch_Manager 等整棵迁成预制体，拖到这里。" +
            "会在进入 LoginScene 之前实例化到 [GameBootstrap] 下并随 DDOL 常驻；并在 LoginScene 中删掉同名物体，避免 SingletonMono 判重销毁。")]
        GameObject persistentCoreSystemsPrefab;

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

            Application.targetFrameRate = 60;
            Application.runInBackground = true;

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
                InitializeFromBoot(this);
        }

        private static async void InitializeFromBoot(GameBootstrap boot)
        {
            Log.Custom("=== [GameBootstrap] Initializing ===","GameBoot", Color.cyan);
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 根节点
            GameObject root = new GameObject("[GameBootstrap]");
            DontDestroyOnLoad(root);

            // 常驻网络 / 登录 / 同步栈：先于 LoginScene 实例化，避免与 Login 场景内第二套物体冲突（预制体由 Boot 上字段指定）
            GameObject corePrefab = boot != null ? boot.persistentCoreSystemsPrefab : null;
            if (corePrefab == null)
                corePrefab = Resources.Load<GameObject>("PersistentCoreSystems");

            if (corePrefab != null)
            {
                var core = UnityEngine.Object.Instantiate(corePrefab, root.transform, false);
                core.name = corePrefab.name;
                Log.Info("[GameBootstrap] 已实例化常驻管理栈（来自 Inspector 预制体或 Resources/PersistentCoreSystems）。");
            }
            else
            {
                Log.Warn(
                    "[GameBootstrap] 未提供常驻栈：请在 Boot 物体上拖入 persistentCoreSystemsPrefab，或放入 Resources/PersistentCoreSystems.prefab；否则仍依赖各场景内管理器，可能与单例判重冲突。");
            }

            // -------------------------------
            // 1️⃣ 自动创建所有核心Mono系统
            // -------------------------------
            CreateOrFindSystem<AssetLoader>(root);
            CreateOrFindSystem<GameAssets>(root);
            CreateOrFindSystem<UIManager>(root);
            CreateOrFindSystem<ObjectPool>(root);
            CreateOrFindSystem<SceneLoadingHelper>(root);
            CreateOrFindSystem<NPCManager>(root);
            CreateOrFindSystem<NPCChatManager>(root);
            CreateOrFindSystem<MissionManager>(root);
            CreateOrFindSystem<PlantManager>(root);
            CreateOrFindSystem<GuideManager>(root);
            CreateOrFindSystem<PayManager>(root);
            CreateOrFindSystem<PromptManager>(root);
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
            CreateOrFindSystem<CharacterHandHeldManager>(root);
            CreateOrFindSystem<GachaManager>(root);
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
       

    
    }
}
