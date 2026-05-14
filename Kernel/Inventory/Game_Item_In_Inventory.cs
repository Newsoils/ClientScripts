using System;
using System.Collections;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.Kernel
{

    [System.Serializable]
    public class Game_Item_In_Inventory
    {
        // 由于Game_Item_Info _item_info是json ignore的（读表获得即可），
        // 因此需要这里再写一个Item_Id，Item_Name用于和服务器同步（会上传到数据库）
        public string item_name;

        public int item_id;

        [JsonIgnore]
        public Game_Item_Info item_info;

        public DateTime _obtain_date;

        public int _item_count;

        /// <summary>
        /// 是否加入喜爱列表清单
        /// </summary>
        public bool is_favorite = false;

        /// <summary>
        /// 收藏（喜爱）日期
        /// </summary>
        public DateTime favorite_time;
    }
}
