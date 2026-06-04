using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// 家具/摆放物<strong>静态配置</strong>（表数据，与房间内存档实例 <see cref="PlacementData"/> 区分）。
    /// </summary>
    [System.Serializable]
    public class Room_Placement_Info
    {
        /// <summary>
        /// 与game_item.xlsx 一一对应
        /// </summary>
        public string room_placement_name;
        public int room_placement_id;
        public List<RoomType> relating_rooms;
        /// <summary>
        /// 描述
        /// </summary>
        public string desc;
        /// <summary>
        /// 放置方法类型
        /// </summary>
        public Room_Placing_Type placing_type;
        /// <summary>
        /// 放置物类型(暂时用string)
        /// </summary>
        public Placement_First_Category first_Category;
        public Placement_Second_Category second_Category;
        public int length;
        public int height;
        public int width;
        public string res_url;
        public string obtain_source;
        /// <summary>
        /// 是否可以摆放到其他物品上
        /// </summary>
        public bool whether_put;
        /// <summary>
        /// 是否可以被摆放物品
        /// </summary>
        public bool whether_be_placed;


        public override string ToString()
        {
            return "{ "
            + "room_placement_id:" + room_placement_id + ","
            + "room_placement_name:" + room_placement_name + ","
            + "relating_rooms:" + JsonConvert.SerializeObject(relating_rooms) + ","
            + "desc:" + desc + ","
            + "placing_type:" + placing_type + ","
            + "first_Category:" + first_Category + ","
            + "length:" + length + ","
            + "width:" + width + ","
            + "res_url:" + res_url + ","
            + "obtain_source:" + obtain_source + ","
             + "whether_put:" + whether_put + ","
            + "whether_be_placed:" + whether_be_placed + ","
            + "}";
        }
    }
}