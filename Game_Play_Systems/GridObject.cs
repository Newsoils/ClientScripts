using System;
using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
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
    public PlacementData data = new PlacementData();
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

        float angle = (int)data.rotation * 90f;
        Vector3 endValue = new Vector3(0, angle, 0);
        Root.transform.DORotate(endValue, 0.3f);
        // 如果将来需要处理子物体或特殊逻辑，可在此扩展
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
        bool swap = rotation == Placement_Rotation.Deg90 || rotation == Placement_Rotation.Deg270;
        return swap
            ? (gridData.width, gridData.length)   // 交换长宽
            : (gridData.length, gridData.width);  // 保持原样
    }

    public void ApplyPositon(Room room)
    {
        Vector3 position = room.GetGridOrigin(data.gridLayerUID).position + new Vector3(data.position.x, 0, data.position.y);

        transform.position = position;
    }
    public void ApplyRotation()
    {
        // 根据枚举值设置模型的本地旋转
        float angle = (int)data.rotation * 90f;
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
