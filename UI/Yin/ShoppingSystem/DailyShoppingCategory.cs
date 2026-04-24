using System;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>日刊商店一级分类（与 UI 标签对应，逻辑用枚举而非中文字符串）。</summary>
    public enum DailyShoppingPrimaryCategory
    {
        None = 0,
        Clothes = 1,
        Furniture = 2,
        Planting = 3,
        Belonging = 4,
    }

    /// <summary>日刊商店二级分类（仅种植、携带物使用）。</summary>
    public enum DailyShoppingSecondaryCategory
    {
        None = 0,
        Pot = 1,
        Seed = 2,
        Fertilizer = 3,
        Food = 4,
        Snack = 5,
        Tape = 6,
    }

    /// <summary>日刊分类与 <see cref="Item_Type"/> 的映射与查询。</summary>
    public static class DailyShoppingCategory
    {
        private static readonly DailyShoppingSecondaryCategory[] PlantingSecondaries =
        {
            DailyShoppingSecondaryCategory.Pot,
            DailyShoppingSecondaryCategory.Seed,
            DailyShoppingSecondaryCategory.Fertilizer,
        };

        private static readonly DailyShoppingSecondaryCategory[] BelongingSecondaries =
        {
            DailyShoppingSecondaryCategory.Food,
            DailyShoppingSecondaryCategory.Snack,
            DailyShoppingSecondaryCategory.Tape,
        };

        public static IReadOnlyList<DailyShoppingSecondaryCategory> GetSecondaries(DailyShoppingPrimaryCategory primary)
        {
            switch (primary)
            {
                case DailyShoppingPrimaryCategory.Planting:
                    return PlantingSecondaries;
                case DailyShoppingPrimaryCategory.Belonging:
                    return BelongingSecondaries;
                default:
                    return Array.Empty<DailyShoppingSecondaryCategory>();
            }
        }

        public static bool LimitDailyListToFourSlots(DailyShoppingPrimaryCategory primary)
        {
            return primary == DailyShoppingPrimaryCategory.Clothes || primary == DailyShoppingPrimaryCategory.Furniture;
        }

        public static bool MatchesPrimary(Item_Type itemType, DailyShoppingPrimaryCategory primary)
        {
            switch (primary)
            {
                case DailyShoppingPrimaryCategory.Clothes:
                    return itemType == Item_Type.Cloth;
                case DailyShoppingPrimaryCategory.Furniture:
                    return itemType == Item_Type.Room_Placement;
                case DailyShoppingPrimaryCategory.Planting:
                    return itemType == Item_Type.Pot || itemType == Item_Type.Seed || itemType == Item_Type.Fertilizer;
                case DailyShoppingPrimaryCategory.Belonging:
                    return itemType == Item_Type.Food || itemType == Item_Type.Snack || itemType == Item_Type.Tape;
                default:
                    return false;
            }
        }

        public static bool MatchesSecondary(Item_Type itemType, DailyShoppingSecondaryCategory secondary)
        {
            switch (secondary)
            {
                case DailyShoppingSecondaryCategory.Pot:
                    return itemType == Item_Type.Pot;
                case DailyShoppingSecondaryCategory.Seed:
                    return itemType == Item_Type.Seed;
                case DailyShoppingSecondaryCategory.Fertilizer:
                    return itemType == Item_Type.Fertilizer;
                case DailyShoppingSecondaryCategory.Food:
                    return itemType == Item_Type.Food;
                case DailyShoppingSecondaryCategory.Snack:
                    return itemType == Item_Type.Snack;
                case DailyShoppingSecondaryCategory.Tape:
                    return itemType == Item_Type.Tape;
                default:
                    return false;
            }
        }
    }
}
