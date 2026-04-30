using System;
using UnityEngine;

namespace CLIP.Framework_Unity
{
    public static class Log
    {
        public enum LogLevel
        {
            Info,
            Success,
            Warning,
            Error,
            Fatal,
            Custom
        }

        // 启动初始化
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Application.logMessageReceived += HandleUnityLog;

            Info($"<color=#00FF7F>[LogSystem] Initialized </color>");
            if (RollingDiskLog.Enable)
            {
                RollingDiskLog.EnsureInitialized();
                RollingDiskLog.AppendLine(
                    $"[LogSystem] Unity log mirror on, max {RollingDiskLog.MaxFileSizeBytes / (1024 * 1024)}MB → {RollingDiskLog.FilePath}");
            }
        }

        private static void HandleUnityLog(string condition, string stackTrace, LogType type)
        {
            string prefix = type switch
            {
                LogType.Error => "[ERROR]",
                LogType.Assert => "[ASSERT]",
                LogType.Warning => "[WARN]",
                LogType.Log => "[INFO]",
                LogType.Exception => "[EXCEPTION]",
                _ => "[UNKNOWN]"
            };

            string logEntry = $"{DateTime.Now:HH:mm:ss} {prefix} {condition}\n{stackTrace}";
            RollingDiskLog.AppendLine(RemoveRichText(logEntry));
        }

        public static void Info(string msg) => Write(msg, LogLevel.Info);
        public static void Warn(string msg) => Write(msg, LogLevel.Warning);
        public static void Error(string msg) => Write(msg, LogLevel.Error);
        public static void Fatal(string msg) => Write(msg, LogLevel.Fatal);

        public static void Sucess(string msg) => Write(msg, LogLevel.Success);


        /// <summary>
        /// 自定义颜色输出（支持 #RRGGBB 或 Unity Color）
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="title">前置Title</param>
        /// <param name="color">颜色</param>

        public static void Custom(string msg, string title = "CUSTOM", Color? color = null)
        {
            string hex = color.HasValue
                ? "#" + ColorUtility.ToHtmlStringRGB(color.Value)
                : "#FFFFFF";

            CustomByHex(msg, title, hex);
        }

        /// <summary>
        /// 自定义颜色输出（HEX字符串，如 "#FF00FF"）
        /// </summary>
        public static void CustomByHex(string msg, string title = "CUSTOM", string hexColor = "#FFFFFF")
        {
            Write(msg, LogLevel.Custom, title, hexColor);
        }

        private static void Write(string msg, LogLevel level, string title = "CUSTOM", string customColor = null)
        {
            string formatted = level switch
            {
                LogLevel.Info => $"<color=#00BFFF>[INFO]</color> {msg}",
                LogLevel.Success => $"<color=green>[SUCCESS]</color> {msg}",
                LogLevel.Warning => $"<color=yellow>[WARN]</color> {msg}",
                LogLevel.Error => $"<color=red>[ERROR] {msg}</color>",
                LogLevel.Fatal => $"<color=#FF1493>[FATAL] {msg}</color>",
                LogLevel.Custom => customColor != null
                    ? $"<color={customColor}>[{title}] {msg}</color>"
                    : $"[{title}] {msg}",
                _ => msg
            };

            // Unity 控制台输出
            switch (level)
            {
                case LogLevel.Warning:
                    Debug.LogWarning(formatted);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    Debug.LogError(formatted);
                    break;
                default:
                    Debug.Log(formatted);
                    break;
            }

            // 通知 UI
            LogEvent?.Invoke(level, formatted);
        }

        private static string RemoveRichText(string text)
        {
            return System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty);
        }

        // 你未来可以用这个事件来连接到UI上（例如LogUI面板）
        public static event Action<LogLevel, string> LogEvent;

        public static void Shutdown()
        {
            Application.logMessageReceived -= HandleUnityLog;
            RollingDiskLog.Shutdown();
        }
    }
}
