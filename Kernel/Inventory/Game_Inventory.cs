using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CLIP.Project_Mouse.ENUM;
using Newtonsoft.Json;


namespace CLIP.Project_Mouse.Kernel
{
    /// <summary>
    /// Inventory(实时数据)
    /// 本项目中暂时只有一个
    /// 提供增删改查,但是尽量在Global_Inventory_Manager 中调用方法
    /// </summary>
    [System.Serializable]
    public class Game_Inventory
    {
        public List<Game_Item_In_Inventory> _current_inventory = new List<Game_Item_In_Inventory>();

        [JsonIgnore]
        public IReadOnlyList<Game_Item_In_Inventory> Items => _current_inventory;

        [JsonIgnore]
        private readonly Dictionary<int, Game_Item_In_Inventory> _idDic = new Dictionary<int, Game_Item_In_Inventory>();
        [JsonIgnore]
        private readonly Dictionary<string, Game_Item_In_Inventory> _nameDic = new Dictionary<string, Game_Item_In_Inventory>();


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]

        public List<string> _new_obtained_item_names = new List<string>();

        #region 查询
        public Game_Item_In_Inventory Get_Item(string name)
        {
            _nameDic.TryGetValue(name, out var game_Item);
            return game_Item;
        }
        public Game_Item_In_Inventory Get_Item(int id)
        {
            _idDic.TryGetValue(id, out var game_Item);
            return game_Item;
        }

        public List<Game_Item_In_Inventory> GetItems(List<Game_Item_Info> _list)
        {
            List<Game_Item_In_Inventory> result = new List<Game_Item_In_Inventory>();
            foreach (var item in _list)
            {
                _idDic.TryGetValue(item.item_id, out var game_Item);
                if (game_Item != null) { result.Add(game_Item); }
            }
            return result;
        }

        public List<Game_Item_In_Inventory> GetItems(List<int> ids)
        {
            List<Game_Item_In_Inventory> result = new List<Game_Item_In_Inventory>();
            foreach (var id in ids)
            {
                _idDic.TryGetValue(id, out var game_Item);
                if (game_Item != null) { result.Add(game_Item); }
            }
            return result;
        }

        public List<Game_Item_In_Inventory> GetItems(List<string> names)
        {
            List<Game_Item_In_Inventory> result = new List<Game_Item_In_Inventory>();
            foreach (var name in names)
            {
                _nameDic.TryGetValue(name, out var game_Item);
                if (game_Item != null) { result.Add(game_Item); }
            }
            return result;
        }

        public int Get_Item_Count(string name)
        {
            _nameDic.TryGetValue(name, out var game_Item);
            return game_Item != null ? game_Item._item_count : 0;
        }

        public int Get_Item_Count(int id)
        {
            _idDic.TryGetValue(id, out var game_Item);
            return game_Item != null ? game_Item._item_count : 0;
        }

        public List<Game_Item_In_Inventory> Get_Items_By_Type(Item_Type type)
        {
            return _current_inventory.Where(i => i.item_info?.type == type).ToList();
        }
        #endregion

        #region 修改

        public bool Change_Item_Count(Game_Item_Info _item_info, int count, List<Game_Item_Info> db)
        {
            if (db == null || _item_info == null || count == 0) return false;
            //校验,如果不含这个物品,直接返回
            if (!db.Contains(_item_info)) return false;

            //当前物品在仓库里不存在时，尝试从数据库中查找，并添加到仓库中
            var _item = _current_inventory.FirstOrDefault(item => item.item_id == _item_info.item_id);
            if (_item == null)
            {
                if (count < 0) return false;
                else
                {
                    var newItem = new Game_Item_In_Inventory();
                    newItem.item_info = _item_info;

                    newItem.is_favorite = false;

                    newItem._obtain_date = DateTime.Now;
                    newItem._item_count = count;

                    newItem.item_name = _item_info.name;
                    newItem.item_id = _item_info.item_id;

                    _current_inventory.Add(newItem);
                    _idDic.Add(_item_info.item_id, newItem);
                    _nameDic.Add(_item_info.name, newItem);
                    // 标记为新获得的物品
                    _new_obtained_item_names.Add(newItem.item_name);
                    return true;
                }
            }
            else
            {
                int remain_count = _item._item_count + count;
                if (remain_count < 0)
                {
                    return false;
                }
                if (remain_count == 0)
                {
                    _current_inventory.Remove(_item);
                    _idDic.Remove(_item.item_id);
                    _nameDic.Remove(_item.item_name);
                }

                _item._item_count = remain_count;
                return true;
            }
        }

        /// <summary>
        /// 改变多个物品的数量
        /// </summary>
        /// <param name="_change_list"></param>
        public void Change_Item_Count(in List<(Game_Item_Info, int)> _change_list, List<Game_Item_Info> db)
        {
            foreach (var item in _change_list)
            {
                Change_Item_Count(item.Item1, item.Item2, db);
            }
        }
        public void Set_Favorite(int item_id, bool is_Favorite)
        {
            if (_idDic.TryGetValue(item_id, out var item))
            {
                item.is_favorite = is_Favorite;
                if (item.is_favorite) item.favorite_time = DateTime.Now;
                else { item.favorite_time = default; }
            }
        }
        public void Set_Favorite(string item_name, bool is_Favorite)
        {
            if (_nameDic.TryGetValue(item_name, out var item))
            {
                item.is_favorite = is_Favorite;
                if (item.is_favorite) item.favorite_time = DateTime.Now;
                else { item.favorite_time = default; }
            }
        }

        #endregion

        public void Load_Inventory_From_Json_Data(string _data, Dictionary<int, Game_Item_Info> infoDIc)
        {
            var _neo_inventory = JsonConvert.DeserializeObject<Game_Inventory>(_data);
            _idDic.Clear();
            _nameDic.Clear();
            if (_neo_inventory != null)
            {
                _current_inventory = _neo_inventory._current_inventory;
                _new_obtained_item_names = _neo_inventory._new_obtained_item_names;
            }
            foreach (var item in _current_inventory)
            {
                //如果当前物品表里面有这个物品，填充info，否则删除这个Item
                if (infoDIc.TryGetValue(item.item_id, out var info))
                {
                    item.item_info = info;
                    _nameDic[item.item_name] = item;
                    _idDic[item.item_id] = item;
                }
                else
                {
                    _current_inventory.Remove(item);
                    //Log.Error("出现了不在物品表里的数据，或是数据格式错误导致json解析失败");
                }
            }

        }

        public void Generate_Full_Inventory(List<Game_Item_Info> _game_item_db)
        {
            int seed = (int)DateTime.Now.ToBinary();
            var _rd = new Random(seed);
            _current_inventory.Clear();
            _idDic.Clear();
            _nameDic.Clear();

            foreach (var _info in _game_item_db)
            {
                var new_item = new Game_Item_In_Inventory();
                new_item.item_name = _info.name;

                new_item.item_id = _info.item_id;
                new_item.item_info = _info;

                new_item.is_favorite = false;

                new_item._obtain_date = DateTime.Now.AddDays(-(double)_rd.Next(0, 365));
                new_item.favorite_time = DateTime.Now.AddDays(-(double)_rd.Next(0, 365));

                if (_info.type == Item_Type.Cloth)
                {
                    new_item._item_count = 1;
                }
                else
                {
                    new_item._item_count = _rd.Next(1, 99);
                }

                _idDic[new_item.item_id] = new_item;
                _nameDic[new_item.item_name] = new_item;
                _current_inventory.Add(new_item);
            }
        }




    }

}
