using CLIP.Project_Mouse.ENUM;
using UnityEngine;

/// <summary>
/// 这个类用来标记墙体所属的房间和网格层，在RoomMeasureTool测量数据的时候会用到
/// </summary>
public class GridLayerTag : MonoBehaviour
{
    public int LayerID;          // 使用场景内自增ID，便于阅读
    public string LayerUID;     // 使用GUID，绝对唯一

    public RoomType roomType;
    public string roomName;

    public GridLayerType gridLayerType;
    public Transform girdOrginalPoint;

    public Vector2 GetDataPos(Vector3 p)
    {
        Vector3 postionOnPlane =  p - girdOrginalPoint.position;
        Vector2 placementPos = new Vector2();
        switch (gridLayerType)
        {
            case GridLayerType.Ceiling:
            case GridLayerType.Floor:
                placementPos = new Vector2(postionOnPlane.x, postionOnPlane.z);
                break;
            case GridLayerType.Wall_N:
            case GridLayerType.Wall_S:
                placementPos = new Vector2(postionOnPlane.x, postionOnPlane.y);
                break;
            case GridLayerType.Wall_W:
                placementPos = new Vector2(postionOnPlane.z, postionOnPlane.y);
                break;
            case GridLayerType.Wall_E:
                placementPos = new Vector2(-postionOnPlane.z, postionOnPlane.y);
                break;
        }

        return placementPos;
    }
}
