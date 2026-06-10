#if UNITY_EDITOR
using UnityEditor;

namespace CLIP.Project_Mouse.AIGameTestTools
{
    [InitializeOnLoad]
    public static class AIGameTestAutoRefreshGuard
    {
        private const string SessionKey = "AIGameTest.AutoRefreshLocked";
        private static bool IsLocked => SessionState.GetBool(SessionKey, false);

        static AIGameTestAutoRefreshGuard()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += Unlock;
        }

        public static void Lock()
        {
            if (IsLocked)
                return;

            // Keep external Codex edits pending until the AI test Play session ends.
            AssetDatabase.DisallowAutoRefresh();
            SessionState.SetBool(SessionKey, true);
        }

        public static void Unlock()
        {
            if (!IsLocked)
                return;

            AssetDatabase.AllowAutoRefresh();
            SessionState.SetBool(SessionKey, false);
        }

        public static void UnlockIfNotPlaying()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                Unlock();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode ||
                state == PlayModeStateChange.EnteredEditMode)
            {
                Unlock();
            }
        }
    }
}
#endif
