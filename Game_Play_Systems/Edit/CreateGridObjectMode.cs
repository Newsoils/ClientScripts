using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using DG.Tweening;
using UnityEngine;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class CreateGridObjectMode : IEditMode
    {
        private LayerMask gridLayerMask = LayerMask.GetMask("GridLayer");
        private LayerMask placementMask = LayerMask.GetMask("Placement");

        private GridObject _selected;
        /// <summary>
        /// 当前扫描位置
        /// </summary>
        private Vector3 _curPosition;
        /// <summary>
        /// 当前家具在grid平面上的位置
        /// </summary>
        private Int2 _curGPosition;
        /// <summary>
        /// 当前扫描的层
        /// </summary>
        private GridLayerTag _curGridTag;
        private GridLayerType _lastLayerType = GridLayerType.None;

        private List<string> _lastPreviewGirdViews = new List<string>();

        private const float LIFT_HEIGHT = 0.5f;
        private const float LIFT_DURATION = 0.2f;

        public CreateGridObjectMode(GridObject selected)
        {
            _selected = selected;
        }

        public void Enter()
        {
            Debug.Log("进创建模式");

            if (_selected == null)
            {
                Debug.LogError("进入CreateGridObjectMode时，传入的selected为null");
                return;
            }
            //找到对应需要打开的墙壁
            var placingType = _selected.placingType;

            Enum_Helper.GridLayerMap.TryGetValue(placingType, out var gridLayerTypes);
            //把网格打开
            GridSystem.OpenGridView(RoomSystem.currentRoom, gridLayerTypes);
            EvtDsp.TriggerEvt(EvtNames.RefreshGridView);


            AudioManager.Instance.PlayAudioByRefKey("getPlacement");

            if (_selected is PlacementRuntime placement)
            {
                placement.UpdateBorderMesh();
                placement.SetBorderVisible(true);
            }
        }

        public void Exit()
        {
            CameraManager.Instance.Unfreeze();
            if (_selected is PlacementRuntime placement)
            {
                placement.SetBorderVisible(false);
            }

            GridSystem.CloseGridView();
            if (_selected != null)
            {
                GameObject.Destroy(_selected.gameObject);
            }
            _selected = null;
        }

        public void OnTap(Vector2 screenPos)
        {
            var curP = GridObjectRaycastUtility.RaycastPlacement(screenPos, placementMask, out var hitpos);
            if (curP != null)
            {
                EditManager.Instance.SetMode(new MoveGridObjectMode(curP));
            }
        }

        public void OnLongPress(Vector2 screenPos)
        {
            OnTap(screenPos);
        }

        public void OnDragBegin(Vector2 screenPos)
        {
            CameraManager.Instance.Freeze();
        }

        public void OnDrag(Vector2 screenPos)
        {
            if (_selected == null) return;

            Vector3 pos;

            //找一下当前摆放家具的可放置区域
            var placingType = _selected.placingType;
            if (!Enum_Helper.GridLayerMap.TryGetValue(placingType, out var validLayers))
            {
                pos = Vector3.zero;
                return;
            }

            if (GridObjectRaycastUtility.RaycastGrid(screenPos, gridLayerMask, _selected, out var gridLayerTag, out var alignPos, out var gPos))
            {
                pos = alignPos;
                _curPosition = alignPos;
                _curGridTag = gridLayerTag;
                _curGPosition = gPos;

                // 切换墙层时，自动将墙面家具对齐到新墙的默认朝向
                if (_lastLayerType != GridLayerType.None && _lastLayerType != gridLayerTag.gridLayerType)
                {
                    _selected.SnapToWallDefaultRotation(gridLayerTag.gridLayerType, animate:false);
                }
                _lastLayerType = gridLayerTag.gridLayerType;

                _selected.transform.position = _curPosition + Vector3.up * LIFT_HEIGHT;

                var (x, y) = _selected.GetCurSize();
                EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _lastPreviewGirdViews, null);

                HashSet<Int2> occupiedPositions = GridUtility.CalculateOccpiedPos(x, y, _curGPosition);
                bool isVaild = GridObjectSystem.CheckGridObjectVaild(_selected, RoomSystem.currentRoom, gridLayerTag.LayerUID, _curGPosition);

                var uids = RoomSystem.currentRoom.GridState.GetGridUids(_curGridTag.LayerUID, occupiedPositions);
                EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, uids, isVaild ? MGridState.Highlight : MGridState.Occupied);

                _lastPreviewGirdViews = uids;
                EvtDsp.TriggerEvt(EvtNames.Move_Placement_Panel, _selected.transform.position);
                Debug.Log("拖动中，尝试移动家具");
            }
            if (_selected is PlacementRuntime placement)
            {
                placement.UpdateBorderMesh();
            }
        }

        public void OnDragRelease(Vector2 screenPos)
        {
            CameraManager.Instance.Unfreeze();
            Debug.Log("释放拖动，尝试放置家具");
            if (_selected == null) return;
            if (_curGridTag == null) return;

            EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _lastPreviewGirdViews, null);
            _lastPreviewGirdViews.Clear();
            if (!GridObjectSystem.TryAddGridObject(_selected, RoomSystem.currentRoom, _curGridTag.LayerUID, _curGPosition))
            {
                EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, "空间不够，无法放置");
                GameObject.Destroy(_selected.gameObject);
            }
            else
            {
                if (_selected is PlacementRuntime placement)
                {
                    placement.SetBorderVisible(false);
                }
                _selected.transform.DOKill();
                _selected.transform.DOMove(_curPosition, LIFT_DURATION);
                TaskEvent.TriggerPlaceFurniture();
            }

            _selected = null;
            EvtDsp.TriggerEvt(EvtNames.Close_Edit_Placement_Panel);
            EditManager.Instance.SetMode(new DefaultGridObjectMode());
        }

        public void OnRotate()
        {
            Debug.Log("旋转家具");
            if (_selected == null) return;

            var gridLayrUid = _selected.data.gridLayerUID;

            var pos = _selected.data.position;


            var (rawX, raxY) = _selected.GetCurSize();
            HashSet<Int2> rawOccupiedPositions = GridUtility.CalculateOccpiedPos(rawX, raxY, pos);
            var rawUids = RoomSystem.currentRoom.GridState.GetGridUids(_selected.data.gridLayerUID, rawOccupiedPositions);
            RoomSystem.currentRoom.GridState.SetOccupied(_selected.data.gridLayerUID, rawOccupiedPositions, false);
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, rawUids, MGridState.Normal);

            bool isVaild = GridObjectSystem.CheckGridObjectVaildAfterRotate(_selected, RoomSystem.currentRoom, _selected.data.gridLayerUID, pos);

            if (isVaild)
            {
                // 更新枚举 + 旋转模型


                var (x, y) = _selected.GetRotated90Size();

                _selected.Rotate90();

                //EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _lastPreviewGirdViews, null);
                HashSet<Int2> occupiedPositions = GridUtility.CalculateOccpiedPos(x, y, pos);
                RoomSystem.currentRoom.GridState.SetOccupied(gridLayrUid, occupiedPositions, true);

                //var uids = RoomSystem.currentRoom.GridState.GetGridUids(_curGridTag.LayerUID, occupiedPositions);
                //EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, uids, isVaild ? MGridState.Highlight : MGridState.Occupied);
                //_lastPreviewGirdViews = uids;
            }
            else
            {
                RoomSystem.currentRoom.GridState.SetOccupied(_selected.data.gridLayerUID, rawOccupiedPositions, true);
                EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, "旋转后位置不合法，无法旋转!");
            }
        }



    }
}
