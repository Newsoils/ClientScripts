using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class MapPanel : UIPanelBase
            {
                private bool isOpen;
                public GameObject panelObj;
                public RectTransform characterIcon;
                public RectTransform livingroomBackground;
                public RectTransform bedroomBackground;
                public RectTransform toiletBackground;
                public RectTransform balconyBackground;

                public Button balcony;
                public Button livingroom;
                public Button bedroom;
                public Button toilet;

                #region 生命周期
                private void Start()
                {
                    EvtDsp.AddEvt(EvtNames.Dispatch_On_Start, OnDispatchStart);
                    EvtDsp.AddEvt(EvtNames.Dispatch_On_End, OnDispatchEnd);
                    if (Dispatch_Manager._instance._player_dispatch_state.player_state == "On_Dispatch")
                    {
                        characterIcon.gameObject.SetActive(false);
                    }
                }
                private void Update()
                {
                    UpdateMousePosition();
                }
                public override void OnDestroy()
                {
                    base.OnDestroy();
                    EvtDsp.RemoveEvt(EvtNames.Dispatch_On_Start, OnDispatchStart);
                    EvtDsp.RemoveEvt(EvtNames.Dispatch_On_End, OnDispatchEnd);
                }
                #endregion
                #region 面板开关
                public override void OpenPanel(params object[] data )
                {
                    panelObj.SetActive(!isOpen);
                    isOpen = !isOpen;
                }
                public override void ClosePanel()
                {
                    panelObj.SetActive(false);
                    isOpen = false;
                }
                #endregion
                #region 位置
                private void UpdateMousePosition()
                {
                    if (IndoorMainCharacter._instance == null) return;
                    var room = IndoorMainCharacter._instance._current_room;
                    if (room == null) return;


                    var floorCollider = room.GetFloorCollider();
                    if (floorCollider == null)
                    {
                        Log.Error("没找到地板的collider" + room.RoomName + "请检查");
                        return;
                    }

                    Vector3 pos = GetNormalizedPositionInCollider(floorCollider);
                    RectTransform image = null;
                    switch (room.RoomName)
                    {
                        case "客厅":
                            image = livingroomBackground;
                            break;
                        case "卧室":
                            image = bedroomBackground;
                            break;
                        case "浴室":
                            image = toiletBackground;
                            break;
                        case "阳台":
                            image = balconyBackground;
                            break;
                    }
                    Vector2 iconPos = MapToImageLocalPosition(image, pos);
                    iconPos = image.TransformPoint(iconPos);
                    iconPos = panelObj.transform.InverseTransformPoint(iconPos);
                    characterIcon.anchoredPosition = iconPos;

                }
                private Vector3 GetNormalizedPositionInCollider(Collider collider)
                {
                    // 获取碰撞箱的边界
                    Bounds bounds = collider.bounds;

                    // 计算物体在边界内的相对位置 (0到1)
                    Vector3 relativePos = bounds.center - IndoorMainCharacter._instance.transform.position;

                    float normalizedX = 1 - ((relativePos.z + bounds.extents.z) / (bounds.size.z));
                    float normalizedY = ((relativePos.x + bounds.extents.x) / (bounds.size.x));
                    return new Vector3(normalizedX, normalizedY, 0);
                }
                private Vector2 MapToImageLocalPosition(RectTransform targetImage, Vector3 normalizedPos)
                {
                    // 获取图片的矩形
                    Rect imageRect = targetImage.rect;

                    // 将标准化位置映射到图片的局部坐标
                    // (0,0) -> (-width/2, -height/2)
                    // (1,1) -> (width/2, height/2)
                    float posX = Mathf.Lerp(-imageRect.width / 2, imageRect.width / 2, normalizedPos.x);
                    float posY = Mathf.Lerp(-imageRect.height / 2, imageRect.height / 2, normalizedPos.y);

                    return new Vector2(posX, posY);
                }
                public void SwitchRoom(string roomName)
                {
                    RoomSystem.Instance.SwitchRoomByName(roomName);
                    ClosePanel();
                }
                #endregion
                #region 派遣
                private void OnDispatchStart()
                {
                    characterIcon.gameObject.SetActive(false);
                }
                private void OnDispatchEnd()
                {
                    characterIcon.gameObject.SetActive(true);
                }
                #endregion
            }
        }
    }
}
