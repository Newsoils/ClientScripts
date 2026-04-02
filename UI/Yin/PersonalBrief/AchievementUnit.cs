using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Achievement;
using CLIP.Project_Mouse.NewFrame.UI;

//using DG.DemiEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class AchievementUnit : MonoBehaviour
            {
                public achievement_design_info info;
                //public AchievementPanelInteractManager panel;
                public int displayedIndex;
                public Image icon;
                public TMP_Text achievementName;
                public TMP_Text achievementCondition;
                public GameObject redPoint;
                public GameObject changeAchievement;
                public GameObject empty;
                public GameObject achievement;
                public GameObject getAward;
                public bool isDisplayUnit;
                public bool isAllAchievementUnit;
                public bool isGetAchievement = false;
                public bool isGetAward = false;

                //public void InitAchievementUnit(AchievementPanelInteractManager panel, achievement_design_info designInfo, bool isDisplayUnit = false, bool isAllAchievementUnit = false)
                public void InitAchievementUnit(achievement_design_info designInfo, bool isDisplayUnit = false, bool isAllAchievementUnit = false)
                {
                    //this.panel = panel;
                    info = designInfo;
                    this.isDisplayUnit = isDisplayUnit;
                    this.isAllAchievementUnit = isAllAchievementUnit;

                    string achievementName = info != null ? info.achievement_name : null;
                    if (!string.IsNullOrEmpty(achievementName))
                    {
                        var obtainedList = Quest_And_Achievement_Manager.instance._obtained_achievement_list;
                        if (obtainedList != null && obtainedList.Exists(r => r.achievement_name == achievementName))
                        {
                            isGetAchievement = true;
                            isGetAward = true;
                        }

                        var pendingList = Quest_And_Achievement_Manager.instance.pending_finished_achievement_list;
                        if (pendingList != null && pendingList.Exists(r => r.achievement_name == achievementName))
                        {
                            isGetAchievement = true;
                        }
                    }

                    if (isAllAchievementUnit)
                    {
                        if (info != null)
                        {
                            // 加载icon
                        }
                        else
                        {
                            // 设置为空图标
                        }
                        return;
                    }

                    if (info == null)
                    {
                        empty.SetActive(true);
                        achievement.SetActive(false);
                        return;
                    }
                    achievement.SetActive(true);
                    empty.SetActive(false);

                    if (isDisplayUnit)
                    {
                        changeAchievement.SetActive(true);
                        getAward.SetActive(false);
                        redPoint.SetActive(false);
                    }
                    else
                    {
                        changeAchievement.SetActive(false);
                        if (!isGetAward && isGetAchievement)
                        {
                            getAward.SetActive(true);
                            redPoint.SetActive(true);
                        }
                        else
                        {
                            getAward.SetActive(false);
                            redPoint.SetActive(false);
                        }
                    }

                    if (info.achievement_rank == "神秘" && !isGetAchievement)
                    {
                        // 加载icon
                        this.achievementName.text = "？？？";
                        achievementCondition.text = "？？？";
                    }
                    else
                    {
                        // 加载icon
                        this.achievementName.text = achievementName;
                        achievementCondition.text = info.achievement_unlock_condition;
                    }
                }

                public void OpenAchievementDetail()
                {
                    //panel.OpenAchievementDetail(this);
                    UIManager.Instance.GetPanel<AchievementPanel>().OpenAchievementDetail(this);
                }

                public void ChangeDisplayedInfo()
                {
                    //panel.OpenChangeAchievement(this);
                    UIManager.Instance.GetPanel<AchievementPanel>().OpenChangeAchievement(this);
                }

                public void GetAwards()
                {
                    getAward.SetActive(false);
                    redPoint.SetActive(false);
                    //panel.GetAward(this);
                    UIManager.Instance.GetPanel<AchievementPanel>().GetAward(this);
                }
            }
        }
    }
}