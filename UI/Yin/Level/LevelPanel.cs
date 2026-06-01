using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using Cmd;
using Google.Protobuf;
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

                private TMP_Text _btnReturnText;
                private bool _isClaimState;

                private const int GetNextLevelRewardReqMsgId = 1329;

                private void Start()
                {
                    panelObj.SetActive(false);
                   _btnReturnText = btnReturn != null ? btnReturn.GetComponentInChildren<TMP_Text>(true) : null;

                    btnReturn.onClick.RemoveAllListeners();
                    btnReturn.onClick.AddListener(OnReturnButtonClicked);
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
                    curLevel.text = ExpManager.instance.curLevel.ToString();
                    // 从 levels 配置中取下一级所需经验；若 curLevel+1 无配置则默认 0
                    int nextLevelExpVal = 0;
                    if (ExpManager.instance.levels != null &&
                        ExpManager.instance.levels.TryGetValue(ExpManager.instance.curLevel + 1, out var nextInfo) &&
                        nextInfo != null)
                    {
                        nextLevelExpVal = nextInfo.nextLevelExp;
                    }
                    int remain = nextLevelExpVal > 0 ? Mathf.Max(0, nextLevelExpVal - ExpManager.instance.curExp) : 0;
                    nextLevel.text = remain.ToString();
                    if (ExpManager.instance.curLevelInfo.nextLevelExp > 0)
                    {
                        curExp.fillAmount = (float)ExpManager.instance.curExp / ExpManager.instance.curLevelInfo.nextLevelExp;
                    }
                    else
                    {
                        curExp.fillAmount = 0;
                    }
                    CreateThresholdLine();
                    CreateRewardItemCells();
                    UpdateReturnButtonState();
                }

                private void UpdateReturnButtonState()
                {
                    if (btnReturn != null) btnReturn.interactable = true;

                    string tip;
                    _isClaimState = CanClaimNextSegmentReward(out tip);
                    if (_btnReturnText != null)
                        _btnReturnText.text = _isClaimState ? "领取" : "返回";

                    if (!string.IsNullOrEmpty(tip))
                    {
                        // 若 tip 不为空，直接显示在 nextLevel 文本上（Prefab 预留）
                        nextLevel.text = tip;
                    }
                }

                /// <summary>
                /// 判断下一档等级奖励是否可领取
                /// 依据 roleInfo.ClientRecords 中的 NextLevelRewardLevel / NextLevelRewardSegment 定位目标等级与档位数
                /// </summary>
                private bool CanClaimNextSegmentReward(out string tip)
                {
                    tip = null;
                    if (ExpManager.instance == null || ExpManager.instance.levels == null) return false;

                    if (!TryResolveNextRewardTarget(out int rewardLevel, out int rewardSegment, out var info, out tip))
                        return false; // tip 已带出外部提示

                    // 旧逻辑：若 thresholdNum 或 nextLevelExp 为 0 则直接不可领，已注释
                    // 现在允许 nextLevelExp 或 thresholdNum 为 0 时继续计算
                    float needExp = 0;
                    if (info.thresholdNum > 0) {
                        float perSegment = (float)info.nextLevelExp / info.thresholdNum;
                        needExp = perSegment * (rewardSegment + 1);
                    }

                    int curLv = ExpManager.instance.curLevel;
                    int curExp = ExpManager.instance.curExp;

                    if (curLv > rewardLevel) return true;
                    if (curLv < rewardLevel) return false;
                    return curExp >= needExp;
                }

                private void OnReturnButtonClicked()
                {
                    if (!_isClaimState)
                    {
                        ClosePanel();
                        return;
                    }

                    // 发送 GetNextLevelRewardReq，回包后更新 roleInfo 并刷新 UI
                    var req = new GetNextLevelRewardReq();
                    var task = new ServerTask(req, (string receive, ServerTask t) =>
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(receive) || receive == "nodata")
                            {
                                Debug.LogWarning($"[LevelPanel] GetNextLevelRewardRes receive={receive}");
                                return;
                            }

                            var parser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
                            var res = parser.Parse<GetNextLevelRewardRes>(receive);
                            if (res != null)
                            {
                                // 弹出奖励面板
                                if (res.Items != null && res.Items.Count > 0)
                                {
                                    var rewardItems = new List<(string, int)>();
                                    foreach (var it in res.Items)
                                    {
                                        var itemInfo = Global_Inventory_Manager.GetItemInfo((int)it.ConfigID);
                                        if (itemInfo != null)
                                            rewardItems.Add((itemInfo.name, (int)it.Count));
                                    }
                                    if (rewardItems.Count > 0)
                                        EvtDsp.ReturnEvt<List<(string, int)>, bool>(EvtNames.Show_Reward, rewardItems);
                                }

                                // 更新 roleInfo.ClientRecords，供后续 UI 判断档位
                                var roleInfo = Global_Game_Manager.Instance != null
                                    ? Global_Game_Manager.Instance.get_current_player_role_info()
                                    : null;
                                if (roleInfo != null)
                                {
                                    roleInfo.ClientRecords["NextLevelRewardLevel"] = res.NextLevel.ToString();
                                    roleInfo.ClientRecords["NextLevelRewardSegment"] = res.NextSegment.ToString();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[LevelPanel] Failed to parse GetNextLevelRewardRes. receive={receive}\n{e}");
                        }
                        finally
                        {
                            Refresh();
                        }
                    });

                    EvtDsp.ReturnEvt<ServerTask, Action<string>, System.Threading.Tasks.Task>(
                        EvtNames.Excute_Server_Task,
                        task,
                        null
                    );
                }
                private void CreateThresholdLine()
                {
                    foreach(Transform child in thresholdLineFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    lines.Clear();
                    if (ExpManager.instance == null || ExpManager.instance.levels == null)
                        return;

                    // 先确定当前目标奖励等级/档位，失败则不创建
                    if (!TryResolveNextRewardTarget(out _, out _, out var levelInfo, out _))
                    {
                        return;
                    }

                    if (levelInfo.rewardItem == null || levelInfo.rewardItem.Count == 0)
                        return;

                    for (int i = 0; i < levelInfo.thresholdNum + 1; i++)
                    {
                        if (i != levelInfo.thresholdNum)
                        {
                            GameObject obj = Instantiate(thresholdLine, thresholdLineFrame);
                            ThresholdLine line = obj.GetComponent<ThresholdLine>();
                            if (i >= 0 && i < levelInfo.rewardItem.Count)
                                line.Init(levelInfo.rewardItem[i].items);
                            else
                                line.Init(new List<(string, int)>());
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

                    if (ExpManager.instance == null || ExpManager.instance.levels == null)
                        return;

                    if (!TryResolveNextRewardTarget(out _, out int seg, out var levelInfo, out string tip))
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(tip))
                    {
                        // 若 tip 不为空说明无可领奖状态，直接返回
                        return;
                    }

                    if (levelInfo.rewardItem == null || levelInfo.rewardItem.Count == 0)
                        return;

                    int idx = Mathf.Clamp(seg, 0, levelInfo.rewardItem.Count - 1);
                    var items = levelInfo.rewardItem[idx].items;
                    if (items == null) return;

                    foreach (var item in items)
                    {
                        GameObject obj = Instantiate(rewardItemCell, rewardItemFrame);
                        obj.GetComponent<RewardItemCell>().Init(item.Item1, item.Item2);
                    }
                }

                private static int GetRoleInfoInt(Common.ServerRoleInfo roleInfo, string key, int defaultValue = 0)
                {
                    if (roleInfo == null || roleInfo.ClientRecords == null) return defaultValue;
                    if (!roleInfo.ClientRecords.TryGetValue(key, out var s) || string.IsNullOrEmpty(s)) return defaultValue;
                    return int.TryParse(s, out var v) ? v : defaultValue;
                }

                /// <summary>
                /// 根据 roleInfo.NextLevelRewardLevel / NextLevelRewardSegment(+1) 解析当前应领取的等级/档位
                /// 数据实际存储在 roleInfo.ClientRecords 的 NextLevelRewardLevel / NextLevelRewardSegment 中
                /// </summary>
                private bool TryResolveNextRewardTarget(out int level, out int segment, out LevelUpInfo info, out string tip)
                {
                    level = 0;
                    segment = 0;
                    info = null;
                    tip = null;

                    if (ExpManager.instance == null || ExpManager.instance.levels == null)
                        return false;

                    var roleInfo = Global_Game_Manager.Instance != null
                        ? Global_Game_Manager.Instance.get_current_player_role_info()
                        : null;

                    int rewardLevel = roleInfo.NextLevelRewardLevel;
                    int segPlusOne = roleInfo.NextLevelRewardSegment+1;

                    // 若服务器未下发目标等级，默认从 1 级 0 档开始
                    if (rewardLevel == 0)
                    {
                        rewardLevel = 1;
                        segPlusOne = 0;
                    }

                    if (!ExpManager.instance.levels.TryGetValue(rewardLevel, out var curInfo) || curInfo == null)
                    {
                        tip = "等级配置缺失";
                        if (btnReturn != null) btnReturn.interactable = false;
                        if (_btnReturnText != null) _btnReturnText.text = "暂无奖励";
                        return false;
                    }

                    // 若 segPlusOne 超过当前等级 thresholdNum，则晋升到下一级，档位归 0
                    if (segPlusOne > curInfo.thresholdNum)
                    {
                        int nextLevel = rewardLevel + 1;
                        if (!ExpManager.instance.levels.TryGetValue(nextLevel, out var nextLevelInfo) || nextLevelInfo == null)
                        {
                            tip = "已达最高等级";
                            if (btnReturn != null) btnReturn.interactable = false;
                            if (_btnReturnText != null) _btnReturnText.text = "已领完";
                            return false;
                        }
                        rewardLevel = nextLevel;
                        segPlusOne = 0;
                        curInfo = nextLevelInfo;
                    }

                    level = rewardLevel;
                    segment = Mathf.Max(0, segPlusOne);
                    info = curInfo;
                    return true;
                }

                public override void OpenPanel(params object[] data)
                {
                    //InputManager.Instance.AllowTouchOnUI = true;
                    panelObj.SetActive(true);
                    Refresh();
                    EvtDsp.TriggerEvt(EvtNames.OnLevelPanelOpen);
                }
                public override void ClosePanel()
                {
                    //InputManager.Instance.AllowTouchOnUI = false;
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

