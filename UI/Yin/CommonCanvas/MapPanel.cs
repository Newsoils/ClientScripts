using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
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
                    RefreshCharacterIconVisibility();
                }
                private void Update()
                {
                    if (isOpen)
                        UpdateMousePosition();
                }

                private void LateUpdate()
                {
                    // 晚于一帧内其它脚本对 UI 的改动，避免 panelObj 启用时子节点被还原为默认激活导致头像闪显。
                    RefreshCharacterIconVisibility();
                }
                public override void OnDestroy()
                {
                    base.OnDestroy();
                }
                #endregion
                #region 面板开关
                public override void OpenPanel(params object[] data )
                {
                    panelObj.SetActive(!isOpen);
                    isOpen = !isOpen;
                    RefreshCharacterIconVisibility();
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
                    if (!(Global_Game_Manager.Instance?.ShouldShowIndoorMainCharacter() ?? false))
                        return;
                    if (IndoorMainCharacter._instance == null) return;
                    if (!IndoorMainCharacter._instance.gameObject.activeInHierarchy) return;
                    var room = IndoorMainCharacter._instance._current_room;
                    if (room == null) return;


                    Vector3 pos = GetNormalizedPositionInRoom(room);
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
                private Vector3 GetNormalizedPositionInRoom(Room room)
                {
                    if (room == null)
                    {
                        return Vector3.zero;
                    }
                    var origin = room.GetGridOrigin(GridLayerType.Floor);
                    if (origin == null)
                    {
                        Log.Error("MapPanel: 未找到地板 GridOrigin，无法计算小地图位置。");
                        return Vector3.zero;
                    }
                    if (!room.GridState.TryGetFirstLayer(GridLayerType.Floor, out var floorLayer) || floorLayer == null)
                    {
                        Log.Error("MapPanel: 未找到地板 GridLayer，无法计算小地图位置。");
                        return Vector3.zero;
                    }
                    if (floorLayer.Width <= 0 || floorLayer.Height <= 0)
                    {
                        Log.Error("MapPanel: Floor 网格尺寸非法。");
                        return Vector3.zero;
                    }

                    // 基于房间网格尺寸进行归一化，替代旧的 collider bounds 算法。
                    Vector3 relativePos = IndoorMainCharacter._instance.transform.position - origin.position;
                    float normalizedX = Mathf.Clamp01(relativePos.z / floorLayer.Height);
                    float normalizedY = Mathf.Clamp01(1f - (relativePos.x / floorLayer.Width));
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
                    ClickManager.NotifyMapRoomSwitchConsumedPick();
                    RoomSystem.Instance.SwitchRoomByName(roomName);
                    ClosePanel();
                }
                #endregion
                #region 派遣 / 小地图头像（ShouldShowIndoorMainCharacter，每帧轮询）
                /// <summary>小地图上的小猫头像（规则与 <see cref="Global_Game_Manager.ShouldShowIndoorMainCharacter"/> 一致）。</summary>
                private void RefreshCharacterIconVisibility()
                {
                    if (characterIcon == null)
                        return;
                    bool show = Global_Game_Manager.Instance?.ShouldShowIndoorMainCharacter() ?? false;
                    characterIcon.gameObject.SetActive(show);
                    Global_Game_Manager.Instance?.LogCatPresenceMapPanelIfChanged(show, characterIcon.gameObject.activeSelf);
                }

                #endregion
            }
        }
    }
}
