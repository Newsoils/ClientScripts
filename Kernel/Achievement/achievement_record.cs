using System;

namespace CLIP.Project_Mouse.Kernel
{
    [Serializable]
    public class achievement_record
    {
        public int achievement_record_id = -1;
        public string user_name;
        public string achievement_name;
        public string achievement_type;
        public string current_state;
        public DateTime date_obtained;

        public achievement_record()
        {
        }
    }
}
