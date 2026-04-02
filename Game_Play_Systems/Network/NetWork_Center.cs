using System.Collections;

using UnityEngine;
using UnityEngine.Events;
//using UnityWebSocket;
using TMPro;
#if WX
using WeChatWASM;
#endif
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using CLIP.Framework_Core.Network;
using CLIP.Framework_Core.Tools;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class NetWork_Center : MonoBehaviour, IMsg_Receiver
    {
        // Start is called before the first frame update
        public static NetWork_Center instance;
        public Msg_Dispatcher _msg_dispatcher;

        public string ws_url_remote;
        public string ws_url_local;
        public string wss_url;
        public bool socket_opened = true;
        public int receive_count = 0;
        public int send_count = 0;
        public string player_id;
        public string player_pw_send;

        public string player_pw_passed;
        //  public WebSocket socket;
        public TextMeshProUGUI _text;
        [Header("UI")]
        public InputField _player_id_input;
        public InputField _player_pw_input;
        [Header("Events")]
        public UnityEvent _on_login_success = new UnityEvent();
        public bool _after_invoke_on_login_success = false;
        [Header("For_Sending_Throttle")]
        public bool _on_sending = false;
        public float _sending_interval = 0.1f;
        [Header("Heart_Beat_Time")]
        public bool _connect_to_player_server = false;
        public string _last_heart_beat_time_str;
        public int delay_in_ms;
        public DateTime _last_heart_beat_time_server_time;
        public DateTime _last_heart_beat_time_client_time;
        [Header("Reconnect")]
        public int _reconnect_time = 4;
        public bool _on_reconnect = false;
        public bool _on_try_login = false;
        void Start()
        {
            if (instance == null)
            {
                instance = this;

                DontDestroyOnLoad(this.gameObject);

                if (_msg_dispatcher != null)
                {
                    _msg_dispatcher.add_msg_receiver(get_receiver_name(), this);
                }

            }
            else
            {
                if (instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
#if UNITY_EDITOR
            wss_url = ws_url_local;
            StartCoroutine(begin_ws_connection_co());
#endif

#if WX && !UNITY_EDITOR
      wss_url = ws_url_remote;
  StartCoroutine(begin_ws_connection_co());

#endif
        }

 
        public IEnumerator begin_ws_connection_co()
        {
            Debug.Log("Start_Looping_For_Waiting_WeChat_Envir_OK");
            while (true)
            {

                Debug.Log("Looping_For_Begin_ws_connection()");
                yield return new WaitForSecondsRealtime(2f);
#if WX && !UNITY_EDITOR

                    if (WeChat_Env.instance == null) continue;
                    if (WeChat_Env.instance.is_web_chat_env_OK == false) continue;
#endif
                if (socket_opened == true)
                {
                    Debug.Log("begin_ws_connection_co()_#_socket_opened == true_#_yield break");
                    yield break;
                }
                begin_ws_connection();

                if (socket_opened == true)
                {
                    Debug.Log("begin_ws_connection_co()_#_socket_opened == true_#_yield break");
                    yield break;
                }

            }
        }
        public void begin_ws_connection()
        {
            /*
               if (socket!=null&&socket.ReadyState!=null)
              {
                  if (socket.ReadyState == WebSocketState.Open) {

                      Debug.Log("begin_ws_connection()_socket.ReadyState == WebSocketState.Open");
                      return;
                  }


              }
              Debug.Log("begin_ws_connection()_Begin_Socket_Connection");
              socket = new WebSocket(wss_url);

              // 注册回调
              socket.OnOpen += Socket_OnOpen;
              socket.OnClose += Socket_OnClose;
              socket.OnMessage += Socket_OnMessage;
              socket.OnError += Socket_OnError;

              // 连接
              socket.ConnectAsync();
             */
            // 发送 string 类型数据
            //socket.SendAsync(str);
            //
            //
            //或者 发送 byte[] 类型数据（建议使用）
            //socket.SendAsync(bytes);
            // 关闭连接
            //
            //StartCoroutine(echo_coro());
        }
        public IEnumerator echo_coro()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(4f);
                if (socket_opened == false) continue;
                // socket.SendAsync("Client_Send_@_" + System.DateTime.Now);
            }
        }

        public void save_player_id()
        {
            Debug.Log("Try_Save_Player_Id");
#if WX && !UNITY_EDITOR
                if (WeChat_Env.instance != null)
                {
                    WeChat_Env.instance.write_file_in_WX("player_id.txt", _player_id_input.text);
                    Debug.Log("save_to_player_id.txt" + "_#_" + _player_id_input.text);
                }
#endif
        }

        public void close_ws()
        {
            Debug.Log(gameObject.name + "_close_ws");
        }

        public void OnApplicationQuit()
        {
            Debug.Log(gameObject.name + "_On_Application_Quit()");
        }
        public void AddLog(string str)
        {

            Debug.Log(str);
        }


        private void OnDestroy()
        {
            if (Msg_Dispatcher._instance != null)
            {
                Msg_Dispatcher._instance.remove_msg_receiver(this.get_receiver_name());
            }
        }

       
        public void handle_msg(Network_Msg _msg)
        {
            if (_msg.action == "Server_Heart_Beat")
            {
                _connect_to_player_server = true;
                _last_heart_beat_time_server_time = GF_SP.DeserializeObject<DateTime>(_msg.detail_info);
                _last_heart_beat_time_str = _last_heart_beat_time_server_time.ToString();
                _last_heart_beat_time_client_time = DateTime.Now;
                delay_in_ms = (int)(_last_heart_beat_time_client_time - _last_heart_beat_time_server_time).TotalMilliseconds;
            }
            else
            {
                _msg_dispatcher._msg_buffer.Add(
                    _msg
                    );
            }
        }
        /*

          private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            socket_opened = false;
            AddLog(string.Format("Closed: StatusCode: {0}, Reason: {1}", e.StatusCode, e.Reason));
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            socket_opened = false;
            AddLog(string.Format("Error: {0}", e.Message));
            if(socket.ReadyState==WebSocketState.Open||
                socket.ReadyState == WebSocketState.Connecting
                ) socket.CloseAsync();
            //begin_ws_connection();
        }


         */


        public void try_login()
        {
#if UNITY_EDITOR
            //   _on_login_success.Invoke();
#endif
            if (_connect_to_player_server == true) return;
            if (socket_opened == false) return;
            if (_player_id_input != null) player_id = _player_id_input.text;
            if (_player_pw_input != null) player_pw_send = _player_pw_input.text;

            if (player_id == null || player_id.Length == 0) return;
            if (player_pw_send == null || player_pw_send.Length == 0) return;


            var _msg = new Network_Msg();
            _msg.player_id = player_id;
            _msg.sender = get_receiver_name();
            _msg.action_target = "Login_Manager";
            _msg.action = "Try_Login";
            _msg.detail_info = JsonConvert.SerializeObject((player_id, player_pw_send));
            _msg._sending_mode = Msg_Sending_Mode.Client_to_Server;

            send_msg_to_server(_msg);
        }
        public IEnumerator try_login_co()
        {
            _on_try_login = true;
            while (true)
            {
                yield return new WaitForSecondsRealtime(4f);
                if (_connect_to_player_server == true)
                {
                    Debug.Log("try_login_co()_Server_Connected");
                    break;
                }
                if (socket_opened == false)
                {
                    Debug.Log("try_login_co()_socket_opened == false");
                    continue;
                }
                _on_try_login = true;
                try_login();

            }
            _on_try_login = false;
        }
        public void receive_msg(Network_Msg _msg)
        {
            if (_msg.action == "Login_Success")
            {
                if (_text != null) _text.text = _msg.detail_info;
                player_pw_passed = player_pw_send;
                //_connect_to_player_server = true;
                _last_heart_beat_time_client_time = DateTime.Now;
                StartCoroutine(_start_heart_beat_co());
                if (_on_reconnect == false && _after_invoke_on_login_success == false)
                {
                    StartCoroutine(on_login_success_co());
                }
                else if (_on_reconnect == true)
                {
                    _on_reconnect = false;
                }
            }
            if (_msg.action == "Login_Failure")
            {
                _text.text = _msg.detail_info;
            }

            if (_msg.action == "Quit_Client")
            {
                quit_game();
            }

            if (Msg_Dispatcher._instance != null)
            {
                if (Msg_Dispatcher._instance._current_msg.action_target == get_receiver_name())
                {
                    Msg_Dispatcher._instance._current_msg = new Network_Msg(-1);
                }
            }
        }

        public IEnumerator _start_heart_beat_co()
        {
            while (true)
            {

                if ((DateTime.Now - _last_heart_beat_time_client_time).TotalSeconds >= _reconnect_time)
                {
                    reconnect_to_server();
                    yield break;
                }
                var _msg = new Network_Msg();
                _msg.sender = get_receiver_name();
                _msg.action_target = "Player_Server_" + player_id;
                _msg.action = "Client_Heart_Beat";
                _msg.detail_info = GF_SP.SerializeObject(DateTime.Now);
                _msg._sending_mode = Msg_Sending_Mode.Client_to_Server;
                send_msg_to_server(_msg);
                yield return new WaitForSecondsRealtime(2f);
            }
        }

        public void reconnect_to_server()
        {
            _on_reconnect = true;
            _connect_to_player_server = false;
            delay_in_ms = 999;
            close_ws();
            StartCoroutine(begin_ws_connection_co());
            //begin_ws_connection();
            if (_on_try_login == false) StartCoroutine(try_login_co());
            Debug.Log("reconnect_to_server()_#_Reconnect");
        }
        public IEnumerator on_login_success_co()
        {
            if (_after_invoke_on_login_success == true) yield break;
            yield return new WaitForSecondsRealtime(0.25f);

            _on_login_success.Invoke();
            _after_invoke_on_login_success = true;
        }
        public void send_msg_to_server(Network_Msg _msg)
        {
            //StartCoroutine(send_msg_to_server_co(_msg));

            _msg.msg_id = send_count;
            _msg.player_id = player_id;
      
            var _msg_str = GF_SP.SerializeObject(_msg);
            var _data = LZ4_Helper.Encode(_msg_str);
            // socket.SendAsync(_data);
            send_count++;
        }


        public IEnumerator send_msg_to_server_co(Network_Msg _msg)
        {
            if (_on_sending == true) yield break;
            _on_sending = true;
            send_msg_to_server(_msg);
            yield return new WaitForSecondsRealtime(_sending_interval);
            _on_sending = false;
        }
        public string get_receiver_name()
        {
            return this.gameObject.name;
        }


        public static System.DateTime try_get_server_time(out bool _flag_is_server_time)
        {
            if (instance == null)
            {
                _flag_is_server_time = false;
                return System.DateTime.Now;

            }
            if (instance._connect_to_player_server == false)
            {
                _flag_is_server_time = false;
                return System.DateTime.Now;
            }
            _flag_is_server_time = true;
            return instance._last_heart_beat_time_server_time;
        }

        public void quit_game()
        {
#if WX && !UNITY_EDITOR
                    if(WeChat_Env.instance != null)
                    {
                        WeChat_Env.instance.quit_game();

                    }
#endif

#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#endif

#if !WX && !UNITY_EDITOR
                Application.Quit();
#endif
        }
    }
}