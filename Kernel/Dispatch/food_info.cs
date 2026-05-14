namespace CLIP.Project_Mouse.Kernel.Dispatch
{

    [System.Serializable]
    public class Food_Info
    {
        /// <summary>
        /// 物品id
        /// </summary>
        public int item_id;
        /// <summary>
        /// 名称
        /// </summary>
        public string name;
        /// <summary>
        /// 一级图权重
        /// </summary>
        public int Lv_01_map_weight;
        /// <summary>
        /// 二级图权重
        /// </summary>
        public int Lv_02_map_weight;
        /// <summary>
        /// 三级图权重
        /// </summary>
        public int Lv_03_map_weight;
        /// <summary>
        /// （roll值单位分钟）下限时间
        /// </summary>
        public int min_travel_time;
        /// <summary>
        /// 上限时间
        /// </summary>
        public int max_travel_time;

        public string model_resource_name;

        public override string ToString()
        {
            return "{ "
            + "item_id:" + item_id + ","
            + "name:" + name + ","
            + "Lv_01_map_weight:" + Lv_01_map_weight + ","
            + "Lv_02_map_weight:" + Lv_02_map_weight + ","
            + "Lv_03_map_weight:" + Lv_03_map_weight + ","
            + "min_travel_time:" + min_travel_time + ","
            + "max_travel_time:" + max_travel_time + ","
            + "}";
        }
    }
}