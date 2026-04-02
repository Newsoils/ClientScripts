//using System;
//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Client_Event_Systems;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using CLIP.Project_Mouse.Kernel;
//using CLIP.Project_Mouse.Scene_View_Control;
//using Newtonsoft.Json;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.Events;

////using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
//using GMC = CLIP.Project_Mouse.Game_Play_System.Game_Grid_Cell;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
//namespace CLIP.Project_Mouse.Game_Play_System.Planting_System
//{
//    public class Planting_System_Manager : MonoBehaviour
//    {
//        public static Planting_System_Manager _instance = null;
//        public List<Pot_In_Scene> _pot_list = new List<Pot_In_Scene>();
//        public List<Plant_In_Scene> _plant_list = new List<Plant_In_Scene>();

//        public List<Plant_In_Scene> _rare_plant_list = new List<Plant_In_Scene>();

//        public planting_level_info _level_Info = new planting_level_info();

//        public planting_configuration_SO _plant_Config_SO;

//        public Transform _pot_root;
//        public LayerMask _check_mask;
//        [Space(15)]
//        [Header("Interaction_Object_In_Level")]
//        [Space(15)]
//        public GameObject current_selected_grid_cell;
//        public GameObject current_selected_pot;
//        public GameObject planting_interaction_manager;
//        public GameObject _output_ui_panel;
//        public flower_pot_info _current_selected_pot_info;
//        public Plant_Info _current_selected_plant_info;
//        public Fertilizer_Info _current_selected_fertilizer_info;

//        public string interaction_state = "NULL";

//        public List<string> _can_select_pot_state = new List<string>();
//        public bool canPotRotate = true;

//        public GameObject _ground_grid;
//        public GameObject _hang_grid;

//        public GameObject potMenu;
//        [Header("Interaction_State")]
//        public bool can_edit_pot = false;
//        public Vector3 _temp_pot_position_before_movement;
//        public bool _can_exit_movement_pot_state = true;
//        [Header("临时数据")]
//        public bool is_temp = false;
//        //public PlantingTemporaryData _temporary_data;

//        [Header("Event_Systems")]
//        public General_Interaction_Event_Hub_SO _planting_event_hub;
//        public Warehouse_UI_Event_Hub_SO _warehouse_event_hub;

//        public UnityEvent _update_data_from_server = new UnityEvent();
//        public UnityEvent _upload_data_to_server = new UnityEvent();
//        [Header("For_Saving")]
//        public bool _is_load_from_json = false;
//        public TextAsset _json_data;
//        private string _tempSaveJson = null;

//        void Start()
//        {
//            _instance = this;
//            _plant_Config_SO.RefrehData();

//            _level_Info._config = _plant_Config_SO._planting_config;
//            StartCoroutine(tick_co());

//            _binding_warehouse_event();
//            _bind_inventory_event();


//            if (_is_load_from_json == true)
//            {
//                if (_json_data != null)
//                {
//                    load_current_state_from_json(_json_data.text);
//                }
//            }
//        }


//        public void set_interaction_state(string input)
//        {
//            interaction_state = input;
//        }

//        public void _bind_inventory_event()
//        {
//            if (_level_Info != null )
//            {
//                _level_Info.change_item_count += change_item_count;
//                _level_Info.change_pot_count += change_pot_count;
    
//            }
//        }

//        public void change_item_count(string _item_name, int _item_count)
//        {
//            List<(string, int)> _list = new List<(string, int)>();
//            _list.Add((_item_name, _item_count));
//            Global_Inventory_Manager.Change_Items_Count(_list);
//            _warehouse_event_hub._invoke_on_refresh_warehouse_UI();
//        }
//        public void change_pot_count(string _item_name, int _item_change_count, int pot_remain_use_count)
//        {
//            List<(string, int, int)> _list = new List<(string, int, int)>();
//            _list.Add((_item_name, _item_change_count, pot_remain_use_count));
//            _warehouse_event_hub._invoke_on_refresh_warehouse_UI();
//        }
//        public void _binding_warehouse_event()
//        {
//            if (_warehouse_event_hub == null) return;
//            _warehouse_event_hub._on_close_warehouse_UI.AddListener(_enter_default);
//            _warehouse_event_hub._first_level_search_tag_change.AddListener(_first_level_search_tag_change);
//            _warehouse_event_hub._selected_item_change.AddListener(_selected_item_change);

//            //EvtDsp.AddEvt<string>(EvtNames.On_Select_Item_Change, _selected_item_change);
//            //EvtDsp.AddEvt<string>(EvtNames.First_Level_Search_Tag_Change, _first_level_search_tag_change);
//            //EvtDsp.AddEvt(EvtNames.On_Close_Warehouse_UI, _enter_default);
//        }

//        public void _first_level_search_tag_change(string tag)
//        {
//            _enter_default();
//        }

//        public void _selected_item_change(string _item_name)
//        {
//            StartCoroutine(_selected_item_change_co(_item_name));
//        }

//        public IEnumerator _selected_item_change_co(string _item_name)
//        {
//            _enter_default();

