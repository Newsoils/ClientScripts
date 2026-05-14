using System.Collections.Generic;
using Newtonsoft.Json;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Kernel
        {
            namespace Social
            {
                [System.Serializable]
                public class Friend_Social_Record
                { 
                    public int hot_daily_count = 0;
                    public string friend_id = "Default_Friend_id";
                    public string friend_name = "玩家";
                    public List<achievement_record> achievements_obtained = new List<achievement_record>();
                    public Player_Social_Setting _brief_info=new Player_Social_Setting();
                    public Cloth_Suit _cloth_suit = new Cloth_Suit();
                  
                    [JsonIgnore]
                    public List<Social_Behavior> _behavior_record = new List<Social_Behavior>();
                    [JsonIgnore]
                    public List<Social_Chat_Msg_Record> _chat_msg = new List<Social_Chat_Msg_Record>();
                }
            }
        }

    }
}