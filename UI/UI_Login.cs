using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 登录界面：按 <see cref="TapTapLoginManager.LoginPlatform"/> 显示 TapTap 或自建账号 UI。
/// </summary>
public class UI_Login : UIBase
{
    [Header("TapTap")]
    public Button btn_taptap_login;

    [Header("Legacy (可选)")]
    public TMP_InputField input_player_name;
    public TMP_InputField input_player_ps;
    public Button btn_login_in;

    [Header("Self Account (可选，未绑定时运行时生成)")]
    public GameObject selfAccountRoot;
    public TMP_Text label_account;
    public TMP_InputField input_self_account;
    public Button btn_self_account_login;

    public Button pg8Button;

    public GameObject agePanel;

    public TMP_Text output_text;

    private GameObject _runtimeSelfAccountRoot;

    private void Start()
    {
        var tapMgr = TapTapLoginManager.Instance;
        if (tapMgr == null)
        {
            Debug.LogError("[UI_Login] TapTapLoginManager.Instance 为空");
            return;
        }

        ApplyLoginPlatformUI(tapMgr.LoginPlatform);

        EvtDsp.AddEvt<string>(EvtNames.Login_Messsage, Show_Login_Text);

        pg8Button?.onClick.AddListener(OpenAgePanel);
    }

    public void OnDestroy()
    {
        EvtDsp.RemoveEvt<string>(EvtNames.Login_Messsage, Show_Login_Text);

        if (btn_taptap_login != null)
            btn_taptap_login.onClick.RemoveAllListeners();
        if (btn_self_account_login != null)
            btn_self_account_login.onClick.RemoveAllListeners();
        if (btn_login_in != null)
            btn_login_in.onClick.RemoveAllListeners();

        pg8Button?.onClick.RemoveAllListeners();
    }

    public void OpenAgePanel()
    {
        agePanel.SetActive(true);
        agePanel.transform.SetAsLastSibling();
    }


    void ApplyLoginPlatformUI(LoginPlatformType platform)
    {
        bool useTapTap = platform == LoginPlatformType.TapTap;

        if (btn_taptap_login != null)
        {
            btn_taptap_login.gameObject.SetActive(useTapTap);
            btn_taptap_login.onClick.RemoveAllListeners();
            if (useTapTap)
                btn_taptap_login.onClick.AddListener(TaptapLogin);
        }

        HideLegacyDevLoginFields();

        if (useTapTap)
        {
            SetSelfAccountUiActive(false);
            return;
        }

        EnsureSelfAccountUi();
        SetSelfAccountUiActive(true);
    }

    void HideLegacyDevLoginFields()
    {
        if (input_player_name != null && input_player_name != input_self_account)
            input_player_name.gameObject.SetActive(false);
        if (input_player_ps != null)
            input_player_ps.gameObject.SetActive(false);
        if (btn_login_in != null)
            btn_login_in.gameObject.SetActive(false);
    }

    void EnsureSelfAccountUi()
    {
        if (selfAccountRoot != null && input_self_account != null && btn_self_account_login != null)
        {
            WireSelfAccountControls();
            return;
        }

        if (_runtimeSelfAccountRoot != null)
        {
            WireSelfAccountControls();
            return;
        }

        Transform parent = btn_taptap_login != null
            ? btn_taptap_login.transform.parent
            : transform;

        _runtimeSelfAccountRoot = new GameObject("Panel_SelfAccountLogin", typeof(RectTransform));
        _runtimeSelfAccountRoot.transform.SetParent(parent, false);

        var rootRt = _runtimeSelfAccountRoot.GetComponent<RectTransform>();
        if (btn_taptap_login != null)
        {
            var tapRt = btn_taptap_login.GetComponent<RectTransform>();
            rootRt.anchorMin = tapRt.anchorMin;
            rootRt.anchorMax = tapRt.anchorMax;
            rootRt.pivot = tapRt.pivot;
            rootRt.anchoredPosition = tapRt.anchoredPosition;
            rootRt.sizeDelta = new Vector2(tapRt.sizeDelta.x, 160f);
        }
        else
        {
            rootRt.sizeDelta = new Vector2(420f, 160f);
        }

        var vlg = _runtimeSelfAccountRoot.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 12f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        label_account = CreateLabelRow(_runtimeSelfAccountRoot.transform, "Account:");
        input_self_account = CreateInputField(_runtimeSelfAccountRoot.transform);
        btn_self_account_login = CreateLoginButton(_runtimeSelfAccountRoot.transform, "Login");

        selfAccountRoot = _runtimeSelfAccountRoot;
        WireSelfAccountControls();
    }

    void WireSelfAccountControls()
    {
        if (btn_self_account_login == null)
            return;

        btn_self_account_login.onClick.RemoveAllListeners();
        btn_self_account_login.onClick.AddListener(SelfAccountLogin);

        if (input_self_account != null)
        {
            var placeholder = input_self_account.placeholder as TextMeshProUGUI;
            if (placeholder != null)
                placeholder.text = "input your account name";
        }
    }

    void SetSelfAccountUiActive(bool active)
    {
        if (selfAccountRoot != null)
            selfAccountRoot.SetActive(active);
        else if (_runtimeSelfAccountRoot != null)
            _runtimeSelfAccountRoot.SetActive(active);
    }

    void SelfAccountLogin()
    {
        var tapMgr = TapTapLoginManager.Instance;
        if (tapMgr == null)
            return;

        string account = input_self_account != null
            ? input_self_account.text
            : input_player_name?.text;

        if (!TapTapLoginManager.ValidateSelfAccountName(account, out string error))
        {
            EvtDsp.TriggerEvt(EvtNames.ShowUpPrompt, error);
            return;
        }

        tapMgr.LoginSelfAccount(account);
    }

    void TaptapLogin()
    {
        TapTapLoginManager.Instance?.Login();
    }

    public void Show_Login_Text(string text)
    {
        if (output_text != null)
            output_text.text = text;
    }

    void ShowPrompt(string text)
    {
        if (output_text != null)
            output_text.text = text;
        EvtDsp.TriggerEvt(EvtNames.ShowUpPrompt, text);
    }

    static TMP_Text CreateLabelRow(Transform parent, string text)
    {
        var go = new GameObject("Label_Account", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 32f;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28f;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        return tmp;
    }

    static TMP_InputField CreateInputField(Transform parent)
    {
        var root = new GameObject("Input_Account", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var rootLe = root.AddComponent<LayoutElement>();
        rootLe.preferredHeight = 56f;

        var bg = root.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.92f);

        var textArea = new GameObject("Text Area", typeof(RectTransform));
        textArea.transform.SetParent(root.transform, false);
        var textAreaRt = textArea.GetComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.offsetMin = new Vector2(12f, 6f);
        textAreaRt.offsetMax = new Vector2(-12f, -6f);

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(textArea.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = 26f;
        text.color = Color.black;

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGo.transform.SetParent(textArea.transform, false);
        var phRt = placeholderGo.GetComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = Vector2.zero;
        phRt.offsetMax = Vector2.zero;
        var placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
        placeholder.fontSize = 24f;
        placeholder.color = new Color(0.4f, 0.4f, 0.4f);
        placeholder.text = "至少3位数字或英文字母";

        var input = root.AddComponent<TMP_InputField>();
        input.textViewport = textAreaRt;
        input.textComponent = text;
        input.placeholder = placeholder;
        return input;
    }

    static Button CreateLoginButton(Transform parent, string label)
    {
        var go = new GameObject("Button_SelfAccountLogin", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 56f;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.55f, 0.95f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var rt = textGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btn;
    }
}
