using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI

        {

            public static class UI_Helper
            {
                public static void add_option_to_dropdown(TMP_Dropdown dropdown, string option)
                {
                    if (dropdown == null || string.IsNullOrEmpty(option))
                        return;
                    var _option = dropdown.options.Find(_o => _o.text == option);
                    if (_option != null) return;
                    TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(option);
                    dropdown.options.Add(newOption);
                    dropdown.RefreshShownValue();
                }
                public static void remove_option_to_dropdown(TMP_Dropdown dropdown, string option)
                {
                    if (dropdown == null || string.IsNullOrEmpty(option))
                        return;
                    int index = dropdown.options.FindIndex(opt => opt.text == option);
                    if (index >= 0)
                    {
                        dropdown.options.RemoveAt(index);
                        dropdown.RefreshShownValue();
                    }
                }
                public static void set_dropdown(TMP_Dropdown dropdown, string option_string)
                {
                    if (dropdown == null || string.IsNullOrEmpty(option_string))
                        return;

                    int index = dropdown.options.FindIndex(opt => opt.text == option_string);
                    if (index >= 0)
                    {
                        dropdown.value = index;
                        dropdown.RefreshShownValue();
                    }
                }
            }
        }
    }
}