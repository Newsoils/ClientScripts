//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using CLIP.Framework_Core.Event;
//using CLIP.Framework_Core.LYC.TaskSystem;
//using CLIP.Project_Mouse.Client_Event_Systems;
//using CLIP.Project_Mouse.ENUM;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_System;
//using CLIP.Project_Mouse.Kernel;
//using Newtonsoft.Json;
//using UnityEditor;
//using UnityEngine;
//using UnityEngine.Events;

//using GMC = CLIP.Project_Mouse.Game_Play_System.Game_Grid_Cell;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
//using Random = UnityEngine.Random;


//namespace CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System
//{
//    public class Indoor_Room_Game_Manager : MonoBehaviour
//    {
//        public static Indoor_Room_Game_Manager _activc_instance;
//        public bool _is_active = false;
//        public Indoor_Room_Info _level_info;

//        public Placement_SO placement_SO;

//        public readonly Dictionary<int, Room_Placement_Info> placementDic = new Dictionary<int, Room_Placement_Info>();
//        public readonly Dictionary<string, Room_Placement_Info> placementNameDic = new Dictionary<string, Room_Placement_Info>();

//        [Header("For_Scene_Management")]
//        public List<PlacementRuntime> _placement_list = new List<PlacementRuntime>();
//        public Transform _grid;
//        public Transform Fixed_Level_Item_Root;
//        public Transform Room_Placement_Root;
//        public List<Renderer> _floor_renderers = new List<Renderer>();
//        public List<Renderer> _wall_renderers = new List<Renderer>();

//        public List<Renderer> _door_renders = new List<Renderer>();
//        public List<Light> _all_light = new List<Light>();
//        public List<Renderer> _all_renderer = new List<Renderer>();
//        public List<ParticleSystem> _all_particle_system = new List<ParticleSystem>();

//        public List<Canvas> _all_canvas = new List<Canvas>();
//        public GameObject controlCanvas;
//        public Transform _camera_root;
//        public GameObject uiPhone;
//        public List<Renderer> _character_renderers;
//        public Collider _room_range;
//        public Camera _room_main_camera;

//        [Header("For_Interaction")]
//        public bool _can_select_placement = false;
//        public string _current_state = "Default";
//        public Game_Grid_Cell _current_selected_grid_cell = null;
//        public Room_Placement_Info _current_selected_placement_info = null;
//        public PlacementRuntime _current_selected_placement = null;
//        public LayerMask _check_mask;
//        //public GameObject _warehouse_canvas;
//        public Warehouse_UI_Event_Hub_SO _warehouse_UI_event_hub;
//        public General_Interaction_Event_Hub_SO _general_interaction_event_hub;

//        public GameObject indoorRoomInteractManager;
//        public GameObject roomPlacementMenu;
//        [Header("入口")]
//        public List<Transform> doors;

//        public UnityEvent _update_data_from_server = new UnityEvent();
//        public UnityEvent _upload_data_to_server = new UnityEvent();
//        [Header("For_Loading")]
//        public bool _is_loading_from_json = false;
//        public TextAsset _level_info_json;
//        private string _tempSaveJson = null;
//        private void Awake()
//        {
//            _room_range = this.GetComponent<Collider>();
//        }
//        //void Start()
//        //{
//            //_level_info._placement_db = placement_SO._placement_db;



//            //_general_interaction_event_hub._on_set_all_placement_collider.AddListener(Set_AllPlacement_Collider);

//            //if (_warehouse_UI_event_hub != null)
//            //{
//            //    _warehouse_UI_event_hub._selected_item_change.AddListener(_selected_item_change);
//            //    _warehouse_UI_event_hub._on_close_warehouse_UI.AddListener(_on_close_warehouse_UI);
//            //    _warehouse_UI_event_hub._second_level_search_tag_change.AddListener(_second_level_search_tag_change);

//            //}
//            //refresh_scene_content();
//            //if (_is_active == true)
//            //{
//            //    acitve_room();
//            //}
//            //else
//            //{
//            //    deactive_room();
//            //}


//            //if (_is_loading_from_json == true)
//            //{
//            //    if (_level_info_json.text != null)
//            //    {
//            //        load_current_state_from_json(_level_info_json.text);
//            //    }
//            //}
//            //else
//            //{
//                //_level_info._floor_info = placementNameDic["初始地板"];
//                //_level_info._wall_info = placementNameDic["初级墙纸"];

//            //    assemble_doors();
//            //}

//            //EvtDsp.AddEvt(EvtNames.On_Placement_Move_Over, _on_exit_moving_placement);
//            //EvtDsp.AddEvt(EvtNames.On_Placement_Move_Start,_on_enter_moving_placement);
//        //}
        

//        //public void OnDestroy()
//        //{

//            //EvtDsp.RemoveEvt<string>(EvtNames.On_Select_Item_Change, _selected_item_change);

//            //_warehouse_UI_event_hub._selected_item_change.RemoveListener(_selected_item_change);
//            //_warehouse_UI_event_hub._on_close_warehouse_UI.RemoveListener(_on_close_warehouse_UI);
//            //_warehouse_UI_event_hub._second_level_search_tag_change.RemoveListener(_second_level_search_tag_change);
//            //_general_interaction_event_hub._on_set_all_placement_collider.RemoveListener(Set_AllPlacement_Collider);

//            //EvtDsp.RemoveEvt(EvtNames.On_Placement_Move_Start, _on_enter_moving_placement);
//            //EvtDsp.RemoveEvt(EvtNames.On_Placement_Move_Over, _on_exit_moving_placement);
//        //}

//        public void FindPlacement()
//        {

//        }

//        public bool is_contain(Vector3 _pos)
//        {
//            if (_room_range == null) return false;
//            return _room_range.bounds.Contains(_pos);
//        }
//        //public Vector3 GetRandomPosInRoom()
//        //{
//        //    Vector3 randomPos = new Vector3(Random.Range(_room_range.bounds.min.x, _room_range.bounds.max.x), 1.3f, Random.Range(_room_range.bounds.min.z, _room_range.bounds.max.z));
//        //    return randomPos;
//        //}

//        //public void _second_level_search_tag_change(List<string> _tags)
//        //{
//        //    _on_exit_add_placement();
//        //}

//        //public void refresh_scene_content()
//        //{
//        //    _all_renderer.Clear();
//        //    _all_light.Clear();
//        //    _all_particle_system.Clear();


//        //    var _all_renderers_1 = Fixed_Level_Item_Root.GetComponentsInChildren<Renderer>(true);
//        //    for (int i = 0; i < _all_renderers_1.Length; i++) _all_renderer.Add(_all_renderers_1[i]);

//        //    var _all_light_1 = Fixed_Level_Item_Root.GetComponentsInChildren<Light>(true);
//        //    for (int i = 0; i < _all_light_1.Length; i++) _all_light.Add(_all_light_1[i]);
//        //    var _all_ps_1 = Fixed_Level_Item_Root.GetComponentsInChildren<ParticleSystem>(true);
//        //    for (int i = 0; i < _all_ps_1.Length; i++) _all_particle_system.Add(_all_ps_1[i]);



//        //    var _all_renderers_2 = Room_Placement_Root.GetComponentsInChildren<Renderer>(true);
//        //    for (int i = 0; i < _all_renderers_2.Length; i++) _all_renderer.Add(_all_renderers_2[i]);

//        //    var _all_light_2 = Room_Placement_Root.GetComponentsInChildren<Light>(true);
//        //    for (int i = 0; i < _all_light_2.Length; i++) _all_light.Add(_all_light_2[i]);
//        //    var _all_ps_2 = Room_Placement_Root.GetComponentsInChildren<ParticleSystem>(true);
//        //    for (int i = 0; i < _all_ps_2.Length; i++) _all_particle_system.Add(_all_ps_2[i]);


