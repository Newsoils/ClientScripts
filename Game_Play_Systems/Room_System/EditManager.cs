using CLIP.Framework_Unity;
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
            InputManager.Instance.OnDragRelease -= HandleDragRelease;
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
        public void HandleTap(Vector2 mousePos)
        {
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
            _currentMode?.OnLongPress(mousePos);
        }

        public void HandleRotate()
        {
            _currentMode?.OnRotate();
        }

        //private MoveGridObjectMode _moveMode;

        //private CreateGridObjectMode _createMode;

        //public void EnterMovePlacementMode(GridObject placement = null)
        //{
        //    if (_moveMode == null)
        //        _moveMode = new MoveGridObjectMode();

        //    if (_currentMode != _moveMode)
        //    {
        //        _currentMode?.Exit();
        //        _currentMode = _moveMode;
        //    }
        //    _currentMode.Enter();

        //}

        //public void EnterCreatePlacementMode(GridObject selected)
        //{
        //    if (_createMode == null)
        //        _createMode = new CreateGridObjectMode(selected);
        //    if (_currentMode != _createMode)
        //    {
        //        _currentMode?.Exit();
        //        _currentMode = _createMode;
        //    }
        //    _currentMode.Enter();
        //}
    }
}