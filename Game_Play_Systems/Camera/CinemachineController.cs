using Cinemachine;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
public class CinemachineCameraController : MonoBehaviour
{
    [Header("Cinemachine References")]
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineCameraOffset offset;
    [SerializeField] private Transform targetObject; // 要观察的物体

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private float minVerticalAngle = 10f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 20f;

    // 输入值（由外部提供）
    private Vector2 singleFingerDelta; // 单指拖动增量
    private Vector2 doubleFingerDelta; // 双指拖动增量
    private float pinchDelta;
    private float scaleRatio;

    private void Start()
    {
        if (freeLookCamera == null)
            freeLookCamera = GetComponent<CinemachineFreeLook>();

        if (freeLookCamera != null && targetObject != null)
        {
            freeLookCamera.Follow = targetObject;
            freeLookCamera.LookAt = targetObject;
        }
    }

    private void Update()
    {
        singleFingerDelta = InputManager.Instance.SingleDragDelta;
        doubleFingerDelta = InputManager.Instance.MultiDragDelta;
        pinchDelta = InputManager.Instance.PinchRatio;
        HandleCameraControl();
        CheckScale();
    }

    private void HandleCameraControl()
    {
        if (targetObject == null || freeLookCamera == null) return;

        // 单指拖动：旋转摄像机
        if (singleFingerDelta != Vector2.zero)
        {
            RotateCamera(singleFingerDelta);
        }

        // 双指拖动：平移摄像机（保持角度不变）
        if (doubleFingerDelta != Vector2.zero)
        {
            PanCamera(doubleFingerDelta);
        }
    }

    private void RotateCamera(Vector2 delta)
    {
        // 水平旋转
        freeLookCamera.m_XAxis.Value += delta.x * rotationSpeed;

        // 垂直旋转（限制角度范围）
        float newY = freeLookCamera.m_YAxis.Value + delta.y * rotationSpeed * -0.01f;
        newY = Mathf.Clamp(newY, minVerticalAngle / maxVerticalAngle, maxVerticalAngle / maxVerticalAngle);
        freeLookCamera.m_YAxis.Value = newY;
    }

    private void PanCamera(Vector2 delta)
    {
        offset.m_Offset += (Vector3)delta * panSpeed * -1;
    }
    public void CheckScale()
    {
        float input = InputManager.Instance.PinchRatio;
        scaleRatio = Mathf.Lerp(scaleRatio, input, Time.deltaTime * 10);
        freeLookCamera.m_Lens.OrthographicSize = Mathf.Lerp(scaleRatio * freeLookCamera.m_Lens.OrthographicSize, Mathf.Clamp(scaleRatio * freeLookCamera.m_Lens.OrthographicSize, 10, 30), Time.deltaTime * 10);
    }
    // 由外部调用的方法，用于传递输入值
    public void SetSingleFingerDelta(Vector2 delta)
    {
        singleFingerDelta = delta;
    }

    public void SetDoubleFingerDelta(Vector2 delta)
    {
        doubleFingerDelta = delta;
    }
}