using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Network;
using UnityEngine;
using UnityEngine.Video;


public class Login_Manager : SingletonMono<Login_Manager>
{
    protected override bool PersistAcrossScenes => true;
    private string _preLoginName;
    private string _preLoginPassword;
    private string receiver_name = "Login_Manager";

    [Header("Login Dedup")]
    [SerializeField] private float _login_inflight_timeout_seconds = 8.0f;
    private bool _login_inflight = false;
    private float _login_inflight_start_time = -999f;
    private string _login_inflight_player_id;

    private enum LoginFlowMode
    {
        Normal,
        ResumeRelogin
    }

    private LoginFlowMode _pendingFlowMode = LoginFlowMode.Normal;

    /// <summary>HTTP 取 token 后先换网关再连 WSS：等 <see cref="EvtNames.WS_Open"/> 再发 1003。</summary>
    private bool _pendingTryLoginAfterWsOpen;
    private string _pendingWsOpenPlayerId;
    private string _pendingWsOpenPassword;
    private string _pendingWsOpenToken;

    /// <summary>最近一次用于 1003 的 HTTP Token（重连后 WS 再次 Open 时复用）。</summary>
    private string _lastHttpLoginToken;

    private float _lastLoginTime = -10f; // 记录上次发送时间
    public float loginCooldown = 1.5f;    // 冷却时间（秒）

    protected override void Awake()
    {
        base.Awake();
        // 与 RegisterMap 的 BeforeSceneLoad 预加载配合；若从非登录场景进 Play 仍保证已加载
        RegisterMap.TryRegisterDefault();
    }

    private void Start()
    {
        EvtDsp.AddEvt(EvtNames.Resume_TryLogin_From_Cache, TryResumeLoginFromCache);
        EvtDsp.AddEvt(EvtNames.WS_Open, OnWebSocketOpened);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EvtDsp.RemoveEvt(EvtNames.Resume_TryLogin_From_Cache, TryResumeLoginFromCache);
        EvtDsp.RemoveEvt(EvtNames.WS_Open, OnWebSocketOpened);
        //_networkCenter?.Remove_Receiver(receiver_name);
        // _networkCenter?.Remove_Receiver(1004);
    }

    /// <summary>在即将 <c>init_wss</c> 前调用；连接就绪后由 <see cref="EvtNames.WS_Open"/> → <see cref="OnWebSocketOpened"/> 发 1003。</summary>
    public void QueueTryLoginAfterWebSocketOpen(string playerId, string password, string token)
    {
        _pendingTryLoginAfterWsOpen = true;
        _pendingWsOpenPlayerId = playerId;
        _pendingWsOpenPassword = password;
        _pendingWsOpenToken = token;
    }

    /// <summary>连接超时等放弃等待时调用，避免稍后误发 1003。</summary>
    public void CancelPendingTryLoginAfterWebSocket()
    {
        _pendingTryLoginAfterWsOpen = false;
        _pendingWsOpenPlayerId = null;
        _pendingWsOpenPassword = null;
        _pendingWsOpenToken = null;
    }

    private void OnWebSocketOpened()
    {
        if (_pendingTryLoginAfterWsOpen)
        {
            string pid = _pendingWsOpenPlayerId;
            string pwd = _pendingWsOpenPassword;
            string tok = _pendingWsOpenToken;
            _pendingTryLoginAfterWsOpen = false;
            _pendingWsOpenPlayerId = null;
            _pendingWsOpenPassword = null;
            _pendingWsOpenToken = null;

            TryLogin(pid, pwd, tok);
            return;
        }

        // 重连：TapTap 首次排队已消费，但 WsClient 再次 OnOpen → WS_Open 时仍需再发 1003（须 force 绕过 connect_to_player_server 拦截）
        if (!NetWork_Center_WSS.IsWsTransportConnected)
            return;
        if (string.IsNullOrEmpty(_preLoginName) || string.IsNullOrEmpty(_preLoginPassword))
            return;

        if (!string.IsNullOrEmpty(_lastHttpLoginToken))
        {
            Debug.Log("[Login] WS_Open（重连）：使用缓存 Token 再次 TryLogin（1003）");
            TryLoginInternal(_preLoginName, _preLoginPassword, LoginFlowMode.ResumeRelogin, force: true, token: _lastHttpLoginToken);
        }
        else
        {
            Debug.Log("[Login] WS_Open（重连）：无缓存 Token，TryLoginForResume");
            TryLoginForResume(_preLoginName, _preLoginPassword);
        }
    }

