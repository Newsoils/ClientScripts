using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class NpcChatHorizontalSnapAndSwipe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
            {
                public NpcChatInteractManager manager;

                public float swipeThreshold = 100f;
                private Vector2 startPos;
                private bool hasSwiped = false;

                public void OnDrag(PointerEventData eventData)
                {
                    if (hasSwiped) return;
                    float dragDelta = eventData.position.x - startPos.x;
                    if (Mathf.Abs(dragDelta) > swipeThreshold)
                    {
                        hasSwiped = true;
                        if (dragDelta < 0)
                        {
                            if (manager.npcChatUnit.info != null)
                            {
                                manager.OpenGiftScrollView();
                            }
                        }
                        else
                        {
                            manager.OpenChatScrollView();
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
    }
}