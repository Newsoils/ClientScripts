using Cinemachine;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class CinemachineCameraController : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("唯一相机名。\n" +
             "房间相机：请按 \"<房间名>_<state>\" 格式填，如 \"客厅_Normal\" / \"客厅_Placement\" / \"客厅_PlacementTopDown\" / \"客厅_PlacementFrontView\"，与 CameraManager.MakeRoomCameraName 一致。\n" +
             "剧情/独立相机：自由起名，如 \"story_intro\"。")]
    [SerializeField] private string cameraName;

    [Header("Capabilities")]
    [Tooltip("是否允许平移（双指拖拽）。")]
    [SerializeField] private bool canMove = true;
    [Tooltip("是否允许旋转（单指拖拽）。装修相机一般关掉。")]
    [SerializeField] private bool canRotate = true;
    [Tooltip("平视视角（PlacementFrontView）下，是否允许绕 X 轴旋转（上下摇头）。关掉时 pitch 锁定在初始值，保持平视。")]
    [SerializeField] private bool canRotatePitch = true;
    [Tooltip("是否允许缩放（双指捏合）。")]
    [SerializeField] private bool canScale = true;
    [Tooltip("是否允许双击复位。")]
    [SerializeField] private bool canReset = true;

    [Header("Cinemachine References")]
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineCameraOffset offset;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private Vector2 yAxisRange = new Vector2(0.1f, 1);

    [Header("Pan Settings")]
    [Tooltip("双指拖拽的平移速度。乘 dragDelta（像素帧增量）。")]
    [SerializeField] private float panSpeed = 5f;
    [Tooltip("Joystick 平移速度（世界单位 / 秒）。乘 joystick 归一化输入再乘 dt。")]
    [SerializeField] private float joystickPanSpeed = 8f;

    private float scaleRatio = 1f;
    private Vector2 originPos;
    private Vector3 originOffset;
    private float originOrthoSize;

    public string CameraName => cameraName;

    private void Awake()
    {
        CameraManager.Register(this);

        if (freeLookCamera == null)
            freeLookCamera = GetComponent<CinemachineFreeLook>();

        originPos = new Vector2(freeLookCamera.m_XAxis.Value, freeLookCamera.m_YAxis.Value);
        originOffset = offset.m_Offset;
        originOrthoSize = freeLookCamera.m_Lens.OrthographicSize;
    }

    private void OnDestroy()
    {
        CameraManager.Unregister(this);
    }

    private void Update()
    {
        if (CameraManager.Instance.InputDisabled) return;

        bool usingJoystick = InputManager.Instance.JoystickInput != Vector2.zero;

        if (canMove) CheckMove();
        if (canRotate && !usingJoystick) CheckRotate();
        if (canScale) CheckScale();
        if (canReset) CheckReset();
        if (canRotate) CheckGridWallFacing();
    }

    public void CheckMove()
    {
        Vector2 drag = InputManager.Instance.MultiDragDelta;
        Vector2 joy = InputManager.Instance.JoystickInput;
        if (drag == Vector2.zero && joy == Vector2.zero) return;

        Vector3 dragOffset = (Vector3)drag * panSpeed * -1f;
        Vector3 joyOffset = (Vector3)joy * joystickPanSpeed * Time.deltaTime;
        offset.m_Offset += dragOffset + joyOffset;
    }

    public void CheckRotate()
    {
        Vector2 input = InputManager.Instance.SingleDragDelta;
        if (input == Vector2.zero) return;

        freeLookCamera.m_XAxis.Value += input.x * rotationSpeed;

        if (canRotatePitch)
        {
            float newY = freeLookCamera.m_YAxis.Value + input.y * rotationSpeed * -0.01f;
            newY = Mathf.Clamp(newY, yAxisRange.x, yAxisRange.y);
            freeLookCamera.m_YAxis.Value = newY;
        }
    }

    public void CheckScale()
    {
        float input = InputManager.Instance.PinchRatio;
        scaleRatio = Mathf.Lerp(scaleRatio, input, Time.deltaTime * 10);
        freeLookCamera.m_Lens.OrthographicSize = Mathf.Lerp(
            scaleRatio * freeLookCamera.m_Lens.OrthographicSize,
            Mathf.Clamp(scaleRatio * freeLookCamera.m_Lens.OrthographicSize, 10, 30),
            Time.deltaTime * 10);
    }

    public void CheckReset()
    {
        if (InputManager.Instance.WasDoubleTapThisFrame) ResetCamera();
    }

    /// <summary>把相机的旋转 / 平移 / 缩放全部恢复到 Awake 时的初始状态。</summary>
    public void ResetCamera()
    {
        freeLookCamera.m_XAxis.Value = originPos.x;
        freeLookCamera.m_YAxis.Value = originPos.y;
        offset.m_Offset = originOffset;
        freeLookCamera.m_Lens.OrthographicSize = originOrthoSize;
        scaleRatio = 1f;
    }

        /// <summary>
        /// FrontView 旋转时刷新墙面格子的朝向过滤。
        /// </summary>
        public void CheckGridWallFacing()
        {
            if (CameraManager.Instance.State == CameraState.PlacementFrontView)
            {
                GridSystem.RefreshWallGridFacing(Camera.main.transform.forward);
            }
        }

        /// <summary>
        /// 将 FreeLook 的 yaw（m_XAxis）设到使相机水平朝向 <paramref name="worldForward"/> 方向。
        /// 用于 PlacementFrontView 相机切换时朝向"视野中心墙"。
        /// 仅当 FreeLook BindingMode 为 LockToTargetWithWorldUp（m_XAxis = 世界绝对 yaw，0° = +Z）时生效。
        /// </summary>
        public void SetYawByForward(Vector3 worldForward)
    {
        worldForward.y = 0f;
        if (worldForward.sqrMagnitude < 1e-6f) return;
        worldForward.Normalize();
        freeLookCamera.m_XAxis.Value = Mathf.Atan2(worldForward.x, worldForward.z) * Mathf.Rad2Deg;
    }
}
