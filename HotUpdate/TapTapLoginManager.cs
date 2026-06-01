using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TapSDK.Compliance;
using TapSDK.Compliance.Model;
using TapSDK.Core;
using TapSDK.Login;
using UnityEngine;
using UnityEngine.Networking;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Network;

public class TapTapLoginManager : SingletonMono<TapTapLoginManager>
{
    protected override bool PersistAcrossScenes => true;
    // Start is called before the first frame update
    private string unionId;

    [Header("Login Platform")]
    [SerializeField] private LoginPlatformType loginPlatform = LoginPlatformType.TapTap;

    [Header("Server Login (HTTP -> Gateway -> WS)")]
    [SerializeField] public string accountLoginPostUrl = "http://192.168.31.49:8000/login";
    [SerializeField] private int loginType = 2;
    [SerializeField] private string defaultPasswordForTryLogin = "123456";

    public const int SelfAccountLoginType = 0;
    public const string SelfAccountDefaultPassword = "123456";
    private static readonly Regex SelfAccountNameRegex = new Regex("^[a-zA-Z0-9]{3,}$", RegexOptions.Compiled);

    public LoginPlatformType LoginPlatform => loginPlatform;

    [Tooltip("等待防沉迷 Startup 回调（如 500 登录成功）的最长时间（秒）")]
    [SerializeField] private float complianceStartupTimeoutSeconds = 120f;

    /// <summary>由 <see cref="WaitForTapTapComplianceLoginSuccessAsync"/> 在调用 Startup 前挂起，回调里 TrySetResult 解除等待。</summary>
    private TaskCompletionSource<int> _complianceStartupAwaiter;

    /// <summary>
    /// TapSDK 在同一次 <see cref="TapTapLogin.LoginWithScopes"/> 未完成前再次调用会抛 <c>TapException: Currently logging in</c>；
    /// 用信号量串行化整段登录（含 UI 授权与后续 HTTP/WS）。
    /// </summary>
    private readonly SemaphoreSlim _tapLoginSemaphore = new SemaphoreSlim(1, 1);

    private void OnComplianceSdkCallback(int code, string s)
    {
        // TapTapCompliance.Model.StartUpResult：500 = LOGIN_SUCCESS（防沉迷校验通过），并非 HTTP 500
        if (_complianceStartupAwaiter != null && !_complianceStartupAwaiter.Task.IsCompleted)
            _complianceStartupAwaiter.TrySetResult(code);

        switch (code)
        {
            case StartUpResult.SWITCH_ACCOUNT:
                Debug.Log($"[TapTap Compliance] SWITCH_ACCOUNT(1001)，拉起登录：{s}");
                _ = LoginAsyncIgnoringConcurrent();
                break;
            case StartUpResult.LOGIN_SUCCESS:
                Debug.Log($"[TapTap Compliance] LOGIN_SUCCESS(500)，防沉迷校验通过 msg={s}");
                break;
            default:
                Debug.Log($"[TapTap Compliance] code={code} msg={s}");
                break;
        }
    }
    void Start()
    {
        if (loginPlatform == LoginPlatformType.TapTap)
            InitTapTapSdk();
        EvtDsp.AddEvt(EvtNames.Resume_Silent_Relogin, SilentReloginAndTryLogin);
    }

    void InitTapTapSdk()
    {
        TapTapSdkOptions coreOptions = new TapTapSdkOptions
        {
            clientId = "bzepc2nhpyue2sqkav",
            clientToken = "7168crDoO06GgIvgxg9CBcZ4CtbXJzhWfPb2mPeX",
        };
        TapTapComplianceOption complianceOption = new TapTapComplianceOption
        {
            showSwitchAccount = true,
            useAgeRange = true
        };
        TapTapSdkBaseOptions[] otherOptions = { complianceOption };
        TapTapSDK.Init(coreOptions, otherOptions);
        TapTapCompliance.RegisterComplianceCallback(OnComplianceSdkCallback);
        TapTapLogin.Instance.Logout();
    }

    public void Login()
    {
        if (loginPlatform != LoginPlatformType.TapTap)
        {
            Debug.LogWarning("[TapTapLogin] 当前为自建账号模式，请使用 LoginSelfAccount");
            return;
        }

        Debug.Log("TapTap 登录");
        _ = LoginAsyncIgnoringConcurrent();
    }

