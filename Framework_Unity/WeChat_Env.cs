using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
#if WX
using WeChatWASM;
#endif

namespace CLIP.Framework_Unity
{

    public class WeChat_Env : MonoBehaviour
    {
        // Start is called before the first frame update
#if WX
            public static WeChat_Env instance;
            public string CDN_URL = "https://127.0.0.1:56467";
            public UnityEvent on_font_loaded = new UnityEvent();
            public Font _wx_font;
            //public TextMeshProUGUI _text;
            public bool is_web_chat_env_OK = false;

            public string _version;

            public WXBannerAd _banner_ad;

            public bool _banner_ad_loaded = false;
            [Header("User_Info_Ouput")]
            public string _phone_number;
            public RawImage _icon;
            public TextMeshProUGUI _user_name_text;
            public WXUserInfo userInfo;
            private bool infoFlag = false;
            public Texture2D avatarTexture;

            [Header("Event")]
            public UnityEvent _on_try_begin_actual_game=new UnityEvent();
            [Header("UI")]
            public Button begin_game_btn;
            void Start()
            {
                Debug.Log("Version_is_" + _version);
                if (instance == null)
                {
                    instance = this;
                    DontDestroyOnLoad(this.gameObject);
                }
                else
                {
                    if (instance != this)
                    {
                        Destroy(this.gameObject);
                    }

                }


#if WX && !UNITY_EDITOR


        Debug.Log("WX_On_ver.20250227");
        var callback = new GetSystemInfoOption();
        callback.success += (info) =>
        {
            Debug.Log(info);

        };
        WX.GetSystemInfo(callback);
        WX.ReportGameStart();
       
        var log_callback = new LoginOption();
        log_callback.success += (LoginSuccessCallbackResult info) =>
        {
            //var result = info();
            Debug.Log("Login_info : "+ info.code);
            is_web_chat_env_OK=true;
        };
        WX.Login(log_callback);

      //     SetEnableDebugOption _enable_debug_callback = new SetEnableDebugOption();
     //   _enable_debug_callback.success += (result) =>
       // {
      //      Debug.Log(" _enable_debug_OK");
     //       Debug.Log(result);
      //  };
      //  WX.SetEnableDebug(_enable_debug_callback);

        set_up_font();
        //try_add_banner_ad();

          //Debug.Log("_test_write_and_read()");
          //StartCoroutine(_test_write_and_read());

           Debug.Log("Try init_wx_sdk()");
          init_wx_sdk();
#if PLATFORM_WEIXINMINIGAME
    WeixinMiniGameInput.mobileKeyboardSupport = true;
#elif PLATFORM_WEBGL
#if UNITY_2022_1_OR_NEWER
        WebGLInput.mobileKeyboardSupport = true;
#endif
#endif
#endif
#if UNITY_EDITOR
                begin_game_btn.onClick.AddListener(() => { 
                
                _on_try_begin_actual_game.Invoke();
                });
#endif
            }

            // Update is called once per frame
            void Update()
            {

            }
#if WX
            public void init_wx_sdk()
            {
                WX.InitSDK((code) => {

                    Debug.Log("init_WX_SDK_code: " + code);

                    this.LoaderWXMess();
                
                
                });
            }
 
