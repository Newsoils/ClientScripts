//using System;
//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.Kernel;
//using UnityEngine;
//using UnityEngine.UI;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_System;
//using CLIP.Project_Mouse.Scene_View_Control;
//using Unity.VisualScripting;
//using CLIP.Project_Mouse.Client_Event_Systems;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;



//#if UNITY_EDITOR
//using UnityEditor;
//#endif
//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace Scene_View_Control
//        {

//            [System.Serializable]
//            public class Pot_In_Scene : MonoBehaviour
//            {
//                public Kernel.flower_pot_in_level_info _pot_info;
//                public Collider _collider;
//                public Renderer _display_render;
//                public Transform _plant_root;

//                public Plant_In_Scene _plant = null;

//                public bool isHarvestLocked = false;
//                public GameObject _weed_go = null;
//                public List<Pot_Rotation_Root> _root_from_sub_rotation = new List<Pot_Rotation_Root>();
//                [Header("Materials")]
//                public Material _mat_normal;
//                public Material _mat_selected;
//                public Material _mat_wet_soil;
//                public Material _mat_dry_soil;
//                public Material _mat_selectable;

//                [Header("UI")]
//                public Canvas _ws_canvas;
//                public GameObject _planting_panel;
//                public GameObject _harvest_panel;
//                public GameObject _remove_plant_panel;
//                public GameObject _remove_weed_panel;
//                public GameObject _add_fertilizer_panel;
//                public Canvas _manipulation_canvas;
//                public Color _btn_color_on_moving;
//                public Color _btn_color_on_normal;
//                public Image _btn_image_moving;


//                void Start()
//                {
//                    if (_display_render != null)
//                    {
//                        _display_render.enabled = false;
//                        _mat_normal = _display_render.sharedMaterials[0];
//                        _mat_wet_soil = _display_render.sharedMaterials[1];
//                    }
//                    if (_ws_canvas != null)
//                    {
//                        _ws_canvas.worldCamera = Camera.main;


//                        _ws_canvas.gameObject.SetActive(false);

//                    }
//                    if (_manipulation_canvas != null)
//                    {
//                        _manipulation_canvas.worldCamera = Camera.main;
//                        _btn_image_moving.color = _btn_color_on_normal;
//                        _manipulation_canvas.gameObject.SetActive(false);
//                        Planting_System_Manager._instance.potMenu.SetActive(false);
//                    }

//                }

//                // Update is called once per frame
//                void Update()
//                {

//                }

//                public void update_view()
//                {
//                    _display_render.enabled = true;
//                    if (_pot_info == null) return;
//                    if (_pot_info.is_wet == true)
//                    {
//                        if (_mat_wet_soil != null) change_mat_at_mats(1, _mat_wet_soil);
//                    }
//                    else
//                    {
//                        if (_mat_dry_soil != null) change_mat_at_mats(1, _mat_dry_soil);
//                    }
//                    if (_pot_info.have_weed == true && _weed_go == null)
//                    {
//                        _spawn_weed();
//                    }

//                    //Global_Home_Room_Manager.Instance._balcony.book_keeping_visibility(this.gameObject);
//                }
//                public void _spawn_weed()
//                {
//                    if (_plant != null) return;
//                    if (Planting_System_Manager._instance == null) return;
//                    //if (Planting_System_Manager.Instance._current_selected_plant_info == null) return;
//                    var res_url = Planting_System_Manager._instance._plant_Config_SO._planting_config._planting_const.weed_res_path;

//                    PM_RM.load_game_object(res_url, (_go) =>
//                    {
//                        var _go_in_level = Instantiate(_go, _plant_root.transform.position, Quaternion.identity);

//                        _go_in_level.transform.SetParent(_plant_root, true);

//                        _weed_go = _go_in_level;
//                    });
//                }

//                public void _remove_weed()
//                {
//                    if (_weed_go == null) return;
//                    Debug.Log("_remove_weed_#_" + this.gameObject.name);
//                    if (Planting_System_Manager._instance == null) return;
//                    Planting_System_Manager._instance.try_remove_weed_from_level_pot(this);

//                    _weed_go = null;
//                }
//                public void on_select()
//                {
//                    Debug.Log("on_select()_#_" + this.gameObject.name);
//                    if (Planting_System_Manager._instance != null)
//                    {
//                        if (Planting_System_Manager._instance.planting_interaction_manager != null)
//                        {
//                            //bool can_select_pot = Planting_System_Manager.Instance.can_select_pot();
//                            if (!CanHarvest())
//                            {
//                                if (Planting_System_Manager._instance._planting_event_hub._invoke_on_check_harvest_state_and_prompt()) return;
//                            }
//                            //if (can_select_pot==false) return;

