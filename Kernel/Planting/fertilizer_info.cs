using Newtonsoft.Json;
using CLIP.Project_Mouse.ENUM;


namespace CLIP
{
    namespace Project_Mouse
    {
           [System.Serializable]

public class Fertilizer_Info 
{
    // Start is called before the first frame update
  /// <summary>
    /// id
    /// </summary>
    public  int fertilizer_id;
    /// <summary>
    /// 名字
    /// </summary>
    public  string fertilizer_name;
    /// <summary>
    /// 一级分类
    /// </summary>
    public Plant_First_Category first_category;
    /// <summary>
    /// 二级分类
    /// </summary>
    public Plant_Second_Category second_category;
    /// <summary>
    /// 是否施肥后植物直接进入【成熟待收期】
    /// </summary>
    public  bool is_to_mature;
    /// <summary>
    /// 生长加速
    /// </summary>
    public  float speed_up_rate;
    /// <summary>
    /// 施肥后出现隐藏款植物的概率加值
    /// </summary>
    public  float hided_variant_plus;

      public override string ToString()
    {
        return "{ "
        + "fertilizer_id:" + fertilizer_id + ","
        + "fertilize_name:" + fertilizer_name + ","
        + "is_to_mature:" + is_to_mature + ","
        + "speed_up_rate:" + speed_up_rate + ","
        + "hided_variant_plus:" + hided_variant_plus + ","
        + "}";
    }
}
    }}