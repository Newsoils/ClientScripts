using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TapSDK.Compliance;
using TapSDK.Core;
using TapSDK.Login;
using UnityEngine;

public class TapTapLoginManager : SingletonMono<TapTapLoginManager>
{
    // Start is called before the first frame update
    private string unionId;
    private Action<int, string> callback => (code, s) => {
        Debug.Log(code);
        switch (code)
        {
            case 1001:
                _ = LoginAsync();
                break;
            case 500:
                Login_Manager.Instance.TryLogin(unionId, "123456");
                break;
        }
    };
    void Start()
    {
        // 核心配置 详细参数见 [TapTapSDK]
        TapTapSdkOptions coreOptions = new TapTapSdkOptions
        {
            clientId = "bzepc2nhpyue2sqkav",
            clientToken = "7168crDoO06GgIvgxg9CBcZ4CtbXJzhWfPb2mPeX",
        };
        TapTapComplianceOption complianceOption = new TapTapComplianceOption
        {
            showSwitchAccount = true,  // 是否显示切换账号按钮
            useAgeRange = true  // 游戏是否需要获取真实年龄段信息
        };
        // 其他模块配置项
        TapTapSdkBaseOptions[] otherOptions = new TapTapSdkBaseOptions[]
        {
            complianceOption
        };
        // TapSDK 初始化
        TapTapSDK.Init(coreOptions, otherOptions);
        TapTapCompliance.RegisterComplianceCallback(callback);
        TapTapLogin.Instance.Logout();
        DontDestroyOnLoad(this.gameObject);

        EvtDsp.AddEvt(EvtNames.Reconnect, Login);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        TapTapLogin.Instance.Logout();

        EvtDsp.RemoveEvt(EvtNames.Reconnect, Login);
    }

    public void Login()
    {
        Debug.Log("登录！");
        _ = LoginAsync();
    }
    private async Task LoginAsync()
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
                TapTapCompliance.Startup(userIdentifier);
                //Login_Manager.Instance.TryLogin(unionId, "123456");
            }
        }
        catch (TaskCanceledException)
        {
            Debug.Log("用户取消登录");
        }
        catch (Exception exception)
        {
            Debug.Log($"登录失败，出现异常：{exception}");
        }
    }
    private void OnApplicationQuit()
    {
        TapTapLogin.Instance.Logout();
    }
  
}
