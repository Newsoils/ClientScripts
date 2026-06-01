using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Dispatch;
using Cmd;
using Google.Protobuf;
using Sirenix.OdinInspector;
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
                public class Dispatch_Manager : SerializedMonoBehaviour
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
                    /// 可以准备的背包个数，暂定为3个
                    /// </summary>
                    private int bags_count = 3;
                    public List<DispatchBagInfo> dispatch_Bags;

                    /// <summary>选包 UI 正在编辑的槽下标；-1 表示未在编辑。</summary>
                    private int _dispatchModelPreviewBagIndex = -1;
                    /// <summary>与 <see cref="_dispatchModelPreviewBagIndex"/> 对应的草稿（与 UI 侧 <c>curBagInfo</c> 同引用，随点选更新）。</summary>
                    private DispatchBagInfo _dispatchModelPreviewBag;

                    /// <summary>3D 模型 / 非 UI 层取「当前显示用」背包：编辑中则返回草稿，否则已存槽位。</summary>
                    public DispatchBagInfo GetBagForModelPreview(int index)
                    {
                        if (index == _dispatchModelPreviewBagIndex && _dispatchModelPreviewBag != null)
                        {
                            return _dispatchModelPreviewBag;
                        }
                        if (dispatch_Bags == null || index < 0 || index >= dispatch_Bags.Count)
                        {
                            return null;
                        }
                        return dispatch_Bags[index];
                    }

                    public void SetDispatchModelPreview(int bagIndex, DispatchBagInfo draft)
                    {
                        _dispatchModelPreviewBagIndex = bagIndex;
                        _dispatchModelPreviewBag = draft;
                    }

                    public void ClearDispatchModelPreview()
                    {
                        _dispatchModelPreviewBagIndex = -1;
                        _dispatchModelPreviewBag = null;
                    }

                    public static bool IsDispatching
                    {
                        get
                        {
                            if (_instance?._player_dispatch_state == null)
                                return false;
                            var ggm = Global_Game_Manager.Instance;
                            return ggm?._current_cat_info != null && ggm.IsCatCurrentlyTraveling();
                        }
                    }

                    /// <summary>Tick 分支与 <see cref="Global_Game_Manager._current_cat_info"/>.Status 对齐（0 在家 / 1 出游）。无数据时视为在家。</summary>
                    private static int GetCatStatusForDispatchTick()
                    {
                        var ggm = Global_Game_Manager.Instance;
                        if (ggm?._current_cat_info == null)
                            return Global_Game_Manager.CatStatusAtHome;
                        return ggm._current_cat_info.Status;
                    }

                    /// <summary>与服端/本地派遣对齐：CatInfo.Status（0 在家 / 1 出游）。</summary>
                    private void ApplyCatStatusForLocalDispatch(int status)
                    {
                        Global_Game_Manager.Instance?.EnsureCatInfoAndSetStatus(status);
                    }

                    /// <summary>
                    /// 是否已经向玩家展示过奖励
                    /// </summary>
                    private bool hasShowReward = true;


                    // -------------------- 事件系统 --------------------


                    public UnityEvent _on_get_middle_way_photo = new UnityEvent();     // 中途照片事件
                    public UnityEvent _on_finishing_dispatch = new UnityEvent();       // 派遣完成事件

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
                        // 仅在未初始化时创建，避免覆盖从服务器拉回的背包数据
                        if (dispatch_Bags == null || dispatch_Bags.Count == 0)
                        {
                            dispatch_Bags = new List<DispatchBagInfo>()
                            {
                                new DispatchBagInfo(),
                                new DispatchBagInfo(),
                                new DispatchBagInfo()
                            };
                        }
                        else
                        {
                            while (dispatch_Bags.Count < 3)
                            {
                                dispatch_Bags.Add(new DispatchBagInfo());
                            }
                        }
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
                        int catStatus = GetCatStatusForDispatchTick();
                        string report = _player_dispatch_state.dispatch_system_tick(catStatus);

                        if (report == "At_Home")
                        {
                            Log.Info("小苔在家");
                        }

                        else if (report == "On_Dispatch")
                        {
                            Log.Info("小苔外出中_\n" +
                                _player_dispatch_state._current_dispatch_info.dispatch_info_to_str() + "\n" +
                                "将于_" + _player_dispatch_state._current_dispatch_info.end_time + "_回家\n");
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
                        if (Global_Game_Manager.Instance?._current_cat_info != null
                            && Global_Game_Manager.Instance.IsCatCurrentlyTraveling())
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
                        if (Global_Game_Manager.Instance?._current_cat_info == null
                            || !Global_Game_Manager.Instance.IsCatCurrentlyTraveling())
                        {
                            Log.Custom("鼠鼠不在派遣中");
                            return;
                        }

                        var reward = _player_dispatch_state.on_end_dispatch();
                        ApplyCatStatusForLocalDispatch(Global_Game_Manager.CatStatusAtHome);
                        On_End_Dispatch(reward);

                        Log.Custom("小苔立刻返回");
                    }

                    public void On_Start_Dispatch()
                    {
                        ApplyDispatchDepartConsumePreparedBag();
                        _player_dispatch_state.on_start_dispatch();
                        ApplyCatStatusForLocalDispatch(Global_Game_Manager.CatStatusTraveling);

                        //Notice_To_UI("立刻出发!!_鼠鼠刚刚出发!!_\n" +
                        //    _player_dispatch_state._current_dispatch_info.dispatch_info_to_str());

                        Log.Info("小苔刚刚出发!!_\n" +
                                _player_dispatch_state._current_dispatch_info.dispatch_info_to_str());
                        // 通知任务系统
                        TaskEvent.TriggerXiaoTaiReturnHome();
                    }


                    /// <summary>
                    /// 派遣结束回调逻辑
                    /// </summary>
                    public void On_End_Dispatch((int, int, string) reward)
                    {
                        hasShowReward = false;

                        ClearDispatchCarriedSlotNamesAfterTrip();

                        List<(string, int)> rewardItems = new List<(string, int)> { ("鱼币", reward.Item1), ("亲密度", reward.Item2) };
                        // TODO zhaorui
                        // Global_Inventory_Manager.Change_Items_Count(rewardItems);
                        rewardItems.Add((reward.Item3, 1));

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

                    /// <summary>
                    /// 派遣出发前：按 1→2→3 号背包找首个「任一格有物」的背包，将槽位写入 <see cref="single_dispatch_info"/>，
                    /// 扣除便当/幸运小物库存（CD 不扣），清空该背包；随后 <see cref="player_dispatch_state.on_start_dispatch"/> 会对仍为空的槽位随机补齐。
                    /// </summary>
                    private void ApplyDispatchDepartConsumePreparedBag()
                    {
                        var cur = _player_dispatch_state._current_dispatch_info;
                        if (cur == null)
                        {
                            return;
                        }

                        if (dispatch_Bags == null || dispatch_Bags.Count == 0)
                        {
                            cur.carried_food_name = null;
                            cur.carried_snack_name = null;
                            cur.carried_tape_name = null;
                            return;
                        }

                        int idx = -1;
                        for (int i = 0; i < dispatch_Bags.Count; i++)
                        {
                            var b = dispatch_Bags[i];
                            if (b == null)
                            {
                                continue;
                            }

                            if (!string.IsNullOrEmpty(b.foodName) || !string.IsNullOrEmpty(b.snackName)
                                || !string.IsNullOrEmpty(b.tapeName))
                            {
                                idx = i;
                                break;
                            }
                        }

                        if (idx < 0)
                        {
                            cur.carried_food_name = null;
                            cur.carried_snack_name = null;
                            cur.carried_tape_name = null;
                            return;
                        }

                        var bag = dispatch_Bags[idx];
                        cur.carried_food_name = string.IsNullOrEmpty(bag.foodName) ? null : bag.foodName;
                        cur.carried_snack_name = string.IsNullOrEmpty(bag.snackName) ? null : bag.snackName;
                        cur.carried_tape_name = string.IsNullOrEmpty(bag.tapeName) ? null : bag.tapeName;

                        var changes = new List<(string, int)>();
                        if (!string.IsNullOrEmpty(bag.foodName))
                        {
                            changes.Add((bag.foodName, -1));
                        }

                        if (!string.IsNullOrEmpty(bag.snackName))
                        {
                            changes.Add((bag.snackName, -1));
                        }

                        if (changes.Count > 0)
                        {
                            Global_Inventory_Manager.Change_Items_Count(changes, "派遣出发");
                        }

                        bag.foodName = null;
                        bag.snackName = null;
                        bag.tapeName = null;
                        bag.isPacked = false;
                    }

                    /// <summary>回家后将本次携带名清空，避免持久化状态里残留；奖励已写入 last_reward_*。</summary>
                    private void ClearDispatchCarriedSlotNamesAfterTrip()
                    {
                        if (_player_dispatch_state._current_dispatch_info == null)
                        {
                            return;
                        }

                        var c = _player_dispatch_state._current_dispatch_info;
                        c.carried_food_name = null;
                        c.carried_snack_name = null;
                        c.carried_tape_name = null;
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
                        if (dispatch_Bags == null)
                        {
                            Debug.LogError("SetBagContent: dispatch_Bags is null");
                            return;
                        }
                        if (index < 0 || index >= dispatch_Bags.Count)
                        {
                            Debug.LogError($"SetBagContent: invalid index {index}, bags.Count={dispatch_Bags.Count}");
                            return;
                        }

                        // 不替换 slot 对象，改成按字段 copy；避免外部用同一个 bagInfo 反复传进来，
                        // 造成三个 slot 指向同一引用后同步变化。同时保留 dispatch_Bags[index] 的原有
                        // 对象身份，避免上层持有它的引用（如 DispatchPanel.curBagInfo）被悄悄挤掉。
                        var slot = dispatch_Bags[index];
                        if (slot == null)
                        {
                            slot = new DispatchBagInfo();
                            dispatch_Bags[index] = slot;
                        }

                        if (bagInfo != null && !ReferenceEquals(bagInfo, slot))
                        {
                            slot.foodName = bagInfo.foodName;
                            slot.snackName = bagInfo.snackName;
                            slot.tapeName = bagInfo.tapeName;
                        }
                        slot.SyncPackedFlag();

                        // 强制让各 slot 保持不同引用，防御上游重复赋值
                        for (int i = 0; i < dispatch_Bags.Count; i++)
                        {
                            if (i == index) continue;
                            if (ReferenceEquals(dispatch_Bags[i], slot))
                            {
                                Debug.LogWarning($"SetBagContent: slot {i} shares ref with slot {index}, cloning");
                                dispatch_Bags[i] = new DispatchBagInfo
                                {
                                    foodName = slot.foodName,
                                    snackName = slot.snackName,
                                    tapeName = slot.tapeName,
                                    isPacked = slot.isPacked
                                };
                            }
                        }

                        Debug.Log($"Dispatch_Manager.SetBagContent: index={index}, food={slot.foodName}, snack={slot.snackName}, tape={slot.tapeName}");
                    }

                    /// <summary>
                    /// 向服务器发送旅行背包打包请求（<see cref="PackageBagReq"/>，msg_id 1335）；成功后由 <see cref="PackageBagRes"/> 回写 <see cref="Global_Game_Manager._current_cat_info"/>。
                    /// </summary>
                    public void SendPackageBagRequest(int bagIndex0Based, DispatchBagInfo bagInfo)
                    {
                        if (bagInfo == null || bagIndex0Based < 0 || bagIndex0Based > 2)
                            return;
                        var travel = Global_Game_Manager.BuildCatTravelBagInfoFromDispatchBag(bagInfo);
                        var req = new PackageBagReq
                        {
                            BagIndex = bagIndex0Based + 1,
                            Bag = travel
                        };
                        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);
                    }

                    /// <summary>
                    /// 使用爱心车票让在外游历的小猫立刻回家（<see cref="Cmd.CatBackImmediatelyReq"/>，msg_id 1337）。
                    /// </summary>
                    public void SendCatBackImmediatelyRequest()
                    {
                        var req = new CatBackImmediatelyReq();
                        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, req);
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
                    /// 获取中途照片事件触发
                    /// </summary>
                    public void on_get_middle_way_photo()
                    {
                        _on_get_middle_way_photo.Invoke();
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
