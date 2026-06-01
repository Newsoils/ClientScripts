using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Network;
using Cmd;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class BuyTicketButton : MonoBehaviour
    {
        public Button button;
        public int price;
        private Game_Item_Info itemInShop;
        private bool pendingPurchase;

        private void Start()
        {
            button.onClick.AddListener(OnClick);
            EvtDsp.AddEvt<BuyTicketsRes>(EvtNames.OnBuyTicketsReceived, OnBuyTicketsRes);
            itemInShop = Global_Inventory_Manager.GetItemInfo("爱心车票");
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(OnClick);
            EvtDsp.RemoveEvt<BuyTicketsRes>(EvtNames.OnBuyTicketsReceived, OnBuyTicketsRes);
        }

        public void OnClick()
        {
            if (itemInShop == null)
            {
                PromptMessage.Instance.ShowUpPrompt("爱心车票配置不存在");
                return;
            }
            PromptMessage.Instance.ShowPrompt("是否要花费" + itemInShop.sell_price + itemInShop.currency_unit + "购买" + itemInShop.name + "?", OnConfirm);
        }

        private void OnConfirm()
        {
            pendingPurchase = true;
            NetWork_Center_WSS.SendMsg(new BuyTicketsReq
            {
                ItemID = itemInShop.item_id,
                Count = 1
            });
        }

        private void OnBuyTicketsRes(BuyTicketsRes res)
        {
            if (!pendingPurchase)
                return;

            pendingPurchase = false;
            PromptMessage.Instance.ShowUpPrompt("购买成功");
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }
    }
}