//        //}

//        //public void _selected_item_change(string _item_name)
//        //{
//        //    if (_activc_instance != this) return;
//        //    placementNameDic.TryGetValue(_item_name, out var _item);
//        //    if (_item == null) return;
//        //    if (_item.placing_type == Room_Placing_Type.Floor_Finishes)
//        //    {
//        //        change_floor(_item_name);
//        //    }

//        //    if (_item.placing_type == Room_Placing_Type.Wall_Finishes)
//        //    {
//        //        change_wall(_item_name);
//        //    }
//        //    if (_item.placing_type == Room_Placing_Type.Floor_Furniture ||
//        //        _item.placing_type == Room_Placing_Type.Wall_Furniture ||
//        //        _item.placing_type == Room_Placing_Type.Ceiling_Furniture)
//        //    {
//        //        if (_item.first_Category == Placement_First_Category.Door)
//        //        {
//        //            _current_selected_placement_info = _item;
//        //            _on_enter_change_door();
//        //        }
//        //        else
//        //        {
//        //            _current_selected_placement_info = _item;
//        //            _on_enter_add_placement();
//        //        }
//        //    }
//        //}

//        //public void on_select_door(string door_name)
//        //{
//        //    if (_activc_instance != this) return;
//        //    if (_current_state == "Change_Door")
//        //    {
//        //        try_change_door(door_name, _current_selected_placement_info.room_placement_name);
//        //    }
//        //}
//        //public void _on_enter_change_door()
//        //{
//        //    if (_activc_instance != this) return;
//        //    _current_state = "Change_Door";
//        //    foreach (var item in _door_renders)
//        //    {

//        //    }
//        //}

//        //public void _on_exit_change_door()
//        //{
//        //    if (_activc_instance != this) return;
//        //    _current_state = "NULL";
//        //}
//        //public void _on_close_warehouse_UI()
//        //{
//        //    _on_exit_add_placement();
//        //    _on_exit_moving_placement();
//        //    _on_exit_change_door();
//        //}
//        //public void change_wall(string _name, bool load_only = false)
//        //{
//        //    placementNameDic.TryGetValue(_name, out var _item);
//        //    if (_item == null) return;
//        //    //if (_item.relating_rooms.Contains(this._level_info._room_name) == false)
//        //    //{
//        //    //    output_to_ui("房间不匹配，无法放置");
//        //    //    return;
//        //    //}
//        //    if (load_only == false)
//        //    {
//        //        if (_level_info._wall_info.room_placement_name == _item.room_placement_name) return;
//        //        string _pre_name = _level_info._wall_info.room_placement_name;
//        //        string _now_name = _item.room_placement_name;
//        //        _level_info.change_wall(_name);

//        //        List<(string, int)> _change_list = new List<(string, int)>();
//        //        _change_list.Add((_pre_name, +1));
//        //        _change_list.Add((_now_name, -1));
//        //        Global_Inventory_Manager.Change_Items_Count(in _change_list);
//        //        _warehouse_UI_event_hub._invoke_on_refresh_warehouse_UI();
//        //    }

//        //    PM_RM.load_material_async(_item.res_url, (_mat) =>
//        //    {
//        //        foreach (var _renderer in _wall_renderers)
//        //        {
//        //            if (_renderer != null)
//        //            {
//        //                _renderer.material = _mat;
//        //            }
//        //        }
//        //    });
//        //}

//        //public void change_floor(string _name, bool load_only = false)
//        //{
//        //    placementNameDic.TryGetValue(_name, out var _item);
//        //    if (_item == null) return;

//        //    //if (_item.relating_rooms.Contains(this._level_info._room_name) == false)
//        //    {
//        //        output_to_ui("房间不匹配，无法放置");
//        //        return;
//        //    }

//        //    if (load_only == false)
//        //    {
//        //        if (_level_info._floor_info.room_placement_name == _item.room_placement_name) return;
//        //        string _pre_floor_name = _level_info._floor_info.room_placement_name;
//        //        string _now_floor_name = _item.room_placement_name;
//        //        _level_info.change_floor(_name);

//        //        List<(string, int)> _change_list = new List<(string, int)>();
//        //        _change_list.Add((_pre_floor_name, +1));
//        //        _change_list.Add((_now_floor_name, -1));
//        //        Global_Inventory_Manager.Change_Items_Count(in _change_list);
//        //        _warehouse_UI_event_hub._invoke_on_refresh_warehouse_UI();
//        //    }

//        //    PM_RM.load_material_async(_item.res_url, (_mat) =>
//        //    {
//        //        foreach (var _renderer in _floor_renderers)
//        //        {
//        //            if (_renderer != null)
//        //            {
//        //                _renderer.material = _mat;
//        //            }
//        //        }
//        //    });
//        //}



//        //public void add_placement(
//        //    string _name,
//        //    Vector3 target_position,
//        //    Vector3 orient,
//        //    string parent_id = "NULL")
//        //{
//        //    var _placement_Info = placementNameDic[_name];
//        //    //if (_placement_Info.relating_rooms.Contains(this._level_info._room_name) == false)
//        //    //{
//        //    //    output_to_ui("房间不匹配，无法放置");
//        //    //    return;
//        //    //}

//        //    // 通知任务系统
//        //    //TaskTriggers.TriggerEventOfMultipleOperations("首次摆放一个家具", 1);
//        //    TaskTriggers.TriggerEventOfMultipleOperations(1, 1);

//        //    //TODO check can place
//        //    PM_RM.load_game_object_async(_placement_Info.res_url, (go) =>
//        //    {
//        //        //AddPlacement(go, _placement_Info, target_position, orient, parent_id);
//        //        StartCoroutine(
//        //            Add_Placement_Co(go, _placement_Info, target_position, orient, parent_id)
//        //        );
//        //        //AddBorderMesh(go, _placement_Info);
//        //    });
//        //}

        

//        //public void AddPlacement(GameObject prefab, Room_Placement_Info item, Vector3 targetPos, Vector3 orient,
//        //                 string parentId = "NULL")
//        //{
//        //    GameObject go = Instantiate(prefab, ResolveInstantiatePosition(item.placing_type, targetPos), Quaternion.identity);
//        //    var placement = go.GetComponent<PlacementRuntime>();

//        //    if (!InitPlacement(placement))
//        //    {
//        //        Destroy(go);
//        //        return;
//        //    }

//        //    if (item.placing_type == ENUM.Room_Placing_Type.Wall_Furniture)
//        //    {
//        //        int rotIndex = room_placement_in_level_info.float_3_to_rotation_index(GMC.from_Vector3(orient));
//        //        placement._placement_in_level_info._rotation_index = rotIndex;
//        //        placement._switch_rotation();
//        //    }

//        //    // 🔥现在立刻与物理系统同步
//        //    Physics.SyncTransforms();

//        //    if (!CheckPlacementValid(item, placement, orient, targetPos))
//        //    {
//        //        Destroy(go);
//        //        return;
//        //    }

//        //    RegisterPlacementToLevel(item, placement, targetPos, orient, parentId);
//        //    HandlePlacementHierarchy(placement, parentId);
//        //    FinalizePlacement(item, placement, orient);

//        //    if (item.placing_type == ENUM.Room_Placing_Type.Floor_Furniture)
//        //        Global_Home_Room_Manager.Instance.re_bake_navmesh();
//        //}


//        /// <summary>
//        /// 通用放置协程（地面 / 墙面 / 天花板）
//        /// 负责：预览实例化 → 空间检测 → 数据写入 → 层级设置 → 可见性与库存更新
//        /// </summary>
//        //public IEnumerator Add_Placement_Co(GameObject prefab, Room_Placement_Info item,
//        //                           Vector3 targetPos, Vector3 orient,
//        //                           string parentId = "NULL")
//        //{
//        //    // 基于放置类型修正生成位置（天花板家具 Y = 0）
//        //    Vector3 instPos = ResolveInstantiatePosition(item.placing_type, targetPos);

