using System.Collections.Generic;
using UnityEngine;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Inventory;


namespace CLIP.Project_Mouse.Game_Play_System
{

    [CreateAssetMenu(fileName = "Shop_List_DB_SO", menuName = "Project_Mouse/Inventory/Shop_List_DB_SO")]
    public class Shop_List_DB_SO : ScriptableObject
    {
        public shop_list_db _db;
        public int refreshCount;
        public List<shop_item> _daily_shop_list;
        public List<item_group> _limited_cloth_item_list;
        public List<item_group> _limited_placement_item_list;

        public Cloth_DB_SO _cloth_info_db_SO;
        [Header("Json")]
        public TextAsset _shop_list_json_file;
        public TextAsset _item_group_json_file;
        [Header("For_Editor_Only")]
        public string _buy_item_name;

        private void OnEnable()
        {
            load_from_json();
            get_daily_shop_list();
            get_limited_shop_list();
        }
        public void load_from_json()
        {
            if (_db == null) _db = new shop_list_db();
            if (_shop_list_json_file == null) return;
            if (_item_group_json_file == null) return;
            string shop_list_json = _shop_list_json_file != null ? _shop_list_json_file.text : null;
            string item_group_json = _item_group_json_file != null ? _item_group_json_file.text : null;
            _db.load_json(shop_list_json, item_group_json);
        }

        public void get_daily_shop_list()
        {
            if (_db == null) return;
            _db.get_daily_shop_list(_cloth_info_db_SO._cloth_db, out _daily_shop_list);
        }
        public void get_limited_shop_list()
        {
            if (_db == null) return;
            _db.get_limited_shop_list(out _limited_cloth_item_list, out _limited_placement_item_list);
        }
        
    }

}