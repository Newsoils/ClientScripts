using System.Collections;
using System.Collections.Generic;
using Lean.Common;
using UnityEngine;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class CameraMoveLimit : MonoBehaviour
            {
                public Transform centerObject;
                public Camera mainCamera;
                public LeanPitchYaw leanPitchYaw;

                public float minSize = 8f;
                public float maxSize = 18f;
                public float minRadius = 6f;
                public float maxRadius = 13f;

                private void Start()
                {
                    if (mainCamera == null)
                    {
                        mainCamera = Camera.main;
                    }
                    //leanPitchYaw = mainCamera.transform.parent.GetComponent<LeanPitchYaw>();
                }

                private void LateUpdate()
                {
                    float size = mainCamera.orthographicSize;

                    float t = Mathf.InverseLerp(minSize, maxSize, size);
                    float curRadius = Mathf.Lerp(maxRadius, minRadius, t);
                    //Debug.Log($"Size: {size}, t: {t}, Current Radius: {curRadius}");

                    float distance = Vector3.Distance(mainCamera.transform.position, centerObject.position);
                    //Debug.Log(distance);

                    Vector3 dir = (mainCamera.transform.position - centerObject.position).normalized;

                    if (distance > curRadius)
                    {
                        mainCamera.transform.position = centerObject.position + dir * curRadius;
                        //Debug.Log("超出边界");
                    }

                    //if (leanPitchYaw != null)
                    //{
                    //    // Pitch
                    //    if (leanPitchYaw.PitchClamp)
                    //    {
                    //        if (leanPitchYaw.Pitch < leanPitchYaw.PitchMin)
                    //            leanPitchYaw.Pitch = leanPitchYaw.PitchMin;
                    //        else if (leanPitchYaw.Pitch > leanPitchYaw.PitchMax)
                    //            leanPitchYaw.Pitch = leanPitchYaw.PitchMax;
                    //    }
                    //    // Yaw
                    //    if (leanPitchYaw.YawClamp)
                    //    {
                    //        if (leanPitchYaw.Yaw < leanPitchYaw.YawMin)
                    //            leanPitchYaw.Yaw = leanPitchYaw.YawMin;
                    //        else if (leanPitchYaw.Yaw > leanPitchYaw.YawMax)
                    //            leanPitchYaw.Yaw = leanPitchYaw.YawMax;
                    //    }
                    //}
                }
            }
        }
    }
}

