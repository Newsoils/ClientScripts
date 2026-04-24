//using System;
//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Client_Event_Systems;
//using CLIP.Project_Mouse.ENUM;
//using CLIP.Project_Mouse.Game_Play_System;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using CLIP.Project_Mouse.Scene_View_Control;
//using Lean.Common;
//using Lean.Touch;
//using MoreMountains.Tools;
//using UnityEngine;
//using UnityEngine.UI;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
//using CLIP.Framework_Core.Event;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class PlantingInteractManager : MonoBehaviour
//            {
//                public static PlantingInteractManager Instance { get; private set; }

//                [Header("UI面板")]
//                //public GameObject bTN_Map;
//                public GameObject bTN_Begin_Planting;
//                public GameObject bTN_Close_All;
//                public GameObject panel_Planting_Function;
//                public GameObject warehouse_Canvas_Prefab;
//                public GameObject exitPlanting;
//                public GameObject giveUpPlanting;
//                public GameObject confirmModification;
//                public GameObject warehouse;
//                //public UI_Control_Warehouse_Panel uI_Control_Warehouse_Panel;
//                public BoxCollider balconyDoor;
//                public GameObject hanging_Pot_Grid;
//                public GameObject normal_Pot_Grid;
//                public GameObject balconyCanvas;
//                public GameObject harvestPlant;
//                public GameObject removeWeed;
//                public GameObject removePlant;
//                public GameObject testButton1;
//                public GameObject testButton2;
//                public GameObject wheel;
//                public Transform upWheelTransform;
//                public Transform downWheelTransform;

//                [Header("粒子特效")]
//                public ParticleSystem wateringParticle;
//                public ParticleSystem removeWeedParticle;

//                [Header("摄像机组件")]
//                public LeanPitchYaw leanPitch;
//                public LeanMultiUpdate leanMultiUpdate;
//                public GameObject doubleTapToResetCamera;
//                public CameraMoveLimit cameraMoveLimit;
//                public Transform centerObject;
//                public Camera balconyCamera;
//                public CustomRaycastTool customRaycastTool;

//                [Header("花盆虚影")]
//                public Transform ghostTransform;
//                public Transform ghostTransformHang;
//                public Transform ghostRoot;
//                public GameObject currentPreGhostPot;
//                public Pot_Type currentPreGhostPotType;
//                public Dictionary<string, GameObject> ghostObjects = new Dictionary<string, GameObject>();

//                [Header("移动花盆")]
//                public LeanFinger activeFinger;
//                public LayerMask layerMask;
//                public Game_Grid_Cell previousGridCell;
//                public Game_Grid_Cell currentGridCell;

//                [Header("选择花盆")]
//                public GameObject selectedObject;
//                public string selectedObjectName;
//                public Vector3 originalPosition;

//                [Header("长按选择家具")]
//                public GameObject longPressTarget;
//                public Pot_In_Scene longPressPot;
//                public float pressTime;
//                public float longPressThreshold = 0.5f;

//                [Header("Event System")]
//                public GameItem_DB_SO Game_Inventory_SO_01;
//                public General_Interaction_Event_Hub_SO generalInteractionEventHub;
//                public Indoor_System_Event_Hub_SO _indoor_event;
//                public Warehouse_UI_Event_Hub_SO _warehouse_UI_event_hub;

//                [Header("Planting state")]
//                public bool isPlantingState;
//                public IState curState;
//                public string state;

//                public GameObject main_Character_AI_Control;
//                public Collider testHit;
//                public LayerMask removeWeedLayer;

//                private void Awake()
//                {
//                    if (Instance != null && Instance != this)
//                    {
//                        Destroy(gameObject);
//                        return;
//                    }
//                    Instance = this;
//                }


//                private void OnEnable()
//                {
//                    ChangeState(new NonePlantState());

//                    StartCoroutine(FindObjectsWithDelay());
//                    main_Character_AI_Control = GameObject.Find("Main_Character_20250820_ver_2");
//                    ChangeWheelUIPosition(downWheelTransform);
//                }

//                private IEnumerator FindObjectsWithDelay()
//                {
//                    yield return new WaitForSeconds(0.1f);
//                    UnsubscribeEventsAndButtons();
//                    bTN_Close_All.GetComponent<Button>().onClick.AddListener(ExitPlanting);

//                    if (generalInteractionEventHub != null)
//                    {
//                        //generalInteractionEventHub._on_put_down_flower_pot.AddListener(TryPutDownPot);
//                        generalInteractionEventHub._on_check_harvest_state_and_prompt = CheckHarvestStateAndPrompt;
//                        generalInteractionEventHub._on_show_up_prompt.AddListener(PromptMessage.Instance.ShowUpPrompt);
//                        generalInteractionEventHub._on_show_prompt.AddListener(PromptMessage.Instance.ShowPrompt);
//                        generalInteractionEventHub._on_enable_or_disable_double_tap_reset_camera.AddListener(EnableORDisableCameraReset);
//                        generalInteractionEventHub._on_show_grid.AddListener(ShowGrid);
//                        generalInteractionEventHub._on_close_pot_ghost.AddListener(CloseGhost);
//                        generalInteractionEventHub._on_set_lean_multi_update.AddListener(SetLeanMultiUpdate);
//                        generalInteractionEventHub._on_change_none_state.AddListener(ChangeNoneState);
//                        generalInteractionEventHub._on_set_select_pot.AddListener(SetSelectPot);
//                        generalInteractionEventHub._on_deselect_pot.AddListener(OnDeselectPot);
//                        generalInteractionEventHub._on_set_lean_pitch.AddListener(SetLeanPitch);
//                        generalInteractionEventHub._on_swipe_close_planting_warehouse_canvas.AddListener(SwipeClosePlantingWarehouseCanvas);
//                    }


