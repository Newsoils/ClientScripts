//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Planting_System;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using UnityEngine;




//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace Scene_View_Control
//        {
//            public class Plant_In_Scene : MonoBehaviour
//            {
//                public plant_in_level_info _plant_info;
//                public Pot_In_Scene _pot_in_level;

//                public List<GameObject> _sub_view_per_stage = new List<GameObject>();



//                void Start()
//                {

//                }

//                public void init_plant(Pot_In_Scene _pot)
//                {

//                    _plant_info = _pot._pot_info._plant_level_info;

//                    _pot_in_level = _pot;
//                    _pot._plant = this;

//                    update_view();
//                }

//                // Update is called once per frame
//                void Update()
//                {

//                }

//                public void hide_plant()
//                {
//                    foreach (var item in _sub_view_per_stage)
//                    {
//                        item.SetActive(false);
//                    }
//                }

//                public void show_plant()
//                {
//                    foreach (var item in _sub_view_per_stage)
//                    {
//                        item.SetActive(true);
//                    }
//                    switch_model();
//                }


//                public void update_view()
//                {
//                    if (_plant_info == null) return;
//                    if (_plant_info._will_become_rare == true)
//                    {
//                        Planting_System_Manager._instance.change_to_rare(
//                            this.transform.position,
//                            _plant_info,_pot_in_level
//                            );

//                        //Destroy(this.gameObject);
//                        prepare_for_destory();
//                        return;
//                    }
//                    Debug.Log("更新植物外观");
//                    switch_model();

//                    //Global_Home_Room_Manager.Instance._balcony.book_keeping_visibility(this.gameObject);
//                }
//                public void prepare_for_destory()
//                {
//                    foreach(var go in _sub_view_per_stage)go.SetActive(false);
//                    _sub_view_per_stage = null;

//                }
//                public void switch_model()
//                {
//                    Debug.Log("_sub_view_per_stage：" + _sub_view_per_stage.Count);
//                    Debug.Log("_plant_info.current_stage：" + _plant_info.current_stage);
//                    for (int i = 0; i < _sub_view_per_stage.Count; i++)
//                    {
//                        if (_plant_info.current_stage == i)
//                        {
//                            _sub_view_per_stage[i].SetActive(true);
//                        }
//                        else
//                        {
//                            _sub_view_per_stage[i].SetActive(false);
//                        }

//                    }
//                }
               
//            }
//        }
//    }
//}