//using System;
//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.Scene_View_Control;
//using UnityEngine;
//using GMC = CLIP.Project_Mouse.Game_Play_System.Game_Grid_Cell;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

//public class PlantingTemporaryData : MonoBehaviour
//{
//    [Header("临时数据")]
//    public List<Pot_In_Scene> _temp_pot_list = new List<Pot_In_Scene>();
//    public List<Plant_In_Scene> _temp_plant_list = new List<Plant_In_Scene>();
//    public List<(Pot_In_Scene pot, Fertilizer_Info fertilizer)> _temp_fertilizer_ops = new List<(Pot_In_Scene, Fertilizer_Info)>();
//    private Dictionary<Pot_In_Scene, Pot_In_Scene> _temp_to_formal_map = new Dictionary<Pot_In_Scene, Pot_In_Scene>();

//    // 放花盆（临时）
//    public void AddTempPot(Pot_In_Scene pot, flower_pot_info potInfo, planting_level_info levelInfo)
//    {
//        pot._pot_info._pot_prototype_name = potInfo.pot_name;
//        pot._pot_info._pot_info = potInfo;
//        pot._pot_info._pot_id_in_level = $"{potInfo.pot_name}_#_{Guid.NewGuid()}";

//        _temp_pot_list.Add(pot);
//        pot.update_view();
//    }

//    // 种种子（临时）
//    public void AddTempPlant(Pot_In_Scene pot, Plant_Info plantInfo, bool isRare)
//    {
//        var temp_plant_info = new plant_in_level_info();
//        temp_plant_info._plant_prototype_name = plantInfo.plant_name;
//        temp_plant_info._plant_info = plantInfo;
//        temp_plant_info._pot_info = pot._pot_info;
//        temp_plant_info._level_pot_id = pot._pot_info._pot_id_in_level;
//        temp_plant_info.current_stage = 0;
//        temp_plant_info._planting_time = DateTime.Now;
//        temp_plant_info._last_tick_time = DateTime.Now;
//        temp_plant_info._remain_harvest_count = plantInfo.harvest_time;
//        temp_plant_info._speed_up_fertilizer_name = "NULL";
//        temp_plant_info._organic_fertilizer_name = "NULL";
//        temp_plant_info._will_become_rare = isRare;

//        pot._pot_info._plant_level_info = temp_plant_info;

//        var res_url = temp_plant_info._plant_info.res_url;
//        PM_RM.load_game_object(res_url, (_go) =>
//        {
//            var _go_in_level = Instantiate(_go, pot._plant_root.position, UnityEngine.Quaternion.identity);
//            var _plant_in_scene = _go_in_level.GetComponent<Plant_In_Scene>();
//            if (_plant_in_scene == null) return;
//            _go_in_level.transform.SetParent(pot._plant_root, true);
//            _plant_in_scene.init_plant(pot);
//            _temp_plant_list.Add(_plant_in_scene);
//            pot.on_exit_planting();
//        });
//    }

//    // 施肥（临时）
//    public void AddTempFertilizer(Pot_In_Scene pot, Fertilizer_Info fertilizer)
//    {
//        _temp_fertilizer_ops.Add((pot, fertilizer));
//    }

//    // 确认修改
//    public void ConfirmModification(Planting_System_Manager manager)
//    {
//        foreach (var pot in _temp_pot_list)
//        {
//            if (_temp_to_formal_map.ContainsKey(pot))
//            {
//                var formalPot = _temp_to_formal_map[pot];
//                // 用临时副本覆盖正式花盆数据
//                formalPot.transform.position = pot.transform.position;
//                formalPot._pot_info._rotation = pot._pot_info._rotation;

//                formalPot.gameObject.SetActive(true);
//                GameObject.Destroy(pot.gameObject);
//            }
//            else
//            {
//                if (pot._pot_info == null || pot._pot_info._pot_info == null || string.IsNullOrEmpty(pot._pot_info._pot_prototype_name))
//                {
//                    Debug.LogError("临时花盆信息不完整，无法确认存储");
//                    continue;
//                }

