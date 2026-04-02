using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Graphics
        {
            [ExecuteAlways]
            public class Billboard_Effect : MonoBehaviour
            {
               
                void Start()
                {
                    //中文测试
                }

                // Update is called once per frame
                void Update()
                {

                }

                private void FixedUpdate()
                {
                    Vector3 _camera_forward = Camera.main.transform.forward;
                    var _rot = quaternion.LookRotation(_camera_forward, math.up());
                    this.transform.rotation = _rot;
                }
            }
        }
    }
}