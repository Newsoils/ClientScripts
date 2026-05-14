using System;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel.Inventory;

namespace CLIP.Project_Mouse.Kernel
{
    [Serializable]
    public class shop_state
    {
        public DateTime _last_time_update_daily=DateTime.MinValue;
        public DateTime _last_time_update_limited = DateTime.MinValue;
        public List<shop_item> _daily_shop_list;
        public List<item_group> _limited_cloth_item_list;
        public List<item_group> _limited_placement_item_list;
    }
}
