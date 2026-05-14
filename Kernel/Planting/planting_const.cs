using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using CLIP.Project_Mouse.ENUM;


namespace CLIP
{
    namespace Project_Mouse
    {
           [System.Serializable]
public class planting_const
{
 
  /// <summary>
    /// id
    /// </summary>
    public  int id;
    /// <summary>
    /// 花盆的湿润度会每分钟下降
    /// </summary>
    public  float pot_wet_decrease_per_minute;
            /// <summary>
            /// 花盆空置多少分钟会产生杂草
            /// </summary>
            public   int weed_time_in_minute;

            /// <summary>
            /// 杂草资源路径
            /// </summary>
            public string weed_res_path;


            /// <summary>
            /// 缺水时植物生长速度倍率
            /// </summary>
            public float no_water_growing_rate=0.7f;
            public override string ToString()
    {
        return "{ "
        + "id:" + id + ","
        + "pot_wet_decrease_per_minute:" + pot_wet_decrease_per_minute + ","

          + "weed_res_path:" + weed_res_path + ","
          + "no_water_growing_rate:" + no_water_growing_rate + ","
        + "}";
    }
}
}
}