//                            Debug.Log("on_select()_#_" + this.gameObject.name);
//                            Planting_System_Manager._instance.on_select_pot(this.gameObject);

//                        }


//                    }
//                }
//                public void on_toggle_moving_pot()
//                {
//                    if (Planting_System_Manager._instance != null)
//                    {
//                        Debug.Log("on_toggle_moving_pot()_#_" + this.gameObject.name);

//                        Func<Color, Color, bool> _color_equal = (Color a, Color b) =>
//                        {
//                            if (a.r != b.r) return false;
//                            if (a.g != b.g) return false;
//                            if (a.b != b.b) return false;
//                            if (a.a != b.a) return false;
//                            return true;
//                        };
//                        if (_color_equal(_btn_image_moving.color, _btn_color_on_normal) == true)
//                        {
//                            Planting_System_Manager._instance.on_enter_moving_pot();
//                            _btn_image_moving.color = _btn_color_on_moving;
//                            Planting_System_Manager._instance.current_selected_pot = this.gameObject;
//                            Planting_System_Manager._instance._planting_event_hub._invoke_on_show_grid(this.gameObject);
//                        }
//                        else if (_color_equal(_btn_image_moving.color, _btn_color_on_moving) == true)
//                        {

//                            _btn_image_moving.color = _btn_color_on_normal;
//                            Planting_System_Manager._instance.on_exit_moving_pot();
//                        }

//                    }

//                }

//                public void try_rotate_pot(int rotation_offset)
//                {

//                    if (Planting_System_Manager._instance != null)
//                    {
//                        Debug.Log("try_rotate_pot()_#_" + this.gameObject.name + "_rotation_offset_=_" + rotation_offset);
//                        Planting_System_Manager._instance.try_rotate_pot(this, rotation_offset);
//                    }
//                }
//                public void after_rotate_pot()
//                {
//                    var _rot = _pot_info._rotation;
//                    _collider = _root_from_sub_rotation[_rot]._collider;
//                    _display_render = _root_from_sub_rotation[_rot]._display_render;
//                    _display_render.enabled = true;
//                    change_mat_at_mats(0, _mat_normal);
//                    _plant_root = _root_from_sub_rotation[_rot]._plant_root;
//                    if (_plant != null) _plant.transform.SetParent(_plant_root, false);
//                    for (int i = 0; i < _root_from_sub_rotation.Count; i++)
//                    {
//                        if (i == _rot)
//                        {
//                            _root_from_sub_rotation[i].gameObject.SetActive(true);
//                        }
//                        else
//                        {
//                            _root_from_sub_rotation[i].gameObject.SetActive(false);
//                        }
//                    }

//                }
//                public void on_exit_moving_pot()
//                {
//                    _btn_image_moving.color = _btn_color_on_normal;
//                    Planting_System_Manager._instance.on_exit_moving_pot();
//                }


//                public void after_select()
//                {
//                    if (Planting_System_Manager._instance == null) return;
//                    if (Planting_System_Manager._instance.current_selected_pot != this.gameObject)
//                    {
//                        _ws_canvas.gameObject.SetActive(false);
//                        _manipulation_canvas.gameObject.SetActive(false);
//                        //Planting_System_Manager.Instance.potMenu.SetActive(false);
//                        return;
//                    }

//                    if (Planting_System_Manager._instance.can_edit_pot)
//                    {
//                        _manipulation_canvas.gameObject.SetActive(true);
//                        Planting_System_Manager._instance.potMenu.SetActive(true);
//                        change_mat_at_mats(0, _mat_selected);
//                        Planting_System_Manager._instance.on_enter_moving_pot();
//                        //Planting_System_Manager.Instance.current_selected_pot = this.gameObject;
//                        Planting_System_Manager._instance._planting_event_hub._invoke_on_show_grid(this.gameObject);
//                        Planting_System_Manager._instance._planting_event_hub._invoke_on_set_select_pot(this.gameObject);
//                        Planting_System_Manager._instance._planting_event_hub._invoke_on_set_lean_multi_update(false);