    /// <summary>自建账号登录：cmd=3，LoginType=0，Pwd=123456，后续连网关与 1003 与 Tap 一致。</summary>
    public void LoginSelfAccount(string account)
    {
        if (!ValidateSelfAccountName(account, out string error))
        {
            EvtDsp.TriggerEvt(EvtNames.ShowUpPrompt, error);
            return;
        }

        _ = LoginSelfAccountAsync(account.Trim());
    }

    public static bool ValidateSelfAccountName(string account, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(account))
        {
            error = "请输入账号";
            return false;
        }

        var trimmed = account.Trim();
        if (!SelfAccountNameRegex.IsMatch(trimmed))
        {
            error = "账号需至少3位，且只能包含数字或英文字母";
            return false;
        }

        return true;
    }

    async Task LoginSelfAccountAsync(string account)
    {
        await _tapLoginSemaphore.WaitAsync();
        try
        {
            await TryLoginViaHttpThenWebSocketAsync(
                account,
                SelfAccountLoginType,
                SelfAccountDefaultPassword);
        }
        finally
        {
            _tapLoginSemaphore.Release();
        }
    }

    /// <summary>与正在进行的 Tap 登录串行执行；若已有登录在执行则等待其结束后再进入（避免 Currently logging in）。</summary>
    private async Task LoginAsyncIgnoringConcurrent()
    {
        await _tapLoginSemaphore.WaitAsync();
        try
        {
            await LoginAsyncCore();
        }
        finally
        {
            _tapLoginSemaphore.Release();
        }
    }

    /// <summary>
    /// 主界面从后台/锁屏恢复时使用：尽量复用已有会话，不弹 UI，成功后重新向服务器发送 TryLogin。
    /// </summary>
    public async void SilentReloginAndTryLogin()
    {
        try
        {
            if (loginPlatform == LoginPlatformType.SelfAccount)
            {
                if (Login_Manager.Instance != null &&
                    Login_Manager.Instance.TryGetCachedCredentials(out var playerId, out var password) &&
                    !string.IsNullOrEmpty(playerId))
                {
                    await LoginSelfAccountAsync(playerId);
                }

                return;
            }

            // 尽量复用已有账号会话（不主动拉起登录 UI）
            TapTapAccount account = await TapTapLogin.Instance.GetCurrentTapAccount();
            if (account != null)
            {
                unionId = account.unionId;
                if (!await WaitForTapTapComplianceLoginSuccessAsync(unionId))
                    return;
                Login_Manager.Instance.TryLoginForResume(unionId, "123456");
                return;
            }

            // 如果拿不到会话，再走一次登录流程（可能会弹 UI，具体取决于 SDK）
            await LoginAsyncIgnoringConcurrent();

            if (!string.IsNullOrEmpty(unionId))
            {
                Login_Manager.Instance.TryLoginForResume(unionId, "123456");
            }
        }
        catch (Exception exception)
        {
            Debug.Log($"SilentReloginAndTryLogin failed: {exception}");
        }
    }

    private async Task LoginAsyncCore()
    {
        try
        {
            // 定义授权范围
            List<string> scopes = new List<string>
            {
                TapTapLogin.TAP_LOGIN_SCOPE_PUBLIC_PROFILE
            };
            // 发起 Tap 登录
            var userInfo = await TapTapLogin.Instance.LoginWithScopes(scopes.ToArray());
            Debug.Log($"登录成功，当前用户 ID：{userInfo.unionId}");
            TapTapAccount account = await TapTapLogin.Instance.GetCurrentTapAccount();
            if (account != null)
            {
                string userIdentifier = account.unionId;
                unionId = account.unionId;

                // 先完成防沉迷 Startup（收到 500 后再向登录服发 cmd=3、连网关 WS、排队 1003）
                if (!await WaitForTapTapComplianceLoginSuccessAsync(userIdentifier))
                {
                    Debug.LogWarning("[TapTapLogin] 防沉迷未完成或失败，已中止：不会发起登录服 cmd=3 与 WebSocket");
                    return;
                }

                await TryLoginViaHttpThenWebSocketAsync(unionId, loginType, null);
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("用户取消登录");
        }
        catch (TapException tapEx)
        {
            // 理论上已由 LoginAsyncIgnoringConcurrent 串行化；保留兜底，避免误 Logout 加重状态错乱
            var msg = tapEx.Message ?? "";
            if (msg.IndexOf("Currently logging in", StringComparison.OrdinalIgnoreCase) >= 0)
                Debug.LogWarning($"[TapTapLogin] TapSDK 登录进行中，忽略重复触发：{tapEx}");
            else
            {
                Debug.Log($"登录失败，出现异常：{tapEx}");
                TapTapLogin.Instance.Logout();
            }
        }
        catch (Exception exception)
        {
            Debug.Log($"登录失败，出现异常：{exception}");
            TapTapLogin.Instance.Logout();
        }
    }

    /// <summary>
    /// 调用 <see cref="TapTapCompliance.Startup"/> 并等待回调为 <see cref="StartUpResult.LOGIN_SUCCESS"/>（500）后再继续。
    /// </summary>
    private async Task<bool> WaitForTapTapComplianceLoginSuccessAsync(string unionIdForStartup)
    {
        if (string.IsNullOrEmpty(unionIdForStartup))
            return false;

        _complianceStartupAwaiter = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        Debug.Log($"[TapTapLogin] 发起防沉迷 Startup，通过后再连接登录服并发 cmd=3 unionId={unionIdForStartup}");
        TapTapCompliance.Startup(unionIdForStartup);

        int timeoutMs = Mathf.Max(5000, Mathf.RoundToInt(complianceStartupTimeoutSeconds * 1000f));
        var timeoutTask = Task.Delay(timeoutMs);
        var completed = await Task.WhenAny(_complianceStartupAwaiter.Task, timeoutTask);
        if (completed == timeoutTask)
        {
            Debug.LogError($"[TapTapLogin] 防沉迷 Startup 等待超时（{timeoutMs}ms），未收到回调");
            _complianceStartupAwaiter = null;
            return false;
        }

        int code = await _complianceStartupAwaiter.Task;
        _complianceStartupAwaiter = null;

        if (code != StartUpResult.LOGIN_SUCCESS)
        {
            Debug.LogWarning($"[TapTapLogin] 防沉迷未通过（code={code}），不发起登录服 HTTP/WS");
            return false;
        }

        return true;
    }

    private async Task TryLoginViaHttpThenWebSocketAsync(
        string loginUid,
        int? overrideLoginType = null,
        string password = null)
    {
        if (string.IsNullOrWhiteSpace(accountLoginPostUrl))
        {
            Debug.LogError("账号登录 POST 地址未配置：请在 Inspector 里设置 TapTapLoginManager.accountLoginPostUrl");
            return;
        }

        int effectiveLoginType = overrideLoginType ?? loginType;
        object payload = string.IsNullOrEmpty(password)
            ? new { LoginUID = loginUid, LoginType = effectiveLoginType }
            : new { LoginUID = loginUid, LoginType = effectiveLoginType, Pwd = password };

        var reqBody = new { cmd = 3, payload };
        string reqJson = JsonConvert.SerializeObject(reqBody);

        string respJson = await PostJsonAsync(accountLoginPostUrl, reqJson);
        if (string.IsNullOrWhiteSpace(respJson))
        {
            Debug.LogError("账号登录 POST 返回为空");
            return;
        }

        string wsUrl;
        string token;
        try
        {
            // 取 Gateways[0].Host + Port，并拼接 /game 作为 websocket 地址
            var root = JObject.Parse(respJson);
            int errorCode = root.Value<int?>("ErrorCode") ?? -1;
            if (errorCode != 0)
            {
                Debug.LogError($"账号登录 POST 返回错误 ErrorCode={errorCode}，resp={respJson}");
                return;
            }

            string host = root.SelectToken("Payload.Role.Server.Gateways[0].Host")?.Value<string>();
            int port = root.SelectToken("Payload.Role.Server.Gateways[0].Port")?.Value<int?>() ?? 0;
            if (string.IsNullOrWhiteSpace(host) || port <= 0)
            {
                Debug.LogError($"账号登录 POST 未返回有效 Gateways[0].Host/Port，resp={respJson}");
                return;
            }

            token = root.SelectToken("Payload.Token")?.Value<string>();
            if (string.IsNullOrWhiteSpace(token))
            {
                Debug.LogError($"账号登录 POST 未返回有效 Token，resp={respJson}");
                return;
            }

            wsUrl = BuildWsGameUrl(host, port);
        }
        catch (Exception e)
        {
            Debug.LogError($"解析账号登录 POST 返回失败：{e}\nresp={respJson}");
            return;
        }

        Debug.Log($"账号登录成功，准备连接 WebSocket：{wsUrl}");

        var wss = NetWork_Center_WSS.Instance;
        if (wss == null)
        {
            Debug.LogError("NetWork_Center_WSS.Instance 为空：请确认场景里已创建网络中心对象");
            return;
        }

        // 关闭旧 WebSocket 时会触发 OnClose/OnError；在未登录阶段否则会误弹「服务器连接失败」
        // wss.SuppressConnectionFailurePromptForSeconds(5f);
        wss.CloseCurrentWsClientForReconnect();

        // 如果之前已经有连接，先关闭再切换地址重连（必须 await，否则 init_wss 会与 Close 并发）
        // try
        // {
        //     Debug.Log("[TapTapLogin] 主动断开 WSS：切换网关前 await close_wss()");
        //     await wss.close_wss();
        // }
        // catch (Exception e)
        // {
        //     Debug.LogWarning($"await close_wss() 时出现异常：{e}");
        // }

        // // 兼容 url 字段不是 public 的情况：用反射设置
        // var urlField = typeof(NetWork_Center_WSS).GetField("url", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        // if (urlField == null)
        // {
        //     Debug.LogError("未找到 NetWork_Center_WSS.url 字段，无法切换 WebSocket 地址");
        //     return;
        // }
        // urlField.SetValue(wss, wsUrl);
        wss.url = wsUrl;

        var loginMgr = Login_Manager.Instance;
        if (loginMgr == null)
        {
            Debug.LogError("Login_Manager.Instance 为空，无法排队 WebSocket 就绪后的 1003 鉴权");
            return;
        }

        // 须在 init_wss 之前排队：OnOpen 后主线程发 EvtNames.WS_Open → Login_Manager 发 1003（不经 TapTap 轮询）。
        string tryLoginPassword = !string.IsNullOrEmpty(password) ? password : defaultPasswordForTryLogin;
        loginMgr.QueueTryLoginAfterWebSocketOpen(loginUid, tryLoginPassword, token);
        wss.init_wss();

        // 仅用于超时提示与取消排队；真正发 1003 在 WS_Open → Login_Manager.TryLogin。
        const int timeoutMs = 15000;
        int elapsed = 0;
        while (!wss._is_connected && elapsed < timeoutMs)
        {
            await Task.Delay(250);
            elapsed += 250;
        }

        if (!wss._is_connected)
        {
            loginMgr.CancelPendingTryLoginAfterWebSocket();
            Debug.LogError($"WebSocket 连接超时（{timeoutMs}ms）：{wsUrl}；{wss.GetWsStateHintForLogs()}。若长期 Connecting，多为服务端未监听/防火墙或路径 /game 不匹配。");
            return;
        }
    }

    private static string BuildWsGameUrl(string host, int port)
    {
        // host 示例：ws://192.168.31.49
        // 目标：ws://192.168.31.49:7000/game
        string trimmed = host.Trim().TrimEnd('/');

        // 如果 host 已包含端口，优先保留（避免重复添加）
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            int finalPort = uri.IsDefaultPort ? port : uri.Port;
            string scheme = string.IsNullOrWhiteSpace(uri.Scheme) ? "ws" : uri.Scheme;
            return $"{scheme}://{uri.Host}:{finalPort}/game";
        }

        return $"{trimmed}:{port}/game";
    }

    public static async Task<string> PostJsonAsync(string url, string json)
    {
        using (var req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json ?? "");
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"账号登录 POST 失败：{req.error} url={url} resp={req.downloadHandler?.text}");
                return null;
            }

            return req.downloadHandler?.text;
        }
    }
    private void OnApplicationQuit()
    {
        if (loginPlatform == LoginPlatformType.TapTap)
            TapTapLogin.Instance.Logout();
    }

    protected override void OnDestroy()
    {
        EvtDsp.RemoveEvt(EvtNames.Resume_Silent_Relogin, SilentReloginAndTryLogin);
        base.OnDestroy();

        if (loginPlatform == LoginPlatformType.TapTap)
            TapTapLogin.Instance.Logout();
        _tapLoginSemaphore.Dispose();
    }
}
