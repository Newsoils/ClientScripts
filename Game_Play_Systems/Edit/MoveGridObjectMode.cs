using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using DG.Tweening;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class MoveGridObjectMode : IEditMode
    {
        private LayerMask gridLayerMask = LayerMask.GetMask("GridLayer");
        private LayerMask placementMask = LayerMask.GetMask("Placement");

        private GridObject _selected;
        private Vector3 _curPosition;
        private Int2 _curGPosition;
        private GridLayerTag _curGridTag;

        private List<string> _lastPreviewGirdViews = new List<string>();

        private Int2 _originalGridPos;
        private string _originalLayerUid;
        private GridLayerTag _originalGridTag;
        private GridLayerType _lastLayerType = GridLayerType.None;
        private List<string> _previewGridUids = new List<string>();
        private Vector3 _originalWorldPosition;
        private float _originalYOffset = 0.5f;

        private const float LIFT_DURATION = 0.2f;

        private bool hasHighed = false;
        private bool isDraging = false;

        public MoveGridObjectMode(GridObject selected = null)
        {
            _selected = selected;
        }

        public void Enter()
        {

            CameraManager.Instance.ChangeState(CameraState.Placement);
            if (_selected != null)
            {
                InitMovePlacement(_selected, _selected.transform.position);
            }
        }

        public void Exit()
        {
            GridSystem.CloseGridView();

            if (_previewGridUids.Count > 0)
            {
                EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _previewGridUids, null);
                _previewGridUids.Clear();
            }
            _lastPreviewGirdViews.Clear();

            if (_selected != null)
            {
                var (w, h) = _selected.GetCurSize();
                var occupied = GridUtility.CalculateOccpiedPos(w, h, _originalGridPos);
                RoomSystem.currentRoom.GridState.SetOccupied(_originalLayerUid, occupied, true);
                var uids = RoomSystem.currentRoom.GridState.GetGridUids(_originalLayerUid, occupied);
                EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Occupied);

                if (!hasHighed)
                {
                    _selected.transform.DOKill();
                    _selected.transform.DOMove(_originalWorldPosition, LIFT_DURATION);
                    hasHighed = true;
                }

                _selected.SetBorderVisible(false);
            }
            _selected = null;
        }

        public void OnTap(Vector2 screenPos)
        {
            var curP = GridObjectRaycastUtility.RaycastPlacement(screenPos, placementMask, out var hitpos);

            if (_selected != null && curP != _selected)
            {
                _selected.SetBorderVisible(false);
            }

            if (curP != null)
            {
                InitMovePlacement(curP, hitpos);
                EvtDsp.TriggerEvt(EvtNames.Move_Placement_Panel, _selected.transform.position);
            }
            else
            {
                EvtDsp.TriggerEvt(EvtNames.Close_Edit_Placement_Panel);
                _selected = null;
                EditManager.Instance.SetMode(new DefaultGridObjectMode());
            }
        }

        public void OnLongPress(Vector2 screenPos)
        {
            OnTap(screenPos);
        }

        private void InitMovePlacement(GridObject target, Vector3 hitpos)
        {
            _selected = target;

            var placingType = _selected.placingType;
            Enum_Helper.GridLayerMap.TryGetValue(placingType, out var gridLayerTypes);

            GridSystem.OpenGridView(RoomSystem.currentRoom, gridLayerTypes);
            EvtDsp.TriggerEvt(EvtNames.RefreshGridView);

            _originalLayerUid = _selected.data.gridLayerUID;
            _originalGridPos = _selected.data.position;
            _originalWorldPosition = _selected.transform.position;

            if (!RoomSystem.currentRoom.TryGetLayerTag(_originalLayerUid, out _originalGridTag))
            {
                _originalGridTag = null;
            }

            var (w, h) = _selected.GetCurSize();
            var occupied = GridUtility.CalculateOccpiedPos(w, h, _originalGridPos);
            RoomSystem.currentRoom.GridState.SetOccupied(_originalLayerUid, occupied, false);

            var uids = RoomSystem.currentRoom.GridState.GetGridUids(_originalLayerUid, occupied);
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Normal);

            _selected.SetBorderVisible(true);

            if (hasHighed)
            {
                _selected.transform.DOKill();
                _selected.transform.DOMoveY(_selected.transform.position.y + _originalYOffset, LIFT_DURATION);
                hasHighed = false;
            }

            AudioManager.Instance.PlayAudioByRefKey("getPlacement");
            EvtDsp.TriggerEvt<GridObject, string, Vector3>(EvtNames.Open_Edit_Placement_Panel, target, _originalLayerUid, hitpos);
        }

        public void OnDragBegin(Vector2 screenPos)
        {
            var curP = GridObjectRaycastUtility.RaycastPlacement(screenPos, placementMask, out var hitpos);

            if (_selected != null && curP != _selected)
            {
                _selected.SetBorderVisible(false);
            }
            InitMovePlacement(curP, hitpos);
        }

        public void OnDrag(Vector2 screenPos)
        {
            if (_selected == null) return;

            Vector3 pos;
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
                    _selected.SnapToWallDefaultRotation(gridLayerTag.gridLayerType, animate: false);
                }
                _lastLayerType = gridLayerTag.gridLayerType;

                _selected.transform.position = GetLiftedPosition(_curPosition);

                var (x, y) = _selected.GetCurSize();
                EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _lastPreviewGirdViews, null);

                HashSet<Int2> occupiedPositions = GridUtility.CalculateOccpiedPos(x, y, _curGPosition);
                bool isVaild = GridObjectSystem.CheckGridObjectVaild(_selected, RoomSystem.currentRoom, gridLayerTag.LayerUID, _curGPosition);

                var uids = RoomSystem.currentRoom.GridState.GetGridUids(_curGridTag.LayerUID, occupiedPositions);
                EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, uids, isVaild ? MGridState.Highlight : MGridState.Occupied);
                _lastPreviewGirdViews = uids;
                EvtDsp.TriggerEvt(EvtNames.Move_Placement_Panel, _selected.transform.position);
            }

            if (_selected is PlacementRuntime placement)
            {
                placement.UpdateBorderMesh();
            }
        }

        public void OnDragRelease(Vector2 screenPos)
        {
            if (_selected == null) return;

            Debug.Log("释放拖动，尝试移动家具");
            _selected.SetBorderVisible(false);

            EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _lastPreviewGirdViews, null);
            _lastPreviewGirdViews.Clear();

            var (x, y) = _selected.GetCurSize();
            HashSet<Int2> occupiedPositions = GridUtility.CalculateOccpiedPos(x, y, _curGPosition);
            bool isVaild = GridObjectSystem.CheckGridObjectVaild(_selected, RoomSystem.currentRoom, _curGridTag.LayerUID, _curGPosition);

            if (isVaild)
            {
                bool success = GridObjectSystem.TryMoveObject(
                    _selected,
                    RoomSystem.currentRoom,
                    _curGridTag.LayerUID,
                    _curGPosition,
                    _originalLayerUid,
                    _originalGridPos);

                if (success)
                {
                    AudioManager.Instance.PlayAudioByRefKey("setPlacement");
                    _selected.transform.DOKill();
                    _selected.transform.DOMove(_curPosition, LIFT_DURATION);
                }
                else
                {
                    RestoreToOriginalPosition();
                    _selected.transform.DOKill();
                    _selected.transform.DOMove(_originalWorldPosition, LIFT_DURATION);
                }
            }
            else
            {
                EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, "空间不够，无法放置");
                RestoreToOriginalPosition();
                _selected.transform.DOKill();
                _selected.transform.DOMove(_originalWorldPosition, LIFT_DURATION);
            }

            EvtDsp.TriggerEvt(EvtNames.Close_Edit_Placement_Panel);
            _selected = null;
        }

        private void RestoreToOriginalPosition()
        {
            if (_selected == null) return;

            _selected.transform.position = _originalWorldPosition;

            var (w, h) = _selected.GetCurSize();
            var occupied = GridUtility.CalculateOccpiedPos(w, h, _originalGridPos);
            RoomSystem.currentRoom.GridState.SetOccupied(_originalLayerUid, occupied, true);

            var uids = RoomSystem.currentRoom.GridState.GetGridUids(_originalLayerUid, occupied);
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Occupied);
        }

        public void OnRotate()
        {
            if (_selected == null) return;

            Int2 targetPos = _selected.data.position;
            var targetLayerUid = _selected.data.gridLayerUID;

            var (x, y) = _selected.GetRotated90Size();

            var (rawX, rawY) = _selected.GetCurSize();
            HashSet<Int2> rawOccupiedPositions = GridUtility.CalculateOccpiedPos(rawX, rawY, targetPos);
            var rawUids = RoomSystem.currentRoom.GridState.GetGridUids(targetLayerUid, rawOccupiedPositions);

            RoomSystem.currentRoom.GridState.SetOccupied(targetLayerUid, rawOccupiedPositions, false);

            bool isVaild = GridObjectSystem.CheckGridObjectVaildAfterRotate(_selected, RoomSystem.currentRoom, targetLayerUid, targetPos);

            if (isVaild)
            {
                _selected.Rotate90();

                HashSet<Int2> occupiedPositions = GridUtility.CalculateOccpiedPos(x, y, targetPos);
                RoomSystem.currentRoom.GridState.SetOccupied(targetLayerUid, occupiedPositions, true);
            }
            else
            {
                RoomSystem.currentRoom.GridState.SetOccupied(targetLayerUid, rawOccupiedPositions, true);
                EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, "旋转后位置不合法，无法旋转!");
            }
        }

        private Vector3 GetLiftedPosition(Vector3 basePosition)
        {
            return basePosition + Vector3.up * _originalYOffset;
        }
    }
}
