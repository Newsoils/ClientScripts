using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginBGMController : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.PlayAduioByResKey(ResKeys.WAV_LOGIN);
    }
}
