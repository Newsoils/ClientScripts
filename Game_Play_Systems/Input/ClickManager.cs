using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using Lean.Touch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class ClickManager : SingletonMono<ClickManager>
    {
        public LayerMask targetLayer;
        public float maxDistance = 100;

       

        private void Update()
        {
            if (InputManager.Instance.WasSingleTapThisFrame)
            {
                CastRay(InputManager.Instance.LastTapPosition);
            }
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.LastTapPosition);
            Debug.DrawRay(ray.origin, ray.direction*100);
        }
        private void CastRay(Vector2 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            bool flag = true;
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, targetLayer);
            foreach (var hit in hits)
            {
                if (hit.collider.enabled)
                {
                    if(hit.collider.gameObject.GetComponentInParent<IClick>() != null)
                    {
                        IClick obj = hit.collider.gameObject.GetComponentInParent<IClick>();
                        if (obj.OnClick(hit.point))
                        {
                            flag = false;
                            break;
                        }
                    }
                }
            }
            if (flag)
            {
                if (InputManager.Instance.AllowTouchOnUI && IsPointerOverUIButton(screenPosition))
                    return;
                EvtDsp.TriggerEvt(EvtNames.OnClickNothing);
            }
        }

        /// <summary>当前屏幕位置是否点在 uGUI <see cref="Button"/> 上（含父级上的 Button）。</summary>
        private static bool IsPointerOverUIButton(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = screenPosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            foreach (var r in results)
            {
                if (r.gameObject.GetComponentInParent<Button>() != null)
                    return true;
            }
            return false;
        }
    }
}

