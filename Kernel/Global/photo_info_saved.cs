using System;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            [System.Serializable]
            public class photo_info_saved
            {
                public int _photo_id;
                public string _photo_name;
                public string _photo_url;
                public string _photo_uploader;
                public DateTime _photo_upload_time;
                public string _photo_type;//normal,present 
                public string _photo_source;//dispatch,indoor,indoor_with_friend
                public string _photo_config_name = "";
                public bool is_local_only = true;
                public string _local_path = "";
            }

        }
    }
}