using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cmd;
using Google.Protobuf;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using CLIP.Framework_Core.Network;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

namespace CLIP.Project_Mouse.Network
{
    /// <summary>
    /// WebSocket 传输封装：读写队列 + 心跳 + 失效标记（与业务层 NetWork_Center_WSS 解耦）。
    /// </summary>
    public sealed class WsClient
    {
        private WebSocket _ws;
        private int _invalidFlag;

        public string Url { get; private set; }

        private const int CmdHeartBeatReq = 1005;
        private const int HeartBeatDuration = 30;

        /// <summary>最后一次心跳发送时间（Unix 秒）</summary>
        private long _lastHeartBeatSendUnixSeconds;

        private readonly List<string> _writeMsgs = new List<string>();
        private readonly List<Network_Msg> _readMsgs = new List<Network_Msg>();
        private readonly List<byte[]> _readBytes = new List<byte[]>();
        private readonly object _queueLock = new object();

        private Coroutine _heartbeatCo;
        private Coroutine _writeCo;
        private Coroutine _readCo;
        private Coroutine _runCo;

        private readonly MonoBehaviour _host;
        private readonly Action<Network_Msg> _dispatchHandleMsg;
        private readonly Action _onOpenMainThread;
        private readonly Func<bool> _heartbeatGate;
        /// <summary>主线程：传输失败（OnError / OnClose），在 SetInvalid 之前调用，供 NetWork_Center_WSS.HandleWsFailure。</summary>
        private readonly Action<bool, string> _onTransportFailureMainThread;

        /// <summary>仅当 invaildFlag 从 0 变为 1 时触发一次。</summary>
        public UnityEvent OnInvalid { get; } = new UnityEvent();

        /// <summary>传输层 OnOpen 且主线程回调完成后触发（用于 1003 鉴权等）。在 <see cref="_onOpenMainThread"/> 之后调用。</summary>
        public UnityEvent OnWsOpen { get; } = new UnityEvent();

        /// <summary>WebSocket 是否已完成 OnOpen（主线程语义，与旧 NetWork_Center_WSS._is_connected 一致）。</summary>
        public bool IsTransportConnected => _transportConnected;

        private bool _transportConnected;

        /// <summary>成功解析的业务帧序号（首帧为 0，与旧 NetWork_Center_WSS.on_message 计数一致）。</summary>
        public int ReceiveMsgSeq { get; private set; } = -1;

        public WsClient(
            MonoBehaviour host,
            string wsAddr,
            Action<Network_Msg> dispatchHandleMsg,
            Action onOpenMainThread,
            Func<bool> heartbeatGate,
            Action<bool, string> transportFailureMainThread)
        {
            _host = host;
            Url = wsAddr ?? string.Empty;
            _dispatchHandleMsg = dispatchHandleMsg ?? throw new ArgumentNullException(nameof(dispatchHandleMsg));
            _onOpenMainThread = onOpenMainThread;
            _heartbeatGate = heartbeatGate ?? (() => true);
            _onTransportFailureMainThread = transportFailureMainThread;
            _lastHeartBeatSendUnixSeconds = UnixNowSeconds();
        }

        private static long UnixNowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void SetInvalid()
        {
            int old = Interlocked.CompareExchange(ref _invalidFlag, 1, 0);
            if (old != 0)
                return;
            _transportConnected = false;
            CloseSocketAfterInvalidTransition();
            RunOnMainThread(() => OnInvalid?.Invoke());
        }

        /// <summary>invalidFlag 从 0 变为 1 时主动关闭与服务器的连接（与 ManualClose 中 Close 条件一致）。</summary>
        private void CloseSocketAfterInvalidTransition()
        {
            try
            {
                var s = _ws;
                if (s != null && (s.State == WebSocketState.Open || s.State == WebSocketState.Connecting))
                    _ = s.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WsClient] CloseSocketAfterInvalidTransition: {ex.Message}");
            }
        }

        private void RunOnMainThread(Action a)
        {
            if (a == null || _host == null) return;
            _host.StartCoroutine(RunNextFrame(a));
        }

        private static IEnumerator RunNextFrame(Action a)
        {
            yield return null;
            try { a(); }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        public bool IsInvalid()
        {
            return Volatile.Read(ref _invalidFlag) != 0;
        }

        private void HandleOnError(string e)
        {
            Debug.LogError($"[WsClient] OnError: {e} url={Url}");
            RunOnMainThread(() =>
            {
                _onTransportFailureMainThread?.Invoke(false, e);
                SetInvalid();
            });
        }

        private void HandleOnClose(WebSocketCloseCode closeCode)
        {
            string detail = $"{(int)closeCode} ({closeCode})";
            Debug.Log($"[WsClient] OnClose: {detail} url={Url}");
            RunOnMainThread(() =>
            {
                _onTransportFailureMainThread?.Invoke(true, detail);
                SetInvalid();
            });
        }

        private void HandleOnMessage(byte[] data)
        {
            if (data == null) return;
            lock (_queueLock)
                _readBytes.Add(data);
        }

        public void ManualClose()
        {
            Debug.Log($"[WsClient] ManualClose url={Url}");
            SetInvalid();
            StopLoops();
            try
            {
                if (_ws != null && (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.Connecting))
                    _ = _ws.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WsClient] ManualClose: {ex.Message}");
            }
        }

