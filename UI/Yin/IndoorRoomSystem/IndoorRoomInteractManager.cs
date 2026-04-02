//using System;
//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Client_Event_Systems;
//using CLIP.Project_Mouse.ENUM;
//using CLIP.Project_Mouse.Game_Play_System;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using CLIP.Project_Mouse.Kernel;
//using Lean.Common;
//using Lean.Touch;
//using MoreMountains.Tools;
//using UnityEngine;
//using UnityEngine.UI;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class IndoorRoomInteractManager : MonoBehaviour
//            {
//                [Header("UI")]
//                public GameObject panel_Main_Function;
//                public GameObject bTN_Main_Menu;
//                public GameObject warehouse_Canvas_Prefab;
//                //public FurnitureWarehouseInteractManager ui_control_warehouse_panel;
//                public GameObject editRoom;
//                public GameObject exitEditRoom;
//                public GameObject deleteAllFurniture;
//                public GameObject confirmModification;
//                public GameObject warehouse;
//                public GameObject indoorRoomCanvas;
//                public GameObject wheel;
//                public Transform upWheelTransform;
//                public Transform downWheelTransform;

//                private Button warehouseBtn;
//                private Button confirmModificationBtn;



//                [Header("摄像机组件")]
//                public GameObject doubleTapToResetCamera;
//                public ResetCamera resetCamera;
//                public CameraMoveLimit cameraMoveLimit;
//                public Transform centerObject;
//                public LeanPitchYaw leanPitchYaw;
//                public LeanMultiUpdate leanMultiUpdate;
//                public CustomRaycastTool customRaycastTool;

//                [Header("房间门碰撞体")]
//                public List<BoxCollider> doors = new List<BoxCollider>();

//                [Header("长按选择家具")]
//                public GameObject longPressTarget; 
//                public PlacementRuntime longPressFurniture; 
//                public float pressTime; 
//                public float longPressThreshold = 0.5f;

//                [Header("材质")]
//                public Material matGreen;
//                public Material matRed;

//                [Header("家具虚影")]
//                public Transform preGhostTransform;
//                public Transform preWallGhostTransform;
//                public Transform preCeilingTransform;
//                public Transform preGhostRoot;
//                public Vector3 placeTransform;
//                public GameObject currentPreGhostObject = null;
//                public string currentPreGhostObjectName;
//                public Room_Placing_Type currentPreGhostObjectType;
//                public Dictionary<string, GameObject> preGhostObjects = new Dictionary<string, GameObject>();

//                [Header("选择家具")]
//                public Material[] selectFurnitureMat;
//                public GameObject selectedObject;
//                public string selectedObjectName;
//                public Room_Placing_Type selectedObjectType;
//                public Game_Grid_Cell previousGridCell;
//                public Game_Grid_Cell currentGridCell;
//                public Vector3 originalPosition;
//                public int originalRotationIndex;
//                public LayerMask layerMask;
//                public LeanFinger activeFinger;
//                public Collider roomCollider;

//                [Header("礼物")]
//                public GameObject one;
//                public GameObject two;
//                public GameObject three;
//                public GameItem_DB_SO game_Inventory;
//                public int waitPhotoCount = 0;

//                [Header("Event System")]
//                public Warehouse_UI_Event_Hub_SO _warehouse_UI_event_hub;
//                public General_Interaction_Event_Hub_SO generalInteractionEventHub;

//                public bool isEditing = false;

//                public GameObject main_Character_AI_Control;
//                public Collider testHit;
//                private float maxLongPressMoveDistance;
//                private Vector2 longPressStartPos;

//                private void Start()
//                {
//                    StartCoroutine(switch_UI_co(1f));
//                    confirmModificationBtn = confirmModification.GetComponent<Button>();
//                    warehouseBtn = warehouse.GetComponent<Button>();
//                }

//                private void OnEnable()
//                {
//                    confirmModificationBtn = confirmModification.GetComponent<Button>();
//                    warehouseBtn = warehouse.GetComponent<Button>();
//                    confirmModificationBtn?.onClick.AddListener(() =>
//                    {
//                        confirmModification.SetActive(false);
//                    });


//                    warehouseBtn?.onClick.AddListener(() =>
//                    {
//                        confirmModification?.SetActive(true);
//                    });


