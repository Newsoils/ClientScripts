using System.Collections;
using System.Collections.Generic;

namespace CLIP.Project_Mouse.Kernel.Dispatch
{
    [System.Serializable]
    public class Dispatch_Configuration
    {
        public List<Tape_Info> tape_info_list = new List<Tape_Info>();
        public List<Food_Info> food_info_list = new List<Food_Info>();
        public List<Snack_Info> snack_info_list = new List<Snack_Info>();
        public List<Map_Info> map_info_list = new List<Map_Info>();

        public List<Map_Reward_Pool> map_reward_pool_list = new List<Map_Reward_Pool>();

        public List<Photo_Info> photo_info_list = new List<Photo_Info>();

        public List<Departure_Probability> departure_probability_list = new List<Departure_Probability>();

        public List<Photo_Rarity> photo_rarity_list = new List<Photo_Rarity>();

        public Dictionary<int, string> photo_rarity_Dic = new ();


        public Dictionary<int,int> photo_map_index_Dic = new ();
    }

}
