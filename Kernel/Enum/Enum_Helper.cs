using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;

namespace CLIP.Project_Mouse.ENUM
{
    public static class Enum_Helper
    {
        // 衣服的关系映射
        public static readonly Dictionary<Cloth_First_Category, List<Cloth_Second_Category>> ClothMap = new Dictionary<Cloth_First_Category, List<Cloth_Second_Category>>
    {
        { Cloth_First_Category.Bottom, new List<Cloth_Second_Category> { Cloth_Second_Category.Pants, Cloth_Second_Category.Skirt } },
        { Cloth_First_Category.Accessory, new List<Cloth_Second_Category> { Cloth_Second_Category.Necklace, Cloth_Second_Category.Backpack, Cloth_Second_Category.SpecialAcc } },
        { Cloth_First_Category.Headwear, new List<Cloth_Second_Category> { Cloth_Second_Category.Hat, Cloth_Second_Category.Glasses, Cloth_Second_Category.SpecialHead } }
    };

        // 种植的关系映射
        public static readonly Dictionary<Plant_First_Category, List<Plant_Second_Category>> PlantMap = new Dictionary<Plant_First_Category, List<Plant_Second_Category>>
    {
        { Plant_First_Category.Pot, new List<Plant_Second_Category> { Plant_Second_Category.BigPot, Plant_Second_Category.MediumPot, Plant_Second_Category.SmallPot, Plant_Second_Category.HangingPot } },
        { Plant_First_Category.Seed, new List<Plant_Second_Category> { Plant_Second_Category.NormalPlant, Plant_Second_Category.VinePlant } },
        { Plant_First_Category.Fertilizer, new List<Plant_Second_Category> { Plant_Second_Category.GrowthFert, Plant_Second_Category.OrganicFert } }
    };

        // 回收箱的关系映射
        public static readonly Dictionary<Recycle_First_Category, List<Recycle_Second_Category>> RecycleMap = new Dictionary<Recycle_First_Category, List<Recycle_Second_Category>>
    {
        { Recycle_First_Category.Crop, new List<Recycle_Second_Category> { Recycle_Second_Category.NormalPlant, Recycle_Second_Category.VinePlant } }
    };

        private static readonly Dictionary<Placement_First_Category, List<Placement_Second_Category>> PlacementMap =
            new Dictionary<Placement_First_Category, List<Placement_Second_Category>>
        {
        {
            Placement_First_Category.Bathroom,
            new List<Placement_Second_Category> { Placement_Second_Category.Shower, Placement_Second_Category.Sink, Placement_Second_Category.Toilet }
        },
        {
            Placement_First_Category.Decoration,
            new List<Placement_Second_Category> { Placement_Second_Category.Hanging, Placement_Second_Category.Ornament }
        },
        {
            Placement_First_Category.Lighting,
            new List<Placement_Second_Category> { Placement_Second_Category.WallLamp, Placement_Second_Category.DeskLamp, Placement_Second_Category.Chandelier, Placement_Second_Category.Groundlamp }
        }
        };

        /// <summary>
        /// 根据一级菜单获取所有关联的二级菜单
        /// </summary>
        public static List<Placement_Second_Category> GetSubCategories(Placement_First_Category primary)
        {
            return PlacementMap.ContainsKey(primary) ? PlacementMap[primary] : new List<Placement_Second_Category>();
        }
        public static List<Cloth_Second_Category> GetSubCategories(Cloth_First_Category primary)

        {
            return ClothMap.ContainsKey(primary) ? ClothMap[primary] : new List<Cloth_Second_Category>();
        }
        public static List<Plant_Second_Category> GetSubCategories(Plant_First_Category primary)
        {
            return PlantMap.ContainsKey(primary) ? PlantMap[primary] : new List<Plant_Second_Category>();
        }
        public static List<Recycle_Second_Category> GetSubCategories(Recycle_First_Category primary)
        {
            return RecycleMap.ContainsKey(primary) ? RecycleMap[primary] : new List<Recycle_Second_Category>();
        }


        /// <summary>
        /// 通过摆放方式确定摆放的具体位置
        /// </summary>
        public static readonly Dictionary<Room_Placing_Type,List<GridLayerType>> GridLayerMap = 
            new Dictionary<Room_Placing_Type, List<GridLayerType>>
            {
                { Room_Placing_Type.Floor_Furniture ,new List<GridLayerType>{ GridLayerType.Floor } },
                { Room_Placing_Type.Wall_Furniture ,new List<GridLayerType>{ GridLayerType.Wall_N, GridLayerType.Wall_S, GridLayerType.Wall_W, GridLayerType.Wall_E } },
                { Room_Placing_Type.Ceiling_Furniture ,new List<GridLayerType>{ GridLayerType.Ceiling } },
                { Room_Placing_Type.Surface_Furniture, new List<GridLayerType>{ GridLayerType.Surface, GridLayerType.Floor} }
            };
    }
}
