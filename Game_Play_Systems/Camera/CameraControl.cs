using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using Lean.Common;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UIElements;

namespace CLIP.Project_Mouse.UI
{

    public class CameraControl : MonoBehaviour
    {
        public LeanPitchYaw yaw;
        public Camera controlCamera;

        [Header("Movement")]
        public float MovementSpeed = 8f;
        public float dragSensitive =0.25f;
        public Transform centerPos;
        public float minSize = 8f;
        public float maxSize = 18f;
        public float minRadius = 6f;
        public float maxRadius = 13f;

        [Header("Scale")]
        public float minScaleSize;
        public float maxScaleSize;
        private float scaleRatio = 1f;

        [Header("Reset")]
        private Vector3 originPos;
        private float originScale;
        private float oringinYaw;
        private float originPitch;

        private Vector3 curOriginPos;
        private float curOriginScale;
        private float curOriginYaw;
        private float curOriginPitch;

        private void Start()
        {
            controlCamera = GetComponent<Camera>();
            InitOrigin();
        }


        #region 通用
        private void InitOrigin()
        {
            originPos = transform.localPosition;
            originScale = controlCamera.orthographicSize;
            oringinYaw = yaw.Yaw;
            originPitch = yaw.Pitch;
        }
        public void CheckMove(bool usingJoystickOnly = false)
        {
            Vector2 value = InputManager.Instance.JoystickInput;
            if(!usingJoystickOnly)
            {
                value += dragSensitive * (-InputManager.Instance.MultiDragDelta * controlCamera.orthographicSize / 30);
            }
            // 计算移动向量
            Vector3 move = (value.x * transform.right + value.y * transform.up) * MovementSpeed * Time.deltaTime;
            transform.position += move;

            //限制范围
            float size = controlCamera.orthographicSize;
            float t = Mathf.InverseLerp(minSize, maxSize, size);
            float curRadius = Mathf.Lerp(maxRadius, minRadius, t);
            float distance = Vector3.Distance(transform.position, centerPos.position);
            Vector3 dir = (transform.position - centerPos.position).normalized;

            if (distance > curRadius)
            {
                transform.position = centerPos.position + dir * curRadius;
            }
        }
        public void CheckScale()
        {
            float input = InputManager.Instance.PinchRatio;
            scaleRatio = Mathf.Lerp(scaleRatio, input, Time.deltaTime * 10);
            controlCamera.orthographicSize = Mathf.Lerp(scaleRatio * controlCamera.orthographicSize, Mathf.Clamp(scaleRatio * controlCamera.orthographicSize, minScaleSize, maxScaleSize), Time.deltaTime * 10);
        }
        public void CheckReset()
        {
            if (InputManager.Instance.WasDoubleTapThisFrame)
            {
                //Reset();
                ResetCamera();
            }
        }

        public void CheckRotate()
        {
            yaw.Rotate(InputManager.Instance.SingleDragDelta);
        }

        public void Reset()
        {
            transform.localPosition = originPos;
            controlCamera.orthographicSize = originScale;

            yaw.Yaw = oringinYaw;
            yaw.Pitch = originPitch;

            transform.localRotation = Quaternion.identity;
        }


        public void ResetCamera()
        {
            transform.localPosition = curOriginPos;
            controlCamera.orthographicSize = curOriginScale;

            yaw.Yaw = curOriginYaw;
            yaw.Pitch = originPitch;

            transform.localRotation = Quaternion.identity;
        }


        #endregion
        public void SnapToPivot(CameraState state, CameraViewData data)
        {
            yaw.enabled = data.enableYaw;
            yaw.transform.position = data.pivotOffset;
            yaw.transform.rotation = Quaternion.Euler(data.pivotEuler);
            controlCamera.orthographicSize = data.orthographicSize;
            if (data.pitch != 0) yaw.Pitch = data.pitch;
            if (data.yaw != 0) yaw.Yaw = data.yaw;
            if (state == CameraState.Normal)
            {
                controlCamera.transform.localPosition = new Vector3(0, 0, -12);
                controlCamera.transform.localRotation  = Quaternion.identity;
            }
            else
            {
                controlCamera.transform.position = data.positionOffset;
                controlCamera.transform.rotation = Quaternion.Euler(data.euler);
            }

          

            curOriginPos = data.pivotOffset;
            curOriginScale = data.orthographicSize;
            curOriginYaw = data.pivotEuler.y;
            curOriginPitch = data.pivotEuler.x;
        }

    }

}