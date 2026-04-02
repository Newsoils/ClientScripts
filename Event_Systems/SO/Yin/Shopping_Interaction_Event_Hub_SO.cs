using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Client_Event_Systems
        {
            [CreateAssetMenu(fileName = "Shopping_Interaction_Event_Hub_SO", menuName = "Project_Mouse/Client_Event_Systems/Shopping_Interaction_Event_Hub_SO")]
            public class Shopping_Interaction_Event_Hub_SO : ScriptableObject
            {
                public UnityEvent<string> _on_selected_item_change = new UnityEvent<string>();
                public UnityEvent<string> _on_primary_category_change = new UnityEvent<string>();
                public UnityEvent<string> _on_secondary_category_change = new UnityEvent<string>();
                public UnityEvent _on_close_shop_UI = new UnityEvent();
                public UnityEvent _on_refresh_shop_UI = new UnityEvent();

                public void _invoke_on_selected_item_change(string itemName)
                {
                    Debug.Log($"InvokeSelectedItemChange: {itemName}");
                    _on_selected_item_change.Invoke(itemName);
                }
                public void _invoke_on_primary_category_change(string category)
                {
                    Debug.Log($"InvokePrimaryCategoryChange: {category}");
                    _on_primary_category_change.Invoke(category);
                }

                public void _invoke_on_secondary_category_change(string category)
                {
                    Debug.Log($"InvokeSecondaryCategoryChange: {category}");
                    _on_secondary_category_change.Invoke(category);
                }

                public void _invoke_on_close_shop_UI()
                {
                    Debug.Log("InvokeCloseShopUI");
                    _on_close_shop_UI.Invoke();
                }
                public void _invoke_on_refresh_shop_UI()
                {
                    Debug.Log("InvokeRefreshShopUI");
                    _on_refresh_shop_UI.Invoke();
                }
            }
        }
    }
}
           