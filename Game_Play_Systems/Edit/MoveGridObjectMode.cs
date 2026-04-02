using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
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

        private List<string> _lastPreviewGirdViews = new List<string>();

        private Int2 _originalGridPos;
        private string _originalLayerUid;
        private GridLayerTag _originalGridTag;
        private List<string> _previewGridUids = new List<string>();
        private Vector3 _originalWorldPosition;
        private float _originalYOffset = 0.5f; // 抬升高度

        private const float LIFT_DURATION = 0.2f; // 抬升动画时长


        private bool hasHighed = false;
        private bool isDraging = false;

        public MoveGridObjectMode(GridObject selected = null)
        {
            _selected = selected;
        }


        public void Enter()
        {
            Debug.Log("进入移动模式");

            CameraManager.Instance.ChangeState(CameraState.Placement);
            if (_selected != null)
            {
                InitMovePlacement(_selected , _selected.transform.position);
            }
        }

        public void Exit()
        {
            GridSystem.CloseGridView();

            // 清理预览高亮
            if (_previewGridUids.Count > 0)
            {
                EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _previewGridUids, null);
                _previewGridUids.Clear();
            }
            _lastPreviewGirdViews.Clear();

            // 如果目标还存在且未放置（例如按ESC取消），恢复原位
            if (_selected != null)
            {
                // 恢复原占用的格子
                var (w, h) = _selected.GetCurSize();
                var occupied = GridUtility.CalculateOccpiedPos(w, h, _originalGridPos);
                RoomSystem.currentRoom.GridState.SetOccupied(_originalLayerUid, occupied, true);
                var uids = RoomSystem.currentRoom.GridState.GetGridUids(_originalLayerUid, occupied);
                EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Occupied);

                // 动画恢复到原始位置（Y 轴也恢复）
                if(!hasHighed)
                {
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

            //换了对象
            if(_selected !=null && curP != _selected)
            {
                _selected.SetBorderVisible(false);
             }
            if (curP != null)
            {
                InitMovePlacement(curP,hitpos);
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

            //移动模式初始化：记录初始位置，用于后续占地格子的计算
            var placingType = _selected.placingType;

            Enum_Helper.GridLayerMap.TryGetValue(placingType, out var gridLayerTypes);

            //把网格打开
            GridSystem.OpenGridView(RoomSystem.currentRoom, gridLayerTypes);
            EvtDsp.TriggerEvt(EvtNames.RefreshGridView);

            _originalLayerUid = _selected.data.gridLayerUID;
            _originalGridPos = _selected.data.position;
            _originalWorldPosition = _selected.transform.position;

            // 找到原始的 GridLayerTag
            if (RoomSystem.currentRoom.TryGetLayerTag(_originalLayerUid, out _originalGridTag) == false)
            {
                _originalGridTag = null;
            }

            // 2. 临时释放原占用的格子
            var (w, h) = _selected.GetCurSize();
            var occupied = GridUtility.CalculateOccpiedPos(w, h, _originalGridPos);
            RoomSystem.currentRoom.GridState.SetOccupied(_originalLayerUid, occupied, false);

            // 3. 更新被释放格子的显示
            var uids = RoomSystem.currentRoom.GridState.GetGridUids(_originalLayerUid, occupied);
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Normal);

            _selected.SetBorderVisible(true);

            // 4. 抬升动画：在记录位置之后再抬升，避免影响原始位置记录
            if(hasHighed)
            {
                _selected.transform.DOMoveY(_selected.transform.position.y + _originalYOffset, LIFT_DURATION);
                hasHighed = false;
            }

            AudioManager.Instance.PlayAudioByRefKey("getPlacement");

            EvtDsp.TriggerEvt<GridObject, string, Vector3>(EvtNames.Open_Edit_Placement_Panel, target, _originalLayerUid, hitpos);
        }

   

        public void OnDragBegin(Vector2 screenPos)
        {
            var curP = GridObjectRaycastUtility.RaycastPlacement(screenPos, placementMask, out var hitpos);

            //换了对象
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

                // 保持抬升高度，只移动 XZ 平面
                _selected.transform.position = new Vector3(_curPosition.x, _originalWorldPosition.y + _originalYOffset, _curPosition.z);

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

            //RaycastGround(screenPos);
            //EvtDsp.TriggerEvt(EvtNames.Move_Placement_Panel, _selected.transform.position);
            //Debug.Log("拖动中，尝试移动家具");
        }

        public void OnDragRelease(Vector2 screenPos)
        {
            // 放置逻辑
            if (_selected == null) return;
            Debug.Log("释放拖动，尝试放置家具");
            _selected.SetBorderVisible(false);

            // 清除预览高亮
            EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, _lastPreviewGirdViews, null);
            _lastPreviewGirdViews.Clear();

            // 检查目标位置是否有效
            var (x, y) = _selected.GetCurSize();
            HashSet<Int2> occupiedPositions = GridUtility.CalculateOccpiedPos(x, y, _curGPosition);
            bool isVaild = GridObjectSystem.CheckGridObjectVaild(_selected, RoomSystem.currentRoom, _curGridTag.LayerUID, _curGPosition);

            if (isVaild)
            {
                // 位置有效，尝试放置
                bool success = GridObjectSystem.TryMoveObject(_selected, RoomSystem.currentRoom, _curGridTag.LayerUID, _curGPosition, _originalGridPos);
                if (success)
                {
                    Debug.Log("家具移动成功");
                    AudioManager.Instance.PlayAudioByRefKey("setPlacement");
                }
                else
                {
                    // 理论上前面已经检查过，这里应该不会走到
                    Debug.LogWarning("移动失败，但位置检查通过");
                    RestoreToOriginalPosition();
                }
            }
            else
            {
                // 位置无效，恢复原位置
                Debug.Log("位置不合法，恢复原位置");
                EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, "空间不够，无法放置");
                RestoreToOriginalPosition();
            }
            EvtDsp.TriggerEvt(EvtNames.Close_Edit_Placement_Panel);

            // 下降动画：恢复到原始 Y 位置
            _selected.transform.DOMoveY(_originalWorldPosition.y, LIFT_DURATION);
            _selected = null;
        }

        /// <summary>
        /// 恢复物体到原始位置和占用
        /// </summary>
        private void RestoreToOriginalPosition()
        {
            if (_selected == null) return;

            // 1. 恢复世界位置
            _selected.transform.position = _originalWorldPosition;

            // 2. 恢复原始格子的占据状态
            var (w, h) = _selected.GetCurSize();
            var occupied = GridUtility.CalculateOccpiedPos(w, h, _originalGridPos);
            RoomSystem.currentRoom.GridState.SetOccupied(_originalLayerUid, occupied, true);

            // 3. 更新网格显示
            var uids = RoomSystem.currentRoom.GridState.GetGridUids(_originalLayerUid, occupied);
            EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, uids, MGridState.Occupied);
        }


        public void OnRotate()
        {
            Debug.Log("旋转家具");
            if (_selected == null) return;

            // 如果还没拖动过，使用原始位置
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
                // 更新枚举 + 旋转模型
        
                //EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, rawUids, MGridState.Normal);

                _selected.Rotate90();


                HashSet<Int2> occupiedPositions = GridUtility.CalculateOccpiedPos(x, y, targetPos);
                RoomSystem.currentRoom.GridState.SetOccupied(targetLayerUid, occupiedPositions, true);

                //var uids = RoomSystem.currentRoom.GridState.GetGridUids(targetLayerUid, occupiedPositions);
                //EvtDsp.TriggerEvt<List<string>, MGridState?>(EvtNames.Update_GridView_Preview_Occupy, uids, MGridState.Highlight);
                //_lastPreviewGirdViews = uids;
            }
            else
            {
                // 恢复原占用
                RoomSystem.currentRoom.GridState.SetOccupied(targetLayerUid, rawOccupiedPositions, true);
                //EvtDsp.TriggerEvt<List<string>, MGridState>(EvtNames.Update_GridView_Occupy, rawUids, MGridState.Occupied);
                EvtDsp.TriggerEvt<string>(EvtNames.Show_Warning_Panel, "旋转后位置不合法，无法旋转!");
            }
        }


    }
}
