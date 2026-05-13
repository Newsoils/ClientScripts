using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class PayPanel : SingletonMono<PayPanel>
            {
                public GameObject panelObj;
                private bool isOpen;
                public Button btnClose;
                public Transform diamondFrame;
                public Transform coinFrame;
                public GameObject payButtonObj;
                public GameObject payDiamondButtonObj;
                public Sprite diamondIcon;
                public Sprite coinIcon;

                public List<MoneyPrice> diamondList;
                public List<MoneyPrice> coinList;

                private RechargeDailyLimits _rechargeLimits;
                private List<PayButton> _diamondButtons = new List<PayButton>();

                private void Start()
                {
                    btnClose.onClick.AddListener(ClosePanel);
                    foreach(Transform child in diamondFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    for (int i = 0; i < diamondList.Count; i++)
                    {
                        var price = diamondList[i];
                        GameObject obj = Instantiate(payButtonObj, diamondFrame);
                        PayButton button = obj.GetComponent<PayButton>();
                        button.Init(price.num, price.reward, price.price, price.icon, true, i);
                        _diamondButtons.Add(button);
                    }
                    foreach (Transform child in coinFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach (var price in coinList)
                    {
                        GameObject obj = Instantiate(payDiamondButtonObj, coinFrame);
                        PayButton button = obj.GetComponent<PayButton>();
                        button.Init(price.num, price.reward, price.price, price.icon, false, -1);
                    }
                }

                public void OpenPanel()
                {
                    panelObj.SetActive(!isOpen);
                    isOpen = !isOpen;
                    if (isOpen)
                        LoadRechargeLimit();
                }

                public void ClosePanel()
                {
                    panelObj.SetActive(false);
                    isOpen = false;
                }

                public void LoadRechargeLimit()
                {
                    string defaultData = JsonConvert.SerializeObject(new RechargeDailyLimits
                    {
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        counts = new Dictionary<int, int>()
                    });
                    ServerTask task = new ServerTask(defaultData, (string data, ServerTask t) =>
                    {
                        if (string.IsNullOrEmpty(data) || data == "nodata")
                            return;
                        _rechargeLimits = JsonConvert.DeserializeObject<RechargeDailyLimits>(data);
                        RefreshDiamondButtonLimits();
                    });
                    EvtDsp.ReturnEvt<string, ServerTask, Action<string>, System.Threading.Tasks.Task>(
                        EvtNames.Excute_Server_Task, "LoadRechargeLimit", task, null);
                }

                private void RefreshDiamondButtonLimits()
                {
                    if (_rechargeLimits == null) return;
                    for (int i = 0; i < _diamondButtons.Count; i++)
                    {
                        _rechargeLimits.counts.TryGetValue(i, out int used);
                        _diamondButtons[i].UpdateDailyLimit(30 - used);
                    }
                }

                public void PayDiamondToGetCoin(int amount, int price)
                {
                    List<(string, int)> changeList = new List<(string, int)>()
                    {
                        ("罐罐", -price),
                        ("鱼币", amount)
                    };
                    MoneyManager.Instance.ChangeCurrencyMulti(changeList, "购买鱼币消耗", HandlePayResult);
                }

                public void PayToGetDiamond(int amount, float money, int optionIndex)
                {
                    if (_rechargeLimits != null)
                    {
                        _rechargeLimits.counts.TryGetValue(optionIndex, out int used);
                        if (used >= 30)
                        {
                            PromptMessage.Instance.ShowUpPrompt("今日该选项已达购买上限");
                            return;
                        }
                    }

                    PayManager.Instance.Pay((int)(money * 100), new Action(() =>
                    {
                        string requestData = JsonConvert.SerializeObject(
                            new List<string> { optionIndex.ToString(), amount.ToString() });
                        ServerTask task = new ServerTask(requestData, (string data, ServerTask t) =>
                        {
                            if (data == "今日该选项已达购买上限")
                            {
                                t.isBreak = true;
                                t.result = data;
                                return;
                            }
                            if (string.IsNullOrEmpty(data) || data == "nodata")
                            {
                                t.isBreak = true;
                                t.result = "充值失败";
                                return;
                            }
                            var resp = JsonConvert.DeserializeObject<RechargeWithLimitResponse>(data);
                            MoneyManager.Instance.SaveToLocal(resp.currencyData);
                            _rechargeLimits = resp.rechargeLimits;
                            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
                            t.result = "success";
                            RefreshDiamondButtonLimits();
                        });
                        EvtDsp.ReturnEvt<string, ServerTask, Action<string>, System.Threading.Tasks.Task>(
                            EvtNames.Excute_Server_Task, "RechargeWithLimit", task, HandlePayResult);
                    }));
                }

                public void HandlePayResult(string result)
                {
                    if(result == "success")
                    {
                        PromptMessage.Instance.ShowUpPrompt("充值成功");
                        AudioManager.Instance.PlayAudioByRefKey("paySuccess");
                    }
                    else
                    {
                        PromptMessage.Instance.ShowUpPrompt(result);
                    }
                }
            }
            [Serializable]
            public class MoneyPrice
            {
                public int num;
                public int reward;
                public float price;
                public Sprite icon;
            }
        }
    }
}