            /// <summary>
            /// 加载微信授权相关信息
            /// </summary>
            private void LoaderWXMess()
            {
                // 1. 询问隐私协议授权情况
                WX.GetPrivacySetting(new GetPrivacySettingOption()
                {
                    success = (res) =>
                    {
                        /**
                         * needAuthorization - 含义
                         * 是否需要用户授权隐私协议（如果开发者没有在[mp后台-设置-服务内容声明-用户隐私保护指引]中声明隐私收集类型则会返回false；
                         * 如果开发者声明了隐私收集，且用户之前同意过隐私协议则会返回false；
                         * 如果开发者声明了隐私收集，且用户还没同意过则返回true；
                         * 如果用户之前同意过、但后来小程序又新增了隐私收集类型也会返回true）
                         */
                        // 询问成功
                        if (res.needAuthorization)
                        {
                            // 有隐私协议，且未授权
                            // 2. 发起隐私协议授权
                            // 弹出隐私协议询问弹窗
                            WX.RequirePrivacyAuthorize(new RequirePrivacyAuthorizeOption()
                            {
                                success = (res) =>
                                {
                                    Debug.Log("同意隐私协议：" + JsonUtility.ToJson(res, true));
                                    // 用户同意隐私协议
                                    // 3. 获取用户信息
                                    this.GetScopeInfoSetting();
                                    // 将 信息获取标志 标记为true
                                    this.infoFlag = true;
                                },
                                fail = (err) =>
                                {
                                    Debug.Log("拒绝隐私协议：" + JsonUtility.ToJson(res, true));
                                },
                                complete = (res) =>
                                {
                                    Debug.Log("询问隐私协议结束");
                                }
                            });
                        }
                    },
                    fail = (err) => { },
                    complete = (res) =>
                    {
                        // 处理询问隐私协议失败或之前已经同意但未授权用户信息的情况
                        if (!this.infoFlag)
                        {
                            this.GetScopeInfoSetting();
                        }
                    }
                });
            }
            private void GetScopeInfoSetting()
            {
                // 询问用户信息授权情况
                WX.GetSetting(new GetSettingOption()
                {
                    success = (res) =>
                    {
                        Debug.Log("获取用户信息授权情况成功: " + JsonUtility.ToJson(res.authSetting, true));
                        // 判断用户信息的授权情况
                        if (!res.authSetting.ContainsKey("scope.userInfo") || !res.authSetting["scope.userInfo"])
                        {
                            // 3.1 未授权，创建授权按钮区
                            // 需引导用户点击所创建的区域，这里的做法是将开始游戏的按钮放在该区域
                            this.CreateUserInfoButton();
                        }
                        else
                        {
                            // 3.2 已授权，直接获取用户信息
                            this.GetUserInfo();
                            // 这里也可以先不获取，留到点击开始游戏按钮再获取，但没必要，先获取后存起来即可
                        }
                    },
                    fail = (err) =>
                    {
                        Debug.Log("获取用户信息授权情况失败：" + JsonUtility.ToJson(err, true));
                    }
                });
            }


            private void CreateUserInfoButton()
            {
                Debug.Log("create userinfo button area");

                /**
                 * 方法一：创建用户信息获取按钮，在底部区域创建一个300高度的 透明！！！ 区域
                 * 首次获取会弹出用户授权窗口, 可通过右上角-设置-权限管理用户的授权记录
                 * 可根据需要设置不同高度，使用屏幕还有其它点击热区的情况
                 */
                // 获取屏幕信息
                // var systemInfo = WX.GetSystemInfoSync();
                // var canvasWith = (int)(systemInfo.screenWidth * systemInfo.pixelRatio);
                // var canvasHeight = (int)(systemInfo.screenHeight * systemInfo.pixelRatio);
                // var buttonHeight = (int)(canvasWith / 1080f * 300f);
                // 很容易被误导，与其叫按钮，不如叫热区
                // WXUserInfoButton btn = WX.CreateUserInfoButton(0, canvasHeight - buttonHeight, canvasWith, buttonHeight, "zh_CN", false);
                /**
                 * 方法二：创建布满整个屏幕的授权按钮区
                 */
                WXUserInfoButton btn = WX.CreateUserInfoButton(0, 0, Screen.width, Screen.height, "zh_CN", false);
                // 监听授权区域的点击
                btn.OnTap((res) =>
                {
                    Debug.Log("click userinfo btn: " + JsonUtility.ToJson(res, true));
                    if (res.errCode == 0)
                    {
                        // 用户已允许获取个人信息，返回的 res.userInfo 即为用户信息
                        Debug.Log("userinfo: " + JsonUtility.ToJson(res.userInfo, true));
                        // 将用户信息存入成员变量，以待后用
                        this.userInfo = res.userInfo;
                        // 展示，只是为了测试看到
                        this.ShowUserInfo(res.userInfo.avatarUrl, res.userInfo.nickName);
                    }
                    else
                    {
                        Debug.Log("用户拒绝获取个人信息");
                    }
                    // 最后隐藏授权区域，防止阻塞游戏继续
                    btn.Hide();
                    Debug.Log("已隐藏热区");
                });
            }

            
            private void GetUserInfo()
            {
                Debug.Log("Try_GetUserInfo");
                WX.GetUserInfo(new GetUserInfoOption()
                {
                    lang = "zh_CN",
                    success = (res) =>
                    {
                        Debug.Log("获取用户信息成功(API): " + JsonUtility.ToJson(res.userInfo, true));
                        // 将用户信息存入成员变量，或存入云端，方便后续使用
                        this.userInfo = this.ConvertUserInfo(res.userInfo);

                        //this.ShowUserInfo(res.userInfo.avatarUrl, res.userInfo.nickName);
                        StartCoroutine(show_user_info_co(res.userInfo.avatarUrl, res.userInfo.nickName));
                    },
                    fail = (err) =>
                    {
                        Debug.Log("获取用户信息失败(API): " + JsonUtility.ToJson(err, true));
                    }
                });
            }
#endif
            public IEnumerator show_user_info_co(string avatarUrl, string nickName)
            {
                ShowUserInfo(avatarUrl, nickName);
                yield return new WaitForSecondsRealtime(0.125f);
              
            }
            /// <summary>
            /// 展示用户信息，对头像、昵称展示的整合
            /// ps: 测试用
            /// </summary>
            /// <param name="avatarUrl"></param>
            /// <param name="nickName"></param>
            private void ShowUserInfo(string avatarUrl, string nickName)
            {
                StartCoroutine(LoadAvatar(avatarUrl));
                showNickname(nickName);
                //_on_try_begin_actual_game.Invoke();
            }

