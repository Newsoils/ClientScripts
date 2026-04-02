//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif
////using NavMeshPlus.Components;
//using UnityEngine.AI;
//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace Game_Play_System
//        {
//            namespace Indoor_System
//            {
//                //using static UnityEditor.PlayerSettings;
//                using CC = Coordinate_Convertor;

//                [System.Serializable]
//                public enum Game_Grid_Type
//                {
//                    Ground,
//                    Left,
//                    Right
//                }
//                public class Indoor_Game_Grid : MonoBehaviour
//                {
//                    // Start is called before the first frame update
//                    public static Indoor_Game_Grid _instance;
//                    public GridLayout _grid;

//                    public Transform _grid_root;
//                    public GameObject _unit_prefab;

//                    //public NavMeshPlus.Components.NavMeshSurface _nav_mesh_surface;
//                    public Game_Grid_Type _grid_type;
//                    public int _grid_width = 8;
//                    public int _grid_length = 8;
//                    public List<GameObject> _grid_units = new List<GameObject>();

//                    public Transform _room_component_root;

              
//                    void Start()
//                    {
//                        if(_grid_type== Game_Grid_Type.Ground)
//                        {
//                            _instance = this;
                    
//                        }

//                    }


//                    public void generating_grid()
//                    {
//                        _grid_units.Clear();
//                        for (int i = 0; i < _grid_width; i++)
//                        {
//                            for (int j = 0; j < _grid_length; j++)
//                            {
//                                var game_pos = new Vector3();
//                                var local_pos = new Vector3();
//                                if (_grid_type == Game_Grid_Type.Ground)
//                                {
//                                    game_pos = new Vector3(i + 0.5f, j + 0.5f, 0);
//                                    local_pos = CC.game_to_ground_cell(game_pos);
//                                }

//                                if (_grid_type == Game_Grid_Type.Left)

//                                {
//                                    game_pos = new Vector3(0, i + 0.5f, j + 0.5f);
//                                    local_pos = CC.game_to_left_cell(game_pos);
//                                }

//                                if (_grid_type == Game_Grid_Type.Right)

//                                {
//                                    game_pos = new Vector3(i + 0.5f, 0, j + 0.5f);
//                                    local_pos = CC.game_to_right_cell(game_pos);
//                                }
//                                var pos = _grid.CellToLocalInterpolated(local_pos);
//#if UNITY_EDITOR
//                                var _grid_unit = (GameObject)PrefabUtility.InstantiatePrefab(_unit_prefab);

//#else
//   var _grid_unit = Instantiate<GameObject>(_unit_prefab);
//#endif
//                                _grid_unit.transform.SetParent(_grid_root);
//                                _grid_unit.transform.localPosition = pos;
//                                _grid_unit.transform.localRotation = Quaternion.identity;

//                                var _obj_vars = Variables.Object(_grid_unit);
//                                _obj_vars.Set("Grid_Type", _grid_type);
//                                _grid_units.Add(_grid_unit);
//                                _grid_unit.name = _grid_unit.name + "_" + game_pos.ToString();
//                                var vars = Variables.Object(_grid_unit);
//                                vars.Set("_game_grid_position", game_pos);
//                            }
//                        }
//                    }
//                    public void  update_navmesh()
//                    {
//                        if (_grid_type != Game_Grid_Type.Ground) return;
//                        /*
//                                     if (this._nav_mesh_surface == null) return;
//                             _nav_mesh_surface.UpdateNavMesh(_nav_mesh_surface.navMeshData);

//                         */
//                    }


//                    public void assign_room_component(GameObject go,Vector3 game_pos)
//                    {
                    
//                        var local_pos = new Vector3();
//                        if (_grid_type == Game_Grid_Type.Ground)
//                        {
//                            //game_pos = new Vector3(i + 0.5f, j + 0.5f, 0);
//                            local_pos = CC.game_to_ground_cell(game_pos);
//                        }

//                        if (_grid_type == Game_Grid_Type.Left)

//                        {
//                            //game_pos = new Vector3(0, i + 0.5f, j + 0.5f);
//                            local_pos = CC.game_to_left_cell(game_pos);
//                        }

//                        if (_grid_type == Game_Grid_Type.Right)

//                        {
//                            //game_pos = new Vector3(i + 0.5f, 0, j + 0.5f);
//                            local_pos = CC.game_to_right_cell(game_pos);
//                        }
//                        var _pos = _grid.CellToLocalInterpolated(local_pos);
//                        go.transform.SetParent(_room_component_root,true);
//                        go.transform.localPosition = _pos;
//                        go.transform.rotation = Quaternion.identity;
//                        if (_grid_type == Game_Grid_Type.Right)

//                        {
//                            //game_pos = new Vector3(i + 0.5f, 0, j + 0.5f);
//                            //local_pos = CC.game_to_right_cell(game_pos);
//                            go.transform.localScale = new Vector3(-1, 1, 1);
//                        }
//                    }

//                    public void _change_grid_size(int _size)
//                    {
//                        foreach(var _grid_unit in _grid_units)
//                        {
//                            var _pos = Variables.Object(_grid_unit).Get<Vector3>("_game_grid_position");
//                            if (_pos.x < _size&& _pos.y < _size)
//                            {
//                                _grid_unit.SetActive(true);
//                            }
//                            else
//                            {
//                                _grid_unit.SetActive(false);
//                            }
//                        }
//                    }
//                }

//#if UNITY_EDITOR
//                [CustomEditor(typeof(Indoor_Game_Grid))]
//                class Indoor_Game_Grid_Editor : Editor
//                {

//                    Indoor_Game_Grid _obj;
//                    //GameObject script_object;

//                    void OnEnable()
//                    {
//                        _obj = (Indoor_Game_Grid)target;
//                        // script_object = _obj.gameObject;
//                    }
//                    public override void OnInspectorGUI()
//                    {
//                        base.OnInspectorGUI();
//                        GUILayout.Space(32);
//                        if (GUILayout.Button("Generating_Grid"))
//                        {

//                            //  _obj.load_info_from_JSON();
//                            _obj.generating_grid();
//                        }
//                        if (GUILayout.Button("Clear_All"))
//                        {

//                            foreach (var unit in _obj._grid_units)
//                            {
//                                DestroyImmediate(unit);
//                            }
//                            _obj._grid_units.Clear();
//                        }

//                    }
//                }
//#endif
//            }
//        }
//    }
//}