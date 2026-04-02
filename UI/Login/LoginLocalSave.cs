using UnityEngine;
using TMPro;

public class LoginLocalSave : MonoBehaviour
{
    [SerializeField] private TMP_InputField accountField;
    [SerializeField] private TMP_InputField passwordField;

    private const string KEY_ACCOUNT = "PlayerAccount";
    private const string KEY_PASSWORD = "PlayerPassword";

    private void Start()
    {
        // 自动填充上一次输入
        if (PlayerPrefs.HasKey(KEY_ACCOUNT))
            accountField.text = PlayerPrefs.GetString(KEY_ACCOUNT);

        if (PlayerPrefs.HasKey(KEY_PASSWORD))
            passwordField.text = PlayerPrefs.GetString(KEY_PASSWORD);
    }

    public void OnLoginButtonClick()
    {
        SaveInput();
        // 在这里写你的登录逻辑……
    }

    private void SaveInput()
    {
        PlayerPrefs.SetString(KEY_ACCOUNT, accountField.text);
        PlayerPrefs.SetString(KEY_PASSWORD, passwordField.text);

        PlayerPrefs.Save();
        Debug.Log("账号密码已保存到本地");
    }
}
