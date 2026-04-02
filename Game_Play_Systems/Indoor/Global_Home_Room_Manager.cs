using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using Unity.AI.Navigation;
using CLIP.Project_Mouse.Client_Event_Systems;

namespace CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System
{

    public class Global_Home_Room_Manager : MonoBehaviour
    {

        public static Global_Home_Room_Manager _instance;

        public NavMeshSurface _current_surface;

        public bool _can_edit_placement = true;

        public bool _can_switch_scene = true;
        public bool _lock_switch_room = false;


        void Awake()
        {
            _instance = this;
        }

        public bool can_edit_placement()
        {
            return _can_edit_placement;
        }

        public void re_bake_navmesh()
        {
            StartCoroutine(re_bake_navmesh_co());
        }
        public IEnumerator re_bake_navmesh_co()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            _current_surface.BuildNavMesh();
        }
       
    }

}