//                string out_str = "";
//                //manager._level_Info.add_pot_to_level(
//                //    pot._pot_info._pot_prototype_name,
//                //    //GMC.from_Vector3(pot.transform.position),
//                //    //GMC.from_Vector3(pot._collider.gameObject.transform.position - pot.gameObject.transform.position),
//                //    pot._pot_info._pot_info,
//                //    out pot._pot_info,
//                //    out out_str
//                //);
//                manager._pot_list.Add(pot);
//            }
//        }
//        _temp_pot_list.Clear();
//        _temp_to_formal_map.Clear();

//        foreach (var plant in _temp_plant_list)
//        {
//            var pot = plant._pot_in_level;
//            var plant_info = plant._plant_info;
//            if (pot == null || plant_info == null) continue;

//            pot._pot_info._plant_level_info = plant_info;

//            string output_result = "";
//            manager._level_Info.add_plant_to_level_pot(
//                plant_info._plant_info.plant_name,
//                pot._pot_info._pot_id_in_level,
//                out output_result
//            );
//            if (output_result == "OK")
//            {
//                manager.add_to_plant_list(plant);
//            }
//        }
//        _temp_plant_list.Clear();

//        foreach (var op in _temp_fertilizer_ops)
//        {
//            var pot = op.pot;
//            var fertilizer = op.fertilizer;
//            if (pot == null || fertilizer == null) continue;

//            string output_result = "";
//            manager._level_Info.add_fertilizer_to_plant_in_level_pot(
//                pot._pot_info._pot_id_in_level,
//                fertilizer.fertilizer_name,
//                out output_result
//            );
//        }
//        _temp_fertilizer_ops.Clear();

//        foreach (var pot in manager._pot_list)
//        {
//            if (pot._plant != null && pot._pot_info != null)
//            {
//                pot._plant._plant_info = pot._pot_info._plant_level_info;
//                pot._plant.update_view();
//            }
//        }

//        Planting_System_Manager._instance.current_selected_pot = null;
//    }

//    // 取消修改
//    public void CancelAllModification()
//    {
//        foreach (var pot in _temp_pot_list)
//        {
//            if (_temp_to_formal_map.ContainsKey(pot))
//            {
//                var formalPot = _temp_to_formal_map[pot];
//                formalPot.gameObject.SetActive(true);
//            }
//            if (pot != null)
//            {
//                Destroy(pot.gameObject);
//            }
//        }
//        _temp_pot_list.Clear();
//        _temp_to_formal_map.Clear();

//        foreach (var plant in _temp_plant_list)
//        {
//            if (plant != null)
//            {
//                // 检查对应花盆是否是正式花盆
//                var pot = plant._pot_in_level;
//                if (pot != null && !_temp_pot_list.Contains(pot))
//                {
//                    // 清空正式花盆的 plant_level_info
//                    pot._pot_info._plant_level_info = null;
//                }

//                Destroy(plant.gameObject);
//            }
//        }
//        _temp_plant_list.Clear();

//        _temp_fertilizer_ops.Clear();
//    }

//    // 旋转花盆（临时）
//    public void RotateTempPot(Pot_In_Scene pot, int rotate_offset)
//    {
//        StartCoroutine(RotateTempPotCo(pot, rotate_offset));
//    }

//    public IEnumerator RotateTempPotCo(Pot_In_Scene pot, int rotate_offset)
//    {
//        if (pot == null || !_temp_pot_list.Contains(pot)) yield break;

//        int pre_rot = pot._pot_info._rotation;
//        int rot = (pot._pot_info._rotation + 4 + rotate_offset) % 4;

//        // 激活对应旋转的子物体
//        for (int i = 0; i < pot._root_from_sub_rotation.Count; i++)
//        {
//            pot._root_from_sub_rotation[i].gameObject.SetActive(i == rot);
//        }

//        // 隐藏渲染和植物
//        pot._root_from_sub_rotation[rot]._display_render.enabled = false;
//        if (pot._plant != null) pot._plant.hide_plant();

//        // 显示格子
//        Planting_System_Manager mgr = Planting_System_Manager._instance;
//        mgr.show_grid(true);
//        yield return new WaitForSecondsRealtime(0.125f);

//        // 检查碰撞
//        var _collider = pot._root_from_sub_rotation[rot]._collider;
//        var _pot_info = pot._pot_info._pot_info;
//        int area = _pot_info.long_grid_size * _pot_info.short_grid_size;

//        mgr.check_pot_overlapped(_collider, out int actual_hit_count, out bool is_hit_other_pot);

