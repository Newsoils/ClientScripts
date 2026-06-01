using CLIP.Framework_Unity;
using Lean.Touch;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class EditManager : SingletonMono<EditManager>
    {
        private IEditMode _currentMode;
        public bool isNoneState;

        private void Start()
        {
            InputManager.Instance.OnSingleTap += HandleTap;
            InputManager.Instance.OnSingleDrag += HandleDrag;
            InputManager.Instance.OnDragBegin += HandleDragBegin;
            InputManager.Instance.OnDragRelease += HandleDragRelease;
            InputManager.Instance.OnRotate += HandleRotate;
            InputManager.Instance.OnLongPress += HandleLongPress;
            isNoneState = true;
        }

        protected override void OnDestroy()
        {
            InputManager.Instance.OnSingleTap -= HandleTap;
            InputManager.Instance.OnSingleDrag -= HandleDrag;
            InputManager.Instance.OnDragBegin -= HandleDragBegin;
            InputManager.Instance.OnDragRelease -= HandleDragRelease;
            InputManager.Instance.OnLongPress -= HandleLongPress;
            InputManager.Instance.OnRotate -= HandleRotate;
            InputManager.Instance.OnLongPress -= HandleLongPress;
        }

        public void SetMode(IEditMode newMode)
        {
            _currentMode?.Exit();
            _currentMode = newMode;
            _currentMode?.Enter();
            isNoneState = false;
        }

        public void ExitCurrentMode()
        {
            _currentMode?.Exit();
            _currentMode = null;
            isNoneState = true;
        }

        /// <summary>退出当前 mode 并切换到 DefaultGridObjectMode。
        /// 用于删除家具等场景：既要让旧 mode 做清理（Exit），又不能让 _currentMode 为空导致后续 tap 全被吞掉。</summary>
        public void ResetToDefault()
        {
            _currentMode?.Exit();
            _currentMode = new DefaultGridObjectMode();
            _currentMode.Enter();
            isNoneState = false;
        }
        public void HandleTap(Vector2 mousePos)
        {
            // 双保险：InputManager.HandleFingerTap 用 StartedOverGui 过滤了 UI 起手的 Tap，
            // 这里再拦一次兜底。
            if (LeanTouch.PointOverGui(mousePos)) return;
            _currentMode?.OnTap(mousePos);
        }

        public void HandleDragBegin(Vector2 mousePos)
        {
            _currentMode?.OnDragBegin(mousePos);
        }

        public void HandleDrag(Vector2 mousePos, Vector2 curPosition)
        {
            _currentMode?.OnDrag(curPosition);
        }
        public void HandleDragRelease(Vector2 mousePos)
        {
            _currentMode?.OnDragRelease(mousePos);
        }

        public void HandleLongPress(Vector2 mousePos)
        {
            if (LeanTouch.PointOverGui(mousePos)) return;
            _currentMode?.OnLongPress(mousePos);
        }

        public void HandleRotate()
        {
            _currentMode?.OnRotate();
        }

    }
}