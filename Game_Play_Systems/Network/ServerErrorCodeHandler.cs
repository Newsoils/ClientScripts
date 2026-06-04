using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using Common;
using CLIP.Project_Mouse.Kernel;

namespace CLIP.Project_Mouse.Network
{
    /// <summary>
    /// 服务器错误码统一处理器。
    /// 将原本硬编码在 Msg_Dispatcher 中的错误码处理逻辑迁移至此，
    /// 新增错误码处理时只需在本类注册，无需修改消息分发器。
    /// </summary>
    public static class ServerErrorCodeHandler
    {
        private static readonly Dictionary<int, Action<string[]>> _handlers = new();

        static ServerErrorCodeHandler()
        {
            RegisterCurrencyNotEnoughHandlers();
            RegisterPlantHandlers();
            RegisterTicketHandlers();
            RegisterGachaHandlers();
        }

        /// <summary>
        /// 注册自定义错误码处理器。
        /// </summary>
        public static void Register(int errorCode, Action<string[]> handler)
        {
            _handlers[errorCode] = handler;
        }

        /// <summary>
        /// 尝试处理指定错误码。若已注册则执行并返回 true，否则返回 false。
        /// </summary>
        public static bool TryHandle(int errorCode, string[] args)
        {
            if (_handlers.TryGetValue(errorCode, out var handler))
            {
                handler?.Invoke(args);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 用 args 填充模板中的 {0} {1} 等占位符。
        /// </summary>
        public static string FormatMessage(string template, string[] args)
        {
            if (string.IsNullOrEmpty(template) || args == null || args.Length == 0)
                return template;
            for (int i = 0; i < args.Length; i++)
                template = template.Replace("{" + i + "}", args[i]);
            return template;
        }

        #region Currency

        private static void RegisterCurrencyNotEnoughHandlers()
        {
            Register((int)ErrorCode.LogicResourceNotEnough, OnCurrencyNotEnough);
            Register((int)ErrorCode.LogicItemNotEnough, OnCurrencyNotEnough);
            Register((int)ErrorCode.LogicResourceSilverCoinNotEnough, OnCurrencyNotEnough);
            Register((int)ErrorCode.LogicResourceGoldIngotNotEnough, OnCurrencyNotEnough);
            Register((int)ErrorCode.LogicResourceGoldBeanNotEnough, OnCurrencyNotEnough);
        }

        private static void OnCurrencyNotEnough(string[] args)
        {
            Log.Warn("[ServerErrorCodeHandler] Currency not enough.");
            string message = PromptManager.Instance.GetText(PromptId.CurrencyNotEnough, args);
            EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, message, OpenPayPanel);
        }

        private static void OpenPayPanel()
        {
            EvtDsp.TriggerEvt(EvtNames.OpenPayPanel);
        }

        #endregion

        #region Plant

        private static void RegisterPlantHandlers()
        {
            Register((int)ErrorCode.LogicRolePlantPotNotInRoom, OnPlantPotNotInRoom);
        }

        private static void OnPlantPotNotInRoom(string[] args)
        {
            Log.Warn("[ServerErrorCodeHandler] Plant pot not in room.");
            string message = PromptManager.Instance.GetText(PromptId.PotPlacementHint, args);
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, message);
        }

        #endregion

        #region Ticket

        private static void RegisterTicketHandlers()
        {
            Register((int)ErrorCode.LogicRoleCatLoveTicketNotEnough, OnLoveTicketNotEnough);
        }

        private static void OnLoveTicketNotEnough(string[] args)
        {
            Log.Warn("[ServerErrorCodeHandler] Love ticket not enough.");
            string message = PromptManager.Instance.GetText(PromptId.LoveTicketNotEnough, args);
            EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, message, OpenShoppingPanel);
        }

        private static void OpenShoppingPanel()
        {
            EvtDsp.TriggerEvt(EvtNames.OpenShoppingPanel);
        }

        #endregion

        #region Gacha

        private static void RegisterGachaHandlers()
        {
            Register((int)ErrorCode.LogicGachaTimesNotEnoughToday, OnGachaTimesNotEnoughToday);
            Register((int)ErrorCode.LogicGachaTimesLeftFewToday, OnGachaTimesLeftFewToday);
        }

        private static void OnGachaTimesNotEnoughToday(string[] args)
        {
            Log.Warn("[ServerErrorCodeHandler] Gacha times not enough today.");
            string message = PromptManager.Instance.GetText(PromptId.GachaTimesExhausted, args);
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, message);
        }

        private static void OnGachaTimesLeftFewToday(string[] args)
        {
            Log.Warn("[ServerErrorCodeHandler] Gacha times left few today.");
            string message = PromptManager.Instance.GetText(PromptId.GachaTimesLow, args);
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, message);
        }

        #endregion
    }
}
