using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_SO", menuName = "Project_Mouse/Room_SO")]
public class Room_SO : ScriptableObject
{
    [System.Serializable]
    public class RoomConfig
    {
        //public string houseName;
        public string RoomName;
        public RoomType RoomType;
    }

    public List<RoomConfig> roomConfigs;

    [System.Serializable]
    public class LayerSize
    {
        public string layer_Name;
        public int id;
        public string layer_UId;
        public string room_Name;
        public RoomType room_Type;
        public GridLayerType layerType;
        public int width;
        public int height;
    }

    /// <summary>
    /// 这里存的是每个房间里面的GridLayer的尺寸信息，比如地板，墙和天花板的网格信息
    /// </summary>
    public List<LayerSize> sizes = new List<LayerSize>();


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
}