//        //    // 生成对象，用于检测与放置
//        //    GameObject go = Instantiate(prefab, instPos, Quaternion.identity);
//        //    var placement = go.GetComponent<PlacementRuntime>();

//        //    // 组件检查（避免空引用）
//        //    if (!InitPlacement(placement))
//        //    {
//        //        Destroy(go);
//        //        yield break;
//        //    }

//        //    // 墙面家具旋转逻辑需提前执行（影响碰撞检测精度）
//        //    if (item.placing_type == ENUM.Room_Placing_Type.Wall_Furniture)
//        //    {
//        //        int rotIndex = room_placement_in_level_info.float_3_to_rotation_index(GMC.from_Vector3(orient));
//        //        placement._placement_in_level_info._rotation_index = rotIndex;
//        //        placement._switch_rotation();
//        //    }

//        //    // 临时启用所有放置物的 Collider 用于重叠检测
//        //    Set_AllPlacement_Collider(true);

//        //    // 等一帧，等待 Unity 物理系统刷新碰撞体
//        //    yield return new WaitForSecondsRealtime(0.125f);

//        //    // 位置有效性检测（是否重叠、空间是否充足）
//        //    if (!CheckPlacementValid(item, placement, orient, targetPos))
//        //    {
//        //        Destroy(go);
//        //        yield break;
//        //    }

//        //    // 写入关卡数据（ID 分配 / 坐标 / 朝向）
//        //    RegisterPlacementToLevel(item, placement, targetPos, orient, parentId);

//        //    // 设置层级关系（根节点或作为子物体）
//        //    HandlePlacementHierarchy(placement, parentId);

//        //    // 最终渲染、隐藏墙面、库存结算等处理
//        //    FinalizePlacement(item, placement, orient);

//        //    // 仅地面家具更新导航网格（影响寻路）
//        //    if (item.placing_type == ENUM.Room_Placing_Type.Floor_Furniture)
//        //        Global_Home_Room_Manager.Instance.re_bake_navmesh();

//        //}
//        /// <summary>
//        /// 根据放置类型修正实例化位置
//        /// 说明：天花板家具统一贴顶处理（避免 Y 偏差）
//        /// </summary>
//        //private Vector3 ResolveInstantiatePosition(ENUM.Room_Placing_Type type, Vector3 pos)
//        //{
//        //    return type == ENUM.Room_Placing_Type.Ceiling_Furniture ?
//        //           new Vector3(pos.x, 0, pos.z) : pos;
//        //}

//        /// <summary>
//        /// 初始化放置物状态：
//        /// - 检查组件是否存在
//        /// - 隐藏渲染器用于预览
//        /// </summary>
//        //private bool InitPlacement(PlacementRuntime placement)
//        //{
//        //    if (placement == null)
//        //        return false;

//        //    placement.set_render_state(false);
//        //    return true;
//        //}

      

//        /// <summary>
//        /// 空间有效性判断：
//        /// 1. 是否与其他家具重叠
//        /// 2. 网格支持区域是否满足需求
//        /// </summary>
//        //private bool CheckPlacementValid(Room_Placement_Info item,PlacementRuntime placement, 
//        //                               Vector3 orient,  Vector3 targetPos)
//        //{
//        //    var collider = placement._grid_collider;
//        //    int area = item.length * item.width;

//        //    check_placement_overlapped(collider, orient,out int hitCount,out bool isOverlap,item.first_Category);
                                       
//        //    if (isOverlap)
//        //    {
//        //        output_to_ui("和其他放置物重叠，无法放置");
//        //        return false;
//        //    }

//        //    if (hitCount != area)
//        //    {
//        //        output_to_ui("空间问题，无法放置");
//        //        Debug.LogWarning($"[AddPlacement] 空间不足: 需求={area}, 可用={hitCount}, name={item.room_placement_name}, pos={targetPos}");
//        //        return false;
//        //    }

//        //    return true;
//        //}

//        ///// <summary>
//        ///// 在关卡数据结构中注册放置信息
//        ///// </summary>
//        //private void RegisterPlacementToLevel(Room_Placement_Info item,PlacementRuntime placement,
//        //                                      Vector3 targetPos, Vector3 orient,string parentId)
//        //{
//        //    _level_info.add_placement(
//        //        item.room_placement_name,
//        //        GMC.from_Vector3(targetPos),
//        //        room_placement_in_level_info.float_3_to_rotation_index(GMC.from_Vector3(orient)),
//        //        out placement._placement_in_level_info,
//        //        parentId
//        //    );
//        //}

//        /// <summary>
//        /// 设置层级关系（根节点或依附父放置物）
//        /// 同步子放置数据列表与旋转纠正
//        /// </summary>
//        //private void HandlePlacementHierarchy(PlacementRuntime placement, string parentId)
//        //{
//        //    if (parentId == "NULL")
//        //    {
//        //        placement.transform.SetParent(Room_Placement_Root, true);
//        //        return;
//        //    }

//        //    var parent = _placement_list.Find(p => p._placement_in_level_info._placement_id_in_level == parentId);
//        //    if (parent != null)
//        //    {
//        //        placement.transform.SetParent(parent.sub_placement_root, true);
//        //        parent._sub_placement_list.Add(placement);
//        //        handle_attach_rotation(placement, parent);
//        //    }
//        //}

//        /// <summary>
//        /// 完成放置流程：显示渲染 → 墙体隐藏 → 可见性管理 → 库存更新 → UI刷新
//        /// </summary>
//        //private void FinalizePlacement(Room_Placement_Info item,
//        //                               PlacementRuntime placement,
//        //                               Vector3 orient)
//        //{
//        //    _placement_list.Add(placement);
//        //    placement.set_render_state(true);
//        //    placement.after_add_placement();

//        //    if (item.placing_type == ENUM.Room_Placing_Type.Wall_Furniture)
//        //        placement.set_up_hide_wall(orient);

//        //    book_keeping_visibility(placement.gameObject);

//        //    Global_Inventory_Manager.Change_Items_Count(new List<(string, int)>
//        //    {
//        //        (item.room_placement_name, -1)
//        //    });

//        //    _warehouse_UI_event_hub._invoke_on_refresh_warehouse_UI();
//        //}



//        //public void book_keeping_visibility(GameObject _go)
//        //{
//        //    var _all_renderers_1 = _go.GetComponentsInChildren<Renderer>(true);
//        //    for (int i = 0; i < _all_renderers_1.Length; i++)
//        //    {
//        //        if (_all_renderer.Contains(_all_renderers_1[i]) == true) continue;
//        //        _all_renderer.Add(_all_renderers_1[i]);
//        //    }

//        //    var _all_light_1 = _go.GetComponentsInChildren<Light>(true);
//        //    for (int i = 0; i < _all_light_1.Length; i++)


//        //    {
//        //        if (_all_light.Contains(_all_light_1[i]) == true) continue;
//        //        _all_light.Add(_all_light_1[i]);
//        //    }
//        //    var _all_ps_1 = _go.GetComponentsInChildren<ParticleSystem>(true);
//        //    for (int i = 0; i < _all_ps_1.Length; i++)
//        //    {
//        //        if (_all_ps_1.Contains(_all_ps_1[i]) == true) continue;
//        //        _all_particle_system.Add(_all_ps_1[i]);
//        //    }

//        //    _refresh_view();
//        //}

//        //public void output_to_ui(string output)
//        //{
//        //    EvtDsp.TriggerEvt<string>(EvtNames.ShowWaringPanel, output);
//        //}



