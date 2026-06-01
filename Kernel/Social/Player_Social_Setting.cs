using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {

            namespace Social {
                [System.Serializable]
                public class Player_Social_Setting
                {
                    public string _player_nick_name = "玩家";
                    public string _main_character_name= "小苔";
                    public int _affinity_with_main_character = 0;
                    public List<achievement_record> achievements_pinned = new List<achievement_record>();
                    public List<PhotoRecordInfo> _brief_photo_selected = new List<PhotoRecordInfo>();
                    [JsonProperty(NullValueHandling=NullValueHandling.Ignore,DefaultValueHandling =DefaultValueHandling.Ignore)]
                    public avatar_icon_info _icon_info = new avatar_icon_info();
                    public avatar_icon_info _icon_frame_info = new avatar_icon_info();
                }



            }
     

        }
    }
    }
 