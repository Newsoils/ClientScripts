using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using ResourceMgr = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

public class Login_Manager : SingletonMono<Login_Manager> , IMsg_Receiver
{
    private NetWork_Center_WSS _networkCenter;
    private string _preLoginName;
    private string receiver_name = "Login_Manager";


    private float _lastLoginTime = -10f; // 记录上次发送时间
    public float loginCooldown = 1.5f;    // 冷却时间（秒）

    private void Start()
    {
        _networkCenter = NetWork_Center_WSS.instance;
        _networkCenter?.Add_Receiver(receiver_name, this);
        DontDestroyOnLoad(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _networkCenter?.Remove_Receiver(receiver_name);
    } 

    #region Login

    public void TryLogin( string playerId,string password)
    {

        // 时间拦截：如果当前时间距离上次发送不足 loginCooldown，则拦截
        if (Time.time - _lastLoginTime < loginCooldown)
        {
            Debug.Log("请求太频繁，请稍后再试");
            return;
        }

        if (_networkCenter == null || !_networkCenter._is_connected)
            return;

        if (_networkCenter._connect_to_player_server)
            return;


        if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(password))
            return;
        _lastLoginTime = Time.time; // 更新发送时间

        Log.Info("TryLoin");

        _preLoginName = playerId;

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

        if (msg.action != "Login_Success")
            return;

        Log.Custom("Login Success", receiver_name,Color.green);
        OnLoginSuccess(msg);
    }

    #endregion

    #region Login Success Chain

    private void OnLoginSuccess(Network_Msg msg)
    {
        // 1. 网络状态
        _networkCenter.set_player_name(_preLoginName);
        _networkCenter._connect_to_player_server = true;

        // 2. Global Game Manager
        Global_Game_Manager._instance.set_up_player_info(msg);
        Global_Game_Data_Sync_Receiver.Instance.On_Login_Success();

        // 3. Player Social
        Invoke(nameof(InitPlayerSocial), 1.0f);

        // 4. Email & Announcement（⚠ 你之前断的就是这里）
        Invoke(nameof(InitEmailAndAnnouncement), 2.0f);

        // 5. Quest / Achievement
        Invoke(nameof(InitQuestAndAchievement), 3.0f);


        // 6. Load Scene
        ResourceMgr.load_scene_async("Scenes/MainScene");

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