//        //public void move_placement(string _id, Vector3 target_position, Vector3 _orient, string _parent_id = "NULL")
//        //{
//        //    Debug.Log("Indoor_Room_Game_Manager:move_placement() _id:" + _id + " target_position:" + target_position + " parent_id:" + _parent_id);

//        //    StartCoroutine(_move_placement_co(_id, target_position, _orient, _parent_id));
//        //}

//        //public IEnumerator _move_placement_co(string _id, Vector3 target_position, Vector3 _orient, string _parent_id)
//        //{

//        //    var _placement = _placement_list.Find(x => x._placement_in_level_info._placement_id_in_level == _id);

//        //    var _item = _placement._placement_in_level_info._placement_info;

//        //    _placement.Set_border_Active(false);
//        //    if (_placement == null) yield break;
//        //    var _pre_pos = _placement._placement_in_level_info.position;
//        //    var _pre_orient = _placement._placement_in_level_info._rotation_index;
//        //    var _pre_parent_id = _placement._placement_in_level_info.parent_placement_id_in_level;
//        //    var _pre_parent = _placement_list.Find(x => x._placement_in_level_info._placement_id_in_level == _pre_parent_id);

//        //    var _next_pos = GMC.from_Vector3(target_position);
//        //    var _next_orient = GMC.from_Vector3(_orient);
//        //    var _next_rot_index = room_placement_in_level_info.float_3_to_rotation_index(_next_orient);
//        //    _placement.set_render_state(false);

//        //    _placement.gameObject.transform.position = new Vector3(_next_pos._x, _next_pos._y, _next_pos._z);
//        //    if (_placement._placement_in_level_info._placement_info.placing_type == ENUM.Room_Placing_Type.Wall_Furniture)
//        //    {
//        //        _placement._placement_in_level_info._rotation_index = _next_rot_index;
//        //        _placement._switch_rotation();
//        //    }

//        //    _general_interaction_event_hub._invoke_on_set_all_placement_collider(true);

//        //    yield return new WaitForSecondsRealtime(0.25f);

//        //    bool can_move = true;

//        //    var _collider = _placement._grid_collider;
//        //    int area = _item.length * _item.width;
//        //    check_placement_overlapped(
//        //    _collider,
//        //    _orient,
//        //  out int actual_hit_count,
//        //  out bool is_hit_other_object,_placement._placement_in_level_info._placement_info.first_Category);


//        //    if (is_hit_other_object == true)
//        //    {
//        //        output_to_ui("和其他放置物重叠，无法移动");
//        //        can_move = false;
//        //    }
//        //    if (actual_hit_count != area)
//        //    {
//        //        output_to_ui("空间问题，无法移动");
//        //        can_move = false;
//        //    }


//        //    //TODO check can move
//        //    if (can_move == true)
//        //    {
//        //        _level_info.move_placement(_id, _next_pos, _next_rot_index, _parent_id);

//        //        if (_parent_id != "NULL")
//        //        {
//        //            var _neo_parent = _placement_list.Find(_p => _p._placement_in_level_info._placement_id_in_level == _parent_id);
//        //            if (_pre_parent != null)
//        //            {
//        //                _pre_parent._sub_placement_list.Remove(_placement);
//        //                handle_detach_rotation(_placement);

//        //            }
//        //            _neo_parent._sub_placement_list.Add(_placement);
//        //            _placement.gameObject.transform.SetParent(_neo_parent.sub_placement_root, true);
//        //            handle_attach_rotation(_placement, _neo_parent);
//        //        }
//        //        else
//        //        {
//        //            if (_pre_parent != null)
//        //            {
//        //                _pre_parent._sub_placement_list.Remove(_placement);
//        //            }
//        //            _placement.gameObject.transform.SetParent(Room_Placement_Root);
//        //            handle_detach_rotation(_placement);
//        //        }
//        //        if (_placement._placement_in_level_info._placement_info.placing_type == ENUM.Room_Placing_Type.Wall_Furniture)
//        //        {
//        //            if (_placement._placement_in_level_info._sub_placement_id_in_level != null)
//        //            {
//        //                foreach (var item_id in _placement._placement_in_level_info._sub_placement_id_in_level)
//        //                {
//        //                    var _sub_placement = _placement_list.Find(x => x._placement_in_level_info._placement_id_in_level == item_id);
//        //                    if (_sub_placement == null) continue;
//        //                    StartCoroutine(handle_sub_placement_rotation_by_parent_co(_sub_placement, _placement));
//        //                }
//        //            }
//        //        }
//        //    }
//        //    else
//        //    {
//        //        //reset position
//        //        _placement.gameObject.transform.position = new Vector3(_pre_pos._x, _pre_pos._y, _pre_pos._z);
//        //        _placement._placement_in_level_info.position = _pre_pos;
//        //        if (_placement._placement_in_level_info._placement_info.placing_type == ENUM.Room_Placing_Type.Wall_Furniture)
//        //        {
//        //            _placement._placement_in_level_info._rotation_index = _pre_orient;
//        //            _placement._switch_rotation();
//        //        }
//        //    }

//        //    _placement.set_render_state(true);
//        //    _placement.set_up_hide_wall(_orient);
//        //    _placement.after_transform();

//        //    Global_Home_Room_Manager.Instance.re_bake_navmesh();
//        //    //yield break;
//        //}

//        //public void _on_enter_add_placement()
//        //{
//        //    if (_activc_instance != this) return;
//        //    _current_state = "Add_Placement";
//        //    Display_grids(_current_selected_placement_info);

//        //    on_deselect_room_placement();

//        //    //_general_interaction_event_hub._invoke_on_set_lean_multi_update(false);
//        //}

//        //public void Display_grids(Room_Placement_Info placementInfo)
//        //{
//        //    _grid.gameObject.SetActive(true);

//        //    var gridManager = _grid.GetComponent<Indoor_Game_Grid_Manager>();
//        //    if (gridManager == null)
//        //        return;

//        //    // 重置状态
//        //    gridManager.set_floor_state(false);
//        //    gridManager.set_wall_state(false);
//        //    gridManager.set_ceiling_state(false);

//        //    switch (placementInfo.placing_type)
//        //    {
//        //        case ENUM.Room_Placing_Type.Floor_Furniture:
//        //            //set_all_sub_grid_active(placementInfo.whether_put);
//        //            gridManager.set_floor_state(true);
//        //            break;

//        //        case ENUM.Room_Placing_Type.Wall_Furniture:
//        //            gridManager.set_wall_state(true);
//        //            break;

//        //        case ENUM.Room_Placing_Type.Ceiling_Furniture:
//        //            gridManager.set_ceiling_state(true);
//        //            break;
//        //    }

//        //    gridManager.Refresh_cells_state();

//        //    SetOtherPlacement_Collider(false);
//        //}

       
//        //public void _on_exit_add_placement()
//        //{
//        //    if (_activc_instance != this) return;
//        //    _current_state = "Default";

//        //    hide_grid();

//        //    if (_current_selected_placement != null)
//        //    {
//        //        _current_selected_placement.on_deselect_placement();
//        //    }
//        //    _current_selected_grid_cell = null;
//        //    _current_selected_placement_info = null;
//        //    _current_selected_placement = null;
//        //    set_all_sub_grid_active(false);

//        //    if (_general_interaction_event_hub != null)
//        //    {
//        //        _general_interaction_event_hub._invoke_on_close_placement_ghost();
//        //        _general_interaction_event_hub._invoke_on_set_lean_multi_update(true);
//        //        _general_interaction_event_hub._invoke_on_set_all_placement_collider(true);
//        //    }

//        //    _warehouse_UI_event_hub._invoke_deselected_item();

//        //    _general_interaction_event_hub._invoke_on_set_all_placement_collider(true);
//        //    //_warehouse_UI_event_hub._invoke_toggle_selection_current_unit();
//        //}


//        //public void hide_grid()
//        //{

//        //}

