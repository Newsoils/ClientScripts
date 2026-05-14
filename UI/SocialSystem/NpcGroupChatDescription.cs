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
            public class NpcGroupChatDescription : MonoBehaviour
            {
                public NpcGroupChatIcon selectedIcon;
                [Header("UI")]
                public GameObject iconRoot;
                public GameObject iconPrefab;
                public GameObject panel;
                public RawImage rawImage;
                public TMP_Text titleText;
                public TMP_Text nameText;
                public TMP_Text personality;
                public TMP_Text personalityText;
                public TMP_Text area;
                public TMP_Text areaText;
                public TMP_Text intimacyLevelText;
                public Slider slider;
                public GameObject intimacy;
                public TMP_Text intimacyText;
                public TMP_Text intimacyValueText;
                public GameObject reward;
                public void Init()
                {
                    panel.SetActive(false);

                    int childCount = iconRoot.transform.childCount;

                    GameObject iconObj;
                    if (0 < childCount)
                    {
                        iconObj = iconRoot.transform.GetChild(0).gameObject;
                        iconObj.SetActive(true);
                    }
                    else
                    {
                        iconObj = Instantiate(iconPrefab, iconRoot.transform);
                    }
                    var icon = iconObj.GetComponent<NpcGroupChatIcon>();
                    icon.InitNpcIcon(null, this);

                    int idx = 1;
                    foreach (var kv in CLIP.Project_Mouse.Game_Play_System.NPCManager.Instance.NPC_Info_Dict)
                    {
                        if (idx < childCount)
                        {
                            iconObj = iconRoot.transform.GetChild(idx).gameObject;
                            iconObj.SetActive(true);
                        }
                        else
                        {
                            iconObj = Instantiate(iconPrefab, iconRoot.transform);
                        }
                        icon = iconObj.GetComponent<NpcGroupChatIcon>();
                        icon.InitNpcIcon(kv.Value, this);
                        idx++;
                    }

                    for (int i = idx; i < childCount; i++)
                    {
                        iconRoot.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }

                public void ShowDescription(NpcGroupChatIcon npc)
                {
                    selectedIcon?._on_exit_selected();
                    selectedIcon = npc;
                    panel.SetActive(true);
                    reward.SetActive(false);
                    if (npc.info == null)
                    {
                        titleText.text = "小苔信息";
                        // 模型照片
                        nameText.text = $"主角名字：{Global_Game_Manager.Instance._current_player_name}";
                        personality.text = "id号码";
                        personalityText.text = Global_Game_Manager.Instance._current_player_id;
                        area.gameObject.SetActive(false);
                        areaText.gameObject.SetActive(false);
                        intimacy.SetActive(false);
                        intimacyText.gameObject.SetActive(true);
                        intimacyText.text = "Lv. 1";
                        // 好感度进度条
                    }
                    else
                    {
                        var info = npc.info;
                        titleText.text = "npc信息";
                        if (info._npc_RuntimeData.is_met)
                        {
                            // 模型照片
                            nameText.text = $"npc名字：{info._npc_Base.npc_name}";
                            personality.text = "npc性格：";
                            personalityText.text = info._npc_Base.npc_personality;
                            // npc所在地点
                            area.gameObject.SetActive(true);
                            areaText.gameObject.SetActive(true);
                            intimacy.SetActive(true);
                            intimacyText.gameObject.SetActive(false);
                            intimacyLevelText.text = info._npc_RuntimeData.favor_level.ToString();
                            intimacyValueText.text = $"{info._npc_RuntimeData.favor_Value}/{info.npc_Next_Favor_Level}";
                        }
                        else
                        {
                            // 模型照片
                            nameText.text = $"npc名字：？？？";
                            personality.text = "npc性格：";
                            personalityText.text = "？？？";
                            area.gameObject.SetActive(true);
                            areaText.gameObject.SetActive(true);
                            areaText.text = "？？？";
                            intimacy.SetActive(true);
                            intimacyText.gameObject.SetActive(false);
                            intimacyLevelText.text = "0";
                            intimacyValueText.text = "0/xxx";
                        }
                    }
                }

                public void RewardForNextLevel()
                {
                    if (selectedIcon.info != null && !selectedIcon.info._npc_RuntimeData.is_met) return;
                    if (reward.activeSelf)
                    {
                        reward.SetActive(false);
                    }
                    else
                    {
                        reward.SetActive(true);
                        // 显示奖励内容
                    }
                }
            }
        }
    }
}