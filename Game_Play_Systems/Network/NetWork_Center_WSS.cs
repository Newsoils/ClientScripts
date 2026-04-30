using System;
using System.Collections;
using System.Threading.Tasks;
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


namespace CLIP.Project_Mouse.Game_Play_System
{

    public class NetWork_Center_WSS : MonoBehaviour
    {
        public static NetWork_Center_WSS instance;
        public Msg_Dispatcher _msg_dispatcher;

        private string url = "ws://127.0.0.1:25564/";

        public string _local_url = "ws://192.168.31.184:25564/";
        public string _remote_url = "ws://47.116.175.125 :25564/";

        [Header("Current_State")]
        public string _player_name;
        public bool _connect_to_player_server = false;
        public bool _is_connected = false;
        public bool _on_connection_closed = false;
        public int _current_send_msg_id = -1;
        public int _current_receive_msg_id = -1;

        public RawImage _output_image;
        public WebSocket ws;

        public bool use_local = true;

        [Header("Event")]
        public float _on_connection_close_delay = 5f;
        public UnityEvent _evt_on_connection_closed = new UnityEvent();

        [Header("Reconnect_Settings")]
        [SerializeField] private int _max_reconnect_attempts = 20;
        [SerializeField] private int _max_reconnect_attempts_after_login = 3;
        [SerializeField] public float _reconnect_interval = 1.5f;
        private int _current_reconnect_attempts = 0;
        private bool _is_connecting = false;
        private Coroutine _reconnect_coroutine;

        [Header("Resume_Relogin_Settings")]
        [SerializeField] private float _resume_relogin_debounce_seconds = 2.0f;
        private float _last_resume_relogin_time = -999f;
        private Coroutine _resume_relogin_coroutine;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;

                DontDestroyOnLoad(this.gameObject);

