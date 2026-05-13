using System;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using DG.Tweening;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public string UId => data.UID;
    public Room_Placing_Type placingType;
    public Transform Root;
    [Header("Data")]
    public GridObjectData data = new GridObjectData();
    public GridData gridData = new GridData(0, 0, 0);
    public Room room;

    [Header("Border")]
    private GameObject currentBorder;
    private bool borderVisible;

    public void Init(string name, int id, GridData gridData, Room_Placing_Type placingType)
    {
        data.name = name;
        data.placementId = id;
        data.UID = Guid.NewGuid().ToString();
        this.gridData = gridData;
        this.placingType = placingType;
        if (Root == null) Root = transform.Find("Root");
        room = RoomSystem.GetRoomByFloorPosition(transform.position);
    }

    /// <summary>
    /// 交互配置（InteractPositionSO）里 <see cref="InteractPositionInfo.position"/> 的本地空间：优先子物体 Root，与家具预制体一致。
    /// </summary>
    public Transform GetInteractLocalTransform()
    {
        if (Root == null) Root = transform.Find("Root");
        return Root != null ? Root : transform;
    }

    public virtual void Rotate90()
    {
        // 将当前枚举值转换为整数，加1后取模，再转回枚举
        int current = (int)data.rotation;
        int next = (current + 1) % 4;
        data.rotation = (Placement_Rotation)next;

        float baseAngle = 0f;
        if (placingType == Room_Placing_Type.Wall_Furniture && room != null && room.TryGetLayerTag(data.gridLayerUID, out var gridLayerTag))
        {
            baseAngle = gridLayerTag.gridLayerType switch
            {
                GridLayerType.Wall_E => 90f,
                GridLayerType.Wall_S => 180f,
                GridLayerType.Wall_W => 270f,
                _ => 0f
            };
        }

        float angle = baseAngle + (int)data.rotation * 90f;
        Vector3 endValue = new Vector3(0, angle, 0);
        Root.transform.DORotate(endValue, 0.3f);
        // 如果将来需要处理子物体或特殊逻辑，可在此扩展
    }

    /// <summary>
    /// 计算墙面家具在指定 GridLayerType 下的默认旋转枚举值（不改变当前旋转枚举）
    /// </summary>
    public static Placement_Rotation CalculateWallDefaultRotation(GridLayerType layerType)
    {
        return layerType switch
        {
            GridLayerType.Wall_E => Placement_Rotation.Deg90,
            GridLayerType.Wall_S => Placement_Rotation.Deg180,
            GridLayerType.Wall_W => Placement_Rotation.Deg270,
            _ => Placement_Rotation.Deg0
        };
    }

    /// <summary>
    /// 根据当前所在的墙层，自动将旋转对齐到该墙的默认朝向（立即生效，无动画）。
    /// 仅对墙面家具有效。如果旋转已经是对应墙的默认朝向则不操作。
    /// </summary>
    /// <param name="gridLayerType">当前墙层类型</param>
    /// <param name="animate">是否播放动画</param>
    public void SnapToWallDefaultRotation(GridLayerType gridLayerType, bool animate = false)
    {
        if (placingType != Room_Placing_Type.Wall_Furniture)
            return;

        var targetRotation = CalculateWallDefaultRotation(gridLayerType);
        if (data.rotation == targetRotation)
            return;


        float baseAngle = gridLayerType switch
        {
            GridLayerType.Wall_E => 90f,
            GridLayerType.Wall_S => 180f,
            GridLayerType.Wall_W => 270f,
            _ => 0f
        };
        float angle = baseAngle + (int)data.rotation * 90f;
        Vector3 endValue = new Vector3(0, angle, 0);

        if (animate)
            Root.transform.DORotate(endValue, 0.3f);
        else
            Root.localRotation = Quaternion.Euler(endValue);

        data.rotation = targetRotation;

    }

    public virtual void Rotate90Back()
    {
        int current = (int)data.rotation;
        int next = (current - 1) % 4;
        data.rotation = (Placement_Rotation)next;

        // 同时旋转模型
        Root.transform.Rotate(0, 90, 0); // 顺时针旋转90度

        //float angle = (int)data.rotation * 90f;
        //Vector3 endValue = new Vector3(0, angle, 0);
        //Root.transform.DORotate(endValue, 0.3f);
    }

    public (int length, int width) GetCurSize()
    {
        if (placingType == Room_Placing_Type.Wall_Furniture)
        {
            return (gridData.length, gridData.height);
        }

        bool swap = data.rotation == Placement_Rotation.Deg90 || data.rotation == Placement_Rotation.Deg270;
        return swap
            ? (gridData.width, gridData.length)   // 交换长宽
            : (gridData.length, gridData.width);  // 保持原样
    }


    /// <summary>
    /// 返回旋转后的尺寸用于计算是否能摆正
    /// </summary>
    /// <returns></returns>
    public (int length, int width) GetRotated90Size()
    {
        if (placingType == Room_Placing_Type.Wall_Furniture)
        {
            return (gridData.length, gridData.height);
        }

        int current = (int)data.rotation;
        int next = (current + 1) % 4;
        var nextR = (Placement_Rotation)next;
        bool swap = nextR == Placement_Rotation.Deg90 || nextR == Placement_Rotation.Deg270;
        return swap
            ? (gridData.width, gridData.length)   // 交换长宽
            : (gridData.length, gridData.width);  // 保持原样
    }

    /// <summary>
    /// 特定旋转下的尺寸
    /// </summary>
    /// <returns></returns>
    public (int length, int width) GetRotatedSize(Placement_Rotation rotation)
    {
        if (placingType == Room_Placing_Type.Wall_Furniture)
        {
            return (gridData.length, gridData.height);
        }

        bool swap = rotation == Placement_Rotation.Deg90 || rotation == Placement_Rotation.Deg270;
        return swap
            ? (gridData.width, gridData.length)   // 交换长宽
            : (gridData.length, gridData.width);  // 保持原样
    }

    public void ApplyPositon(Room room)
    {
        this.room = room;
        room.TryGetLayerTag(data.gridLayerUID,out var gridLayerTag);

        GridUtility.CalucateGridPosition(gridLayerTag, data.position, out var worldPos);

        if (gridLayerTag != null)
        {
            switch (gridLayerTag.gridLayerType)
            {
                case GridLayerType.Wall_S:
                case GridLayerType.Wall_N:
                    worldPos.z = gridLayerTag.girdOrginalPoint.position.z - gridData.width;
                    break;
                case GridLayerType.Wall_W:
                case GridLayerType.Wall_E:
                    worldPos.x = gridLayerTag.girdOrginalPoint.position.x - gridData.width;
                    break;
            }
        }

        transform.position = worldPos;
    }
    public void ApplyRotation()
    {
        // 根据枚举值设置模型的本地旋转
        //float baseAngle = 0f;
        //if (placingType == Room_Placing_Type.Wall_Furniture && room != null && room.TryGetLayerTag(data.gridLayerUID, out var gridLayerTag))
        //{
        //    baseAngle = gridLayerTag.gridLayerType switch
        //    {
        //        GridLayerType.Wall_E => 90f,
        //        GridLayerType.Wall_S => 180f,
        //        GridLayerType.Wall_W => 270f,
        //        _ => 0f
        //    };
        //}

        float angle =  (int)data.rotation * 90f;
        Root.localRotation = Quaternion.Euler(0, angle, 0);
        // 如果有边界框等需要同步旋转，也在这里处理
    }


    //========================
    // Border System
    //========================

    public void UpdateBorderMesh()
    {
        if (currentBorder != null)
            Destroy(currentBorder);

        currentBorder = CreateBorder();

        // Border 放在 Root 下，localPosition 和 localRotation 都设为 identity
        // 因为 Root 已经包含了 meshSize/2 的偏移
        // Border 生成的网格是以自身为中心的矩形，Root 旋转时会带着 Border 一起旋转
        currentBorder.transform.localPosition = Vector3.zero;
        currentBorder.transform.localRotation = Quaternion.identity;

        currentBorder.SetActive(borderVisible);
    }

    public void SetBorderVisible(bool visible)
    {
        borderVisible = visible;
        if (currentBorder != null)
            currentBorder.SetActive(visible);
    }

    private GameObject CreateBorder()
    {
        var go = new GameObject("Border");
        // Border 放在 Root 下，这样 ApplyRotation 旋转 Root 时 Border 会一起旋转
        go.transform.SetParent(Root, false);

        var generator = go.AddComponent<PlacementBorderGenerator>();
        generator.width = gridData.width;
        generator.length = gridData.length;
        generator.GenerateBorderMesh_DS();

        var renderer = go.GetComponent<MeshRenderer>();
        if (renderer && GameAssets.Instance)
            renderer.material = GameAssets.Instance.funiture_Border_Mat;

        return go;
    }

}
[Serializable]
public class GridData
{
    public int length, width, height;
    public GridData(int length, int width, int height)
    {
        this.length = length;
        this.width = width;
        this.height = height;
    }
}
