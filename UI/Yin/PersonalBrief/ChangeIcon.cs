using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ChangeIcon : MonoBehaviour
            {
                //public PersonalBriefCanvasInteractManager manager;
                public string currentstate = "头像";
                public PersonalIconUnit selectedIcon;

                [Header("UI")]
                public Image iconBg;
                public Image iconFrameBg;
                public Color selectedColor;
                public Color deselectedColor;
                public Transform iconRoot;
                public GameObject personalIconUnitPrefab;

                private void OnEnable()
                {
                    ChangeState("头像");
                    if (selectedIcon != null)
                    {
                        selectedIcon.DeselectUnit();
                        selectedIcon = null;
                    }
                }

                // 确认
                public void Confirm()
                {
                    if (currentstate == "头像")
                    {
                        //manager.icon.mSprite = selectedIcon.icon.mSprite;
                        UIManager.Instance.GetPanel<PersonalBriefPanel>().icon.sprite = selectedIcon.icon.sprite;
                        //manager.currentIcon = selectedIcon.avatar_Icon_Info;
                        UIManager.Instance.GetPanel<PersonalBriefPanel>().currentIcon = selectedIcon.avatar_Icon_Info;
                    }
                    else if (currentstate == "边框")
                    {
                        //manager.iconFrame.mSprite = selectedIcon.icon.mSprite;
                        UIManager.Instance.GetPanel<PersonalBriefPanel>().iconFrame.sprite = selectedIcon.icon.sprite;
                        //manager.currentIconFrame = selectedIcon.avatar_Icon_Info;
                        UIManager.Instance.GetPanel<PersonalBriefPanel>().currentIconFrame = selectedIcon.avatar_Icon_Info;
                    }
                }

                // 选择头像或边框
                public void SelectIcon(PersonalIconUnit personalIconUnit)
                {
                    if (selectedIcon != null)
                    {
                        selectedIcon.DeselectUnit();
                    }

                    selectedIcon = personalIconUnit;
                }

                // 选择修改头像或边框
                public void ChangeState(string state)
                {
                    currentstate = state;
                    if (currentstate == "头像")
                    {
                        iconBg.color = selectedColor;
                        iconFrameBg.color = deselectedColor;
                        InitIconItemView();
                    }
                    else if (currentstate == "边框")
                    {
                        iconBg.color = deselectedColor;
                        iconFrameBg.color = selectedColor;
                        InitIconFrameItemView();
                    }
                }

                // 初始化头像列表
                public void InitIconItemView()
                {
                    int needCount = Global_Game_Manager.Instance._avatar_icon_list.Count;
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
                            itemObj = Instantiate(personalIconUnitPrefab, iconRoot);
                        }
                        itemObj.SetActive(true);
                        PersonalIconUnit item = itemObj.GetComponent<PersonalIconUnit>();
                        item.InitUnit(this, Global_Game_Manager.Instance._avatar_icon_list[i]);
                    }

                    for (int i = needCount; i < childCount; i++)
                    {
                        iconRoot.GetChild(i).gameObject.SetActive(false);
                    }
                }

                // 初始化边框列表
                public void InitIconFrameItemView()
                {
                    int needCount = Global_Game_Manager.Instance._avatar_icon_frame_list.Count;
                    int childCount = iconRoot.childCount;

                    for (int i = 0; i < needCount; i++)
                    {
                        GameObject itemObj;
                        if (i < needCount)
                        {
                            itemObj = iconRoot.GetChild(i).gameObject;
                        }
                        else
                        {
                            itemObj = Instantiate(personalIconUnitPrefab, iconRoot);
                        }
                        itemObj.SetActive(true);
                        PersonalIconUnit item = itemObj.GetComponent<PersonalIconUnit>();
                        item.InitUnit(this, Global_Game_Manager.Instance._avatar_icon_frame_list[i]);
                    }

                    for (int i = needCount; i < childCount; i++)
                    {
                        iconRoot.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}