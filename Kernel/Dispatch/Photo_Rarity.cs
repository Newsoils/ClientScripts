

namespace CLIP.Project_Mouse.Kernel.Dispatch
{
    [System.Serializable]
    public class Photo_Rarity
    {
        /// <summary>
        /// 稀有度ID
        /// </summary>
        public int id;
        /// <summary>
        /// 稀有度名称
        /// </summary>
        public string photo_Rarity_Name;
        public override string ToString()
        {
            return "{ "
            + "rarity_name:" + photo_Rarity_Name + ","
            + "}";
        }
    }
}