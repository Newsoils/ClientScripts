using System;
using Sirenix.OdinInspector;
using UnityEngine;

public enum CameraState
{
    Normal,
    Placement,
    PlacementTopDown,
    PlacementFrontView,
}

[Serializable]
public class CameraViewData
{
    [Title("中心点设置", Bold = true)]
    public Vector3 pivotOffset;
    public Vector3 pivotEuler;

    [Title("相机自身设置", Bold = true)]
    public Vector3 positionOffset;

    [Header("相机旋转")]
    public Vector3 euler = new Vector3(45f, -45f, 0);


    public float pitch;
    public float yaw;

    [Header("缩放")]
    public float orthographicSize = 18f;

    [Header("Yaw开关")]
    public bool enableYaw = true;
}


[Serializable]
public class RoomCameraData
{
    [GUIColor(0.2f, 0.8f, 1f)] // 浅蓝色
    [Title("房间名称"), LabelWidth(60)]
    [Required] // 👈 空了会变红提示！Odin 最强防呆
    public string roomName;

    [FoldoutGroup("正常视角", Expanded = false)] // 默认折叠
    [HideReferenceObjectPicker]
    public CameraViewData normalView;


    [FoldoutGroup("放置视角", Expanded = false)]
    [HideReferenceObjectPicker]
    public CameraViewData placementView;

    public RoomCameraData(string roomName)
    {
        this.roomName = roomName;
        this.normalView = new CameraViewData();
        this.placementView = new CameraViewData();
    }

    public CameraViewData GetView(CameraState state)
    {
        return state switch
        {
            CameraState.Normal => normalView,
            CameraState.Placement => placementView,
            _ => normalView
        };
    }
}