//                    if (preGhostTransform != null) placeTransform = new Vector3(preGhostTransform.position.x, preGhostTransform.position.y, preGhostTransform.position.z);
//                    StartCoroutine(FindObjectsWithDelay());
//                    RefreshGift();
//                    StartCoroutine(switch_UI_co(0.2f));
//                    main_Character_AI_Control = GameObject.Find("Main_Character_20250820_ver_2");
//                    ChangeWheelUIPosition(downWheelTransform);
//                }

//                private void OnDisable()
//                {
//                    confirmModificationBtn?.onClick.RemoveAllListeners();
//                    warehouseBtn?.onClick.RemoveAllListeners();

//                    UnsubscribeEventsAndButtons();
//                }

//                private IEnumerator FindObjectsWithDelay()
//                {
//                    yield return new WaitForSeconds(0.1f);
//                    panel_Main_Function = GameObject.Find("Panel_Main_Function");
//                    bTN_Main_Menu = GameObject.Find("BTN_Main_Menu");

//                    yield return null;

//                    UnsubscribeEventsAndButtons();

//                    yield return null;

//                    if (panel_Main_Function == null || bTN_Main_Menu == null)
//                    {
//                        Debug.LogError("目标物体未找到！");
//                    }

//                    bTN_Main_Menu.GetComponent<Button>().onClick.AddListener(OpenPhone);

//                    _warehouse_UI_event_hub._selected_item_change.AddListener(DisplayPreFurnitureGhost);

//                    //EvtDsp.AddEvt<string>(EvtNames.On_Select_Item_Change, DisplayPreFurnitureGhost);

//                    generalInteractionEventHub._on_change_select_object_material.AddListener(ChangeSelectObjectMaterial);
//                    generalInteractionEventHub._on_set_lean_multi_update.AddListener(SetLeanMultiUpdate);
//                    generalInteractionEventHub._on_swipe_close_indoor_room_warehouse_canvas.AddListener(SwipeCloseIndoorRoomWarehouseCanvas);
//                    generalInteractionEventHub._on_refresh_gift.AddListener(RefreshGift);
//                }

//                private void Update()
//                {

////                    if (isEditing)
////                    {
////#if UNITY_EDITOR || UNITY_STANDALONE
////                        if (LeanTouch.Fingers.Count == 2)
////                        {
////                            var finger = LeanTouch.Fingers[1];
////                            SelectPlacement(finger);
////                            MoveGhostObject(finger);
////                        }
////#endif

////#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
////                        if (LeanTouch.Fingers.Count == 1)
////                        {
////                            var finger = LeanTouch.Fingers[0];
////                            SelectPlacement(finger);
////                            MoveSelectObject(finger);
////                            MoveGhostObject(finger);
////                        }
////#endif
////                    }
//                }
//                private void FixedUpdate()
//                {
//                  //  switch_UI();
//                }
//                public IEnumerator switch_UI_co(float waitTime)
//                {
//                    yield return new WaitForSeconds(waitTime);
//                    switch_UI();
//                }
//                public void switch_UI()
//                {
//                    //Debug.Log("IndoorRoomInteractManager_switch_UI");
//                    if (Player_Social_Manager._instance == null) return;

//                    if ( Player_Social_Manager._instance._on_visit_friend_room==true)
//                    {
//                       if(indoorRoomCanvas.activeSelf==true) indoorRoomCanvas.SetActive(false);
//                    }
//                    else  
//                    {
//                        if (indoorRoomCanvas.activeSelf == false) indoorRoomCanvas.SetActive(true);
//                    }
//                }
//                private void UnsubscribeEventsAndButtons()
//                {
//                    if (bTN_Main_Menu != null)
//                    {
//                        bTN_Main_Menu.GetComponent<Button>().onClick.RemoveListener(OpenPhone);
//                    }
//                    if (_warehouse_UI_event_hub != null)
//                    {
//                        _warehouse_UI_event_hub._selected_item_change.RemoveListener(DisplayPreFurnitureGhost);
//                    }
//                    //EvtDsp.RemoveEvt<string> (EvtNames.On_Select_Item_Change, DisplayPreFurnitureGhost);
//                    if (generalInteractionEventHub != null)
//                    {
//                        generalInteractionEventHub._on_set_lean_multi_update.RemoveListener(SetLeanMultiUpdate);
//                        generalInteractionEventHub._on_swipe_close_indoor_room_warehouse_canvas.RemoveListener(SwipeCloseIndoorRoomWarehouseCanvas);
//                        generalInteractionEventHub._on_refresh_gift.RemoveListener(RefreshGift);
//                    }
//                }

