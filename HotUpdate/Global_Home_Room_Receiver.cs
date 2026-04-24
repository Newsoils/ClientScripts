using System.Collections;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class Global_Home_Room_Receiver : SingletonMono<Global_Home_Room_Receiver>, IMsg_Receiver
{
    private Global_Home_Room_Manager _homeManager;
    private NetWork_Center_WSS _networkCenter;

    private const string ReceiverName = "Global_Home_Room_Receiver";

    #region Unity Life Cycle


    private void Start()
    {

        _homeManager = GetComponent<Global_Home_Room_Manager>();
        if (_homeManager == null)
        {
            Debug.LogError("Global_Home_Room_Receiver: Home_Manager not found");
            return;
        }

        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError("Global_Home_Room_Receiver: NetworkCenter not initialized");
            return;
        }

        RegisterToNetwork();

        Debug.Log($"Global_Home_Room_Receiver initialized on {_homeManager.gameObject.name}");

        // 连接成功后更新场景数据
        if (_networkCenter._connect_to_player_server)
        {
            StartCoroutine(UpdateWholeSceneFromServer());
        }
    }

    private void OnDestroy()
    {
        UnregisterFromNetwork();
    }

    #endregion

    #region Network Send Helpers

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

    #region Coroutine Helpers

    private IEnumerator UpdateWholeSceneFromServer()
    {
        Debug.Log("Global_Home_Room_Receiver: Updating whole scene from server");

        if (_homeManager == null)
        {
            Debug.LogError("Global_Home_Room_Receiver: Home_Manager is not initialized");
            yield break;
        }

        Debug.Log("Global_Home_Room_Receiver: Updating Living RoomData from server");
        yield return new WaitForSeconds(1.024f);

        //if (_homeManager._living_room != null)
        //{
        //    _homeManager._living_room.Update_Data_From_Server();
        //}

        //yield return new WaitForSeconds(1.024f);

        //if (_homeManager._bed_room != null)
        //{
        //    _homeManager._bed_room.Update_Data_From_Server();
        //}

        //yield return new WaitForSeconds(1.024f);

        //if (_homeManager._toilet != null)
        //{
        //    _homeManager._toilet.Update_Data_From_Server();
        //}

        yield return new WaitForSeconds(1.024f);

        //if (_homeManager._planting_system_manager != null)
        //{
        //    _homeManager._planting_system_manager.Update_Data_From_Server();
        //}
    }

    private IEnumerator WaitSomeSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds / 1000f); // 将毫秒转换为秒
    }

    #endregion

    #region IMsg_Receiver (Server -> Client)

    public string get_receiver_name()
    {
        return ReceiverName;
    }

    public void receive_msg(Network_Msg msg)
    {
        if (_homeManager == null)
            return;

        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", ReceiverName);
        switch (msg.action)
        {
            // 这里可以根据需要添加具体的消息处理逻辑
            // 例如：
            // case "Update_Home_Data":
            //     _homeManager.load_home_data(msg.detail_info);
            //     break;

            default:
                Debug.LogWarning($"Global_Home_Room_Receiver: Unknown action {msg.action}");
                break;
        }

        Msg_Dispatcher._instance.set_locking(false);
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

    #region Public Methods

    // 提供外部调用的方法来更新整个场景
    public void RequestUpdateWholeScene()
    {
        if (_networkCenter != null && _networkCenter._connect_to_player_server)
        {
            StartCoroutine(UpdateWholeSceneFromServer());
        }
    }

    #endregion
}