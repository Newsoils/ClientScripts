using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;


namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "Cloth_DB_SO", menuName = "Project_Mouse/Cloth_DB_SO")]
    public class Cloth_DB_SO : ScriptableObject
    {
        public Cloth_Default_Config _cloth_config = new Cloth_Default_Config();

        public Cloth_DB _cloth_db = new Cloth_DB();

        public void OnEnable()
        {
            RefreshData();
        }

        public void OnValidate()
        {
            RefreshData();
        }

        public TextAsset _db_json_file;


        [ContextMenu("加载数据")]
        public void RefreshData()
        {
            _cloth_db._all_cloth_list.Clear();
            TextAsset json_file = _db_json_file;
            if (_db_json_file != null) _cloth_db._all_cloth_list = JsonConvert.DeserializeObject<List<cloth_info>>(_db_json_file.text);
        }
    }


}