            IEnumerator LoadAvatar(string url)
            {
                // 加载头像图片
                Debug.Log("Avatar_URL_#_" + url);
                UnityWebRequest request = new UnityWebRequest(url);
                DownloadHandlerTexture texture = new DownloadHandlerTexture(true);
                request.downloadHandler = texture;
                yield return request.SendWebRequest();
                if (string.IsNullOrEmpty(request.error))
                {
                    avatarTexture = texture.texture;
                }

                //Sprite sprite = Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), new Vector2(0.5f, 0.5f));
                // 场景中图片对象名称为Avatar
                //Image tempImage = GameObject.Find("Avatar").GetComponent<Image>();

               // _icon.sprite = sprite;
               _icon.texture = avatarTexture;
            }
            void showNickname(string name)
            {
               
                _user_name_text.text = name;
            }

            WXUserInfo ConvertUserInfo(UserInfo userInfo)
            {
                return new WXUserInfo()
                {
                    nickName = userInfo.nickName,
                    avatarUrl = userInfo.avatarUrl,
                    country = userInfo.country,
                    province = userInfo.province,
                    city = userInfo.city,
                    language = userInfo.language,
                    gender = (int)userInfo.gender
                };
            }
 
 
            public IEnumerator _test_write_and_read()
            {
                var _file_name = "test_file_223.txt";
                var _data = "OKOKOK_at_" + _file_name;
                write_file_in_WX(_file_name, _data);
                yield return new WaitForSecondsRealtime(0.5f);
                var _str = read_file_in_WX(_file_name);
                Debug.Log("Write_OK_" + _str);
            }

