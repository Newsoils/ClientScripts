using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour
{
    public TMP_InputField input_player_name;
    public TMP_InputField input_player_ps;
    public Button btn_login_in;
    public Button btn_taptap_login;
    public TMP_Text output_text;


    private void Start()
    {
        if (btn_login_in != null)
            btn_login_in.onClick.AddListener(TryLogin);
        btn_taptap_login.onClick.AddListener(TaptapLogin);
        EvtDsp.AddEvt<string>(EvtNames.Login_Messsage, Show_Login_Text);
    }

    public void OnDestroy()
    {
        EvtDsp.RemoveEvt<string>(EvtNames.Login_Messsage, Show_Login_Text);
    }

    public void TryLogin()
    {
        string playerId = input_player_name?.text;
        string password = input_player_ps?.text;
     
        Login_Manager.Instance.TryLogin(playerId, password);

    }

    public void Show_Login_Text(string text)
    {
        if (output_text != null)
            output_text.text = text;
    }
    private void TaptapLogin()
    {
        TapTapLoginManager.Instance.Login();
    }
}
