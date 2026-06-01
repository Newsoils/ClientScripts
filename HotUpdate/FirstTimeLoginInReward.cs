using System.Collections.Generic;
using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using Google.Protobuf;
using System.Linq;


public class FirstTimeLoginInReward : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //AskForLoginRewardData();
        //EvtDsp.AddEvt(EvtNames.GetFirstLoginReward,HandleFirstLoginReward);
        StartCoroutine(HandleFirstLoginRewardNextFrame());
    }

    private IEnumerator HandleFirstLoginRewardNextFrame()
    {
        yield return null;
        HandleFirstLoginReward();
    }

    private void OnDestroy()
    {
        //EvtDsp.RemoveEvt(EvtNames.GetFirstLoginReward, HandleFirstLoginReward);
    }

    /// <summary>
    /// Client 按钮点击逻辑
    /// </summary>
    private void AskForLoginRewardData()
    {
        Debug.Log("=== 请求首次登录奖励状态 0 未领取 1   已领取 ===");
        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, new Cmd.GetFirstLoginRewardReq());
    }

    //private void HandleResponseData(string detailInfo)
    //{
    //    try
    //    {
    //        // 使用提供的JSON解析方式
    //        var data = Serialization_Provider.DeserializeObject<string[]>(detailInfo);
    //        if (data != null && data.Length == 2)
    //        {
    //            if (data[0] == "Player_LoginReward")
    //            {
    //                string rewardStatus = data[1];
    //                Debug.Log($"首次登录奖励状态: {rewardStatus}");
    //                // 根据rewardStatus更新UI或游戏状态
    //                if (rewardStatus == "0")
    //                {
    //                    //List<(string,int)> rewards =  new List<string>() { };
    //                    JsonDataManager.Load_FirstTimeLoginReward_Data(out List<(string, int)> rewards);
    //                    EvtDsp.TriggerEvt<List<(string, int)>>(EvtNames.ShowFirstLoginReward, rewards);

    //                    Global_Inventory_Manager.Change_Items_Count(rewards);
    //                    SendRewardGetToServer();
    //                    // 显示领取奖励按钮
    //                    Debug.Log("显示领取奖励按钮");
    //                }
    //                else if (rewardStatus == "1")
    //                {
    //                    // 显示已领取状态
    //                    Debug.Log("已领取,什么也不做");
    //                }
    //            }
    //        }
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError($"处理响应数据时发生错误: {ex.Message}");
    //    }
    //}

    private void HandleFirstLoginReward()
    {
        var roleInfo = Global_Game_Manager.Instance.get_current_player_role_info();
        Debug.Log("=== 处理首次登录奖励状态 0 未领取 1   已领取 ===");
        Debug.Log($"首次登录奖励状态: {roleInfo.FirstInfoFlag}");
        if ((roleInfo.FirstInfoFlag & 1 << 0)==0)
        {
            Debug.Log($"加载首次登录奖励Json数据");
            JsonDataManager.Load_FirstTimeLoginReward_Data(out List<(string, int)> rewards);

            Debug.Log($"发放首次登录奖励"+ rewards.Capacity + rewards.FirstOrDefault().Item1);
            EvtDsp.TriggerEvt<List<(string, int)>>(EvtNames.ShowFirstLoginReward, rewards);
            //Global_Inventory_Manager.Change_Items_Count(rewards);
           
        }
        else
        {
            // 显示已领取状态
            Debug.Log("首次登录奖励已领取,什么也不做");
        }
    }

}