        public async Task CloseAsync()
        {
            Debug.Log($"[WsClient] CloseAsync url={Url}");
            SetInvalid();
            StopLoops();
            var s = _ws;
            if (s != null)
            {
                try
                {
                    if (s.State == WebSocketState.Open || s.State == WebSocketState.Connecting || s.State == WebSocketState.Closing)
                        await s.Close();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[WsClient] CloseAsync: {ex.Message}");
                }
            }
        }

        private void StopLoops()
        {
            if (_host == null) return;
            if (_heartbeatCo != null) _host.StopCoroutine(_heartbeatCo);
            if (_writeCo != null) _host.StopCoroutine(_writeCo);
            if (_readCo != null) _host.StopCoroutine(_readCo);
            if (_runCo != null) _host.StopCoroutine(_runCo);
            _heartbeatCo = _writeCo = _readCo = _runCo = null;
        }

        public void SendTextMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;
            lock (_queueLock)
                _writeMsgs.Add(msg);
        }

        public void StartClient()
        {
            if (_host == null)
            {
                Debug.LogError("[WsClient] StartClient: host null");
                return;
            }

            Volatile.Write(ref _invalidFlag, 0);
            _transportConnected = false;
            _lastHeartBeatSendUnixSeconds = UnixNowSeconds();

            if (!string.IsNullOrEmpty(Url))
                Url = Url.Trim().Replace(" ", "");

            Debug.Log($"[WsClient] StartClient Connect url={Url}");
            _ws = new WebSocket(Url);
            BindCallbacks();
            _ws.Connect();

            _runCo = _host.StartCoroutine(RunLoop());
            _writeCo = _host.StartCoroutine(WriteLoop());
            _readCo = _host.StartCoroutine(ReadLoop());
            _heartbeatCo = _host.StartCoroutine(HeartbeatLoop());
        }

        private void BindCallbacks()
        {
            var bound = _ws;
            _ws.OnOpen += () =>
            {
                if (!ReferenceEquals(bound, _ws)) return;
                Debug.Log($"[WsClient] OnOpen url={Url}");
                RunOnMainThread(() =>
                {
                    if (!ReferenceEquals(bound, _ws)) return;
                    _transportConnected = true;
                    _onOpenMainThread?.Invoke();
                    OnWsOpen.Invoke();
                });
            };
            _ws.OnMessage += bytes =>
            {
                if (!ReferenceEquals(bound, _ws)) return;
                HandleOnMessage(bytes);
            };
            _ws.OnError += e =>
            {
                if (!ReferenceEquals(bound, _ws)) return;
                HandleOnError(e);
            };
            _ws.OnClose += code =>
            {
                if (!ReferenceEquals(bound, _ws)) return;
                HandleOnClose(code);
            };
        }

        public void DispatchMessageQueue()
        {
#if !UNITY_WEBGL
            if (_ws != null) _ws.DispatchMessageQueue();
#endif
        }

        public string GetStateHintForLogs()
        {
            if (_ws == null) return "ws=null";
            try { return $"State={_ws.State}"; }
            catch (Exception ex) { return $"State=? ({ex.Message})"; }
        }

        public bool IsSocketOpenForSend()
        {
            return _ws != null && _ws.State == WebSocketState.Open && !IsInvalid();
        }

        public async Task SendBinaryAsync(byte[] data)
        {
            if (data == null || !IsSocketOpenForSend()) return;
            await _ws.Send(data);
        }

        private void SendHeartBeat()
        {
            if (!IsSocketOpenForSend()) return;
            var hbReq = new HeartBeatReq();
            string hbReqJson = JsonFormatter.Default.Format(hbReq);
            var authMsg = new JObject
            {
                ["msg_id"] = CmdHeartBeatReq,
                ["detail_info"] = JObject.Parse(hbReqJson)
            };
            SendTextMsg(authMsg.ToString(Formatting.None));
            _lastHeartBeatSendUnixSeconds = UnixNowSeconds();
        }

        private IEnumerator RunLoop()
        {
            while (true)
            {
                if (IsInvalid())
                {
                    Debug.Log("[WsClient] RunLoop exit (invalid)");
                    yield break;
                }

                Network_Msg next = default;
                bool has = false;
                lock (_queueLock)
                {
                    if (_readMsgs.Count > 0)
                    {
                        next = _readMsgs[0];
                        _readMsgs.RemoveAt(0);
                        has = true;
                    }
                }

                if (has)
                {
                    try
                    {
                        _dispatchHandleMsg(next);
                    }
                    catch (Exception ex)
                    {
                        SetInvalid();
                        Debug.LogException(ex);
                        throw;
                    }
                }
                else
                    yield return null;
            }
        }

        private IEnumerator WriteLoop()
        {
            while (true)
            {
                if (IsInvalid())
                {
                    Debug.Log("[WsClient] WriteLoop exit (invalid)");
                    yield break;
                }

                string msg = null;
                lock (_queueLock)
                {
                    if (_writeMsgs.Count > 0)
                    {
                        msg = _writeMsgs[0];
                        _writeMsgs.RemoveAt(0);
                    }
                }

                if (msg != null && _ws != null && _ws.State == WebSocketState.Open)
                {
                    Task sendTask;
                    try
                    {
                        sendTask = _ws.SendText(msg);
                    }
                    catch (Exception ex)
                    {
                        SetInvalid();
                        Debug.LogException(ex);
                        throw;
                    }

                    while (!sendTask.IsCompleted)
                        yield return null;
                    if (sendTask.IsFaulted)
                        Debug.LogError($"[WsClient] SendText fault: {sendTask.Exception}");
                }
                else
                    yield return null;
            }
        }

        private IEnumerator ReadLoop()
        {
            while (true)
            {
                if (IsInvalid())
                {
                    Debug.Log("[WsClient] ReadLoop exit (invalid)");
                    yield break;
                }

                byte[] chunk = null;
                lock (_queueLock)
                {
                    if (_readBytes.Count > 0)
                    {
                        chunk = _readBytes[0];
                        _readBytes.RemoveAt(0);
                    }
                }

                if (chunk == null)
                {
                    yield return null;
                    continue;
                }

                string raw;
                try
                {
                    raw = Encoding.UTF8.GetString(chunk);
                }
                catch (Exception ex)
                {
                    SetInvalid();
                    Debug.LogException(ex);
                    throw;
                }

                if (raw == "server_ping")
                {
                    if (_ws != null && _ws.State == WebSocketState.Open)
                    {
                        Task pongTask;
                        try
                        {
                            pongTask = _ws.SendText("client_pong");
                        }
                        catch (Exception ex)
                        {
                            SetInvalid();
                            Debug.LogException(ex);
                            throw;
                        }

                        while (!pongTask.IsCompleted)
                            yield return null;
                    }

                    continue;
                }

                string normalizedMsg;
                try
                {
                    normalizedMsg = NormalizeNetworkMsgJson(raw);
                }
                catch (Exception ex)
                {
                    SetInvalid();
                    Debug.LogException(ex);
                    throw;
                }

                ReceiveMsgSeq++;
                Network_Msg netMsg;
                try
                {
                    netMsg = GF_SP.DeserializeObject<Network_Msg>(normalizedMsg);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[WsClient] Deserialize Network_Msg failed: {ex.GetType().Name}: {ex.Message}\npreview={(raw?.Length > 600 ? raw.Substring(0, 600) + "..." : raw)}");
                    continue;
                }

                try
                {
                    lock (_queueLock)
                        _readMsgs.Add(netMsg);
                }
                catch (Exception ex)
                {
                    SetInvalid();
                    Debug.LogException(ex);
                    throw;
                }
            }
        }

        private IEnumerator HeartbeatLoop()
        {
            var wait = new WaitForSecondsRealtime(1f);
            while (true)
            {
                if (IsInvalid())
                {
                    Debug.Log("[WsClient] HeartbeatLoop exit (invalid)");
                    yield break;
                }

                if (_heartbeatGate != null && !_heartbeatGate())
                {
                    yield return wait;
                    continue;
                }

                long now = UnixNowSeconds();
                try
                {
                    if (now - _lastHeartBeatSendUnixSeconds >= HeartBeatDuration)
                        SendHeartBeat();
                }
                catch (Exception ex)
                {
                    SetInvalid();
                    Debug.LogException(ex);
                    throw;
                }

                yield return wait;
            }
        }

        private static string NormalizeNetworkMsgJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            try
            {
                var root = JObject.Parse(raw);
                if (root["msg_id"] == null)
                {
                    var altId = root["MsgId"] ?? root["Msg_ID"] ?? root["msgID"];
                    if (altId != null && altId.Type != JTokenType.Null)
                    {
                        int idVal = altId.Type == JTokenType.Integer
                            ? altId.Value<int>()
                            : int.Parse(altId.ToString().Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                        root["msg_id"] = idVal;
                    }
                }
                if (root["ErrorCode"] == null)
                {
                    var ec = root["error_code"] ?? root["errorCode"] ?? root["Error_Code"];
                    if (ec != null && ec.Type != JTokenType.Null)
                    {
                        int ecVal = ec.Type == JTokenType.Integer
                            ? ec.Value<int>()
                            : int.Parse(ec.ToString().Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                        root["ErrorCode"] = ecVal;
                    }
                }
                if (root["ErrorArgs"] == null)
                {
                    var ea = root["error_args"] ?? root["errorArgs"] ?? root["Error_Args"];
                    if (ea != null && ea.Type != JTokenType.Null)
                        root["ErrorArgs"] = ea;
                }
                var detail = root["detail_info"];
                if (detail != null && detail.Type != JTokenType.String && detail.Type != JTokenType.Null)
                    root["detail_info"] = detail.ToString(Formatting.None);
                return root.ToString(Formatting.None);
            }
            catch
            {
                return raw;
            }
        }
    }
}
