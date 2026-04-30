using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Global_Inventory_Manager : MonoBehaviour
    {
        public static Global_Inventory_Manager _instance;

        public shop_state _shop_state;
        public GameItem_DB_SO _itemDB_SO;
        public Shop_List_DB_SO _shop_list_db_SO;

        [SerializeField]
        private Game_Inventory _inventory;

        /// <summary>
        /// 注意！这是只读接口，不要对其进行修改操作
        /// 修改请通过Global_Inventory_Manager.Change_Items_count
        /// </summary>
        public static IReadOnlyList<Game_Item_In_Inventory> Items  => _instance._inventory.Items;
  

        [HideInInspector]
        public UnityEvent _update_inventory_from_server;
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
                return _instance._itemDB_SO._gameItem_db;
            }
        }

        public static List<string> New_Obtain_Items
        {
            get
            {
                return _instance._inventory._new_obtained_item_names;
            }
        }



        void Start()
        {

            _itemDB_SO.RefreshData();
            _shop_list_db_SO.load_from_json();

            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject);
                SceneManager.sceneLoaded += update_inventory_from_server;
                SceneManager.sceneLoaded += update_shop_state_from_server;

                RefreshShopState();
            }
            else
            {
                if (_instance != this)
                {
#if UNITY_EDITOR
                    DestroyImmediate(this.gameObject);
#else
           Destroy(this.gameObject);
#endif
                }
            }
        }
        private void Update()
        {
            //Keyboard keyboard = Keyboard.current;
            //if (keyboard.digit1Key.wasPressedThisFrame)
            //{
            //    generate_test_inventory();
            //}
            //if (keyboard.digit2Key.wasPressedThisFrame)
            //{
            //    generate_middle_inventory();
            //}
            //if (keyboard.digit3Key.wasPressedThisFrame)
            //{
            //    generate_high_inventory();
            //}
        }

        public void OnDestroy()
        {
            SceneManager.sceneLoaded -= update_inventory_from_server;
            SceneManager.sceneLoaded -= update_shop_state_from_server;
        }

        #region 增删改
        public static void Change_Item_Count(int id, int count)
        {
            //TODO:修改钱币
            if (MoneyManager.Instance.idDic.ContainsKey(id))
            {
                MoneyManager.Instance.ChangeCurrency(id, count, "");
            }
            _instance._inventory.Change_Item_Count(GetItemInfo(id), count, _instance._itemDB_SO._gameItem_db);

        }

        public static void Change_Item_Count(string name, int count)
        {
            //TODO:修改钱币
            if (MoneyManager.Instance.nameDic.ContainsKey(name))
            {
                MoneyManager.Instance.ChangeCurrency(name, count, "");
            }
            _instance._inventory.Change_Item_Count(GetItemInfo(name), count, _instance._itemDB_SO._gameItem_db);
        }

       

        public static void Change_Items_Count(in List<(string, int)> changeList, string source = "")
        {
            List<(string, int)> moneyList = new();

            _instance.ChangeItemsInner(
                 changeList,
                 GetItemInfo,
                 (key, count) => moneyList.Add((key, count)),
                 source
             );

            MoneyManager.Instance.ChangeCurrencyMulti(moneyList, source);
        }

        public static void Change_Items_Count(in List<(int, int)> changeList, string source = "")
        {
            List<(int, int)> moneyList = new();

            _instance.ChangeItemsInner(
                changeList,
                GetItemInfo,
                (key, count) => moneyList.Add((key, count)),
                source
            );

            MoneyManager.Instance.ChangeCurrencyTask(moneyList, source);
        }

        public static bool ReduceItemCount(string itemName, int changeCount, string source = "")
        {
            int currentCount = _instance.GetItemNum(itemName);
            if(currentCount < changeCount)
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
            //addMoney
            _inventory.Change_Item_Count(in itemList, _itemDB_SO._gameItem_db);
            Send_inventory_to_server();
        }

        public void ClearAllItems()
        {
            _inventory._current_inventory = new List<Game_Item_In_Inventory>();
        }

        public void Set_Favorite(string item_Name,bool isFrvorite)
        {
            _inventory.Set_Favorite(item_Name, isFrvorite);
            Send_inventory_to_server();
        }

        public void Set_Favorite(int item_Id, bool isFrvorite)
        {
            _inventory.Set_Favorite(item_Id, isFrvorite);
            Send_inventory_to_server();
        }



        #endregion

        #region 查
        public static List<Game_Item_Info> GetItemInfos(IEnumerable<int> ids)
        {
            return _instance._itemDB_SO.GetItemInfos(ids);
        }

        public static List<Game_Item_Info> GetItemInfos(IEnumerable<string> names)
        {
            return _instance._itemDB_SO.GetItemInfos(names);
        }

        public static Game_Item_Info GetItemInfo(string itemName)
        {
            return _instance._itemDB_SO.GetItemInfo(itemName);
        }
        public static Game_Item_Info GetItemInfo(int itemId)
        {
            return _instance._itemDB_SO.GetItemInfo(itemId);
        }

        public static List<Game_Item_Info> Get_Current_Has_Item()
        {
            return _instance._inventory._current_inventory.Select(i => i.item_info).ToList();
        }

        public static List<Game_Item_In_Inventory>  Get_Items_By_Type(Item_Type type)
        {
            return _instance._inventory.Get_Items_By_Type(type);
        }

        public static IReadOnlyList<Game_Item_In_Inventory> GetAllItems()
        {
            return _instance._inventory.Items;
        }

        public static void Add_New_Obtain_Item(string name)
        {
            New_Obtain_Items.Add(name);
        }
        public static void Remove_New_Obtain_Item(string name)
        {
            New_Obtain_Items.Remove(name);
        }
        public static Game_Item_In_Inventory GetItem(string name)
        {
            return _instance._inventory.Get_Item(name);
        }
        public static Game_Item_In_Inventory GetItem(int itemId)
        {
            return _instance._inventory.Get_Item(itemId);
        }

        public static List<Game_Item_In_Inventory> GetItems(IEnumerable<int> ids)
        {
            return _instance._inventory.GetItems(ids.ToList());
        }

        public static List<Game_Item_In_Inventory> GetItems(IEnumerable<string> names)
        {
            return _instance._inventory.GetItems(names.ToList());
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

        #endregion


        private void RefreshShopState()
        {
            _shop_state = new shop_state();

            _shop_state._limited_cloth_item_list = _shop_list_db_SO._limited_cloth_item_list;
            _shop_state._limited_placement_item_list = _shop_list_db_SO._limited_placement_item_list;
            _shop_state._daily_shop_list = _shop_list_db_SO._daily_shop_list;
        }


        /// <summary>
        /// 【对外接口】直接对全局背包进行排序
        /// 会修改 Instance._inventory 中的列表顺序
        /// </summary>
        /// <param name="sortType">排序类型</param>
        /// <param name="isAscending">是否正序 (false=降序，通常游戏用false)</param>
        public static void SortGlobalInventory(InventorySortType sortType, bool isAscending = false)
        {
            if (_instance == null || _instance._inventory == null) return;

            // 调用内部方法操作私有成员
            _instance.SortInternalInventory(sortType, isAscending);
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

        public void update_inventory_from_server(Scene _s, LoadSceneMode _mode)
        {
            update_inventory_from_server();
        }
        public void update_inventory_from_server()
        {
            if (_update_inventory_from_server != null) _update_inventory_from_server.Invoke();
        }

        public void Send_inventory_to_server()
        {
            if (_send_inventory_to_server != null) _send_inventory_to_server.Invoke();
        }

        public void update_shop_state_from_server(Scene _s, LoadSceneMode _mode)
        {
            update_shop_state_from_server();
        }
        public void send_shop_state_to_server(Scene _s)
        {
            send_shop_state_to_server();
        }

        public void load_shop_state_from_json(string _json_str)
        {
            _shop_state = GF_SP.DeserializeObject<shop_state>(_json_str);
            _shop_list_db_SO._daily_shop_list = _shop_state._daily_shop_list;
            _shop_list_db_SO._limited_cloth_item_list = _shop_state._limited_cloth_item_list;
            _shop_list_db_SO._limited_placement_item_list = _shop_state._limited_placement_item_list;
        }
        public void try_refresh_shop_state()
        {
            Debug.Log("Try_Refresh_Shop_State_@_C#");
            var _now = DateTime.Now;
            bool _flag_changed = false;
            if ((_now - _shop_state._last_time_update_daily).TotalHours >= 24)
            {
                _shop_list_db_SO.get_daily_shop_list();
                _shop_state._daily_shop_list = _shop_list_db_SO._daily_shop_list;
                _shop_state._last_time_update_daily = DateTime.Today.AddHours(6);
                _flag_changed = true;
            }
            if ((_now - _shop_state._last_time_update_limited).TotalDays >= 14)
            {
                _shop_list_db_SO.get_limited_shop_list();
                _shop_state._limited_cloth_item_list = _shop_list_db_SO._limited_cloth_item_list;
                _shop_state._limited_placement_item_list = _shop_list_db_SO._limited_placement_item_list;
                _shop_state._last_time_update_limited = DateTime.Today.AddHours(6);
                _flag_changed = true;
            }
            if (_flag_changed == true)
            {
                send_shop_state_to_server();
                Debug.Log("Try_Refresh_Shop_State_OK_@_Update_to_Server");
            }

        }

        public void update_shop_state_from_server()
        {
            if (_update_shop_state_from_server != null) _update_shop_state_from_server.Invoke();
        }
        public void send_shop_state_to_server()
        {
            RefreshShopState();
            if (_send_shop_state_to_server != null) _send_shop_state_to_server.Invoke();
        }

        #endregion

        public void Load_Data_From_Json(string json)
        {
            if (json == null || json == "")
            {
                generate_default_inventory();
                Send_inventory_to_server();
            }
            else
            {
                _inventory.Load_Inventory_From_Json_Data(json,_itemDB_SO._idDic);
            }
        }
        public void generate_default_inventory()
        {
            _inventory._current_inventory = new List<Game_Item_In_Inventory>();
            List<(string, int)> defaultItems = JsonConvert.DeserializeObject<List<(string, int)>>(JsonData_Manager.Load_Single_JsonData("project_mouse_tb_default_item"));
            Change_Items_Count(defaultItems);
            //string json = JsonConvert.SerializeObject(_itemDB_SO._inventory);
            //Debug.Log(json);
        }
        public void generate_test_inventory()
        {
            _inventory._current_inventory = new List<Game_Item_In_Inventory>();
            TestInventory inventory = JsonConvert.DeserializeObject<List<TestInventory>>(JsonData_Manager.Load_Single_JsonData("project_mouse_tb_test_item"))[0];
            Change_Items_Count(inventory.low);
            Send_inventory_to_server();
        }
        public void generate_middle_inventory()
        {
            _inventory._current_inventory = new List<Game_Item_In_Inventory>();
            TestInventory inventory = JsonConvert.DeserializeObject<List<TestInventory>>(JsonData_Manager.Load_Single_JsonData("project_mouse_tb_test_item"))[0];
            Change_Items_Count(inventory.middle);
            Send_inventory_to_server();
        }
        public void generate_high_inventory()
        {
            _inventory._current_inventory = new List<Game_Item_In_Inventory>();
            TestInventory inventory = JsonConvert.DeserializeObject<List<TestInventory>>(JsonData_Manager.Load_Single_JsonData("project_mouse_tb_test_item"))[0];
            Change_Items_Count(inventory.high);
            Send_inventory_to_server();
        }

        public void Generate_Full_Inventory()
        {
            _inventory.Generate_Full_Inventory(GameItem_DB);
        }
        /// <summary>
        /// 序列化shop_state为JSON字符串，TS上传服务器使用
        /// </summary>
        /// <returns></returns>
        public string get_shop_state_json()
        {
            var _str = GF_SP.SerializeObject(_shop_state);
            return _str;
        }

        public static string Inventory_Serialization()
        {
            return GF_SP.SerializeObject(_instance._inventory);
        }
    }
    public class TestInventory
    {
        public List<(string, int)> low;
        public List<(string, int)> middle;
        public List<(string, int)> high;
    }
}