//        // 隐藏格子
//        mgr.hide_grid();
//        CustomEvent.Trigger(mgr.planting_interaction_manager, "_CE_exit_moving_pot");

//        // 恢复渲染和植物
//        pot._root_from_sub_rotation[rot]._display_render.enabled = true;
//        if (pot._plant != null) pot._plant.show_plant();

//        // 判定结果
//        if (is_hit_other_pot)
//        {
//            mgr.output_to_ui("和其他花盆重叠，无法旋转花盆");
//            pot._pot_info._rotation = pre_rot;
//            pot.after_rotate_pot();
//            yield break;
//        }
//        if (actual_hit_count != area)
//        {
//            mgr.output_to_ui("空间问题，无法旋转花盆");
//            pot._pot_info._rotation = pre_rot;
//            pot.after_rotate_pot();
//            yield break;
//        }

//        // 旋转成功
//        pot._pot_info._rotation = rot;
//        pot.after_rotate_pot();
//    }

//    // 移动花盆（临时）
//    public void MoveTempPot(Pot_In_Scene pot, Vector3 targetPosition)
//    {
//        StartCoroutine(MoveTempPotCo(pot, targetPosition));
//    }

//    public IEnumerator MoveTempPotCo(Pot_In_Scene pot, Vector3 targetPosition)
//    {
//        if (pot == null || !_temp_pot_list.Contains(pot)) yield break;

//        var mgr = Planting_System_Manager._instance;
//        Vector3 prevPosition = pot.transform.position;
//        pot.transform.position = targetPosition;
//        var _pot_info = pot._pot_info._pot_info;

//        // 隐藏渲染和植物
//        pot._display_render.enabled = false;
//        if (pot._plant != null)
//        {
//            pot._plant.hide_plant();
//        }
//        yield return new WaitForSecondsRealtime(0.125f);

//        var _collider = pot._collider;
//        int area = _pot_info.long_grid_size * _pot_info.short_grid_size;
//        mgr.check_pot_overlapped(
//            _collider,
//            out int actual_hit_count,
//            out bool is_hit_other_pot);

//        Action resetPot = () => {
//            pot.transform.position = prevPosition;
//            pot._display_render.enabled = true;
//            if (pot._plant != null)
//            {
//                pot._plant.show_plant();
//            }
//        };

//        if (is_hit_other_pot == true)
//        {
//            mgr.output_to_ui("和其他花盆重叠，无法移动花盆");
//            resetPot();
//            yield break;
//        }
//        if (actual_hit_count != area)
//        {
//            mgr.output_to_ui("空间问题，无法移动花盆");
//            resetPot();
//            yield break;
//        }

//        pot._display_render.enabled = true;
//        if (pot._plant != null)
//        {
//            pot._plant.show_plant();
//        }
//    }

//    // 移除临时花盆
//    public void RemoveTempPot(Pot_In_Scene pot)
//    {
//        if (pot == null) return;
//        if (_temp_pot_list.Contains(pot))
//        {
//            _temp_pot_list.Remove(pot);
//            Destroy(pot.gameObject);
//        }
//    }

//    // 复制花盆到临时
//    public Pot_In_Scene CopyPotToTemp(Pot_In_Scene srcPot)
//    {
//        GameObject tempGo = GameObject.Instantiate(srcPot.gameObject, srcPot.transform.position, srcPot.transform.rotation);
//        Pot_In_Scene tempPot = tempGo.GetComponent<Pot_In_Scene>();
//        tempPot.gameObject.name = srcPot.gameObject.name + "_temp";
//        srcPot.gameObject.SetActive(false);
//        _temp_pot_list.Add(tempPot);
//        _temp_to_formal_map[tempPot] = srcPot; // 记录映射
//        return tempPot;
//    }

//    // 通过正式花盆获取对应临时花盆
//    public Pot_In_Scene GetTempPotByFormal(Pot_In_Scene formalPot)
//    {
//        foreach (var kv in _temp_to_formal_map)
//        {
//            if (kv.Value == formalPot)
//                return kv.Key;
//        }
//        return null;
//    }

//    // 是否有未保存
//    public bool IsSave()
//    {
//        return _temp_pot_list.Count == 0 && _temp_plant_list.Count == 0 && _temp_fertilizer_ops.Count == 0;
//    }
//}