    #region Login

    public void TryLogin(string playerId, string password, string token = null)
    {
        TryLoginInternal(playerId, password, flowMode: LoginFlowMode.Normal, force: false, token: token);
    }

    public void TryLoginForResume(string playerId, string password)
    {
        // 主界面恢复时：允许强制重新登录（即使之前 _connect_to_player_server 为 true）
        TryLoginInternal(playerId, password, flowMode: LoginFlowMode.ResumeRelogin, force: true, token: null);
    }

    public void TryResumeLoginFromCache()
    {
        if (string.IsNullOrEmpty(_preLoginName) || string.IsNullOrEmpty(_preLoginPassword))
            return;

        TryLoginForResume(_preLoginName, _preLoginPassword);
    }

    public bool TryGetCachedCredentials(out string playerId, out string password)
    {
        playerId = _preLoginName;
        password = _preLoginPassword;
        return !string.IsNullOrEmpty(playerId) && !string.IsNullOrEmpty(password);
    }

    private void TryLoginInternal(string playerId, string password, LoginFlowMode flowMode, bool force, string token = null)
    {
        // in-flight 去重：同一时刻只允许一个 Try_Login 在路上
        if (_login_inflight)
        {
            var elapsed = Time.unscaledTime - _login_inflight_start_time;
            if (elapsed < _login_inflight_timeout_seconds)
            {
                // 如果是同账号重复触发（最常见：OnFocus/OnPause 连续触发或 TapTap+Cache 双触发），直接丢弃
                if (string.Equals(_login_inflight_player_id, playerId))
                {
                    Debug.Log($"TryLogin dedup: in-flight for {playerId}, elapsed={elapsed:0.00}s");
                    return;
                }

                // 不同账号的并发登录也先拦截，避免协议状态乱
                Debug.Log($"TryLogin blocked: another login in-flight ({_login_inflight_player_id}), elapsed={elapsed:0.00}s");
                return;
            }

            // 超时放行（防止卡死）
            Debug.LogWarning($"TryLogin in-flight timeout, releasing lock. player={_login_inflight_player_id}, elapsed={elapsed:0.00}s");
            _login_inflight = false;
            _login_inflight_player_id = null;
        }

        // 时间拦截：如果当前时间距离上次发送不足 loginCooldown，则拦截
        if (!force && Time.time - _lastLoginTime < loginCooldown)
        {
            Debug.Log("请求太频繁，请稍后再试");
            return;
        }

        if (!NetWork_Center_WSS.IsWsTransportConnected)
        {
            Debug.LogWarning("[Login] TryLogin 放弃：WebSocket 未连接");
            return;
        }

        if (!force && NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            Debug.LogWarning("[Login] TryLogin 放弃：已标记 connect_to_player_server（需 TryLoginForResume(force) 才会重发）");
            return;
        }

        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("[Login] TryLogin 放弃：playerId/password 为空");
            return;
        }
        _lastLoginTime = Time.time; // 更新发送时间

        Log.Info("TryLoin");

        _preLoginName = playerId;
        _preLoginPassword = password;
        if (!string.IsNullOrEmpty(token))
            _lastHttpLoginToken = token;
        _pendingFlowMode = flowMode;
        _login_inflight = true;
        _login_inflight_start_time = Time.unscaledTime;
        _login_inflight_player_id = playerId;

        // 先发送 WS token 鉴权消息（服务器约定格式）
        if (!string.IsNullOrEmpty(token))
        {
            var verifyReq = new Cmd.UserVerifyLoginReq
            {
                UserID = playerId ?? string.Empty,
                Token = token
            };
            Debug.Log("[Login] 已发送 UserVerifyLogin（msg_id=1003），detail_info 为对象；请服务端回 **msg_id=1004** 且为可解析 Network_Msg JSON。");
            NetWork_Center_WSS.SendMsg(verifyReq);
        }
        else
        {
            Debug.LogWarning("[Login] token 为空，未发送 1003 鉴权；若必须 Token 登录，请检查 HTTP 登录是否返回 Payload.Token。");
        }