//                // 刷新礼物数量
//                public void RefreshGift()
//                {
//                    if (Player_Social_Manager._instance != null && one != null && two != null && three != null)
//                    {
//                        if (Player_Social_Manager._instance._current_social_info._present_records == null || Player_Social_Manager._instance._current_social_info._present_records.Count == 0)
//                        {
//                            one.SetActive(false);
//                            two.SetActive(false);
//                            three.SetActive(false);
//                        }
//                        else if (Player_Social_Manager._instance._current_social_info._present_records.Count == 1)
//                        {
//                            one.SetActive(true);
//                            two.SetActive(false);
//                            three.SetActive(false);
//                        }
//                        else if (Player_Social_Manager._instance._current_social_info._present_records.Count == 2)
//                        {
//                            one.SetActive(false);
//                            two.SetActive(true);
//                            three.SetActive(false);
//                        }
//                        else
//                        {
//                            one.SetActive(false);
//                            two.SetActive(false);
//                            three.SetActive(true);
//                        }
//                    }
//                }

//                // 打开所有礼物
//                public void OpenAllGift()
//                {
//                    if (Player_Social_Manager._instance != null)
//                    {
//                        if (one != null && two != null && three != null)
//                        {
//                            one.SetActive(false);
//                            two.SetActive(false);
//                            three.SetActive(false);
//                        }

//                        var presentRecords = Player_Social_Manager._instance._current_social_info._present_records;
//                        //if (presentRecords == null || game_Inventory == null || game_Inventory._inventory == null) return;

//                        GroupPhoto.Instance.photos.Clear();
//                        waitPhotoCount = 0;
//                        List<int> mailId = new List<int>();

//                        foreach (var present in presentRecords)
//                        {
//                            string giftName = present._msg_good_item_name;
//                            if (string.IsNullOrEmpty(giftName)) continue;

//                            if (giftName.StartsWith("#Photo#"))
//                            {
//                                GroupPhoto.Instance.photos.Add(giftName, null);
//                                Global_Photo_Manager.Instance.get_photo_from_server(giftName);
//                                StartCoroutine(WaitForPhoto(giftName));
//                                waitPhotoCount++;
//                            }
//                            else
//                            {
//                                //var item = game_Inventory._inventory._current_inventory.Find(i => i.item_name == giftName);
//                                //if (item != null)
//                                //{
//                                //    item._item_count += 1;
//                                //    Debug.Log("获得礼物: " + giftName);
//                                //}
//                            }

//                            var mailList = Email_And_Announcement_Manager.instance._mail_record;
//                            foreach (var mail in mailList)
//                            {
//                                if (mail._send_present_msg_id_related == present._msg_id)
//                                {
//                                    mail.isGetReward = true;
//                                    mailId.Add(mail.mail_id);
//                                }
//                            }
//                        }

//                        Email_And_Announcement_Manager.instance.on_read_mail(mailId);
//                        Email_And_Announcement_Manager.instance.on_get_mail_reward(mailId);

//                        Player_Social_Manager._instance._current_social_info._present_records.Clear();
//                        StartCoroutine(UploadAfterOpenGifts());

//                        //GroupPhoto.Instance.OpenGroupPhoto();
//                    }
//                }

//                public IEnumerator WaitForPhoto(string photoName)
//                {
//                    while (true)
//                    {
//                        var photoList = Global_Photo_Manager.Instance._photo_from_network;
//                        Texture2D found = null;
//                        foreach (var tex in photoList)
//                        {
//                            if (tex != null && tex.name == photoName)
//                            {
//                                found = tex;
//                                break;
//                            }
//                        }
//                        if (found != null)
//                        {
//                            GroupPhoto.Instance.photos[photoName] = found;
//                            waitPhotoCount--;
//                            if (waitPhotoCount == 0)
//                            {
//                                GroupPhoto.Instance.OpenGroupPhoto();
//                            }
//                            //GroupPhoto.Instance.rawImage.texture = found;
//                            //GroupPhoto.Instance.OpenGroupPhoto();
//                            yield break;
//                        }
//                        yield return new WaitForSeconds(0.2f);
//                    }
//                }

//                public IEnumerator UploadAfterOpenGifts()
//                {
//                    Player_Social_Manager._instance.upload_present_records_to_server();

