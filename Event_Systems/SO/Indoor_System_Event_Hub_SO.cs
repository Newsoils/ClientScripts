using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Client_Event_Systems

        {
            [CreateAssetMenu(fileName = "Indoor_System_Event_Hub_SO", menuName = "Project_Mouse/Client_Event_Systems/Indoor_System_Event_Hub_SO")]
            public class Indoor_System_Event_Hub_SO : ScriptableObject
            {
                public UnityEvent<string> _on_switch_room = new UnityEvent<string>();
                public UnityEvent<GameObject> _on_reset_camera = new UnityEvent<GameObject>();

                public void _invoke_on_switch_room(string input)
                {
                    Debug.Log("_invoke_on_switch_room()_str_=_" + input);

                    _on_switch_room.Invoke(input);
                }

                public void _invoke_on_reset_camera(GameObject cameraRoot)
                {
                    _on_reset_camera.Invoke(cameraRoot);
                }
            }
        }
    }
}