//            yield return new WaitForSecondsRealtime(0.125f);
//            bool _flag = false;
//            on_select_pot_info(_item_name, out _flag);
//            if (_flag == true)
//            {
//                CustomEvent.Trigger(planting_interaction_manager, "CE_enter_Placing_Pot");
//                on_enter_placing_pot();
//                yield break;
//            }
//            on_select_plant_info_by_seed(_item_name, out _flag);
//            if (_flag == true)
//            {
//                CustomEvent.Trigger(planting_interaction_manager, "CE_enter_Placing_Plant");
//                yield break;
//            }
//            on_select_fertilizer_info(_item_name, out _flag);
//            if (_flag == true)
//            {
//                CustomEvent.Trigger(planting_interaction_manager, "CE_toggle_Add_Fertilizer");
//                if (interaction_state == "During_Placing_Pot")
//                {
//                    on_exit_placing_pot();
//                }
//                yield break;
//            }
//        }
//        public void _enter_default()
//        {
//            CustomEvent.Trigger(planting_interaction_manager, "CE_enter_Default");
//        }
//        public IEnumerator tick_co()
//        {
//            while (true)
//            {
//                Debug.Log("Planting_System_Manager:tick_co()_#_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//                string out_str = "";
//                _level_Info._config = _plant_Config_SO._planting_config;
//                _level_Info.tick(DateTime.Now, out string output_result);
//                if (out_str != "OK")
//                {

//                }
//                _update_view();
//                yield return new WaitForSeconds(60f);
//            }
//        }
//        public void add_to_plant_list(Plant_In_Scene _plant)
//        {
//            _plant_list.Add(_plant);
//            //Indoor_Room_Game_Manager._activc_instance.book_keeping_visibility(_plant.gameObject);
//        }
//        public void add_to_pot_list(Pot_In_Scene _pot)
//        {
//            _pot_list.Add(_pot);
//            //Indoor_Room_Game_Manager._activc_instance.book_keeping_visibility(_pot.gameObject);
//        }
//        public void _update_view()
//        {
//            foreach (var _rare_plant in _rare_plant_list)
//            {
//                _plant_list.Add(_rare_plant);
//            }
//            _rare_plant_list.Clear();


//            foreach (var _pot in _pot_list)
//            {
//                _pot.update_view();
//            }
//            foreach (var _plant in _plant_list)
//            {
//                _plant.update_view();
//            }


//            var _plant_old = _plant_list.FindAll(_p => _p._sub_view_per_stage == null);
//            _plant_list.RemoveAll(_p => _p._sub_view_per_stage == null);
//            foreach (var _plant in _plant_old)
//            {
//                Destroy(_plant.gameObject);
//            }

//            foreach (var _rare_plant in _rare_plant_list)
//            {
//                _plant_list.Add(_rare_plant);
//            }
//            _rare_plant_list.Clear();

//        }
//        public void output_to_ui(string output)
//        {
//            //CustomEvent.Trigger(_output_ui_panel, "output_to_warning", output);
//            _planting_event_hub._invoke_on_show_up_prompt(output);
//        }
//        public void random_select_pot()
//        {
//            int seed = (int)DateTime.Now.ToBinary();
//            System.Random rand = new System.Random(seed);

//            var _list = _plant_Config_SO._planting_config.flower_pot_info_list;
//            _current_selected_pot_info = _list[rand.Next(_list.Count)];
//        }

//        public void try_irrigate_all_level_pot()
//        {
//            string output_result = "";
//            _level_Info.irrigate_all_level_pot(out output_result);
//            _update_view();
//        }

//        public void try_irrigate_all_level_pot(float volume)
//        {
//            string output_result = "";
//            _level_Info.irrigate_all_level_pot(out output_result, volume);
//            _update_view();
//        }
//        public void random_select_plant()
//        {
//            int seed = (int)DateTime.Now.ToBinary();
//            System.Random rand = new System.Random(seed);
//            var _list = _plant_Config_SO._planting_config.plant_info_list;
//            _current_selected_plant_info = _list[rand.Next(_list.Count)];
//        }
//        public void on_select_pot_info(string pot_name)
//        {
//            var _list = _plant_Config_SO._planting_config.flower_pot_info_list;
//            _current_selected_pot_info = _list.Find(_p => _p.pot_name == pot_name);
//        }

//        public void on_select_plant_info(string _plant_name)
//        {
//            var _list = _plant_Config_SO._planting_config.plant_info_list;

//            _current_selected_plant_info = _list.Find(_p => _p.plant_name == _plant_name);

//        }

//        public void on_select_fertilizer_info(string _fertilizer_name)
//        {
//            var _list = _plant_Config_SO._planting_config.fertilizer_info_list;

//            _current_selected_fertilizer_info = _list.Find(_p => _p.fertilizer_name == _fertilizer_name);

//        }

//        public void on_select_pot_info(string pot_name, out bool _flag)
//        {
//            var _list = _plant_Config_SO._planting_config.flower_pot_info_list;
//            _current_selected_pot_info = _list.Find(_p => _p.pot_name == pot_name);
//            _flag = _current_selected_pot_info != null;
//        }

//        public void on_select_plant_info_by_seed(string _plant_name, out bool _flag)
//        {
//            var _list = _plant_Config_SO._planting_config.plant_info_list;

//            _current_selected_plant_info = _plant_Config_SO._planting_config.find_plant_info_by_seed_name(_plant_name);

