using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Custom_Tool
        {
            public class Collider_Min_Display : MonoBehaviour
            {
                public Collider _coliider;

                public void OnEnable()
                {
                    _coliider = GetComponent<Collider>();
                }
                public void OnDrawGizmos()
                {
                    if (_coliider != null)
                    {
                        Gizmos.color = Color.red;
                     
                     
                        Gizmos.DrawCube(_coliider.bounds.min, Vector3.one);
                    }
                    else
                    {
                        Debug.LogWarning("Collider_Min_Display: No collider assigned.");
                    }
                }
            }
        }
    }
}