//                    yield return new WaitForSeconds(1f);

//                    Global_Inventory_Manager._instance.Send_inventory_to_server();
//                }

//                // 滑动关闭仓库界面
//                public void SwipeCloseIndoorRoomWarehouseCanvas()
//                {
//                    ChangeWheelUIPosition(downWheelTransform);
//                    warehouse_Canvas_Prefab.SetActive(false);
//                    warehouse.SetActive(true);
//                    //confirmModification.SetActive(false);
//                    CameraDown();
//                    //indoorRoomInteractManager.leanMultiUpdate.enabled = true;
//                    //Indoor_Room_Game_Manager._activc_instance._on_exit_add_placement();
//                }

//                // 滑动旋转启用或禁用
//                public void SetLeanMultiUpdate(bool enable)
//                {
//                    if (leanMultiUpdate != null)
//                    {
//                        leanMultiUpdate.enabled = enable;
//                        //Debug.Log("leanMultiUpdate.enabled = " + enable);
//                    }
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

//                // 编辑房间按钮
//                public void EditRoom()
//                {
//                    ChangeWheelUIPosition(upWheelTransform);
//                    warehouse_Canvas_Prefab.SetActive(true);
//                    editRoom.SetActive(false);
//                    panel_Main_Function.SetActive(false);
//                    confirmModification.SetActive(true );
//                    //giveUpEditRoom.SetActive(true);
//                    exitEditRoom.SetActive(true);
//                    deleteAllFurniture.SetActive(true);
//                    confirmModification.SetActive(true);
//                    isEditing = true;
//                    //Indoor_Room_Game_Manager._activc_instance.save_current_state_to_memory();
//                    //Indoor_Room_Game_Manager._activc_instance.saving_current_state_to_disk_as_json(Indoor_Room_Game_Manager._activc_instance._level_info._room_name + ".json");
//                    CameraUp();
//                    //leanPitchYaw.Pitch = 90;
//                    //leanMultiUpdate.enabled = false;

//                    Global_Home_Room_Manager._instance._can_switch_scene = false;
//                    Indoor_Room_Game_Manager._activc_instance._can_select_placement = true;
//                    main_Character_AI_Control.SetActive(false);
//                    //foreach (BoxCollider door in doors)
//                    //{
//                    //    door.enabled = false;
//                    //}
//                }

//                // 退出编辑房间按钮
//                public void ExitEditRoom()
//                {
//                    ChangeWheelUIPosition(downWheelTransform); 
//                    warehouse_Canvas_Prefab.SetActive(false);
//                    editRoom.SetActive(true);
//                    panel_Main_Function.SetActive(true);
//                    confirmModification.SetActive(false);
//                    //giveUpEditRoom.SetActive(false);
//                    exitEditRoom.SetActive(false);
//                    deleteAllFurniture.SetActive(false);
//                    confirmModification.SetActive(false);
//                    warehouse.SetActive(false);
//                    isEditing = false;
//                    //leanMultiUpdate.enabled = true;
//                    CameraDown();
//                    //Indoor_Room_Game_Manager._activc_instance.hide_grid();

//                    Global_Home_Room_Manager._instance._can_switch_scene = true;
//                    Indoor_Room_Game_Manager._activc_instance._can_select_placement = false;
//                    main_Character_AI_Control.SetActive(true);
//                    Indoor_Room_Game_Manager._activc_instance._current_selected_placement = null;
//                    //Indoor_Room_Game_Manager._activc_instance._current_selected_placement_info = null;
//                    //if (Indoor_Room_Game_Manager._activc_instance._current_state == "Add_Placement" || Indoor_Room_Game_Manager._activc_instance._current_state == "Moving_Placement")
//                    //{
//                    //    Indoor_Room_Game_Manager._activc_instance._on_exit_add_placement();
//                    //    Indoor_Room_Game_Manager._activc_instance._on_exit_moving_placement();
//                    //}
//                    IndoorMainCharacter._instance.WrapPosition();
//                    IndoorMainCharacter._instance.EnterIdleState();
//                    //foreach (BoxCollider door in doors)
//                    //{
//                    //    door.enabled = true;
//                    //}
//                }

