using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class FirstTimeLoginInReward : MonoBehaviour,IMsg_Receiver
{
    private string receiver_name;
    private NetWork_Center_WSS _networkCenter;
    // Start is called before the first frame update
    void Start()
    {
        receiver_name = gameObject.name;
        _networkCenter = NetWork_Center_WSS.instance;
        if (_networkCenter == null)
        {
            Debug.LogError($"{receiver_name}: NetworkCenter not initialized");
            return;
        }

        AskForLoginRewardData();
        RegisterToNetwork();
    }

    void OnDestroy()
    {
        UnregisterFromNetwork();
    }


    /// <summary>
    /// Server 按钮点击逻辑
    /// </summary>
    private void SendRewardGetToServer()
    {
        Debug.Log("=== 领取奖励后保存 ===");
        // 扩展你的服务端逻辑
        //RoomSystem.Instance.Update_Data_From_Server?.Invoke();
        List<string> data = new() { "Player_LoginReward" };

        string lastData = Serialization_Provider.SerializeObject(data);

        Network_Msg msg = new Network_Msg
        {
            sender = receiver_name,
            action = "Save_Data",
            action_target = "Player_Server",
            detail_info = lastData,
            _sending_mode = Msg_Sending_Mode.Client_to_Server
        };

        _networkCenter.send_via_wss(msg);
    }

    /// <summary>
    /// Client 按钮点击逻辑
    /// </summary>
    private void AskForLoginRewardData()
    {
        Debug.Log("=== 请求首次登录奖励状态 0 未领取 1   已领取 ===");
        // 扩展你的客户端逻辑

        Network_Msg msg = new Network_Msg
        {
            sender = receiver_name,
            action = "Get_Data",
            action_target = "Player_Server",
            detail_info = "Player_LoginReward",
            _sending_mode = Msg_Sending_Mode.Client_to_Server
        };

        _networkCenter.send_via_wss(msg);

    }

    public string get_receiver_name()
    {
        return this.gameObject.name;
    }

    public void receive_msg(Network_Msg msg)
    {
        Log.Custom($"Receive Message : action = {msg.action},detail = {msg.detail_info}", receiver_name);

        switch (msg.action)
        {
            case "Response_Data":
                HandleResponseData(msg.detail_info);
                break;

            default:
                Debug.LogWarning($"{receiver_name}: Unknown action {msg.action}");
                break;
        }

        Msg_Dispatcher._instance.set_locking(false);
        Debug.Log($"{receiver_name}: Msg_Dispatcher.Instance._is_locking set to false");
    }


    private void HandleResponseData(string detailInfo)
    {
        try
        {
            // 使用提供的JSON解析方式
            var data = Serialization_Provider.DeserializeObject<string[]>(detailInfo);
            if (data != null && data.Length == 2)
            {
                if (data[0] == "Player_LoginReward")
                {
                    string rewardStatus = data[1];
                    Debug.Log($"首次登录奖励状态: {rewardStatus}");
                    // 根据rewardStatus更新UI或游戏状态
                    if (rewardStatus == "0")
                    {
                        //List<(string,int)> rewards =  new List<string>() { };
                        JsonData_Manager.Load_FirstTimeLoginReward_Data(out List<(string, int)> rewards);
                        EvtDsp.TriggerEvt<List<(string, int)>>(EvtNames.ShowFirstLoginReward, rewards);

                        Global_Inventory_Manager.Change_Items_Count(rewards);
                        SendRewardGetToServer();
                        // 显示领取奖励按钮
                        Debug.Log("显示领取奖励按钮");
                    }
                    else if (rewardStatus == "1")
                    {
                        // 显示已领取状态
                        Debug.Log("已领取,什么也不做");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"处理响应数据时发生错误: {ex.Message}");
        }
    }

    private void RegisterToNetwork()
    {
        _networkCenter?.Add_Receiver(receiver_name, this);
    }

    private void UnregisterFromNetwork()
    {
        _networkCenter?.Remove_Receiver(receiver_name);
    }
}