//                    _warehouse_UI_event_hub._selected_item_change.AddListener(DisplayPotGhost);
//                    _warehouse_UI_event_hub._first_level_search_tag_change.AddListener(OnFirstLevelSearchTagChange);

//                    //EvtDsp.AddEvt<string>(EvtNames.On_Select_Item_Change, DisplayPotGhost);
//                    //EvtDsp.AddEvt<string>(EvtNames.First_Level_Search_Tag_Change, OnFirstLevelSearchTagChange);

//                }

//                private void UnsubscribeEventsAndButtons()
//                {
//                    if (bTN_Close_All != null)
//                        bTN_Close_All.GetComponent<Button>().onClick.RemoveListener(ExitPlanting);

//                    // 取消事件订阅
//                    if (generalInteractionEventHub != null)
//                    {
//                        //generalInteractionEventHub._on_put_down_flower_pot.RemoveListener(TryPutDownPot);
//                        generalInteractionEventHub._on_show_up_prompt.RemoveListener(PromptMessage.Instance.ShowUpPrompt);
//                        generalInteractionEventHub._on_show_prompt.RemoveListener(PromptMessage.Instance.ShowPrompt);
//                        generalInteractionEventHub._on_enable_or_disable_double_tap_reset_camera.RemoveListener(EnableORDisableCameraReset);
//                        generalInteractionEventHub._on_show_grid.RemoveListener(ShowGrid);
//                        generalInteractionEventHub._on_close_pot_ghost.RemoveListener(CloseGhost);
//                        generalInteractionEventHub._on_set_lean_multi_update.RemoveListener(SetLeanMultiUpdate);
//                        generalInteractionEventHub._on_change_none_state.RemoveListener(ChangeNoneState);
//                        generalInteractionEventHub._on_set_select_pot.RemoveListener(SetSelectPot);
//                        generalInteractionEventHub._on_deselect_pot.RemoveListener(OnDeselectPot);
//                        generalInteractionEventHub._on_set_lean_pitch.RemoveListener(SetLeanPitch);
//                        generalInteractionEventHub._on_swipe_close_planting_warehouse_canvas.RemoveListener(SwipeClosePlantingWarehouseCanvas);
//                    }

//                    //EvtDsp.RemoveEvt<string>(EvtNames.On_Select_Item_Change, DisplayPotGhost);
//                    //EvtDsp.RemoveEvt<string>(EvtNames.First_Level_Search_Tag_Change, OnFirstLevelSearchTagChange);
//                    _warehouse_UI_event_hub._selected_item_change.RemoveListener(DisplayPotGhost);
//                    _warehouse_UI_event_hub._first_level_search_tag_change.RemoveListener(OnFirstLevelSearchTagChange);
//                }

//                // 修改滚轮位置
//                public void ChangeWheelUIPosition(Transform targetTransform)
//                {
//                    if (wheel == null || targetTransform == null) return;

//                    RectTransform wheelRect = wheel.GetComponent<RectTransform>();
//                    RectTransform upWheelRect = targetTransform.GetComponent<RectTransform>();

//                    if (wheelRect == null || upWheelRect == null) return;

//                    wheelRect.anchorMin = upWheelRect.anchorMin;
//                    wheelRect.anchorMax = upWheelRect.anchorMax;
//                    wheelRect.pivot = upWheelRect.pivot;
//                    wheelRect.anchoredPosition = upWheelRect.anchoredPosition;

//                    var joystick = wheel.GetComponentInChildren<MMTouchJoystick>();
//                    if (joystick != null)
//                    {
//                        joystick.SetNeutralPosition();
//                    }
//                }

//                public void SwipeClosePlantingWarehouseCanvas()
//                {
//                    ChangeWheelUIPosition(downWheelTransform);
//                    warehouse_Canvas_Prefab.SetActive(false);
//                    warehouse.SetActive(true);
//                    //confirmModification.SetActive(false);
//                    //bTN_Map.SetActive(false);
//                    bTN_Begin_Planting.SetActive(false);
//                    CameraDown();
//                    ChangeState(new NonePlantState());
//                    Planting_System_Manager.Instance.can_edit_pot = true;
//                }

//                public void SetLeanPitch(int x)
//                {
//                    if (leanPitch != null)
//                    {
//                        leanPitch.Pitch = x;
//                    }
//                }

//                public void OnDeselectPot()
//                {
//                    selectedObject = null;
//                    selectedObjectName = null;
//                }

//                public void SetSelectPot(GameObject pot)
//                {
//                    selectedObject = pot;
//                    if (selectedObject != null)
//                    {
//                        selectedObjectName = selectedObject.name;
//                        originalPosition = selectedObject.transform.position;
//                    }
//                }

//                public void ChangeNoneState()
//                {
//                    ChangeState(new NonePlantState());
//                }

