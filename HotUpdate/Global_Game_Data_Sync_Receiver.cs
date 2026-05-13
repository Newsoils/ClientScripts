using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

public class Global_Game_Data_Sync_Receiver : SingletonMono<Global_Game_Data_Sync_Receiver>, IMsg_Receiver
{
    private NetWork_Center_WSS _networkCenter;

    private Global_Game_Manager _globalGameManager;
    private Global_Inventory_Manager _inventoryManager;
    private GameItem_DB_SO _inventorySO;
    private Dictionary<string, TaskCompletionSource<string>> responseTasks = new Dictionary<string, TaskCompletionSource<string>>();


    private bool _inventoryInitialSyncCompleted = false;

    private const string ReceiverName = "Global_Game_Manager";

    #region Unity Life Cycle

    private void Start()
    {
        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError("Global_Data_Sync: NetworkCenter not initialized");
            return;
        }

        _globalGameManager = GetComponent<Global_Game_Manager>();
        _inventoryManager = GetComponent<Global_Inventory_Manager>();

        if (_inventoryManager != null)
        {
            _inventorySO = _inventoryManager._itemDB_SO;
        }

        BindUnityEvents();
        RegisterToNetwork();
        EvtDsp.AddEvt<string, string>(EvtNames.Save_Data_To_Server, SaveDataToServer);
        EvtDsp.AddEvt<string, string, Action<string>>(EvtNames.Get_Data_From_Server, GetDataFromServer);
        EvtDsp.AddReturnEvt<string, List<ServerTask>, Action<string>,Task>(EvtNames.Excute_Server_Tasks, ExcuteServerTasksAsync);
        EvtDsp.AddReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, ExcuteServerTaskAsync);
        Debug.Log("Global_Game_Data_Sync_Receiver initialized");
    }

    private void OnDestroy()
    {
        UnbindUnityEvents();
        UnregisterFromNetwork();
        EvtDsp.RemoveEvt<string, string>(EvtNames.Save_Data_To_Server, SaveDataToServer);
        EvtDsp.RemoveEvt<string, string, Action<string>>(EvtNames.Get_Data_From_Server, GetDataFromServer);
        EvtDsp.RemoveReturnEvt<string, List<ServerTask>, Action<string>, Task>(EvtNames.Excute_Server_Tasks, ExcuteServerTasksAsync);
        EvtDsp.RemoveReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, ExcuteServerTaskAsync);
    }

    #endregion

    #region UnityEvent Binding

    private void BindUnityEvents()
    {
        if (_globalGameManager != null)
        {
            _globalGameManager._update_weather_state_from_server
                .AddListener(UpdateWeatherStateFromServer);
            _globalGameManager._upload_weather_state_to_server
                .AddListener(UploadWeatherStateToServer);

            _globalGameManager._on_update_player_brief_from_server
                .AddListener(UpdatePlayerBriefFromServer);
            _globalGameManager._on_upload_player_brief_to_server
                .AddListener(UploadPlayerBriefToServer);
        }

        if (_inventoryManager != null)
        {
            _inventoryManager._update_inventory_from_server
                .AddListener(UpdateInventoryFromServer);
            _inventoryManager._send_inventory_to_server
                .AddListener(SendInventoryToServer);

            _inventoryManager._update_shop_state_from_server
                .AddListener(UpdateShopStateFromServer);
            _inventoryManager._send_shop_state_to_server
                .AddListener(SendShopStateToServer);
        }
    }

    private void UnbindUnityEvents()
    {
        if (_globalGameManager != null)
        {
            _globalGameManager._update_weather_state_from_server
                .RemoveListener(UpdateWeatherStateFromServer);
            _globalGameManager._upload_weather_state_to_server
                .RemoveListener(UploadWeatherStateToServer);

            _globalGameManager._on_update_player_brief_from_server
                .RemoveListener(UpdatePlayerBriefFromServer);
            _globalGameManager._on_upload_player_brief_to_server
                .RemoveListener(UploadPlayerBriefToServer);
        }

        if (_inventoryManager != null)
        {
            _inventoryManager._update_inventory_from_server
                .RemoveListener(UpdateInventoryFromServer);
            _inventoryManager._send_inventory_to_server
                .RemoveListener(SendInventoryToServer);

            _inventoryManager._update_shop_state_from_server
                .RemoveListener(UpdateShopStateFromServer);
            _inventoryManager._send_shop_state_to_server
                .RemoveListener(SendShopStateToServer);
        }
    }

    #endregion

    #region Network Send Helper

    private void SendMsg(string action, string detail)
    {
        if (_networkCenter == null || !_networkCenter._connect_to_player_server)
            return;

        Network_Msg msg = new Network_Msg
        {
            sender = ReceiverName,
            action_target = "Player_Server",
            action = action,
            detail_info = detail,
            _sending_mode = Msg_Sending_Mode.Client_to_Server
        };

        _networkCenter.send_via_wss(msg);
    }

    #endregion

    #region Client -> Server
    private void SaveDataToServer(string action, string data)
    {
        SendMsg(action, data);
    }
    private void GetDataFromServer(string action, string target, Action<string> onTaskComplete)
    {
        _ = GetDataFromServerAsync(action, target, onTaskComplete);
    }
    private async Task<string> GetDataFromServerAsync(string action, string target, Action<string> onTaskComplete)
    {
        SendMsg(action, target);
        TaskCompletionSource<string> result = new TaskCompletionSource<string>();
        responseTasks.Add(action, result);
        // 2 秒在真机/退后台恢复时太激进（主线程卡顿/网络抖动都会误判断线）
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(6));
        var completedTask = await Task.WhenAny(result.Task, timeoutTask);
        if (completedTask == timeoutTask)
        {
            responseTasks.Remove(action);
            Debug.Log($"获取服务器{action}数据超时");
            var nc = NetWork_Center_WSS.instance;

            // 连接还在（ws 未断），但某个请求超时：视为“软失败”，不要直接判定断线并弹窗/踢回登录
            bool connectionSeemsAlive = nc != null && nc._is_connected;

            // App 切后台/失焦/恢复宽限期/正在重连时：一律不弹断线窗
            bool shouldSuppressDisconnectPrompt = nc != null ? nc.ShouldSuppressDisconnectPrompts : !Application.isFocused;

            if (!shouldSuppressDisconnectPrompt && !connectionSeemsAlive)
            {
                EvtDsp.TriggerEvt<string, Action>(
                    EvtNames.ShowPrompt,
                    "与服务器断开连接，请检查网络设置！",
                    () => EvtDsp.TriggerEvt(EvtNames.Network_Disconnect)
                );
            }
            onTaskComplete?.Invoke(null);
            return null;
        }
        else
        {
            responseTasks.Remove(action);
            if(result.Task.Result == null)
            {
                onTaskComplete?.Invoke("nodata");
            }
            onTaskComplete?.Invoke(result.Task.Result);
            return result.Task.Result;
        }
    }
    private async Task ExcuteServerTaskAsync(string action, ServerTask task, Action<string> onTasksComplete)
    {
        List<ServerTask> tasks = new List<ServerTask> { task };
        await ExcuteServerTasksAsync(action, tasks, onTasksComplete);
    }
    private async Task ExcuteServerTasksAsync(string action, List<ServerTask> tasks, Action<string> onTasksComplete)
    {
        string result = "success";
        string data = JsonConvert.SerializeObject(tasks.Select(task => task.data).ToList());

        string receive = await GetDataFromServerAsync(action, data, null);
        List<(bool,string)> receives = JsonConvert.DeserializeObject<List<(bool, string)>>(receive);
        //检查是否执行失败，执行失败时只执行最终失败的任务
        for (int i = 0; i < receives.Count; i++)
        {
            if (receives[i].Item1 == false)
            {
                tasks[i].onReceiveMsg.Invoke(receives[i].Item2, tasks[i]);
                onTasksComplete?.Invoke(tasks[i].result);
                return;
            }
        }
        //逐个执行任务
        for (int i = 0; i < receives.Count; i++)
        {
            if (!tasks[i].isSendToServer)
            {
                tasks[i].onReceiveMsg.Invoke("", tasks[i]);
            }
            else
            {
                tasks[i].onReceiveMsg.Invoke(receives[i].Item2, tasks[i]);
            }
            if (tasks[i].isBreak)
            {
                result = tasks[i].result;
                onTasksComplete?.Invoke(result);
                return;
            }
        }

        onTasksComplete?.Invoke(result);
       
    }
    private void UpdateInventoryFromServer()
    {
        SendMsg("Get_Data", "Game_Inventory");
    }

    private void SendInventoryToServer()
    {
        if (_inventorySO == null) return;

        //等待从服务器拉取完毕后，才可以向服务器传输物品栏，防止错误的覆盖服务器数据
        if (!_inventoryInitialSyncCompleted)
        {
            Log.Info("Inventory initial sync not completed yet, postpone upload.");
            return;
        }

        string dataJson = Global_Inventory_Manager.Inventory_Serialization();
        string lastJson = GF_SP.SerializeObject(new List<string>() { "Game_Inventory", dataJson });

        SendMsg("Save_Data",lastJson) ;
    }
    private void UpdateWeatherStateFromServer()
    {
        SendMsg("Get_Data", "Weather_State");
    }

    private void UploadWeatherStateToServer()
    {
        if (_globalGameManager == null) return;

        string json = GF_SP.SerializeObject(_globalGameManager._weather_state);

        var lastData = GF_SP.SerializeObject( new List<string>() { "Weather_State", json });

        SendMsg("Save_Data", lastData );
    }

    private void UpdateShopStateFromServer()
    {
        SendMsg("Get_Data", "Shop_State");
    }

    private void SendShopStateToServer()
    {
        if (_inventoryManager == null) return;

        string json = _inventoryManager.get_shop_state_json();
        var lastData = GF_SP.SerializeObject( new List<string>() { "Shop_State", json });
        SendMsg("Save_Data", lastData);
    }

    private void UpdatePlayerBriefFromServer()
    {
        SendMsg("Get_Data", "Player_Brief");
    }

    private void UploadPlayerBriefToServer()
    {
        if (_globalGameManager == null) return;

        string json = GF_SP.SerializeObject(_globalGameManager._player_brief);
        var lastData = GF_SP.SerializeObject( new List<string>() { "Player_Brief", json });
        SendMsg("Save_Data", lastData);
    }

    public void On_Login_Success()
    {
        _networkCenter._connect_to_player_server = true;
        StartCoroutine(LoginSequence());
    }

    private IEnumerator LoginSequence()
    {
        yield return new WaitForSeconds(1.024f);
        UpdateInventoryFromServer();

        yield return new WaitForSeconds(1.024f);
        UpdateWeatherStateFromServer();

        yield return new WaitForSeconds(1.024f);
        UpdateShopStateFromServer();

        yield return new WaitForSeconds(1.024f);
        StartRefreshState();

        yield return new WaitForSeconds(1.024f);
        UpdatePlayerBriefFromServer();
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

            _globalGameManager?.try_update_weahter();
            yield return new WaitForSeconds(1.024f);

            _inventoryManager?.try_refresh_shop_state();
            yield return new WaitForSeconds(60f);
        }
    }

    #endregion

    #region IMsg_Receiver
    public string get_receiver_name()
    {
        return ReceiverName;
    }

    public void receive_msg(Network_Msg msg)
    {
        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", ReceiverName);
        if (msg.action == "Quit_Game")
        {
            _globalGameManager.quit_game(msg.detail_info, 5.0f);
            return;
        }

        if (msg.action == "Response_Data")
        {
            HandleResponseData(msg.detail_info);
        }
        if(responseTasks.TryGetValue(msg.action, out var task))
        {
            task.SetResult(msg.detail_info);
        }
        Msg_Dispatcher._instance._is_locking = false;
    }

    private void HandleResponseData(string json)
    {
        var data = GF_SP.DeserializeObject<List<string>>(json);
        if (data == null || data.Count != 2)
            return;

        string key = data[0];
        string payload = data[1];

        switch (key)
        {
            case "Game_Inventory":
                _inventoryInitialSyncCompleted = true;
                _inventoryManager?.Load_Data_From_Json(payload);
                break;

            case "Weather_State":
                _globalGameManager?.load_weather_state_from_json(payload);
                _globalGameManager?.try_update_weahter();
                break;

            case "Shop_State":
                _inventoryManager?.load_shop_state_from_json(payload);
                _inventoryManager?.try_refresh_shop_state();
                break;

            case "Player_Brief":
                _globalGameManager?.load_player_brief_from_json(payload);
                break;
        }
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

    private void RegisterToNetwork()
    {
        _networkCenter?.Add_Receiver(ReceiverName, this);
    }

    private void UnregisterFromNetwork()
    {
        _networkCenter?.Remove_Receiver(ReceiverName);
    }

    #endregion

}

