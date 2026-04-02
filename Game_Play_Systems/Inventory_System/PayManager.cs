using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using TapSDK.Compliance;
using UnityEngine;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {
            public class PayManager : SingletonMono<PayManager>
            {
                [Header("付款金额（单位：分）")]
                public int payAmount;
                public void Pay(int payAmount, Action action)
                {
                    var amount = payAmount;
                    TapTapCompliance.CheckPaymentLimit(
                       amount: amount,
                       handleCheckPayLimit: checkResult =>
                       {
                           if (checkResult.status == 1)
                           {
                               //todo:此处要接入支付相关方法
                               Debug.Log("充值成功");
                               SubmitPay(amount, action);
                           }
                       },
                        handleCheckPayLimitException: errorMsg =>
                        {
                            // 检查充值异常, 处理异常
                        }
                    );
                }
                private void SubmitPay(int amount, Action action)
                {
                    TapTapCompliance.SubmitPayment(
                    amount: amount,
                    handleSubmitPayResult: () =>
                    {
                        Debug.Log("提交成功");
                        action?.Invoke();
                    },
                    handleSubmitPayResultException: (exception) =>
                    {
                        // 处理异常
                    }

                    );
                }
            }
        }
    }
}