using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class LevelPanel : UIPanelBase
            {
                public GameObject panelObj;

                public TMP_Text curLevel;
                public TMP_Text nextLevel;
                public Button btnReturn;
                public Button btnExit;
                public Image curExp;
                public Transform thresholdLineFrame;
                public Transform rewardItemFrame;

                public GameObject rewardItemCell;
                public GameObject thresholdLine;
                public List<ThresholdLine> lines = new List<ThresholdLine>();

                private void Start()
                {
                    btnReturn.onClick.AddListener(ClosePanel);
                    btnExit.onClick.AddListener(ClosePanel);
                    EvtDsp.AddEvt(EvtNames.On_Get_Exp, Refresh);
                }
                public override void OnDestroy()
                {
                    base.OnDestroy();
                    EvtDsp.RemoveEvt(EvtNames.On_Get_Exp, Refresh);
                }

                public void Refresh()
                {
                    curLevel.text = ExpManager.Instance.curLevel.ToString();
                    nextLevel.text = (ExpManager.Instance.curLevelInfo.nextLevelExp - ExpManager.Instance.curExp).ToString();
                    if (ExpManager.Instance.curLevelInfo.nextLevelExp > 0)
                    {
                        curExp.fillAmount = (float)ExpManager.Instance.curExp / ExpManager.Instance.curLevelInfo.nextLevelExp;
                    }
                    else
                    {
                        curExp.fillAmount = 0;
                    }
                    CreateThresholdLine();
                    CreateRewardItemCells();
                }
                private void CreateThresholdLine()
                {
                    foreach(Transform child in thresholdLineFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    lines.Clear();
                    if(ExpManager.Instance.curLevelInfo.rewardItem.Count < ExpManager.Instance.curLevelInfo.thresholdNum)
                    {
                        return;
                    }
                    for(int i = 0; i < ExpManager.Instance.curLevelInfo.thresholdNum + 1; i++)
                    {
                        if(i != ExpManager.Instance.curLevelInfo.thresholdNum)
                        {
                            GameObject obj = Instantiate(thresholdLine, thresholdLineFrame);
                            ThresholdLine line = obj.GetComponent<ThresholdLine>();
                            line.Init(ExpManager.Instance.curLevelInfo.rewardItem[i].items);
                            lines.Add(line);
                        }
                        else
                        {
                            GameObject obj = Instantiate(thresholdLine, thresholdLineFrame);
                            obj.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                        }
                    }
                }
                private void CreateRewardItemCells()
                {
                    foreach (Transform child in rewardItemFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    if (ExpManager.Instance.curLevelInfo.rewardItem.Count < ExpManager.Instance.curLevelInfo.thresholdNum)
                    {
                        return;
                    }
                    foreach (var item in ExpManager.Instance.curLevelInfo.rewardItem[^1].items)
                    {
                        GameObject obj = Instantiate(rewardItemCell, rewardItemFrame);
                        obj.GetComponent<RewardItemCell>().Init(item.Item1, item.Item2);
                    }
                }

                public override void OpenPanel(params object[] data)
                {
                    panelObj.SetActive(true);
                    Refresh();
                    EvtDsp.TriggerEvt(EvtNames.OnLevelPanelOpen);
                }
                public override void ClosePanel()
                {
                    panelObj.SetActive(false);
                    EvtDsp.TriggerEvt(EvtNames.OnLevelPanelClose);
                }
                public void CloseOtherLine(ThresholdLine line)
                {
                    foreach(var l in lines)
                    {
                        if(l != line)
                        {
                            l.CloseRewardPanel();
                        }
                    }
                }
            }
        }
    }
}

