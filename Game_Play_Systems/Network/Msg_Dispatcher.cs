using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Unity;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 消息接收者信息类。
    /// 用于存储一个消息接收对象的基本信息，包括名字、接口引用以及所在GameObject。
    /// </summary>
    [System.Serializable]
    public class Msg_Receicer_Info
    {
        public string _name;                // 接收者名称（对应消息目标）
        public IMsg_Receiver _receiver;     // 实现了消息接收接口的实例

        public Msg_Receicer_Info() { }
    }

    /// <summary>
    /// 消息调度器（Message Dispatcher）
    /// 
    /// 职责：
    /// 1. 统一管理和分发系统中的消息（Network_Msg）。
    /// 2. 维护所有可接收消息的对象列表。
    /// 3. 通过协程定时检查消息缓冲区并进行派发。
    /// 
    /// </summary>
    public class Msg_Dispatcher : MonoBehaviour
    {
        public static Msg_Dispatcher _instance;  // 单例引用

        public List<Msg_Receicer_Info> _receiver_infos = new List<Msg_Receicer_Info>(); // 已注册的消息接收者列表
        public List<Network_Msg> _msg_buffer = new List<Network_Msg>();                  // 消息缓冲区（待分发消息队列）
        public Network_Msg _current_msg;                                               // 当前正在处理的消息

        public float _refresh_interval = 0.1f; // 消息分发刷新间隔（秒）
        public bool _is_locking = false;       // 是否锁定（可用来暂停消息处理）

        /// <summary>
        /// 初始化单例与调度循环
        /// </summary>
        void Start()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject); // 在场景切换时保持存在

                StartCoroutine(try_dispatching_msg()); // 启动消息分发协程
            }
            else
            {
                // 保证场景中只存在一个 Msg_Dispatcher
                if (_instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        /// <summary>
        /// 消息分发循环
        /// 每隔 _refresh_interval 秒检查一次消息队列。
        /// </summary>
        public IEnumerator try_dispatching_msg()
        {
            while (true)
            {
                // 无消息则等待
                if (_msg_buffer.Count == 0)
                {
                    yield return new WaitForSecondsRealtime(_refresh_interval);
                    continue;
                }

                try
                {
                    // 取出首条消息并从队列中移除
                    _current_msg = _msg_buffer[0];
                    _msg_buffer.RemoveAt(0);

                    Log.Info($"[MsgDispatcher] Processing message id={_current_msg.msg_id}, target={_current_msg.action_target}");

                    // 无效消息ID检查
                    if (_current_msg.msg_id < 0)
                    {
                        Log.Warn($"[MsgDispatcher] Invalid msg_id={_current_msg.msg_id}, skipping.");
                        continue;
                    }

                    // 查找目标接收者
                    var _receiver_info = _receiver_infos.Find(_info => _info._name == _current_msg.action_target);
                    if (_receiver_info == null)
                    {
                        Log.Error($"[MsgDispatcher] No receiver found for '{_current_msg.action_target}'");
                        continue;
                    }

                    _is_locking = true;

                    // 打印部分详细信息（避免日志爆炸）
                    if (!string.IsNullOrEmpty(_current_msg.detail_info))
                    {
                        string preview = _current_msg.detail_info.Length > 200
                            ? _current_msg.detail_info.Substring(0, 200) + "..."
                            : _current_msg.detail_info;
                        Debug.Log($"[MsgDispatcher] detail_info: {preview}");
                    }

                    // 安全调用接收者方法
                    Debug.Log($"[MsgDispatcher] Dispatching to '{_receiver_info._name}'");
                    _receiver_info._receiver.receive_msg(_current_msg);

                    //SafeInvoke.TryInvoke(
                    //    _receiver_info._receiver.receive_msg,
                    //    _current_msg,
                    //    "TSBehaviour_with_msg_receiving.receive_msg"
                    //);
                }
                catch (Exception ex)
                {
                    // 捕获并打印异常，不中断循环
                    Debug.LogError($"[MsgDispatcher] Exception: {ex}");
                    if (_current_msg.detail_info != null)
                    {
                        Debug.LogError($"detail_info (preview): {_current_msg.detail_info.Substring(0, Math.Min(300, _current_msg.detail_info.Length))}");
                    }
                }

                yield return new WaitForSecondsRealtime(_refresh_interval);
            }
        }

        /// <summary>
        /// 注册一个消息接收者。
        /// 若名称重复，则更新已有接收者引用。
        /// </summary>
        public void add_msg_receiver(string _name, IMsg_Receiver _receiver)
        {
            for (int i = 0; i < _receiver_infos.Count; i++)
            {
                if (_receiver_infos[i]._name == _name)
                {
                    _receiver_infos[i]._receiver = _receiver;
                    return;
                }
            }

            var _info = new Msg_Receicer_Info();
            _info._name = _name;
            _info._receiver = _receiver;
            _receiver_infos.Add(_info);
        }

        /// <summary>
        /// 移除指定接收者（根据名字和GameObject）。
        /// </summary>
        public void remove_msg_receiver(string _name)
        {
            _receiver_infos.RemoveAll((_info) =>
            {
                return _info._name == _name;
            });
        }

        /// <summary>
        /// 设置消息调度锁定状态。
        /// 当锁定时，可暂停消息分发（由外部系统控制）。
        /// </summary>
        public void set_locking(bool _lockinig)
        {
            _is_locking = _lockinig;
            Debug.LogWarning("Set Msg Dispatcher Locking to " + _lockinig);
        }
    }
}
