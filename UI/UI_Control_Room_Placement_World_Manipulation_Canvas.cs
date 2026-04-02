//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.UI;
//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI

//        {
//            public class UI_Control_Room_Placement_World_Manipulation_Canvas : MonoBehaviour
//            {
//            //    public PlacementRuntime _room_placement_in_level;
//            //    public Image _btn_move_img;
//            //    public bool _on_move = false;
//            //    public Color _off_move_color;
//            //    public Color _on_move_color;

//            //    private bool _moveBtnCooldown = false;


//            //    void Start()
//            //    {
//            //        _room_placement_in_level=this.GetComponentInParent<PlacementRuntime>();
//            //    }
//            //    private void OnEnable()
//            //    {
//            //        _room_placement_in_level = this.GetComponentInParent<PlacementRuntime>();
//            //        if (_btn_move_img != null)
//            //        {
//            //            _btn_move_img.color = _off_move_color;
//            //            _btn_move_img.GetComponent<Button>().onClick.AddListener(on_toggle_move);
//            //        }
//            //    }

//            //    // Update is called once per frame
//            //    void Update()
//            //    {
//            //        //if (_room_placement_in_level == null)
//            //        //{
//            //        //    return;
//            //        //}

//            //        //// 获取主摄像机
//            //        //Camera mainCamera = Camera.main;
//            //        //if (mainCamera == null)
//            //        //{
//            //        //    Debug.LogWarning("主摄像机未找到，无法更新 UI 位置！");
//            //        //    return;
//            //        //}

//            //        //// 将 PlacementRuntime 的世界坐标转换为屏幕坐标
//            //        //Vector3 screenPosition = mainCamera.WorldToScreenPoint(gameObject.transform.position);

//            //        //// 检查 z 分量是否有效
//            //        //if (screenPosition.z <= 0)
//            //        //{
//            //        //    Debug.LogWarning("物体在摄像机后方，无法转换屏幕坐标！");
//            //        //    return;
//            //        //}

//            //        //Vector2 centeredPosition = new Vector2(screenPosition.x - (Screen.width / 2),screenPosition.y - (Screen.height / 2));

//            //        //// 更新 UI 的位置
//            //        //RectTransform rectTransform = Indoor_Room_Game_Manager._activc_instance.roomPlacementMenu.GetComponent<RectTransform>();
//            //        //rectTransform.anchoredPosition = centeredPosition;
//            //    }

//            //    public void OnDisable()
//            //    {
//            //        if(_room_placement_in_level!=null) _room_placement_in_level.On_All_Placement_Deselected();
//            //    }

//            //    public void on_toggle_move()
//            //    {
//            //        //Debug.Log("on_toggle_move");
//            //        if (_moveBtnCooldown) return; // 冷却中直接返回
//            //        _moveBtnCooldown = true;
//            //        StartCoroutine(MoveBtnCooldownCoroutine(0.3f));

//            //        if (_room_placement_in_level != null) {
//            //            if (_on_move == false)
//            //            {

//            //                _room_placement_in_level.on_enter_move();
                            
//            //                _on_move = true;
//            //                _btn_move_img.color = _on_move_color;
//            //            }
//            //            else
//            //            {
//            //                _room_placement_in_level.on_exit_move();
//            //                _on_move = false;

//            //                _btn_move_img.color = _off_move_color;
//            //            }
                       
                    
//            //        }
//            //    }

                
//            //    public void on_delete()
//            //    {
//            //        Indoor_Room_Game_Manager._activc_instance.on_delete_room_placement(_room_placement_in_level);
//            //        if (_room_placement_in_level != null) _room_placement_in_level.on_delete();
//            //    }

//            //    public IEnumerator MoveBtnCooldownCoroutine(float delay)
//            //    {
//            //        yield return new WaitForSeconds(delay);
//            //        _moveBtnCooldown = false;
//            //    }

                
//            //}
//        }
//    }
//}