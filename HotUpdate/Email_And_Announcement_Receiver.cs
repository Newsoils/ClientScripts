using System;
using System.Collections;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

public class Email_And_Announcement_Receiver : SingletonMono<Email_And_Announcement_Receiver> , IMsg_Receiver
{
    public Email_And_Announcement_Manager  _manager;
    private NetWork_Center_WSS _networkCenter;
    private string receiver_name = "Email_And_Announcement_Receiver";

    #region Unity Life Cycle

    private void Start()
    {
        _manager = GetComponent<Email_And_Announcement_Manager>();
        if (_manager == null)
        {
            Debug.LogError("Email_Receiver: Email_Announcement_Manager not found");
            return;
        }

        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError("Email_Receiver: NetworkCenter not initialized");
            return;
        }

        BindUnityEvents();
        RegisterToNetwork();

        Log.Info($"Email_And_Announcement_Receiver initialized on {_manager.gameObject.name}");
    }

    private void OnDestroy()
    {
        UnbindUnityEvents();
        UnregisterFromNetwork();
    }

    #endregion

    #region UnityEvent Binding

    private void BindUnityEvents()
    {
        _manager._update_mail_from_server.AddListener(UpdateMailFromServer);
        _manager._update_annoncement_from_server.AddListener(UpdateAnnoFromServer);
        _manager._on_read_mail.AddListener(OnReadMail);
        _manager._on_delete_mail.AddListener(OnDeleteMail);
        _manager._on_get_mail_reward.AddListener(OnGetMailReward);
    }

    private void UnbindUnityEvents()
    {
        _manager._update_mail_from_server.RemoveListener(UpdateMailFromServer);
        _manager._update_annoncement_from_server.RemoveListener(UpdateAnnoFromServer);
        _manager._on_read_mail.RemoveListener(OnReadMail);
        _manager._on_delete_mail.RemoveListener(OnDeleteMail);
        _manager._on_get_mail_reward.RemoveListener(OnGetMailReward);
    }

    #endregion

    #region Network Send Helpers

    private void SendMsg(string action, string detail)
    {
        if (_networkCenter == null || !_networkCenter._connect_to_player_server)
            return;

        Network_Msg msg = new Network_Msg
        {
            sender = receiver_name,
            action_target = "Player_Server",
            action = action,
            detail_info = detail,
            _sending_mode = Msg_Sending_Mode.Client_to_Server
        };

        _networkCenter.send_via_wss(msg);
    }

    #endregion

    #region Client -> Server

    private void UpdateMailFromServer()
    {
        SendMsg("Get_Mail", string.Empty);
    }

    private void UpdateAnnoFromServer()
    {
        SendMsg("Get_Anno", string.Empty);
    }

    private void OnReadMail()
    {
        SendMailIdList("On_Read_Mail");
    }

    private void OnDeleteMail()
    {
        SendMailIdList("On_Delete_Mail");
    }

    private void OnGetMailReward()
    {
        SendMailIdList("On_Get_Mail_Reward");
    }

    private void SendMailIdList(string action)
    {
        if (_manager == null)
            return;

        var list = _manager._temp_mail_id_list;
        if (list == null || list.Count == 0)
            return;

        string json = GF_SP.SerializeObject(list);
        SendMsg(action, json);
    }

    #endregion

    #region IMsg_Receiver (Server -> Client)

    public string get_receiver_name()
    {
        return receiver_name;
    }

    public void receive_msg(Network_Msg msg)
    {
        if (_manager == null)
            return;
        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", receiver_name);

        switch (msg.action)
        {
            case "Response_Mail":
                _manager.load_mail_list_from_json(msg.detail_info);
                _manager.on_refresh_mail();
                break;

            case "Response_Anno":
                _manager.load_announcement_list_from_json(msg.detail_info);
                _manager.on_refresh_announcement();
                break;

            default:
                Debug.LogWarning($"Email_Receiver: Unknown action {msg.action}");
                break;
        }
    }

    public void Init_Data()
    {
        Log.Info("Email_And_Announcement_Receiver: Init_Data called");
        StartCoroutine(Init_Data_Corotine());
    }

    private IEnumerator Init_Data_Corotine()
    {
        yield return new WaitForSeconds(1f);
        UpdateMailFromServer();

        yield return new WaitForSeconds(1f);
        UpdateAnnoFromServer();
    }

    #endregion

    #region Network Register

    private void RegisterToNetwork()
    {
        _networkCenter?.Add_Receiver(receiver_name, this);
    }

    private void UnregisterFromNetwork()
    {
        _networkCenter?.Remove_Receiver(receiver_name);
    }

    #endregion
}