//                public void OnFirstLevelSearchTagChange(string tag)
//                {
//                    if (tag == "Pot")
//                    {
//                        ChangeState(new PlantingState());
//                    }
//                    else if (tag == "Seed")
//                    {
//                        ChangeState(new AddSeedState());
//                    }
//                    else if (tag == "Fertilizer")
//                    {
//                        ChangeState(new AddFertilizerState());
//                    }
//                }

//                // 设置是否启用滑动旋转房间
//                public void SetLeanMultiUpdate(bool enable)
//                {
//                    if (leanMultiUpdate != null)
//                    {
//                        leanMultiUpdate.enabled = enable;
//                        //Debug.Log("leanMultiUpdate.enabled = " + enable);
//                    }
//                }

//                public void SetUIOnVisitFriendRoom(bool visible)
//                {
//                    bTN_Begin_Planting.SetActive(visible);
//                    harvestPlant.SetActive(visible);
//                    removeWeed.SetActive(visible);
//                    removePlant.SetActive(visible);
//                    //testButton1.SetActive(visible);
//                    //testButton2.SetActive(visible);
//                }

//                private void Update()
//                {
//                    if (Player_Social_Manager.Instance != null && Player_Social_Manager.Instance._on_visit_friend_room)
//                    {
//                        SetUIOnVisitFriendRoom(false);
//                    }
//                    else
//                    {
//                        SetUIOnVisitFriendRoom(true);
//                    }

//                    if (Camera.main == balconyCamera)
//                    {
//                        if (!isPlantingState)
//                        {
//                            isPlantingState = true;
//                        }
//                    }
//                    else
//                    {
//                        if (isPlantingState)
//                        {
//                            isPlantingState = false;
//                            if (curState is not NonePlantState)
//                            {
//                                ChangeState(new NonePlantState());
//                            }
//                        }
//                    }

//                    if (curState != null)
//                        curState.UpdateState(this);

//                    //if (Planting_System_Manager.Instance.is_temp)
//                    //{
//                    //    balconyDoor.enabled = false;
//                    //}

//#if UNITY_EDITOR || UNITY_STANDALONE
//                    if (LeanTouch.Fingers.Count == 2)
//                    {
//                        var finger = LeanTouch.Fingers[1];
//                        SelectPlacement(finger);
//                        MoveSelectObject(finger);
//                        MoveGhostObject(finger);
//                    }
//#endif

//#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//                        if (LeanTouch.Fingers.Count == 1)
//                        {
//                            var finger = LeanTouch.Fingers[0];
//                            SelectPlacement(finger);
//                            MoveSelectObject(finger);
//                            MoveGhostObject(finger);
//                        }
//#endif
//                }

//                // 手机按钮
//                public void OpenPhone()
//                {
//                    if (isPlantingState && !(curState is NonePlantState))
//                    {
//                        PromptMessage.Instance.ShowPrompt(3, () =>
//                        {
//                            StartCoroutine(OpenPhoneCo());
//                        });
//                    }
//                }

//                public IEnumerator OpenPhoneCo()
//                {
//                    ExitPlanting();

//                    yield return new WaitForSeconds(0.1f);

//                    balconyCanvas.SetActive(false);

//                    CommonInteractManager.Instance.OpenPhone();
//                }

//                // 地图按钮
//                //public void OpenMap()
//                //{
//                //    if (isPlantingState && !(curState is NonePlantState))
//                //    {
//                //        PromptMessage.Instance.ShowPrompt(6, () =>
//                //        {
//                //            // 进入地图界面
//                //        });
//                //    }
//                //}

//                // 任务按钮
//                public void OpenAssignment()
//                {
//                    if (isPlantingState && !(curState is NonePlantState))
//                    {
//                        PromptMessage.Instance.ShowPrompt(4, () =>
//                        {
//                            // 进入任务界面
//                        });
//                    }
//                }

//                // 相机按钮
//                public void OpenTakePhoto()
//                {
//                    if (isPlantingState && !(curState is NonePlantState))
//                    {
//                        PromptMessage.Instance.ShowPrompt(5, () =>
//                        {
//                            // 进入相机界面
//                        });
//                    }
//                }

//                // 切换状态机状态
//                public void ChangeState(IState newState)
//                {
//                    if (curState != null)
//                        curState.ExitState(this);
//                    curState = newState;
//                    state = curState.GetType().Name;
//                    if (curState != null)
//                        curState.EnterState(this);
//                }

//                // 仓库按钮
//                public void OpenWarehouse()
//                {
//                    ChangeWheelUIPosition(upWheelTransform);
//                    warehouse_Canvas_Prefab.SetActive(true);
//                    warehouse.SetActive(false);
//                    //confirmModification.SetActive(true);
//                    //bTN_Map.SetActive(false);
//                    bTN_Begin_Planting.SetActive(false);
//                    CameraUp();
//                }

//                // 种植布置按钮
//                public void BeginPlanting()
//                {
//                    ChangeWheelUIPosition(upWheelTransform);
//                    bTN_Begin_Planting.SetActive(false);
//                    CommonInteractManager.Instance.SetMainFunctionActive(false);
//                    warehouse_Canvas_Prefab.SetActive(true);
//                    Close_panel_Planting_Function();
//                    generalInteractionEventHub._invoke_on_enable_or_disable_reset_camera(false);
//                    SetIsTemp(true);
//                    exitPlanting.SetActive(true);
//                    giveUpPlanting.SetActive(true);
                 
