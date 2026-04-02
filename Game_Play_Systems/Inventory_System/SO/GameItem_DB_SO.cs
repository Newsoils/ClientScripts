using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "GameItem_DB_SO", menuName = "Project_Mouse/GameItem_DB_SO")]
    public class GameItem_DB_SO : ScriptableObject
    {
        [SerializeField]
        public List<Game_Item_Info> _gameItem_db = new List<Game_Item_Info>();

        [Header("GameItem_DB数据Json文件")]
        public TextAsset _game_item_json_file;

        public readonly Dictionary<int, Game_Item_Info> _idDic = new Dictionary<int, Game_Item_Info>();
        public readonly Dictionary<string, Game_Item_Info> _nameDic = new Dictionary<string, Game_Item_Info>();

        public Game_Item_Info GetItemInfo(string itemName)
        {
            _nameDic.TryGetValue(itemName, out var info);
            return info;
        }
        public Game_Item_Info GetItemInfo(int itemId)
        {
            _idDic.TryGetValue(itemId, out var info);
            return info;
        }

        public List<Game_Item_Info> GetItemInfos(IEnumerable<int> ids)
        {
            List<Game_Item_Info> result = new List<Game_Item_Info>();
            foreach (var id in ids)
            {
                _idDic.TryGetValue(id, out var info);
                if (info != null) result.Add(info);
            }
            return result;
        }
        public List<Game_Item_Info> GetItemInfos(IEnumerable<string> names)
        {
            List<Game_Item_Info> result = new List<Game_Item_Info>();
            foreach (var name in names)
            {
                _nameDic.TryGetValue(name, out var info);
                if (info != null) result.Add(info);
            }
            return result;
        }
        private void OnEnable()
        {
            RefreshData();
        }
        private void OnValidate()
        {
            RefreshData();
        }



        [ContextMenu("加载数据")]
        public void RefreshData()
        {
            _gameItem_db = JsonConvert.DeserializeObject<List<Game_Item_Info>>(_game_item_json_file.text);

            foreach (var item_info in _gameItem_db)
            {
                _idDic[item_info.item_id] = item_info;
                _nameDic[item_info.name] = item_info;
            }
        }


    }

}