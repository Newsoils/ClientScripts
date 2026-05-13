using System;
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
    /// <summary>
    /// 相机围绕的中心点的位置
    /// </summary>
    public Vector3 pivotOffset;
    /// <summary>
    /// 相机围绕的中心点的旋转
    /// </summary>
    public Vector3 pivotEuler;
    /// <summary>
    /// 相机位置
    /// </summary>
    public Vector3 positionOffset;
    /// <summary>
    /// 相机旋转
    /// </summary>
    public Vector3 euler = new Vector3(45f, -45f, 0);
    public float pitch;
    public float yaw;
    public float orthographicSize = 18f;
    public bool enableYaw = true;



    //[Header("Behavior")]
    //public bool allowMove = true;
    //public bool allowZoom = true;
    //public bool allowRotate = true;
}

[Serializable]
public class RoomCameraData
{
    public string roomName;

    [Header("Views")]
    public CameraViewData normalView;
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