using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{

    /// <summary>房间内一件摆放物的存档字段（隶属于某个 <see cref="RoomData"/> 的 <c>placementDatas</c>）。</summary>
    [System.Serializable]
    public class PlacementData
    {
        public string UID;
        public string name;
        public int placementId;

        public Int2 position = Int2.zero;

        public Placement_Rotation rotation = Placement_Rotation.Deg0;

        //当前被放在了哪一层
        public string gridLayerUID;   

        public List<string> subInstanceIds = new();

        public bool HasSubPlacements()
        {
            return subInstanceIds != null && subInstanceIds.Count > 0;
        }
      
    }
}