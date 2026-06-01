using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using UnityEngine;

/// <summary>
/// 与 <see cref="Dispatch_Manager"/> 同物体的占位组件；派遣数据拉取/上传已由其它协议路径负责。
/// </summary>
public class Dispatch_Receiver : SingletonMono<Dispatch_Receiver>
{
    private const string ReceiverName = "Dispatch_Receiver";

    private void Start()
    {
        if (GetComponent<Dispatch_Manager>() == null)
            Debug.LogError($"{ReceiverName}: DispatchMgr not found on {gameObject.name}");
        else
            Debug.Log($"{ReceiverName} initialized on {gameObject.name}");
    }
}
