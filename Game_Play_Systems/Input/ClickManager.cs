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

        /// <summary>小地图选房间会关 UI，同帧内 EventSystem 可能已点不到图；由 <see cref="MapPanel.SwitchRoom"/> 置位，本帧 LateUpdate 里跳过世界射线。</summary>
        private static bool s_mapRoomSwitchConsumedPick;

        public static void NotifyMapRoomSwitchConsumedPick() => s_mapRoomSwitchConsumedPick = true;

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
            bool mapTap = s_mapRoomSwitchConsumedPick;
            s_mapRoomSwitchConsumedPick = false;
            if (mapTap)
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

