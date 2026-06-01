using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;

namespace CLIP.Project_Mouse.Game_Play_System
{

    public class Global_Home_Room_Manager : MonoBehaviour
    {

        public static Global_Home_Room_Manager Instance;

        public NavMeshSurface _current_surface;


        void Awake()
        {
            Instance = this;
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