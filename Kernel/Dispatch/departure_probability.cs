

namespace CLIP.Project_Mouse.Kernel.Dispatch
{

    [System.Serializable]
    public class Departure_Probability
    {

        /// <summary>
        /// id
        /// </summary>
        public int id;
        /// <summary>
        /// 在家时长下限（分钟）
        /// </summary>
        public int min_time;
        /// <summary>
        /// 在家时长上限（分钟）
        /// </summary>
        public int max_time;
        /// <summary>
        /// 每分钟出发概率
        /// </summary>
        public float probability;


        public override string ToString()
        {
            return "{ "
            + "id:" + id + ","
            + "min_time:" + min_time + ","
            + "max_time:" + max_time + ","
            + "probability:" + probability + ","
            + "}";
        }
    }


}
