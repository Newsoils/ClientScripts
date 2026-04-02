using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class HorizontalSnapAndSwipe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float swipeThreshold = 100f;
        private Vector2 startPos;
        private bool hasSwiped = false;

        public void OnDrag(PointerEventData eventData)
        {
            var friendChatPanel = UIManager.Instance.GetPanel<SocialPanel>().friendChatPanel;
            if (friendChatPanel == null) return;

            if (hasSwiped) return;
            float dragDelta = eventData.position.x - startPos.x;
            if (Mathf.Abs(dragDelta) > swipeThreshold)
            {
                hasSwiped = true;
                if (dragDelta < 0)
                {
                    friendChatPanel.ShowGiftView();
                }
                else
                {
                    friendChatPanel.ShowChatView();
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            startPos = eventData.position;
            hasSwiped = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            hasSwiped = false;
        }
    }

}
