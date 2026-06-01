using System;

namespace CLIP.Project_Mouse.Kernel.Achievement
{
    [Serializable]
    public class achievement_design_info
    {
        public int achievement_id;
        public string achievement_name;
        public string achievement_type;
        public string achievement_desc;
        public string achievement_unlock_condition;
        public string achievement_unlock_condition_SQL;
        public string achievement_rank;
        public string affinity_reaward_target;
        public int affinity_reaward_value;
        public string achievement_icon_res_url;

        public override string ToString()
        {
            return "{ "
                + "achievement_id:" + achievement_id + ","
                + "achievement_name:" + achievement_name + ","
                + "achievement_type:" + achievement_type + ","
                + "achievement_desc:" + achievement_desc + ","
                + "achievement_unlock_condition:" + achievement_unlock_condition + ","
                + "achievement_unlock_condition_SQL:" + achievement_unlock_condition_SQL + ","
                + "achievement_rank:" + achievement_rank + ","
                + "affinity_reaward_target:" + affinity_reaward_target + ","
                + "affinity_reaward_value:" + affinity_reaward_value + ","
                + "achievement_icon_res_url:" + achievement_icon_res_url + ","
                + "}";
        }
    }
}
