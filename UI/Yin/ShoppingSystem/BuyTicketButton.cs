using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Network;
using Cmd;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Project_Mouse.Kernel;

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
                PromptManager.ShowUpPrompt(PromptId.LoveTicketConfigMissing);
                return;
            }
            PromptManager.ShowPrompt(PromptId.BuyTicketConfirm, OnConfirm, itemInShop.sell_price, itemInShop.currency_unit, itemInShop.name);
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
            PromptManager.ShowUpPrompt(PromptId.PurchaseSuccess);
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }
    }
}
