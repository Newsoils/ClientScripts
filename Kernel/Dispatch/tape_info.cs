using CLIP.Project_Mouse.ENUM;
using System.Collections.Generic;

namespace CLIP.Project_Mouse
{
    [System.Serializable]
    public class Tape_Info
    {
        /// <summary>
        /// id
        /// </summary>
        public int item_id;
        /// <summary>
        /// 物品名
        /// </summary>
        public string tape_name;
        /// <summary>
        /// 心情tag列表
        /// </summary>
        public List<Mood_Tag> mood_tag_list;

        public string mat_resource_name;

        public string wav_resource_name;

        public override string ToString()
        {
            return "{ "
            + "item_id:" + item_id + ","
            + "tape_name:" + tape_name + ","
            + "mood_tag_list:" + mood_tag_list.ToString() + ","
            + "}";
        }
    }
}