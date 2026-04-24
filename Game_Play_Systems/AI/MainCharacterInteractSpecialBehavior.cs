using System;
using System.Collections.Generic;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// InteractInfo.specialBehavior 与进/出交互回调的映射；新增行为请调用 <see cref="Register"/>。
    /// </summary>
    public static class MainCharacterInteractSpecialBehavior
    {
        static readonly Dictionary<string, Action> EnterByKey = new Dictionary<string, Action>();
        static readonly Dictionary<string, Action> ExitByKey = new Dictionary<string, Action>();

        static MainCharacterInteractSpecialBehavior()
        {
            Register("bath",
                () => CharacterClothesManager.Instance.ApplyMainCharacterSuitVisualOnly("bath"),
                () => CharacterClothesManager.Instance.RestoreMainCharacterOutfitFromRecordedData());
        }

        public static void Register(string key, Action onEnter, Action onExit)
        {
            if (string.IsNullOrEmpty(key) || onEnter == null || onExit == null)
                return;
            EnterByKey[key] = onEnter;
            ExitByKey[key] = onExit;
        }

        public static void TryInvokeEnter(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            if (EnterByKey.TryGetValue(key, out var a))
                a.Invoke();
        }

        public static void TryInvokeExit(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            if (ExitByKey.TryGetValue(key, out var a))
                a.Invoke();
        }
    }
}
