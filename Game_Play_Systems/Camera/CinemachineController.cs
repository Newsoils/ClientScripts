using Cinemachine;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
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

    private float scaleRatio;
    private Vector2 originPos;

    private void Start()
    {
        if (freeLookCamera == null)
            freeLookCamera = GetComponent<CinemachineFreeLook>();

        if (freeLookCamera != null && targetObject != null)
        {
            freeLookCamera.Follow = targetObject;
            freeLookCamera.LookAt = targetObject;
        }
        originPos = new Vector2(freeLookCamera.m_XAxis.Value, freeLookCamera.m_YAxis.Value);
    }
    public void CheckMove()
    {
        Vector2 input = InputManager.Instance.MultiDragDelta;
        if(input != Vector2.zero)
        {
            offset.m_Offset += (Vector3)input * panSpeed * -1;
        }
    }
    public void CheckRotate()
    {
        Vector2 input = InputManager.Instance.SingleDragDelta;
        if(input != Vector2.zero)
        {
            // 水平旋转
            freeLookCamera.m_XAxis.Value += input.x * rotationSpeed;

            // 垂直旋转（限制角度范围）
            float newY = freeLookCamera.m_YAxis.Value + input.y * rotationSpeed * -0.01f;
            newY = Mathf.Clamp(newY, 0.5f, 1);
            freeLookCamera.m_YAxis.Value = newY;
        }
    }
    public void CheckScale()
    {
        float input = InputManager.Instance.PinchRatio;
        scaleRatio = Mathf.Lerp(scaleRatio, input, Time.deltaTime * 10);
        freeLookCamera.m_Lens.OrthographicSize = Mathf.Lerp(scaleRatio * freeLookCamera.m_Lens.OrthographicSize, Mathf.Clamp(scaleRatio * freeLookCamera.m_Lens.OrthographicSize, 10, 30), Time.deltaTime * 10);
    }
    public void CheckReset()
    {
        bool input = InputManager.Instance.WasDoubleTapThisFrame;
        if (input)
        {
            freeLookCamera.m_XAxis.Value = originPos.x;
            freeLookCamera.m_YAxis.Value = originPos.y;
        }
    }
}