//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.Scene_View_Control;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.UI;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class Pot_World_Manipulation_Canvas : MonoBehaviour
//            {
//                public Button btn_close;
//                public Button btn_lock;
//                public Button btn_move;

//                public GameObject pot_World_Manipulation_Canvas;

//                private void Start()
//                {
//                    btn_close.onClick.AddListener(DeselectPot);
//                    btn_lock.image.color = Color.green;
//                    btn_lock.onClick.AddListener(EnableOrDisableLock);
//                    btn_move.onClick.AddListener(SelectPot);
//                }

//                public void EnableOrDisableLock()
//                {
//                    Pot_In_Scene pot_In_Scene = gameObject.GetComponentInParent<Pot_In_Scene>();
//                    if (pot_In_Scene == null)
//                    {
//                        Debug.LogError("Pot_In_Scene component not found in parent.");
//                        return;
//                    }
//                    if (pot_In_Scene.isHarvestLocked)
//                    {
//                        pot_In_Scene.isHarvestLocked = false;
//                        btn_lock.image.color = Color.green;
//                        Debug.Log("Lock is disabled");
//                    }
//                    else
//                    {
//                        pot_In_Scene.isHarvestLocked = true;
//                        btn_lock.image.color = Color.red;
//                        Debug.Log("Lock is enabled");
//                    }
//                }

//                // 取消选择
//                public void DeselectPot()
//                {
//                    Pot_In_Scene pot_In_Scene = gameObject.GetComponentInParent<Pot_In_Scene>();
//                    if (Planting_System_Manager._instance == null)
//                    {
//                        Debug.LogError("Planting_System_Manager.Instance is null");
//                        return;
//                    }
//                    Debug.Log("EnableResetCamera");
//                    Planting_System_Manager._instance.on_deselect_pot();
//                    pot_World_Manipulation_Canvas.SetActive(false);
//                    CustomEvent.Trigger(Planting_System_Manager._instance.planting_interaction_manager, "_CE_exit_moving_pot");
//                    if (pot_In_Scene != null)
//                    {
//                        pot_In_Scene.on_exit_moving_pot();
//                    }
//                    else
//                    {
//                        Debug.LogError("Pot_In_Scene component not found in parent.");
//                    }
//                }

//                public void SelectPot()
//                {
//                    Pot_In_Scene pot_In_Scene = gameObject.GetComponentInParent<Pot_In_Scene>();
//                    if (pot_In_Scene != null)
//                    {
//                        Planting_System_Manager._instance.current_selected_pot = pot_In_Scene.gameObject;
//                    }
//                }
//            }
//        }
//    }
//}

