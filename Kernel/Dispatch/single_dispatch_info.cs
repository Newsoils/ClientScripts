using System.Collections.Generic;
using Newtonsoft.Json;
using System;

[System.Serializable]

public class single_dispatch_info 
{
    public string carried_food_name;
    public string carried_snack_name;
    public string carried_tape_name;
    public string go_to_map_name ;
    public DateTime start_time;
    public DateTime end_time;
    public int dispatch_length_in_minute=0;

    public bool can_get_middle_way_photo = false;
    public string middle_way_photo_name = "NULL";
    public string middle_way_message_name = "NULL";

    public int reward_currency;
    public int reward_exp;
    public string reward_photo_name;
    public List<string> reward_item_list=new List<string>();

    public string friend_event_name;
                   
    public single_dispatch_info()
    {
        go_to_map_name = "NULL_MAP";
        friend_event_name = "NULL_Friend_Event";
    }

    public string reward_to_str()
    {
        return "获得金币_=_" + reward_currency + "\n"
                    + "获得经验_=_" + reward_exp + "\n"
                    + "获得照片_=_" + reward_photo_name + "\n"
                    + "获得物品_=_" + JsonConvert.SerializeObject(reward_item_list) +"\n" 
                    +"触发伙伴事件=_" + friend_event_name;  
    }

    public string dispatch_info_to_str()
    {
        return "派遣目的地_=_" + go_to_map_name + "\n"
                + "派遣时长_=_" + dispatch_length_in_minute+"_分钟\n";
                              
    }
}