//                // 仓库按钮
//                public void OpenWareHouse()
//                {
//                    ChangeWheelUIPosition(upWheelTransform);
//                    warehouse_Canvas_Prefab.SetActive(true);
//                    //confirmModification.SetActive(true);
//                    warehouse.SetActive(false);
//                    //leanMultiUpdate.enabled = false;
//                    CameraUp();
//                    //leanPitchYaw.Pitch = 90;
//                }

//                // 房间上移并关闭移动限制并开启移动限制
//                public void CameraUp()
//                {
//                    //cameraMoveLimit.enabled = false;
//                    resetCamera.Reset();
//                    centerObject.transform.localPosition = new Vector3(centerObject.transform.localPosition.x, centerObject.transform.localPosition.y - 5, centerObject.transform.localPosition.z);
//                    resetCamera.mainCamera.transform.localPosition = new Vector3(resetCamera.mainCamera.transform.localPosition.x, resetCamera.mainCamera.transform.localPosition.y - 5, resetCamera.mainCamera.transform.localPosition.z);
//                    EnableORDisableDoubleTapCameraReset(false);
//                }

//                // 房间回到原位置
//                public void CameraDown()
//                {
//                    resetCamera.Reset();
//                    //cameraMoveLimit.enabled = true;
//                    EnableORDisableDoubleTapCameraReset(true);
//                }

//                public void EnableORDisableDoubleTapCameraReset(bool b)
//                {
//                    if (doubleTapToResetCamera != null)
//                    {
//                        doubleTapToResetCamera.SetActive(b);
//                    }
//                }

//                // 退出房间编辑按钮
//                public void ExitEditing()
//                {
//                    //if (Indoor_Room_Game_Manager._activc_instance.IsSave())
//                    //{
//                    //    ExitEditRoom();
//                    //}
//                    //else
//                    //{
//                    //    PromptMessage.Instance.ShowPrompt(4, () =>
//                    //    {
//                    //        Indoor_Room_Game_Manager._activc_instance.CancelAllModifacation();
//                    //        ExitEditRoom();
//                    //    });
//                    //}
//                }

//                // 手机按钮
//                public void OpenPhone()
//                {
//                    if (isEditing)
//                    {
//                        PromptMessage.Instance.ShowPrompt(3, () =>
//                        {
//                            ExitEditing();
//                            CommonInteractManager.Instance.OpenPhone();
//                        });
//                    }
//                }

//                public void SelectPlacement(LeanFinger finger)
//                {
//                    if (finger.Down)
//                    {
//                        GameObject go = customRaycastTool.UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);
//                        PlacementRuntime placement = null;
//                        if (go != null)
//                        {
//                            placement = go.GetComponentInParent<PlacementRuntime>();
//                        }
//                        if (placement != null)
//                        {
//                            //// 立即选中家具，取消长按延迟
//                            //placement.Set_border_Active(true);
//                            //placement.Update_BorderMesh();
//                            //placement.on_select_placement();

//                            //// 立即开始移动逻辑
//                            //StartMovingImmediately(finger, placement, go);

//                            //Debug.Log("立即选中并开始移动家具：" + placement.gameObject.name);

//                            longPressTarget = go;
//                            longPressFurniture = placement;
//                            pressTime = 0;
//                            //Debug.Log($"开始长按检测: {longPressTarget.name}");
//                            longPressStartPos = finger.ScreenPosition; // 记录长按开始位置
//                        }
//                    }

//                    if (finger.Set && longPressTarget != null)
//                    {
//                        GameObject go = customRaycastTool.UpdateHitObject(finger.ScreenPosition, out RaycastHit hitInfo);

//                        // 检查是否移动太远，如果是则取消长按
//                        float moveDistance = Vector2.Distance(finger.ScreenPosition, longPressStartPos);
//                        if (moveDistance > maxLongPressMoveDistance) // 设置一个阈值，比如10像素
//                        {
//                            longPressTarget = null;
//                            pressTime = 0;
//                            return;
//                        }

//                        if (go != longPressTarget)
//                        {
//                            //Debug.Log("长按目标改变，重置计时");
//                            longPressTarget = null;
//                            pressTime = 0;
//                        }
//                        else
//                        {
//                            pressTime += Time.deltaTime;
//                            if (pressTime >= longPressThreshold)
//                            {
//                                var placement = go.GetComponentInParent<PlacementRuntime>();
//                                //placement.Set_border_Active(true);
//                                //placement.Update_BorderMesh();
//                                //longPressFurniture.on_select_placement();

