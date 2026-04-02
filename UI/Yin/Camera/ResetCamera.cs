using System.Collections;
using System.Collections.Generic;
using Lean.Common;
using Lean.Touch;
using UnityEngine;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ResetCamera : MonoBehaviour
            {
                public Transform originPos;
                public LeanPitchYaw leanPitch;
                public LeanPinchCamera pinchCamera;
                public GameObject mainCamera;
                public GameObject cameraPivot;
                public GameObject origin_Pos;
                float pitch;
                float yaw;
                float zoom;
                Vector3 mainCemeraTransform;
                Vector3 cameraPivotTransform;
                Vector3 originPosTransform;

                public bool canReset = true;
                void Awake()
                {
                    //cameraPivot = GameObject.Find("Camera Pivot");
                    //mainCamera = GameObject.Find("Main Camera");
                    leanPitch = cameraPivot.GetComponent<LeanPitchYaw>();
                    pinchCamera = mainCamera.GetComponent<LeanPinchCamera>();
                    pitch = leanPitch.Pitch;
                    yaw = leanPitch.Yaw;
                    zoom = pinchCamera.Zoom;
                    mainCemeraTransform = mainCamera.transform.localPosition;
                    cameraPivotTransform = cameraPivot.transform.position;
                    originPosTransform = originPos.position;
                }

                //private void OnEnable()
                //{
                //    pitch = leanPitch.Pitch;
                //    yaw = leanPitch.Yaw;
                //    zoom = pinchCamera.Zoom;
                //    mainCemeraTransform = mainCamera.transform.localPosition;
                //    cameraPivotTransform = cameraPivot.transform.position;
                //    originPosTransform = originPos.position;
                //}

                public void Reset()
                {
                    leanPitch.Pitch = pitch;
                    leanPitch.Yaw = yaw;
                    pinchCamera.enabled = true;
                    pinchCamera.Zoom = zoom;
                    mainCamera.transform.localPosition = mainCemeraTransform;
                    cameraPivot.transform.position = cameraPivotTransform;
                    originPos.position = originPosTransform;
                }

                public void ResetRotation()
                {
                    if (!canReset)
                    {
                        return;
                    }
                    //mainCameraOb.transform.LookAt(originPos.position, Vector3.up);
                    float _dist = Vector3.Distance(mainCamera.transform.position, originPos.position);
                    mainCamera.transform.position = originPos.position - mainCamera.transform.forward * _dist;

                    //mainCameraOb.transform.rotation = Quaternion.Euler();

                    //Quaternion targetRotation = mainCameraOb.transform.localRotation;
                    //Quaternion rotation = Quaternion.Inverse(targetRotation);
                    //mainCameraOb.transform.localRotation = Quaternion.identity;
                    //cameraPivot.transform.rotation = rotation;
                }

                public void SetCanReset(bool canReset)
                {
                    this.canReset = canReset;
                }
            }
        }
    }
}