//                    CameraUp();
//                    balconyDoor.enabled = false;
//                    ChangeState(new PlantingState());

//                    //Planting_System_Manager.Instance.saving_current_state_to_disk_as_json("planting.json");
//                    Planting_System_Manager.Instance.save_current_state_to_memory();
//                    Global_Home_Room_Manager.Instance._can_switch_scene = false;
//                    main_Character_AI_Control.SetActive(false);

//                    Planting_System_Manager.Instance.can_edit_pot = true;
//                }

//                // 关闭右侧种植功能面板和任务拍照面板
//                public void Close_panel_Planting_Function()
//                {
//                    if (panel_Planting_Function != null)
//                    {
//                        panel_Planting_Function.SetActive(false);
//                    }
//                }


//                // 打开右侧种植功能面板和任务拍照面板
//                public void Open_panel_Planting_Function()
//                {
//                    if (panel_Planting_Function != null)
//                    {
//                        panel_Planting_Function.SetActive(true);
//                    }
//                }

//                // 房间上移并关闭移动限制并开启移动限制
//                public void CameraUp()
//                {
//                    //cameraMoveLimit.enabled = false;
//                    //resetCamera.Reset();
//                    centerObject.transform.localPosition = new Vector3(centerObject.transform.localPosition.x, centerObject.transform.localPosition.y - 5, centerObject.transform.localPosition.z);
//                    //resetCamera.mainCamera.transform.localPosition = new Vector3(resetCamera.mainCamera.transform.localPosition.x, resetCamera.mainCamera.transform.localPosition.y - 5, resetCamera.mainCamera.transform.localPosition.z);
//                    EnableORDisableCameraReset(false);
//                }

//                // 房间回到原位置
//                public void CameraDown()
//                {
//                    //resetCamera.Reset();
//                    //cameraMoveLimit.enabled = true;
//                    EnableORDisableCameraReset(true);
//                }

//                // 浇水按钮
//                public void EnterIrrigateState()
//                {
//                    ChangeState(new IrrigatePlantState());
//                }

//                // 割草按钮
//                public void EnterRemoveWeedState()
//                {
//                    if (AnyPotHasWeed())
//                    {
//                        ChangeState(new RemoveWeedState());
//                    }
//                    else
//                    {
//                        PromptMessage.Instance.ShowUpPrompt("所有杂草都已清理");
//                    }
//                }

//                // 收获按钮
//                public void EnterOrExitHarvestState()
//                {
//                    if (curState is HarvestPlantState)
//                    {
//                        ChangeState(new NonePlantState());
//                    }
//                    else
//                    {
//                        ChangeState(new HarvestPlantState());
//                    }
//                }

//                // 铲除按钮
//                public void EnterOrExitRemovePlantState()
//                {
//                    if (curState is RemovePlantState)
//                    {
//                        ChangeState(new NonePlantState());
//                    }
//                    else
//                    {
//                        ChangeState(new RemovePlantState());
//                    }
//                }


//                // 退出种植按钮
//                public void ExitPlanting()
//                {
//                    CommonInteractManager.Instance.SetMainFunctionActive(true);
//                    if (Planting_System_Manager.Instance.isSave())
//                    {
//                        Exit_Planting();
//                    }
//                    else
//                    {
//                        PromptMessage.Instance.ShowPrompt(0, () =>
//                        {
//                            Exit_Planting();
//                        });
//                    }

//                    Planting_System_Manager.Instance.current_selected_pot = null;
//                }

//                // 退出种植功能
//                public void Exit_Planting()
//                {
//                    ChangeWheelUIPosition(downWheelTransform);
//                    warehouse_Canvas_Prefab.SetActive(false);
//                    exitPlanting.SetActive(false);
//                    giveUpPlanting.SetActive(false);
//                    //confirmModification.SetActive(false);
//                    warehouse.SetActive(false);
//                    //bTN_Map.SetActive(true);
//                    bTN_Begin_Planting.SetActive(true);
//                    Open_panel_Planting_Function();
//                    generalInteractionEventHub._invoke_on_enable_or_disable_reset_camera(true);
//                    GiveUpPlantingAndSetIsTempFalse();
//                    CameraDown();
//                    balconyDoor.enabled = true;
//                    ChangeState(new NonePlantState());

//                    Global_Home_Room_Manager.Instance._can_switch_scene = true;

//                    Planting_System_Manager.Instance.can_edit_pot = false;
//                    main_Character_AI_Control.SetActive(true);
//                }

//                // 放弃种植按钮
//                public void GiveUpPlanting()
//                {
//                    PromptMessage.Instance.ShowPrompt(1, () =>
//                    {
//                        Planting_System_Manager.Instance.CancelAllModifacation();
//                    });
//                }

//                // 放弃种植并退出临时状态
//                public void GiveUpPlantingAndSetIsTempFalse()
//                {
//                    Planting_System_Manager.Instance.CancelAllModifacation();
//                    SetIsTemp(false);
//                }

//                // 粒子特效跟随手指
//                public void SetParticleToFinger(LeanFinger finger, ParticleSystem particleSystem)
//                {
//                    Camera cam = Camera.main;
//                    if (cam != null && particleSystem != null)
//                    {
//                        Vector3 screenPos = finger.ScreenPosition;
//                        screenPos.z = 5f;
//                        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
//                        particleSystem.transform.position = worldPos;
//                    }
//                }