//        //public void _on_enter_moving_placement()
//        //{
//        //    if (_activc_instance != this) return;
//        //    _current_state = "Moving_Placement";
//        //    Display_grids(_current_selected_placement._placement_in_level_info._placement_info);

//        //    _general_interaction_event_hub._invoke_on_set_lean_multi_update(false);
//        //}

//        //public void _on_exit_moving_placement()
//        //{

//        //}
//        //public void acitve_room()
//        //{
//        //    _is_active = true;

//        //    _activc_instance = this;
//        //    _grid.gameObject.SetActive(true);
//        //    foreach (var _renderer in _all_renderer)
//        //    {
//        //        if (_renderer != null)
//        //        {

//        //            var hide_wall = _renderer.gameObject.GetComponent<Scene_View_Control.Hide_Wall>();
//        //            if (hide_wall != null)
//        //            {
//        //                hide_wall._force_hide = false;
//        //            }
//        //            else
//        //            {
//        //                _renderer.enabled = true;
//        //            }
//        //        }
//        //    }
//        //    foreach (var _light in _all_light)
//        //    {
//        //        if (_light != null)
//        //        {
//        //            _light.enabled = true;
//        //        }
//        //    }
//        //    foreach (var _ps in _all_particle_system)
//        //    {
//        //        if (_ps != null)
//        //        {
//        //            _ps.gameObject.SetActive(true);
//        //        }
//        //    }
//        //    foreach (var canvas in _all_canvas)
//        //    {
//        //        if (canvas != null)
//        //        {
//        //            canvas.gameObject.SetActive(true);
//        //        }
//        //    }
//        //    foreach (var _renderer in _character_renderers)
//        //    {
//        //        if (_renderer != null)
//        //        {
//        //            _renderer.enabled = true;
//        //        }
//        //    }

//        //    foreach (var door in _door_renders)
//        //    {
//        //        if (door != null)
//        //        {
//        //            door.gameObject.GetComponent<Collider>().enabled = true;
//        //        }
//        //    }
//        //    foreach (var _wall in _wall_renderers)
//        //    {
//        //        if (_wall != null)
//        //        {
//        //            _wall.gameObject.GetComponent<Collider>().enabled = true;
//        //        }
//        //    }

//        //    _camera_root.gameObject.SetActive(true);

//        //    _on_active_room.Invoke();

//        //    if (indoorRoomInteractManager != null)
//        //    {
//        //        indoorRoomInteractManager.SetActive(true);
//        //    }
//        //}

//        //public IEnumerator ResetCameraCo()
//        //{
//        //    yield return new WaitForSeconds(0.1f);
//        //    Global_Home_Room_Manager.Instance._indoor_event._invoke_on_reset_camera(_camera_root.gameObject);
//        //}
//        //public void deactive_room()
//        //{
//        //    _is_active = false;

//        //    _grid.gameObject.SetActive(false);
//        //    foreach (var _renderer in _all_renderer)
//        //    {
//        //        if (_renderer != null)
//        //        {
//        //            _renderer.enabled = false;
//        //            var hide_wall = _renderer.gameObject.GetComponent<Scene_View_Control.Hide_Wall>();
//        //            if (hide_wall != null) hide_wall._force_hide = true;
//        //        }
//        //    }
//        //    foreach (var _light in _all_light)
//        //    {
//        //        if (_light != null)
//        //        {
//        //            _light.enabled = false;
//        //        }
//        //    }
//        //    foreach (var _ps in _all_particle_system)
//        //    {
//        //        if (_ps != null)
//        //        {
//        //            _ps.gameObject.SetActive(false);
//        //        }
//        //    }
//        //    foreach (var canvas in _all_canvas)
//        //    {
//        //        if (canvas != null)
//        //        {
//        //            canvas.gameObject.SetActive(false);
//        //        }
//        //    }
//        //    foreach (var _renderer in _character_renderers)
//        //    {
//        //        if (_renderer != null)
//        //        {
//        //            _renderer.enabled = false;
//        //        }
//        //    }
//        //    foreach (var door in _door_renders)
//        //    {
//        //        if (door != null)
//        //        {
//        //            door.gameObject.GetComponent<Collider>().enabled = false;
//        //        }
//        //    }
//        //    foreach (var _wall in _wall_renderers)
//        //    {
//        //        if (_wall != null)
//        //        {
//        //            _wall.gameObject.GetComponent<Collider>().enabled = false;
//        //        }
//        //    }
//        //    /*
//        //        foreach (var _door in _door_colliders)
//        //       {
//        //           _door.enabled = false;
//        //       }
//        //     */
//        //    _camera_root.gameObject.SetActive(false);
//        //    _on_deactive_room.Invoke();

//        //    if (indoorRoomInteractManager != null)
//        //    {
//        //        indoorRoomInteractManager.SetActive(false);
//        //    }
//        //}
//        //public void _refresh_view()
//        //{
//        //    if (_activc_instance != this)
//        //    {
//        //        deactive_room();
//        //    }
//        //    else
//        //    {
//        //        acitve_room();
//        //    }

//        //}

//        //public void check_placement_overlapped(
//        //    Collider _collider,
//        //    Vector3 _valid_grid_orient,
//        //    out int actual_hit_count,
//        //    out bool is_hit_other_object, Placement_First_Category _First_Category)
//        //{
//        //    Collider[] _hits = new Collider[200];
//        //    int _hit_count = Physics.OverlapBoxNonAlloc(
//        //     _collider.bounds.center,
//        //     _collider.bounds.extents,
//        //        _hits,
//        //          //_collider.gameObject.transform.rotation,
//        //          Quaternion.identity,
//        //        _check_mask,
//        //        QueryTriggerInteraction.Collide
//        //        );

//        //    actual_hit_count = 0;
//        //    is_hit_other_object = false;
//        //    for (int i = 0; i < _hit_count; i++)
//        //    {
//        //        var hit = _hits[i];
//        //        if (hit.gameObject == _collider.gameObject) continue;

//        //        if (hit.gameObject.tag == "Room_Placement")
//        //        {

//        //            var _obj = hit.gameObject.GetComponentInParent<PlacementRuntime>();

//        //            //如果有一方是地毯直接跳过，如果都是地毯不能跳过（地毯不能放在地毯上）
//        //            // 判断对方是否是地毯（_obj为空则视为不是）
//        //            bool otherIsCarpet = (_obj != null) && (_obj._placement_in_level_info._placement_info.first_Category == Placement_First_Category.Carpet);
//        //            // 判断自己是否是地毯
//        //            bool selfIsCarpet = (_First_Category == Placement_First_Category.Carpet);

//        //            // 只要有一方是地毯，就跳过
//        //            if (selfIsCarpet ^ otherIsCarpet) // 异或运算：一方为true，一方为false
//        //            {
//        //                continue;
//        //            }

//        //            if (_obj == null)
//        //            {

//        //                is_hit_other_object = true;
//        //                break;
//        //            }
//        //            else
//        //            {
//        //                //self
//        //                if (_collider.gameObject == hit.gameObject)
//        //                {
//        //                    continue;
//        //                }

//        //                if (_current_selected_placement != null)
//        //                {
//        //                    //children
//        //                    if (_current_selected_placement._sub_placement_list != null)
//        //                    {
//        //                        if (_current_selected_placement._sub_placement_list.Contains(_obj))
//        //                        {
//        //                            continue;
//        //                        }
//        //                    }