        // Network_Msg msg = new Network_Msg
        // {
        //     player_id = playerId,
        //     sender = receiver_name,
        //     action_target = "Login_Manager",
        //     action = "Try_Login",
        //     detail_info = GF_SP.SerializeObject(new[] { playerId, password }),
        //     _sending_mode = Msg_Sending_Mode.Client_to_Server
        // };

        // _networkCenter.send_via_wss(msg);
    }

    #endregion

    /// <summary>由 <see cref="Global_Game_Data_Sync_Receiver"/> 在收到 msg_id=1004 时转发。</summary>
    public void HandleUserVerifyLoginResponse(Cmd.UserVerifyLoginRes protoRes)
    {

        // 仅 Login_Manager 订阅 1004：用 msg_id 释放 in-flight（与 action 字段无关）
        if (_login_inflight)
        {
            _login_inflight = false;
            _login_inflight_player_id = null;
        }

        if (protoRes == null)
        {
            Log.Custom("Login Failed");
            if (Msg_Dispatcher.Instance != null)
                Msg_Dispatcher.Instance._is_locking = false;
            return;
        }

        Log.Custom("Login Success", receiver_name, Color.green);
        OnLoginSuccess(protoRes);

        // 与旧「action=Login_Success」解耦：Dispatch/UI 仍监听约定字符串；新协议只保证 msg_id=1004 + detail_info
        EvtDsp.TriggerEvt<string>(EvtNames.Login_Messsage, "Login_Success");
    }

    #region Login Success Chain

    private void OnLoginSuccess(Cmd.UserVerifyLoginRes msg)
    {
        // 1. 网络状态
        var net = NetWork_Center_WSS.Instance;
        if (net != null)
        {
            net.set_player_name(_preLoginName);
            net._connect_to_player_server = true;
        }

        // // 2. Global Game Manager（先与 DataSync 同物体上的权威组件对齐，再读写 Instance，避免登录链与 Receiver.Start 顺序导致的「两个 GGM」）
        // var recv = FindObjectOfType<Global_Game_Data_Sync_Receiver>(true);
        // var sceneGgm = recv != null ? recv.GetComponent<Global_Game_Manager>() : null;
        // if (sceneGgm != null)
        //     SingletonMono<Global_Game_Manager>.ForceReplaceInstance(sceneGgm);

        if (Global_Game_Manager.Instance == null)
        {
            Debug.LogError("[Login] Global_Game_Manager.Instance 为空：场景中未找到 Global_Game_Manager（可与 Global_Game_Data_Sync_Receiver 同物体挂载）；已取消自动 new，请检查引导场景。");
            return;
        }

        Global_Game_Manager.Instance.set_up_player_info(msg);


        var ggm = Global_Game_Manager.Instance;
        // 会清空 ggm._current_cat_info；随后 ExecInitDataReqSend 发 GetCatInfoReq，1332 回包后再写入 Cat。
        ggm.ClearLoginInitState();
        ggm.RegisterInitDataAfterLogin(0, new Cmd.GetItemBagReq()); //拉取背包
        ggm.RegisterInitDataAfterLogin(1, new Cmd.GetCatInfoReq()); //拉取小猫信息
        ggm.RegisterInitDataAfterLogin(2, new Cmd.GetAllNpcInfoReq()); //拉取NPC信息
        ggm.RegisterInitDataAfterLogin(4, new Cmd.GetRoomDataReq()); //拉取房间信息
        ggm.RegisterInitDataAfterLogin(5, new Cmd.MissionListReq()); //拉取任务信息
        ggm.RegisterInitDataAfterLogin(6, new Cmd.GachaListReq()); //拉取扭蛋信息
        ggm.RegisterInitDataAfterLogin(7, new Cmd.GetAllPlantReq()); //拉取植物信息

        ggm.ExecInitDataReqSend();
		Msg_Dispatcher.Instance._is_locking = false;
    }

 

    #endregion

}