//            _flag = _current_selected_plant_info != null;

//        }

//        public void on_select_fertilizer_info(string _fertilizer_name, out bool _flag)
//        {
//            var _list = _plant_Config_SO._planting_config.fertilizer_info_list;

//            _current_selected_fertilizer_info = _list.Find(_p => _p.fertilizer_name == _fertilizer_name);

//            _flag = _current_selected_fertilizer_info != null;

//        }
//        public void random_select_fertilizer()
//        {
//            int seed = (int)DateTime.Now.ToBinary();
//            System.Random rand = new System.Random(seed);

//            var _list = _plant_Config_SO._planting_config.fertilizer_info_list;
//            _current_selected_fertilizer_info = _list[rand.Next(_list.Count)];

//        }

//        public void on_select_pot(GameObject pot_go)
//        {
//            if (pot_go == null) return;
//            if (current_selected_pot != null)
//            {
//                on_deselect_pot();
//            }
//            current_selected_pot = pot_go;
//            current_selected_pot.GetComponent<Pot_In_Scene>().after_select();
//        }

//        public void on_deselect_pot()
//        {
//            potMenu.SetActive(false);
//            if (current_selected_pot != null)
//            {
//                Pot_In_Scene pot_In_Scene = current_selected_pot.GetComponent<Pot_In_Scene>();
//                pot_In_Scene.change_mat_at_mats(0, pot_In_Scene._mat_normal);
//            }
//            on_exit_moving_pot();

//            current_selected_pot = null;
//            _planting_event_hub._invoke_on_enable_or_disable_double_tap_reset_camera(true);
//            _planting_event_hub._invoke_on_set_lean_multi_update(true);
//            _planting_event_hub._invoke_on_deselect_pot();
//        }

//        public void on_enter_placing_pot()
//        {
//            on_deselect_pot();
//        }

//        public void on_exit_placing_pot()
//        {
//            CustomEvent.Trigger(planting_interaction_manager, "exitPlacingPot");
//            CustomEvent.Trigger(planting_interaction_manager, "_CE_exit_Placing_Pot");
//            current_selected_pot = null;
//        }

//        public void on_enter_planting()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_enter_planting();
//            }
//        }
//        public void on_exit_planting()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_exit_planting();
//            }

//            _current_selected_plant_info = null;
//        }
//        public void on_enter_add_fertilizer()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_enter_add_fertilizer();
//            }
//        }

//        public void on_exit_add_fertilizer()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_exit_add_fertilizer();
//            }
//            _current_selected_fertilizer_info = null;
//        }
//        public void on_enter_remove_plant()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_enter_removing_plant();
//            }
//            CustomEvent.Trigger(planting_interaction_manager, "enterRemovePlant");
//        }

//        public void on_exit_remove_plant()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_exit_removing_plant();
//            }
//            // _current_selected_plant_info = null;
//            //CustomEvent.Trigger(planting_interaction_manager, "exitRemovePlant");
//        }


//        public void on_enter_remove_weed()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_enter_removing_weed();
//            }
//        }

//        public void on_exit_remove_weed()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_exit_removing_weed();
//            }
//        }

//        public void on_enter_harvest_plant()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_enter_harvest();
//            }
//        }

//        public void on_exit_harvest_plant()
//        {
//            foreach (var item in _pot_list)
//            {
//                item.on_exit_harvest();
//            }
//        }

//        public void on_select_cell(GameObject cell_go)
//        {
//            current_selected_grid_cell = cell_go;
//            interaction_state = (string)Variables.Object(planting_interaction_manager).Get("_Current_State");

//            if (interaction_state == "During_Placing_Pot")
//            {
//                if (_current_selected_pot_info == null) return;
//                string _result = "";
//                if (_planting_event_hub != null) _planting_event_hub._invoke_on_put_down_flower_pot();
//            }
//            ;

//            if (interaction_state == "During_Moving_Pot")
//            {
//                if (current_selected_pot == null) return;

//                if (current_selected_grid_cell == null) return;
//                try_moving_pot_to_select_cell();
//            }
//        }


//        public void on_enter_moving_pot()
//        {
//            CustomEvent.Trigger(planting_interaction_manager, "_CE_enter_moving_pot");
//            if (current_selected_pot != null) _temp_pot_position_before_movement = current_selected_pot.transform.position;
//        }

//        public void on_exit_moving_pot()
//        {
//            if (_can_exit_movement_pot_state == false) return;
//            CustomEvent.Trigger(planting_interaction_manager, "_CE_exit_moving_pot");
//            current_selected_pot = null;
//            interaction_state = "NULL";
//        }
//        public void try_moving_pot_to_select_cell()
//        {
//            //TODO 
//            //check overlap 
//            StartCoroutine(try_moving_pot_to_select_cell_co());
//        }
//        public IEnumerator try_moving_pot_to_select_cell_co()
//        {
//            var _pot = current_selected_pot.GetComponent<Pot_In_Scene>();
//            if (_pot == null) yield break;

//            UnityEngine.Vector3 prevPosition = current_selected_pot.transform.position;
//            UnityEngine.Vector3 targetPosition = current_selected_grid_cell.transform.position;

