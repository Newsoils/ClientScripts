using System.Collections;
using CLIP.Project_Mouse.Game_Play_System.Planting_System;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
  
    public class Game_Grid_Cell : MonoBehaviour
    {
        //    public Collider _collider;
        //    public string cell_usage;
        //    public Renderer _display_render;
        //    public Vector3 cell_position;
        //    public Vector3 cell_orient = new Vector3(0, 1, 0);
        //    public Material _normal_mat;
        //    public Material _selected_mat;
        //    public Material _occupied_mat;

        //    public GameObject _object_occupied;

        //    public PlacementRuntime _parent_placement;

        //    public void Start()
        //    {
        //        _display_render = GetComponentInChildren<Renderer>();
        //        if (_display_render != null)
        //        {
        //            // _display_render.enabled = false;
        //            //cell_position = transform.position;
        //            update_position();
        //            _normal_mat = _display_render.sharedMaterial;
        //        }
        //        _parent_placement = this.gameObject.GetComponentInParent<PlacementRuntime>();
        //        //cell_orient = transform.up;
        //    }


        //    public static float_3 from_Vector3(Vector3 v)
        //    {
        //        return new float_3(v.x, v.y, v.z);
        //    }

        //    public static Vector3 from_float_3(float_3 _f3)
        //    {
        //        return new Vector3(_f3._x, _f3._y, _f3._z);
        //    }

        //    public void Show_Display()
        //    {
        //        if(_display_render!=null)
        //        {
        //            if(_object_occupied!=null)
        //            {
        //                _display_render.material = _occupied_mat;
        //            }
        //            else
        //            {
        //                _display_render.material = _normal_mat;
        //            }
        //        }
        //    }

        //    public void update_position()
        //    {
        //        if (cell_orient == new Vector3(0, 1, 0))
        //        {
        //            cell_position = _collider.bounds.center - _collider.bounds.extents;
        //        }
        //        if (cell_orient == new Vector3(1, 0, 0))
        //        {
        //            cell_position = this.transform.position;
        //        }
        //        if (cell_orient == new Vector3(-1, 0, 0))
        //        {
        //            cell_position = this.transform.position;
        //        }
        //        if (cell_orient == new Vector3(0, 0, 1))
        //        {
        //            cell_position = this.transform.position;
        //        }
        //        if (cell_orient == new Vector3(0, 0, -1))
        //        {
        //            cell_position = this.transform.position;
        //        }
        //    }
        //    public void on_select()
        //    {
        //        StartCoroutine(show_select_co());
        //        if (Planting_System_Manager.Instance != null)
        //        {
        //            Planting_System_Manager.Instance.on_select_cell(this.gameObject);
        //        }

        //        if (Indoor_Room_Game_Manager._activc_instance != null)
        //        {
        //            Indoor_Room_Game_Manager._activc_instance.on_select_grid_cell(this);
        //        }
        //    }


        //    public IEnumerator show_select_co()
        //    {
        //        if (_display_render != null && _selected_mat != null)
        //        {
        //            _display_render.material = _selected_mat;
        //            yield return new WaitForSeconds(3f);
        //            _display_render.material = _normal_mat;
        //        }
        //    }

        //    public void OnEnable()
        //    {
        //        if (_normal_mat != null) _display_render.material = _normal_mat;
        //        else _normal_mat = _display_render.sharedMaterial;
        //        //update_position();
        //    }
    }

}