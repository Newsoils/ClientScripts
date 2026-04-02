using System;
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
            [CreateAssetMenu(fileName = "PlantingSystem_Event_Hub_SO", menuName = "Project_Mouse/Client_Event_Systems/PlantingSystem_Event_Hub_SO")]
            public class PlantingSystem_Event_Hub_SO : ScriptableObject
            {
                public UnityEvent _on_put_down_flower_pot = new UnityEvent();
                public Func<bool> _on_check_harvest_state_and_prompt;
                public UnityEvent<string> _on_show_up_prompt = new UnityEvent<string>();
                public UnityEvent<int, Action> _on_show_prompt = new UnityEvent<int, Action>();
                public UnityEvent<bool> _on_enable_or_disable_reset_camera = new UnityEvent<bool>();

                public void _invoke_on_put_down_flower_pot()
                {
                    _on_put_down_flower_pot.Invoke();
                }

                public bool _invoke_on_check_harvest_state_and_prompt()
                {
                    return _on_check_harvest_state_and_prompt.Invoke();
                }

                public void _invoke_on_show_up_prompt(string input)
                {
                    _on_show_up_prompt.Invoke(input);
                }

                public void _invoke_on_show_prompt(int id, Action onConfirm)
                {
                    _on_show_prompt.Invoke(id, onConfirm);
                }

                public void _invoke_on_enable_or_disable_reset_camera(bool enable)
                {
                    _on_enable_or_disable_reset_camera.Invoke(enable);
                }
            }
        }
    }
}
