using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


namespace CLIP.Project_Mouse.Game_Play_System.Planting_System
{

    [CreateAssetMenu(fileName = "planting_configuration_SO", menuName = "Project_Mouse/planting_configuration_SO")]
    public class planting_configuration_SO : ScriptableObject
    {

        public Planting_Configuration _planting_config = new Planting_Configuration();

        [Header("JSON_File")]

        public TextAsset pot_info_json_file;
        public TextAsset plant_info_json_file;
        public TextAsset fertilizer_info_json_file;
        public TextAsset _planting_const_json_file;


        private void OnEnable()
        {
            RefrehData();
        }

        [ContextMenu("加载数据")]
        public void RefrehData()
        {
            if (pot_info_json_file != null)
                _planting_config.flower_pot_info_list = JsonConvert.DeserializeObject<List<flower_pot_info>>(pot_info_json_file.text);
            if (plant_info_json_file != null)
                _planting_config.plant_info_list = JsonConvert.DeserializeObject<List<Plant_Info>>(plant_info_json_file.text);
            if (fertilizer_info_json_file != null)
                _planting_config.fertilizer_info_list = JsonConvert.DeserializeObject<List<Fertilizer_Info>>(fertilizer_info_json_file.text);

            if (_planting_const_json_file != null)
            {
                var temp_list = JsonConvert.DeserializeObject<List<planting_const>>(_planting_const_json_file.text);
                if (temp_list.Count != 0)
                {
                    _planting_config._planting_const = temp_list[0];
                }
            }
        }


    }

}