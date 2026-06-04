using System;
using System.Collections;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using Google.Protobuf;
using CLIP.Project_Mouse.Network;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Email_And_Announcement_Receiver : SingletonMono<Email_And_Announcement_Receiver>
{
    public Email_And_Announcement_Manager  _manager;

    #region Unity Life Cycle

    private void Start()
    {
        _manager = GetComponent<Email_And_Announcement_Manager>();
        if (_manager == null)
        {
            Debug.LogError("Email_Receiver: Email_Announcement_Manager not found");
            return;
        }

        BindUnityEvents();

        Log.Info($"Email_And_Announcement_Receiver initialized on {_manager.gameObject.name}");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnbindUnityEvents();
    }

    #endregion

    #region UnityEvent Binding

    private void BindUnityEvents()
    {
        _manager._update_mail_from_server.AddListener(UpdateMailFromServer);
        _manager._on_read_mail.AddListener(OnReadMail);
        _manager._on_delete_mail.AddListener(OnDeleteMail);
        _manager._on_get_mail_reward.AddListener(OnGetMailReward);
    }

    private void UnbindUnityEvents()
    {
        _manager._update_mail_from_server.RemoveListener(UpdateMailFromServer);
        _manager._on_read_mail.RemoveListener(OnReadMail);
        _manager._on_delete_mail.RemoveListener(OnDeleteMail);
        _manager._on_get_mail_reward.RemoveListener(OnGetMailReward);
    }

    #endregion

    #region Network Send Helpers

    // private void SendMsg(int msg_id, string detail)
    // {
    //     if (_networkCenter == null || !_networkCenter._connect_to_player_server)
    //         return;

    //     Network_Msg msg = new Network_Msg
    //     {
    //         msg_id = msg_id,
    //         action_target = "Player_Server",
    //         detail_info = detail,
    //         _sending_mode = Msg_Sending_Mode.Client_to_Server
    //     };

    //     _networkCenter.send_via_wss(msg);
    // }

    #endregion

    #region Client -> Server

    private void UpdateMailFromServer()
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.MailListReq());
    }

    private async Task UpdateAnnoFromServer()
    {
        var reqBody = new
        {
            cmd = 5,
            payload = new
            {
                ModuleType = 2,
                ServerID = 1
            }
        };
        string reqJson = JsonConvert.SerializeObject(reqBody);

        string respJson = await TapTapLoginManager.PostJsonAsync(
            TapTapLoginManager.Instance.accountLoginPostUrl, reqJson);
        if (string.IsNullOrWhiteSpace(respJson))
        {
            Debug.LogError("[Email_And_Announcement_Receiver] 获取公告 POST 返回为空");
            return;
        }

        try
        {
            var root = JObject.Parse(respJson);
            int errorCode = root.Value<int?>("ErrorCode") ?? -1;
            if (errorCode != 0)
            {
                Debug.LogError(
                    $"[Email_And_Announcement_Receiver] 获取公告失败 ErrorCode={errorCode} resp={respJson}");
                return;
            }

            var syss = root.SelectToken("Payload.Syss") as JArray;
            if (syss == null)
            {
                Debug.LogWarning(
                    "[Email_And_Announcement_Receiver] 回包中无 Payload.Syss，已清空公告列表");
                syss = new JArray();
            }

            var manager = _manager != null ? _manager : Email_And_Announcement_Manager.instance;
            if (manager == null)
            {
                Debug.LogError("[Email_And_Announcement_Receiver] Email_And_Announcement_Manager 为空，无法写入公告");
                return;
            }

            manager.load_announcement_list_from_syss(syss);

            try
            {
                manager.on_refresh_announcement();
            }
            catch (Exception refreshEx)
            {
                Debug.LogError(
                    $"[Email_And_Announcement_Receiver] 公告列表已写入，刷新 UI 失败: {refreshEx.Message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"[Email_And_Announcement_Receiver] 解析公告回包失败: {ex.Message}\nresp={respJson}");
        }
    }

    private void OnReadMail()
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer){

            var req = new Cmd.MailReadReq();
            foreach(var mail_id in _manager._temp_mail_id_list)
            {
                req.MailID.Add(mail_id);
                _manager._mail_record.Find(x => x.mail_id == mail_id).mail_state = "read";
            }
            NetWork_Center_WSS.SendMsg(req);
        }
    }

    private void OnDeleteMail()
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer){

            var req = new Cmd.MailDeleteReq();
            foreach(var mail_id in _manager._temp_mail_id_list)
            {
                req.MailIDs.Add(mail_id);
                _manager._mail_record.Find(x => x.mail_id == mail_id).mail_state = "deleted";
            }
            NetWork_Center_WSS.SendMsg(req);
        }
    }

    private void OnGetMailReward()
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer){

            var req = new Cmd.MailRewardReq();
            foreach(var mail_id in _manager._temp_mail_id_list)
            {
                req.MailIDs.Add(mail_id);
            }
            NetWork_Center_WSS.SendMsg(req);
        }
    }

    private void SendMailIdList(int msg_id)
    {
        if (_manager == null)
            return;

        // var list = _manager._temp_mail_id_list;
        // if (list == null || list.Count == 0)
        //     return;

        // string json = GF_SP.SerializeObject(list);
        // SendMsg(msg_id, json);
        // TODO zhaorui
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.EmptyReq());
    }

    #endregion

    #region Init bootstrap

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
        var annoTask = UpdateAnnoFromServer();
        while (!annoTask.IsCompleted)
            yield return null;
        if (annoTask.IsFaulted && annoTask.Exception != null)
            Debug.LogError(annoTask.Exception);
    }

    #endregion
}
