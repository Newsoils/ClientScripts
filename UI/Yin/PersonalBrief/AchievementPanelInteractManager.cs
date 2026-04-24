using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Achievement;
using CLIP.Project_Mouse.Kernel.Inventory;
using CLIP.Project_Mouse.NewFrame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using static UnityEditor.Progress;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class AchievementPanelInteractManager : MonoBehaviour
            {
                public GameItem_DB_SO gameInventorySO;
                public Achievement_Design_Info_DB_SO achievementSO;
                //public PersonalBriefCanvasInteractManager personalBriefCanvasInteractManager;

                [Header("Achievement Panel")]
                public List<achievement_design_info> displayedAchievementInfo = new List<achievement_design_info>();
                public List<AchievementUnit> displayedAchievementUnits = new List<AchievementUnit>();

                public List<AchievementUnit> allAchievementsUnits = new List<AchievementUnit>();
                public TMP_Text moreAchievementsText;
                public GameObject moreAchievementsRedPoint;

                [Header("Achievement Detail")]
                public GameObject achievementDetailPanel;
                public Image detailIcon;
                public TMP_Text detailNameText;
                public TMP_Text detailConditionText;
                public Transform detailAwardsRoot;
                public GameObject achievementAwardUnitPrefab;

                [Header("Change Achievement")]
                public GameObject changeAchievementPanel;
                public AchievementUnit achievementToChange;
                public AchievementIconUnit selectedIcon;
                public Transform iconRoot;
                public GameObject achievementIconUnitPrefab;

                [Header("AllAchievements")]
                public GameObject allAchievementsPanel;
                public Transform allAchievementRoot;
                public GameObject achievementUnitPrefab;

                private void OnEnable()
                {
                    InitAchievementPanel();
                }

                // 一键领取
                public void GetAllAwards()
                {
                    int childCount = allAchievementRoot.childCount;
                    for (int i = 0; i < childCount; i++)
                    {
                        AchievementUnit unit = allAchievementRoot.GetChild(i).GetComponent<AchievementUnit>();
                        if (unit != null)
                        {
                            if (unit.isGetAchievement && !unit.isGetAward)
                            {
                                GetAward(unit);
                            }
                        }
                    }
                }

                public void GetAward(AchievementUnit achievementUnit)
                {
                    foreach (var award in achievementUnit.info.item_reward)
                    {
                        var invItem = Global_Inventory_Manager.GameItem_DB.Find(i => i.name == award.item_name);
                        Global_Inventory_Manager.Change_Item_Count(award.item_name, award.item_count);
                    }
                    Quest_And_Achievement_Manager.instance.on_get_achievement_reward(achievementUnit.info.achievement_name);
                }

                // 打开全部成就界面
                public void OpenAllAchievementsPanel()
                {
                    int needCount = achievementSO.achievement_db.Count;
                    int childCount = allAchievementRoot.childCount;

                    for (int i = 0; i < needCount; i++)
                    {
                        GameObject itemObj;
                        if (i < childCount)
                        {
                            itemObj = allAchievementRoot.GetChild(i).gameObject;
                        }
                        else
                        {
                            itemObj = Instantiate(achievementUnitPrefab, allAchievementRoot);
                        }
                        itemObj.SetActive(true);
                        AchievementUnit item = itemObj.GetComponent<AchievementUnit>();
                        //item.InitAchievementUnit(this, achievementSO.achievement_db[i]);
                    }

                    for (int i = needCount; i < childCount; i++)
                    {
                        iconRoot.GetChild(i).gameObject.SetActive(false);
                    }

                    allAchievementsPanel.SetActive(true);
                }

                // 确认选择成就
                public void ConfirmChangeAchievement()
                {
                    //achievementToChange.InitAchievementUnit(this, selectedIcon.info, true);
                    displayedAchievementInfo[achievementToChange.displayedIndex] = selectedIcon.info;

                    var obtainedList = Quest_And_Achievement_Manager.instance._obtained_achievement_list;
                    var record = obtainedList.Find(r => r.achievement_name == selectedIcon.info.achievement_name);
                    if (record != null)
                    {
                        //personalBriefCanvasInteractManager.achievementRecordList[achievementToChange.displayedIndex] = record;
                        UIManager.Instance.GetPanel<PersonalBriefPanel>().achievementRecordList[achievementToChange.displayedIndex] = record;
                    }
                    else
                    {
                        var pendingList = Quest_And_Achievement_Manager.instance.pending_finished_achievement_list;
                        record = pendingList.Find(r => r.achievement_name == selectedIcon.info.achievement_name);
                        if (record != null)
                        {
                            //personalBriefCanvasInteractManager.achievementRecordList[achievementToChange.displayedIndex] = record;
                            UIManager.Instance.GetPanel<PersonalBriefPanel>().achievementRecordList[achievementToChange.displayedIndex] = record;
                        }
                    }

                    changeAchievementPanel.SetActive(false);
                }

                // 选择成就
                public void SelectAchievement(AchievementIconUnit achievementIconUnit)
                {
                    if (selectedIcon != null)
                    {
                        selectedIcon.DeselectUnit();
                    }

                    selectedIcon = achievementIconUnit;
                }

                // 打开选择成就界面
                public void OpenChangeAchievement(AchievementUnit achievementUnit)
                {
                    changeAchievementPanel.SetActive(true);
                    achievementToChange = achievementUnit;
                    selectedIcon = null;

                    var obtainedList = Quest_And_Achievement_Manager.instance._obtained_achievement_list;
                    var pendingList = Quest_And_Achievement_Manager.instance.pending_finished_achievement_list;

                    List<achievement_design_info> selectableAchievements = new List<achievement_design_info>();
                    if (obtainedList != null)
                    {
                        foreach (var record in obtainedList)
                        {
                            var info = achievementSO.achievement_db.Find(x => x.achievement_name == record.achievement_name);
                            if (info != null)
                                selectableAchievements.Add(info);
                        }
                    }
                    if (pendingList != null)
                    {
                        foreach (var record in pendingList)
                        {
                            var info = achievementSO.achievement_db.Find(x => x.achievement_name == record.achievement_name);
                            if (info != null)
                                selectableAchievements.Add(info);
                        }
                    }

                    int needCount = selectableAchievements.Count;
                    int childCount = iconRoot.childCount;

                    for (int i = 0; i < needCount; i++)
                    {
                        GameObject itemObj;
                        if (i < childCount)
                        {
                            itemObj = iconRoot.GetChild(i).gameObject;
                        }
                        else
                        {
                            itemObj = Instantiate(achievementAwardUnitPrefab, iconRoot);
                        }
                        itemObj.SetActive(true);
                        AchievementIconUnit item = itemObj.GetComponent<AchievementIconUnit>();
                        var info = achievementSO.achievement_db.Find(r => r.achievement_name == selectableAchievements[i].achievement_name);
                        //item.InitUnit(this, info);
                    }

                    for (int i = needCount; i < childCount; i++)
                    {
                        iconRoot.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // 打开成就详情界面
                public void OpenAchievementDetail(AchievementUnit achievementUnit)
                {
                    if (achievementUnit.info.achievement_rank == "神秘" && !achievementUnit.isGetAchievement)
                    {
                        InitAchievementDetail(achievementUnit, true);
                    }
                    else
                    {
                        InitAchievementDetail(achievementUnit);
                    }

                    achievementDetailPanel.SetActive(true);
                }

                // 初始化成就详情界面
                public void InitAchievementDetail(AchievementUnit achievementUnit, bool isMystery = false)
                {
                    achievement_design_info info = achievementUnit.info;

                    if (isMystery)
                    {
                        // 加载icon
                        detailNameText.text = "？？？";
                        detailConditionText.text = "？？？";
                    }
                    else
                    {
                        // 加载icon
                        detailNameText.text = info.achievement_name;
                        detailConditionText.text = info.achievement_unlock_condition;
                    }

                    List<shop_item> allAwards = new List<shop_item>();
                    if (info.item_reward != null && info.item_reward.Count > 0)
                    {
                        allAwards.AddRange(info.item_reward);
                    }

                    int awardCount = 0;
                    int childCount = detailAwardsRoot.childCount;
                    foreach (var item in allAwards)
                    {
                        GameObject awardObj;
                        if (awardCount < childCount)
                        {
                            awardObj = detailAwardsRoot.GetChild(awardCount).gameObject;
                            awardObj.SetActive(true);
                        }
                        else
                        {
                            awardObj = Instantiate(achievementAwardUnitPrefab, detailAwardsRoot);
                        }

                        var awardUnit = awardObj.GetComponent<AchievementAwardUnit>();
                        if (awardUnit != null)
                        {
                            var dbItem = Global_Inventory_Manager.GameItem_DB.Find(db => db.name == item.item_name);
                            string iconPath = dbItem != null ? dbItem.res_url : "";
                            awardUnit.InitAwardUnit(iconPath, item.item_count, isMystery);
                        }
                        awardCount++;
                    }

                    for (int i = awardCount; i < childCount; i++)
                    {
                        detailAwardsRoot.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // 初始化成就界面
                public void InitAchievementPanel()
                {
                    // 已展示的成就
                    displayedAchievementInfo.Clear();
                    var pinnedList = Global_Game_Manager.Instance._player_brief.achievements_pinned;
                    if (pinnedList != null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (i < pinnedList.Count)
                                displayedAchievementInfo.Add(achievementSO.achievement_db.Find(x => x.achievement_name == pinnedList[i].achievement_name));
                            else
                                displayedAchievementInfo.Add(null);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                            displayedAchievementInfo.Add(null);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        //displayedAchievementUnits[i].InitAchievementUnit(this, displayedAchievementInfo[i], true);
                    }

                    // 全部成就
                    var obtainedList = Quest_And_Achievement_Manager.instance._obtained_achievement_list;
                    var pendingList = Quest_And_Achievement_Manager.instance.pending_finished_achievement_list;
                    List<achievement_design_info> allAchievementInfo = new List<achievement_design_info>();
                    HashSet<string> usedNames = new HashSet<string>();
                    if (obtainedList != null)
                    {
                        foreach (var record in obtainedList)
                        {
                            if (allAchievementInfo.Count < 7)
                            {
                                var info = achievementSO.achievement_db.Find(x => x.achievement_name == record.achievement_name);
                                if (info != null)
                                {
                                    allAchievementInfo.Add(info);
                                    usedNames.Add(info.achievement_name);
                                }
                            }
                        }
                    }

                    if (pendingList != null)
                    {
                        foreach (var record in pendingList)
                        {
                            if (allAchievementInfo.Count < 7)
                            {
                                var info = achievementSO.achievement_db.Find(x => x.achievement_name == record.achievement_name);
                                if (info != null)
                                {
                                    allAchievementInfo.Add(info);
                                    usedNames.Add(info.achievement_name);
                                }
                            }
                        }
                    }

                    if (allAchievementInfo.Count < 7)
                    {
                        foreach (var info in achievementSO.achievement_db)
                        {
                            if (allAchievementInfo.Count >= 7) break;
                            if (!usedNames.Contains(info.achievement_name))
                            {
                                allAchievementInfo.Add(info);
                                usedNames.Add(info.achievement_name);
                            }
                        }
                    }

                    while (allAchievementInfo.Count < 7)
                    {
                        allAchievementInfo.Add(null);
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        //allAchievementsUnits[i].InitAchievementUnit(this, allAchievementInfo[i], false, true);
                    }

                    int moreCount = Mathf.Max(0, achievementSO.achievement_db.Count - 7);
                    moreAchievementsText.text = "+" + moreCount.ToString();

                    bool showRedPoint = false;

                    foreach (var record in pendingList)
                    {
                        if (!usedNames.Contains(record.achievement_name))
                        {
                            showRedPoint = true;
                            break;
                        }
                    }

                    moreAchievementsRedPoint.SetActive(showRedPoint);
                }

                // 关闭成就界面
                public void CloseAchievementPanel()
                {
                    //StartCoroutine(personalBriefCanvasInteractManager.LoadAchievementIconFromAchievementList());
                    StartCoroutine(UIManager.Instance.GetPanel<PersonalBriefPanel>().LoadAchievementIconFromAchievementList());
                    this.gameObject.SetActive(false);
                }
            }
        }
    }
}