//            var _prev_position = current_selected_pot.transform.position;
//            current_selected_pot.transform.position = current_selected_grid_cell.transform.position;
//            var _pot_info = _pot._pot_info._pot_info;
//            //hided pot
//            _pot._display_render.enabled = false;
//            if (_pot._plant != null)
//            {
//                _pot._plant.hide_plant();
//            }
//            yield return new WaitForSecondsRealtime(0.125f);
//            var _collider = _pot._collider;
//            int area = _pot_info.long_grid_size * _pot_info.short_grid_size;
//            check_pot_overlapped(
//            _collider,
//          out int actual_hit_count,
//          out bool is_hit_other_pot);

//            Action _reset_pot = () =>
//            {
//                // current_selected_pot.transform.position = _prev_position;
//                current_selected_pot.transform.position = _temp_pot_position_before_movement;

//                _pot._display_render.enabled = true;
//                if (_pot._plant != null)
//                {
//                    _pot._plant.show_plant();
//                }

//            };

//            if (is_hit_other_pot == true)
//            {
//                //Destroy(_pot.gameObject);
//                output_to_ui("和其他花盆重叠，无法移动花盆");
//                _reset_pot();
//                yield break;
//            }
//            if (actual_hit_count != area)
//            {
//                //Destroy(_pot.gameObject);
//                output_to_ui("空间问题，无法移动花盆");
//                _reset_pot();
//                yield break;
//            }

//            _level_Info.move_pot_in_level(_pot._pot_info._pot_id_in_level,
//              new float_3(
//                  current_selected_pot.transform.position.x,
//                  current_selected_pot.transform.position.y,
//                  current_selected_pot.transform.position.z
//                  ),
//              out string output_result
//              );
//            if (output_result != "OK")
//            {
//                output_to_ui("其他问题，无法移动花盆");
//                _reset_pot();
//                yield break;
//            }

//            _pot._display_render.enabled = true;

//            if (_pot._plant != null)
//            {
//                _pot._plant.show_plant();
//            }
//        }

//        public void try_rotate_pot(Pot_In_Scene _pot, int rotate_offset)
//        {
//            StartCoroutine(try_rotate_pot_co(_pot, rotate_offset));
//        }

//        public IEnumerator try_rotate_pot_co(Pot_In_Scene _pot, int rotate_offset)
//        {
//            // 判断是否为临时花盆
//            //if (_temporary_data != null && _temporary_data._temp_pot_list.Contains(_pot))
//            //{
//            //    // 临时花盆直接调用临时旋转逻辑
//            //    _temporary_data.RotateTempPot(_pot, rotate_offset);
//            //    yield break;
//            //}

//            int pre_rot = _pot._pot_info._rotation;
//            int rot = (_pot._pot_info._rotation + 4 + rotate_offset) % 4;



//            for (int i = 0; i < _pot._root_from_sub_rotation.Count; i++)
//            {
//                if (i == rot)
//                {
//                    _pot._root_from_sub_rotation[i].gameObject.SetActive(true);
//                }
//                else
//                {
//                    _pot._root_from_sub_rotation[i].gameObject.SetActive(false);
//                }
//            }

//            Action _reset_pot = () =>
//            {
//                _pot._pot_info._rotation = pre_rot;


//                _pot.after_rotate_pot();
//                if (_pot._plant != null) _pot._plant.show_plant();
//                //  CustomEvent.Trigger(planting_interaction_manager, "_CE_exit_moving_pot");
//            };


//            //CustomEvent.Trigger(planting_interaction_manager, "_CE_enter_moving_pot");
//            _pot._root_from_sub_rotation[rot]._display_render.enabled = false;

//            if (_pot._plant != null) _pot._plant.hide_plant();
//            show_grid(true);
//            yield return new WaitForSecondsRealtime(0.125f);

//            var _collider = _pot._root_from_sub_rotation[rot]._collider;
//            var _pot_info = _pot._pot_info._pot_info;
//            int area = _pot_info.long_grid_size * _pot_info.short_grid_size;

//            check_pot_overlapped(
//            _collider,
//          out int actual_hit_count,
//          out bool is_hit_other_pot);

//            hide_grid();
//            CustomEvent.Trigger(planting_interaction_manager, "_CE_exit_moving_pot");

//            _level_Info.rotate_pot_in_level(_pot._pot_info._pot_id_in_level, rotate_offset, out string output_result);

//            if (output_result != "OK")
//            {
//                output_to_ui("其他问题，无法旋转花盆");

//                _reset_pot();
//                yield break;
//            }
//            if (is_hit_other_pot == true)
//            {
//                //Destroy(_pot.gameObject);
//                output_to_ui("和其他花盆重叠，无法旋转花盆");
//                _reset_pot();
//                yield break;
//            }
//            if (actual_hit_count != area)
//            {
//                //Destroy(_pot.gameObject);
//                output_to_ui("空间问题，无法旋转花盆");
//                _reset_pot();
//                yield break;
//            }

//            _pot.after_rotate_pot();
//            if (_pot._plant != null)
//            {
//                _pot._plant.show_plant();
//            }
//        }