//                        //if (_pot_info._pot_info.pot_type == ENUM.Pot_Type.Hang)
//                        //{
//                        //    Planting_System_Manager.Instance._planting_event_hub._invoke_on_set_lean_pitch(90);
//                        //}
//                        return;
//                    }

//                    if (Planting_System_Manager._instance.canPotRotate)
//                    {
//                        StartCoroutine(on_select_co());
//                    }
//                    if (_planting_panel.gameObject.activeSelf == true)
//                    {
//                        planting_on_this_pot();
//                        return;
//                    }

//                    if (_remove_plant_panel.gameObject.activeSelf == true)
//                    {
//                        remove_plant_on_this_pot();
//                        return;
//                    }

//                    if (_remove_weed_panel.gameObject.activeSelf == true)
//                    {
//                        _remove_weed();
//                        return;
//                    }

//                    if (_harvest_panel.gameObject.activeSelf == true)
//                    {
//                        _harvest_plant();
//                        return;

//                    }

//                    if (_add_fertilizer_panel.gameObject.activeSelf == true)
//                    {
//                        _add_fertilizer_to_level_pot();
//                        return;
//                    }
//                }

//                public void _harvest_plant()
//                {
//                    Debug.Log("_harvest_plant#_" + this.gameObject.name);
//                    if (Planting_System_Manager._instance == null) return;
//                    //if (!CanHarvest()) Planting_System_Manager.Instance._planting_event_hub._invoke_on_show_up_prompt("当前没有植物可以收获！");
//                    Planting_System_Manager._instance.try_harvest_at_level_pot(this);
//                }

//                public void _remove_pot()
//                {
//                    Debug.Log("_remove_pot#_" + this.gameObject.name);
//                    if (Planting_System_Manager._instance == null) return;
//                    Planting_System_Manager._instance.try_remove_pot_from_level(this);
//                }

//                public void remove_plant_on_this_pot()
//                {
//                    Debug.Log("remove_plant_on_this_pot_#_" + this.gameObject.name);
//                    if (Planting_System_Manager._instance == null) return;
//                    Planting_System_Manager._instance._planting_event_hub._invoke_on_show_prompt(2, () =>
//                    {
//                        Planting_System_Manager._instance.try_remove_plant_from_level_pot(this);
//                    });
//                    //Planting_System_Manager.Instance.try_remove_plant_from_level_pot(this);
//                }

//                public void _add_fertilizer_to_level_pot()
//                {
//                    Debug.Log("add_fertilizer_to_level_pot_#_" + this.gameObject.name);
//                    if (Planting_System_Manager._instance == null) return;
//                    Planting_System_Manager._instance.try_add_fertilizer_to_plant_in_level_pot(this);
//                }
//                public void on_enter_planting()
//                {
//                    Planting_System_Manager._instance.current_selected_pot = null;
//                    _collider.gameObject.SetActive(false);
//                    if (_plant != null) return;
//                    if (Planting_System_Manager._instance == null) return;
//                    if (Planting_System_Manager._instance._current_selected_plant_info == null) return;

//                    var _current_selected_plant_info = Planting_System_Manager._instance._current_selected_plant_info;
//                    if (_pot_info.have_weed == true) return;
//                    if (_pot_info._pot_info.plant_size != _current_selected_plant_info.Plant_size) return;
//                    //if (Planting_System_Manager.Instance._current_selected_plant_info.plant_Type == ENUM.Plant_Type.Soil)
//                    //{
//                    //    if (_pot_info._pot_info.pot_type == ENUM.Pot_Type.Soil)
//                    //    {
//                    //        _ws_canvas.gameObject.SetActive(true);
//                    //        _planting_panel.gameObject.SetActive(true);

//                    //        change_mat_at_mats(0, _mat_selectable);
//                    //    }
//                    //    else
//                    //    {

//                    //        _planting_panel.gameObject.SetActive(false);
//                    //        _ws_canvas.gameObject.SetActive(false);
//                    //        change_mat_at_mats(0, _mat_normal);
//                    //    }
//                    //}

//                    //if (Planting_System_Manager.Instance._current_selected_plant_info.plant_Type == ENUM.Plant_Type.Water)
//                    //{
//                    //    if (_pot_info._pot_info.pot_type == ENUM.Pot_Type.Water)
//                    //    {
//                    //        _ws_canvas.gameObject.SetActive(true);
//                    //        _planting_panel.gameObject.SetActive(true);

