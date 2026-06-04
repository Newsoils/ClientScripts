using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Game_Play_System;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class PayButton : MonoBehaviour
            {
                public Button button;
                public TMP_Text numText;
                public TMP_Text priceText;
                public Image icon;
                private int num;
                private float price;
                public void Init(int num, int reward, float price, Sprite icon, bool isGetDiamond)
                {
                    numText.text = num.ToString();
                    if(isGetDiamond)
                    {
                        priceText.text = price.ToString("0") + "元";
                    }
                    else
                    {
                        priceText.text = price.ToString("0");
                    }
                    this.icon.sprite = icon;
                    this.num = num;
                    this.price = price;
                    if (isGetDiamond )
                    {
                        button.onClick.AddListener(ShowPromptDiamond);
                    }
                    else
                    {
                        button.onClick.AddListener(ShowPromptCoin);
                    }
                }
                public void ShowPromptDiamond()
                {
                    string text = PromptManager.Instance.GetText(PromptId.RechargeDiamondConfirm, price, num);
                    PromptMessage.Instance.ShowPrompt(text, PayToGetDiamond);
                }
                public void ShowPromptCoin()
                {
                    string text = PromptManager.Instance.GetText(PromptId.ExchangeCoinConfirm, price, num);
                    PromptMessage.Instance.ShowPrompt(text, PayDiamondToGetCoin);
                }
                public void PayToGetDiamond()
                {
                    PayManager.Instance.PayToGetDiamond(num, price);
                }
                public void PayDiamondToGetCoin()
                {
                    PayManager.Instance.PayDiamondToGetCoin(num, (int)price);
                }
            }
        }
    }
}
