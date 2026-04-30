using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System.Dispatch_System
{
    /// <summary>
    /// 派遣选包界面：仓库格子的「占用 / 使用中 / 可派遣」仅影响显示与点击，不扣真实库存。
    /// </summary>
    public enum DispatchWarehouseCellState
    {
        Normal,
        OccupiedByCurrentBag,
        InUseElsewhere
    }

    public static class DispatchWarehouseVisual
    {
        public static int CountBagsUsingItem(Item_Type slotType, string itemName, IReadOnlyList<DispatchBagInfo> bags)
        {
            if (string.IsNullOrEmpty(itemName) || bags == null)
            {
                return 0;
            }

            int n = 0;
            for (int i = 0; i < bags.Count; i++)
            {
                var b = bags[i];
                if (b == null)
                {
                    continue;
                }

                if (slotType == Item_Type.Food && b.foodName == itemName)
                {
                    n++;
                }
                else if (slotType == Item_Type.Snack && b.snackName == itemName)
                {
                    n++;
                }
            }

            return n;
        }

        public static bool CurrentBagUsesItem(Item_Type slotType, string itemName, DispatchBagInfo curBag)
        {
            if (curBag == null || string.IsNullOrEmpty(itemName))
            {
                return false;
            }

            if (slotType == Item_Type.Food)
            {
                return curBag.foodName == itemName;
            }

            if (slotType == Item_Type.Snack)
            {
                return curBag.snackName == itemName;
            }

            return false;
        }

        /// <summary>
        /// 列表上展示的数量：真实持有 − 已被各背包槽位占用的份数（仅显示，不写库存）。
        /// </summary>
        public static int GetDisplayCount(int realInventoryCount, Item_Type slotType, string itemName,
            IReadOnlyList<DispatchBagInfo> bags)
        {
            int committed = CountBagsUsingItem(slotType, itemName, bags);
            return Mathf.Max(0, realInventoryCount - committed);
        }

        public static DispatchWarehouseCellState GetCellState(int realInventoryCount, Item_Type slotType, string itemName,
            DispatchBagInfo currentBag, IReadOnlyList<DispatchBagInfo> allBags)
        {
            if (CurrentBagUsesItem(slotType, itemName, currentBag))
            {
                return DispatchWarehouseCellState.OccupiedByCurrentBag;
            }

            int display = GetDisplayCount(realInventoryCount, slotType, itemName, allBags);
            if (display <= 0)
            {
                return DispatchWarehouseCellState.InUseElsewhere;
            }

            return DispatchWarehouseCellState.Normal;
        }
    }
}
