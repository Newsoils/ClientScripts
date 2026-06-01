using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Cmd;
using Common;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Video;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

public class Global_Game_Data_Sync_Receiver : SingletonMono<Global_Game_Data_Sync_Receiver>, IMsg_Receiver
{
    protected override bool PersistAcrossScenes => true;
    /// <summary>与 <see cref="NPCManager.instance"/> 绑定客户端发往服务器的 NPC 请求（原 <c>NPC_Receiver</c>）。</summary>
    private NPCManager _npcManager;
    private bool _npcSendEventsBound;
    private Dictionary<int, TaskCompletionSource<string>> responseTasks = new Dictionary<int, TaskCompletionSource<string>>();

    /// <summary>非 MainScene 时收到的回家奖励推送，回到 MainScene 后再执行拍照与弹窗。</summary>
    private Cmd.BackToHomeRewardS2C _pendingBackToHomeReward;

    private bool _inventoryInitialSyncCompleted = false;


    private const string ReceiverName = "Global_Game_Manager";

    #region Unity Life Cycle

    /// <summary>
    /// 先于 <see cref="Start"/> 执行；与 <see cref="RegisterMap.LoadMessageMapBeforeFirstScene"/> 双保险，保证
    /// <see cref="RegisterToNetwork"/> 时 <see cref="RegisterMap.IsLoaded"/> 已就绪（同步加载，无协程）。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        RegisterMap.TryRegisterDefault();
    }

    private void Start()
    {
        // Awake 里 Destroy(duplicate) 在本帧末执行，重复体仍会跑到 Start → 会二次 RegisterMapProtoHandlers（如 1004 覆盖警告）
        if (!ReferenceEquals(this, Instance))
            return;

        if (NetWork_Center_WSS.Instance == null)
        {
            Debug.LogError("Global_Data_Sync: NetworkCenter not initialized");
            return;
        }

        BindUnityEvents();
        StartCoroutine(TryBindNpcManagerSendEventsDeferred());
        RegisterToNetwork();
        EvtDsp.AddEvt<IMessage>(EvtNames.Send_Req_To_Server, SendReqToServer); //保存数据
        EvtDsp.AddEvt(EvtNames.Login_Mandatory_Data_Ready, OnLoginMandatoryDataReady);
        EvtDsp.AddEvt<IMessage, Action<string>>(EvtNames.Receive_Msg_From_Server, GetDataFromServer);
        EvtDsp.AddReturnEvt<List<ServerTask>, Action<string>, Task>(EvtNames.Excute_Server_Tasks, ExcuteServerTasksAsync);
        EvtDsp.AddReturnEvt<ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, ExcuteServerTaskAsync);
        Debug.Log("Global_Game_Data_Sync_Receiver initialized");
    }

    private void Update()
    {
        TryFlushPendingBackToHomeRewardForMainScene();
    }

    /// <summary>
    /// 若启动后很快进入本方法，常见原因见日志里 <c>iAmRegisteredSingleton</c> 说明；另见 <see cref="SingletonMono{T}.Awake"/> 里对重复组件的 <c>Destroy</c>。
    /// </summary>
    protected override void OnDestroy()
    {
        var inst = Global_Game_Data_Sync_Receiver.Instance;
        bool iAmRegisteredSingleton = inst != null && ReferenceEquals(inst, this);
        Debug.LogWarning(
            "[Global_Game_Data_Sync_Receiver] OnDestroy | " +
            $"go={gameObject.name} activeSelf={gameObject.activeSelf} scene={gameObject.scene.name} " +
            $"iAmRegisteredSingleton={iAmRegisteredSingleton} " +
            "(false 通常=第二份单例被 SingletonMono 销毁；true=当前登记的单例被卸载体，如未进 DDOL 的切场景/父物体 Destroy/退出播放) " +
            "| 若同帧有 [SingletonMono] 重复… 警告可对照。");

        UnbindUnityEvents();
        UnbindNpcManagerSendEvents();
        if (iAmRegisteredSingleton)
        {
            UnregisterFromNetwork();
            RegisterMap.ClearHandlers();
        }
        if (iAmRegisteredSingleton)
        {
            EvtDsp.RemoveEvt<IMessage>(EvtNames.Send_Req_To_Server, SendReqToServer);
            EvtDsp.RemoveEvt(EvtNames.Login_Mandatory_Data_Ready, OnLoginMandatoryDataReady);
            EvtDsp.RemoveEvt<IMessage, Action<string>>(EvtNames.Receive_Msg_From_Server, GetDataFromServer);
            EvtDsp.RemoveReturnEvt<List<ServerTask>, Action<string>, Task>(EvtNames.Excute_Server_Tasks, ExcuteServerTasksAsync);
            EvtDsp.RemoveReturnEvt<ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, ExcuteServerTaskAsync);
        }

        base.OnDestroy();
    }

    #endregion

    #region UnityEvent Binding

    private void BindUnityEvents()
    {
        if (Global_Game_Manager.Instance != null)
        {
            Global_Game_Manager.Instance._update_weather_state_from_server
                .AddListener(UpdateWeatherStateFromServer);
            Global_Game_Manager.Instance._upload_weather_state_to_server
                .AddListener(UploadWeatherStateToServer);

            Global_Game_Manager.Instance._on_update_player_brief_from_server
                .AddListener(UpdatePlayerBriefFromServer);
            Global_Game_Manager.Instance._on_upload_player_brief_to_server
                .AddListener(UploadPlayerBriefToServer);
        }

        if (Global_Inventory_Manager.Instance != null)
        {
            // Global_Inventory_Manager.Instance._update_inventory_from_server
            //     .AddListener(UpdateInventoryFromServer);
            // Global_Inventory_Manager.Instance._send_inventory_to_server
            //     .AddListener(SendInventoryToServer);

            Global_Inventory_Manager.Instance._update_shop_state_from_server
                .AddListener(UpdateShopStateFromServer);
            Global_Inventory_Manager.Instance._send_shop_state_to_server
                .AddListener(SendShopStateToServer);
        }
    }

    private void UnbindUnityEvents()
    {
        if (Global_Game_Manager.Instance != null)
        {
            Global_Game_Manager.Instance._update_weather_state_from_server
                .RemoveListener(UpdateWeatherStateFromServer);
            Global_Game_Manager.Instance._upload_weather_state_to_server
                .RemoveListener(UploadWeatherStateToServer);

            Global_Game_Manager.Instance._on_update_player_brief_from_server
                .RemoveListener(UpdatePlayerBriefFromServer);
            Global_Game_Manager.Instance._on_upload_player_brief_to_server
                .RemoveListener(UploadPlayerBriefToServer);
        }

        if (Global_Inventory_Manager.Instance != null)
        {
            // Global_Inventory_Manager.Instance._update_inventory_from_server
            //     .RemoveListener(UpdateInventoryFromServer);
            // Global_Inventory_Manager.Instance._send_inventory_to_server
            //     .RemoveListener(SendInventoryToServer);

            Global_Inventory_Manager.Instance._update_shop_state_from_server
                .RemoveListener(UpdateShopStateFromServer);
            Global_Inventory_Manager.Instance._send_shop_state_to_server
                .RemoveListener(SendShopStateToServer);
        }
    }

    #endregion

    #region NPC Manager — client → server（原 NPC_Receiver）

    /// <summary>
    /// <see cref="NPCManager"/> 可能在 <see cref="FinishLoginFlowAfterMandatoryInitData"/> 异步加载 MainScene 后才存在，故延迟重试绑定。
    /// </summary>
    private IEnumerator TryBindNpcManagerSendEventsDeferred()
    {
        BindNpcManagerSendEvents();
        if (_npcSendEventsBound)
            yield break;

        for (var i = 0; i < 180 && !_npcSendEventsBound; i++)
        {
            yield return null;
            BindNpcManagerSendEvents();
        }

        if (!_npcSendEventsBound)
            Debug.LogWarning("Global_Game_Data_Sync_Receiver: NPCManager.instance 长时间为空，NPC 网络发送事件未绑定。");
    }

    private void BindNpcManagerSendEvents()
    {
        if (_npcSendEventsBound)
            return;

        _npcManager = NPCManager.Instance;
        if (_npcManager == null)
            return;

        //_npcManager.on_Meet_NPC.AddListener(OnNpcMeetNpcSend);
        _npcManager.ask_Single_NPC_Data.AddListener(OnNpcAskSingleNpcDataSend);
        _npcSendEventsBound = true;
    }

    private void UnbindNpcManagerSendEvents()
    {
        if (!_npcSendEventsBound || _npcManager == null)
        {
            _npcManager = null;
            _npcSendEventsBound = false;
            return;
        }

        //_npcManager.on_Meet_NPC.RemoveListener(OnNpcMeetNpcSend);
        _npcManager.ask_Single_NPC_Data.RemoveListener(OnNpcAskSingleNpcDataSend);
        _npcManager = null;
        _npcSendEventsBound = false;
    }


    private void OnNpcAskSingleNpcDataSend(string _)
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }



    #endregion

    #region Network Send Helper

    // private void SendMsg(string action, string detail)
    // {
    //     if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
    //         return;

    //     Network_Msg msg = new Network_Msg
    //     {
    //         sender = ReceiverName,
    //         action_target = "Player_Server",
    //         action = action,
    //         detail_info = detail,
    //         _sending_mode = Msg_Sending_Mode.Client_to_Server
    //     };

    //     _networkCenter.send_via_wss(msg);
    // }

    #endregion

    #region Client -> Server
    // private void SaveDataToServer(string action, string data)
    // {
    //     SendMsg(action, data);
    // }

    private void SendReqToServer(IMessage data)
    {
        if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
            return;
        NetWork_Center_WSS.SendMsg(data);
    }
    private void GetDataFromServer(IMessage target, Action<string> onTaskComplete)
    {
        int msg_id = RegisterMap.GetMsgIdForType(target.GetType());

        _ = GetDataFromServerAsync(msg_id, target, onTaskComplete);
    }
    private async Task<string> GetDataFromServerAsync(int msg_id, IMessage target, Action<string> onTaskComplete)
    {
        if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            onTaskComplete?.Invoke(null);
            return null;
        }
        NetWork_Center_WSS.SendMsg(target);
        TaskCompletionSource<string> result = new TaskCompletionSource<string>();
        var res_msg_id = msg_id + 1;
        responseTasks.Add(res_msg_id, result);
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(2));
        var completedTask = await Task.WhenAny(result.Task, timeoutTask);
        if (completedTask == timeoutTask)
        {
            responseTasks.Remove(res_msg_id);
            //var reqTypeName = target != null ? target.GetType().Name : "null";
            //Debug.LogWarning(
            //    "[Global_Game_Data_Sync_Receiver] GetDataFromServerAsync：等待响应超过 2s，将弹出「与服务器断开连接，请检查网络设置！」并触发 Network_Disconnect。 " +
            //    $"req_msg_id={msg_id}, expect_res_msg_id={res_msg_id}, req_payload_type={reqTypeName}, " +
            //    $"IsWsTransportConnected={NetWork_Center_WSS.IsWsTransportConnected}, IsConnectedToPlayerServer={NetWork_Center_WSS.IsConnectedToPlayerServer}, " +
            //    $"pending_response_tasks_after_remove={responseTasks.Count}",
            //    this);
            //EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, "与服务器断开连接，请检查网络设置！",()=> EvtDsp.TriggerEvt(EvtNames.Network_Disconnect));
            onTaskComplete?.Invoke(null);
            return null;
        }
        else
        {
            responseTasks.Remove(res_msg_id);
            if (result.Task.Result == null)
            {
                onTaskComplete?.Invoke("nodata");
            }
            onTaskComplete?.Invoke(result.Task.Result);
            return result.Task.Result;
        }
    }
    private async Task ExcuteServerTaskAsync(ServerTask task, Action<string> onTasksComplete)
    {
        List<ServerTask> tasks = new List<ServerTask> { task };
        await ExcuteServerTasksAsync(tasks, onTasksComplete);
    }
    private async Task ExcuteServerTasksAsync(List<ServerTask> tasks, Action<string> onTasksComplete)
    {
        string result = "success";
        List<IMessage> datas = tasks.Select(task => task.data).ToList();
        if (datas.Count == 0)
        {
            return;
        }
        var msg_id = RegisterMap.GetMsgIdForType(datas[0].GetType());
        string receive = await GetDataFromServerAsync(msg_id, datas[0], null);
        // List<(bool,string)> receives = JsonConvert.DeserializeObject<List<(bool, string)>>(receive);
        //检查是否执行失败，执行失败时只执行最终失败的任务
        // for (int i = 0; i < receives.Count; i++)
        // {
        //     if (receives[i].Item1 == false)
        //     {
        tasks[0].onReceiveMsg.Invoke(receive, tasks[0]);
        onTasksComplete?.Invoke(tasks[0].result);
        return;
        //     }
        // }
        // //逐个执行任务
        // for (int i = 0; i < receives.Count; i++)
        // {
        //     if (!tasks[i].isSendToServer)
        //     {
        //         tasks[i].onReceiveMsg.Invoke("", tasks[i]);
        //     }
        //     else
        //     {
        //         tasks[i].onReceiveMsg.Invoke(receives[i].Item2, tasks[i]);
        //     }
        //     if (tasks[i].isBreak)
        //     {
        //         result = tasks[i].result;
        //         onTasksComplete?.Invoke(result);
        //         return;
        //     }
        // }

        // onTasksComplete?.Invoke(result);

    }
    private void UpdateInventoryFromServer()
    {
        // SendMsg("Get_Data", "Game_Inventory");
        var req = new Cmd.GetItemBagReq();
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(req);
    }

    // private void SendInventoryToServer()
    // {
    //     if (_inventorySO == null) return;

    //     // string dataJson = Global_Inventory_Manager.Inventory_Serialization();
    //     // string lastJson = GF_SP.SerializeObject(new List<string>() { "Game_Inventory", dataJson });
    //     SendMsg(1321,new Cmd.EmptyReq()) ;
    // }
    private void UpdateWeatherStateFromServer()
    {
        // SendMsg("Get_Data", "Weather_State");
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }

    private void UploadWeatherStateToServer()
    {
        if (Global_Game_Manager.Instance == null) return;

        // string json = GF_SP.SerializeObject(Global_Game_Manager.Instance._weather_state);

        // var lastData = GF_SP.SerializeObject( new List<string>() { "Weather_State", json });

        // SendMsg("Save_Data", lastData );
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }

    private void UpdateShopStateFromServer()
    {
        // SendMsg("Get_Data", "Shop_State");
        // if (NetWork_Center_WSS.IsConnectedToPlayerServer)
        //     NetWork_Center_WSS.SendMsg(1063, new Cmd.GetAllShopInfoReq());
    }

    private void SendShopStateToServer()
    {
        if (Global_Inventory_Manager.Instance == null) return;

        // string json = Global_Inventory_Manager.Instance.get_shop_state_json();
        // var lastData = GF_SP.SerializeObject( new List<string>() { "Shop_State", json });
        // SendMsg("Save_Data", lastData);
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }

    private void UpdatePlayerBriefFromServer()
    {
        // SendMsg("Get_Data", "Player_Brief");
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }

    private void UploadPlayerBriefToServer()
    {
        if (Global_Game_Manager.Instance == null) return;

        // string json = GF_SP.SerializeObject(Global_Game_Manager.Instance._player_brief);
        // var lastData = GF_SP.SerializeObject( new List<string>() { "Player_Brief", json });
        // SendMsg("Save_Data", lastData);
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }

    private void OnLoginMandatoryDataReady()
    {
        Global_Game_Manager.Instance?.NotifyMandatoryLoginDataReady();
        FinishLoginFlowAfterMandatoryInitData();
    }

    /// <summary>登录前置数据（背包、小猫等）齐套后的后续流程（原 Login_Manager / On_Login_Success 链）。</summary>
    public void FinishLoginFlowAfterMandatoryInitData()
    {
        StartCoroutine(FinishLoginFlowAfterMandatoryInitDataCoroutine());
    }

    private IEnumerator FinishLoginFlowAfterMandatoryInitDataCoroutine()
    {
        UpdateInventoryFromServer();

        yield return new WaitForSeconds(1.024f);
        UpdateWeatherStateFromServer();
        StartRefreshState();
        UpdatePlayerBriefFromServer();

        Player_Social_Manager._instance?.on_update_social_info_from_server();
        Email_And_Announcement_Receiver.Instance?.Init_Data();
        Quest_And_Achievement_Receiver.Instance?.UpdateAchievementInfoFromServer();

        yield return PlayTransitionVideoAsync();

        if (Msg_Dispatcher.Instance != null)
            Msg_Dispatcher.Instance._is_locking = false;
    }

    /// <summary>
    /// 如果没播放过视频，播放视频（后续逻辑移交给VideoPanel），否则直接进入主场景
    /// </summary>
    /// <returns></returns>
    private async Task PlayTransitionVideoAsync()
    {
        var roleInfo = Global_Game_Manager.Instance?._current_player_role_info;
        if ((roleInfo.FirstInfoFlag & 1 << 1) == 0)
        {
            var clip = await GameAssets.Instance.LoadAsycByKey<VideoClip>("MP4_STORY");
            if (clip == null)
            {
                Debug.LogWarning("[TransitionVideoPanel] VideoClip load failed, trigger scene load directly.");
                EvtDsp.TriggerEvt(EvtNames.OnTransitionVideoFinished);
                return;
            }
            EvtDsp.TriggerEvt<VideoClip>(EvtNames.PlayTransitionVideo, clip);
        }
        else
            SceneLoadHelper.Load_MainScene();
    }

    public void StartRefreshState()
    {
        StartCoroutine(RefreshStateCoroutine());
    }

    private IEnumerator RefreshStateCoroutine()
    {
        while (true)
        {
            Log.Info("in_refresh_state_loop");

            Global_Game_Manager.Instance?.try_update_weahter();
            yield return new WaitForSeconds(1.024f);

            yield return new WaitForSeconds(60f);
        }
    }

    #endregion

    #region IMsg_Receiver
    // public string get_receiver_name()
    // {
    //     return ReceiverName;
    // }

    public void receive_msg(Network_Msg msg)
    {
        Log.Custom($"Receive Message : msg_id={msg.msg_id}, detail_len={msg.detail_info?.Length ?? 0}", "Global_Game_Manager");

        /* 原 Quit_Game 处理（保留备查）
        if (string.Equals(msg.action, "Quit_Game", StringComparison.Ordinal))
        {
            Global_Game_Manager.Instance.QuitGame(msg.detail_info, 5.0f);
            return;
        }
        */

        Log.Custom($"Receive Message : msg_id = {msg.msg_id}, detail_info = {msg.detail_info}");

        try
        {

            if (!RegisterMap.TryParseProtoJson(msg.msg_id, msg.detail_info, out var req) || req == null)
            {
                Debug.LogWarning(
                    $"[Global_Game_Data_Sync_Receiver] TryParseProtoJson 失败 msg_id={msg.msg_id} detail_len={msg.detail_info?.Length ?? 0}");
                return;
            }

            if (responseTasks.TryGetValue(msg.msg_id, out var tFail))
            {
                tFail.SetResult(msg.detail_info);
                return;
            }

            if (!RegisterMap.TryInvokeHandler(msg.msg_id, req))
            {
                Debug.LogWarning(
                    $"[Global_Game_Data_Sync_Receiver] TryInvokeHandler 失败 msg_id={msg.msg_id} detail_len={msg.detail_info?.Length ?? 0}");
                return;
            }
        }
        catch (Exception e)
        {
            var preview = string.IsNullOrEmpty(msg.detail_info)
                ? "(empty)"
                : (msg.detail_info.Length > 200 ? msg.detail_info.Substring(0, 200) + "..." : msg.detail_info);
            Debug.LogWarning($"[Global_Game_Data_Sync_Receiver] receive_msg exception action={msg.action} preview={preview}\n{e}");
        }
        finally
        {
            if (Msg_Dispatcher.Instance != null)
                Msg_Dispatcher.Instance._is_locking = false;
        }

        /*
        原 switch（保留备查）：将 detail_info 反序列化后分发到各 Handle*
        if (!string.IsNullOrWhiteSpace(msg.detail_info) || msg.msg_id == 1338 || msg.msg_id == 1340 || msg.msg_id == 1004)
        {
            try
            {
                var parser = new global::Google.Protobuf.JsonParser(
                    global::Google.Protobuf.JsonParser.Settings.Default.WithIgnoreUnknownFields(true)
                );
                switch (msg.msg_id)
                {
                    case 1004:
                        Login_Manager.Instance?.HandleUserVerifyLoginResponse(msg);
                        break;
                    case 1012:
                        HandleResponseData1012(parser.Parse<GetItemBagRes>(msg.detail_info));
                        break;
                    case 1308:
                        ...
                }
            }
            catch (Exception e) { ... }
        }
        Msg_Dispatcher.Instance._is_locking = false;
        */
    }

    // private void HandleResponseData(string json)
    // {
    //     var data = GF_SP.DeserializeObject<List<string>>(json);
    //     if (data == null || data.Count != 2)
    //         return;

    //     string key = data[0];
    //     string payload = data[1];

    //     switch (key)
    //     {
    //         case "Game_Inventory":
    //             Global_Inventory_Manager.Instance?.Load_Data_From_Json(payload);
    //             break;

    //         case "Weather_State":
    //             Global_Game_Manager.Instance?.load_weather_state_from_json(payload);
    //             Global_Game_Manager.Instance?.try_update_weahter();
    //             break;

    //         case "Shop_State":
    //             Global_Inventory_Manager.Instance?.load_shop_state_from_json(payload);
    //             Global_Inventory_Manager.Instance?.try_refresh_shop_state();
    //             break;

    //         case "Player_Brief":
    //             Global_Game_Manager.Instance?.load_player_brief_from_json(payload);
    //             break;
    //     }
    // }

    private void HandleResponseData1012(Cmd.GetItemBagRes res)
    {
        Global_Inventory_Manager.Instance?.ApplyGetItemBagRes(res);
        Global_Game_Manager.Instance.UpdateLoginProcessFlagAfterDataBack(0);
    }

    private void HandleResponseData1316(Cmd.GetAllNpcInfoRes res)
    {
        try
        {
            NPCManager.Instance?.Receive_All_NPC_Data(res);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Login] GetAllNpcInfoRes(1316) 处理异常，仍推进必拉 flag(2): {ex}");
        }
        finally
        {
            // 与 Login_Manager.OnLoginSuccess 中 RegisterInitDataAfterLogin(2, ...) 成对，缺少则永远不会触发 Login_Mandatory_Data_Ready → 无法进 MainScene
            Global_Game_Manager.Instance?.UpdateLoginProcessFlagAfterDataBack(2);
        }
    }

    private void HandleResponseData1332(Cmd.GetCatInfoRes res)
    {
        var ggm = Global_Game_Manager.Instance;
        if (ggm == null)
        {
            Debug.LogError("[Global_Game_Data_Sync_Receiver] 1332：Global_Game_Manager.Instance 为空，无法写入 _current_cat_info。");
            return;
        }

        if (res != null && res.Cat != null)
        {
            ggm._current_cat_info = res.Cat;
            if (ggm.DebugLogCatPresence)
            {
                var c = ggm._current_cat_info;
                Debug.Log(
                    $"[CatInfoTrace] 1332 GetCatInfoRes 已写入 **Instance**._current_cat_info | Status={c.Status} " +
                    $"CurrentTravel={(c.CurrentTravel == null ? "null" : $"dest={c.CurrentTravel.Destination}")}");
            }
        }
        else
        {
            // 必拉仍会推进 LoginProcessFlag(1)；若此处不写 _current_cat_info，进主场景后到 CatInfoChange 前断点一直为 null。
            if (res == null)
                Debug.LogWarning("[Global_Game_Data_Sync_Receiver] GetCatInfoRes(1332) 整体为 null，使用客户端默认 CatInfo（Status=在家），直至服端推送；请查网关/反序列化。");
            else
                Debug.LogWarning("[Global_Game_Data_Sync_Receiver] GetCatInfoRes(1332) 的 Cat 为 null，使用客户端默认 CatInfo（Status=在家）；请核对服端是否下发 cat 字段（proto3 建议小驼峰）。");

            ggm.EnsureCatInfoAndSetStatus(Global_Game_Manager.CatStatusAtHome);
        }

        ggm.ReplaceDispatchBagsFromCurrentCatInfo();
        CharacterClothesManager.Instance?.ApplyCatInfoWears(ggm._current_cat_info);
        ggm.UpdateLoginProcessFlagAfterDataBack(1);
        ggm.LogCatPresenceAlways("GetCatInfoRes 1332");
    }

    /// <summary>小猫信息变动推送：更新本地 CatInfo；室内猫与小地图头像由 Global_Game_Manager.Update 与 MapPanel 轮询 CatInfo.Status。</summary>
    private void HandleCatInfoChangeS2C(Cmd.CatInfoChangeS2C res)
    {
        if (res == null || res.Cat == null)
            return;

        var ggm = Global_Game_Manager.Instance;
        if (ggm == null)
        {
            Debug.LogError("[Global_Game_Data_Sync_Receiver] CatInfoChangeS2C：Global_Game_Manager.Instance 为空，无法写入 _current_cat_info。");
            return;
        }

        ggm._current_cat_info = res.Cat;
        if (ggm.DebugLogCatPresence)
        {
            var c = ggm._current_cat_info;
            Debug.Log(
                $"[CatInfoTrace] CatInfoChangeS2C 已写入并规范化 **Instance**._current_cat_info | Status={c.Status} " +
                $"CurrentTravel={(c.CurrentTravel == null ? "null" : $"dest={c.CurrentTravel.Destination}")}");
        }
        ggm.ReplaceDispatchBagsFromCurrentCatInfo();
        CharacterClothesManager.Instance?.ApplyCatInfoWears(ggm._current_cat_info);
        ggm.LogCatPresenceAlways("CatInfoChangeS2C");
    }

    private void HandlePackageBagRes(Cmd.PackageBagRes res)
    {
        Global_Game_Manager.Instance.ApplyPackageBagRes(res);
    }

    private void HandleGachaTakeRes(Cmd.GachaTakeRes res)
    {
        var rewardItems = BuildRewardListFromItemInfos(res?.Items);
        rewardItems.AddRange(BuildRewardListFromItemInfos(res?.ExItems));

        if (rewardItems.Count > 0)
        {
            EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, rewardItems);
            ExpManager.TempAddExpForRoomPlacementGains(rewardItems);
        }

        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
    }

    private static List<(string, int)> BuildRewardListFromItemInfos(IEnumerable<Common.ItemInfo> items)
    {
        var list = new List<(string, int)>();
        if (items == null)
            return list;

        foreach (var item in items)
        {
            if (item == null || item.ConfigID == 0UL || item.Count <= 0L || item.ConfigID > int.MaxValue)
                continue;

            int configId = (int)item.ConfigID;
            int count = item.Count > int.MaxValue ? int.MaxValue : (int)item.Count;
            var info = Global_Inventory_Manager.GetItemInfo(configId);
            string itemName = info != null && !string.IsNullOrEmpty(info.name) ? info.name : $"道具_{configId}";
            list.Add((itemName, count));
        }

        return list;
    }

    /// <summary>
    /// 爱心车票立刻回家成功：走原 <see cref="Dispatch_Manager.force_dispatch_end"/> 的派遣结束与奖励展示链路；车票与背包/小猫数据由服务器后续推送更新，本地不再扣道具。
    /// </summary>
    private void HandleCatBackImmediatelyRes(Cmd.CatBackImmediatelyRes _)
    {
        if (Dispatch_Manager._instance == null)
            return;
        if (!Dispatch_Manager.IsDispatching)
            return;
        Dispatch_Manager._instance.force_dispatch_end();
        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
    }

    private void HandleResponseData1064(Cmd.GetAllShopInfoRes res)
    {
        EvtDsp.TriggerEvt<GetAllShopInfoRes>(EvtNames.OnShopInfoReceived, res);
    }

    private void HandleManualRefreshShopRes(Cmd.ManualRefreshShopRes res)
    {
        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        EvtDsp.TriggerEvt<ManualRefreshShopRes>(EvtNames.OnManualRefreshShopReceived, res);
    }

    private void HandleBuyGoodsRes(Cmd.BuyGoodsRes res)
    {
        var rewardItems = BuildRewardListFromItemInfos(res?.Items);
        if (rewardItems.Count > 0)
        {
            ExpManager.TempAddExpForRoomPlacementGains(rewardItems);
        }
        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        EvtDsp.TriggerEvt<BuyGoodsRes>(EvtNames.OnBuyGoodsReceived, res);
    }

    private void HandleBuyTicketsRes(Cmd.BuyTicketsRes res)
    {
        var rewardItems = BuildRewardListFromItemInfos(res?.RewardItems);
        if (rewardItems.Count > 0)
        {
            ExpManager.TempAddExpForRoomPlacementGains(rewardItems);
        }
        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        EvtDsp.TriggerEvt<BuyTicketsRes>(EvtNames.OnBuyTicketsReceived, res);
    }

    private void HandleBuyDiamondTimesRes(BuyDiamondTimesRes res)
    {
        PayManager.Instance?.OnBuyDiamondTimesRes();
    }

    private void HandleSoldItemRes(SoldItemRes res)
    {
        var rewardItems = BuildRewardListFromItemInfos(res?.Items);
        if (rewardItems.Count > 0)
            EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, rewardItems);

        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        EvtDsp.TriggerEvt(EvtNames.ReloadRecycleData);
        EvtDsp.TriggerEvt(EvtNames.OnSoldItemReceived);
    }

    private void HandleResponseData1326(Cmd.RoleInfoChangeS2C res)
    {
        Global_Game_Manager.Instance.update_player_info_from_s2c(res);
    }

    private void HandleSetRoleInfoRes(SetRoleInfoRes res)
    {
        if (res?.RoleInfo == null || Global_Game_Manager.Instance == null)
            return;

        var ggm = Global_Game_Manager.Instance;
        ggm._current_player_role_info = res.RoleInfo;
        ggm._current_player_name = res.RoleInfo.RoleName;
        if (ggm._player_brief != null)
            ggm._player_brief._player_nick_name = res.RoleInfo.RoleName;

        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
    }

    private void HandleResponseData1328(Cmd.ItemChangeS2C res)
    {
        Global_Inventory_Manager.Instance.update_inventory_from_s2c(res);
    }

    private void HandleResponseData1322(Cmd.EmptyRes res)
    {
    }


    private void HandleOpenPhotoBookRes(Cmd.OpenPhotoBookRes res)
    {

    }


    private void HandleNpcGiftToPlayerS2C(Cmd.NpcGiftToPlayerS2C res)
    {
        if (NPCManager.Instance != null)
            NPCManager.Instance.Receive_NPC_Gift_TO_Player(res);
    }

    private const int DebugMissionTaskType = 10002;
    private const int DebugMissionTaskParameter = 1;
    private static Dictionary<int, MissionStaticData> _taskConfigByTaskId;

    private void HandleMissionChangeS2C(Cmd.MissionChangeS2C res)
    {
        LogMissionChangeForTaskType(res);

        MissionUpdateInfo info = new MissionUpdateInfo();

        if (res.MissionAdd != null && res.MissionAdd.Count > 0)
        {
            info.missionAdd = new List<MissionRuntimeData>();
            foreach (var m in res.MissionAdd)
            {
                info.missionAdd.Add(new MissionRuntimeData()
                {
                    missionId = (int)m.MissionConfigID,
                    current = m.Progress,
                    status = m.Status,
                    missionUId = m.MissionUID,
                });
            }
        }

        if (res.MissionUpd != null && res.MissionUpd.Count > 0)
        {
            info.missionUpdate = new List<MissionRuntimeData>();
            foreach (var m in res.MissionUpd)
            {
                info.missionUpdate.Add(new MissionRuntimeData()
                {
                    missionId = (int)m.MissionConfigID,
                    current = m.Progress,
                    status = m.Status,
                    missionUId = m.MissionUID,
                });
            }
        }

        if (res.MissionDel != null && res.MissionDel.Count > 0)
        {
            info.missionDelete = new List<ulong>();
            foreach (var m in res.MissionDel)
            {
                info.missionDelete.Add(m.MissionUID);
            }
        }

        EvtDsp.TriggerEvt<MissionUpdateInfo>(EvtNames.OnMissionUpdate, info);
    }

    /// <summary>调试：按 MissionConfigID 匹配配置表 taskId，taskType=10002 且 taskParameter=1 时打印。</summary>
    private static void LogMissionChangeForTaskType(Cmd.MissionChangeS2C res)
    {
        if (res == null)
            return;

        LogMissionInfosForTaskType("MissionAdd", res.MissionAdd);
        LogMissionInfosForTaskType("MissionUpd", res.MissionUpd);
        LogMissionInfosForTaskType("MissionDel", res.MissionDel);
    }

    private static Dictionary<int, MissionStaticData> GetTaskConfigByTaskId()
    {
        if (_taskConfigByTaskId != null)
            return _taskConfigByTaskId;

        JsonDataManager.LoadTaskData(out _taskConfigByTaskId);
        return _taskConfigByTaskId;
    }

    private static void LogMissionInfosForTaskType(string section, IEnumerable<Common.MissionInfo> missions)
    {
        if (missions == null)
            return;

        var taskConfigs = GetTaskConfigByTaskId();
        foreach (var m in missions)
        {
            if (m == null)
                continue;

            int taskId = (int)m.MissionConfigID;
            if (!taskConfigs.TryGetValue(taskId, out var cfg))
                continue;
            if ((cfg.taskType != DebugMissionTaskType && cfg.taskType != 10004) || cfg.taskParameter != DebugMissionTaskParameter)
                continue;

            Debug.Log(
                $"[MissionChangeS2C] taskType={cfg.taskType} taskParameter={cfg.taskParameter} section={section} " +
                $"taskId={cfg.taskId} desc={cfg.desc} MissionUID={m.MissionUID} MissionConfigID={m.MissionConfigID} " +
                $"MissionType={m.MissionType} Progress={m.Progress} Status={m.Status} UpdateTime={m.UpdateTime}");
        }
    }


    private void HandleMissionRewardsRes(Cmd.MissionRewardsRes res)
    {
        List<(string, int)> itemsToShow = new List<(string, int)>();

        foreach (var item in res.Items)
        {
            var itemInfo = Global_Inventory_Manager.GetItemInfo((int)item.ConfigID);
            itemsToShow.Add(new(itemInfo.name, (int)item.Count));
        }

        EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, itemsToShow);

    }

    private void HandleMissionListRes(Cmd.MissionListRes res)
    {
        // Mission_Manager.Instance.update_mission_list_from_s2c(res);
        Global_Game_Manager.Instance.UpdateLoginProcessFlagAfterDataBack(5);

        List<MissionRuntimeData> missions = new List<MissionRuntimeData>();
        foreach (var m in res.Missions)
        {
            MissionRuntimeData mission = new MissionRuntimeData()
            {
                missionId = (int)m.MissionConfigID,
                missionUId = m.MissionUID,
                current = m.Progress,
                status = m.Status
            };
            missions.Add(mission);
        }

        MissionManager.Instance.Init(missions);
    }

    private void HandleMailListRes(Cmd.MailListRes res)
    {
        Email_And_Announcement_Manager.instance.load_mail_list_from_json(res);
        Email_And_Announcement_Manager.instance.on_refresh_mail();
    }

    private void HandleMailReadRes(Cmd.MailReadRes res)
    {
        Email_And_Announcement_Manager.instance.on_refresh_mail();
    }

    private void HandleMailDeleteRes(Cmd.MailDeleteRes res)
    {
        Email_And_Announcement_Manager.instance.on_refresh_mail();
    }

    private void HandleMailRewardRes(Cmd.MailRewardRes res)
    {
        var manager = Email_And_Announcement_Manager.instance;
        if (manager?._temp_mail_id_list != null)
        {
            foreach (var mail_id in manager._temp_mail_id_list)
            {
                var record = manager._mail_record.Find(x => x.mail_id == mail_id);
                if (record != null)
                    record.isGetReward = true;
            }
        }
        manager?.on_refresh_mail();

        if (res?.Items == null || res.Items.Count == 0)
            return;

        List<(string, int)> rewardItems = new List<(string, int)>();
        foreach (var item in res.Items)
        {
            var itemInfo = Global_Inventory_Manager.GetItemInfo((int)item.ConfigID);
            if (itemInfo != null)
                rewardItems.Add((itemInfo.name, (int)item.Count));
        }
        if (rewardItems.Count > 0)
            EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, rewardItems);
    }


    private void HandleGachaListRes(Cmd.GachaListRes res)
    {
        GachaManager.Instance?.ApplyGachaListRes(res);
        Global_Game_Manager.Instance.UpdateLoginProcessFlagAfterDataBack(6);
    }

    /// <summary>
    /// 由 <see cref="Global_Game_Manager.Update"/> 在 MainScene 每帧调用：推迟执行在非主城收到的回家奖励。
    /// </summary>
    public void TryFlushPendingBackToHomeRewardForMainScene()
    {
        if (!SceneLoadHelper.IsMainScene || _pendingBackToHomeReward == null)
            return;

        var pending = _pendingBackToHomeReward;
        _pendingBackToHomeReward = null;
        ApplyBackToHomeRewardFlow(pending);
    }

    /// <summary>
    /// 回家奖励推送：先用 <see cref="Cmd.BackToHomeRewardS2C.PhotoInfo"/> 走派遣拍照（与原先一致），结束后用 <see cref="Cmd.BackToHomeRewardS2C.Rewards"/> 弹派遣奖励窗（与 <see cref="EvtNames.Dispatch_Show_Reward"/> 一致）。
    /// 不在 MainScene 时仅缓存，回到主城后再弹。
    /// </summary>
    private void HandleBackToHomeRewardS2C(Cmd.BackToHomeRewardS2C res)
    {
        if (res == null)
            return;

        if (!SceneLoadHelper.IsMainScene)
        {
            _pendingBackToHomeReward = res.Clone();
            return;
        }

        ApplyBackToHomeRewardFlow(res);
    }

    private void HandlePlantRes(Cmd.PlantRes res)
    {
        PlantPlantMode.HandlePlantCompleted();
    }

    private void HandlePlantOpRes(Cmd.PlantOpRes res)
    {
        FertilizePlantMode.HandleFertilizeCompleted();
        HarvestPlantMode.HandleHarvestCompleted();
        RemovePlantMode.HandleRemoveCompleted();
    }

    private void HandleGetAllPlantRes(Cmd.GetAllPlantRes res)
    {
        try
        {
            PlantManager.Instance?.CacheAllPlants(res?.Plants);
            PlantManager.Instance?.ApplyCachedPlantsToCurrentRoom();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Login] GetAllPlantRes(1352) 处理异常，仍推进必拉 flag(7): {ex}");
        }
        Global_Game_Manager.Instance?.UpdateLoginProcessFlagAfterDataBack(7);
    }

    /// <summary>
    /// 收到新手引导记录
    /// </summary>
    /// <param name="res"></param>
    private void HandleGetGuideRes(Cmd.GetGuideRes res)
    {
        GuideManager.Instance.ReceiveGuideHistory(res.GuideData);
    }

    /// <summary>
    /// 新手引导保存的服务器响应，收到说明保存成功
    /// </summary>
    /// <param name="res"></param>
    private void HandlSaveGuideRes(Cmd.SaveGuideRes res)
    {
        //GuideManager.Instance.ApplySaveGuideRes(res);
    }


    private void HandlePlantChangeS2C(Cmd.PlantChangeS2C res)
    {
        PlantManager.Instance.ApplyPlantChange(res);
    }

    private void ClaimFirstRewardLoginReward(ClaimFirstLoginRewardRes res)
    {
        GuideManager.Instance.StartGuide();
    }

    private void HandleNPCChangeS2C(Cmd.NpcChangeS2C res)
    {
        NPCManager.Instance.ReceiveNpcDataChange(res);
    }

    private void HandleGiveGiftToNpcRes(Cmd.GiveGiftToNpcRes res)
    {
        Log.Sucess("GiveGiftToNpcRes");
    }

    private void ApplyBackToHomeRewardFlow(Cmd.BackToHomeRewardS2C res)
    {
        if (res == null)
            return;

        var rewardItems = BuildDispatchRewardListFromBackToHome(res);

        void ShowDispatchRewardIfAny()
        {
            if (rewardItems.Count == 0)
                return;
            EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Dispatch_Show_Reward, rewardItems);
        }

        var photo = res.PhotoInfo;
        if (photo == null)
        {
            Debug.LogWarning("[BackToHomeRewardS2C] PhotoInfo 为空，跳过派遣拍照。");
            ShowDispatchRewardIfAny();
            return;
        }

        int mapId = photo.MapID;
        int photoConfigId = photo.PhotoConfigID;
        if (mapId <= 0 || photoConfigId <= 0)
        {
            Debug.LogWarning($"[BackToHomeRewardS2C] 无效的 MapID={mapId} 或 PhotoConfigID={photoConfigId}，跳过派遣拍照。");
            ShowDispatchRewardIfAny();
            return;
        }

        var pm = Global_Photo_Manager.Instance;
        if (pm == null)
        {
            Debug.LogWarning("[BackToHomeRewardS2C] Global_Photo_Manager 未初始化，无法拍照。");
            ShowDispatchRewardIfAny();
            return;
        }

        pm.CaptureDispatchPhotoFromServerIds(photo, ShowDispatchRewardIfAny);
    }

    /// <summary>将 <c>BackToHomeRewardS2C.Rewards</c>（<see cref="ItemInfo"/>）转为 <see cref="EvtNames.Dispatch_Show_Reward"/> 所需的列表。</summary>
    private static List<(string, int)> BuildDispatchRewardListFromBackToHome(Cmd.BackToHomeRewardS2C res)
    {
        var list = new List<(string, int)>();
        if (res?.Rewards == null || res.Rewards.Count == 0)
            return list;

        foreach (var it in res.Rewards)
        {
            if (it == null)
                continue;
            if (it.ConfigID == 0UL)
                continue;
            if (it.Count <= 0L)
                continue;
            if (it.ConfigID > (ulong)int.MaxValue)
                continue;

            int cfg = (int)it.ConfigID;
            int cnt = it.Count > int.MaxValue ? int.MaxValue : (int)it.Count;

            var gi = Global_Inventory_Manager.GetItemInfo(cfg);
            string name = gi != null && !string.IsNullOrEmpty(gi.name) ? gi.name : $"道具_{cfg}";
            list.Add((name, cnt));
        }

        return list;
    }


    #endregion

    #region Helper Struct

    [Serializable]
    private class StringArrayWrapper
    {
        public string[] items;
    }

    #endregion

    #region Network Register

    /// <summary>将原 <see cref="receive_msg"/> switch 中的消息类型与实例方法注册到 <see cref="RegisterMap"/>。</summary>
    void RegisterMapProtoHandlers()
    {
        if (!RegisterMap.IsLoaded)
            return;

        RegisterMap.RegisterHandler(typeof(UserVerifyLoginRes), new Action<UserVerifyLoginRes>(DispatchUserVerifyLoginToLoginManager));
        RegisterMap.RegisterHandler(typeof(GetItemBagRes), new Action<GetItemBagRes>(HandleResponseData1012));
        RegisterMap.RegisterHandler(typeof(GetRoomDataRes), new Action<GetRoomDataRes>(ApplyGetRoomDataRes1308));
        RegisterMap.RegisterHandler(typeof(ChangeRoomDataRes), new Action<ChangeRoomDataRes>(ApplyChangeRoomDataRes1310));
        RegisterMap.RegisterHandler(typeof(GetAllNpcInfoRes), new Action<GetAllNpcInfoRes>(HandleResponseData1316));
        RegisterMap.RegisterHandler(typeof(GetCatInfoRes), new Action<GetCatInfoRes>(HandleResponseData1332));
        RegisterMap.RegisterHandler(typeof(CatInfoChangeS2C), new Action<CatInfoChangeS2C>(HandleCatInfoChangeS2C));
        RegisterMap.RegisterHandler(typeof(PackageBagRes), new Action<PackageBagRes>(HandlePackageBagRes));
        RegisterMap.RegisterHandler(typeof(GachaTakeRes), new Action<GachaTakeRes>(HandleGachaTakeRes));
        RegisterMap.RegisterHandler(typeof(CatBackImmediatelyRes), new Action<CatBackImmediatelyRes>(HandleCatBackImmediatelyRes));
        RegisterMap.RegisterHandler(typeof(GetAllShopInfoRes), new Action<GetAllShopInfoRes>(HandleResponseData1064));
        RegisterMap.RegisterHandler(typeof(ManualRefreshShopRes), new Action<ManualRefreshShopRes>(HandleManualRefreshShopRes));
        RegisterMap.RegisterHandler(typeof(BuyGoodsRes), new Action<BuyGoodsRes>(HandleBuyGoodsRes));
        RegisterMap.RegisterHandler(typeof(BuyTicketsRes), new Action<BuyTicketsRes>(HandleBuyTicketsRes));
        RegisterMap.RegisterHandler(typeof(BuyDiamondTimesRes), new Action<BuyDiamondTimesRes>(HandleBuyDiamondTimesRes));
        RegisterMap.RegisterHandler(typeof(SoldItemRes), new Action<SoldItemRes>(HandleSoldItemRes));
        RegisterMap.RegisterHandler(typeof(RoleInfoChangeS2C), new Action<RoleInfoChangeS2C>(HandleResponseData1326));
        RegisterMap.RegisterHandler(typeof(SetRoleInfoRes), new Action<SetRoleInfoRes>(HandleSetRoleInfoRes));
        RegisterMap.RegisterHandler(typeof(ItemChangeS2C), new Action<ItemChangeS2C>(HandleResponseData1328));
        RegisterMap.RegisterHandler(typeof(BackToHomeRewardS2C), new Action<BackToHomeRewardS2C>(HandleBackToHomeRewardS2C));
        RegisterMap.RegisterHandler(typeof(PlantRes), new Action<PlantRes>(HandlePlantRes));
        RegisterMap.RegisterHandler(typeof(PlantOpRes), new Action<PlantOpRes>(HandlePlantOpRes));
        RegisterMap.RegisterHandler(typeof(GetAllPlantRes), new Action<GetAllPlantRes>(HandleGetAllPlantRes));
        RegisterMap.RegisterHandler(typeof(PlantChangeS2C), new Action<PlantChangeS2C>(HandlePlantChangeS2C));
        RegisterMap.RegisterHandler(typeof(EmptyRes), new Action<EmptyRes>(HandleResponseData1322));
        RegisterMap.RegisterHandler(typeof(OpenPhotoBookRes), new Action<OpenPhotoBookRes>(HandleOpenPhotoBookRes));
        RegisterMap.RegisterHandler(typeof(MissionChangeS2C), new Action<MissionChangeS2C>(HandleMissionChangeS2C));
        RegisterMap.RegisterHandler(typeof(MissionListRes), new Action<MissionListRes>(HandleMissionListRes));
        RegisterMap.RegisterHandler(typeof(NpcGiftToPlayerS2C), new Action<NpcGiftToPlayerS2C>(HandleNpcGiftToPlayerS2C));
        RegisterMap.RegisterHandler(typeof(GachaListRes), new Action<GachaListRes>(HandleGachaListRes));
        RegisterMap.RegisterHandler(typeof(MissionRewardsRes), new Action<MissionRewardsRes>(HandleMissionRewardsRes));
        RegisterMap.RegisterHandler(typeof(NpcChangeS2C), new Action<NpcChangeS2C>(HandleNPCChangeS2C));
        RegisterMap.RegisterHandler(typeof(MailListRes), new Action<MailListRes>(HandleMailListRes));
        RegisterMap.RegisterHandler(typeof(MailReadRes), new Action<MailReadRes>(HandleMailReadRes));
        RegisterMap.RegisterHandler(typeof(MailDeleteRes), new Action<MailDeleteRes>(HandleMailDeleteRes));
        RegisterMap.RegisterHandler(typeof(MailRewardRes), new Action<MailRewardRes>(HandleMailRewardRes));
        RegisterMap.RegisterHandler(typeof(FriendsListRes), new Action<FriendsListRes>(HandleFriendsListRes));
        RegisterMap.RegisterHandler(typeof(SearchPlayerRes), new Action<SearchPlayerRes>(HandleSearchPlayerRes));
        RegisterMap.RegisterHandler(typeof(EditFriendsListRes), new Action<EditFriendsListRes>(HandleEditFriendsListRes));
        RegisterMap.RegisterHandler(typeof(EditApplicationListRes), new Action<EditApplicationListRes>(HandleEditApplicationListRes));
        RegisterMap.RegisterHandler(typeof(FriendChangeS2C), new Action<FriendChangeS2C>(HandleFriendChangeS2C));
        RegisterMap.RegisterHandler(typeof(GetGuideRes), new Action<GetGuideRes>(HandleGetGuideRes));
        RegisterMap.RegisterHandler(typeof(SaveGuideRes), new Action<SaveGuideRes>(HandlSaveGuideRes));
        RegisterMap.RegisterHandler(typeof(ClaimFirstLoginRewardRes), new Action<ClaimFirstLoginRewardRes>(ClaimFirstRewardLoginReward));
        RegisterMap.RegisterHandler(typeof(SyncChatChannelInfoS2C), new Action<SyncChatChannelInfoS2C>(HandleSyncChatChannelInfoS2C));
        RegisterMap.RegisterHandler(typeof(OpenOrCloseChatChannelS2C), new Action<OpenOrCloseChatChannelS2C>(HandleOpenOrCloseChatChannelS2C));
        RegisterMap.RegisterHandler(typeof(SyncChatChannelMsgS2C), new Action<SyncChatChannelMsgS2C>(HandleSyncChatChannelMsgS2C));
        RegisterMap.RegisterHandler(typeof(ChatSendRes), new Action<ChatSendRes>(HandleChatSendRes));
        RegisterMap.RegisterHandler(typeof(GiveGiftToNpcRes), new Action<GiveGiftToNpcRes>(HandleGiveGiftToNpcRes));
    }

    private void HandleFriendsListRes(FriendsListRes res) => Player_Social_Receiver.HandleFriendsListRes(res);

    private void HandleSearchPlayerRes(SearchPlayerRes res) => Player_Social_Receiver.HandleSearchPlayerRes(res);

    private void HandleEditFriendsListRes(EditFriendsListRes res) => Player_Social_Receiver.HandleEditFriendsListRes(res);

    private void HandleEditApplicationListRes(EditApplicationListRes res) =>
        Player_Social_Receiver.HandleEditApplicationListRes(res);

    private void HandleFriendChangeS2C(FriendChangeS2C res) =>
        Player_Social_Receiver.HandleFriendChangeS2C(res);

    private void HandleSyncChatChannelInfoS2C(SyncChatChannelInfoS2C res) =>
        Player_Social_Receiver.HandleSyncChatChannelInfoS2C(res);

    private void HandleOpenOrCloseChatChannelS2C(OpenOrCloseChatChannelS2C res) =>
        Player_Social_Receiver.HandleOpenOrCloseChatChannelS2C(res);

    private void HandleSyncChatChannelMsgS2C(SyncChatChannelMsgS2C res) =>
        Player_Social_Receiver.HandleSyncChatChannelMsgS2C(res);

    private void HandleChatSendRes(ChatSendRes res) =>
        Player_Social_Receiver.HandleChatSendRes(res);

    void DispatchUserVerifyLoginToLoginManager(UserVerifyLoginRes res)
    {
        Login_Manager.Instance?.HandleUserVerifyLoginResponse(res);
    }

    void ApplyGetRoomDataRes1308(GetRoomDataRes res)
    {
        if (res?.RoomData == null || res.RoomData.Count == 0)
            Debug.LogWarning("[Login] GetRoomDataRes(1308) 房间数据为空（新角色常见）；仍推进必拉 flag(4)，请确认服务端已回包。");

        RoomSystem_Receiver.ApplyGetRoomDataRes(res);
        Global_Game_Manager.Instance?.UpdateLoginProcessFlagAfterDataBack(4);
    }

    void ApplyChangeRoomDataRes1310(ChangeRoomDataRes res)
    {
        if (RoomSystem_Receiver.Instance != null)
            RoomSystem_Receiver.Instance.ApplyChangeRoomDataRes(res);
        else
            Debug.LogWarning("[Global_Game_Data_Sync_Receiver] 1310 ChangeRoomDataRes：RoomSystem_Receiver.Instance 为空。");
    }

    private void RegisterToNetwork()
    {
        // 若 BeforeSceneLoad / Awake 未执行到（极罕见），再补一次
        if (!RegisterMap.IsLoaded)
            RegisterMap.TryRegisterDefault();

        var net = NetWork_Center_WSS.Instance;
        if (net == null)
            return;

        if (RegisterMap.IsLoaded && RegisterMap.AllMessageIds.Count > 0)
        {
            foreach (var id in RegisterMap.AllMessageIds)
                net.Add_Receiver(id, this);
            RegisterMapProtoHandlers();
            return;
        }
    }

    private void UnregisterFromNetwork()
    {
        var net = NetWork_Center_WSS.Instance;
        if (net == null)
            return;

        if (RegisterMap.IsLoaded && RegisterMap.AllMessageIds.Count > 0)
        {
            foreach (var id in RegisterMap.AllMessageIds)
                net.Remove_Receiver(id);
            return;
        }
    }

    #endregion

}