//        //                    //parent
//        //                    var _parent_id = _current_selected_placement._placement_in_level_info.parent_placement_id_in_level;
//        //                    if (_parent_id != null &&
//        //                        _parent_id.Length != 0 &&
//        //                        _parent_id != "NULL"
//        //                        )
//        //                    {
//        //                        if (_obj._placement_in_level_info._placement_id_in_level == _parent_id)
//        //                        {
//        //                            continue;
//        //                        }
//        //                    }
//        //                }
//        //                //parent during add placementInfo
//        //                if (_current_selected_placement_info != null && _current_selected_grid_cell != null)
//        //                {
//        //                    if (_current_selected_grid_cell._parent_placement != null &&
//        //                        _current_selected_grid_cell._parent_placement._placement_in_level_info != null
//        //                        )
//        //                    {
//        //                        if (_obj._placement_in_level_info._placement_id_in_level ==
//        //                    _current_selected_grid_cell._parent_placement._placement_in_level_info._placement_id_in_level)
//        //                        {
//        //                            continue;
//        //                        }
//        //                    }

//        //                }
//        //                //TODO parent during moving
//        //                if (_current_selected_grid_cell != null)
//        //                {
//        //                    if (_current_selected_grid_cell._parent_placement != null &&
//        //                        _current_selected_grid_cell._parent_placement._placement_in_level_info != null
//        //                        )
//        //                    {
//        //                        if (_obj._placement_in_level_info._placement_id_in_level ==
//        //                    _current_selected_grid_cell._parent_placement._placement_in_level_info._placement_id_in_level)
//        //                        {
//        //                            continue;
//        //                        }
//        //                    }
//        //                }

//        //                is_hit_other_object = true;
//        //                break;
//        //            }

//        //        }
//        //        else if (hit.gameObject.tag == "Cell")
//        //        {
//        //            var _cell = hit.gameObject.GetComponentInParent<Game_Grid_Cell>();
//        //            if (_cell == null) continue;
//        //            if (_cell._parent_placement != null)
//        //            {
//        //                if (_current_selected_grid_cell != null)
//        //                {
//        //                    if (_current_selected_grid_cell._parent_placement != null)
//        //                    {
//        //                        if (_current_selected_grid_cell._parent_placement != _cell._parent_placement) continue;
//        //                    }
//        //                }
//        //            }
//        //            if (_cell.cell_orient == _valid_grid_orient)
//        //            {
//        //                actual_hit_count++;
//        //            }

//        //        }

//        //    }
//        //}

//        //public void on_delete_room_placement(PlacementRuntime room_Placement_In_Level)
//        //{
//        //    if (_activc_instance != this) return;
//        //    if (room_Placement_In_Level._sub_placement_list != null
//        //        && room_Placement_In_Level._sub_placement_list.Count != 0)
//        //    {
//        //        foreach (var sub_placement in room_Placement_In_Level._sub_placement_list)
//        //        {
//        //            if (sub_placement != null)
//        //            {
//        //                _level_info.remove_placement(sub_placement._placement_in_level_info._placement_id_in_level);
//        //                _placement_list.Remove(sub_placement);
//        //                //Destroy(sub_placement.gameObject);
//        //            }

//        //        }
//        //    }
//        //    _level_info.remove_placement(room_Placement_In_Level._placement_in_level_info._placement_id_in_level);
//        //    _placement_list.Remove(room_Placement_In_Level);
//        //    List<(string, int)> _inventory_change = new List<(string, int)>();

//        //    foreach (var sub_placement in room_Placement_In_Level._sub_placement_list)
//        //    {
//        //        if (sub_placement != null)
//        //        {

//        //            Destroy(sub_placement.gameObject);
//        //            _inventory_change.Add((sub_placement._placement_in_level_info._placement_info.room_placement_name, 1));
//        //        }

//        //    }
//        //    //change _inventory
//        //    _inventory_change.Add((room_Placement_In_Level._placement_in_level_info._placement_info.room_placement_name, 1));
//        //    Global_Inventory_Manager.Change_Items_Count(in _inventory_change);
//        //    _warehouse_UI_event_hub._invoke_on_refresh_warehouse_UI();
//        //    Destroy(room_Placement_In_Level.gameObject);

//        //    StartCoroutine(clear_placement_list_co());
//        //    Global_Home_Room_Manager.Instance.re_bake_navmesh();
//        //}


//        //public IEnumerator clear_placement_list_co()
//        //{
//        //    yield return new WaitForSecondsRealtime(0.5f);
//        //    _placement_list.RemoveAll(_p => _p == null);
//        //}

//        //public void on_select_room_placement(PlacementRuntime room_Placement_In_Level)
//        //{

//        //    if (_activc_instance != this) return;
//        //    if (_current_selected_placement != null &&
//        //        _current_selected_placement != room_Placement_In_Level)
//        //    {
//        //        _current_selected_placement.on_deselect_placement();
//        //    }
//        //    _current_selected_placement = room_Placement_In_Level;

//        //    _on_enter_moving_placement();

//        //    _warehouse_UI_event_hub._invoke_deselected_item();
//        //    _general_interaction_event_hub._invoke_on_close_placement_ghost();

//        //    _general_interaction_event_hub._invoke_on_change_select_object_material(room_Placement_In_Level.gameObject);
//        //    _general_interaction_event_hub._invoke_on_set_lean_multi_update(false);
//        //    roomPlacementMenu.SetActive(true);
//        //}

//        //public void on_deselect_room_placement()
//        //{
//        //    if (_activc_instance != this) return;
//        //    _general_interaction_event_hub._invoke_on_restore_select_object_mateiral();
//        //    roomPlacementMenu.SetActive(false);

//        //    if(_current_selected_placement!=null)
//        //    {
//        //        _current_selected_placement.on_deselect_placement();
//        //        _current_selected_placement = null;
//        //    }    
//        //}

//        //public void on_rotate_placement(PlacementRuntime room_Placement_In_Level, int rotation_offset)
//        //{
//        //    if (_activc_instance != this) return;
//        //    if (room_Placement_In_Level._placement_in_level_info._placement_info.placing_type == ENUM.Room_Placing_Type.Wall_Furniture)
//        //    {
//        //        Debug.Log("无法旋转墙面家具!!!!");
//        //        return;
//        //    }
//        //    StartCoroutine(on_rotate_placement_co(room_Placement_In_Level, rotation_offset));
//        //}

//        //public IEnumerator on_rotate_placement_co(PlacementRuntime room_Placement_In_Level, int rotation_offset)
//        //{
//        //    if (_activc_instance != this)
//        //    {
//        //        yield break;
//        //    }
//        //    Display_grids(room_Placement_In_Level._placement_in_level_info._placement_info);

//        //    // _general_interaction_event_hub._invoke_on_set_all_placement_collider(true);
//        //    yield return new WaitForSecondsRealtime(0.2f);

//        //    int pre_rot = room_Placement_In_Level._placement_in_level_info._rotation_index;
//        //    int rot = (pre_rot + 4 + rotation_offset) % 4;

//        //    var _placement = room_Placement_In_Level;

//        //    _placement._placement_in_level_info._rotation_index = rot;
//        //    var _item = _placement._placement_in_level_info._placement_info;

//        //    _placement._switch_rotation();
//        //    _placement.set_render_state(false);
//        //    _placement.Show_Grid(false);
//        //    yield return new WaitForSecondsRealtime(0.125f);

//        //    Action _reset = () =>
//        //    {
//        //        _placement._placement_in_level_info._rotation_index = pre_rot;
//        //        _placement._switch_rotation();
//        //        _placement.set_render_state(true);
//        //        hide_grid();
//        //    };
//        //    var _orient = new Vector3(0, 1, 0);

//        //    var _collider = _placement._grid_collider;
//        //    int area = _item.length * _item.width;
//        //    check_placement_overlapped(
//        //    _collider,
//        //    _orient,
//        //  out int actual_hit_count,
//        //  out bool is_hit_other_object, 
//        //  room_Placement_In_Level._placement_in_level_info._placement_info.first_Category);

