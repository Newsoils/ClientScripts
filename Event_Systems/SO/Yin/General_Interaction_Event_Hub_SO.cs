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
            [CreateAssetMenu(fileName = "General_Interaction_Event_Hub_SO", menuName = "Project_Mouse/Client_Event_Systems/General_Interaction_Event_Hub_SO")]
            public class General_Interaction_Event_Hub_SO : ScriptableObject
            {
                public UnityEvent _on_put_down_flower_pot = new UnityEvent();
                public Func<bool> _on_check_harvest_state_and_prompt;
                public UnityEvent<string> _on_show_up_prompt = new UnityEvent<string>();
                public UnityEvent<int, Action> _on_show_prompt = new UnityEvent<int, Action>();
                public UnityEvent<bool> _on_enable_or_disable_double_tap_reset_camera = new UnityEvent<bool>();
                public UnityEvent<bool> _on_enable_or_disable_reset_camera = new UnityEvent<bool>();
                public UnityEvent _on_deselect_pot = new UnityEvent();
                public UnityEvent<GameObject> _on_show_grid = new UnityEvent<GameObject>();
                public UnityEvent _on_close_placement_ghost = new UnityEvent();
                public UnityEvent _on_close_pot_ghost = new UnityEvent();
                public UnityEvent<GameObject> _on_change_select_object_material = new UnityEvent<GameObject>();
                public UnityEvent _on_restore_select_object_mateiral = new UnityEvent();
                public UnityEvent<bool> _on_set_lean_multi_update = new UnityEvent<bool>();
                public UnityEvent<bool> _on_set_all_placement_collider = new UnityEvent<bool>();
                public UnityEvent _on_change_none_state = new UnityEvent();
                public UnityEvent<GameObject> _on_set_select_pot = new UnityEvent<GameObject>();
                public UnityEvent<int> _on_set_lean_pitch = new UnityEvent<int>();
                public UnityEvent _on_swipe_close_planting_warehouse_canvas = new UnityEvent();
                public UnityEvent _on_swipe_close_indoor_room_warehouse_canvas = new UnityEvent();
                public UnityEvent<string> _on_show_warning_panel = new UnityEvent<string>();
                public UnityEvent _on_refresh_gift = new UnityEvent();

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

                public void _invoke_on_enable_or_disable_double_tap_reset_camera(bool enable)
                {
                    _on_enable_or_disable_reset_camera.Invoke(enable);
                }

                public void _invoke_on_enable_or_disable_reset_camera(bool enable)
                {
                    _on_enable_or_disable_reset_camera.Invoke(enable);
                }

                public void _invoke_on_deselect_pot()
                {
                    _on_deselect_pot.Invoke();
                }

                public void _invoke_on_show_grid(GameObject gameObject)
                {
                    _on_show_grid.Invoke(gameObject);
                }

                public void _invoke_on_close_placement_ghost()
                {
                    _on_close_placement_ghost.Invoke();
                }

                public void _invoke_on_close_pot_ghost()
                {
                    _on_close_pot_ghost.Invoke();
                }

                public void _invoke_on_change_select_object_material(GameObject go)
                {
                    _on_change_select_object_material.Invoke(go);
                }

                public void _invoke_on_restore_select_object_mateiral()
                {
                    _on_restore_select_object_mateiral.Invoke();
                }

                public void _invoke_on_set_lean_multi_update(bool enabled)
                {
                    _on_set_lean_multi_update.Invoke(enabled);
                }

                public void _invoke_on_set_all_placement_collider(bool enabled)
                {
                    _on_set_all_placement_collider.Invoke(enabled);
                }

                public void _invoke_on_change_none_state()
                {
                    _on_change_none_state.Invoke();
                }

                public void _invoke_on_set_select_pot(GameObject pot)
                {
                    _on_set_select_pot.Invoke(pot);
                }

                public void _invoke_on_set_lean_pitch(int x)
                {
                    _on_set_lean_pitch.Invoke(x);
                }

                public void _invoke_on_swipe_close_planting_warehouse_canvas()
                {
                    _on_swipe_close_planting_warehouse_canvas.Invoke();
                }

                public void _invoke_on_swipe_close_indoor_room_warehouse_canvas()
                {
                    _on_swipe_close_indoor_room_warehouse_canvas.Invoke();
                }

                public void _invoke_on_show_warning_panel(string output)
                {
                    _on_show_warning_panel.Invoke(output);
                }

                public void _invoke_on_refresh_gift()
                {
                    _on_refresh_gift.Invoke();
                }
            }
        }
    }
}
