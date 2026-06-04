using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Cmd;
using TapSDK.Compliance;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class PayManager : SingletonMono<PayManager>
    {
        protected override bool PersistAcrossScenes => true;

        public static PayManager EnsureInstance()
        {
            if (Instance != null)
                return Instance;

            var go = new GameObject(nameof(PayManager));
            return go.AddComponent<PayManager>();
        }

        private static readonly Dictionary<int, int> DiamondMoneyYuanToIndex = new Dictionary<int, int>
        {
            { 1, 0 },
            { 6, 1 },
            { 30, 2 },
            { 68, 3 },
            { 198, 4 },
            { 328, 5 },
        };

        private static readonly Dictionary<int, int> CoinPriceToIndex = new Dictionary<int, int>
        {
            { 60, 6 },
            { 200, 7 },
            { 1000, 8 },
            { 2000, 9 },
            { 4000, 10 },
            { 8000, 11 },
        };

        private Action<string> _pendingPayResultCallback;

        public static int GetDiamondPurchaseIndex(float moneyYuan)
        {
            int yuan = Mathf.RoundToInt(moneyYuan);
            return DiamondMoneyYuanToIndex.TryGetValue(yuan, out var index) ? index : -1;
        }

        public static int GetDiamondPurchaseIndexFromAmountCents(int amountCents) =>
            GetDiamondPurchaseIndex(amountCents / 100f);

        public static int GetCoinExchangeIndex(int price) =>
            CoinPriceToIndex.TryGetValue(price, out var index) ? index : -1;

        public static long GetBuyDiamondTimes(int index)
        {
            var record = Global_Game_Manager.Instance?._current_player_role_info?.BuyDiamondTimesRecord;
            if (record == null || index < 0 || index >= record.Count)
                return 0;
            return record[index];
        }

        /// <summary>人民币充值罐罐：校验每日次数后走 Tap 支付，提交成功后发 <see cref="BuyDiamondTimesReq"/>。</summary>
        public void PayToGetDiamond(int amount, float money, Action<string> onResult)
        {
            int index = GetDiamondPurchaseIndex(money);
            if (index < 0)
            {
                onResult?.Invoke(PromptManager.Instance.GetText(PromptId.RechargeOptionInvalid));
                return;
            }

            if (GetBuyDiamondTimes(index) >= 30)
            {
                onResult?.Invoke(PromptManager.Instance.GetText(PromptId.RechargeDailyLimit));
                return;
            }

            Pay((int)(money * 100), index, onResult);
        }

        /// <summary>罐罐兑换鱼币：直接发 <see cref="BuyDiamondTimesReq"/>。</summary>
        public void PayDiamondToGetCoin(int price, Action<string> onResult)
        {
            int index = GetCoinExchangeIndex(price);
            if (index < 0)
            {
                onResult?.Invoke(PromptManager.Instance.GetText(PromptId.PurchaseOptionInvalid));
                return;
            }

            SendBuyDiamondTimes(index, onResult);
        }

        public void SendBuyDiamondTimes(int index, Action<string> onResult)
        {
            _pendingPayResultCallback = onResult;
            if (!NetWork_Center_WSS.IsConnectedToPlayerServer)
            {
                _pendingPayResultCallback = null;
                onResult?.Invoke(PromptManager.Instance.GetText(PromptId.NetworkDisconnected));
                return;
            }

            NetWork_Center_WSS.SendMsg(new BuyDiamondTimesReq { Index = index });
        }

        public void OnBuyDiamondTimesRes()
        {
            _pendingPayResultCallback?.Invoke("success");
            _pendingPayResultCallback = null;
        }

        private void Pay(int payAmount, int buyIndex, Action<string> onResult)
        {
            TapTapCompliance.CheckPaymentLimit(
                amount: payAmount,
                handleCheckPayLimit: checkResult =>
                {
                    if (checkResult.status == 1)
                    {
                        Debug.Log("充值成功");
                        SubmitPay(payAmount, buyIndex, onResult);
                    }
                },
                handleCheckPayLimitException: _ =>
                {
                    onResult?.Invoke(PromptManager.Instance.GetText(PromptId.RechargeCheckFailed));
                });
        }

        private void SubmitPay(int amount, int buyIndex, Action<string> onResult)
        {
            TapTapCompliance.SubmitPayment(
                amount: amount,
                handleSubmitPayResult: () =>
                {
                    Debug.Log("提交成功");
                    SendBuyDiamondTimes(buyIndex, onResult);
                },
                handleSubmitPayResultException: _ =>
                {
                    onResult?.Invoke(PromptManager.Instance.GetText(PromptId.RechargeSubmitFailed));
                });
        }

        public void PayDiamondToGetCoin(int amount, int price)
        {
            EnsureInstance().PayDiamondToGetCoin(price, HandlePayResult);
        }


        public void PayToGetDiamond(int amount, float money)
        {
            EnsureInstance().PayToGetDiamond(amount, money, HandlePayResult);
        }
        public void HandlePayResult(string result)
        {
            if (result == "success")
            {
                PromptManager.ShowUpPrompt(PromptId.RechargeSuccess);
                AudioManager.Instance.PlayAudioByRefKey("paySuccess");
            }
            else
            {
                EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, result);
            }
        }
    }
}
