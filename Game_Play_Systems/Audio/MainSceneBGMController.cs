using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class MainSceneBGMController : MonoBehaviour
{
    private void Start()
    {
        EvtDsp.AddEvt(EvtNames.On_Set_Time, PlaySong);
        PlaySong();
    }
    private void OnDestroy()
    {
        EvtDsp.RemoveEvt(EvtNames.On_Set_Time, PlaySong);
    }
    public void PlaySong()
    {
        if(TimeManager.Instance.isDay)
        {
            AudioManager.Instance.PlayAduioByResKey(ResKeys.WAV_DAY, 1, 1);
        }
        else
        {
            AudioManager.Instance.PlayAduioByResKey(ResKeys.WAV_NIGHT, 1, 1);
        }
    }
}
