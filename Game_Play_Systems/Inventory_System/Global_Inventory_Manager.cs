using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using Cmd;
using UnityEngine;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Global_Inventory_Manager : SingletonMono<Global_Inventory_Manager>
    {
        protected override bool PersistAcrossScenes => true;

        public GameItem_DB_SO _itemDB_SO;

        [SerializeField]
        private Game_Inventory _inventory;

        /// <summary>最近一次 <see cref="GetItemBagRes"/> 中的道具背包上限。</summary>
        public long LastServerItemBagCapacity { get; private set; }

        /// <summary>
        /// 注意！这是只读接口，不要对其进行修改操作
        /// 修改请通过Global_Inventory_Manager.Change_Items_count
        /// </summary>
        public static IReadOnlyList<Game_Item_In_Inventory> Items  => Instance._inventory.Items;
  

        // [HideInInspector]
        // public UnityEvent _update_inventory_from_server;
        [HideInInspector]
        public UnityEvent _send_inventory_to_server;
        [HideInInspector]
        public UnityEvent _update_shop_state_from_server;
        [HideInInspector]
        public UnityEvent _send_shop_state_to_server;

        public static List<Game_Item_Info> GameItem_DB
        {
            get
            {
                return Instance._itemDB_SO._gameItem_db;
            }
        }

        public static List<long> New_Obtain_Items
        {
            get
            {
                return Instance._inventory._new_obtained_item_names;
            }
        }


        void Start()
        {
            _itemDB_SO.RefreshData();
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
            _inventory.Change_Item_Count(in itemList, _itemDB_SO._gameItem_db);
        }

        public void ClearAllItems()
        {
            _inventory._current_inventory = new List<Game_Item_In_Inventory>();
        }

        public void Set_Favorite(string item_Name,bool isFrvorite)
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
            return Instance._itemDB_SO.GetItemInfos(ids);
        }

        public static List<Game_Item_Info> GetItemInfos(IEnumerable<string> names)
        {
            return Instance._itemDB_SO.GetItemInfos(names);
        }

        public static Game_Item_Info GetItemInfo(string itemName)
        {
            return Instance._itemDB_SO.GetItemInfo(itemName);
        }
        public static Game_Item_Info GetItemInfo(int itemId)
        {
            return Instance._itemDB_SO.GetItemInfo(itemId);
        }

        public static List<Game_Item_Info> Get_Current_Has_Item()
        {
            return Instance._inventory._current_inventory.Select(i => i.item_info).ToList();
        }

        public static List<Game_Item_In_Inventory>  Get_Items_By_Type(Item_Type type)
        {
            return Instance._inventory.Get_Items_By_Type(type);
        }

        public static IReadOnlyList<Game_Item_In_Inventory> GetAllItems()
        {
            return Instance._inventory.Items;
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

        public int GetItemNum(string itemName)
        {
            //TODO:获取钱币数量
            //if (MoneyManager_v2.Instance.nameDic.ContainsKey(itemName))
            //{
            //    MoneyManager_v2.Instance.GetMoneyCount(itemName);
            //}
            return _inventory.Get_Item_Count(itemName);
        }

        public int GetItemNum(int itemId)
        {
            //TODO:获取钱币数量
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
            // 直接对 _current_inventory 进行原地排序
            InventorySorter.SortList(_inventory._current_inventory, sortType, isAscending);

            // 可选：如果你的背包UI是基于事件刷新的，这里建议调用一次刷新事件
            // _update_inventory_from_server?.Invoke(); // 或者是专门的 UI_Refresh_Event
            Debug.Log($"Inventory Sorted by {sortType}, Ascending: {isAscending}");
        }
        #region 传输消息

        /// <summary>
        /// 根据服务器返回的 <see cref="GetItemBagRes"/> 全量刷新本地背包。
        /// 不会自动 <see cref="Send_inventory_to_server"/>，避免与服务器对推。
        /// </summary>
        public void ApplyGetItemBagRes(GetItemBagRes res)
        {
            if (res == null || _inventory == null || _itemDB_SO == null)
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
                    IsNew = it.IsNew,
                    VaildTime = it.VaildTime,
                    _obtain_date = DateTime.Now,
                    _item_count = count,           // Common.ItemInfo.Count
                    is_favorite = false,
                };
                slots.Add(slot);
            }

            _inventory.ReplaceInventoryFromServerSlots(slots);
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }

        public void update_inventory_from_s2c(ItemChangeS2C s2c)
        {
            _inventory.update_inventory_from_s2c(s2c, GetItemInfo);
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