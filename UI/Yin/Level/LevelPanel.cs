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
            public class LevelPanel : MonoBehaviour
            {
                public static LevelPanel instance;
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
                private void Awake()
                {
                    instance = this;
                    btnReturn.onClick.AddListener(ClosePanel);
                    btnExit.onClick.AddListener(ClosePanel);
                    EvtDsp.AddEvt(EvtNames.On_Get_Exp, Refresh);
                }

                private void OnDestroy()
                {
                    EvtDsp.RemoveEvt(EvtNames.On_Get_Exp, Refresh);
                }

                public void Refresh()
                {
                    curLevel.text = ExpManager.instance.curLevel.ToString();
                    nextLevel.text = (ExpManager.instance.curLevel + 1).ToString();
                    if(ExpManager.instance.curLevelInfo.nextLevelExp > 0)
                    {
                        curExp.fillAmount = (float)ExpManager.instance.curExp / ExpManager.instance.curLevelInfo.nextLevelExp;
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
                    if(ExpManager.instance.curLevelInfo.rewardItem.Count < ExpManager.instance.curLevelInfo.thresholdNum)
                    {
                        return;
                    }
                    for(int i = 0; i < ExpManager.instance.curLevelInfo.thresholdNum + 1; i++)
                    {
                        if(i != ExpManager.instance.curLevelInfo.thresholdNum)
                        {
                            GameObject obj = Instantiate(thresholdLine, thresholdLineFrame);
                            ThresholdLine line = obj.GetComponent<ThresholdLine>();
                            line.Init(ExpManager.instance.curLevelInfo.rewardItem[i].items);
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
                    if (ExpManager.instance.curLevelInfo.rewardItem.Count < ExpManager.instance.curLevelInfo.thresholdNum)
                    {
                        return;
                    }
                    foreach (var item in ExpManager.instance.curLevelInfo.rewardItem[^1].items)
                    {
                        GameObject obj = Instantiate(rewardItemCell, rewardItemFrame);
                        obj.GetComponent<RewardItemCell>().Init(item.Item1, item.Item2);
                    }
                }

                public void OpenPanel()
                {
                    InputManager.Instance.AllowTouchOnUI = true;
                    panelObj.SetActive(true);
                    Refresh();
                    EvtDsp.TriggerEvt(EvtNames.OnLevelPanelOpen);
                }
                public void ClosePanel()
                {
                    InputManager.Instance.AllowTouchOnUI = false;
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

