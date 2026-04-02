using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ClothWarehouseSortModeUnit : MonoBehaviour
            {
                public string sortMode;
                public GameObject isSelected;
                public ClothWarehouseInteractManager parentControl;

                public void SelectSortMode()
                {
                    if (parentControl.currentSortMode != null)
                    {
                        parentControl.currentSortMode.isSelected.SetActive(false);
                    }

                    parentControl.currentSortMode = this;
                    parentControl._sort_mode = sortMode;
                    isSelected.SetActive(true);
                    Debug.Log("Search Mode Changed: " + sortMode);

                    parentControl._warehouse_ui_event_hub._invoke_sort_mode_change_change(sortMode);

                    parentControl.refresh_filtering_and_sorting();
                    parentControl.normalSortMode.SetActive(false);
                }
            }
        }
    }
}