//        public void try_harvest_at_level_pot(Pot_In_Scene _pot)
//        {
//            var _harvest_flag = _pot._pot_info._plant_level_info._plant_info.return_stage_info;
//            string out_str = "";
//            _level_Info.harvest_plant_from_level_pot(_pot._pot_info._pot_id_in_level, out out_str);
//            if (out_str != "OK")
//            {


//                return;
//            }


//            if (_harvest_flag == -1 || (
//                 _pot._pot_info._plant_level_info == null || _pot._pot_info._plant_level_info.is_valid() == false
//                ))
//            {
//                var _plant = _pot._plant;
//                _plant_list.Remove(_pot._plant);
//                _pot._plant = null;
//                Destroy(_plant.gameObject);
//            }
//            else if (_harvest_flag == 0)

//            {

//                _update_view();
//            }
//            else
//            {

//                _update_view();
//            }

//            if (_pot._pot_info.remain_use_count < 0 && _pot._plant == null)
//            {
//                _pot_list.Remove(_pot);
//                Destroy(_pot.gameObject);
//            }
//            else
//            {
//                _pot.on_exit_harvest();
//            }

//        }


//        public void check_pot_overlapped(
//            Collider _collider,
//            out int actual_hit_count,
//            out bool is_hit_other_pot)
//        {
//            Collider[] _hits = new Collider[16];
//            int _hit_count = Physics.OverlapBoxNonAlloc(
//             _collider.bounds.center,
//             _collider.bounds.extents,
//                _hits,
//                  //_collider.gameObject.transform.rotation,
//                  UnityEngine.Quaternion.identity,
//                _check_mask,
//                QueryTriggerInteraction.Collide
//                );

//            actual_hit_count = 0;
//            is_hit_other_pot = false;
//            for (int i = 0; i < _hit_count; i++)
//            {
//                var hit = _hits[i];
//                if (hit.gameObject != _collider.gameObject)
//                {
//                    if (hit.gameObject.tag == "Pot")
//                    {
//                        is_hit_other_pot = true;
//                        break;
//                    }
//                    if (hit.gameObject.tag == "Cell")
//                    {
//                        actual_hit_count++;
//                    }
//                }
//            }
//        }
//        public void try_add_pot_to_level(string pot_name, out string output_result)
//        {
//            //_planting_event_hub._invoke_on_put_down_flower_pot();
//            var _list = _plant_Config_SO._planting_config.flower_pot_info_list;
//            var pot = _list.Find(_p => _p.pot_name == pot_name);
//            if (pot == null)
//            {
//                output_result = "Not_Pot";
//                return;
//            }
//            PM_RM.load_game_object(pot.res_url, (_go) =>
//            {

//                StartCoroutine(try_add_pot_to_level_co(_go));

//            });
//            output_result = "OK";
//        }

//        public IEnumerator try_add_pot_to_level_co(GameObject go)
//        {
//            var _go_in_level = Instantiate(go, current_selected_grid_cell.transform.position, UnityEngine.Quaternion.identity);
//            yield return new WaitForSeconds(0.125f);

//            var _pot = _go_in_level.GetComponent<Pot_In_Scene>();
//            if (_pot == null)
//            {
//                Destroy(_pot.gameObject);
//                yield break;
//            }
//            var _collider = _pot._collider;

//            int area = _current_selected_pot_info.long_grid_size * _current_selected_pot_info.short_grid_size;
//            check_pot_overlapped(_collider,out int actual_hit_count,out bool is_hit_other_pot);


//            if (is_hit_other_pot == true)
//            {
//                Destroy(_pot.gameObject);
//                output_to_ui("和其他花盆重叠，无法放置花盆");
//                yield break;
//            }
//            if (actual_hit_count != area)
//            {
//                Destroy(_pot.gameObject);
//                output_to_ui("空间问题，无法放置花盆");
//                yield break;
//            }
//            //assemble pot info in level
//            if (_pot_root != null)
//            {
//                _pot.gameObject.transform.SetParent(_pot_root, true);
//            }

//            _pot._pot_info = new flower_pot_in_level_info();
//            string out_str = "";

//            //_level_Info.add_pot_to_level(
//            //_current_selected_pot_info.pot_name,
//            //GMC.from_Vector3(current_selected_grid_cell.transform.position),
//            ////GMC.from_Vector3(current_selected_grid_cell.transform.rotation.eulerAngles),
//            //GMC.from_Vector3(
//            //_pot._collider.gameObject.transform.position - _pot.gameObject.transform.position),
//            //_current_selected_pot_info,
//            //out _pot._pot_info,
//            //out out_str
//            //);

//            _pot_list.Add(_pot);

//            _pot.update_view();
//            //Indoor_Room_Game_Manager._activc_instance.book_keeping_visibility(_pot.gameObject);
//            //_pot_list.Add(_pot);

//            // saving_current_state_to_disk_as_json("planting.json");
//        }
//        public void try_add_plant_to_level_pot(Pot_In_Scene _pot,bool is_rare = false)
//        {
//            _level_Info._config = _plant_Config_SO._planting_config;
//            string plant_name = _current_selected_plant_info.plant_name;
//            string pot_id_in_level = _pot._pot_info._pot_id_in_level;

//            var pot_info = FindPotInfo(pot_id_in_level);
//            if (pot_info == null) return;
//            if (pot_info._plant_level_info != null && pot_info._plant_level_info.is_valid()) return;

