using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.UI;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ThresholdLine : MonoBehaviour
            {
                public Button btnShowReward;
                public GameObject rewardPanel;
                public GameObject rewardDecoration;
                public GameObject rewardItemCell;
                bool isRewardPanelOpen;
                private void Start()
                {
                    EvtDsp.AddEvt(EvtNames.OnClickNothing, CloseRewardPanel);
                }
                private void OnDestroy()
                {
                    EvtDsp.RemoveEvt(EvtNames.OnClickNothing, CloseRewardPanel);
                }
                public void Init(List<(string, int)> items)
                {
                    isRewardPanelOpen = false;
                    btnShowReward.onClick.AddListener(BtnShowReward);
                    foreach(Transform child in rewardPanel.transform)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach(var item in items)
                    {
                        GameObject obj = Instantiate(rewardItemCell, rewardPanel.transform);
                        obj.GetComponent<RewardItemCell>().Init(item.Item1,item.Item2);
                    }
                }
                public void OpenRewardPanel()
                {
                    isRewardPanelOpen = true;
                    rewardPanel.SetActive(true);
                    rewardDecoration.SetActive(true);
                }
                public void CloseRewardPanel()
                {
                    isRewardPanelOpen = false;
                    rewardPanel.SetActive(false);
                    rewardDecoration.SetActive(false);
                }
                private void BtnShowReward()
                {
                    UIManager.Instance.GetPanel<LevelPanel>().CloseOtherLine(this);
                    if (isRewardPanelOpen)
                    {
                        CloseRewardPanel();
                    }
                    else
                    {
                        OpenRewardPanel();
                    }
                }
            }
        }
    }
}
