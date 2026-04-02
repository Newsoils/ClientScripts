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

        [Header("UI")]
        public TMP_Text _ouput_text;

        public TMP_InputField _inputfield_Player_Count;
        public TMP_InputField _inputfield_PW;
        public RawImage _output_image;
        //public string image_url;
        public WebSocket ws;
        public string _output_str;

        public bool use_local = true;

        [Header("Event")]
        public float _on_connection_close_delay = 5f;
        public UnityEvent _evt_on_connection_closed;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;

                DontDestroyOnLoad(this.gameObject);

                if (use_local == true)
                {
                    url = _local_url;
                }
                else
                {
                    url = _remote_url;
                }
                init_wss();
                _ouput_text.text = "OK";
                //StartCoroutine(DownloadPhotoCoroutine());

            }
            else
            {
                if (instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        private void Start()
        {
            _evt_on_connection_closed.AddListener( TryReconnect);
        }
        private async void OnDestroy()
        {
            _evt_on_connection_closed.RemoveAllListeners();

            if (ws != null) await ws.Close();
            //StopAllCoroutines();
        }
       

        void TryReconnect()
        {
            SceneLoadHelper.LoadLoginScene((s) => EvtDsp.TriggerEvt(EvtNames.Reconnect));
        }
        



        public void FixedUpdate()
        {
            //_ouput_text.text = _output_str;
            if (_on_connection_closed == true)
            {
                StartCoroutine(_on_connection_closed_co());
                _on_connection_closed = false;
            }

#if !UNITY_WEBGL
            ws.DispatchMessageQueue();
#endif
        }

        public IEnumerator _on_connection_closed_co()
        {
            _is_connected = false;
            _ouput_text.text = "Connection closed, quit game in " + _on_connection_close_delay + " seconds";
            yield return new WaitForSecondsRealtime(_on_connection_close_delay);
            _evt_on_connection_closed.Invoke();
        }





        public void init_wss()
        {
            ws = new WebSocket(url);


            ws.OnOpen += () =>
            {
                Debug.Log("ws_open");
                _is_connected = true;
            };

            ws.OnMessage += on_message;

            ws.OnError += (e) =>
            {
                Debug.Log("ws.OnError()");
                Debug.Log($"ws.OnError()_msg_={e}");
            };

            ws.OnClose += (e) =>
            {
                if (_is_connected == true)
                {
                    Debug.Log("ws.OnClose()_Close_Code_=_" + e);
                    _on_connection_closed = true;

                    _is_connected = false;

                }

            };


            //StartCoroutine(send_msg_loop("client_ws.Send()"));
            //  StartCoroutine(try_connnect_co());

            try_connnect_async();
        }
        public void set_player_name(string str)
        {
            _player_name = str;
        }
        public async void try_connnect_async()
        {
            while (true)
            {
                if (ws != null && (ws.State == WebSocketState.Closed))
                {
                    Debug.Log("try_connnect_async(): Socket is closed, attempting to connect.");
                    ws.Connect();
                }
                if (_is_connected == true)
                {
                    Debug.Log("try_connnect_async(): Connected successfully._breaking_loop");
                    return;
                }
#if UNITY_EDITOR
                if (Application.isPlaying == false)
                {
                    Debug.Log("try_connnect_async(): Application.isPlaying == false, breaking loop");
                    break;
                }
#endif
                await Task.Delay(4096);
            }
        }


        public async void on_message(byte[] _raw_data)
        {
            //Debug.Log("ws.OnMessage()_e.e.IsBinary_#_+" + _raw_data.LongLength);
            var _msg_str = LZ4_Helper.Decode(_raw_data);
            on_message(_msg_str);
            await Task.CompletedTask;
            // getting the message as a string
            // var message = System.Text.Encoding.UTF8.GetString(bytes);
            // Debug.Log("OnMessage! " + message);
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

            /*
              string _msg = "ws.OnMessage()_e.IsText_#_+" + msg;
             Debug.Log(_msg);
             _output_str = _msg;
             */
            /*
                 if (_msg.Contains("quit_game"))
                {
                    await System.Threading.Tasks.Task.Delay(4096);

                    _wait_quit = true;
                    return;
                }

             */

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

            _msg_dispatcher._msg_buffer.Add(
                   _msg
                   );
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
                    _ouput_text.text = _output_str;
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
            await ws.Close();
        }


        public void after_send(bool _flag)
        {
            Debug.Log("after_send_flag_=" + _flag);
        }


        public void upload_photo_to_server(
            string _photo_name,
            Texture2D _photo
            )
        {

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

