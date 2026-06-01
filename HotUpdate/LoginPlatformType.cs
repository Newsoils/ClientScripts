/// <summary>登录平台：Inspector 配置后由 <see cref="TapTapLoginManager"/> / <see cref="UI_Login"/> 使用。</summary>
public enum LoginPlatformType
{
    /// <summary>自建账号（HTTP cmd=3，LoginType=0）。</summary>
    SelfAccount = 0,

    /// <summary>TapTap SDK 登录（LoginType=2，走防沉迷）。</summary>
    TapTap = 1,
}
