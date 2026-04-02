using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Client_Event_Systems

        {
            [CreateAssetMenu(fileName = "Scene_Management_Event_Hub_SO", menuName = "Project_Mouse/Client_Event_Systems/Scene_Management_Event_Hub_SO")]
            public class Scene_Management_Event_Hub_SO : ScriptableObject
            {
                public UnityEvent<string> _on_load_new_scene=new UnityEvent<string>();

                public void _invoke_on_load_new_scene(string scene_name)
                {
                    Debug.Log("_invoke_on_load_new_scene()_str_=_" + scene_name);

                    _on_load_new_scene.Invoke(scene_name);
                }
            }
        }
    }
}