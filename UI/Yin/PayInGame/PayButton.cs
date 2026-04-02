using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
                    if (reward == 0)
                    {
                        numText.text = num.ToString();
                    }
                    else
                    {
                        numText.text = num + "<voffset=0.5em><size=75%>+" + reward + "</size></voffset>";
                    }
                    if(isGetDiamond)
                    {
                        priceText.text = "￥" + price.ToString("0");
                    }
                    else
                    {
                        priceText.text = price.ToString("0");
                    }
                    this.icon.sprite = icon;
                    this.num = num + reward;
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
                    string text = "是否要花费￥" + price + "购买" + num + "罐罐？";
                    PromptMessage.Instance.ShowPrompt(text, PayToGetDiamond);
                }
                public void ShowPromptCoin()
                {
                    string text = "是否要花费" + price + "罐罐购买" + num + "鱼币？";
                    PromptMessage.Instance.ShowPrompt(text, PayDiamondToGetCoin);
                }
                public void PayToGetDiamond()
                {
                    PayPanel.Instance.PayToGetDiamond(num, price);
                }
                public void PayDiamondToGetCoin()
                {
                    PayPanel.Instance.PayDiamondToGetCoin(num, (int)price);
                }
            }
        }
    }
}
