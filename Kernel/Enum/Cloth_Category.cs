
namespace CLIP.Project_Mouse.ENUM
{
    /// <summary>
    /// 衣服一级分类
    /// </summary>
    public enum Cloth_First_Category
    {
        None = 0,
        /// <summary> 上衣 </summary>
        Top = 1,
        /// <summary> 下装 </summary>
        Bottom = 2,
        /// <summary> 连体 </summary>
        OnePiece = 3,
        /// <summary> 鞋袜 </summary>
        Footwear = 4,
        /// <summary> 配饰 </summary>
        Accessory = 5,
        /// <summary> 头饰 </summary>
        Headwear = 6,
        /// <summary> 套组 </summary>
        Set = 7
    }


    /// <summary>
    /// 衣服二级分类
    /// </summary>
    public enum Cloth_Second_Category
    {
      
        /// <summary> 无 </summary>
        None = 0,
        // --- 下装类 ---
        /// <summary> 裤子 </summary>
        Pants = 301,
        /// <summary> 半裙 </summary>
        Skirt = 302,

        // --- 配饰类 ---
        /// <summary> 项链 </summary>
        Necklace = 501,
        /// <summary> 背包 </summary>
        Backpack = 502,
        /// <summary> 特殊配饰 </summary>
        SpecialAcc = 503,

        // --- 头饰类 ---
        /// <summary> 帽子 </summary>
        Hat = 601,
        /// <summary> 眼镜 </summary>
        Glasses = 602,
        /// <summary> 特殊头饰 </summary>
        SpecialHead = 603
    }

}
