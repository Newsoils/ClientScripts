using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Scene_View_Control;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>场景中的房间根物体：驱动显示与交互；持久化数据在 <see cref="Kernel.RoomData"/>（经 <see cref="RoomSystem"/> 绑定）。</summary>
    public class Room : MonoBehaviour
    {
        private RoomData roomData;

        public RoomData Data => roomData;

        public RoomGridState GridState => roomData.GridState;
        public string RoomUID
        {
            get { return roomData?.roomUID; }
            private set { }
        }

        //在场景中填写，用于和RoomData绑定对应
        public string RoomName;
        public RoomType RoomType;

        public bool isActive = false;

        public Dictionary<string, PlacementRuntime> placementsDic = new Dictionary<string, PlacementRuntime>();
        public Dictionary<string, Pot> potsDic = new Dictionary<string, Pot>();

        //每个gridLayer里面放的Placement
        public Dictionary<string, List<PlacementRuntime>> gridLayerPlacementDic = new Dictionary<string, List<PlacementRuntime>>();
        public Dictionary<string, List<Pot>> gridLayerPotDic = new Dictionary<string, List<Pot>>();

        public List<PlacementRuntime> placements => placementsDic.Values.ToList();
        public List<Pot> pots => potsDic.Values.ToList();

        public List<Renderer> renderers;
        private List<Light> lights = new List<Light>();


        private Dictionary<string, GridLayerTag> tagMap;
        private Dictionary<GridLayerType, List<GridLayerTag>> tagTypeMap;

        [SerializeField]
        private List<GridLayerTag> gridLayerTags = new();

        public Transform WallRoot;
        public Transform DoorRoot;
        public Transform FloorRoot;

        public List<Transform> DoorTs;

        private List<Renderer> wallRenderers = new List<Renderer>();
        private List<Renderer> doorRenderers = new List<Renderer>();
        private List<Renderer> floorRenderers = new List<Renderer>();

        public Dictionary<string,Hide_Wall> wallsMap = new Dictionary<string, Hide_Wall>();  

        void Start()
        {
            ClearRenders();
            GetRenders();
            GetDoorTs();
            GetGridLayers();
            tagMap = new Dictionary<string, GridLayerTag>();
            tagTypeMap = new Dictionary<GridLayerType, List<GridLayerTag>>();

            foreach (var tag in gridLayerTags)
            {
                tagMap[tag.LayerUID] = tag;
                if (!tagTypeMap.ContainsKey(tag.gridLayerType))
                {
                    tagTypeMap[tag.gridLayerType] = new List<GridLayerTag>();
                }
                tagTypeMap[tag.gridLayerType].Add(tag);
            }

            wallRenderers = new List<Renderer>(WallRoot?.GetComponentsInChildren<Renderer>());
            doorRenderers = new List<Renderer>(DoorRoot?.GetComponentsInChildren<Renderer>());
            floorRenderers = new List<Renderer>(FloorRoot?.GetComponentsInChildren<Renderer>());
            lights = new List<Light>(GetComponentsInChildren<Light>());

            var walls = new List<Hide_Wall>(WallRoot?.GetComponentsInChildren<Hide_Wall>());

            foreach(var wall in walls)
            {
                if (!this.wallsMap.ContainsKey(wall.UID))
                {
                    this.wallsMap.Add(wall.UID, wall);
                }
            }

        }


        public void Init(RoomData data)
        {
            roomData = data;
        }

        public bool TryGetLayerTag(string layerUID, out GridLayerTag tag)
        {
            if (tagMap != null && tagMap.TryGetValue(layerUID, out tag))
            {
                return true;
            }
            tag = null;
            return false;
        }

        public bool TryGetLayerTag(GridLayerType type, out List<GridLayerTag> tag)
        {
            if (tagMap != null && tagTypeMap.TryGetValue(type, out var tags))
            {
                if (tags.Count > 0)
                {
                    tag = tags;
                    return true;
                }
            }
            tag = null;
            return false;
        }

        public Transform GetGridOrigin(GridLayerType type)
        {
            if (TryGetLayerTag(type, out var tag))
            {
                return tag[0].girdOrginalPoint;
            }
            return null;
        }

        public Transform GetGridOrigin(string layerUID)
        {
            if (TryGetLayerTag(layerUID, out var tag))
            {
                return tag.girdOrginalPoint;
            }
            return null;
        }

        public void AddGridLayer()
        {

        }

        public void RemoveGridLayer()
        {

        }


        #region 家具
        public void AddPlacementToRoom(PlacementRuntime placement, string gridLayerUId)
        {
            roomData.placementDatas.Add(placement.data);
            placementsDic[placement.data.UID] = placement;

            if (gridLayerPlacementDic.TryGetValue(gridLayerUId, out var placements))
            {
                if (placements == null)
                {
                    placements = new List<PlacementRuntime>();
                    gridLayerPlacementDic[gridLayerUId] = placements;
                }
                placements.Add(placement);
            }
            else
            {
                gridLayerPlacementDic.Add(gridLayerUId, new List<PlacementRuntime>() { placement });
            }

            BindPlacementWallVisibility(placement, gridLayerUId);
        }

        public void AddOtherPlacementToRoomData(GridObjectData placementData)
        {
            roomData.placementDatas.Add(placementData);
        }

        public void RemoveOtherPlacementToRoomDat(GridObjectData placementData)
        {
            roomData.placementDatas.Remove(placementData);
        }

        public void RemovePlacementFromRoom(PlacementRuntime placement, string gridLayerUId)
        {
            roomData.placementDatas.Remove(placement.data);
            placementsDic.Remove(placement.data.UID);

            if (gridLayerPlacementDic.TryGetValue(gridLayerUId, out var placements))
            {
                if (placements != null) placements.Remove(placement);
            }

            UnbindPlacementWallVisibility(placement, gridLayerUId);
        }

        public void BindPlacementWallVisibility(PlacementRuntime placement, string gridLayerUId)
        {
            if (placement == null || string.IsNullOrEmpty(gridLayerUId)) return;

            if (wallsMap.TryGetValue(gridLayerUId, out var hideWall))
            {
                hideWall.OnVisableChange -= placement.SetRenderState;
                hideWall.OnVisableChange += placement.SetRenderState;
            }
        }

        public void UnbindPlacementWallVisibility(PlacementRuntime placement, string gridLayerUId)
        {
            if (placement == null || string.IsNullOrEmpty(gridLayerUId)) return;

            if (wallsMap.TryGetValue(gridLayerUId, out var hideWall))
            {
                hideWall.OnVisableChange -= placement.SetRenderState;
            }
        }

        public Hide_Wall FindClosestFacingWall(Camera camera = null)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            if (camera == null || wallsMap == null) return null;

            Vector3 camDir = camera.transform.forward;
            camDir.y = 0f;
            if (camDir.sqrMagnitude < 1e-6f) return null;
            camDir.Normalize();

            Hide_Wall best = null;
            float minDot = float.PositiveInfinity;
            foreach (var wall in wallsMap.Values)
            {
                if (wall == null) continue;

                Vector3 front = wall._wall_front;
                front.y = 0f;
                if (front.sqrMagnitude < 1e-6f) continue;
                front.Normalize();

                float dot = Vector3.Dot(front, camDir);
                if (dot < minDot)
                {
                    minDot = dot;
                    best = wall;
                }
            }

            return best;
        }

        public void ClearAllPlacements()
        {
            foreach (var placement in placementsDic.Values)
            {
                if (placement != null)
                {
                    Destroy(placement.gameObject);
                }
            }
            placementsDic.Clear();
            gridLayerPlacementDic.Clear();
            roomData.placementDatas.Clear();
            roomData.wallPlacementId = null;
            roomData.floorPlacementId = null;
            roomData.doorPlacementId = null;
        }
        #endregion
        #region 花盆
        public void AddPotToRoom(Pot pot, string gridLayerUId)
        {
            roomData.potDatas.Add(pot.data);
            potsDic[pot.data.UID] = pot;

            if (gridLayerPotDic.TryGetValue(gridLayerUId, out var pots))
            {
                if (pots == null)
                {
                    pots = new List<Pot>();
                    gridLayerPotDic[gridLayerUId] = pots;
                }
                pots.Add(pot);
            }
            else
            {
                gridLayerPotDic.Add(gridLayerUId, new List<Pot> { pot });
            }
        }
        public void RemovePotFromRoom(Pot pot, string gridLayerUId)
        {
            roomData.potDatas.Remove(pot.data);
            potsDic.Remove(pot.data.UID);
            if (gridLayerPotDic.TryGetValue(gridLayerUId, out var pots))
            {
                if (pots != null) pots.Remove(pot);
            }
        }
        public void ClearAllPots()
        {
            foreach (var pot in potsDic.Values)
            {
                if (pot != null)
                {
                    Destroy(pot.gameObject);
                }
            }
            potsDic.Clear();
            gridLayerPotDic.Clear();
            roomData.potDatas.Clear();
        }
        #endregion
        #region Renders管理
        public void GetRenders()
        {
            // 检查renderers是否为空或空列表
            if (renderers == null || renderers.Count == 0)
            {
                // 定义需要排除的层级名称
                string excludedLayerName = "GridLayer";
                // 获取该层级对应的层ID
                int excludedLayer = LayerMask.NameToLayer(excludedLayerName);

                // 获取所有子物体的Renderer组件，并过滤掉指定层级的物体
                renderers = GetComponentsInChildren<Renderer>(true) // true表示包含非激活物体，false则不包含
                    .Where(renderer => renderer.gameObject.layer != excludedLayer) // 排除指定层级的Renderer
                    .ToList();
            }
        }

        public void ClearRenders()
        {
            renderers = null;
        }

        public void GetDoorTs()
        {
            DoorTs = DoorRoot?.GetComponentsInChildren<Transform>()
                             .Where(t => t != DoorRoot.transform)
                             .ToList();
        }

        public void GetGridLayers()
        {
            gridLayerTags = new List<GridLayerTag>(GetComponentsInChildren<GridLayerTag>());
        }

        public void ActiveRoom(bool value)
        {
            if (renderers == null || renderers.Count == 0)
            {
                GetRenders();
            }

            isActive = value;

            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    var hide_wall = renderer.gameObject.GetComponent<Hide_Wall>();
                    if (hide_wall != null)
                    {
                        hide_wall._force_hide = !value;
                    }
                    renderer.enabled = value;
                }
            }
            foreach (var light in lights)
            {
                if (light != null)
                {
                    light.enabled = value;
                }
            }
            foreach (var placement in placementsDic.Values)
            {
                placement.SetRenderState(value);
            }
            foreach (var pot in potsDic.Values)
            {
                pot.SetRenderState(value);
            }
        }

        public void SetWallRender(Material material)
        {
            foreach (var renderer in wallRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }

        public void SetDoorRender(Material material)
        {
            foreach (var renderer in doorRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }

        public void SetFloorRender(Material material)
        {
            foreach (var renderer in floorRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }

        public void SetSpecialDecoration(Placement_Second_Category category, Material material, int placementId)
        {
            switch (category)
            {
                case Placement_Second_Category.Door:
                    if (roomData.doorPlacementId.HasValue)
                        RemoveSpecialDecorationData(category);
                    roomData.doorPlacementId = placementId;
                    SetDoorRender(material);
                    break;
                case Placement_Second_Category.Floor:
                    if (roomData.floorPlacementId.HasValue)
                        RemoveSpecialDecorationData(category);
                    roomData.floorPlacementId = placementId;
                    SetFloorRender(material);
                    break;
                case Placement_Second_Category.Wallpaper:
                    if (roomData.wallPlacementId.HasValue)
                        RemoveSpecialDecorationData(category);
                    roomData.wallPlacementId = placementId;
                    SetWallRender(material);
                    break;
            }
        }

        public int? GetSpecialDecorationId(Placement_Second_Category category)
        {
            return category switch
            {
                Placement_Second_Category.Door => roomData.doorPlacementId,
                Placement_Second_Category.Floor => roomData.floorPlacementId,
                Placement_Second_Category.Wallpaper => roomData.wallPlacementId,
                _ => null
            };
        }

        public void RemoveSpecialDecorationData(Placement_Second_Category category)
        {
            switch (category)
            {
                case Placement_Second_Category.Door:
                    roomData.doorPlacementId = null;
                    break;
                case Placement_Second_Category.Floor:
                    roomData.floorPlacementId = null;
                    break;
                case Placement_Second_Category.Wallpaper:
                    roomData.wallPlacementId = null;
                    break;
            }
        }

        public void RestoreSpecialDecoration(Placement_Second_Category category, int placementId, string resUrl)
        {
            _ = GameAssets.LoadAsync<Material>(resUrl, (mat) =>
             {
                 if (mat != null)
                 {
                     SetSpecialDecoration(category, mat, placementId);
                 }
             });
        }

        #endregion


        public Collider GetFloorCollider()
        {
            return FloorRoot.GetComponentInChildren<Collider>();
        }

        public Vector3 GetRandomFloorPosition()
        {
            var orginal = GetGridOrigin(GridLayerType.Floor);
            if (orginal == null) return Vector3.zero;

            var randomPos = roomData.GetRandomFloorPosiontion();
            var result = orginal.position + new Vector3(randomPos.x, 0, randomPos.y);
            return result;
        }

        public Vector3 GetRandomPosition(GridLayerType gridLayerType)
        {
            Int2 randomPos = Int2.zero;
            if (GridState.TryGetLayers(gridLayerType, out var gridLayers))
            {
                if (gridLayers.Count > 0)
                {
                    var gridLayer = gridLayers[0];
                    randomPos = gridLayer.GetRandomPosition();

                    var origin = GetGridOrigin(gridLayer.uid);
                    if (origin != null)
                    {
                        return origin.position + new Vector3(randomPos.x, 0, randomPos.y);
                    }
                }
            }
            return Vector3.zero;
        }

        public bool Is_FloorPosition_InRoom(Vector3 pos)
        {
            var orginal = GetGridOrigin(GridLayerType.Floor);
            if (orginal != null)
            {
                var relativePos = pos - orginal.position;

                return roomData.IsPointInFloor(new System.Numerics.Vector2(relativePos.x, relativePos.z));
            }
            return false;
        }

    }
}