//                    //        change_mat_at_mats(0, _mat_selectable);
//                    //    }
//                    //    else
//                    //    {
//                    //        _planting_panel.gameObject.SetActive(false);
//                    //        _ws_canvas.gameObject.SetActive(false);
//                    //        change_mat_at_mats(0, _mat_normal);
//                    //    }
//                    //}
//                    //if (Planting_System_Manager.Instance._current_selected_plant_info.plant_Type == ENUM.Plant_Type.Vine)
//                    //{
//                    //    if (_pot_info._pot_info.pot_type == ENUM.Pot_Type.Hang)
//                    //    {
//                    //        _ws_canvas.gameObject.SetActive(true);
//                    //        _planting_panel.gameObject.SetActive(true);
//                    //        change_mat_at_mats(0, _mat_selectable);
//                    //    }
//                    //    else
//                    //    {
//                    //        _planting_panel.gameObject.SetActive(false);
//                    //        _ws_canvas.gameObject.SetActive(false);
//                    //        change_mat_at_mats(0, _mat_normal);
//                    //    }
//                    //}
//                }

//                public void on_exit_planting()
//                {

//                    _planting_panel.gameObject.SetActive(false);
//                    _ws_canvas.gameObject.SetActive(false);

//                    change_mat_at_mats(0, _mat_normal);
//                    _collider.gameObject.SetActive(true);
//                    Planting_System_Manager._instance.current_selected_pot = null;
//                }
//                public void on_enter_harvest()
//                {
//                    _collider.gameObject.SetActive(false);
//                    if (_plant == null) return;
//                    if (_plant._plant_info == null) return;
//                    if (_plant._plant_info.is_valid() == false) return;
//                    if (_plant._plant_info.current_stage != 4) return;
//                    if (_ws_canvas != null)
//                    {
//                        _ws_canvas.gameObject.SetActive(true);
//                        if (_harvest_panel != null) _harvest_panel.gameObject.SetActive(true);
//                    }

//                }


//                public void on_exit_harvest()
//                {
//                    if (_ws_canvas != null)
//                    {
//                        _ws_canvas.gameObject.SetActive(false);
//                        if (_harvest_panel != null) _harvest_panel.gameObject.SetActive(false);
//                    }
//                    _collider.gameObject.SetActive(true);
//                }

//                public void on_enter_removing_plant()
//                {
//                    _collider.gameObject.SetActive(false);
//                    if (_plant == null) return;
//                    if (_ws_canvas != null)
//                    {
//                        _ws_canvas.gameObject.SetActive(true);
//                        if (_remove_plant_panel != null) _remove_plant_panel.gameObject.SetActive(true);
//                    }
//                }
//                public void on_exit_removing_plant()
//                {
//                    if (_ws_canvas != null)
//                    {

//                        if (_remove_plant_panel != null) _remove_plant_panel.gameObject.SetActive(false);
//                        _ws_canvas.gameObject.SetActive(false);
//                    }
//                    _collider.gameObject.SetActive(true);
//                }

//                public void on_enter_removing_weed()

//                {
//                    _collider.gameObject.SetActive(false);
//                    if (_weed_go == null) return;
//                    if (_ws_canvas != null)
//                    {
//                        _ws_canvas.gameObject.SetActive(true);
//                        if (_remove_weed_panel != null) _remove_weed_panel.gameObject.SetActive(true);
//                    }
//                }

//                public void on_exit_removing_weed()
//                {
//                    if (_ws_canvas != null)
//                    {
//                        if (_remove_weed_panel != null) _remove_weed_panel.gameObject.SetActive(false);

//                        _ws_canvas.gameObject.SetActive(false);
//                    }
//                    _collider.gameObject.SetActive(true);
//                }
//                public void on_enter_add_fertilizer()
//                {
//                    Planting_System_Manager._instance.current_selected_pot = null;
//                    _collider.gameObject.SetActive(false);
//                    if (_plant == null) return;
//                    if (_plant._plant_info._speed_up_fertilizer_name != "NULL" &&
//                        _plant._plant_info._organic_fertilizer_name != "NULL") return;
//                    var _f = Planting_System_Manager._instance._current_selected_fertilizer_info;
//                    //if (_f.is_speed_up == true)
//                    //{
//                    //    if (_plant._plant_info._speed_up_fertilizer_name != "NULL") return;
//                    //}
//                    //if (_f.is_organic == true)
//                    //{
//                    //    if (_plant._plant_info._organic_fertilizer_name != "NULL") return;
//                    //}
//                    _collider.gameObject.SetActive(false);
//                    if (_ws_canvas != null)
//                    {
//                        _ws_canvas.gameObject.SetActive(true);
//                        if (_add_fertilizer_panel != null) _add_fertilizer_panel.gameObject.SetActive(true);


