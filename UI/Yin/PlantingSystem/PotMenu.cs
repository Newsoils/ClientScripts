//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using CLIP.Project_Mouse.Scene_View_Control;
//using UnityEngine;
//using UnityEngine.UI;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class PotMenu : MonoBehaviour
//            {
//                public Pot_In_Scene curentPot;
//                public Button btn_lock;
//                private void OnEnable()
//                {
//                    GameObject pot = Planting_System_Manager.Instance.current_selected_pot;
//                    if (pot != null)
//                    {
//                        curentPot = pot.GetComponent<Pot_In_Scene>();
//                    }
//                }

//                private void Update()
//                {
//                     if (curentPot != null)
//                    {
//                        Camera mainCamera = Camera.main;
//                        if (mainCamera == null)
//                        {
//                            Debug.LogWarning("主摄像机未找到，无法更新 UI 位置！");
//                            return;
//                        }

//                        Vector3 screenPosition = mainCamera.WorldToScreenPoint(curentPot._collider.transform.position);
//                        if (screenPosition.z <= 0)
//                        {
//                            Debug.LogWarning("物体在摄像机后方，无法转换屏幕坐标！");
//                            return;
//                        }
//                        Vector2 centeredPosition = new Vector2(screenPosition.x - (Screen.width / 2), screenPosition.y - (Screen.height / 2) + 150);
//                        // 更新 UI 的位置
//                        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
//                        rectTransform.anchoredPosition = centeredPosition;
//                    }
//                    else
//                    {
//                        gameObject.SetActive(false);
//                        return;
//                    }
//                }

//                public void DeletePot()
//                {
//                    if (curentPot != null)
//                    {
//                        Pot_In_Scene pot_In_Scene = null;
//                        GameObject pot = Planting_System_Manager.Instance.current_selected_pot;
//                        if (pot != null)
//                        {
//                            pot_In_Scene = pot.GetComponent<Pot_In_Scene>();
//                        }
//                        if (pot_In_Scene != null)
//                        {
//                            Planting_System_Manager.Instance.try_remove_pot_from_level(pot_In_Scene);
//                        }
//                    }
//                }

//                public void RotatePot()
//                {
//                    Pot_In_Scene pot_In_Scene = null;
//                    GameObject pot = Planting_System_Manager.Instance.current_selected_pot;
//                    if (pot != null)
//                    {
//                        pot_In_Scene = pot.GetComponent<Pot_In_Scene>();
//                    }
//                    if (pot_In_Scene != null)
//                    {
//                        StartCoroutine(RotateCo(pot_In_Scene));
//                    }
//                }

//                public IEnumerator RotateCo(Pot_In_Scene pot_In_Scene)
//                {
//                    pot_In_Scene.try_rotate_pot(1);

//                    yield return new WaitForSeconds(0.5f);

//                    PlantingInteractManager.Instance.ShowGrid(pot_In_Scene.gameObject);
//                }

//                public void EnableOrDisableLock()
//                {
//                    Pot_In_Scene pot_In_Scene = null;
//                    GameObject pot = Planting_System_Manager.Instance.current_selected_pot;
//                    if (pot != null)
//                    {
//                        pot_In_Scene = pot.GetComponent<Pot_In_Scene>();
//                    }
//                    if (pot_In_Scene != null)
//                    {
//                        if (pot_In_Scene.isHarvestLocked)
//                        {
//                            pot_In_Scene.isHarvestLocked = false;
//                            btn_lock.image.color = Color.green;
//                            Debug.Log("Lock is disabled");
//                        }
//                        else
//                        {
//                            pot_In_Scene.isHarvestLocked = true;
//                            btn_lock.image.color = Color.red;
//                            Debug.Log("Lock is enabled");
//                        }
//                    }
//                }

//                public void OnDisable()
//                {
//                    if (curentPot != null)
//                    {
//                        curentPot = null;
//                    }
//                }
//            }
//        }
//    }
//}

