using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CLIP.Project_Mouse.ENUM;
using Cmd;
using Common;
using Newtonsoft.Json;
using UnityEngine;


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
        private readonly Dictionary<int, List<Game_Item_In_Inventory>> _idDic = new Dictionary<int, List<Game_Item_In_Inventory>>();
        [JsonIgnore]
        private readonly Dictionary<string, List<Game_Item_In_Inventory>> _nameDic = new Dictionary<string, List<Game_Item_In_Inventory>>();

        public List<long> _new_obtained_item_names = new List<long>();

        /// <summary>
        /// UI 刷新回调（由上层系统订阅）。
        /// </summary>
        [JsonIgnore]
        public Action OnInventoryUIUpdateRequested;

        #region 查询
        public Game_Item_In_Inventory Get_Item(string name)
        {
            if (_nameDic.TryGetValue(name, out var list) && list != null && list.Count > 0)
                return list[0]; // 数量最小的优先
            return null;
        }
        public Game_Item_In_Inventory Get_Item(int id)
        {
            if (_idDic.TryGetValue(id, out var list) && list != null && list.Count > 0)
                return list[0]; // 数量最小的优先
            return null;
        }

        public Game_Item_In_Inventory Get_Item(long uid)
        {
            return _current_inventory.FirstOrDefault(i => i != null && i.uid == uid);
        }


        public int Get_Item_Count(string name)
        {
            if (_nameDic.TryGetValue(name, out var list) && list != null && list.Count > 0)
                return list.Sum(i => i != null ? i._item_count : 0);
            return 0;
        }

        public int Get_Item_Count(int id)
        {
            if (_idDic.TryGetValue(id, out var list) && list != null && list.Count > 0)
                return list.Sum(i => i != null ? i._item_count : 0);
            return 0;
        }

        public List<Game_Item_In_Inventory> Get_Items_By_Type(Item_Type type)
        {
            return _current_inventory.Where(i => i.item_info?.type == type).ToList();
        }
        #endregion

        /// <summary>
        /// 根据服务器下发的道具变更消息更新本地背包数据：
        /// - ItemAdd / ItemUpd：按 UID 新增或更新
        /// - ItemDel：按 UID 删除
        /// 更新完成后会重建索引并调用 <see cref="Update_Inventory_UI"/>。
        /// </summary>
        public void update_inventory_from_s2c(ItemChangeS2C s2c, Func<int, Game_Item_Info> getItemInfo)
        {
            if (s2c == null) return;

            ApplyItemInfosAddOrUpd(s2c.ItemAdd, getItemInfo);
            ApplyItemInfosAddOrUpd(s2c.ItemUpd, getItemInfo);
            ApplyItemInfosDel(s2c.ItemDel);

            RebuildIndexesAndSort();
            Update_Inventory_UI();
        }

        private void ApplyItemInfosAddOrUpd(IEnumerable<ItemInfo> infos, Func<int, Game_Item_Info> getItemInfo)
        {
            if (infos == null) return;
            foreach (var it in infos)
            {
                if (it == null) continue;
                long uid = unchecked((long)it.UID);
                int itemId = unchecked((int)it.ConfigID);
                int count = it.Count > int.MaxValue ? int.MaxValue : (int)it.Count;

                if (uid == 0 || itemId == 0) continue;

                Game_Item_Info itemInfo = getItemInfo?.Invoke(itemId);

                var local = _current_inventory.FirstOrDefault(x => x != null && x.uid == uid);
                if (local == null)
                {
                    local = new Game_Item_In_Inventory
                    {
                        uid = uid,
                        item_id = itemId,
                        item_name = itemInfo?.name ?? string.Empty,
                        item_info = itemInfo,
                        _obtain_date = DateTime.Now,
                        _item_count = Mathf.Max(0, count),
                        is_favorite = false,
                        IsNew = it.IsNew,
                        VaildTime = it.VaildTime,
                    };
                    _current_inventory.Add(local);
                    _new_obtained_item_names.Add(uid);
                }
                else
                {
                    local.item_id = itemId;
                    local._item_count = Mathf.Max(0, count);
                    local.IsNew = it.IsNew;
                    local.VaildTime = it.VaildTime;
                    if (itemInfo != null)
                    {
                        local.item_name = itemInfo.name;
                        local.item_info = itemInfo;
                    }
                }
            }
        }

        private void ApplyItemInfosDel(IEnumerable<ItemInfo> infos)
        {
            if (infos == null) return;
            foreach (var it in infos)
            {
                if (it == null) continue;
                long uid = unchecked((long)it.UID);
                if (uid == 0) continue;

                for (int i = _current_inventory.Count - 1; i >= 0; i--)
                {
                    var local = _current_inventory[i];
                    if (local != null && local.uid == uid)
                        _current_inventory.RemoveAt(i);
                }
            }
        }

        private void RebuildIndexesAndSort()
        {
            _idDic.Clear();
            _nameDic.Clear();

            for (int i = _current_inventory.Count - 1; i >= 0; i--)
            {
                var item = _current_inventory[i];
                if (item == null)
                {
                    _current_inventory.RemoveAt(i);
                    continue;
                }

                if (!_idDic.TryGetValue(item.item_id, out var idList) || idList == null)
                    _idDic[item.item_id] = idList = new List<Game_Item_In_Inventory>();
                idList.Add(item);

                if (!string.IsNullOrEmpty(item.item_name))
                {
                    if (!_nameDic.TryGetValue(item.item_name, out var nameList) || nameList == null)
                        _nameDic[item.item_name] = nameList = new List<Game_Item_In_Inventory>();
                    nameList.Add(item);
                }
            }

            foreach (var kv in _idDic)
                kv.Value.Sort((a, b) => (a?._item_count ?? 0).CompareTo(b?._item_count ?? 0));
            foreach (var kv in _nameDic)
                kv.Value.Sort((a, b) => (a?._item_count ?? 0).CompareTo(b?._item_count ?? 0));
        }

        public void Update_Inventory_UI()
        {
            OnInventoryUIUpdateRequested?.Invoke();
        }

        #region 修改


        /// <summary>
        /// 用服务器全量背包列表替换本地数据，并按物品表重建索引（与 Load_Inventory_From_Json_Data 行为一致，不经 JSON）。
        /// </summary>
        public void ReplaceInventoryFromServerSlots(List<Game_Item_In_Inventory> newSlots)
        {
            _idDic.Clear();
            _nameDic.Clear();
            _current_inventory = newSlots ?? new List<Game_Item_In_Inventory>();
            for (int i = _current_inventory.Count - 1; i >= 0; i--)
            {
                var item = _current_inventory[i];

                if (!_idDic.TryGetValue(item.item_id, out var idList) || idList == null)
                    _idDic[item.item_id] = idList = new List<Game_Item_In_Inventory>();
                if (!_nameDic.TryGetValue(item.item_name, out var nameList) || nameList == null)
                    _nameDic[item.item_name] = nameList = new List<Game_Item_In_Inventory>();

                idList.Add(item);
                nameList.Add(item);
            }

            foreach (var kv in _idDic) kv.Value.Sort((a, b) => (a?._item_count ?? 0).CompareTo(b?._item_count ?? 0));
            foreach (var kv in _nameDic) kv.Value.Sort((a, b) => (a?._item_count ?? 0).CompareTo(b?._item_count ?? 0));
        }

        public void Set_Favorite(int item_id, bool is_Favorite)
        {
            if (!_idDic.TryGetValue(item_id, out var list) || list == null) return;
            foreach (var item in list)
            {
                if (item == null) continue;
                item.is_favorite = is_Favorite;
                if (item.is_favorite) item.favorite_time = DateTime.Now;
                else item.favorite_time = default;
            }
        }
        public void Set_Favorite(string item_name, bool is_Favorite)
        {
            if (!_nameDic.TryGetValue(item_name, out var list) || list == null) return;
            foreach (var item in list)
            {
                if (item == null) continue;
                item.is_favorite = is_Favorite;
                if (item.is_favorite) item.favorite_time = DateTime.Now;
                else item.favorite_time = default;
            }
        }

        /// <summary>
        /// 客户端本地批量增减道具数量（用于奖励、消耗等）；第二参数保留与旧接口兼容，可为 null。
        /// </summary>
        public void Change_Item_Count(in List<(Game_Item_Info info, int delta)> changes, List<Game_Item_Info> _)
        {
            if (changes == null || changes.Count == 0)
                return;

            foreach (var (info, delta) in changes)
            {
                if (info == null || delta == 0)
                    continue;

                if (delta > 0)
                {
                    AddItemCountLocal(info, delta);
                    continue;
                }

                RemoveItemCountLocal(info, -delta);
            }

            RebuildIndexesAndSort();
            Update_Inventory_UI();
        }

        private static long _nextLocalNegativeUid = -1L;

        private void AddItemCountLocal(Game_Item_Info info, int add)
        {
            if (add <= 0) return;

            Game_Item_In_Inventory stack = Get_Item(info.name);
            if (stack != null)
            {
                stack._item_count += add;
                if (stack.item_info == null)
                    stack.item_info = info;
                return;
            }

            var slot = new Game_Item_In_Inventory
            {
                uid = Interlocked.Decrement(ref _nextLocalNegativeUid),
                item_id = info.item_id,
                item_name = info.name,
                item_info = info,
                _obtain_date = DateTime.Now,
                _item_count = add,
                is_favorite = false,
            };
            _current_inventory.Add(slot);
        }

        private void RemoveItemCountLocal(Game_Item_Info info, int remove)
        {
            if (remove <= 0) return;

            if (!_nameDic.TryGetValue(info.name, out var list) || list == null || list.Count == 0)
                return;

            var ordered = list.OrderBy(x => x != null ? x._item_count : 0).ToList();
            int remaining = remove;

            foreach (var slot in ordered)
            {
                if (slot == null || remaining <= 0)
                    continue;

                int take = Mathf.Min(remaining, slot._item_count);
                slot._item_count -= take;
                remaining -= take;

                if (slot._item_count <= 0)
                    _current_inventory.Remove(slot);
            }
        }

        #endregion




    }

}
