using System;
using System.Collections;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Google.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using NativeWebSocket;
using UnityEngine.UI;
using UnityEngine.Networking;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using CLIP.Framework_Unity;
using CLIP.Framework_Core.Tools;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Core.Event;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace CLIP.Project_Mouse.Network
{

    public class NetWork_Center_WSS : SingletonMono<NetWork_Center_WSS>
    {
        protected override bool PersistAcrossScenes => true;
        public Msg_Dispatcher _msg_dispatcher;

        public string url = "ws://127.0.0.1:25564/";

        [Header("Current_State")]
        public string _player_name;
        public bool _connect_to_player_server = false;
        /// <summary>传输层是否已 Open（由 <see cref="WsClient.IsTransportConnected"/> 反映）。</summary>
        public bool _is_connected => _wsClient != null && _wsClient.IsTransportConnected;
        public bool _on_connection_closed = false;
        public int _current_send_msg_id = -1;
        public int _current_receive_msg_id => _wsClient != null ? _wsClient.ReceiveMsgSeq : -1;

        public RawImage _output_image;
        /// <summary>Optional debug/status label (typo preserved for serialized scenes).</summary>
        public TMP_Text _ouput_text;

        public bool use_local = false;

        private WsClient _wsClient;
        [Header("Event")]
        public float _on_connection_close_delay = 5f;
        public UnityEvent _evt_on_connection_closed = new UnityEvent();

        [Header("Reconnect_Settings")]
        [SerializeField] private int _max_reconnect_attempts = 20;
        /// <summary>WsClient.OnInvalid 触发的重连最大次数（不使用协程，由 InvokeRepeating 驱动）。</summary>
        private const int MaxInvalidReconnectAttempts = 10;
        [SerializeField] public float _reconnect_interval = 1.5f;
        private int _current_reconnect_attempts = 0;
        private bool _is_connecting = false;
        private Coroutine _reconnect_coroutine;
        private int _invalidReconnectAttemptsDone;

        [Header("Resume_Relogin_Settings")]
        [SerializeField] private float _resume_relogin_debounce_seconds = 2.0f;
        private float _last_resume_relogin_time = -999f;
        private Coroutine _resume_relogin_coroutine;

        /// <summary>
        /// TapTap/网关切换时会先关旧 WebSocket 再连新地址；关闭过程中的 OnError/OnClose 不应当作「服务器连接失败」弹窗。
        /// </summary>
        private float _suppressConnectionFailurePromptUntilUnscaled = -999f;

        /// <summary>
        /// 在接下来若干秒内忽略「未登录阶段」的连接失败弹窗（用于网关 URL 切换、主动 close 重连等）。
        /// </summary>
        public void SuppressConnectionFailurePromptForSeconds(float seconds)
        {
            _suppressConnectionFailurePromptUntilUnscaled = Time.unscaledTime + Mathf.Max(0.1f, seconds);
        }

        /// <summary>本类内所有「主动」Close / 替换 WsClient 处统一打日志，便于与被动 OnClose 区分。</summary>
        private void LogWssActiveClose(string reason)
        {
            Debug.Log($"[WSS] 主动关闭/重建连接 | {reason} | {_wsClient?.GetStateHintForLogs() ?? "wsClient=null"} | url={url}");
        }

        private void OnWsTransportFailure(bool isCloseEvent, string detail)
        {
            if (isCloseEvent)
            {
                bool wasConnected = _wsClient != null && _wsClient.IsTransportConnected;
                if (!wasConnected)
                    return;
            }

            HandleWsFailure(isCloseEvent, detail);
        }

        private void CreateAndStartWsClient()
        {
            _wsClient = new WsClient(
                this,
                url,
                handle_msg,
                () => { _current_reconnect_attempts = 0; },
                () => _connect_to_player_server,
                OnWsTransportFailure
            );
            _wsClient.OnWsOpen.AddListener(OnWsClientWsOpenPublish);
            _wsClient.OnInvalid.AddListener(OnWsClientInvalid);
            _wsClient.StartClient();
        }

        private void OnWsClientWsOpenPublish()
        {
            EvtDsp.TriggerEvt(EvtNames.WS_Open);
        }

        /// <summary>WsClient 失效（0→1）时：任意场景均尝试重连；若已在 InvokeRepeating 重连周期内则由定时器继续，避免打断计数。</summary>
        private void OnWsClientInvalid()
        {
            if (IsInvoking(nameof(ReconnectAfterInvalidTick)))
                return;
            BeginReconnectAfterInvalid();
        }

        private void BeginReconnectAfterInvalid()
        {
            StopReconnectAttempts();
            CancelInvoke(nameof(ReconnectAfterInvalidTick));
            _invalidReconnectAttemptsDone = 0;
            InvokeRepeating(nameof(ReconnectAfterInvalidTick), 0f, _reconnect_interval);
        }

        private void ReconnectAfterInvalidTick()
        {
            if (_is_connected)
            {
                CancelInvoke(nameof(ReconnectAfterInvalidTick));
                return;
            }

            if (_invalidReconnectAttemptsDone >= MaxInvalidReconnectAttempts)
            {
                CancelInvoke(nameof(ReconnectAfterInvalidTick));
                OnInvalidReconnectExhausted();
                return;
            }

            _invalidReconnectAttemptsDone++;
            Debug.Log($"[WSS] OnInvalid 重连 ({_invalidReconnectAttemptsDone}/{MaxInvalidReconnectAttempts})");
            CloseCurrentWsClientForReconnect();
            CreateAndStartWsClient();
#if !UNITY_WEBGL
            _wsClient?.DispatchMessageQueue();
#endif
        }

        private void OnInvalidReconnectExhausted()
        {
            Debug.LogError($"[WSS] 重连失败：已连续尝试 {MaxInvalidReconnectAttempts} 次仍未恢复连接。");
            if (SceneLoadHelper.IsLoginScene)
            {
                Debug.LogError("[WSS] LoginScene：重连失败，重启游戏。");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            else
            {
                EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, "已与服务器断开连接！", null);
            }
        }

        public void CloseCurrentWsClientForReconnect()
        {
            if (_wsClient == null) return;
            var c = _wsClient;
            _wsClient = null;
            c.OnWsOpen.RemoveListener(OnWsClientWsOpenPublish);
            c.OnInvalid.RemoveListener(OnWsClientInvalid);
            c.ManualClose();
        }

        private void Start()
        {
            if (_evt_on_connection_closed == null)
                _evt_on_connection_closed = new UnityEvent();
            _current_reconnect_attempts = 0;
            _evt_on_connection_closed.AddListener(OnDisconnected);

            EvtDsp.AddEvt(EvtNames.Network_Disconnect, BackToLogin);
        }

        public void FixedUpdate()
        {
#if !UNITY_WEBGL
            _wsClient?.DispatchMessageQueue();
#endif
        }

        private async void OnDestroy()
        {
            CancelInvoke(nameof(ReconnectAfterInvalidTick));

            _evt_on_connection_closed?.RemoveAllListeners();

            var c = _wsClient;
            _wsClient = null;
            if (c != null)
            {
                LogWssActiveClose("OnDestroy（MonoBehaviour 销毁）");
                c.OnWsOpen.RemoveListener(OnWsClientWsOpenPublish);
                c.OnInvalid.RemoveListener(OnWsClientInvalid);
                await c.CloseAsync();
            }
            //StopAllCoroutines();

            EvtDsp.RemoveEvt(EvtNames.Network_Disconnect, BackToLogin);
        }

        //private void OnApplicationPause(bool pauseStatus)
        //{
        //    if (!pauseStatus)
        //    {
        //        TryResumeReloginIfNeeded("OnApplicationPause(false)");
        //    }
        //}

        //private void OnApplicationFocus(bool hasFocus)
        //{
        //    if (hasFocus)
        //    {
        //        TryResumeReloginIfNeeded("OnApplicationFocus(true)");
        //    }
        //}

        private void TryResumeReloginIfNeeded(string source)
        {
            // 仅：登录成功进入主界面后（主界面静置/退后台/锁屏回来）才触发隐式重登 + TryLogin + 数据刷新
            if (!_connect_to_player_server) return;
            if (!SceneLoadHelper.IsMainScene) return;

            var now = Time.unscaledTime;
            if (now - _last_resume_relogin_time < _resume_relogin_debounce_seconds) return;
            _last_resume_relogin_time = now;

            if (_resume_relogin_coroutine != null)
            {
                StopCoroutine(_resume_relogin_coroutine);
                _resume_relogin_coroutine = null;
            }

            Debug.Log($"Resume relogin triggered by {source}");
            _resume_relogin_coroutine = StartCoroutine(ResumeReloginFlow());
        }

        private IEnumerator ResumeReloginFlow()
        {
            // 1) 确保 WebSocket 已连接（如果断了，先后台重连）
            if (!_is_connected)
            {
                BeginReconnectAfterInvalid();
            }

            var connectTimeoutAt = Time.unscaledTime + 8.0f;
            while (!_is_connected && Time.unscaledTime < connectTimeoutAt)
            {
                yield return null;
            }

            if (!_is_connected)
            {
                // 连接未恢复时不继续触发 SDK/登录，避免一连串失败弹窗；失败提示由网络层现有逻辑处理
                yield break;
            }

            // 2) 后台隐式重新登录 SDK，并重新向服务器发送 TryLogin
            // 通过事件解耦触发 TapTap 隐式重登（避免直接类型引用导致编译依赖问题）
            EvtDsp.TriggerEvt(EvtNames.Resume_Silent_Relogin);

            // 兜底：如果没有 TapTap 管理器或隐式重登失败，仍尝试本地缓存账号重登
            EvtDsp.TriggerEvt(EvtNames.Resume_TryLogin_From_Cache);
        }


        public void init_wss()
        {
            // 防御：配置字符串里若误带空格，先清洗，避免建立连接后被服务端立即踢下线
            if (!string.IsNullOrEmpty(url))
                url = url.Trim().Replace(" ", "");

            if (_wsClient != null)
            {
                LogWssActiveClose("init_wss：替换当前 WsClient（若需干净断开可先 await close_wss）");
                _wsClient.OnWsOpen.RemoveListener(OnWsClientWsOpenPublish);
                _wsClient.OnInvalid.RemoveListener(OnWsClientInvalid);
                _wsClient.ManualClose();
                _wsClient = null;
            }

            Debug.Log($"[WSS] init url={url}");
            CreateAndStartWsClient();
        }

        /// <summary>供未引用 NativeWebSocket 的程序集打印诊断（避免暴露 WebSocket 类型）。</summary>
        public string GetWsStateHintForLogs()
        {
            return _wsClient?.GetStateHintForLogs() ?? "wsClient=null";
        }

        private void HandleWsFailure(bool isCloseEvent, string detail)
        {
            if (!_connect_to_player_server && Time.unscaledTime < _suppressConnectionFailurePromptUntilUnscaled)
            {
                // 切换网关：主动 Close 多为 1000；旧连接被替换时多为 1006（无 Close 帧），均不作为故障提示
                bool quietBenignClose = isCloseEvent && detail != null && (
                    detail.StartsWith("1000 ", StringComparison.Ordinal)
                    || detail.StartsWith("1006 ", StringComparison.Ordinal)
                    || string.Equals(detail, nameof(WebSocketCloseCode.Normal), StringComparison.Ordinal));
                if (!quietBenignClose)
                    Debug.LogWarning($"[WSS] 连接失败提示已抑制（网关切换/重连窗口） phase={(isCloseEvent ? "OnClose" : "OnError")} detail={detail}");
                return;
            }

            // 登录后：重连统一由 WsClient.OnInvalid → BeginReconnectAfterInvalid（任意场景），此处仅记录
            if (_connect_to_player_server)
            {
                Debug.LogWarning($"WSS {(isCloseEvent ? "OnClose" : "OnError")} after login: {detail}");
                return;
            }

            // 未登录阶段：保持原有行为（立即提示连接失败）
            Debug.LogWarning($"WSS {(isCloseEvent ? "OnClose" : "OnError")} before login: {detail}");
            TriggerOriginalServerConnectionFailed();
        }

        private void TriggerOriginalServerConnectionFailed()
        {
            Debug.LogError("[WSS] 服务器连接失败（未进入游戏服登录态）。请查看上一条 WSS OnError/OnClose 日志中的 detail；检查网关 ws 地址、本机网络与防火墙。");
            EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, "服务器连接失败，请检查网络设置！", null);
        }

        private void OnDisconnected()
        {
            Debug.LogError("已断联");
            StartCoroutine(_on_connection_closed_co());
            if (_connect_to_player_server) BeginReconnectAfterInvalid();
            else TryReconnectCoroutine();
        }

        public IEnumerator _on_connection_closed_co()
        {
            Debug.Log("Connection closed, quit game in " + _on_connection_close_delay + " seconds");
            yield return new WaitForSecondsRealtime(_on_connection_close_delay);
        }


        public void BackToLogin()
        {
            //返回会有UI等问题，暂时先直接退出游戏
            //if(!SceneLoadHelper.IsLoginScene)
            //{
            //    SceneLoadingHelper.LoadLoginScene();
            //}
            Application.Quit();
        }


        public void TryReconnectCoroutine()
        {
            StopReconnectAttempts();
            _reconnect_coroutine = StartCoroutine(AttemptReconnectLoop(_max_reconnect_attempts, showSuccessPrompt: true, onFinalFail: () =>
            {
                EvtDsp.TriggerEvt<string, Action>(
                    EvtNames.ShowPrompt,
                    $"连接失败（已重试{_max_reconnect_attempts}次），请检查网络后重试！",
                    () => TryReconnectCoroutine()
                );
            }));
        }

        private IEnumerator AttemptReconnectLoop(int maxAttempts, bool showSuccessPrompt, Action onFinalFail)
        {
            if (_is_connecting) yield break;

            _is_connecting = true;
            _current_reconnect_attempts = 0;

            while (_current_reconnect_attempts < maxAttempts)
            {
                _current_reconnect_attempts++;
                Debug.Log($"正在尝试连接... ({_current_reconnect_attempts}/{maxAttempts})");

                // 等待旧连接完全关闭，避免 Close/Connect 并发导致状态错乱
                // yield return CloseWsCoroutine();
                //尝试关闭旧连接
                CloseCurrentWsClientForReconnect();

                LogWssActiveClose("AttemptReconnectLoop：CloseWsCoroutine 已执行，即将新建 WsClient 并 Connect");
                CreateAndStartWsClient();

#if !UNITY_WEBGL
                _wsClient?.DispatchMessageQueue();
#endif

                yield return new WaitForSecondsRealtime(_reconnect_interval);

                if (_is_connected)
                {
                    _is_connecting = false;
                    _current_reconnect_attempts = 0;

                    if (showSuccessPrompt)
                    {
                        EvtDsp.TriggerEvt<string, Action>(
                            EvtNames.ShowPrompt,
                            "重连成功，请尝试登录", null
                        );
                    }

                    Debug.Log("Connected!");
                    yield break;
                }

            }

            _is_connecting = false;
            Debug.Log($"连接失败（已重试{maxAttempts}次）");
            onFinalFail?.Invoke();
        }

        private void StopReconnectAttempts()
        {
            //_current_reconnect_attempts = 0;
            if (_reconnect_coroutine != null)
            {
                StopCoroutine(_reconnect_coroutine);
                _reconnect_coroutine = null;
            }
            CancelInvoke(nameof(ReconnectAfterInvalidTick));
            _is_connecting = false;
        }

      
        public void set_player_name(string str)
        {
            _player_name = str;
        }

        public void Add_Receiver(int msg_id,IMsg_Receiver msg_Receiver)
        {
            _msg_dispatcher.add_msg_receiver(
                msg_id,msg_Receiver
                );
        }
        public void Remove_Receiver(int msg_id)
        {
            Debug.Log("Remove_Receiver: msg_id=" + msg_id);
            _msg_dispatcher.remove_msg_receiver(
                msg_id
                );
        }
        public void handle_msg(Network_Msg _msg)
        {
            _msg_dispatcher._msg_buffer.Add( _msg );
        }


        public void send_via_wss(string _data)
        {
            if (_wsClient != null && _wsClient.IsSocketOpenForSend())
            {
                // 打印即将发送的消息（避免日志刷屏：超过 500 字符只打印前 500）
                if (!string.IsNullOrEmpty(_data))
                {
                    string preview = _data.Length > 500 ? _data.Substring(0, 500) + "..." : _data;
                    Debug.Log($"[WSS SendText] len={_data.Length} preview={preview}");
                }
                else
                {
                    Debug.Log("[WSS SendText] (empty)");
                }
                _wsClient.SendTextMsg(_data);
            }
        }

        public async Task send_via_wss(byte[] _data)
        {
            if (_wsClient != null && _wsClient.IsSocketOpenForSend())
            {
                // 打印二进制消息长度，并尝试给出 UTF8 预览（失败则只打印长度）
                if (_data == null)
                {
                    Debug.Log("[WSS SendBytes] (null)");
                }
                else
                {
                    string utf8Preview = null;
                    try
                    {
                        utf8Preview = System.Text.Encoding.UTF8.GetString(_data);
                        if (utf8Preview.Length > 200) utf8Preview = utf8Preview.Substring(0, 200) + "...";
                    }
                    catch
                    {
                        // ignore
                    }

                    if (!string.IsNullOrEmpty(utf8Preview))
                        Debug.Log($"[WSS SendBytes] len={_data.Length} utf8_preview={utf8Preview}");
                    else
                        Debug.Log($"[WSS SendBytes] len={_data.Length}");
                }

                await _wsClient.SendBinaryAsync(_data);
            }
        }

        // public void send_via_wss(Network_Msg _msg)
        // {
        //     _current_send_msg_id++;
        //     _msg.player_id = _player_name;
        //     _msg.msg_id = _current_send_msg_id;
        //     Debug.Log($"[WSS Send Network_Msg] msg_id={_msg.msg_id} action_target={_msg.action_target} action={_msg.action} sender={_msg.sender}");
        //     send_via_wss(GF_SP.SerializeObject(_msg));
        // }

        /// <summary>传输层 WebSocket 已连接（可发送含 1003 在内的帧）。</summary>
        public static bool IsWsTransportConnected =>
            Instance != null && Instance._is_connected;

        /// <summary>已通过登录鉴权，可向玩法服发送业务 Protobuf。</summary>
        public static bool IsConnectedToPlayerServer =>
            Instance != null && Instance._connect_to_player_server;

        /// <summary>发送 Protobuf 请求：由 <see cref="RegisterMap.GetMsgIdForType"/> 根据请求体类型解析 <c>msg_id</c>，外层 JSON 含 msg_id 与 detail_info。需单例 <see cref="Instance"/> 已存在且传输层可发送。</summary>
        public static void SendMsg(object req)
        {
            if (req == null) return;
            if (req is not IMessage imessage)
            {
                Debug.LogError($"[WSS] SendMsg: 仅支持 IMessage 请求体，实际类型 {req.GetType().FullName}");
                return;
            }

            int msgId = RegisterMap.GetMsgIdForType(req.GetType());
            if (msgId == 0)
            {
                Debug.LogError(
                    $"[WSS] SendMsg: RegisterMap 中无类型 {req.GetType().FullName} 的 msg_id（请确认 all.json 已加载且含该消息名）。");
                return;
            }

            var self = Instance;
            if (self?._wsClient == null || !self._wsClient.IsSocketOpenForSend())
                return;
            string reqJson = JsonFormatter.Default.Format(imessage);
            var authMsg = new JObject
            {
                ["msg_id"] = msgId,
                ["detail_info"] = JObject.Parse(reqJson)
            };
            self._wsClient.SendTextMsg(authMsg.ToString(Formatting.None));
        }

        /// <summary>主动关闭当前连接。切换网关前请 <c>await close_wss()</c> 再 <c>init_wss()</c>。</summary>
        public async Task close_wss()
        {
            LogWssActiveClose("close_wss 入口（对外 API）");

            var toClose = _wsClient;
            _wsClient = null;
            if (toClose == null)
            {
                Debug.Log("[WSS] close_wss：WsClient 已为 null，无需关闭");
                return;
            }

            LogWssActiveClose("close_wss：关闭进入时捕获的 WsClient");
            toClose.OnWsOpen.RemoveListener(OnWsClientWsOpenPublish);
            toClose.OnInvalid.RemoveListener(OnWsClientInvalid);
            try
            {
                await toClose.CloseAsync();
                Debug.Log("[WSS] close_wss：CloseAsync 结束");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"close_wss: {ex.Message}");
            }
        }


        public void after_send(bool _flag)
        {
            Debug.Log("after_send_flag_=" + _flag);
        }
        public void start_load_image(string _image_url)
        {
            StartCoroutine(DownloadPhotoCoroutine(_image_url));
        }
        private IEnumerator DownloadPhotoCoroutine(string _image_url)
        {
            string fullUrl = _image_url;
            Debug.Log("开始下载图片: " + fullUrl);

            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullUrl))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(www);
                    Debug.Log("图片下载成功！");
                    //onComplete?.Invoke(downloadedTexture);
                    _output_image.texture = downloadedTexture;
                }
                else
                {
                    Debug.LogError($"图片下载失败: {www.error}");
                    // onComplete?.Invoke(null);
                }
            }
        }

       
    }
}

