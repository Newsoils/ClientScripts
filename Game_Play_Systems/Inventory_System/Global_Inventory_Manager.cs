using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using Cmd;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.Game_Play_System
{

    //背包Id   
    //1	食物背包
    //2	幸运小物背包
    //3	唱片背包
    //4	种子背包
    //5	肥料背包
    //6	花盆背包
    //7	家具背包
    //8	服装背包
    //9	可回收物品背包


    public class Global_Inventory_Manager : SingletonMono<Global_Inventory_Manager>
    {
        protected override bool PersistAcrossScenes => true;

        [Header("物品配置表（仅作展示，数据实际由 JsonDataManager 加载）")]
        [SerializeField]
        private List<Game_Item_Info> _gameItem_db;

        /// <summary>最近一次 <see cref="GetItemBagRes"/> 中的道具背包上限。</summary>
        public long LastServerItemBagCapacity { get; private set; }

        /// <summary>
        /// 注意！这是只读接口，不要对其进行修改操作
        /// 修改请通过Global_Inventory_Manager.Change_Items_count
        /// </summary>
        public static IReadOnlyList<Game_Item_In_Inventory> Items => Instance._inventory._items;

        [HideInInspector]
        public Dictionary<int, ItemGroupInfo> itemGroupDic = new Dictionary<int, ItemGroupInfo>();
        [ReadOnly]
        public Dictionary<GroupType, List<ItemGroupInfo>> itemGroupByTypeDic = new Dictionary<GroupType, List<ItemGroupInfo>>();

        /// <summary>套组实时状态缓存（玩家已获得的套组信息）。</summary>
        private Dictionary<int, ItemGroupData> _obtainedItemGroups = new Dictionary<int, ItemGroupData>();

        #region 物品静态数据字典

        /// <summary>物品 ID 到信息的字典缓存。</summary>
        private Dictionary<int, Game_Item_Info> _itemInfoIdDic = new Dictionary<int, Game_Item_Info>();

        /// <summary>物品名称到信息的字典缓存。</summary>
        private Dictionary<string, Game_Item_Info> _itemInfoNameDic = new Dictionary<string, Game_Item_Info>();

        #endregion

        [ShowInInspector]
        private Game_Inventory _inventory = new Game_Inventory();

        public static List<Game_Item_Info> GameItem_DB
        {
            get
            {
                return Instance._gameItem_db;
            }
        }

        public static List<long> New_Obtain_Items
        {
            get
            {
                return Instance._inventory._new_Obtained_Item_Uid;
            }
        }


        #region 套组相关 查询


        /// <summary>
        /// 重建套组缓存（全量刷新，初始化或服务器全量同步后调用）。
        /// </summary>
        private void BuildObtainedItemGroups()
        {
            _obtainedItemGroups.Clear();

            if (itemGroupDic == null || _inventory == null)
                return;

            var currentItems = _inventory._items;
            if (currentItems == null || currentItems.Count == 0)
                return;

            var obtainedItemIds = new HashSet<int>();
            foreach (var slot in currentItems)
            {
                if (slot != null && slot.item_info != null)
                    obtainedItemIds.Add(slot.item_info.item_id);
            }

            foreach (var kvp in itemGroupDic)
            {
                var group = kvp.Value;
                if (group.group_item_list == null || group.group_item_list.Count == 0)
                    continue;

                var runtime = new ItemGroupData
                {
                    group_id = group.group_id,
                    itemGroupInfo = group,
                    totalCount = group.group_item_list.Count,
                    obtainCount = 0,
                    obtained = new List<int>()
                };

                foreach (var itemId in group.group_item_list)
                {
                    if (obtainedItemIds.Contains(itemId))
                    {
                        runtime.obtainCount++;
                        runtime.obtained.Add(itemId);
                    }
                }

                _obtainedItemGroups[group.group_id] = runtime;
            }
        }

        private void RefreshObtainedItemGroups()
        {
            BuildObtainedItemGroups();
        }

        /// <summary>
        /// 增量更新套组缓存（物品变更时调用，自动处理增加和移除）。
        /// </summary>
        private void UpdateObtainedItemGroups(IEnumerable<(Game_Item_Info info, int count)> changedItems)
        {
            if (itemGroupDic == null || _inventory == null || changedItems == null)
                return;

            var affectedGroupIds = new HashSet<int>();
            foreach (var (info, _) in changedItems)
            {
                if (info == null) continue;

                foreach (var kvp in itemGroupDic)
                {
                    var group = kvp.Value;
                    if (group.group_item_list != null && group.group_item_list.Contains(info.item_id))
                        affectedGroupIds.Add(group.group_id);
                }
            }

            if (affectedGroupIds.Count == 0)
                return;

            var obtainedItemIds = new HashSet<int>();
            foreach (var slot in _inventory._items)
            {
                if (slot != null && slot.item_info != null)
                    obtainedItemIds.Add(slot.item_info.item_id);
            }

            foreach (var groupId in affectedGroupIds)
            {
                if (!itemGroupDic.TryGetValue(groupId, out var group))
                    continue;

                if (!_obtainedItemGroups.TryGetValue(groupId, out var runtime))
                {
                    runtime = new ItemGroupData
                    {
                        group_id = group.group_id,
                        itemGroupInfo = group,
                        totalCount = group.group_item_list.Count,
                        obtainCount = 0,
                        obtained = new List<int>()
                    };
                    _obtainedItemGroups[groupId] = runtime;
                }

                runtime.obtainCount = 0;
                runtime.obtained.Clear();
                foreach (var itemId in group.group_item_list)
                {
                    if (obtainedItemIds.Contains(itemId))
                    {
                        runtime.obtainCount++;
                        runtime.obtained.Add(itemId);
                    }
                }
            }
        }

        /// <summary>
        /// 查询玩家当前已获得的套组列表（至少拥有一件套组物品）。
        /// </summary>
        public static List<ItemGroupData> GetObtainedGroups()
        {
            if (Instance == null)
                return new List<ItemGroupData>();

            Instance.RefreshObtainedItemGroups();
            if (Instance._obtainedItemGroups == null)
                return new List<ItemGroupData>();

            var result = new List<ItemGroupData>();
            foreach (var kvp in Instance._obtainedItemGroups)
            {
                if (kvp.Value.obtainCount > 0)
                    result.Add(kvp.Value);
            }
            return result;
        }

        /// <summary>
        /// 查询特定套组中玩家已获得的物品信息。
        /// </summary>
        public static List<Game_Item_In_Inventory> GetGroupObtainedItems(int groupId)
        {
            if (Instance == null)
                return new List<Game_Item_In_Inventory>();

            Instance.RefreshObtainedItemGroups();
            if (Instance._obtainedItemGroups == null)
                return new List<Game_Item_In_Inventory>();

            var res = new List<Game_Item_In_Inventory>();
            if (!Instance._obtainedItemGroups.TryGetValue(groupId, out var runtime) || runtime?.obtained == null)
                return res;

            foreach (var itemId in runtime.obtained)
            {
                var item = Instance._inventory.Get_Item(itemId);
                if (item != null)
                    res.Add(item);
            }
            return res;
        }

        public static List<ItemGroupData> GetObtainedGroupsByType(GroupType groupType)
        {
            if (Instance == null)
                return new List<ItemGroupData>();

            Instance.RefreshObtainedItemGroups();
            if (Instance._obtainedItemGroups == null || Instance.itemGroupByTypeDic == null)
                return new List<ItemGroupData>();
            if (!Instance.itemGroupByTypeDic.TryGetValue(groupType, out var groupsOfType) || groupsOfType == null)
                return new List<ItemGroupData>();
            var result = new List<ItemGroupData>();
            foreach (var group in groupsOfType)
            {
                if (Instance._obtainedItemGroups.TryGetValue(group.group_id, out var runtime) && runtime.obtainCount > 0)
                    result.Add(runtime);
            }
            return result;
        }

        #endregion

        void Start()
        {
            JsonDataManager.LoadGameItemData(out _gameItem_db, out _itemInfoIdDic, out _itemInfoNameDic);
            JsonDataManager.LoadItemGroupData(out itemGroupDic, out itemGroupByTypeDic);
            BuildObtainedItemGroups();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #region 增删改

        public static void Change_Items_Count(in List<(string, int)> changeList, string source = "")
        {
            List<(string, int)> moneyList = new();

            Instance.ChangeItemsInner(
                 changeList,
                 GetItemInfo,
                 (key, count) => moneyList.Add((key, count)),
                 source
             );

            _ = MoneyManager.Instance.ChangeCurrencyMulti(moneyList, source);
        }

        public static void Change_Items_Count(in List<(int, int)> changeList, string source = "")
        {
            List<(int, int)> moneyList = new();

            Instance.ChangeItemsInner(
                changeList,
                GetItemInfo,
                (key, count) => moneyList.Add((key, count)),
                source
            );

            List<(string, int)> nameChange = new List<(string, int)>();
            foreach (var changeItem in moneyList)
            {
                if (MoneyManager.Instance.idDic.TryGetValue(changeItem.Item1, out var currency))
                    nameChange.Add((currency.currency_name, changeItem.Item2));
            }
            _ = MoneyManager.Instance.ChangeCurrencyMulti(nameChange, source);
        }

        public static bool ReduceItemCount(string itemName, int changeCount, string source = "")
        {
            int currentCount = Instance.GetItemNum(itemName);
            if (currentCount < changeCount)
            {
                return false;
            }
            Change_Items_Count(new List<(string, int)> { (itemName, -changeCount) }, source);
            return true;
        }

        private void ChangeItemsInner<TKey>(IEnumerable<(TKey key, int count)> changeList, Func<TKey, Game_Item_Info> getItemInfo,
                Action<TKey, int> addMoney, string source)
        {
            string resLog = "获得了：";
            List<(Game_Item_Info, int)> itemList = new();

            foreach (var (key, count) in changeList)
            {
                var itemInfo = getItemInfo(key);
                if (itemInfo == null) continue;

                resLog += $"{itemInfo.name}*{count},";

                switch (itemInfo.name)
                {
                    case "鱼币":
                    case "罐罐":
                        addMoney(key, count);
                        break;

                    case "亲密度":
                        ExpManager.instance.AddExp(count);
                        break;

                    default:
                        itemList.Add((itemInfo, count));
                        break;
                }
            }

            Log.Info(resLog);
            _inventory.Change_Item_Count(in itemList, _gameItem_db);
            UpdateObtainedItemGroups(itemList);
        }

        public void ClearAllItems()
        {
            _inventory._items.Clear();
        }

        public void Set_Favorite(string item_Name, bool isFrvorite)
        {
            _inventory.Set_Favorite(item_Name, isFrvorite);
        }

        public void Set_Favorite(int item_Id, bool isFrvorite)
        {
            _inventory.Set_Favorite(item_Id, isFrvorite);
        }

        #endregion

        #region 查
        public static List<Game_Item_Info> GetItemInfos(IEnumerable<int> ids)
        {
            var result = new List<Game_Item_Info>();
            foreach (var id in ids)
            {
                if (Instance._itemInfoIdDic.TryGetValue(id, out var info) && info != null)
                    result.Add(info);
            }
            return result;
        }

        public static List<Game_Item_Info> GetItemInfos(IEnumerable<string> names)
        {
            var result = new List<Game_Item_Info>();
            foreach (var name in names)
            {
                if (Instance._itemInfoNameDic.TryGetValue(name, out var info) && info != null)
                    result.Add(info);
            }
            return result;
        }

        public static Game_Item_Info GetItemInfo(string itemName)
        {
            if (Instance == null || string.IsNullOrEmpty(itemName))
                return null;

            Instance._itemInfoNameDic.TryGetValue(itemName, out var info);
            return info;
        }
        public static Game_Item_Info GetItemInfo(int itemId)
        {
            if (Instance == null)
                return null;

            Instance._itemInfoIdDic.TryGetValue(itemId, out var info);
            return info;
        }

        public static List<Game_Item_Info> Get_Current_Has_Item()
        {
            return Instance._inventory._items.Select(i => i.item_info).ToList();
        }

        public static List<Game_Item_In_Inventory> Get_Items_By_Type(Item_Type type)
        {
            return Instance._inventory.Get_Items_By_Type(type);
        }


        public static Game_Item_In_Inventory GetItem(string name)
        {
            return Instance._inventory.Get_Item(name);
        }
        public static Game_Item_In_Inventory GetItem(int itemId)
        {
            return Instance._inventory.Get_Item(itemId);
        }

        public static Game_Item_In_Inventory GetItem(long uid)
        {
            return Instance._inventory.Get_Item(uid);
        }

        public static bool IsNewObtainItem(long uid)
        {
            return Instance != null && Instance._inventory != null && Instance._inventory.Is_New_Obtained_Item(uid);
        }


        public int GetItemNum(string itemName)
        {
            return _inventory.Get_Item_Count(itemName);
        }

        public int GetItemNum(int itemId)
        {
            return _inventory.Get_Item_Count(itemId);
        }

        /// <summary>
        /// 通过背包数据获取货币数量（货币道具约定：1=鱼币，2=罐罐）。
        /// </summary>
        public static int GetCurrencyNum(int currencyItemId)
        {
            if (Instance == null) return 0;
            return Instance.GetItemNum(currencyItemId);
        }

        /// <summary>
        /// 通过货币名称获取数量（鱼币/罐罐）。
        /// </summary>
        public static int GetCurrencyNum(string currencyName)
        {
            if (Instance == null || string.IsNullOrEmpty(currencyName)) return 0;
            return Instance.GetItemNum(currencyName);
        }
        public static int GetCoinNum() => GetCurrencyNum(1);
        public static int GetDiamondNum() => GetCurrencyNum(2);

        #endregion


        #region 排序
        /// <summary>
        /// 【对外接口】直接对全局背包进行排序
        /// 会修改 Instance._inventory 中的列表顺序
        /// </summary>
        /// <param name="sortType">排序类型</param>
        /// <param name="isAscending">是否正序 (false=降序，通常游戏用false)</param>
        public static void SortGlobalInventory(InventorySortType sortType, bool isAscending = false)
        {
            if (Instance == null || Instance._inventory == null) return;

            // 调用内部方法操作私有成员
            Instance.SortInternalInventory(sortType, isAscending);
        }

        /// <summary>
        /// 【对外接口】给定一组物品，返回排序后的列表（不影响全局背包）
        /// </summary>
        public static List<Game_Item_In_Inventory> SortItemList(IEnumerable<Game_Item_In_Inventory> sourceItems, InventorySortType sortType, bool isAscending = false)
        {
            List<Game_Item_In_Inventory> tempList = new List<Game_Item_In_Inventory>(sourceItems);
            tempList.SortByRarity();
            InventorySorter.SortList(tempList, sortType, isAscending);
            return tempList;
        }

        // 内部实现，用于访问 _inventory 私有变量
        private void SortInternalInventory(InventorySortType sortType, bool isAscending)
        {
            // 直接对 _items 进行原地排序
            InventorySorter.SortList(_inventory._items, sortType, isAscending);

            // 可选：如果你的背包UI是基于事件刷新的，这里建议调用一次刷新事件
            // _update_inventory_from_server?.Invoke(); // 或者是专门的 UI_Refresh_Event
            Debug.Log($"Inventory Sorted by {sortType}, Ascending: {isAscending}");
        }

        #endregion

        #region 传输消息

        /// <summary>
        /// 根据服务器返回的 <see cref="GetItemBagRes"/> 全量刷新本地背包。
        /// 不会自动 <see cref="Send_inventory_to_server"/>，避免与服务器对推。
        /// </summary>
        public void ApplyGetItemBagRes(GetItemBagRes res)
        {
            if (res == null || _inventory == null)
                return;

            LastServerItemBagCapacity = res.ItemBagCapacity;

            // Game_Inventory 当前按 item_id 建索引（单栈），因此这里不做 Count 合并，只保留每个 ConfigID 的最后一条。

            var slots = new List<Game_Item_In_Inventory>(res.Items.Count);
            foreach (var it in res.Items)
            {
                if (it == null)
                    continue;
                int cfgId = (int)it.ConfigID;
                if (cfgId == 0)
                    continue;

                var info = GetItemInfo(cfgId);
                if (info == null)
                    continue;

                long c = it.Count;
                if (c <= 0)
                    continue;

                int count = c > int.MaxValue ? int.MaxValue : (int)c;
                var slot = new Game_Item_In_Inventory
                {
                    uid = unchecked((long)it.UID),
                    item_id = cfgId,              // Common.ItemInfo.ConfigID
                    item_name = info.name,
                    item_info = info,
                    IsNew = it.IsNew != 0,
                    VaildTime = it.VaildTime,
                    _obtain_date = DateTime.Now,
                    _item_count = count,           // Common.ItemInfo.Count
                    is_favorite = false,
                };
                slots.Add(slot);
            }

            _inventory.ReplaceInventoryFromServerSlots(slots);
            RefreshObtainedItemGroups();
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }

        public void UpdateInventory_From_S2c(ItemChangeS2C s2c)
        {
            _inventory.update_inventory_from_s2c(s2c, GetItemInfo);
            RefreshObtainedItemGroups();
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }
        #endregion

    }
    public class TestInventory
    {
        public List<(string, int)> low;
        public List<(string, int)> middle;
        public List<(string, int)> high;
    }
}
