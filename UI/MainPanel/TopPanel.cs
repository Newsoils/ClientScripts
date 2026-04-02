using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class TopPanel : MonoBehaviour
            {
                public Button levelButton;
                public Button coinButton;
                public Button diamondButton;
                public TMP_Text level;
                public TMP_Text coinNum;
                public TMP_Text diamondNum;
                private void Awake()
                {
                    EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshUI);
                    EvtDsp.AddEvt(EvtNames.OnGetCoin, RefreshUI);
                    EvtDsp.AddEvt(EvtNames.OnGetDiamond, RefreshUI);
                    EvtDsp.AddEvt(EvtNames.On_Get_Exp, RefreshUI);
                    levelButton.onClick.AddListener(BtnLevel);
                    coinButton.onClick.AddListener(BtnCoin);
                    diamondButton.onClick.AddListener(BtnDiamond);
                }
                private void OnDestroy()
                {
                    EvtDsp.RemoveEvt(EvtNames.RefreshUI,RefreshUI);
                    EvtDsp.RemoveEvt(EvtNames.OnGetCoin, RefreshUI);
                    EvtDsp.RemoveEvt(EvtNames.OnGetDiamond, RefreshUI);
                    EvtDsp.RemoveEvt(EvtNames.On_Get_Exp, RefreshUI);
                }
                private void Start()
                {
                    RefreshUI();
                }
                private void RefreshUI()
                {
                    level.text = ExpManager.instance.curLevel.ToString();
                    coinNum.text = MoneyManager.Instance.curCoin.ToString();
                    diamondNum.text = MoneyManager.Instance.curDiamond.ToString();
                }
                private void BtnLevel()
                {
                    LevelPanel.instance.OpenPanel();
                }
                private void BtnCoin()
                {
                    PayPanel.Instance.OpenPanel();
                }
                private void BtnDiamond()
                {
                    PayPanel.Instance.OpenPanel();
                }
            }
        }
    }
}
