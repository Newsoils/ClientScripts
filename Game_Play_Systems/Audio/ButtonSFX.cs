using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSFX : MonoBehaviour
{
    public List<ButtonConfig> buttons = new List<ButtonConfig>();
    public string defaultAudio;
    private void Start()
    {
        foreach(var btn in buttons)
        {
            btn.button.onClick.AddListener(() => PlaySFX(btn.onClick));
        }
    }
    public void PlaySFX(string refKey)
    {
        AudioManager.Instance.PlayAudioByRefKey(refKey);
    }
    public void GetButtons()
    {
        buttons.Clear();
        List<Button> b = GetComponentsInChildren<Button>(true).ToList();
        foreach(var btn in b)
        {
            ButtonConfig info = new ButtonConfig();
            info.button = btn;
            info.onClick = defaultAudio;
            buttons.Add(info);
        }
    }
}

[Serializable]
public class ButtonConfig
{
    public Button button;
    public string onClick;
}