using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class ClickManager : SingletonMono<ClickManager>
    {
        public LayerMask targetLayer;
        public float maxDistance = 100;

        /// <summary>UI 同帧关闭后 EventSystem 可能已射不到原 UI；置位后本帧 LateUpdate 跳过世界射线，避免点击穿透到门/家具。</summary>
        private static bool s_uiConsumedPick;

        public static void NotifyUiConsumedPick() => s_uiConsumedPick = true;
        public static void NotifyMapRoomSwitchConsumedPick() => NotifyUiConsumedPick();

        private void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.LastTapPosition);
            Debug.DrawRay(ray.origin, ray.direction * 100);
        }

        /// <summary>世界点击在 LateUpdate，避免与 uGUI 同帧抢顺序（地图切房后仍用同一 WasSingleTap 打到花盆）。
        /// UI 上的 tap 永远不打世界射线——InputManager 已经用 StartedOverGui 过滤了 UI 起手的 Tap。</summary>
        private void LateUpdate()
        {
            if (!InputManager.Instance.WasSingleTapThisFrame)
                return;
            bool uiConsumedPick = s_uiConsumedPick;
            s_uiConsumedPick = false;
            if (uiConsumedPick)
                return;
            if (IsScreenPositionOverRaycastableUi(InputManager.Instance.LastTapPosition))
                return;
            CastRay(InputManager.Instance.LastTapPosition);
        }

        private void CastRay(Vector2 screenPosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            bool flag = true;
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, targetLayer);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
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
                EvtDsp.TriggerEvt(EvtNames.OnClickNothing);
            }
        }

        private static bool IsScreenPositionOverRaycastableUi(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = screenPosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            return results.Count > 0;
        }
    }
}

