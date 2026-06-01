using System;
using System.Collections.Generic;
using System.Numerics;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 单房间存档与运行态：类型/名称/<see cref="roomUID"/>、可 JSON 序列化的家具与花盆（<see cref="PlacementData"/>）、墙地门槽位 ID。
    /// <see cref="GridState"/> 仅内存（<c>[JsonIgnore]</c>），读档后由场景内的 Room 配置与 <c>Room_SO</c> 重建网格。
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public RoomType roomType;

        /// <summary>
        /// 用于展示或区分的名字
        /// </summary>
        public string roomName;

        public string roomUID;

        [JsonIgnore]
        public RoomGridState GridState { get; private set; }

        public List<GridObjectData> placementDatas = new List<GridObjectData>();
        public List<GridObjectData> potDatas = new List<GridObjectData>();

        public int? wallPlacementId;
        public int? floorPlacementId;
        public int? doorPlacementId;

        public RoomData()
        {

        }
        public RoomData(RoomType _roomType, string name, string uid = null)
        {
            if(string.IsNullOrEmpty(uid))
            {
                roomUID = System.Guid.NewGuid().ToString();
            }
            else
            {
                roomUID = uid;
            }

            GridState = new RoomGridState();
            roomName = name;
            roomType = _roomType;
        }

        public RoomData(RoomType _roomType)
        {
            roomUID = System.Guid.NewGuid().ToString();
            GridState = new RoomGridState();
            roomName = _roomType.ToString();
            roomType = _roomType;
        }

        public Int2 GetRandomFloorPosiontion()
        {
            return GridState.GetFloorRandomPosition();
        }
        public bool IsPointInFloor(Vector2 floorPosiontion)
        {
            return GridState.IsPointInFloor(floorPosiontion);
        }

    }

    /// <summary>房间内多层网格的运行时索引（不参与 JSON；由场景布置与 <c>Room_SO</c> 生成）。</summary>
    public class RoomGridState
    {
        private Dictionary<int, GridLayer> _idDic;
        private Dictionary<string, GridLayer> _uidDic;
        private Dictionary<GridLayerType, List<GridLayer>> _typeDic;

        public IReadOnlyDictionary<GridLayerType, List<GridLayer>> TypeDic => _typeDic;

        public IReadOnlyDictionary<int, GridLayer> IdDic => _idDic;

        public IReadOnlyDictionary<string, GridLayer> UIdDic => _uidDic;
        public IEnumerable<GridLayer> GridLayers => _idDic.Values;


        public RoomGridState()
        {
            _typeDic = new Dictionary<GridLayerType, List<GridLayer>>();
            _idDic = new Dictionary<int, GridLayer>();
            _uidDic = new Dictionary<string, GridLayer>();
        }


        #region 增删

        public void AddLayer(GridLayer layer)
        {
            
            if (layer == null) return;

            // id 注入
            if (_idDic.ContainsKey(layer.id))
            {
                //Debug.LogError($"Duplicate GridLayer Id: {layer.id}");
                return;
            }

            _idDic[layer.id] = layer;

            // uid 注入
            if (!string.IsNullOrEmpty(layer.uid))
            {
                if (_uidDic.ContainsKey(layer.uid))
                {
                    //Debug.LogError($"Duplicate GridLayer Uid: {layer.uid}");
                }
                else
                {
                    _uidDic[layer.uid] = layer;
                }
            }

            // type 注入
            if (!_typeDic.TryGetValue(layer.LayerType, out var list))
            {
                list = new List<GridLayer>();
                _typeDic[layer.LayerType] = list;
            }

            list.Add(layer);
        }

        public void RemoveLayer(GridLayer layer)
        {
            if (layer == null) return;

            _idDic.Remove(layer.id);

            if (!string.IsNullOrEmpty(layer.uid))
                _uidDic.Remove(layer.uid);

            if (_typeDic.TryGetValue(layer.LayerType, out var list))
            {
                list.Remove(layer);
                if (list.Count == 0)
                    _typeDic.Remove(layer.LayerType);
            }
        }

        public void ClearAll()
        {
            _idDic.Clear();
            _uidDic.Clear();
            _typeDic.Clear();
        }

        #endregion

        #region 查询
        /// <summary>
        /// 通过 Int ID 获取层
        /// </summary>
        public GridLayer GetLayer(int id)
        {
            _idDic.TryGetValue(id, out var layer);
            return layer;
        }

        /// <summary>
        /// 通过 String UID 获取层 (推荐用于 Surface 多实例查找)
        /// </summary>
        public GridLayer GetLayer(string uid)
        {
            if (string.IsNullOrEmpty(uid)) return null;
            _uidDic.TryGetValue(uid, out var layer);
            return layer;
        }

        /// <summary>
        /// 获取某类型的所有层
        /// </summary>
        public List<GridLayer> GetLayers(GridLayerType type)
        {
            if (_typeDic.TryGetValue(type, out var list))
            {
                return list;
            }
            return null; // 或者返回 new List<GridLayer>()
        }
        public bool TryGetLayer(int id, out GridLayer gridLayer)
        {
            if (_idDic.TryGetValue(id, out gridLayer))
            {
                return true;
            }
            return false;
        }

        public bool TryGetLayer(string uid, out GridLayer gridLayer)
        {
            if (_uidDic.TryGetValue(uid, out gridLayer))
            {
                return true;
            }
            return false;
        }

        public bool TryGetLayers(GridLayerType type, out List<GridLayer> gridLayers)
        {
            if (_typeDic.TryGetValue(type, out gridLayers))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取某类型的第一个层 (可能没有这个层，会返回空)
        /// </summary>
        public GridLayer GetFirstLayer(GridLayerType type)
        {
            if (_typeDic.TryGetValue(type, out var list) && list.Count > 0)
            {
                return list[0];
            }
            return null;
        }

        /// <summary>
        /// 获取某类型的第一个层 (可能没有这个层)
        /// </summary>
        public bool TryGetFirstLayer(GridLayerType type, out GridLayer gridLayer)
        {
            if (_typeDic.TryGetValue(type, out var list) && list.Count > 0)
            {
                gridLayer = list[0];
                return true;
            }
            gridLayer = null;
            return false;
        }


        public string GetGridUId(int id, Int2 pos)
        {
            if (_idDic.TryGetValue(id, out var gridLayer))
            {
                return gridLayer.GetUId(pos);
            }
            return null;
        }

        public List<string> GetGridUids(int id, IEnumerable<Int2> positions)
        {
            if (_idDic.TryGetValue(id, out var gridLayer))
            {
                return gridLayer.GetUIds(positions);
            }
            return null;
        }

        public string GetGridUId(string uid, Int2 pos)
        {
            if (_uidDic.TryGetValue(uid, out var gridLayer))
            {
                return gridLayer.GetUId(pos);
            }
            return null;
        }

        public List<string> GetGridUids(string uid, IEnumerable<Int2> positions)
        {
            if (_uidDic.TryGetValue(uid, out var gridLayer))
            {
                return gridLayer.GetUIds(positions);
            }
            return null;
        }

        public bool TryGetGridUId(int id, Int2 pos, out string gridUid)
        {
            gridUid = null;

            if (_idDic.TryGetValue(id, out var gridLayer))
            {
                gridUid = gridLayer.GetUId(pos);
                return !string.IsNullOrEmpty(gridUid);
            }

            return false;
        }

        public bool TryGetGridUids(int id, IEnumerable<Int2> positions, out List<string> gridUids)
        {
            gridUids = null;

            if (_idDic.TryGetValue(id, out var gridLayer))
            {
                var result = gridLayer.GetUIds(positions);
                if (result != null && result.Count > 0)
                {
                    gridUids = result;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetGridUId(string uid, Int2 pos, out string gridUid)
        {
            gridUid = null;

            if (!string.IsNullOrEmpty(uid) && _uidDic.TryGetValue(uid, out var gridLayer))
            {
                gridUid = gridLayer.GetUId(pos);
                return !string.IsNullOrEmpty(gridUid);
            }

            return false;
        }

        public bool TryGetGridUids(string uid, IEnumerable<Int2> positions, out List<string> gridUids)
        {
            gridUids = null;

            if (!string.IsNullOrEmpty(uid) && _uidDic.TryGetValue(uid, out var gridLayer))
            {
                var result = gridLayer.GetUIds(positions);
                if (result != null && result.Count > 0)
                {
                    gridUids = result;
                    return true;
                }
            }

            return false;
        }

        #endregion


        public bool CheckVaild(int id, Int2 pos)
        {
            if (_idDic.TryGetValue(id, out var gridLayer))
            {
                return gridLayer.CheckVaild(pos);
            }
            return false;
        }

        public bool CheckVaild(int id,IEnumerable<Int2> positions)
        {
            if (_idDic.TryGetValue(id, out var gridLayer))
            {
                return gridLayer.CheckVaild(positions);
            }
            return false;
        }

        public bool CheckVaild(string uid,Int2 pos)
        {
            if(_uidDic.TryGetValue(uid, out var gridLayer))
            {
                return gridLayer.CheckVaild(pos);
            }
            return false;
        }

        public bool CheckVaild(string uid, IEnumerable<Int2> positions)
        {
            if(_uidDic.TryGetValue(uid, out var gridLayer))
            {
                return gridLayer.CheckVaild(positions);
            }
            return false;
        }


        public string SetOccupied(int id, Int2 pos, bool isOccupied)
        {
            if (_idDic.TryGetValue(id, out var gridLayer))
            {
                var uid = gridLayer.SetOccupied(pos, isOccupied);
                return uid;
            }
            return null;
        }

        public string SetOccupied(string uid, Int2 pos, bool isOccupied)
        {
            if (_uidDic.TryGetValue(uid, out var gridLayer))
            {
                var gridUid = gridLayer.SetOccupied(pos, isOccupied);
                return gridUid;
            }
            return null;
        }

        public List<string> SetOccupied(int id , IEnumerable<Int2> positions, bool isOccupied)
        {
            var list = new List<string>();
            foreach (var pos in positions)
            {
                var uid = SetOccupied(id, pos, isOccupied);
                if (!string.IsNullOrEmpty(uid)) list.Add(uid);
            }
            return list;
        }

        public List<string> SetOccupied(string uid, IEnumerable<Int2> positions, bool isOccupied)
        {
            var list = new List<string>();
            foreach (var pos in positions)
            {
                var gridUid = SetOccupied(uid, pos, isOccupied);
                if (!string.IsNullOrEmpty(gridUid)) list.Add(uid);
            }
            return list;
        }

        //public List<string> SetOccupied(GridLayerType type, HashSet<Int2> occupied, bool isOccupied)
        //{
        //    var list = new List<string>();
        //    foreach (var pos in occupied)
        //    {
        //        var uid = SetOccupied(type, pos, isOccupied);
        //        if (string.IsNullOrEmpty(uid)) list.Add(uid);
        //    }
        //    return list;
        //}


        public Int2 GetFloorRandomPosition()
        {
            if (TryGetFirstLayer(GridLayerType.Floor, out var floorLayer))
            {
                return floorLayer.GetRandomPosition();
            }
            return new Int2(0, 0);
        }

        public bool IsPointInFloor(Vector2 position)
        {
            if (TryGetFirstLayer(GridLayerType.Floor, out var floorLayer))
            {
                return floorLayer.IsPoiontIn(position);
            }
            return false;
        }

        /// <summary>
        /// 清空所有层的占据状态
        /// </summary>
        public void ClearAllOccupied()
        {
            foreach (var layer in _idDic.Values)
            {
                layer.ClearAllOccupied();
            }
        }

        /// <summary>
        /// 获取所有格子的UID列表
        /// </summary>
        public List<string> GetAllGridUids()
        {
            var allUids = new List<string>();
            foreach (var layer in _idDic.Values)
            {
                allUids.AddRange(layer.GetAllGridUids());
            }
            return allUids;
        }

    }

}