//                // 坏花盆提示
//                //public void TryPutDownPot()
//                //{
//                //    var selectedItem = uI_Control_Warehouse_Panel._current_selected_game_item;
//                //    if (selectedItem == null || !selectedItem._is_pot)
//                //    {
//                //        return;
//                //    }

//                //    var potInfo = Game_Inventory_SO_01._inventory._additional_pot_info;
//                //    int minDurability = int.MaxValue;
//                //    foreach (var pot in potInfo)
//                //    {
//                //        if (pot._pot_name == selectedItem.item_name)
//                //        {
//                //            if (pot._pot_count_of_remained_use_count.Length < 1)
//                //            {
//                //                return;
//                //            }
//                //            for (int durability = 1; durability < pot._pot_count_of_remained_use_count.Length; durability++)
//                //            {
//                //                int count = pot._pot_count_of_remained_use_count[durability];
//                //                if (count > 0)
//                //                {
//                //                    minDurability = durability;
//                //                    break;
//                //                }
//                //            }
//                //        }
//                //    }
//                //    if (minDurability == int.MaxValue)
//                //    {
//                //        return;
//                //    }

//                //    //Debug.Log(minDurability);
//                //    if (minDurability <= 2)
//                //    {
//                //        PromptMessage.Instance.ShowUpPrompt("这个花盆快坏了");
//                //    }
//                //    string result;
//                //    Planting_System_Manager.Instance.try_add_pot_to_level(selectedItem.item_name, out result);
//                //}

//                // 无可收获植物提示
//                public bool CheckHarvestStateAndPrompt()
//                {
//                    if (curState is HarvestPlantState)
//                    {
//                        PromptMessage.Instance.ShowUpPrompt("当前没有植物可以收获！");
//                        return true;
//                    }
//                    return false;
//                }

//                // 检查是否有花盆有杂草
//                public bool AnyPotHasWeed()
//                {
//                    foreach (var pot in Planting_System_Manager.Instance._pot_list)
//                    {
//                        if (pot._pot_info != null && pot._pot_info.have_weed)
//                        {
//                            return true;
//                        }
//                    }
//                    return false;
//                }

//                // 启用或禁用双击重置摄像机
//                public void EnableORDisableCameraReset(bool b)
//                {
//                    if (doubleTapToResetCamera != null)
//                    {
//                        doubleTapToResetCamera.SetActive(b);
//                    }
//                }

//                // 设置临时状态
//                public void SetIsTemp(bool isTemp)
//                {
//                    Planting_System_Manager.Instance.is_temp = isTemp;
//                }

//                // 设置是否可以旋转花盆
//                public void SetCanRotatePot(bool canRotatePot)
//                {
//                    Planting_System_Manager.Instance.canPotRotate = canRotatePot;
//                }

//                public void ShowGrid(GameObject gameObject)
//                {
//                    //Pot_In_Scene pot = gameObject.GetComponent<Pot_In_Scene>();
//                    //Pot_Type potType = pot._pot_info._pot_info.pot_type;
//                    //switch (potType)
//                    //{
//                    //    case Pot_Type.Water:
//                    //        normal_Pot_Grid.SetActive(true);
//                    //        break;
//                    //    case Pot_Type.Soil:
//                    //        normal_Pot_Grid.SetActive(true);
//                    //        break;
//                    //    case Pot_Type.Hang:
//                    //        hanging_Pot_Grid.SetActive(true);
//                    //        break;
//                    //    default:
//                    //        break;
//                    //}
//                }

//                // 显示花盆虚影
//                public void DisplayPotGhost(string potName)
//                {
//                    //var selectedPot = uI_Control_Warehouse_Panel._current_selected_game_item;
//                    //if (selectedPot == null || !selectedPot.item_info.IsPot)
//                    //{
//                    //    //Debug.LogWarning("未选中任何花盆！");
//                    //    return;
//                    //}

//                    //// 获取花盆信息
//                    //var potInfo = Planting_System_Manager.Instance._plant_Config_SO._planting_config.flower_pot_info_list.Find(_p => _p.pot_name == potName);
//                    //if (potInfo == null)
//                    //{
//                    //    Debug.LogWarning($"未找到名称为 {potName} 的花盆信息！");
//                    //    return;
//                    //}

//                    //SetAllPlacementCollider(false);
//                    //if (ghostObjects.TryGetValue(potName, out GameObject existingGhost))
//                    //{
//                    //    if (currentPreGhostPot != null)
//                    //    {
//                    //        currentPreGhostPot.SetActive(false);
//                    //    }

//                    //    if (potInfo.pot_type == Pot_Type.Hang)
//                    //    {
//                    //        existingGhost.transform.position = ghostTransformHang.position;
//                    //        leanPitch.Pitch = 90;
//                    //        //SetLeanMultiUpdate(false);
//                    //    }
//                    //    else
//                    //    {
//                    //        existingGhost.transform.position = ghostTransform.position;
//                    //    }

//                    //    SetSinglePlacementCollider(existingGhost, true);
//                    //    existingGhost.SetActive(true);
//                    //    currentPreGhostPot = existingGhost;
//                    //    currentPreGhostPotType = potInfo.pot_type;
//                    //    return;
//                    //}