//                    }
//                }

//                public void on_exit_add_fertilizer()
//                {
//                    if (_ws_canvas != null)
//                    {
//                        if (_add_fertilizer_panel != null) _add_fertilizer_panel.gameObject.SetActive(false);

//                        _ws_canvas.gameObject.SetActive(false);
//                    }
//                    _collider.gameObject.SetActive(true);
//                    Planting_System_Manager._instance.current_selected_pot = null;
//                }


//                public IEnumerator on_select_co()
//                {
//                    if (_display_render != null && _mat_selected != null)
//                    {
//                        _display_render.material = _mat_selected;
//                    }
//                    yield return new WaitForSeconds(1f);
//                    if (_display_render != null && _mat_selected != null)
//                    {
//                        _display_render.material = _mat_normal;
//                    }

//                }

//                public void show_planting_panel()
//                {
//                    _planting_panel.gameObject.SetActive(true);
//                }

//                public void planting_on_this_pot()
//                {
//                    Debug.Log("planting_on_this_pot_#_" + this.gameObject.name);
//                    if (Planting_System_Manager._instance == null) return;
//                    Planting_System_Manager._instance.try_add_plant_to_level_pot(this);
//                }


//                public void change_mat_at_mats(int i, Material _mat)
//                {
//                    Material[] mats = _display_render.sharedMaterials;
//                    for (int j = 0; j < mats.Length; j++)
//                    {
//                        if (j == i) mats[j] = _mat;
//                        else
//                        {
//                            mats[j] = _display_render.sharedMaterials[j];
//                        }
//                    }
//                    _display_render.materials = mats;
//                }

//                public void sync_model()
//                {
//                    var _sub_rot_0 = _root_from_sub_rotation[0];
//                    var _mesh = _sub_rot_0._display_render.GetComponent<MeshFilter>().sharedMesh;
//                    for (int i = 1; i < 4; i++)
//                    {
//                        var _sub_rot = _root_from_sub_rotation[i];
//                        _sub_rot._display_render.gameObject.GetComponent<MeshFilter>().sharedMesh = _mesh;
//                        _sub_rot._display_render.gameObject.GetComponent<MeshCollider>().sharedMesh = _mesh;
//                        _sub_rot._display_render.gameObject.transform.localPosition = _sub_rot_0._display_render.gameObject.transform.localPosition;
//                        _sub_rot._display_render.gameObject.transform.localScale = _sub_rot_0._display_render.gameObject.transform.localScale;
//                        int mat_count = _sub_rot._display_render.sharedMaterials.Length;
//                        var _mats = _sub_rot._display_render.sharedMaterials;
//                        for (int m = 0; m < mat_count; m++)
//                        {
//                            _mats[m] = _sub_rot_0._display_render.sharedMaterials[m];
//                        }
//                        _sub_rot._display_render.sharedMaterials = _mats;


//                        _sub_rot._plant_root.gameObject.transform.localPosition = _sub_rot_0._plant_root.gameObject.transform.localPosition;
//                        _sub_rot._plant_root.gameObject.transform.localScale = _sub_rot_0._plant_root.gameObject.transform.localScale;


//                        _sub_rot._collider.gameObject.transform.localPosition = _sub_rot_0._collider.gameObject.transform.localPosition;
//                        _sub_rot._collider.size = _sub_rot_0._collider.size;
//                        _sub_rot._collider.center = _sub_rot_0._collider.center;

//                        _sub_rot._plant_root.localPosition = _sub_rot_0._plant_root.localPosition;
//                        _sub_rot._display_render.gameObject.transform.localRotation = Quaternion.Euler(0, 90, 0);
//                    }

//                }

//                public bool CanHarvest()
//                {
//                    if (_plant == null) return false;
//                    if (_plant._plant_info == null) return false;
//                    if (_plant._plant_info.is_valid() == false) return false;
//                    if (isHarvestLocked) return false;
//                    if (_plant._plant_info.current_stage == 4) return true;
//                    else return false;
//                }

//                public bool IsSelected()
//                {
//                    return Planting_System_Manager._instance != null &&
//                           Planting_System_Manager._instance.current_selected_pot == this.gameObject;
//                }
//            }



//        }

//    }
//}