//        //    if (is_hit_other_object == true)
//        //    {
//        //        //Destroy(_pot.gameObject);
//        //        output_to_ui("和其他物品重叠，无法旋转");
//        //        _reset();
//        //        yield break;
//        //    }
//        //    if (actual_hit_count != area)
//        //    {
//        //        //Destroy(_pot.gameObject);
//        //        output_to_ui("空间问题，无法旋转");
//        //        _reset();
//        //        yield break;
//        //    }


//        //    _level_info.rotate_placement(
//        //        room_Placement_In_Level._placement_in_level_info._placement_id_in_level,
//        //        rot);
//        //    // room_Placement_In_Level._placement_info._rotation_index = rot;
//        //    //TODO handle sub placementInfo
//        //    _placement.set_render_state(true);
//        //    _placement.after_transform();
//        //    hide_grid();

//        //    if (room_Placement_In_Level._sub_placement_list != null && room_Placement_In_Level._sub_placement_list.Count != 0)
//        //    {
//        //        foreach (var sub_placement in room_Placement_In_Level._sub_placement_list)
//        //        {
//        //            if (sub_placement != null)
//        //            {
//        //                handle_sub_placement_rotation_by_parent(sub_placement, room_Placement_In_Level);
//        //            }
//        //        }
//        //    }

//        //    Global_Home_Room_Manager.Instance.re_bake_navmesh();
//        //}
//        //public IEnumerator handle_sub_placement_rotation_by_parent_co(
//        //     PlacementRuntime _sub_placement,
//        //    PlacementRuntime _parent_placement)
//        //{

//        //    int parent_rot = _parent_placement._placement_in_level_info._rotation_index;
//        //    int sub_rot = _sub_placement._placement_in_level_info._rotation_index;
//        //    int rot = (parent_rot + 4 + sub_rot) % 4;
//        //    Vector3 _origin = _sub_placement._grid_collider.bounds.min - new Vector3(0.1f, 0, 0.1f);
//        //    Debug.Log("Sub_Placement_Origin_" + _origin + "_Sub_Placement_Origin_rot_" + rot);
//        //    yield return new WaitForSecondsRealtime(0.125f);
//        //    _level_info.move_placement(
//        //        _sub_placement._placement_in_level_info._placement_id_in_level,
//        //        GMC.from_Vector3(_origin),
//        //        sub_rot,
//        //        _parent_placement._placement_in_level_info._placement_id_in_level
//        //        );
//        //    _sub_placement.transform.SetParent(Room_Placement_Root, true);
//        //    _sub_placement.transform.position = _origin;
//        //    _sub_placement.transform.rotation = Quaternion.identity;
//        //    _sub_placement._switch_rotation();
//        //    _sub_placement.transform.SetParent(_parent_placement.sub_placement_root, true);
//        //}
//        //public void handle_sub_placement_rotation_by_parent(
//        //    PlacementRuntime _sub_placement,
//        //    PlacementRuntime _parent_placement
//        //    )
//        //{
//        //    StartCoroutine(handle_sub_placement_rotation_by_parent_co(_sub_placement, _parent_placement));
//        //}

//        //public void handle_detach_rotation(PlacementRuntime _sub_placement)
//        //{
//        //    _sub_placement._placement_in_level_info._rotation_index = _sub_placement._placement_in_level_info._actual_rotation_index;
//        //}
//        //public void handle_attach_rotation(PlacementRuntime _sub_placement, PlacementRuntime _parent)
//        //{
//        //    _sub_placement._placement_in_level_info._rotation_index =
//        //        (
//        //        _sub_placement._placement_in_level_info._actual_rotation_index -
//        //        _parent._placement_in_level_info._rotation_index + 4
//        //        ) % 4;

//        //}


//        //public async void saving_current_state_to_disk_as_json(string file_name)
//        //{
//        //    string _json = JsonConvert.SerializeObject(_level_info);

//        //    string path = System.IO.Path.Combine(Application.persistentDataPath, file_name);
//        //    await System.IO.File.WriteAllTextAsync(path, _json);
//        //    Debug.Log("Indoor_Room_Game_Manager:saving_current_state_to_disk_as_json()_OK_path:" + path);
//        //}

//        //public void save_current_state_to_memory()
//        //{
//        //    _tempSaveJson = JsonConvert.SerializeObject(_level_info);
//        //}

//        //public void load_current_state_from_json(string json)
//        //{
//        //    if (_level_info._room_name.Contains("阳台")) return;
//        //    var _level_info_loaded = JsonConvert.DeserializeObject<Indoor_Room_Info>(json);
//        //    if (_level_info_loaded == null)
//        //    {
//        //        Debug.Log("Indoor_Room_Game_Manager:load_current_state_from_json()_ERROR:json_parse_error");
//        //        return;
//        //    }
//        //    _level_info = _level_info_loaded;
//        //    if (_level_info._floor_info != null)
//        //    {
//        //        change_floor(_level_info._floor_info.room_placement_name, true);
//        //    }

//        //    if (_level_info._wall_info != null)
//        //    {
//        //        change_wall(_level_info._wall_info.room_placement_name, true);
//        //    }
//        //    _level_info_loaded._placement_db = placement_SO._placement_db;

//        //    _level_info_loaded._placement_list.Sort((a, b) =>
//        //    {
//        //        if (a.have_sub_placement() == true && b.have_parent_placement() == true) return -1;
//        //        if (a.have_parent_placement() == true && b.have_sub_placement() == true) return 1;
//        //        return 0;
//        //    });

//        //    foreach (var _item in _level_info_loaded._placement_list)
//        //    {
//        //        var _item_info = placementNameDic[_item._placement_prototype_name];
//        //        _item._placement_info = _item_info;
//        //        if (_item_info == null) continue;

//        //        PM_RM.load_game_object_async(_item_info.res_url, (go) =>
//        //        {
//        //            add_placement_without_check(go, _item);
//        //        });
//        //    }
//        //    if (_level_info._door_info_list != null)
//        //    {
//        //        foreach (var _door in _level_info._door_info_list)
//        //        {
//        //            try_change_door(_door._door_name, _door._door_item_name, false);
//        //        }
//        //    }
//        //    if (Global_Home_Room_Manager.Instance != null) Global_Home_Room_Manager.Instance.re_bake_navmesh();

//        //}
//        //public void add_placement_without_check(
//        //    GameObject go,
//        //    room_placement_in_level_info _item_in_level)

//        //{
//        //    var _item = _item_in_level._placement_info;
//        //    var target_position = GMC.from_float_3(_item_in_level.position);
//        //    var orient = _item_in_level._actual_rotation_index;
//        //    var _parent_id = _item_in_level.parent_placement_id_in_level;

//        //    var _go_in_level = Instantiate(go, target_position, Quaternion.identity);
//        //    var _placement_in_level = _go_in_level.GetComponent<PlacementRuntime>();
//        //    _placement_in_level._placement_in_level_info = _item_in_level;
//        //    if (_item.placing_type == ENUM.Room_Placing_Type.Floor_Furniture)
//        //    {
//        //        // _placement_in_level.set_render_state(true);
//        //        int rot_index = orient;
//        //        _placement_in_level._placement_in_level_info._rotation_index = rot_index;
//        //        _placement_in_level._switch_rotation();

//        //        if (Room_Placement_Root != null && _parent_id == "NULL")
//        //        {
//        //            _placement_in_level.gameObject.transform.SetParent(Room_Placement_Root, true);
//        //        }
//        //        else if (_parent_id != "NULL")
//        //        {
//        //            var _placement = _placement_list.Find(x => x._placement_in_level_info._placement_id_in_level == _parent_id);
//        //            if (_placement != null)
//        //            {
//        //                _placement_in_level.gameObject.transform.SetParent(_placement.sub_placement_root, true);
//        //            }
//        //            _placement._sub_placement_list.Add(_placement_in_level);
//        //        }

//        //        //  _placement_in_level.update_view();

