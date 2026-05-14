namespace CLIP.Project_Mouse.Kernel.Dispatch
{
    [System.Serializable]
    public class Snack_Info
    {
        /// <summary>
        /// id
        /// </summary>
        public int item_id;
        /// <summary>
        /// 名字
        /// </summary>
        public string snack_name;
        /// <summary>
        /// 获取照片luck加成
        /// </summary>
        public float photo_luck_plus;
        /// <summary>
        /// 额外物品概率加成
        /// </summary>
        public float more_item_plus_value;

        public string model_resource_name;

        public override string ToString()
        {
            return "{ "
            + "item_id:" + item_id + ","
            + "snack_name:" + snack_name + ","
            + "photo_luck_plus:" + photo_luck_plus + ","
            + "more_item_plus_value:" + more_item_plus_value + ","
            + "}";
        }
    }
}