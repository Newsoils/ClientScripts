using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 弹窗文本单例管理器。
    /// 加载 project_mouse_tb_prompt_text.json，按 PromptId 提供文本查询，支持语言切换。
    /// 同时提供静态快捷方法直接触发弹窗事件。
    /// </summary>
    public class PromptManager : SingletonMono<PromptManager>
    {
        private const string JsonFileName = "project_mouse_tb_prompt_text";

        /// <summary>当前语言，默认中文。</summary>
        public string CurrentLanguage { get; private set; } = "cn";

        private Dictionary<int, PromptTextInfo> _promptDic;

        protected override void Awake()
        {
            base.Awake();
            LoadData();
        }

        private void LoadData()
        {
            string json = JsonDataManager.Load_Single_JsonData(JsonFileName);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError($"[PromptManager] 无法加载弹窗文本 JSON：{JsonFileName}");
                return;
            }

            var list = JsonConvert.DeserializeObject<List<PromptTextInfo>>(json);
            _promptDic = list.ToDictionary(p => p.PromptId, p => p);
            Debug.Log($"[PromptManager] 加载完成，共 {_promptDic.Count} 条弹窗文本。");
        }

        /// <summary>
        /// 切换当前语言。支持 "cn"/"en"/"jp"/"kr"，默认回退中文。
        /// </summary>
        public void SetLanguage(string lang)
        {
            CurrentLanguage = lang?.ToLower() switch
            {
                "en" => "en",
                "jp" => "jp",
                "kr" => "kr",
                _ => "cn",
            };
        }

        /// <summary>
        /// 按 PromptId 获取当前语言的文本。未找到返回空字符串。
        /// </summary>
        public string GetText(int promptId)
        {
            if (_promptDic == null)
            {
                Debug.LogWarning($"[PromptManager] 数据未加载，尝试重新加载。");
                LoadData();
            }

            if (_promptDic.TryGetValue(promptId, out var info))
            {
                string text = CurrentLanguage switch
                {
                    "en" => info.En,
                    "jp" => info.Jp,
                    "kr" => info.Kr,
                    _ => info.Cn,
                };

                // 若目标语言为空，回退中文
                if (string.IsNullOrEmpty(text))
                    text = info.Cn;

                return text;
            }

            Debug.LogWarning($"[PromptManager] 未找到 PromptId={promptId}");
            return string.Empty;
        }

        /// <summary>
        /// 按 PromptId 获取文本并用 args 填充 {0} {1} 等占位符。
        /// </summary>
        public string GetText(int promptId, params object[] args)
        {
            string template = GetText(promptId);
            if (string.IsNullOrEmpty(template) || args == null || args.Length == 0)
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                Debug.LogWarning($"[PromptManager] 文本格式化失败：PromptId={promptId}, template={template}");
                return template;
            }
        }

        /// <summary>
        /// 快捷获取上方提示文本。
        /// </summary>
        public string Up(int promptId, params object[] args) => GetText(promptId, args);

        /// <summary>
        /// 快捷获取确认弹窗文本。
        /// </summary>
        public string Confirm(int promptId, params object[] args) => GetText(promptId, args);

        /// <summary>
        /// 快捷获取警告面板文本。
        /// </summary>
        public string Warning(int promptId, params object[] args) => GetText(promptId, args);

        /// <summary>
        /// 判断指定 PromptId 是否存在。
        /// </summary>
        public bool HasPrompt(int promptId)
        {
            return _promptDic != null && _promptDic.ContainsKey(promptId);
        }

        #region 静态快捷触发方法

        /// <summary>
        /// 触发上方提示（ShowUpPrompt），文本从 prompt_text 表读取。
        /// </summary>
        public static void ShowUpPrompt(int promptId, params object[] args)
        {
            string text = Instance.GetText(promptId, args);
            if (!string.IsNullOrEmpty(text))
                EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, text);
        }

        /// <summary>
        /// 触发确认弹窗（ShowPrompt），文本从 prompt_text 表读取。
        /// </summary>
        public static void ShowPrompt(int promptId, Action onConfirm, params object[] args)
        {
            string text = Instance.GetText(promptId, args);
            if (!string.IsNullOrEmpty(text))
                EvtDsp.TriggerEvt<string, Action>(EvtNames.ShowPrompt, text, onConfirm);
        }

        /// <summary>
        /// 触发警告面板（Show_Warning_Panel），文本从 prompt_text 表读取。
        /// </summary>
        public static void ShowWarning(int promptId, params object[] args)
        {
            string text = Instance.GetText(promptId, args);
            if (!string.IsNullOrEmpty(text))
                EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, text);
        }

        #endregion
    }
}
