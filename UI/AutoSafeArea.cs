using System.Collections;
using System.Collections.Generic;
#if WX
using CLIP.Fundamental_Framework;
#endif

using UnityEngine;
using UnityEngine.UI;


namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class AutoSafeArea : MonoBehaviour
            {
                public CanvasScaler CS;
                private RectTransform _rectTransform;
                private Rect _lastSafeArea;

                private void Awake()
                {
                    _rectTransform = GetComponent<RectTransform>();

                    ApplySafeArea();
                }
                private void Update()
                {
                    ApplySafeArea();
                }


                private void ApplySafeArea()
                {
#if WX && !UNITY_EDITOR
                    _lastSafeArea = WeChat_Env.instance.ApplySafeArea(_rectTransform, CS, _lastSafeArea);
#else
                    Rect safeArea = Screen.safeArea;

                    if (safeArea != _lastSafeArea)
                    {
                        _lastSafeArea = safeArea;

                        Vector2 anchorMin = safeArea.position;
                        Vector2 anchorMax = safeArea.position + safeArea.size;

                        anchorMin.x /= Screen.width;
                        anchorMin.y /= Screen.height;
                        anchorMax.x /= Screen.width;
                        anchorMax.y /= Screen.height;

                        _rectTransform.anchorMin = anchorMin;
                        _rectTransform.anchorMax = anchorMax;
                    }
#endif

                }
            }
        }
    }
}