//            var _pot_in_scene = _pot_list.Find((_p) =>
//            {
//                return _p._pot_info._pot_id_in_level == pot_id_in_level;
//            });
//            if (_pot_in_scene == null) return;
//            if (_pot_in_scene._plant != null) return;
//            if (_pot_in_scene._pot_info._plant_level_info != null
//                && _pot_in_scene._pot_info._plant_level_info.is_valid() == true) return;

//            string output_result = "";

//            _level_Info.add_plant_to_level_pot(
//            plant_name,
//            pot_id_in_level,
//            out output_result
//            );
//            if (output_result != "OK") return;

//            var level_pot = FindPotInfo(pot_id_in_level);
//            if (level_pot == null) return;
//            _pot._pot_info = level_pot;
//            _pot._pot_info._plant_level_info = level_pot._plant_level_info;

//            var res_url2 = _pot._pot_info._plant_level_info._plant_info.res_url;

//            PM_RM.load_game_object(res_url2, (_go) =>
//            {
//                var _go_in_level = Instantiate(_go, _pot._plant_root.position, UnityEngine.Quaternion.identity);
//                var _plant_in_scene = _go_in_level.GetComponent<Plant_In_Scene>();
//                if (_plant_in_scene == null) return;
//                _go_in_level.transform.SetParent(_pot._plant_root, true);
//                _plant_in_scene.init_plant(_pot);

//                _plant_list.Add(_plant_in_scene);
//                //Indoor_Room_Game_Manager._activc_instance.book_keeping_visibility(_plant_in_scene.gameObject);

//                _pot.on_exit_planting();
//            });

            
//        }

//        public flower_pot_in_level_info FindPotInfo(string pot_id_in_level)
//        {
//            // 正式数据
//            var potInfo = _level_Info._pots_in_level.Find(p => p._pot_id_in_level == pot_id_in_level);
//            return potInfo;
//        }


//        public void force_all_plant_to_next_stage()
//        {
//            _level_Info._config = _plant_Config_SO._planting_config;
//            foreach (var pot in _pot_list)
//            {
//                if (pot._pot_info._plant_level_info == null || pot._pot_info._plant_level_info.is_valid() == false) continue;

//                pot._pot_info._plant_level_info.remaining_minute_to_next_stage = 0;


//            }

//            string output_result = "";
//            _level_Info.tick(DateTime.Now, out output_result);

//            _update_view();

//        }


//        public void force_growing_weed()
//        {
//            var _now = DateTime.Now;
//            foreach (var pot in _pot_list)
//            {
//                if (pot._pot_info._plant_level_info != null && pot._pot_info._plant_level_info.is_valid() == true) continue;
//                pot._pot_info._last_harvest_time = _now - TimeSpan.FromHours(90);

//            }
//            string output_result = "";
//            _level_Info.tick(_now, out output_result);

//            _update_view();
//        }

//        public void try_remove_plant_from_level_pot(Pot_In_Scene pot_In_Scene)
//        {
//            if (pot_In_Scene._pot_info.remain_use_count == 2)
//            {
//                _planting_event_hub._invoke_on_show_up_prompt("这个花盆快坏了");
//            }

//            if (pot_In_Scene._plant == null) return;
//            var _plant = pot_In_Scene._plant;
//            string out_str = "";
//            _level_Info.remove_plant_from_level_pot(pot_In_Scene._pot_info._pot_id_in_level, out out_str);
//            if (out_str != "OK")
//            {

//                return;
//            }
//            _plant_list.Remove(_plant);
//            pot_In_Scene._plant = null;
//            Destroy(_plant.gameObject);

//            if (pot_In_Scene._pot_info.remain_use_count < 0)
//            {
//                _pot_list.Remove(pot_In_Scene);

//                Destroy(pot_In_Scene.gameObject);
//            }
//            pot_In_Scene.on_exit_removing_plant();
//        }

//        public void try_remove_weed_from_level_pot(Pot_In_Scene pot_In_Scene)
//        {
//            string out_str = "";
//            _level_Info.remove_weed_from_level_pot(pot_In_Scene._pot_info._pot_id_in_level, out out_str);
//            if (out_str != "OK")
//            {
//                return;
//            }

//            Destroy(pot_In_Scene._weed_go);
//            pot_In_Scene.on_exit_removing_weed();
//        }

//        public void try_add_fertilizer_to_plant_in_level_pot(Pot_In_Scene pot_In_Scene)
//        {
//            var pot_info = FindPotInfo(pot_In_Scene._pot_info._pot_id_in_level);
//            if (pot_info == null || pot_info._plant_level_info == null) return;

//            string output_result = "";

//            _level_Info.add_fertilizer_to_plant_in_level_pot(
//                pot_In_Scene._pot_info._pot_id_in_level,
//                _current_selected_fertilizer_info.fertilizer_name,
//                out output_result
//                );
//            if (output_result != "OK")
//            {
//                return;
//            }
//            if (_current_selected_fertilizer_info.is_to_mature == true)
//            {
//                _update_view();
//            }

//            pot_In_Scene.on_exit_add_fertilizer();
//        }

