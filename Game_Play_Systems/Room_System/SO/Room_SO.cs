using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_SO", menuName = "Project_Mouse/Room_SO")]
public class Room_SO : SerializedScriptableObject
{
    // ======================= 房间配置 =======================
    [Title("房间配置", Bold = true, TitleAlignment = TitleAlignments.Left)]
    
    [ListDrawerSettings(
        ShowFoldout = true,          // 每个房间可折叠
        ListElementLabelName = nameof(RoomConfig.RoomName), // 显示房间名
        DraggableItems = true,       // 可拖拽排序
        ShowIndexLabels = false
    )]
    public List<RoomConfig> roomConfigs;

    // ======================= 网格层尺寸 =======================
    [Title("网格层尺寸配置", Bold = true)]
    //[ListDrawerSettings(
    //    //ShowFoldout = true,
    //    //ListElementLabelName = nameof(LayerSize.layer_Name),
    //    DraggableItems = true,
    //    ShowIndexLabels = false
    //)]
    public List<LayerSize> sizes = new List<LayerSize>();

    // ======================= 方法 =======================
    [GUIColor(1, 0.8f, 0.2f)]
    public bool TryGetSize(GridLayerType type, out int w, out int h)
    {
        foreach (var s in sizes)
        {
            if (s.layerType == type)
            {
                w = s.width;
                h = s.height;
                return true;
            }
        }

        w = h = 0;
        return false;
    }


    // ======================= 子结构体 =======================
    [System.Serializable]
    public class RoomConfig
    {
        [GUIColor(0.1f, 0.8f, 1f)]
        [Required]
        [LabelText("房间名称")]
        public string RoomName;

        [LabelText("房间类型")]
        public RoomType RoomType;
    }

    [System.Serializable]
    public class LayerSize
    {
        [LabelText("层名称")]
        public string layer_Name;

        [LabelText("层 ID")]
        public int id;

        [Required]
        [GUIColor("green")]
        [LabelText("层 UID")]
        public string layer_UId;

        [LabelText("归属房间")]
        public string room_Name;

        [LabelText("房间类型")]
        public RoomType room_Type;

        [LabelText("网格层类型")]
        public GridLayerType layerType;

        [LabelText("宽度")]
        public int width;

        [LabelText("高度")]
        public int height;
    }

}