                url =  use_local?_local_url:_remote_url;
                init_wss();
                //StartCoroutine(DownloadPhotoCoroutine());

            }
            else
            {
                if (instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
            _current_reconnect_attempts = 0;
        }

        private void Start()
        {
            _evt_on_connection_closed.AddListener(OnDisconnected);

            EvtDsp.AddEvt(EvtNames.Network_Disconnect, BackToLogin);

        }

        public void FixedUpdate()
        {
#if !UNITY_WEBGL
            if (ws != null) ws.DispatchMessageQueue();
#endif
        }

        private async void OnDestroy()
        {
            _evt_on_connection_closed.RemoveAllListeners();

            if (ws != null) await ws.Close();
            //StopAllCoroutines();

            EvtDsp.RemoveEvt(EvtNames.Network_Disconnect, BackToLogin);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                TryResumeReloginIfNeeded("OnApplicationPause(false)");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                TryResumeReloginIfNeeded("OnApplicationFocus(true)");
            }
        }

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
                TryReconnectAfterLoginCoroutine();
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
            ws = new WebSocket(url);
            SetupWsCallbacks();
            ws.Connect();
        }
        private void SetupWsCallbacks()
        {
            // 重要：回调里捕获当前 ws，避免旧 ws 的回调影响新连接的状态
            var boundWs = ws;

            ws.OnOpen += () =>
            {
                if (!ReferenceEquals(boundWs, ws)) return;
                Debug.Log("ws_open");
                _is_connected = true;
                _current_reconnect_attempts = 0;
            };

            ws.OnMessage += (bytes) =>
            {
                if (!ReferenceEquals(boundWs, ws)) return;
                on_message(bytes);
            };

            ws.OnError += (e) =>
            {
                if (!ReferenceEquals(boundWs, ws)) return;
                Debug.Log("ws.OnError()_msg_=" + e);
                HandleWsFailure(isCloseEvent: false, detail: e);
            };

            ws.OnClose += (e) =>
            {
                if (!ReferenceEquals(boundWs, ws)) return;
                if (_is_connected)
                {
                    _is_connected = false;
                    Debug.Log("ws.OnClose()_Close_Code_=_" + e);
                    HandleWsFailure(isCloseEvent: true, detail: e.ToString());
                }
            };
        }

        private void HandleWsFailure(bool isCloseEvent, string detail)
        {
            // 登录进入主界面后：后台尝试重连，失败 3 次后再触发原有“服务器连接失败”逻辑
            if (_connect_to_player_server)
            {
                Debug.LogWarning($"WSS {(isCloseEvent ? "OnClose" : "OnError")} after login: {detail}");
                TryReconnectAfterLoginCoroutine();
                return;
            }

            // 未登录阶段：保持原有行为（立即提示连接失败）
            Debug.LogWarning($"WSS {(isCloseEvent ? "OnClose" : "OnError")} before login: {detail}");
            TriggerOriginalServerConnectionFailed();
        }

        private void TriggerOriginalServerConnectionFailed()
        {
            EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, "服务器连接失败，请检查网络设置！", null);
        }

        private void OnDisconnected()
        {
            Debug.LogError("已断联");
            StartCoroutine(_on_connection_closed_co());
            // 这里依旧保留事件链：如果是登录后断联，会走 3 次后台重连；否则走原来的重连逻辑
            if (_connect_to_player_server) TryReconnectAfterLoginCoroutine();
            else TryReconnectCoroutine();
        }

        public IEnumerator _on_connection_closed_co()
        {
            _is_connected = false;
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

        private void TryReconnectAfterLoginCoroutine()
        {
            StopReconnectAttempts();
            _reconnect_coroutine = StartCoroutine(AttemptReconnectLoop(_max_reconnect_attempts_after_login, showSuccessPrompt: false, onFinalFail: TriggerOriginalServerConnectionFailed));
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
                yield return CloseWsCoroutine();

                ws = new WebSocket(url);
                SetupWsCallbacks();
                ws.Connect();

#if !UNITY_WEBGL
                ws.DispatchMessageQueue();
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
            _is_connecting = false;
        }

        private IEnumerator CloseWsCoroutine()
        {
            if (ws == null) yield break;

            Task closeTask = null;
            try
            {
                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.Connecting)
                {
                    closeTask = ws.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"CloseWsCoroutine exception: {ex.Message}");
                yield break;
            }

            if (closeTask != null)
            {
                while (!closeTask.IsCompleted) yield return null;
            }
        }
      
        public void set_player_name(string str)
        {
            _player_name = str;
        }

        public async void on_message(byte[] _raw_data)
        {
            //Debug.Log("ws.OnMessage()_e.e.IsBinary_#_+" + _raw_data.LongLength);
            var _msg_str = LZ4_Helper.Decode(_raw_data);
            on_message(_msg_str);
            await Task.CompletedTask;

        }
        public async void on_message(string msg)
        {
            // ① 处理服务器心跳
            if (msg == "server_ping")
            {
                // 回复心跳
                _ = ws.SendText("client_pong");
                Log.Custom("Received server_ping, sent client_pong","NetWork_Center_WSS",Color.cyan);
                return;     // ⬅️ 不继续往下解析 JSON
            }

            _current_receive_msg_id++;
            var _network_msg = GF_SP.DeserializeObject<Network_Msg>(msg);
            handle_msg(_network_msg);
            return;
        }
        public void Add_Receiver(string name,IMsg_Receiver msg_Receiver)
        {
            _msg_dispatcher.add_msg_receiver(
                name,msg_Receiver
                );
        }
        public void Remove_Receiver(string name)
        {
            _msg_dispatcher.remove_msg_receiver(
                name
                );
        }
        public void handle_msg(Network_Msg _msg)
        {
            _msg_dispatcher._msg_buffer.Add( _msg );
        }

        public IEnumerator send_msg_loop(string message)
        {
            while (true)
            {
                //_text.text = Time.time.ToString();
                var _message = message + "_" + DateTime.Now.ToString();
                if (ws != null && ws.State == WebSocketState.Open)
                {
                    //ws.SendAsync(message, after_send);
                    send_via_wss(_message);
                }
                else
                {
                    string _output_str = "WebSocket is not connected.";
                    Debug.Log(_output_str);
                    Debug.Log(_output_str);
                    //yield break;
                }
                yield return new WaitForSeconds(2f);
            }

        }

        public async void send_via_wss(string _data)
        {
            if (ws != null && ws.State == WebSocketState.Open)
            {
                //ws.Send(_data);
                byte[] _data_binary = LZ4_Helper.Encode(_data);
                await send_via_wss(_data_binary);
            }
        }

        public async Task send_via_wss(byte[] _data)
        {
            if (ws != null && ws.State == WebSocketState.Open)
            {
                await ws.Send(_data);
            }
        }

        public void send_via_wss(Network_Msg _msg)
        {
            _current_send_msg_id++;
            _msg.player_id = _player_name;
            _msg.msg_id = _current_send_msg_id;
            send_via_wss(GF_SP.SerializeObject(_msg));
        }
 
        public async void close_wss()
        {
            if (ws != null) await ws.Close();
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

