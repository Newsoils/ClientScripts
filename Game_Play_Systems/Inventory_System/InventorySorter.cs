using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.Kernel;
using Lean.Touch;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public enum InventorySortType
    {
        Rarity,      // 稀有度
        ObtainDate,  // 获取时间
        Count,       // 持有数量
        FavoriteDate,     // 收藏状态 

    }
    public static class InventorySorter
    {
        // 获取时间：降序
        public static int CompareByDate(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            int res = b._obtain_date.CompareTo(a._obtain_date);
            return res != 0 ? res : a.item_id.CompareTo(b.item_id);
        }

        // 售价：降序
        public static int CompareByPrice(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            int res = b.item_info.sell_price.CompareTo(a.item_info.sell_price);
            return res != 0 ? res : a.item_id.CompareTo(b.item_id);
        }

        // 数量：降序
        public static int CompareByCount(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            int res = b._item_count.CompareTo(a._item_count);
            return res != 0 ? res : a.item_id.CompareTo(b.item_id);
        }

        public static int CompareByRarity(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            int res = b.item_info.rarity.CompareTo(a.item_info.rarity);
            return res != 0 ? res : a.item_id.CompareTo(b.item_id);
        }

        public static List<Game_Item_In_Inventory> SortByRarity(this List<Game_Item_In_Inventory> list, bool isAscending = false)
        {
            SortList(list,InventorySortType.Rarity,isAscending);
            return list;
        }

        public static List<Game_Item_In_Inventory> SortByDate(this List<Game_Item_In_Inventory> list, bool isAscending = false)
        {
            SortList(list,InventorySortType.ObtainDate,isAscending);
            return list;
        }

        public static List<Game_Item_In_Inventory> SortByCount(this List<Game_Item_In_Inventory> list, bool isAscending = false)
        {
            SortList(list,InventorySortType.Count,isAscending);
            return list;
        }

        public static List<Game_Item_In_Inventory> SortByFavoriteDate(this List<Game_Item_In_Inventory> list, bool isAscending = false)
        {
            SortList(list,InventorySortType.FavoriteDate,isAscending);
            return list;
        }

        public static List<Game_Item_In_Inventory> SortByType(this List<Game_Item_In_Inventory> list, InventorySortType sortType, bool isAscending = false)
        {
            SortList(list,sortType,isAscending);
            return list;
        }

        /// <summary>
        /// 根据名称改变返回值（注意，没有改变原数组）
        /// </summary>
        /// <param name="list"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<Game_Item_In_Inventory> SelectByName(this List<Game_Item_In_Inventory> list, string str)
        {
            return list.Where(i => i.item_name.Contains(str)).ToList();
        }

        /// <summary>
        /// 对给定的列表进行排序
        /// </summary>
        /// <param name="list">要排序的列表</param>
        /// <param name="sortType">排序主类型</param>
        /// <param name="isAscending">是否为正序（从小到大/从旧到新/从少到多）</param>
        public static void SortList(List<Game_Item_In_Inventory> list, InventorySortType sortType, bool isAscending)
        {
            if (list == null || list.Count <= 1) return;

            list.Sort((a, b) =>
            {
                int result = 0;
                switch (sortType)
                {
                    case InventorySortType.Rarity:
                        result = CompareByRarityHybrid(a, b);
                        break;
                    case InventorySortType.ObtainDate:
                        result = CompareByDateHybrid(a, b);
                        break;
                    case InventorySortType.Count:
                        result = CompareByCountHybrid(a, b);
                        break;
                    case InventorySortType.FavoriteDate:
                        result = CompareByFavoriteDateHybrid(a, b);
                        break;
                    default:
                        result = CompareByDateHybrid(a, b); // 默认按时间
                        break;
                }

                // 如果是正序，保持原结果；如果是反序（通常游戏里是降序），则取反
                // 注意：下面的Hybrid方法默认是按照“降序”（高->低）逻辑写的
                // 所以：如果想要正序 (Low->High)，需要取反结果？
                // 让我们统一约定：Hybrid方法返回 1 代表 a > b (a排在b前，即降序)。
                // List.Sort默认是升序 (-1: a<b, a在前)。

                // 下面的Hybrid逻辑我将按照 C# CompareTo 标准：
                // b.CompareTo(a) 代表 降序 (大的在前)

                return isAscending ? -result : result;
            });
        }

        // ---------------- 混合比较逻辑 (默认返回降序结果: >0 代表 a 优于 b) ----------------

        // 逻辑：稀有度 > 获取时间 > 持有数量
        private static int CompareByRarityHybrid(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            // 1. 稀有度 (Desc)
            int res = b.item_info.rarity.CompareTo(a.item_info.rarity);
            if (res != 0) return res;

            // 2. 获取时间 (Desc: 新的在前)
            res = b._obtain_date.CompareTo(a._obtain_date);
            if (res != 0) return res;

            // 3. 持有数量 (Desc)
            res = b._item_count.CompareTo(a._item_count);
            if (res != 0) return res;

            // 4. ID兜底 (Asc: ID小的在前，保证稳定性)
            return a.item_id.CompareTo(b.item_id);
        }

        // 逻辑：获取时间 > 稀有度 > 持有数量
        private static int CompareByDateHybrid(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            // 1. 获取时间 (Desc)
            int res = b._obtain_date.CompareTo(a._obtain_date);
            if (res != 0) return res;

            // 2. 稀有度 (Desc)
            res = b.item_info.rarity.CompareTo(a.item_info.rarity);
            if (res != 0) return res;

            // 3. 持有数量 (Desc)
            res = b._item_count.CompareTo(a._item_count);
            if (res != 0) return res;

            return a.item_id.CompareTo(b.item_id);
        }

        // 逻辑：持有数量 > 稀有度 > 获取时间
        private static int CompareByCountHybrid(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            // 1. 持有数量 (Desc)
            int res = b._item_count.CompareTo(a._item_count);
            if (res != 0) return res;

            // 2. 稀有度 (Desc)
            res = b.item_info.rarity.CompareTo(a.item_info.rarity);
            if (res != 0) return res;

            // 3. 获取时间 (Desc)
            res = b._obtain_date.CompareTo(a._obtain_date);
            if (res != 0) return res;

            return a.item_id.CompareTo(b.item_id);
        }

        // 逻辑：收藏(date) > 稀有度 > 获取时间
        private static int CompareByFavoriteDateHybrid(Game_Item_In_Inventory a, Game_Item_In_Inventory b)
        {
            // 1. 收藏 日期
            int res = b.favorite_time.CompareTo(a.favorite_time);
            if (res != 0) return res;

            // 2. 稀有度 (Desc)
            res = b.item_info.rarity.CompareTo(a.item_info.rarity);
            if (res != 0) return res;

            // 3. 获取时间 (Desc)
            res = b._obtain_date.CompareTo(a._obtain_date);
            if (res != 0) return res;

            return a.item_id.CompareTo(b.item_id);
        }
    }
}
   