//                    //// 加载花盆模型
//                    //PM_RM.load_game_object_async(potInfo.res_url, (Action<GameObject>)((potPrefab) =>
//                    //{
//                    //    GameObject potGhost;
//                    //    if (potInfo.pot_type == Pot_Type.Hang)
//                    //    {
//                    //        potGhost = Instantiate(potPrefab, ghostTransformHang.position, Quaternion.identity);
//                    //        leanPitch.Pitch = 90;
//                    //        //SetLeanMultiUpdate(false);
//                    //    }
//                    //    else
//                    //    {
//                    //        potGhost = Instantiate(potPrefab, ghostTransform.position, Quaternion.identity);
//                    //    }
                        
//                    //    potGhost.name = $"{potName}_Ghost";

//                    //    // 替换材质为透明材质
//                    //    Pot_In_Scene pot_In_Scene = potGhost.GetComponent<Pot_In_Scene>();
//                    //    pot_In_Scene.change_mat_at_mats(0, pot_In_Scene._mat_selected);

//                    //    potGhost.transform.SetParent(ghostRoot, true);

//                    //    if (currentPreGhostPot != null)
//                    //    {
//                    //        currentPreGhostPot.SetActive(false);
//                    //    }

//                    //    potGhost.SetActive(true);
//                    //    currentPreGhostPot = potGhost;
//                    //    currentPreGhostPotType = potInfo.pot_type;

//                    //    ghostObjects[potName] = potGhost;

//                    //    StartCoroutine(EnableDisplayRenderAfterDelay(potGhost.GetComponentInParent<Pot_In_Scene>()));
//                    //}));
//                }

//                private IEnumerator EnableDisplayRenderAfterDelay(Pot_In_Scene potInScene)
//                {
//                    yield return new WaitForSeconds(0.2f);

//                    if (potInScene != null && potInScene._display_render != null)
//                    {
//                        potInScene._display_render.enabled = true;
//                        Debug.Log($"_display_render 已启用: {potInScene.name}");
//                    }
//                }

//                public void CloseGhost()
//                {
//                    if (currentPreGhostPot != null)
//                    {
//                        currentPreGhostPot.SetActive(false);
//                    }
//                    currentPreGhostPot = null;
//                }

//                public bool IsPointerOverUI(LeanFinger finger)
//                {
//                    return LeanTouch.PointOverGui(finger.ScreenPosition);
//                }

//                public void SetSinglePlacementCollider(GameObject go, bool enabled)
//                {
//                    Collider[] colliders = go.GetComponentsInChildren<Collider>(true);
//                    foreach (var collider in colliders)
//                    {
//                        if (collider.gameObject.GetComponentInParent<Game_Grid_Cell>(true) != null) continue;
//                        collider.enabled = enabled;
//                    }
//                }

//                public void SetAllPlacementCollider(bool enabled)
//                {
//                    foreach (var placement_In_Level in Planting_System_Manager.Instance._pot_list)
//                    {
//                        Collider[] colliders = placement_In_Level.gameObject.GetComponentsInChildren<Collider>(true);
//                        foreach (var collider in colliders)
//                        {
//                            if (collider.gameObject.GetComponentInParent<Game_Grid_Cell>(true) != null) continue;
//                            collider.enabled = enabled;
//                        }
//                    }
//                }

//                private IEnumerator WaitForGridSelection(Game_Grid_Cell gridCell, Action action)
//                {
//                    yield return new WaitForSecondsRealtime(0.25f);

//                    Planting_System_Manager.Instance.on_select_cell(gridCell.gameObject);

//                    yield return new WaitForSecondsRealtime(0.25f);

//                    action.Invoke();
//                }

//                public void OnExitPlacePot()
//                {
//                    SetLeanMultiUpdate(true);
//                    CloseGhost();

//                    _warehouse_UI_event_hub._invoke_deselected_item();

//                    Planting_System_Manager.Instance.hide_grid();
//                    SetAllPlacementCollider(true);
//                }

//                public void MoveGhostObject(LeanFinger finger)
//                {
//                    if (currentPreGhostPot == null)
//                    {
//                        //Debug.Log("没有选中的物体，无法移动！");
//                        return;
//                    }

//                    if (finger.Down && activeFinger == null)
//                    {
//                        if (IsPointerOverUI(finger))
//                        {
//                            Debug.Log("触摸在UI上，忽略MoveGhostObject");
//                            //Planting_System_Manager.Instance.on_exit_placing_pot();
//                            //OnExitPlacePot();
//                            return;
//                        }

//                        Collider collider1 = currentPreGhostPot.GetComponent<Pot_In_Scene>()._display_render.GetComponent<Collider>();
//                        // 检查射线是否命中当前选中的物体
//                        GameObject go = customRaycastTool.UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);
//                        if (go == collider1.gameObject)
//                        {
//                            //Debug.Log("手指按下在选中的物体上，开始拖动");
//                            activeFinger = finger;
//                            SetLeanMultiUpdate(false);

//                            // 禁用选中物体的碰撞器
//                            SetSinglePlacementCollider(currentPreGhostPot, false);