//        public void try_remove_pot_from_level(Pot_In_Scene pot_In_Scene)
//        {
//            // 删除临时花盆
//            //if (_temporary_data != null && _temporary_data._temp_pot_list.Contains(pot_In_Scene))
//            //{
//            //    _temporary_data.RemoveTempPot(pot_In_Scene);
//            //    return;
//            //}

//            string out_str = "";
//            _level_Info.remove_pot_from_level(
//                pot_In_Scene._pot_info._pot_id_in_level,
//                out out_str
//                );

//            _plant_list.Remove(pot_In_Scene._plant);
//            _pot_list.Remove(pot_In_Scene);
//            Destroy(pot_In_Scene.gameObject);
//        }

//        public void change_to_rare(
//              UnityEngine.Vector3 _position,
//              plant_in_level_info _plant_info,
//              Pot_In_Scene _pot_in_level
//              )


//        {
//            var res_url = _plant_info._plant_info.res_url;
//            GameObject _current_go = this.gameObject;
//            PM_RM.load_game_object(res_url, (_go) =>
//            {
//                var _go_in_level = Instantiate(_go, _position, UnityEngine.Quaternion.identity);
//                var _plant_in_scene = _go_in_level.GetComponent<Plant_In_Scene>();
//                if (_plant_in_scene == null) return;
//                _go_in_level.transform.SetParent(_pot_in_level._plant_root, true);
//                _plant_in_scene._plant_info = _plant_info;
//                _plant_in_scene._pot_in_level = _pot_in_level;
//                _plant_in_scene._pot_in_level._plant = _plant_in_scene;
//                _plant_in_scene._plant_info._will_become_rare = false;
//                _plant_in_scene.switch_model();
//                //Planting_System_Manager.Instance._plant_list.Remove(this);
//                Planting_System_Manager._instance._rare_plant_list.Add(_plant_in_scene);

//            });
//        }

//        public string _get_interaction_state()
//        {
//            var go = Planting_System_Manager._instance.planting_interaction_manager;
//            string _interaction_state = Variables.Object(go).Get("_Current_State") as string;
//            return _interaction_state;
//        }

//        public bool can_select_pot()
//        {
//            if (can_edit_pot == false) return false;
//            if (is_temp == true) return true;
//            var _state = _get_interaction_state();

//            return _can_select_pot_state.Contains(_state);

//        }

//        public void ConfirmModification()
//        {
//            //saving_current_state_to_disk_as_json("planting.json");
//            save_current_state_to_memory();

//            string state = interaction_state;

//            if (state == "During_Placing_Pot")
//            {
//                on_exit_placing_pot();
//            }
//            else if (state == "During_Placing_Plant")
//            {
//                on_exit_planting();
//            }
//            else if (state == "During_Add_Fertilizer")
//            {
//                on_exit_add_fertilizer();
//            }
//            //on_exit_planting();
//            //on_exit_add_fertilizer();
//            _planting_event_hub._invoke_on_close_pot_ghost();
//            //_planting_event_hub._invoke_on_change_none_state();
//            on_deselect_pot();

//            _planting_event_hub._invoke_on_swipe_close_planting_warehouse_canvas();

//            StartCoroutine(saveCo());
//        }

//        public IEnumerator saveCo()
//        {
//            upload_data_to_server();

//            yield return new WaitForSeconds(1f);

//            if (Global_Inventory_Manager._instance != null)
//            {
//                Global_Inventory_Manager._instance.Send_inventory_to_server();
//            }
//        }

//        public void CancelAllModifacation()
//        {
//            clear_all_planting_item();

//            load_current_state_from_json(_tempSaveJson);

//            string inventoryJson = Global_Inventory_Manager.Inventory_Serialization();

//            _level_Info._config = _plant_Config_SO._planting_config;

//            on_deselect_pot();
//        }

//        public bool isSave()
//        {
//            //return _temporary_data.IsSave();
//            string currentJson = JsonConvert.SerializeObject(_level_Info);

//            if (!string.IsNullOrEmpty(_tempSaveJson))
//            {
//                return currentJson == _tempSaveJson;
//            }

//            string path = System.IO.Path.Combine(Application.persistentDataPath, "planting.json");
//            if (!System.IO.File.Exists(path))
//            {
//                if (_level_Info._pots_in_level.Count == 0)
//                    return true;
//                else
//                    return false;
//            }

//            string savedJson = System.IO.File.ReadAllText(path);

//            return currentJson == savedJson;
//        }

//        public void show_grid(bool is_collider_only)
//        {

//            _ground_grid.SetActive(true);
//            _hang_grid.SetActive(true);
//            if (is_collider_only == true)
//            {
//                var _layer_mask = LayerMask.GetMask("Scene_Grid");
//                // Camera.main.cullingMask &= ~_layer_mask;
//            }
//        }
//        public void hide_grid()
//        {
//            _ground_grid.SetActive(false);
//            _hang_grid.SetActive(false);
//            var _layer_mask = LayerMask.GetMask("Scene_Grid");
//            //  Camera.main.cullingMask |= _layer_mask;
//        }


//        public async void saving_current_state_to_disk_as_json(string file_name)
//        {
//            string _json = JsonConvert.SerializeObject(_level_Info);

