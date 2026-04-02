//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using CLIP.Project_Mouse.Scene_View_Control;
//using UnityEngine;

//namespace CLIP.Project_Mouse.Game_Play_System.Indoor_System
//{
//    public class Indoor_Game_Grid_Manager : MonoBehaviour
//    {
//        [Header("Layers")]
//        public LayerMask _wall_layer_mask;
//        public List<Collider> _inside_boxes;
//        public List<Collider> _remove_boxes;

//        public List<Hide_Wall> _wall_ref = new List<Hide_Wall>();
//        public List<Transform> _wall_grid_roots = new List<Transform>();

//        public Transform _ceiling_grid_root;
//        public Transform _floor_grid_root;
//        public bool _show_floor = false;
//        public bool _show_wall = false;
//        public bool _show_ceiling = false;

//        public LayerMask _check_mask;
//        [Header("Material")]
//        public Material _ceil_material_normal;
//        public Material _ceil_material_occupied;

//        public void set_floor_state(bool flag)
//        {
//            if(_show_floor == flag) return;
//            _show_floor = flag;
//            _floor_grid_root?.gameObject.SetActive(flag);
//        }
//        public void set_wall_state(bool _flag)
//        {
//            _show_wall = _flag;
//            RefreshWallDisplay();
//        }
//        public void set_ceiling_state(bool flag)
//        {
//            if (_show_ceiling == flag) return;
//            _show_ceiling = flag;

//            _ceiling_grid_root?.gameObject.SetActive(flag);
//        }

//        // Only update wall display when something changes
//        private void RefreshWallDisplay()
//        {
//            for (int i = 0; i < _wall_grid_roots.Count; i++)
//            {
//                //bool show = _show_wall && _wall_ref[i]._renderer.enabled;
//                //_wall_grid_roots[i].gameObject.SetActive(show);
//            }
//        }

//        //看下能不能避免FixedUpdate
//        private void FixedUpdate()
//        {
//            //set_floor_state(_show_floor);
//            //set_ceiling_state(_show_ceiling);
//            //if (_show_wall == false)
//            //{
//            //    for (int i = 0; i < 4; i++)
//            //    {
//            //        _wall_grid_roots[i].gameObject.SetActive(false);
//            //        _wall_grid_roots[i].gameObject.SetActive(false);
//            //        _wall_grid_roots[i].gameObject.SetActive(false);
//            //        _wall_grid_roots[i].gameObject.SetActive(false);
//            //    }
//            //    return;
//            //}
//            //for (int i = 0; i < 4; i++)
//            //{
//            //    if (_wall_ref[i]._renderer.enabled == true)
//            //    {
//            //        _wall_grid_roots[i].gameObject.SetActive(true);
//            //    }
//            //    else
//            //    {
//            //        _wall_grid_roots[i].gameObject.SetActive(false);
//            //    }
//            //}
//        }

//        public void clear_cells()
//        {
//            var _cells = this.gameObject.GetComponentsInChildren<Game_Grid_Cell>();
//            for (int i = 0; i < _cells.Length; i++)
//            {
//                var _cell = _cells[i];
//                var _box = _cell._collider;
//                Collider[] results = new Collider[16];
//                int count = Physics.OverlapBoxNonAlloc(_box.bounds.center, _box.bounds.extents, results,
//                    Quaternion.identity, _wall_layer_mask);
//                bool has_inside = false;
//                bool has_remove = false;
//                for (int j = 0; j < count; j++)
//                {
//                    Collider hit = results[j];
//                    if (_inside_boxes.Contains(hit) == true) has_inside = true;
//                    if (_remove_boxes.Contains(hit) == true) has_remove = true;
//                }

//                if (has_inside == false || has_remove == true)
//                {
//#if UNITY_EDITOR
//                    DestroyImmediate(_cell.gameObject);
//#else
//           Destroy(this.gameObject);
//#endif
//                }
//            }
//        }

//        public void Refresh_cells_state()
//        {
//            var _placement_list = Indoor_Room_Game_Manager._activc_instance._placement_list;

//            var _cells = this.gameObject.GetComponentsInChildren<Game_Grid_Cell>();

//            foreach (var _p in _placement_list)
//            {
//                var _collider = _p._grid_collider;
//                Collider[] _hits = new Collider[64];
//                int _hit_count = Physics.OverlapBoxNonAlloc(_collider.bounds.center, _collider.bounds.extents,
//                    _hits, Quaternion.identity, _check_mask, QueryTriggerInteraction.Collide);

//                for (int i = 0; i < _hit_count; i++)
//                {
//                    var _cell = _hits[i].gameObject.transform.parent.GetComponent<Game_Grid_Cell>();
//                    if (_cell != null)
//                    {
//                        if (_cell._parent_placement == _p) continue;
//                        _cell._object_occupied = _p.gameObject;
//                    }
//                }
//            }
//            foreach (var item in _cells)
//            {
//                item.Show_Display();
//            }


//        }
        
//    }
//}