//                            Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
//                            // 初始化当前网格
//                            if (Physics.Raycast(ray, out RaycastHit gridHit, Mathf.Infinity, layerMask))
//                            {
//                                Game_Grid_Cell newGridCell = gridHit.collider.GetComponentInParent<Game_Grid_Cell>(true);
//                                testHit = gridHit.collider;
//                                Debug.Log(testHit.name);
//                                previousGridCell = newGridCell;
//                                currentGridCell = newGridCell;
//                            }
//                            else
//                            {
//                                if (currentPreGhostPotType == Pot_Type.Hang)
//                                {
//                                    previousGridCell = ghostTransformHang.GetComponent<Game_Grid_Cell>();
//                                    currentGridCell = previousGridCell;
//                                }
//                                else
//                                {
//                                    previousGridCell = ghostTransform.GetComponent<Game_Grid_Cell>();
//                                    currentGridCell = previousGridCell;
//                                }
//                                //Debug.LogWarning("射线未命中任何对象！");
//                            }
//                        }
//                        else
//                        {
//                            //Debug.Log("手指按下不在选中的物体上，取消选择");
//                            //Planting_System_Manager.Instance.on_exit_placing_pot();
//                            //OnExitPlacePot();
//                            return;
//                        }
//                    }

//                    // 手指移动时检测网格变化
//                    if (finger.Set && finger == activeFinger)
//                    {
//                        //GameObject newGridCell = customRaycastTool.PerformRaycast(finger.ScreenPosition, "Cell", layerMask);
//                        Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
//                        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
//                        {
//                            Game_Grid_Cell newGridCell = hit.collider.GetComponentInParent<Game_Grid_Cell>(true);

//                            if (newGridCell != null && newGridCell != currentGridCell && previousGridCell != null)
//                            {
//                                previousGridCell = currentGridCell;
//                                currentGridCell = newGridCell;

//                                //Vector3 offset = currentGridCell.cell_position - previousGridCell.cell_position;
//                                //currentPreGhostPot.transform.position += offset;
//                            }
//                        }
//                    }

//                    // 松开时启用碰撞器并尝试移动
//                    if (finger.Up && finger == activeFinger)
//                    {
//                        activeFinger = null;
//                        SetAllPlacementCollider(true);
//                        Collider boxCollider = currentPreGhostPot.GetComponent<Pot_In_Scene>()._display_render.GetComponent<Collider>();

//                            if (boxCollider != null)
//                            {
//                                Vector3 leftTopCorner = new Vector3(currentPreGhostPot.transform.position.x + 0.5f, currentPreGhostPot.transform.position.y + 32, currentPreGhostPot.transform.position.z + 0.5f);

//                                Ray ray = new Ray(leftTopCorner, Vector3.down);
//                                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
//                                {
//                                    Game_Grid_Cell gridCell = hit.collider.GetComponentInParent<Game_Grid_Cell>(true);
//                                    if (gridCell != null)
//                                    {
//                                        StartCoroutine(WaitForGridSelection(gridCell, () =>
//                                        {
//                                            Planting_System_Manager.Instance.on_exit_placing_pot();
//                                            OnExitPlacePot();
//                                        }));
//                                        //Debug.Log(leftTopCorner);
//                                        //Debug.Log($"左下角网格：{gridCell.name}");
//                                        //Debug.Log(gridCell.transform.position);
//                                        return;
//                                    }
//                                    else
//                                    {
//                                        Debug.LogWarning("未检测到有效的 GridCell！");
//                                    }
//                                }
//                                else
//                                {
//                                    Debug.LogWarning("射线未命中任何对象！");
//                                }
//                            }
//                            else
//                            {
//                                Debug.LogError("选中物体没有 BoxCollider！");
//                            }

//                        Planting_System_Manager.Instance.on_exit_placing_pot();
//                        OnExitPlacePot();
//                    }
//                }

//                public void MoveSelectObject(LeanFinger finger)
//                {
//                    if (selectedObject == null)
//                    {
//                        //Debug.LogWarning("没有选中的物体，无法移动！");
//                        return;
//                    }

//                    if (finger.Down && activeFinger == null)
//                    {
//                        if (IsPointerOverUI(finger))
//                        {
//                            //Debug.Log("触摸在UI上，忽略MoveSelectObject");
//                            //Planting_System_Manager.Instance.on_deselect_pot();
//                            return;
//                        }

//                        Collider collider1 = selectedObject.GetComponent<Pot_In_Scene>()._display_render.GetComponent<Collider>();
//                        // 检查射线是否命中当前选中的物体
//                        GameObject go = customRaycastTool.UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);
//                        if (go == collider1.gameObject)
//                        {
//                            //Debug.Log("手指按下在选中的物体上，开始拖动");
//                            activeFinger = finger;
//                            Vector3 hitPoint = hitInfo.point;
//                            //SetSinglePlacementCollider(selectedObject, false);

//                            GameObject hitGo = customRaycastTool.PerformRaycast(finger.ScreenPosition, "Cell", layerMask, Vector3.zero, out RaycastHit hit);
//                            if (hitGo != null)
//                            {
//                                Game_Grid_Cell newGridCell = hit.collider.GetComponentInParent<Game_Grid_Cell>(true);
//                                previousGridCell = newGridCell;
//                                currentGridCell = newGridCell;
//                            }
//                            else
//                            {
//                                Vector3 leftTopCorner = new Vector3(selectedObject.transform.position.x + 0.5f, selectedObject.transform.position.y + 32, selectedObject.transform.position.z + 0.5f);