//        //        _placement_list.Add(_placement_in_level);
//        //        _placement_in_level.set_render_state(true);
//        //        _placement_in_level.after_add_placement();
//        //        book_keeping_visibility(_placement_in_level.gameObject);


//        //        Global_Home_Room_Manager.Instance.re_bake_navmesh();
//        //    }

//        //    if (_item.placing_type == Room_Placing_Type.Wall_Furniture)
//        //    {
//        //        // var _go_in_level = Instantiate(go, target_position, Quaternion.identity);
//        //        //var _placement_in_level = _go_in_level.GetComponent<PlacementRuntime>();

//        //        int rot_index = orient;
//        //        _placement_in_level._placement_in_level_info._rotation_index = rot_index;
//        //        _placement_in_level._switch_rotation();

//        //        _placement_in_level.set_render_state(false);

//        //        if (Room_Placement_Root != null && _parent_id == "NULL")
//        //        {
//        //            _placement_in_level.gameObject.transform.SetParent(Room_Placement_Root, true);
//        //        }
//        //        else if (_parent_id != "NULL")
//        //        {
//        //            var _parent_placement = _placement_list.Find(x => x._placement_in_level_info._placement_id_in_level == _parent_id);
//        //            if (_parent_placement != null)
//        //            {
//        //                _placement_in_level.gameObject.transform.SetParent(_parent_placement.sub_placement_root, true);

//        //                _parent_placement._sub_placement_list.Add(_placement_in_level);
//        //                // _placement_in_level._placement_info.parent_placement_id_in_level = _parent_id;
//        //            }
//        //        }

//        //        //  _placement_in_level.update_view();

//        //        _placement_list.Add(_placement_in_level);

//        //        _placement_in_level.after_add_placement();

//        //        _placement_in_level.set_up_hide_wall(PlacementRuntime.Rotation_index_to_Vector3(orient));
//        //        book_keeping_visibility(_placement_in_level.gameObject);

//        //        _warehouse_UI_event_hub._invoke_on_refresh_warehouse_UI();
//        //    }
//        //}

//        //public void clear_all_placement()
//        //{
//        //    for (int i = 0; i < _placement_list.Count; i++)
//        //    {
//        //        var placement = _placement_list[i];
//        //        Destroy(placement.gameObject);
//        //    }
//        //    _placement_list.Clear();


//        //    _level_info._placement_list.Clear();

//        //    clear_visible_item();
//        //}

//        public void clear_visible_item()
//        {
//            _all_light.RemoveAll(_obj => _obj == null);
//            _all_renderer.RemoveAll(_obj => _obj == null);
//            _all_particle_system.RemoveAll(_obj => _obj == null);
//            _all_canvas.RemoveAll(_obj => _obj == null);
//            StartCoroutine(RebakeNavMash(0.1f));
//        }
//        public IEnumerator RebakeNavMash(float seconds)
//        {
//            yield return new WaitForSeconds(seconds);
//            Global_Home_Room_Manager._instance.re_bake_navmesh();
//        }

//        //public void DeleteAllFurniture()
//        //{
//        //    clear_all_placement();
//        //    _on_exit_add_placement();
//        //    _on_exit_moving_placement();
//        //}

//        //public void ConfirmModification()
//        //{
//        //    save_current_state_to_memory();
//        //    _on_exit_add_placement();
//        //    _on_exit_moving_placement();
//        //    _general_interaction_event_hub._invoke_on_close_placement_ghost();
//        //    _general_interaction_event_hub._invoke_on_swipe_close_indoor_room_warehouse_canvas();

//        //    StartCoroutine(saveCo());
//        //}

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
//            //clear_all_placement();
//            //string savePath = System.IO.Path.Combine(Application.persistentDataPath, _level_info._room_name + ".json");
//            //if (System.IO.File.Exists(savePath))
//            //{
//            //    string json = System.IO.File.ReadAllText(savePath);
//            //    load_current_state_from_json(_tempSaveJson);
//            //}
//            //else
//            //{
//            //    Debug.LogWarning("存档文件不存在: " + savePath);
//            //}

//            //string inventoryJson = Global_Inventory_Manager.Inventory_Serialization();

//            //Global_Inventory_Manager.Instance.Load_Data_From_Json(inventoryJson);
//            //_on_exit_add_placement();
//            //_on_exit_moving_placement();
//        }

//        //public bool IsSave()
//        //{
//            //string currentJson = JsonConvert.SerializeObject(_level_info);

//            //if (!string.IsNullOrEmpty(_tempSaveJson))
//            //{
//            //    return currentJson == _tempSaveJson;
//            //}

//            //string path = System.IO.Path.Combine(Application.persistentDataPath, _level_info._room_name + ".json");
//            //if (!System.IO.File.Exists(path))
//            //{
//            //    if (_placement_list.Count == 0)
//            //        return true;
//            //    else
//            //        return false;
//            //}

//            //string savedJson = System.IO.File.ReadAllText(path);

//            //return currentJson == savedJson;
//        //}

//        //public void try_change_door(string door_name, string door_item_name, bool _change_inventory = true)
//        //{
//        //    Debug.Log("Indoor_Room_Game_Manager:try_change_door()_door_name:" + door_name + "_door_item_name:" + door_item_name);
//        //    //TODO : check if input is valid
//        //    var _item = placementNameDic[door_item_name];
//        //    if (_item==null ||  _item.first_Category != Placement_First_Category.Door) return;
//        //    var _door_renderer = _door_renders.Find(x => x.gameObject.name == door_name);
//        //    if (_door_renderer == null) return;

//        //    var _door_in_level = _door_renderer.GetComponent<Door_In_Level>();
//        //    if (_door_in_level == null) return;
//        //    var _pre_door_item_name = _door_in_level._door_info._door_item_name;

//        //    PM_RM.load_material_async(_item.res_url, (mat) =>
//        //    {
//        //        _level_info.change_door(door_name, door_item_name);

//        //        _door_in_level._door_info._door_item_name = door_item_name;
//        //        _door_renderer.material = mat;
//        //        if (_change_inventory == true)
//        //        {
//        //            List<(string, int)> _change_list = new List<(string, int)>();
//        //            _change_list.Add((_pre_door_item_name, +1));
//        //            _change_list.Add((door_item_name, -1));
//        //            Global_Inventory_Manager. Change_Items_Count(in _change_list);


//        //            _warehouse_UI_event_hub._invoke_on_refresh_warehouse_UI();
//        //        }

//        //    });
//        //    //TODO change inventory

//        //}

//        public void update_data_from_server()
//        {
//            _update_data_from_server.Invoke();
//        }
//        public void upload_data_to_server()
//        {
//            _upload_data_to_server.Invoke();
//        }

//    }

////#if UNITY_EDITOR
////    [CustomEditor(typeof(Indoor_Room_Game_Manager))]
////    class Indoor_Room_Game_Manager_Editor : Editor
////    {

////        Indoor_Room_Game_Manager _obj;
////        //GameObject script_object;

////        void OnEnable()
////        {
////            _obj = (Indoor_Room_Game_Manager)target;
////            // script_object = _obj.gameObject;
////        }
////        public override void OnInspectorGUI()
////        {
////            base.OnInspectorGUI();
////            GUILayout.Space(32);
////            if (GUILayout.Button("Active_Room"))
////            {
////                _obj.acitve_room();

////            }
////            if (GUILayout.Button("Deactive_Room"))
////            {
////                _obj.deactive_room();
////            }

////            if (GUILayout.Button("Save_Scene_to_Json"))
////            {
////                _obj.saving_current_state_to_disk_as_json(_obj._level_info._room_name + ".json");
////            }

////            if (GUILayout.Button("Save_Scene_to_Server"))
////            {
////                _obj.Upload_Data_To_Server();
////            }
////        }
////    }
////#endif

//}