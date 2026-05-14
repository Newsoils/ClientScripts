using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class LevelButton : MonoBehaviour
            {
                public TMP_Text level;
                private void Start()
                {
                    Refresh();
                    EvtDsp.AddEvt(EvtNames.On_Get_Exp, Refresh);
                }
                private void Refresh()
                {
                    level.text = "Lv." + ExpManager.Instance.curLevel;
                }
            }
        }
    }
}