            public void write_file_in_WX(string _file_name, string _data, Action on_writtten = null)
            {
                var _path = WX.env.USER_DATA_PATH + "/" + _file_name;
                var _fs = WX.GetFileSystemManager();

                _fs.WriteFileSync(_path, _data);

            }
            public string read_file_in_WX(string _file_name)
            {
                string _str = "ERROR";
                try
                {
                    var _path = WX.env.USER_DATA_PATH + "/" + _file_name;
                    var _fs = WX.GetFileSystemManager();
                    var rtn = _fs.AccessSync(_path);
                    if (rtn != "access:ok")
                    {
                        Debug.Log("Can't_Read_File_" + _file_name + "_rtn_#_" + rtn);
                        return "ERROR";
                    }
                    var _data = _fs.ReadFileSync(_path);
                    _str = System.Text.Encoding.UTF8.GetString(_data);
                }
                catch
                {
                    return "ERROR";
                }

                return _str;
            }
            public void set_up_font()
            {
                var fallbackFont = CDN_URL + "/fonts/NotoSansSC-VariableFont_wght.ttf";
                WeChatWASM.WX.GetWXFont(fallbackFont, (font) =>
                {
                    //text.font = font;
                    Debug.Log("Font_Loaded");
                    _wx_font = font;
                    //_text.font = TMP_FontAsset.CreateFontAsset(font);
                    on_font_loaded.Invoke();
                });
            }

            public void try_add_banner_ad()
            {
                _banner_ad = WX.CreateBannerAd(new WXCreateBannerAdParam()
                {
                    adUnitId = "xxxx",
                    adIntervals = 30,
                    style = new Style()
                    {
                        left = 0,
                        top = 0,
                        width = 600,
                        height = 200
                    }
                });
                _banner_ad.OnLoad((action) =>
                {
                    _banner_ad_loaded = true;
                    _banner_ad.Show();
                });
                _banner_ad.OnError((WXADErrorResponse res) =>
                {
                    Debug.Log("AD_Error _err_code =");
                    Debug.Log(res.errCode);
                });

            }

            public void show_banner_ad()
            {
                if (_banner_ad == null) return;
                if (_banner_ad_loaded == false) return;
                _banner_ad.Show();

            }
            public void hide_banner_ad()
            {
                if (_banner_ad == null) return;
                if (_banner_ad_loaded == false) return;
                _banner_ad.Hide();
            }
            public void start_game()
            {
#if WX
                Debug.Log("start game");
                Debug.Log("用户信息：" + JsonUtility.ToJson(this.userInfo, true));
                _on_try_begin_actual_game.Invoke();
#endif
            }

            public Rect ApplySafeArea(RectTransform rect, CanvasScaler cs, Rect lastRect)
            {
#if WX

                // 1. 获取微信窗口信息（包含安全区域）
                var windowInfo = WX.GetWindowInfo();
                Rect nowRect = new()
                {
                    x = (float)windowInfo.safeArea.left,
                    y = (float)windowInfo.safeArea.top,
                    width = (float)windowInfo.windowWidth,
                    height = (float)windowInfo.windowHeight
                };

                if (nowRect != lastRect)
                {
                    float windowHeight = (float)windowInfo.windowHeight;

                    // 2. 计算安全区域上下边距的归一化比例
                    float safeTopNormalized = (float)windowInfo.safeArea.top / windowHeight;
                    float safeBottomNormalized = (windowHeight - (float)windowInfo.safeArea.bottom) / windowHeight;

                    // 3. 动态调整UI根节点的锚点
                    rect.anchorMin = new Vector2(0, safeBottomNormalized);
                    rect.anchorMax = new Vector2(1, 1 - safeTopNormalized);

                    // 4. 调整Canvas Scaler的参考分辨率（可选）
                    cs.referenceResolution = new Vector2(
                        cs.referenceResolution.x,
                        cs.referenceResolution.y * (1 - (safeTopNormalized + safeBottomNormalized))
                    );
                }
                return nowRect;
#endif
            }

            public void quit_game()
            {
#if WX && !UNITY_EDITOR
                var _exit_option = new ExitMiniProgramOption();
                _exit_option.complete += (msg) =>
                {
                    Debug.Log("ExitMiniProgram_complete");
                    Debug.Log(msg.errMsg);
                };
                _exit_option.success += (msg) =>
                {
                    Debug.Log("ExitMiniProgram_success");
                    Debug.Log(msg.errMsg);
                };
                _exit_option.fail += (msg) =>
                {
                    Debug.Log("ExitMiniProgram_fail");
                    Debug.Log(msg.errMsg);
                };

                WX.ExitMiniProgram(_exit_option);
#endif
            }
#endif
    }

}
