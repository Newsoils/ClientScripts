using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.LYC.TaskSystem;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Client_Event_Systems;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel.Dispatch;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {
            namespace Dispatch_System
            {
                /// <summary>
                /// 派遣系统管理器
                /// - 控制玩家派遣（外出任务）的启动、更新与结束
                /// - 持有玩家派遣状态对象 player_dispatch_state
                /// - 与 UI / Server / 仓库系统交互
                /// </summary>
                public class Dispatch_Manager : MonoBehaviour
                {
                    // -------------------- 基础引用 --------------------

                    /// <summary>
                    /// 单例实例，保持全局唯一
                    /// </summary>
                    public static Dispatch_Manager _instance;

                    /// <summary>
                    /// 当前玩家的派遣状态对象，保存派遣中的各种状态数据
                    /// </summary>
                    public player_dispatch_state _player_dispatch_state;

                    /// <summary>
                    /// 派遣配置数据资源（ScriptableObject）
                    /// 包含地图列表、奖励方案等信息
                    /// </summary>
                    public Dispatch_DB_SO _dispatch_configuration_so;

                    /// <summary>
                    /// 派遣中拍摄的照片路径列表
                    /// </summary>
                    public List<string> _dispatch_photo_path_list = new List<string>();

                    /// <summary>
                    /// 可以准备的背包个数，暂定为3个
                    /// </summary>
                    private int bags_count = 3;
                    public List<DispatchBagInfo> dispatch_Bags;

                    public static bool IsDispatching
                    {
                        get
                        {
                            return _instance._player_dispatch_state.player_state == "On_Dispatch";
                        }
                    }

                    /// <summary>
                    /// 是否已经向玩家展示过奖励
                    /// </summary>
                    private bool hasShowReward = true;


                    // -------------------- 事件系统 --------------------

                    [Header("Event_System")]
                    //public Warehouse_UI_Event_Hub_SO _warehouse_UI_event_hub_so; // 仓库UI事件中心

                    public UnityEvent _on_get_middle_way_photo = new UnityEvent();     // 中途照片事件
                    public UnityEvent _on_finishing_dispatch = new UnityEvent();       // 派遣完成事件
                    public UnityEvent _update_dispatch_info_from_server = new UnityEvent(); // 从服务器更新派遣信息
                    public UnityEvent _upload_dispatch_info_to_server = new UnityEvent();   // 上传派遣信息到服务器
                    public UnityEvent _on_clear_previous_dispatch = new UnityEvent();  // 清除上次派遣事件

                    public static Dictionary<Mood_Tag, string> chineseMoodMap = new Dictionary<Mood_Tag, string>()
                    {
                        {Mood_Tag.Leisure, "悠闲" },
                        {Mood_Tag.Relax, "轻松" },
                        {Mood_Tag.Calm, "平静" },
                        {Mood_Tag.Natural, "自然" },
                        {Mood_Tag.Slient, "安静" },
                        {Mood_Tag.City, "都市" },
                        {Mood_Tag.Chinese, "中国风" },
                        {Mood_Tag.Happy, "开心" },
                    };

                    #region// -------------------- 生命周期 --------------------

                    void Awake()
                    {
                        // 单例初始化
                        if (_instance == null)
                        {
                            _instance = this;
                            DontDestroyOnLoad(this.gameObject); // 切换场景不销毁
                        }
                        else
                        {
                            // 如果重复创建，销毁后者
                            if (_instance != this)
                            {
#if UNITY_EDITOR
                                DestroyImmediate(this.gameObject);
#else
                                Destroy(this.gameObject);
#endif
                            }
                        }
                    }

                    private void Start()
                    {
                        // 加载配置数据并初始化派遣状态
                        if (_dispatch_configuration_so != null)
                        {
                            _dispatch_configuration_so.RefreshData(); // 从json读取派遣配置

                            _player_dispatch_state = new player_dispatch_state();
                            _player_dispatch_state._dispatch_config = _dispatch_configuration_so._dispatch_config;

                            // 绑定回调
                            //_player_dispatch_state._on_finishing_preparation += _on_finishing_preparation;
                            //_player_dispatch_state._on_get_mid_way_photo += on_get_middle_way_photo;

                            // 开启周期性tick协程
                            StartCoroutine(start_dispatch_co());
                        }
                        dispatch_Bags = new List<DispatchBagInfo>()
                        {
                            new DispatchBagInfo(),
                            new DispatchBagInfo(),
                            new DispatchBagInfo()
                        };
                        // 监听场景加载事件
                        SceneManager.sceneLoaded += on_scene_loaded;
                    }

                    #endregion
                    // -------------------- 场景切换 --------------------

                    public void on_scene_loaded(Scene _s, LoadSceneMode _m)
                    {
                        on_scene_loaded();
                    }

                    /// <summary>
                    /// 场景加载完成后，根据场景名判断是否显示派遣UI
                    /// </summary>
                    public void on_scene_loaded()
                    {
                        // 判断当前是否是派遣场景
                        bool _is_dispatch = false;
                        string currentSceneName = SceneManager.GetActiveScene().name;
                        if (currentSceneName.Contains("Dispatch") || currentSceneName.Contains("dispatch"))
                            _is_dispatch = true;

                        _dispatch_configuration_so._dispatch_config.map_info_list.ForEach((x) =>
                        {
                            if (currentSceneName == x.map_name)
                                _is_dispatch = true;
                        });

                        // 开启协程处理，不要直接调用
                        StartCoroutine(DelayedTriggerReward());
                    }


                    private void OnDestroy()
                    {
                        // 解除事件绑定，防止内存泄漏
                        //if (_player_dispatch_state != null)
                        //    _player_dispatch_state._on_finishing_preparation -= _on_finishing_preparation;

                        SceneManager.sceneLoaded -= on_scene_loaded;
                    }

                    private IEnumerator DelayedTriggerReward()
                    {
                        // 等待一帧，让新场景所有物体的 Start 跑完
                        //yield return null;

                        // 或者更保险一点，等到帧末尾
                        yield return new WaitForEndOfFrame();

                        if (hasShowReward == false)
                        {
                            List<(string, int)> rewardItems = new List<(string, int)> { ("鱼币", _player_dispatch_state.last_reward_currency)
                                , ("亲密度",_player_dispatch_state.last_reward_exp),(_player_dispatch_state.last_reward_photo_name,1) };

                            hasShowReward = EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Dispatch_Show_Reward, rewardItems);
                        }
                    }


                    // -------------------- 派遣主逻辑 --------------------

                    /// <summary>
                    /// 每60秒调用一次派遣tick
                    /// </summary>
                    public IEnumerator start_dispatch_co()
                    {
                        while (true)
                        {
                            if (Time.time < 0.5f)
                                yield return new WaitForSecondsRealtime(0.5f);

                            dispatch_tick();
                            yield return new WaitForSecondsRealtime(60f); // 每分钟检查一次派遣状态
                        }
                    }


                    /// <summary>
                    /// 派遣系统周期性状态更新（由协程调用）
                    /// </summary>
                    public void dispatch_tick()
                    {
                        string report = _player_dispatch_state.dispatch_system_tick();

                        if (report == "At_Home")
                        {
                            //Notice_To_UI($"鼠鼠在家_\n{_player_dispatch_state.player_at_home_length_in_minute}_分钟");
                            Log.Info($"小苔在家_\n{_player_dispatch_state.player_at_home_length_in_minute}_分钟");
                        }

                        else if (report == "On_Dispatch")
                        {
                            //Notice_To_UI("鼠鼠外出中_\n" +
                            //    _player_dispatch_state._current_dispatch_info.dispatch_info_to_str() + "\n" +
                            //    "将于_" + _player_dispatch_state._current_dispatch_info.end_time + "_回家\n");
                            Log.Info("小苔外出中_\n" +
                                _player_dispatch_state._current_dispatch_info.dispatch_info_to_str() + "\n" +
                                "将于_" + _player_dispatch_state._current_dispatch_info.end_time + "_回家\n");
                        }

                        else if (report == "On_Dispatch_Start")
                        {
                            On_Start_Dispatch();
                        }

                        else if (report == "On_Dispatch_End")
                        {
                            hasShowReward = false;
                            On_End_Dispatch((_player_dispatch_state.last_reward_currency, _player_dispatch_state.last_reward_exp, _player_dispatch_state.last_reward_photo_name));
                            //Notice_To_UI("鼠鼠回家_\n" +
                            //    "获得如下奖励_\n" +
                            //    _player_dispatch_state.last_reward_info + "_\n");
                            Log.Info("小苔回家_\n" +
                                "获得如下奖励_\n" +
                                _player_dispatch_state.last_reward_info + "_\n");
                        }
                    }



                    /// <summary>
                    /// 强制立即开始派遣
                    /// </summary>
                    public void force_dispatch_start()
                    {
                        //if (!_player_dispatch_state.Try_Finish_Preparation())
                        //{
                        //    Log.Error("DispatchManager：出发失败！");
                        //    return;
                        //}
                        //if (string.IsNullOrEmpty(_player_dispatch_state._current_dispatch_info.carried_food_name) ||
                        //      string.IsNullOrEmpty(_player_dispatch_state._current_dispatch_info.carried_snack_name) ||
                        //      string.IsNullOrEmpty(_player_dispatch_state._current_dispatch_info.carried_tape_name))
                        //{
                        //    Notice_To_UI("未完成准备，无法开始派遣！");
                        //    return;
                        //}
                        if (_player_dispatch_state.player_state != "At_Home")
                        {
                            Notice_To_UI("鼠鼠不在家");
                            return;
                        }

                        On_Start_Dispatch();

                        //clear_text_displpay();
                    }


                    /// <summary>
                    /// 强制立即结束派遣
                    /// </summary>
                    public void force_dispatch_end()
                    {
                        if (_player_dispatch_state.player_state != "On_Dispatch")
                        {
                            Log.Custom("鼠鼠不在派遣中");
                            return;
                        }

                        var reward = _player_dispatch_state.on_end_dispatch();
                        On_End_Dispatch(reward);

                        Log.Custom("小苔立刻返回");
                    }

                    public void On_Start_Dispatch()
                    {
                        _player_dispatch_state.on_start_dispatch();
                        upload_dispatch_info_to_server();

                        EvtDsp.TriggerEvt(EvtNames.Dispatch_On_Start);
                        //Notice_To_UI("立刻出发!!_鼠鼠刚刚出发!!_\n" +
                        //    _player_dispatch_state._current_dispatch_info.dispatch_info_to_str());

                        Log.Info("小苔刚刚出发!!_\n" +
                                _player_dispatch_state._current_dispatch_info.dispatch_info_to_str());
                        // 通知任务系统
                        //TaskTriggers.TriggerEventOfMultipleOperations("第一次完成派遣", 1);
                        TaskTriggers.TriggerEventOfMultipleOperations(3, 1);
                    }


                    /// <summary>
                    /// 派遣结束回调逻辑
                    /// </summary>
                    public void On_End_Dispatch((int, int, string) reward)
                    {
                        hasShowReward = false;

                        List<(string, int)> rewardItems = new List<(string, int)> { ("鱼币", reward.Item1), ("亲密度", reward.Item2) };
                        Global_Inventory_Manager.Change_Items_Count(rewardItems);
                        rewardItems.Add((reward.Item3, 1));

                        EvtDsp.TriggerEvt(EvtNames.Dispatch_On_End);


                        // 上传成就系统记录
                        if (Quest_And_Achievement_Manager.instance != null)
                        {
                            Quest_And_Achievement_Manager.instance.upload_player_action_to_server(
                                "Dispatch_Finish",
                                GF_SP.SerializeObject(_player_dispatch_state)
                            );
                        }

                        Log.Custom("Dispatch_Manager_on_finishing_dispatch\t" + "获得如下奖励_\t"
                            + _player_dispatch_state.last_reward_info + "_\n", "Dispatch_Manager");
                        // 通知监听者派遣结束
                        _on_finishing_dispatch.Invoke();
                    }

                    #region 获取派遣携带物相关信息
                    /// <summary>
                    /// 获取Food信息
                    /// </summary>
                    /// <param name="itemName">物品名，来自Inventory</param>
                    /// <returns></returns>
                    public static Food_Info GetFoodInfo(string itemName)
                    {
                        var config = _instance._dispatch_configuration_so._dispatch_config;
                        var item = Global_Inventory_Manager.GetItemInfo(itemName);
                        if (item == null) return null;
                        var foodInfo = config.food_info_list.Find(x => x.item_id == item.item_id);
                        return foodInfo;
                    }

                    /// <summary>
                    /// 获取Snack信息
                    /// </summary>
                    /// <param name="itemName">物品名，来自Inventory</param>
                    /// <returns></returns>
                    public static Snack_Info GetSnackInfo(string itemName)
                    {
                        var config = _instance._dispatch_configuration_so._dispatch_config;
                        var item = Global_Inventory_Manager.GetItemInfo(itemName);
                        if (item == null) return null;
                        var snackInfo = config.snack_info_list.Find(x => x.item_id == item.item_id);
                        return snackInfo;
                    }

                    /// <summary>
                    /// 获取CD信息
                    /// </summary>
                    /// <param name="itemName">物品名，来自Inventory</param>
                    /// <returns></returns>
                    public static Tape_Info GetCDInfo(string itemName)
                    {
                        var config = _instance._dispatch_configuration_so._dispatch_config;
                        var item = Global_Inventory_Manager.GetItemInfo(itemName);
                        if (item == null) return null;
                        var tapeInfo = config.tape_info_list.Find(x => x.item_id == item.item_id);
                        return tapeInfo;
                    }

                    /// <summary>
                    /// 获取Food信息
                    /// </summary>
                    /// <param name="id">物品ID（唯一）</param>
                    /// <returns></returns>
                    public static Food_Info GetFoodInfo(int id)
                    {
                        var config = _instance._dispatch_configuration_so._dispatch_config;
                        var item = Global_Inventory_Manager.GetItemInfo(id);
                        if (item == null) return null;
                        var foodInfo = config.food_info_list.Find(x => x.item_id == item.item_id);
                        return foodInfo;
                    }

                    /// <summary>
                    /// 获取Snack信息
                    /// </summary>
                    /// <param name="id">物品ID（唯一）</param>
                    /// <returns></returns>
                    public static Snack_Info GetSnackInfo(int id)
                    {
                        var config = _instance._dispatch_configuration_so._dispatch_config;
                        var item = Global_Inventory_Manager.GetItemInfo(id);
                        if (item == null) return null;
                        var snackInfo = config.snack_info_list.Find(x => x.item_id == item.item_id);
                        return snackInfo;
                    }

                    /// <summary>
                    /// 获取CD信息
                    /// </summary>
                    /// <param name="id">物品ID（唯一）</param>
                    /// <returns></returns>
                    public static Tape_Info GetCDInfo(int id)
                    {
                        var config = _instance._dispatch_configuration_so._dispatch_config;
                        var item = Global_Inventory_Manager.GetItemInfo(id);
                        if (item == null) return null;
                        var tapeInfo = config.tape_info_list.Find(x => x.item_id == item.item_id);
                        return tapeInfo;
                    }
                    #endregion


                    // -------------------- UI / Server 交互 --------------------

                    /// <summary>
                    /// 输出内容到UI文本组件
                    /// </summary>
                    public void Notice_To_UI(string output)
                    {
                        EvtDsp.TriggerEvt<string>(EvtNames.Dispatch_Text_Notice, output);
                        Log.Custom(output, "Dispatch_Manager");
                    }

                    /// <summary>
                    /// 清除UI文字显示
                    /// </summary>
                    public void clear_text_displpay()
                    {
                        EvtDsp.TriggerEvt(EvtNames.Dispatch_Text_Notice, "");
                    }

                    /// <summary>
                    /// 获取一个随机派遣任务包（调用配置SO）
                    /// </summary>
                    public single_dispatch_info Get_Random_Package()
                    {
                        return _dispatch_configuration_so.Get_Random_Package();
                    }
                    public void SetBagContent(int index, DispatchBagInfo bagInfo)
                    {
                        dispatch_Bags[index] = bagInfo;
                        dispatch_Bags[index].isPacked = true;
                    }

                    // -------------------- 各类事件回调 --------------------

                    ///// <summary>
                    ///// 准备派遣完成
                    ///// </summary>
                    //public void _on_finishing_preparation()
                    //{
                    //    Debug.Log("准备完成，开始派遣");
                    //    _warehouse_UI_event_hub_so._invoke_on_refresh_warehouse_UI();
                    //}

                    /// <summary>
                    /// 从服务器更新派遣信息
                    /// </summary>
                    public void update_dispatch_info_from_server()
                    {
                        _update_dispatch_info_from_server.Invoke();
                    }

                    /// <summary>
                    /// 上传派遣信息到服务器
                    /// </summary>
                    public void upload_dispatch_info_to_server()
                    {
                        _upload_dispatch_info_to_server.Invoke();
                    }

                    /// <summary>
                    /// 获取中途照片事件触发
                    /// </summary>
                    public void on_get_middle_way_photo()
                    {
                        _on_get_middle_way_photo.Invoke();
                    }

                    /// <summary>
                    /// 清理上次派遣记录
                    /// </summary>
                    public void on_clear_previous_dispatch()
                    {
                        _on_clear_previous_dispatch?.Invoke();
                    }




                    public void SwitchBag(int bagIndex)
                    {
                        if (bagIndex < 0 || bagIndex >= bags_count)
                        {
                            Debug.LogError("SwitchBag: Invalid bag index " + bagIndex);
                            return;
                        }

                        //_player_dispatch_state._current_dispatch_info = _player_dispatch_state.dispatch_Bags[bagIndex];
                        Debug.Log("Switched to bag index: " + bagIndex);
                    }
                }
            }
        }
    }
}
