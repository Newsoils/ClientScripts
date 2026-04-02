//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.EventSystems;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class SwipeCloseIndoorRoomWarehouseCanvas : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
//            {
//                private Vector2 pointerDownPos;
//                private Vector2 pointerUpPos;
//                public float swipeThreshold = 100f;
//                public IndoorRoomInteractManager indoorRoomInteractManager;
//                public RectTransform validRectTransform;
//                public bool isPointerDownInRect = false;

//                public void OnPointerDown(PointerEventData eventData)
//                {
//                    isPointerDownInRect = false;
//                    if (validRectTransform != null)
//                    {
//                        Vector2 localPoint;
//                        var canvas = validRectTransform.GetComponentInParent<Canvas>();
//                        if (canvas != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
//                            validRectTransform, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localPoint))
//                        {
//                            if (validRectTransform.rect.Contains(localPoint))
//                            {
//                                pointerDownPos = eventData.position;
//                                isPointerDownInRect = true;
//                            }
//                        }
//                    }
//                    else
//                    {
//                        pointerDownPos = eventData.position;
//                        isPointerDownInRect = true;
//                    }
//                }

//                public void OnPointerUp(PointerEventData eventData)
//                {
//                    if (!isPointerDownInRect) return;

//                    pointerUpPos = eventData.position;
//                    if (Vector2.Distance(pointerDownPos, pointerUpPos) > swipeThreshold)
//                    {
//                        Vector2 swipeDirection = pointerUpPos - pointerDownPos;
//                        if (swipeDirection.y < 0 && Mathf.Abs(swipeDirection.y) > Mathf.Abs(swipeDirection.x))
//                        {
//                            indoorRoomInteractManager.warehouse_Canvas_Prefab.SetActive(false);
//                            indoorRoomInteractManager.warehouse.SetActive(true);
//                            indoorRoomInteractManager.confirmModification.SetActive(false);
//                            indoorRoomInteractManager.CameraDown();
//                            //indoorRoomInteractManager.leanMultiUpdate.enabled = true;
//                            //Indoor_Room_Game_Manager._activc_instance._on_exit_add_placement();
//                            indoorRoomInteractManager.ChangeWheelUIPosition(indoorRoomInteractManager.downWheelTransform);
//                        }
//                    }
//                }
//            }
//        }
//    }
//}