//                                Ray ray = new Ray(leftTopCorner, Vector3.down);
//                                if (Physics.Raycast(ray, out RaycastHit hitGrid, Mathf.Infinity, layerMask))
//                                {
//                                    Game_Grid_Cell gridCell = hitGrid.collider.GetComponentInParent<Game_Grid_Cell>(true);
//                                    if (gridCell != null)
//                                    {
//                                        previousGridCell = gridCell;
//                                        currentGridCell = gridCell;
//                                    }
//                                    else
//                                    {
//                                        Debug.LogWarning("未检测到有效的 GridCell！");
//                                    }
//                                }
//                            }
//                        }
//                        else
//                        {
//                            //Debug.Log("手指按下不在选中的物体上，取消选择");
//                            Planting_System_Manager.Instance.on_deselect_pot();
//                            return;
//                        }
//                    }

//                    // 手指移动时检测网格变化
//                    if (finger.Set && finger == activeFinger)
//                    {
//                        GameObject go = customRaycastTool.PerformRaycast(finger.ScreenPosition, "Cell", layerMask, Vector3.zero, out RaycastHit hit);
//                        //Ray ray = Camera.main.ScreenPointToRay(finger.ScreenPosition);
//                        if (go != null)
//                        {
//                            Game_Grid_Cell newGridCell = hit.collider.GetComponentInParent<Game_Grid_Cell>(true);

//                            if (newGridCell != null && newGridCell != currentGridCell && previousGridCell != null)
//                            {
//                                previousGridCell = currentGridCell;
//                                currentGridCell = newGridCell;

//                                //Vector3 offset = currentGridCell.cell_position - previousGridCell.cell_position;
//                                //selectedObject.transform.position += offset;

//                                //Debug.Log($"物体移动到新网格：{currentGridCell.name}");
//                            }
//                        }
//                    }

//                    // 松开时启用碰撞器并尝试移动
//                    if (finger.Up && finger == activeFinger)
//                    {
//                        activeFinger = null;

//                        Collider collider = selectedObject.GetComponent<Pot_In_Scene>()._collider.GetComponent<Collider>();
//                        SetSinglePlacementCollider(selectedObject, true);

//                        if (collider != null)
//                        {
//                            //Vector3 leftTopCorner = new Vector3(boxCollider.bounds.min.x, boxCollider.bounds.max.y, boxCollider.bounds.min.z);
//                            Vector3 leftTopCorner = new Vector3(selectedObject.transform.position.x + 0.5f, selectedObject.transform.position.y + 32, selectedObject.transform.position.z + 0.5f);

//                            GameObject go = customRaycastTool.PerformRaycast(leftTopCorner, "Cell", layerMask, Vector3.down, out RaycastHit hit);
//                            //Ray ray = new Ray(leftTopCorner, Vector3.down);
//                            if (go != null)
//                            {
//                                Game_Grid_Cell gridCell = hit.collider.GetComponentInParent<Game_Grid_Cell>(true);
//                                testHit = hit.collider;
//                                if (gridCell != null)
//                                {
//                                    StartCoroutine(WaitForGridSelection(gridCell, () =>
//                                    {
//                                        Planting_System_Manager.Instance.on_deselect_pot();
//                                    }));
//                                    //Debug.Log(leftTopCorner);
//                                    //Debug.Log($"左下角网格：{gridCell.name}");
//                                    //Debug.Log($"左下角网格：{gridCell.transform.position}");
//                                    return;
//                                }
//                                else
//                                {
//                                    Debug.LogWarning("未检测到有效的 GridCell！");
//                                    selectedObject.transform.position = originalPosition;
//                                }
//                            }
//                            else
//                            {
//                                Debug.LogWarning("射线未命中任何对象！");
//                                selectedObject.transform.position = originalPosition;
//                            }
//                        }
//                        else
//                        {
//                            Debug.LogError("选中物体没有 Collider！");
//                        }

//                        Planting_System_Manager.Instance.on_deselect_pot();
//                    }
//                }

//                public void SelectPlacement(LeanFinger finger)
//                {
//                    if (finger.Down)
//                    {
//                        GameObject go = customRaycastTool.UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);
//                        Pot_In_Scene pot = null;
//                        if (go != null)
//                        {
//                            pot = go.GetComponentInParent<Pot_In_Scene>();
//                        }
//                        if (pot != null)
//                        {
//                            longPressTarget = go;
//                            longPressPot = pot;
//                            pressTime = 0;
//                            Debug.Log($"开始长按检测: {longPressTarget.name}");
//                        }
//                    }

//                    if (finger.Set && longPressTarget != null)
//                    {
//                        GameObject go = customRaycastTool.UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);
//                        if (go != longPressTarget)
//                        {
//                            Debug.Log("长按目标改变，重置计时");
//                            longPressTarget = null;
//                            pressTime = 0;
//                        }
//                        else
//                        {
//                            pressTime += Time.deltaTime;
//                            if (pressTime >= longPressThreshold)
//                            {
//                                Planting_System_Manager.Instance.on_select_pot(longPressPot.gameObject);
//                                longPressTarget = null;
//                                pressTime = 0;
//                                Debug.Log("长按成功，选中家具" + longPressPot.gameObject.name);
//                            }
//                        }
//                    }

//                    if (finger.Up)
//                    {
//                        longPressTarget = null;
//                        pressTime = 0;
//                    }
//                }
//                private void OnDisable()
//                {
//                    UnsubscribeEventsAndButtons();
//                }
//            }
//        }
//    }
//}
