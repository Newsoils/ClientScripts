using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace CLIP
{
    namespace Project_Mouse
    {
        [System.Serializable]
        public class Planting_Configuration
        {
            public List<Fertilizer_Info> fertilizer_info_list;
            public List<flower_pot_info> flower_pot_info_list;
            public List<Plant_Info> plant_info_list;
            public planting_const _planting_const;

            public Fertilizer_Info find_fertilizer(string _ferilizer_name)
            {

                return fertilizer_info_list.Find((_f) =>
                {
                    return _f.fertilizer_name == _ferilizer_name;
                });
            }
            public Plant_Info find_plant_info_by_seed_name(string _seed_name)
            {

                return plant_info_list.Find((_p) =>
                {
                    return _p.seed_name == _seed_name;
                });
            }

            public flower_pot_info find_pot(string _pot_info)
            {
                return flower_pot_info_list.Find((_p) =>
                {
                    return _p.pot_name == _pot_info;
                });
            }

        }

    }

}
