using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Serialization;
using CLIP.Framework_Unity;
using UnityEngine;

public class ServerClientGUIManager : SingletonMono<ServerClientGUIManager>
{
    // 单例实例，确保全局唯一
    private static ServerClientGUIManager _instance;
    private string _preLoginName;
    private string receiver_name = "ServerClientGUIManager";
    // 按钮尺寸和位置配置（保留基础布局参数）
    [SerializeField] private int buttonWidth = 150;
    [SerializeField] private int buttonHeight = 60;
    [SerializeField] private int buttonSpacing = 20; // 按钮间距
    [SerializeField] private Vector2 startPosition = new Vector2(50, 50); // 起始位置

    // 标记是否初始化完成
    private bool _isInitialized = false;

    private void Awake()
    {

        // 单例+不销毁逻辑（保留）
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _isInitialized = true; // 仅标记初始化完成，无样式初始化
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    private void OnGUI()
    {
        // 初始化检查，避免空引用
        if (!_isInitialized) return;

        // 绘制 Server 按钮：直接使用 Unity 默认样式（GUI.skin.button）
        if (GUI.Button(
            new Rect(startPosition.x, startPosition.y, buttonWidth, buttonHeight),
            "Server" // 移除自定义样式参数，或显式传 GUI.skin.button 均可
        ))
        {
            // OnServerButtonClicked();
        }

        // 绘制 Client 按钮：同样使用默认样式
        if (GUI.Button(
            new Rect(startPosition.x + buttonWidth + buttonSpacing, startPosition.y, buttonWidth, buttonHeight),
            "Client"
        ))
        {
            // OnClientButtonClicked();
        }
    }

    /// <summary>
    /// Server 按钮点击逻辑
    /// </summary>
    // private void OnServerButtonClicked()
    // {
    //     Debug.Log("=== 领取奖励后保存 ===");
    //     // 扩展你的服务端逻辑
    //     //RoomSystem.Instance.Update_Data_From_Server?.Invoke();
    //     List<string> data = new() { "Player_LoginReward"};

    //     string lastData = Serialization_Provider.SerializeObject(data);

    //     Network_Msg msg = new Network_Msg
    //     {
    //         sender = receiver_name,
    //         action = "Save_Data",
    //         action_target = "Player_Server",
    //         detail_info = lastData,
    //         _sending_mode = Msg_Sending_Mode.Client_to_Server
    //     };

    //     _networkCenter.send_via_wss(msg);
    // }

    /// <summary>
    /// Client 按钮点击逻辑
    /// </summary>
    // private void OnClientButtonClicked()
    // {
    //     Debug.Log("=== 请求首次登录奖励状态 0 未领取 1   已领取 ===");
    //     // 扩展你的客户端逻辑

    //     Network_Msg msg = new Network_Msg
    //     {
    //         sender = receiver_name,
    //         action = "Get_Data",
    //         action_target = "Player_Server",
    //         detail_info = "Player_LoginReward",
    //         _sending_mode = Msg_Sending_Mode.Client_to_Server
    //     };

    //     _networkCenter.send_via_wss(msg);

    // }


    // 可选：对外暴露单例实例
    public static ServerClientGUIManager Instance => _instance;
}