//                                // 长按成功后，立即开始移动逻辑
//                                StartMovingAfterLongPress(finger, placement, go);

//                                longPressTarget = null;
//                                pressTime = 0;
//                                Debug.Log("长按成功，选中家具" + longPressFurniture.gameObject.name);
//                            }
//                        }
//                    }

//                    if (finger.Up)
//                    {
//                        longPressTarget = null;
//                        pressTime = 0;
//                    }
//                }

//                private void StartMovingAfterLongPress(LeanFinger finger, PlacementRuntime placement, GameObject hitObject)
//                {
//                    // 设置选中对象
//                    selectedObject = placement.gameObject;

//                    // 设置活动手指
//                    activeFinger = finger;

//                    // 立即执行移动初始化逻辑
//                    InitializeMovement(finger, placement);
//                }

//                private void InitializeMovement(LeanFinger finger, PlacementRuntime placement)
//                {
                   
//                }
                

//                public bool CheckPlacementOverlapped(Transform transform, GameObject go, string furnitureName)
//                {
                    
//                    return true;
//                }

//                public void ChangeMaterialIfOverLapped(Transform transform, GameObject go, string furnitureName)
//                {
//                    if (CheckPlacementOverlapped(transform, go, furnitureName))
//                    {
//                        ChangeMaterial(go, matGreen);
//                    }
//                    else
//                    {
//                        ChangeMaterial(go, matRed);
//                    }
//                }

//                // 放置家具虚影
//                public void DisplayPreFurnitureGhost(string furnitureName)
//                {
  
                        
//                }
//                private void InitializeGhostObject(GameObject ghost, Room_Placement_Info info,Room_Placing_Type type, bool isNewObject)
//                {
 
//                }

//                private void ApplyPositionAndRotation(GameObject ghost, PlacementRuntime placement, Room_Placing_Type type)
//                {

//                }

          

//                public void MoveGhostObject(LeanFinger finger)
//                {
                   
//                }



//                // 替换选中家具材质
//                public void ChangeSelectObjectMaterial(GameObject selectObject)
//                {
                    
//                }

//                public void ChangeMaterial(GameObject go, Material mat)
//                {
                 
//                }


//                public bool IsFurnitureInsideRoom(Collider furniture)
//                {
//                    if (roomCollider == null)
//                    {
//                        Debug.LogError("房间碰撞体未设置！");
//                        return true; // 如果未设置房间碰撞体，默认返回 true
//                    }

//                    // 检测家具的所有顶点是否都在房间碰撞体内
//                    Bounds furnitureBounds = furniture.bounds;
//                    Vector3[] corners = new Vector3[8];
//                    corners[0] = furnitureBounds.min;
//                    corners[1] = new Vector3(furnitureBounds.min.x, furnitureBounds.min.y, furnitureBounds.max.z);
//                    corners[2] = new Vector3(furnitureBounds.min.x, furnitureBounds.max.y, furnitureBounds.min.z);
//                    corners[3] = new Vector3(furnitureBounds.min.x, furnitureBounds.max.y, furnitureBounds.max.z);
//                    corners[4] = new Vector3(furnitureBounds.max.x, furnitureBounds.min.y, furnitureBounds.min.z);
//                    corners[5] = new Vector3(furnitureBounds.max.x, furnitureBounds.min.y, furnitureBounds.max.z);
//                    corners[6] = new Vector3(furnitureBounds.max.x, furnitureBounds.max.y, furnitureBounds.min.z);
//                    corners[7] = furnitureBounds.max;

//                    foreach (Vector3 corner in corners)
//                    {
//                        if (!roomCollider.bounds.Contains(corner))
//                        {
//                            return false; // 如果有任意一个顶点超出房间边界，返回 false
//                        }
//                    }

//                    return true; // 所有顶点都在房间边界内，返回 true
//                }

//                // 取消选中家具
//                public void DeselectRoomPlacement()
//                {
//                    Collider[] colliders = selectedObject.GetComponentsInChildren<Collider>();
//                    foreach (var collider in colliders)
//                    {
//                        collider.enabled = true;
//                    }

//                    previousGridCell = null;
//                    currentGridCell = null;
//                    activeFinger = null;
//                    selectedObject = null;
//                    selectedObjectName = null;
//                    //Indoor_Room_Game_Manager._activc_instance.on_deselect_room_placement();
//                }

//            }

//        }
//    }
//}
