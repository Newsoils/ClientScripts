using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Client_Event_Systems
        {
            [CreateAssetMenu(fileName = "Change_Clothes_Warehouse_Event_Hub_SO", menuName = "Project_Mouse/Client_Event_Systems/Change_Clothes_Warehouse_Event_Hub_SO")]
            public class Change_Clothes_Warehouse_Event_Hub_SO : ScriptableObject
            {
                public UnityEvent<string> _selected_item_change = new UnityEvent<string>();
                public UnityEvent<string> _deselected_item = new UnityEvent<string>();
                public UnityEvent _toggle_selection_current_unit = new UnityEvent();
                public UnityEvent<string> _first_level_search_tag_change = new UnityEvent<string>();
                public UnityEvent<List<string>> _second_level_search_tag_change = new UnityEvent<List<string>>();
                public UnityEvent<List<string>> _detail_search_item_tags_tag_change = new UnityEvent<List<string>>();
                public UnityEvent<string> _sort_mode_change = new UnityEvent<string>();
                public UnityEvent _on_close_warehouse_UI = new UnityEvent();

                public UnityEvent _on_refresh_warehouse_UI = new UnityEvent();
                public void _invoke_selected_item_change(string input)
                {
                    Debug.Log("_invoke_selected_item_change()_str_=_" + input);

                    _selected_item_change.Invoke(input);
                }

                public void _invoke_deselected_item(string input)
                {
                    Debug.Log("_invoke_deselected_item()");

                    _deselected_item.Invoke(input);
                }

                public void _invoke_toggle_selection_current_unit()
                {
                    Debug.Log("_invoke_toggle_selection_current_unit()");

                    _toggle_selection_current_unit.Invoke();
                }

                public void _invoke_first_level_search_tag_change_change(string input)
                {
                    Debug.Log("_invoke_first_level_search_tag_change()_str_=_" + input);

                    _first_level_search_tag_change.Invoke(input);
                }

                public void _invoke_second_level_search_tag_change(List<string> input)
                {
                    Debug.Log("_invoke_second_level_search_tag_change()_str_=_" + JsonConvert.SerializeObject(input));
                    _second_level_search_tag_change.Invoke(input);
                }

                public void _invoke_detail_search_item_tags_tag_change(List<string> input)
                {
                    Debug.Log("_invoke_detail_search_item_tags_tag()_str_=_" + JsonConvert.SerializeObject(input));
                    _detail_search_item_tags_tag_change.Invoke(input);
                }

                public void _invoke_sort_mode_change_change(string input)
                {
                    Debug.Log("_invoke_sort_mode_change()_str_=_" + input);
                    _sort_mode_change.Invoke(input);
                }
                public void _invoke_on_close_warehouse_UI()
                {
                    Debug.Log("_invoke_on_close_warehouse_UI()");
                    _on_close_warehouse_UI.Invoke();
                }
                public void _invoke_on_refresh_warehouse_UI()
                {
                    Debug.Log("_on_refresh_warehouse_UI()");
                    _on_refresh_warehouse_UI.Invoke();
                }
            }

        }
    }
}
