using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

public class Login_Manager : SingletonMono<Login_Manager> , IMsg_Receiver
{
    private NetWork_Center_WSS _networkCenter;
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

    private float _lastLoginTime = -10f; // 记录上次发送时间
    public float loginCooldown = 1.5f;    // 冷却时间（秒）

    private void Start()
    {
        _networkCenter = NetWork_Center_WSS.instance;
        _networkCenter?.Add_Receiver(receiver_name, this);
        EvtDsp.AddEvt(EvtNames.Resume_TryLogin_From_Cache, TryResumeLoginFromCache);
        DontDestroyOnLoad(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EvtDsp.RemoveEvt(EvtNames.Resume_TryLogin_From_Cache, TryResumeLoginFromCache);
        _networkCenter?.Remove_Receiver(receiver_name);
    } 

    #region Login

    public void TryLogin(string playerId,string password)
    {
        TryLoginInternal(playerId, password, flowMode: LoginFlowMode.Normal, force: false);
    }

    public void TryLoginForResume(string playerId, string password)
    {
        // 主界面恢复时：允许强制重新登录（即使之前 _connect_to_player_server 为 true）
        TryLoginInternal(playerId, password, flowMode: LoginFlowMode.ResumeRelogin, force: true);
    }

    public void TryResumeLoginFromCache()
    {
        if (string.IsNullOrEmpty(_preLoginName) || string.IsNullOrEmpty(_preLoginPassword))
            return;

        TryLoginForResume(_preLoginName, _preLoginPassword);
    }

    private void TryLoginInternal(string playerId, string password, LoginFlowMode flowMode, bool force)
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

        if (_networkCenter == null || !_networkCenter._is_connected)
            return;

        if (!force && _networkCenter._connect_to_player_server)
            return;


        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(password))
            return;
        _lastLoginTime = Time.time; // 更新发送时间

        Log.Info("TryLoin");

        _preLoginName = playerId;
        _preLoginPassword = password;
        _pendingFlowMode = flowMode;
        _login_inflight = true;
        _login_inflight_start_time = Time.unscaledTime;
        _login_inflight_player_id = playerId;

        Network_Msg msg = new Network_Msg
        {
            player_id = playerId,
            sender = receiver_name,
            action_target = "Login_Manager",
            action = "Try_Login",
            detail_info = GF_SP.SerializeObject(new[] { playerId, password }),
            _sending_mode = Msg_Sending_Mode.Client_to_Server
        };

        _networkCenter.send_via_wss(msg);
    }

    #endregion

    #region IMsg_Receiver

    public string get_receiver_name() => receiver_name;

    public void receive_msg(Network_Msg msg)
    {
        EvtDsp.TriggerEvt<string>(EvtNames.Login_Messsage, msg.action);

        // 收到任何 Login_* 响应都释放 in-flight 锁（成功/失败都算一次完成）
        if (_login_inflight && msg.action != null && msg.action.Contains("Login"))
        {
            _login_inflight = false;
            _login_inflight_player_id = null;
        }

        if (msg.action != "Login_Success") return;

        Log.Custom("Login Success", receiver_name,Color.green);
        var mode = _pendingFlowMode;
        _pendingFlowMode = LoginFlowMode.Normal;
        OnLoginSuccess(msg, mode);
    }

    #endregion

    #region Login Success Chain

    private void OnLoginSuccess(Network_Msg msg, LoginFlowMode mode)
    {
        // 1. 网络状态
        _networkCenter.set_player_name(_preLoginName);
        _networkCenter._connect_to_player_server = true;

        // 2. Global Game Manager
        Global_Game_Manager.Instance.set_up_player_info(msg);
        Global_Game_Data_Sync_Receiver.Instance.On_Login_Success();

        // 恢复场景后的隐式重登：只刷新数据，不做切场景/界面链路
        if (mode == LoginFlowMode.ResumeRelogin)
        {
            Msg_Dispatcher._instance._is_locking = false;
            return;
        }

        // 3. Player Social
        Invoke(nameof(InitPlayerSocial), 1.0f);

        // 4. Email & Announcement（⚠ 你之前断的就是这里）
        Invoke(nameof(InitEmailAndAnnouncement), 2.0f);

        // 5. Quest / Achievement
        Invoke(nameof(InitQuestAndAchievement), 3.0f);


        // 6. Load Scene
        //ResourceMgr.load_scene_async("Scenes/MainScene");
        SceneLoadingHelper.Load_MainScene();

        Msg_Dispatcher._instance._is_locking = false;
    }

    private void InitPlayerSocial()
    {
        Player_Social_Manager._instance.on_update_social_info_from_server();
    }

    private void InitEmailAndAnnouncement()
    {
        Email_And_Announcement_Receiver.Instance.Init_Data();
    }

    private void InitQuestAndAchievement()
    {
        Quest_And_Achievement_Receiver.Instance.UpdateAchievementInfoFromServer();
    }

    #endregion

}
