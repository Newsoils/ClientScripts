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
        protected override bool PersistAcrossScenes => true;
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
        public bool IsDragging => _isDragging;
        #endregion

        #region Settings
        [Header("Settings")]
        [SerializeField] private float _tapThreshold = 0.2f;      // 点击时间阈值（秒）
        [SerializeField] private float _dragThreshold = 10f;     // 拖动最小距离阈值（像素）
        [SerializeField] private float _pinchThreshold = 0.01f;   // 捏合最小变化阈值

        [SerializeField] private float _longPressThreshold = 0.4f;
        [SerializeField] private float _longPressMoveTolerance = 10f;
        [Tooltip("开：双指每帧打 pinch 判定与 gesture。配合 CinemachineCameraController.debugZoomRotationTrace。复现完关。")]
        [SerializeField] private bool debugPinchGestureTrace;
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
        private float _pinchRatio = 1f;

        // 多指操作相关
        private Vector2 _multiDragStartCenter;
        private Vector2 _multiDragCenter;
        private Vector2 _multiDragDelta;
        private float _pinchStartDistance;
        private float _pinchCurrentDistance;
        private bool _longPressTriggered;
        private bool _isDragging;
        //private bool _dragOver;

        /// <summary>已滑出 UI 区域的手指 ID → 是否已触发过 DragBegin。
        /// UI 起手的手指在滑出 UI 前不产生 delta，滑出后开始对外暴露 drag 信号。</summary>
        private Dictionary<int, bool> _uiFingersLeftUI = new Dictionary<int, bool>();
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
            // UI 起手的手指也进 _currentFingers——否则 OnDragBegin/OnDrag/OnDragRelease 永远不会触发，
            // "从仓库 UI 拖出家具到世界"的流程就断了。真正的区别在 HandleSingleFingerUpdate：
            // 只有当 UI 起手的手指滑出 UI 区域后，才把 delta 暴露给外部（SingleDragDelta/MultiDragDelta），
            // 保证 UI 内的滚动（分类 tab / 仓库列表）不会触发 CinemachineController.CheckRotate。
            _currentFingers.Add(finger);

            // 重置单指点击状态
            _isSingleTapPossible = true;
            _singleTapTimer = 0f;
            _lastTapPosition = finger.ScreenPosition;

            // 单指按下：准备可能的点击或拖动
            if (_currentFingers.Count == 1)
            {
                _singleDragStartPosition = finger.ScreenPosition;
                _currentDragPosition = finger.ScreenPosition;
            }

            if (finger.StartedOverGui)
                _uiFingersLeftUI[finger.Index] = false;
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
            _uiFingersLeftUI.Remove(finger.Index);

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
            // 用 StartedOverGui（按下时位置）兜底"UI 起手→轻微滑出 UI 仍被 LeanTouch 算 Tap"
            // 的边界 case；HandleFingerDown 已经按 IsOverGui 拦了主路。
            if (finger.StartedOverGui) return;
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
            Vector2 rawDelta = finger.ScreenDelta;

            // UI 起手的手指：在滑出 UI 前不产生 SingleDragDelta，否则仓库列表 / 分类 tab 滚动
            // 会触发 CinemachineController.CheckRotate 导致相机误转。
            // 世界起手的手指始终正常工作。
            bool uiFingerLeftUI = false;
            if (_uiFingersLeftUI.TryGetValue(finger.Index, out bool hasLeftUI))
            {
                if (!hasLeftUI && !finger.IsOverGui)
                {
                    // 手指刚滑出 UI，标记并触发 DragBegin
                    _uiFingersLeftUI[finger.Index] = true;
                    uiFingerLeftUI = true;
                }
                else
                {
                    uiFingerLeftUI = hasLeftUI;
                }
            }

            // 对外暴露的 delta：UI 起手但未滑出 UI → 归零；其余正常
            _singleDragDelta = (uiFingerLeftUI || !_uiFingersLeftUI.ContainsKey(finger.Index))
                ? rawDelta : Vector2.zero;

            // 检查是否达到拖动阈值（用 rawDelta 算，不受 UI 过滤影响）
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
                _currentDragPosition = currentPosition;

                // 触发拖动事件（拖拽链只看 _singleDragDelta，已被上面的逻辑过滤）
                OnSingleDrag?.Invoke(_singleDragDelta, currentPosition);
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

            if (debugPinchGestureTrace)
            {
                Debug.Log(
                    $"[PinchDbg] f={Time.frameCount} pinchCh={pinchChange:F5} centerMv={centerMoveDistance:F1} " +
                    $"gesture={_currentGesture} pinchRatio={_pinchRatio:F4} multiΔ={_multiDragDelta} fingers={_currentFingers.Count}",
                    this);
            }
        }

        private void UpdateMultiFingerData()
        {
            if (_currentFingers.Count < 2) return;

            // 计算多个手指的中心点
            Vector2 sum = Vector2.zero;
            Vector2 deltaSum = Vector2.zero;
            int activeDeltaCount = 0;
            foreach (var finger in _currentFingers)
            {
                sum += finger.ScreenPosition;
                bool hasLeftUI = !_uiFingersLeftUI.TryGetValue(finger.Index, out bool v) || v;
                if (hasLeftUI)
                {
                    deltaSum += finger.ScreenDelta;
                    activeDeltaCount++;
                }
            }
            _multiDragCenter = sum / _currentFingers.Count;
            // 只有离开 UI 的手指才产生 delta；全部仍在 UI 内时归零，不触发相机平移
            _multiDragDelta = activeDeltaCount > 0 ? deltaSum / activeDeltaCount : Vector2.zero;

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

