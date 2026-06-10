using DG.Tweening;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class DispatchDeskPageController : MonoBehaviour
{
    public Transform leftPageItems;
    public Transform rightPageItems;
    public CD3DCarousel cdCarousel;
    public Transform itemPendant;
    public Transform itemFood;
    public Camera controlledCamera;
    public CinemachineVirtualCamera bagVirtualCamera;
    public CinemachineVirtualCamera leftVirtualCamera;
    public CinemachineVirtualCamera rightVirtualCamera;
    public string bagVirtualCameraName = "DispatchDesk_BagCam";
    public string leftVirtualCameraName = "DispatchDesk_LeftVCam";
    public string rightVirtualCameraName = "DispatchDesk_RightVCam";
    public Vector3 cameraLeftPosition;
    public Vector3 cameraLeftEulerAngles;
    public float cameraLeftOrthographicSize = 14f;
    public Vector3 cameraRightPosition;
    public Vector3 cameraRightEulerAngles;
    public float cameraRightOrthographicSize = 14f;
    public bool useLegacyCameraInterpolation;

    public Vector3 leftItemsExitOffset = new Vector3(0f, 15f, 0f);
    public float cdEntryOffset = 5f;
    [Range(0f, 0.5f)]
    public float staggerAmount = 0.15f;
    public float snapDuration = 0.6f;
    public float pageProgress;

    private Vector3 pendantStartLocalPosition;
    private Vector3 foodStartLocalPosition;
    private bool hasStartPositions;
    private bool isTweening;
    private CinemachineBrain controlledBrain;
    private CinemachineBlenderSettings originalCustomBlends;
    private CinemachineBlenderSettings runtimeCustomBlends;

    private void Awake()
    {
        ResolveControlledCamera();
        ResolveVirtualCameras();
        ConfigureDirectionalCameraBlend();
        CacheStartPositions();
        ApplyProgress(pageProgress);
        ShowBagCamera();
    }

    private void OnDestroy()
    {
        if (controlledBrain != null && controlledBrain.m_CustomBlends == runtimeCustomBlends)
            controlledBrain.m_CustomBlends = originalCustomBlends;

        if (runtimeCustomBlends != null)
            Destroy(runtimeCustomBlends);
    }

    public void ShowBagCamera()
    {
        ResolveVirtualCameras();
        SetVirtualCameraPriorities(bag: 20, left: 0, right: 0);
    }

    public void SwitchToPage(int pageIndex)
    {
        if (isTweening)
            return;

        float targetProgress = Mathf.Clamp01(pageIndex);
        SwitchVirtualCamera(targetProgress);
        isTweening = true;

        DOTween.To(() => pageProgress, ApplyProgress, targetProgress, snapDuration)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => isTweening = false);
    }

    public void ApplyProgress(float progress)
    {
        CacheStartPositions();

        pageProgress = Mathf.Clamp01(progress);

        float pendantProgress = Mathf.Clamp01(pageProgress / (1f - staggerAmount));
        float foodProgress = Mathf.Clamp01((pageProgress - staggerAmount) / (1f - staggerAmount));

        if (itemPendant != null)
            itemPendant.localPosition = Vector3.Lerp(pendantStartLocalPosition, pendantStartLocalPosition + leftItemsExitOffset, pendantProgress);

        if (itemFood != null)
            itemFood.localPosition = Vector3.Lerp(foodStartLocalPosition, foodStartLocalPosition + leftItemsExitOffset, foodProgress);

        if (rightPageItems != null && pageProgress > 0.001f && !rightPageItems.gameObject.activeSelf)
            rightPageItems.gameObject.SetActive(true);

        if (cdCarousel != null)
        {
            cdCarousel.scrollProgress = (1f - pageProgress) * cdEntryOffset;
            cdCarousel.UpdateCDPositions();
        }

        if (useLegacyCameraInterpolation && leftVirtualCamera == null && rightVirtualCamera == null)
        {
            ResolveControlledCamera();
            if (controlledCamera != null)
            {
                controlledCamera.transform.position = Vector3.Lerp(cameraLeftPosition, cameraRightPosition, pageProgress);
                controlledCamera.transform.rotation = Quaternion.Euler(Vector3.Lerp(cameraLeftEulerAngles, cameraRightEulerAngles, pageProgress));
                controlledCamera.orthographicSize = Mathf.Lerp(cameraLeftOrthographicSize, cameraRightOrthographicSize, pageProgress);
            }
        }
    }

    private void CacheStartPositions()
    {
        if (hasStartPositions)
            return;

        if (itemPendant != null)
            pendantStartLocalPosition = itemPendant.localPosition;

        if (itemFood != null)
            foodStartLocalPosition = itemFood.localPosition;

        hasStartPositions = true;
    }

    private void ResolveControlledCamera()
    {
        if (controlledCamera != null)
            return;

        controlledCamera = Camera.main;
    }

    private void ConfigureDirectionalCameraBlend()
    {
        if (controlledCamera == null || bagVirtualCamera == null || leftVirtualCamera == null)
            return;

        controlledBrain = controlledCamera.GetComponent<CinemachineBrain>();
        if (controlledBrain == null)
            return;

        originalCustomBlends = controlledBrain.m_CustomBlends;
        runtimeCustomBlends = ScriptableObject.CreateInstance<CinemachineBlenderSettings>();

        var blends = new List<CinemachineBlenderSettings.CustomBlend>();
        if (originalCustomBlends != null && originalCustomBlends.m_CustomBlends != null)
            blends.AddRange(originalCustomBlends.m_CustomBlends);

        string fromName = bagVirtualCamera.gameObject.name;
        string toName = leftVirtualCamera.gameObject.name;
        blends.RemoveAll(blend => blend.m_From == fromName && blend.m_To == toName);
        blends.Add(new CinemachineBlenderSettings.CustomBlend
        {
            m_From = fromName,
            m_To = toName,
            m_Blend = new CinemachineBlendDefinition
            {
                m_Style = CinemachineBlendDefinition.Style.Cut,
                m_Time = 0f
            }
        });

        runtimeCustomBlends.m_CustomBlends = blends.ToArray();
        controlledBrain.m_CustomBlends = runtimeCustomBlends;
    }

    private void SwitchVirtualCamera(float targetProgress)
    {
        ResolveVirtualCameras();
        if (leftVirtualCamera == null || rightVirtualCamera == null)
            return;

        bool rightPage = targetProgress >= 0.5f;
        SetVirtualCameraPriorities(bag: 0, left: rightPage ? 0 : 20, right: rightPage ? 20 : 0);
    }

    private void SetVirtualCameraPriorities(int bag, int left, int right)
    {
        if (bagVirtualCamera != null)
            bagVirtualCamera.Priority = bag;

        if (leftVirtualCamera != null)
            leftVirtualCamera.Priority = left;

        if (rightVirtualCamera != null)
            rightVirtualCamera.Priority = right;
    }

    private void ResolveVirtualCameras()
    {
        if (bagVirtualCamera == null && !string.IsNullOrEmpty(bagVirtualCameraName))
        {
            var bag = GameObject.Find(bagVirtualCameraName);
            if (bag != null)
                bagVirtualCamera = bag.GetComponent<CinemachineVirtualCamera>();
        }

        if (leftVirtualCamera == null && !string.IsNullOrEmpty(leftVirtualCameraName))
        {
            var left = GameObject.Find(leftVirtualCameraName);
            if (left != null)
                leftVirtualCamera = left.GetComponent<CinemachineVirtualCamera>();
        }

        if (rightVirtualCamera == null && !string.IsNullOrEmpty(rightVirtualCameraName))
        {
            var right = GameObject.Find(rightVirtualCameraName);
            if (right != null)
                rightVirtualCamera = right.GetComponent<CinemachineVirtualCamera>();
        }
    }
}
