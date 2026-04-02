using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class GridSystem : SingletonMono<GridSystem>
    {
        public Transform gridRoot;

        public GameObject gridPrefab;

        public Dictionary<string, MGridView> gridDic = new Dictionary<string, MGridView>();


        // 记录已经打开的 Grid Layer
        private HashSet<string> openedLayers = new HashSet<string>();

        private void Start()
        {
            EvtDsp.AddEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, UpdateOccupyState);
            EvtDsp.AddEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, UpdatePreviewState);
            EvtDsp.AddEvt(EvtNames.RefreshGridView, OnRefreshGridView);
        }

        protected override void OnDestroy()
        {
            EvtDsp.RemoveEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, UpdateOccupyState);
            EvtDsp.RemoveEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, UpdatePreviewState);
            EvtDsp.RemoveEvt(EvtNames.RefreshGridView, OnRefreshGridView);
        }

        /// <summary>
        /// 刷新网格视图 - 将所有网格状态重置为 Normal
        /// </summary>
        private void OnRefreshGridView()
        {
            foreach (var gridView in gridDic.Values)
            {
                if (gridView != null)
                {
                    gridView.SetRealState(MGridState.Normal);
                }
            }
        }

        public void UpdateOccupyState(List<string> uids, MGridState mGridState)
        {
            if (uids == null) return;
            foreach (var uid in uids)
            {
                if (gridDic.TryGetValue(uid, out var mGridView))
                {
                    mGridView.SetRealState(mGridState);
                }
            }
        }

        public void UpdatePreviewState(List<string> uids, MGridState? state)
        {
            if (uids == null) return;
            foreach (var uid in uids)
            {
                if (gridDic.TryGetValue(uid, out var view))
                {
                    view.SetPreviewState(state);
                }
            }
        }

        public static void OpenGridView(Room room, List<GridLayerType> gridLayers)
        {
            if (room == null || gridLayers == null || gridLayers.Count == 0) return;
            foreach (var layertype in gridLayers)
            {
                OpenGridView(room, layertype);
            }
        }


        /// <summary>
        /// 打开GridView（生成GameObject）
        /// </summary>
        /// <param name="room"></param>
        /// <param name="tag"></param>
        public static void OpenGridView(Room room, GridLayerTag tag)
        {
            if (tag == null)
            {
                Debug.LogError($"GridSystem:Tag==Null,无法确定原点坐标位置");
                return;
            }
            var layerType = tag.gridLayerType;
            var layer = room.GridState.GetLayer(tag.LayerUID);
            var orginal = tag.girdOrginalPoint;

            //如果已经打开了，直接返回，避免重复生成网格
            if (Instance.openedLayers.Contains(layer.uid)) return;

            //生成这一层的所有Gird
            Instance.openedLayers.Add(layer.uid);
            foreach (var mGrid in layer.Grids)
            {
                var ob = ObjectPool.Instance.GetObj(Instance.gridPrefab, Instance.gridRoot);
                Unity_Tools.IdentityGameObject(ob);
                var mGridView = ob.GetComponent<MGridView>();
                Vector3 worldPos = orginal.position
                + orginal.right * mGrid.pos.x + orginal.forward * mGrid.pos.y;
                ob.transform.position = worldPos;
                ob.transform.rotation = orginal.rotation;

                ob.name = $"{room.RoomType}_{layerType}_LayerID:{layer.id} _{mGrid.pos.x}_{mGrid.pos.y}";

                mGridView.UId = mGrid.UId;
                Instance.gridDic[mGrid.UId] = mGridView;
            }

            //把场上已经存在的Placement所占据的格子的状态显示一下
            foreach (var placement in room.placements)
            {
                var gridPositions = GridObjectSystem.CalculateOccpiedPos(placement, placement.data.position);
                var uids = room.GridState.GetGridUids(tag.LayerID, gridPositions);
                Instance.UpdateOccupyState(uids, MGridState.Occupied);
            }
        }

        public static void OpenGridView(Room room, GridLayerType layerType)
        {
            if (room.TryGetLayerTag(layerType, out var tags))
            {
                foreach (var tag in tags)
                {
                    OpenGridView(room, tag);
                }
            }
            else
            {
                Debug.LogError($"No GridLayerTag found for room type {room.RoomType} and layertype type {layerType}");
            }
        }

        public static void CloseGridView()
        {
            Instance.openedLayers.Clear();
            var root = Instance.gridRoot;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                var gridView = child.GetComponent<MGridView>();
                if (gridView != null) Instance.gridDic.Remove(gridView.UId);
                ObjectPool.Instance.ReleaseObj(child.gameObject);
            }
        }

        /// <summary>
        /// 生成所有的Grid(编辑器调试使用）
        /// </summary>
        /// <param name="rooms"></param>
        public void GenerateMGridView(List<RoomData> rooms)
        {
            var LayerTags = transform.GetComponentsInChildren<GridLayerTag>();
            foreach (var room in rooms)
            {
                foreach (var (layerId, layer) in room.GridState.IdDic)
                {
                    //需要房间相同并且是同一层的gridLayerTag（地面，天花板，墙）
                    var tag = LayerTags.Where(t => t.roomType == room.roomType && t.gridLayerType == layer.LayerType).FirstOrDefault();
                    foreach (var cell in layer.Grids)
                    {
                        var orginal = tag.girdOrginalPoint;
                        var ob = Instantiate(gridPrefab, gridRoot);

                        Unity_Tools.IdentityGameObject(ob);

                        Vector3 worldPos =
                            orginal.position
                          + orginal.right * cell.pos.x + orginal.forward * cell.pos.y;

                        ob.transform.position = worldPos;
                        ob.transform.rotation = orginal.rotation;

                        ob.name = $"{room.roomType}_{layerId}_{cell.pos.x}_{cell.pos.y}";
                    }
                }
            }

        }



    }
}