//            string path = System.IO.Path.Combine(Application.persistentDataPath, file_name);
//            await System.IO.File.WriteAllTextAsync(path, _json);
//            Debug.Log("Planting_System_Manager:saving_current_state_to_disk_as_json()_OK_path:" + path);
//        }

//        public void save_current_state_to_memory()
//        {
//            _tempSaveJson = JsonConvert.SerializeObject(_level_Info);
//        }

//        public void load_current_state_from_disk(string json_path)
//        {

//        }


//        public void clear_all_planting_item()
//        {
//            foreach (var item in _plant_list)
//            {
//                Destroy(item.gameObject);
//            }

//            foreach (var item in _pot_list)
//            {
//                Destroy(item.gameObject);
//            }
//            _plant_list.Clear();
//            _pot_list.Clear();
//            _level_Info._pots_in_level.Clear();
//            _level_Info._config = _plant_Config_SO._planting_config;
//            //Global_Home_Room_Manager.Instance._balcony.clear_visible_item();
//        }
//        public void load_current_state_from_json(string json)
//        {
//            var _temp = JsonConvert.DeserializeObject<planting_level_info>(json);
//            if (_temp == null) return;
//            _level_Info = _temp;
//            _level_Info._config = _plant_Config_SO._planting_config;
//            _bind_inventory_event();
//            List<plant_in_level_info> _plants = new List<plant_in_level_info>();
//            foreach (var _pot in _level_Info._pots_in_level)
//            {
//                load_pot_without_check(_pot);
//                if (_pot._plant_level_info != null && _pot._plant_level_info.is_valid())
//                {
//                    _plants.Add(_pot._plant_level_info);
//                }
//            }
//            //Global_Home_Room_Manager.Instance._balcony._refresh_view();
//            StartCoroutine(load_plant_without_check_co(_plants));
//        }

//        public IEnumerator load_plant_without_check_co(List<plant_in_level_info> _plants)
//        {
//            yield return new WaitForSeconds(0.5f);
//            foreach (var item in _plants)
//            {
//                load_plant_without_check(item);
//            }
//            //Global_Home_Room_Manager.Instance._balcony._refresh_view();
//        }


//        public void load_pot_without_check(flower_pot_in_level_info _pot_info)
//        {
//            //var _list = _plant_Config_SO._planting_config.flower_pot_info_list;
//            _pot_info._pot_info = _plant_Config_SO._planting_config.find_pot(_pot_info._pot_prototype_name);


//            if (_pot_info._pot_info == null)
//            {
//                Debug.Log("Planting_System_Manager:load_pot_without_check()_Not_Found_Pot_Info:" + _pot_info._pot_info.pot_name);
//                return;
//            }
//            var pot = _pot_info._pot_info;
//            PM_RM.load_game_object(pot.res_url, (_go) =>
//            {

//                //Vector3 _pos = GMC.from_float_3(_pot_info.position);
//                //var _go_in_level = Instantiate(_go, _pos, UnityEngine.Quaternion.identity);
//                //var _pot = _go_in_level.GetComponent<Pot_In_Scene>();
//                //_pot._pot_info = _pot_info;
//                //_pot.update_view();
//                //_pot.after_rotate_pot();
//                //_pot_list.Add(_pot);

//                //if (_pot_root != null)
//                //{
//                //    _pot.gameObject.transform.SetParent(_pot_root, true);
//                //}
//                //Global_Home_Room_Manager.Instance._balcony.book_keeping_visibility(_pot.gameObject);
//            });
//        }

//        public void load_plant_without_check(plant_in_level_info _plant_info)
//        {
//            string plant_name = _plant_info._plant_prototype_name;
//            var _pot = _level_Info._pots_in_level.Find(p => p._pot_id_in_level == _plant_info._level_pot_id);
//            var _pot_in_level = _pot_list.Find(p => p._pot_info._pot_id_in_level == _plant_info._level_pot_id);
//            if (_pot == null)
//            {
//                Debug.Log("load_plant_without_check()_#_pot_info_in_level==null");
//                return;
//            }
//            _plant_info._pot_info = _pot;
//            _plant_info._plant_info = _plant_Config_SO._planting_config.plant_info_list.Find(p => p.plant_name == plant_name);
//            var res_url2 = _pot._plant_level_info._plant_info.res_url;

//            PM_RM.load_game_object(res_url2, (_go) =>
//            {
//                Transform _plant_root = _pot_in_level._plant_root;
//                var _go_in_level = Instantiate(_go, _plant_root.transform.position, UnityEngine.Quaternion.identity);
//                var _plant_in_scene = _go_in_level.GetComponent<Plant_In_Scene>();
//                if (_plant_in_scene == null) return;
//                _go_in_level.transform.SetParent(_plant_root, true);
//                _go_in_level.transform.localEulerAngles = new Vector3(0, 0, 0);
//                _plant_in_scene.init_plant(_pot_in_level);

//                _plant_list.Add(_plant_in_scene);
//                //Global_Home_Room_Manager.Instance._balcony.book_keeping_visibility(_plant_in_scene.gameObject);

//                _pot_in_level.on_exit_planting();
//            });

//        }
//        public void update_data_from_server()
//        {
//            _update_data_from_server.Invoke();
//        }
//        public void upload_data_to_server()
//        {
//            _upload_data_to_server.Invoke();
//        }


//    }



//}