using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using Lean.Touch;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public enum TouchGestureType
    {
        None,
        SingleTap,      // 单指点击
        DoubleTap,      // 单指双击
        SingleDrag,     // 单指拖动
        MultiDrag,      // 多指拖动
        PinchSpread,  // 捏合/张开
        LongPress
    }

    public class InputManager : SingletonMono<InputManager>
    {
        #region Public Events
        // 事件：单指点击
        public Action<Vector2> OnSingleTap;
        public Action<Vector2> OnDoubleTap;

        // 事件：单指拖动
        public Action<Vector2, Vector2> OnSingleDrag; // 参数：移动向量，当前位置

        // 事件：多指拖动
        public Action<Vector2, Vector2> OnMultiDrag;  // 参数：移动向量，中心点位置

        public Action<Vector2> OnDragBegin;

        public Action<Vector2> OnDragRelease; // 参数：释放位置

        // 事件：捏合/张开
        public Action<float, float> OnPinchSpread;    // 参数：距离变化，当前距离比例

        public Action<Vector2> OnLongPress;

        // 事件：手势状态改变
        public Action<TouchGestureType> OnGestureChanged;

        public Action OnRotate;


        #endregion

        #region 公共变量
        public TouchGestureType CurrentGesture => _currentGesture;
        public Vector2 SingleDragDelta => _singleDragDelta;
        public Vector2 MultiDragDelta => _multiDragDelta;
        public bool WasSingleTapThisFrame;
        public bool WasDoubleTapThisFrame;
        public Vector2 LastTapPosition => _lastTapPosition;
        public Vector2 CurrentDragPosition => _currentDragPosition;
        public Vector2 JoystickInput => _joystickInput;
        public float PinchRatio => _pinchRatio;


        private bool _allowTouchOnUI;
        public bool AllowTouchOnUI
        {
            get => _allowTouchOnUI;
            set
            {
                _allowTouchOnUI = value;
                if (!value)
                {
                    // 可选：当关闭允许 UI 触摸时，清除当前所有在 UI 上的手指
                    // 但通常不需要
                }
            }
        }
        #endregion

        #region Settings
        [Header("Settings")]
        [SerializeField] private float _tapThreshold = 0.2f;      // 点击时间阈值（秒）
        [SerializeField] private float _dragThreshold = 10f;     // 拖动最小距离阈值（像素）
        [SerializeField] private float _pinchThreshold = 0.01f;   // 捏合最小变化阈值

        [SerializeField] private float _longPressThreshold = 0.4f;
        [SerializeField] private float _longPressMoveTolerance = 10f;
        #endregion

        #region Private Variables
        [SerializeField] private TouchGestureType _currentGesture = TouchGestureType.None;
        private List<LeanFinger> _currentFingers = new List<LeanFinger>();

        // 单指操作相关
        private Vector2 _lastTapPosition;
        private Vector2 _singleDragStartPosition;
        private Vector2 _singleDragDelta;
        private Vector2 _currentDragPosition;
        private Vector2 _joystickInput;
        private float _singleTapTimer;
        private float _doubleTapTimer;
        private bool _isSingleTapPossible;
        public float _pinchRatio = 1;

        // 多指操作相关
        private Vector2 _multiDragStartCenter;
        private Vector2 _multiDragCenter;
        private Vector2 _multiDragDelta;
        private float _pinchStartDistance;
        private float _pinchCurrentDistance;
        private bool _longPressTriggered;
        private bool _isDragging;
        //private bool _dragOver;
        #endregion

        #region 生命周期
        private void OnEnable()
        {
            // 订阅 LeanTouch 事件
            LeanTouch.OnFingerDown += HandleFingerDown;
            LeanTouch.OnFingerUpdate += HandleFingerUpdate;
            LeanTouch.OnFingerUp += HandleFingerUp;
            LeanTouch.OnFingerTap += HandleFingerTap;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 取消订阅
            LeanTouch.OnFingerDown -= HandleFingerDown;
            LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
            LeanTouch.OnFingerUp -= HandleFingerUp;
            LeanTouch.OnFingerTap -= HandleFingerTap;
        }

        private void Update()
        {
            _doubleTapTimer += Time.deltaTime;
            // 处理点击计时
            if (_isSingleTapPossible)
            {
                _singleTapTimer += Time.deltaTime;
                if (_singleTapTimer > _tapThreshold)
                {
                    _isSingleTapPossible = false;
                }
            }
            if(Keyboard.current.rKey.wasPressedThisFrame)
            {
                OnRotate?.Invoke();
            }
        }
        #endregion

        #region Event Handlers
        private void HandleFingerDown(LeanFinger finger)
        {
            // 修改这里：如果手指在 UI 上且不允许 UI 触摸，才忽略
            if (finger.IsOverGui && !AllowTouchOnUI)
            {
                Log.Info("触碰UI，忽略输入");
                return;
            }

            _currentFingers.Add(finger);

            // 重置单指点击状态
            _isSingleTapPossible = true;
            _singleTapTimer = 0f;

            // 记录点击位置
            _lastTapPosition = finger.ScreenPosition;

            // 单指按下：准备可能的点击或拖动
            if (_currentFingers.Count == 1)
            {
                _singleDragStartPosition = finger.ScreenPosition;
                _currentDragPosition = finger.ScreenPosition;
            }

            // 多指按下：初始化多指操作
            if (_currentFingers.Count >= 2)
            {
                //UpdateMultiFingerData();
            }
        }

        private void HandleFingerUpdate(LeanFinger finger)
        {
            // 更新当前手指列表
            UpdateFingerList();

            int fingerCount = _currentFingers.Count;

            if (fingerCount == 0) return;

            // 单指操作
            if (fingerCount == 1)
            {
                HandleSingleFingerUpdate();
            }
            // 多指操作
            else if (fingerCount >= 2)
            {
                HandleMultiFingerUpdate();
            }
        }

        private void HandleFingerUp(LeanFinger finger)
        {
            // 从列表中移除手指
            _currentFingers.Remove(finger);

            if (_isDragging && _currentFingers.Count == 0)
            {
                OnDragRelease?.Invoke(_currentDragPosition);
                _isDragging = false;
            }

            // 如果所有手指都抬起，重置状态
            if (_currentFingers.Count == 0)
            {
                ResetGesture();
                StartCoroutine(DelayReset());
            }

            _longPressTriggered = false;
        }

        private IEnumerator DelayReset()
        {
            yield return null; // 等一帧，让 Tap 先触发
        }

        private void HandleFingerTap(LeanFinger finger)
        {
            if (finger.IsOverGui && !AllowTouchOnUI) return;
            // 单指点击事件
            //if (_currentFingers.IndexOf(finger) == 0) // 只处理第一个手指的点击
            {
                StartCoroutine(ClearTap());
                if(_doubleTapTimer < _tapThreshold)
                {
                    WasDoubleTapThisFrame = true;
                    SetGesture(TouchGestureType.DoubleTap);
                    OnDoubleTap?.Invoke(finger.ScreenPosition);
                    _lastTapPosition = finger.ScreenPosition;
                }
                else
                {
                    _doubleTapTimer = 0;
                    WasSingleTapThisFrame = true;
                    SetGesture(TouchGestureType.SingleTap);
                    OnSingleTap?.Invoke(finger.ScreenPosition);
                    _lastTapPosition = finger.ScreenPosition;
                }
            }
        }
        private IEnumerator ClearTap()
        {
            yield return new WaitForEndOfFrame();
            WasSingleTapThisFrame = false;
            WasDoubleTapThisFrame = false;
        }
        #endregion

        #region Gesture Handling
        private void HandleSingleFingerUpdate()
        {
            if (_currentFingers.Count == 0) return;

            var finger = _currentFingers[0];
            Vector2 currentPosition = finger.ScreenPosition;
            Vector2 delta = finger.ScreenDelta;

            // 检查是否达到拖动阈值
            float dragDistance = Vector2.Distance(currentPosition, _singleDragStartPosition);

            if (!_isDragging && dragDistance > _dragThreshold)
            {
                _isDragging = true;
                // 取消可能的点击
                _isSingleTapPossible = false;
                SetGesture(TouchGestureType.SingleDrag);

                OnDragBegin?.Invoke(finger.ScreenPosition);
            }
            if (_isDragging)
            {
                // 更新当前位置
                _currentDragPosition = currentPosition;
                _singleDragDelta = delta;

                // 触发拖动事件
                OnSingleDrag?.Invoke(delta, currentPosition);
            }

            // 长按检测
            if (!_longPressTriggered &&
                finger.Age > _longPressThreshold &&
                dragDistance < _longPressMoveTolerance)
            {
                _longPressTriggered = true;

                SetGesture(TouchGestureType.LongPress);
                OnLongPress?.Invoke(finger.ScreenPosition);
                Debug.Log("检测到长按");
            }
        }

        private void HandleMultiFingerUpdate()
        {
            if (_currentFingers.Count < 2) return;

            UpdateMultiFingerData();

            // 检查是拖动还是捏合
            Vector2 centerDelta = _multiDragCenter - _multiDragStartCenter;
            float centerMoveDistance = centerDelta.magnitude;

            float pinchRatio = _pinchStartDistance > 0 ?
                _pinchCurrentDistance / _pinchStartDistance : 1f;
            float pinchChange = Mathf.Abs(1f - pinchRatio);

            // 判断操作类型
            if (pinchChange > _pinchThreshold)
            {
                _pinchRatio = LeanGesture.GetPinchRatio(_currentFingers);
                // 捏合/张开操作
                SetGesture(TouchGestureType.PinchSpread);
                OnPinchSpread?.Invoke(_pinchCurrentDistance - _pinchStartDistance, pinchRatio);
            }
            else if (centerMoveDistance > _dragThreshold)
            {
                // 多指拖动操作
                SetGesture(TouchGestureType.MultiDrag);
                OnMultiDrag?.Invoke(_multiDragDelta, _multiDragCenter);
            }
        }

        private void UpdateMultiFingerData()
        {
            if (_currentFingers.Count < 2) return;

            // 计算多个手指的中心点
            Vector2 sum = Vector2.zero;
            Vector2 deltaSum = Vector2.zero;
            foreach (var finger in _currentFingers)
            {
                sum += finger.ScreenPosition;
                deltaSum += finger.ScreenDelta;
            }
            _multiDragCenter = sum / _currentFingers.Count;
            _multiDragDelta = deltaSum / _currentFingers.Count;

            // 如果是第一次记录，保存为起始中心点
            if (_multiDragStartCenter == Vector2.zero)
            {
                _multiDragStartCenter = _multiDragCenter;
            }

            // 计算两个主要手指间的距离（用于捏合检测）
            if (_currentFingers.Count >= 2)
            {
                var finger1 = _currentFingers[0];
                var finger2 = _currentFingers[1];
                _pinchCurrentDistance = Vector2.Distance(
                    finger1.ScreenPosition,
                    finger2.ScreenPosition
                );

                if (_pinchStartDistance == 0)
                {
                    _pinchStartDistance = _pinchCurrentDistance;
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 获取当前屏幕触摸点（单指或多指中心点）
        /// </summary>
        public Vector2 GetScreenPosition()
        {
            if (_currentFingers.Count == 0)
                return _lastTapPosition;

            if (_currentFingers.Count == 1)
                return _currentFingers[0].ScreenPosition;

            // 多指返回中心点
            Vector2 sum = Vector2.zero;
            foreach (var finger in _currentFingers)
            {
                sum += finger.ScreenPosition;
            }
            return sum / _currentFingers.Count;
        }

        /// <summary>
        /// 获取单指拖动距离（像素）
        /// </summary>
        public float GetSingleDragDistance()
        {
            if (_currentGesture != TouchGestureType.SingleDrag || _currentFingers.Count == 0)
                return 0;

            return Vector2.Distance(
                _currentFingers[0].ScreenPosition,
                _singleDragStartPosition
            );
        }

        /// <summary>
        /// 获取单指拖动方向向量
        /// </summary>
        public Vector2 GetSingleDragDirection()
        {
            if (_currentGesture != TouchGestureType.SingleDrag || _currentFingers.Count == 0)
                return Vector2.zero;

            return (_currentFingers[0].ScreenPosition - _singleDragStartPosition).normalized;
        }

        /// <summary>
        /// 获取多指拖动距离（像素）
        /// </summary>
        public float GetMultiDragDistance()
        {
            if (_currentGesture != TouchGestureType.MultiDrag)
                return 0;

            return Vector2.Distance(_multiDragCenter, _multiDragStartCenter);
        }

        /// <summary>
        /// 获取捏合/张开的距离变化
        /// </summary>
        public float GetPinchSpreadDistance()
        {
            if (_currentGesture != TouchGestureType.PinchSpread)
                return 0;

            return _pinchCurrentDistance - _pinchStartDistance;
        }

        /// <summary>
        /// 获取捏合/张开的变化比例
        /// </summary>
        public float GetPinchSpreadRatio()
        {
            if (_currentGesture != TouchGestureType.PinchSpread || _pinchStartDistance == 0)
                return 1f;

            return _pinchCurrentDistance / _pinchStartDistance;
        }

        /// <summary>
        /// 重置手势状态
        /// </summary>
        public void ResetGesture()
        {
            SetGesture(TouchGestureType.None);
            _currentFingers.Clear();
            _singleDragStartPosition = Vector2.zero;
            _singleDragDelta = Vector2.zero;
            _multiDragStartCenter = Vector2.zero;
            _multiDragDelta = Vector2.zero;
            _pinchStartDistance = 0;
            _isSingleTapPossible = false;
        }
        public void SetJoystickInput(Vector2 input)
        {
            _joystickInput = input;
        }
        #endregion

        #region Helper Methods
        private void UpdateFingerList()
        {
            // 移除已经抬起的手指
            _currentFingers.RemoveAll(finger => finger.Up);
        }

        private void SetGesture(TouchGestureType newGesture)
        {
            if (_currentGesture != newGesture)
            {
                _pinchRatio = 1;
                _singleDragStartPosition = Vector2.zero;
                _singleDragDelta = Vector2.zero;
                _multiDragStartCenter = Vector2.zero;
                _multiDragDelta = Vector2.zero;
                _currentGesture = newGesture;
                OnGestureChanged?.Invoke(newGesture);
            }
        }
        #endregion

        public bool IsPointerOverUI(LeanFinger finger)
        {
            return LeanTouch.PointOverGui(finger.ScreenPosition);
        }
    }
}

