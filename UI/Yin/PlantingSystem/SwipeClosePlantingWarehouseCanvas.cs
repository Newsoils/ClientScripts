//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class SwipeClosePlantingWarehouseCanvas : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
//            {
//                private Vector2 pointerDownPos;
//                private Vector2 pointerUpPos;
//                public float swipeThreshold = 100f;
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
//                            PlantingInteractManager.Instance.ChangeWheelUIPosition(PlantingInteractManager.Instance.downWheelTransform);
//                            PlantingInteractManager.Instance.warehouse_Canvas_Prefab.SetActive(false);
//                            PlantingInteractManager.Instance.warehouse.SetActive(true);
//                            PlantingInteractManager.Instance.confirmModification.SetActive(false);
//                            //PlantingInteractManager.Instance.bTN_Map.SetActive(false);
//                            PlantingInteractManager.Instance.bTN_Begin_Planting.SetActive(false);
//                            PlantingInteractManager.Instance.CameraDown();
//                            PlantingInteractManager.Instance.ChangeState(new NonePlantState());
//                            Planting_System_Manager.Instance.can_edit_pot = true;
//                        }
//                    }
//                }
//            }

//        }
//    }
//}
