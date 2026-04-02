using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class BuyTicketButton : MonoBehaviour
            {
                public Button button;
                public int price;
                private Game_Item_Info itemInShop;
                private void Start()
                {
                    button.onClick.AddListener(OnClick);
                    itemInShop = Global_Inventory_Manager.GetItemInfo("爱心车票");
                }
                public void OnClick()
                {
                    PromptMessage.Instance.ShowPrompt("是否要花费" + itemInShop.sell_price + itemInShop.currency_unit + "购买" + itemInShop.name + "?", OnConfirm);
                }
                private void OnConfirm()
                {
                    Action<string> onPaySuccess = (string result) =>
                    {
                        if (result == "success")
                        {
                            PromptMessage.Instance.ShowUpPrompt("购买成功");
                            Global_Inventory_Manager.Change_Items_Count(new List<(string, int)> { (itemInShop.name, 1) }, "购买获取");
                            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
                        }
                        else
                        {
                            PromptMessage.Instance.ShowUpPrompt(result);
                        }
                    };
                    MoneyManager.Instance.ChangeCurrency(itemInShop.currency_unit, -itemInShop.sell_price, "购买物品", onPaySuccess);
                }
            }
        }
    }
}
