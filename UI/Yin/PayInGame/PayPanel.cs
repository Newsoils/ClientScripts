using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
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
                private void Start()
                {
                    btnClose.onClick.AddListener(ClosePanel);
                    foreach(Transform child in diamondFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach(var price in diamondList)
                    {
                        GameObject obj = Instantiate(payButtonObj, diamondFrame);
                        PayButton button = obj.GetComponent<PayButton>();
                        button.Init(price.num,price.reward, price.price, price.icon,true);
                    }
                    foreach (Transform child in coinFrame)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach (var price in coinList)
                    {
                        GameObject obj = Instantiate(payDiamondButtonObj, coinFrame);
                        PayButton button = obj.GetComponent<PayButton>();
                        button.Init(price.num, price.reward, price.price, price.icon, false);
                    }
                }
                public void OpenPanel()
                {
                    panelObj.SetActive(!isOpen);
                    isOpen = !isOpen;
                }
                public void ClosePanel()
                {
                    panelObj.SetActive(false);
                    isOpen = false;
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
                public void PayToGetDiamond(int amount, float money)
                {
                    PayManager.Instance.Pay((int)(money * 100), new Action(()=> MoneyManager.Instance.ChangeCurrency("罐罐", amount, "充值", HandlePayResult)));
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

