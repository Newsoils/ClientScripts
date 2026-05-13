using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel.Dispatch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class IllustratePanel : UIPanelBase
    {
        public GameObject obj;

        [Header("图鉴数据")]
        public string currentCategory;
        public List<Game_Item_Info> currentGameItemList = new List<Game_Item_Info>();
        public List<Game_Item_Info> obtainedGameItemList = new List<Game_Item_Info>();
        public List<Map_Info> currentMapInfoList = new List<Map_Info>();
        public List<Map_Info> visitedMapList = new List<Map_Info>();

        [Header("UI")]
        public GameObject illustratePanel;
        public GameObject SmallItemGridView;
        public GameObject MediumItemGridView;
        public GameObject BigIllustrateItem;
        public IllustrateSlider illustrateSlider;
        public GameObject smallIllustrateItem;
        public GameObject smallIllustrateItemRoot;
        public GameObject mediumIllustrateItem;
        public GameObject mediumIllustrateItemRoot;
        public GameObject mediumItem;
        public Image mediumItemImage;
        public TMP_Text mediumItemName;
        public BigIllustrateItem bigIllustrateItem;


        public override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
            List<Map_Info> maps = Dispatch_Manager._instance._dispatch_configuration_so._dispatch_config.map_info_list;
            currentMapInfoList = maps;
            bigIllustrateItem.maps = maps;
        }


        #region 接口方法实现

        // 打开图鉴界面
        public override void OpenPanel(params object[] data)
        {
            obj.SetActive(true);
            MainPanel.CloseMainFuncP();
        }

        // 关闭图鉴界面
        public override void ClosePanel()
        {
            illustratePanel.SetActive(false);
            MainPanel.OpenMainFuncP();
        }

        #endregion


        #region 内部方法

        private void OnChooosePlantOrFood(ENUM.Item_Type item_Type)
        {
            List<Game_Item_Info> items = new List<Game_Item_Info>();
            foreach (var item in Global_Inventory_Manager.GameItem_DB)
            {
                if (item.type == item_Type)
                {
                    items.Add(item);
                }
            }
            int needCount = items.Count;
            currentGameItemList = items;
            if (illustrateSlider.currentStep == 0)
            {
                Dictionary<int, SmallIllustrateItem> smallItemDict = new Dictionary<int, SmallIllustrateItem>();
                int childCount = smallIllustrateItemRoot.transform.childCount;

                for (int i = 0; i < needCount; i++)
                {
                    GameObject itemObj;
                    if (i < childCount)
                    {
                        itemObj = smallIllustrateItemRoot.transform.GetChild(i).gameObject;
                        itemObj.SetActive(true);
                    }
                    else
                    {
                        itemObj = Instantiate(smallIllustrateItem, smallIllustrateItemRoot.transform);
                        itemObj.SetActive(true);
                    }
                    SmallIllustrateItem item = itemObj.GetComponent<SmallIllustrateItem>();
                    item.item_id = items[i].item_id;
                    item.item_name = items[i].name;
                    smallItemDict[item.item_id] = item;
                    item.InitItem();
                }

                for (int i = needCount; i < childCount; i++)
                {
                    smallIllustrateItemRoot.transform.GetChild(i).gameObject.SetActive(false);
                }

                foreach (var inventoryItem in Global_Inventory_Manager.Items)
                {
                    if (inventoryItem.item_info.type == item_Type)
                    {
                        obtainedGameItemList.Add(inventoryItem.item_info);
                        SmallIllustrateItem item = smallItemDict[inventoryItem.item_info.item_id];
                        item.itemNameText.text = item.item_name;
                        // 设置icon
                    }
                }

                bigIllustrateItem.obtainedItems = obtainedGameItemList;


                foreach (var inventoryItem in Global_Inventory_Manager.New_Obtain_Items)
                {
                    var inventory = Global_Inventory_Manager.GameItem_DB.Find(x => x.name == inventoryItem);
                    if (inventory.type == item_Type)
                    {
                        SmallIllustrateItem item = smallItemDict[inventory.item_id];
                        item.redPoint.SetActive(true);
                    }
                }
            }
            else if (illustrateSlider.currentStep == 0.5f)
            {
                Dictionary<int, MediumIllustrateItem> mediumItemDict = new Dictionary<int, MediumIllustrateItem>();
                int childCount = mediumIllustrateItemRoot.transform.childCount;

                for (int i = 0; i < needCount; i++)
                {
                    GameObject itemObj;
                    if (i < childCount)
                    {
                        itemObj = mediumIllustrateItemRoot.transform.GetChild(i).gameObject;
                        itemObj.SetActive(true);
                    }
                    else
                    {
                        itemObj = Instantiate(mediumIllustrateItem, mediumIllustrateItemRoot.transform);
                        itemObj.SetActive(true);
                    }
                    MediumIllustrateItem item = itemObj.GetComponent<MediumIllustrateItem>();
                    item.item_id = items[i].item_id;
                    item.item_name = items[i].name;
                    mediumItemDict[item.item_id] = item;
                    item.InitItem();
                }

                for (int i = needCount; i < childCount; i++)
                {
                    mediumIllustrateItemRoot.transform.GetChild(i).gameObject.SetActive(false);
                }

                foreach (var inventoryItem in Global_Inventory_Manager.Items)
                {
                    if (inventoryItem.item_info.type == item_Type)
                    {
                        obtainedGameItemList.Add(inventoryItem.item_info);
                        MediumIllustrateItem item = mediumItemDict[inventoryItem.item_info.item_id];
                        item.itemNameText.text = item.item_name;
                        // 设置icon
                    }
                }

                bigIllustrateItem.obtainedItems = obtainedGameItemList;

                foreach (var inventoryItem in Global_Inventory_Manager.New_Obtain_Items)
                {
                    var inventory = Global_Inventory_Manager.GameItem_DB.Find(x => x.name == inventoryItem);
                    if (inventory.type == item_Type)
                    {
                        MediumIllustrateItem item = mediumItemDict[inventory.item_id];
                        item.redPoint.SetActive(true);
                    }
                }
            }
            else if (illustrateSlider.currentStep == 1)
            {
                bigIllustrateItem.SetItems(items, 0);
            }
        }

        #endregion

        // 外部挂载
        public void OpenPanel()
        {
            this.OpenPanel(null);
        }


        public void OnChooseCategory(string category)
        {
            currentCategory = category;
            if (category == "Plant")
            {
                OnChooosePlantOrFood(ENUM.Item_Type.Harvest);
            }
            else if (category == "Food")
            {
                OnChooosePlantOrFood(ENUM.Item_Type.Food);
            }
            else if (category == "Gift")
            {
                if (illustrateSlider.currentStep == 0)
                {

                }
                else if (illustrateSlider.currentStep == 0.5f)
                {

                }
                else if (illustrateSlider.currentStep == 1)
                {

                }
            }
            else if (category == "Card")
            {
                int needCount = currentMapInfoList.Count;
                if (illustrateSlider.currentStep == 0)
                {
                    Dictionary<int, SmallIllustrateItem> smallItemDict = new Dictionary<int, SmallIllustrateItem>();
                    int childCount = smallIllustrateItemRoot.transform.childCount;

                    for (int i = 0; i < needCount; i++)
                    {
                        GameObject itemObj;
                        if (i < childCount)
                        {
                            itemObj = smallIllustrateItemRoot.transform.GetChild(i).gameObject;
                            itemObj.SetActive(true);
                        }
                        else
                        {
                            itemObj = Instantiate(smallIllustrateItem, smallIllustrateItemRoot.transform);
                            itemObj.SetActive(true);
                        }
                        SmallIllustrateItem item = itemObj.GetComponent<SmallIllustrateItem>();
                        item.item_id = currentMapInfoList[i].map_id;
                        item.item_name = currentMapInfoList[i].map_name;
                        smallItemDict[item.item_id] = item;
                        item.InitItem();
                    }

                    for (int i = needCount; i < childCount; i++)
                    {
                        smallIllustrateItemRoot.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    foreach (var map in Dispatch_Manager._instance._player_dispatch_state._map_visited)
                    {
                        visitedMapList.Add(currentMapInfoList.Find(x => x.map_name == map));
                        Map_Info map_Info = currentMapInfoList.Find(x => x.map_name == map);
                        SmallIllustrateItem item = smallItemDict[map_Info.map_id];
                        item.itemNameText.text = item.item_name;
                        // 设置icon
                    }

                    bigIllustrateItem.obtainedMaps = visitedMapList;

                    foreach (var map in Dispatch_Manager._instance._player_dispatch_state._new_visited_map)
                    {
                        Map_Info map_Info = currentMapInfoList.Find(x => x.map_name == map);
                        SmallIllustrateItem item = smallItemDict[map_Info.map_id];
                        item.redPoint.SetActive(true);
                    }
                }
                else if (illustrateSlider.currentStep == 0.5f)
                {
                    Dictionary<int, MediumIllustrateItem> mediumItemDict = new Dictionary<int, MediumIllustrateItem>();
                    int childCount = mediumIllustrateItemRoot.transform.childCount;

                    for (int i = 0; i < needCount; i++)
                    {
                        GameObject itemObj;
                        if (i < childCount)
                        {
                            itemObj = mediumIllustrateItemRoot.transform.GetChild(i).gameObject;
                            itemObj.SetActive(true);
                        }
                        else
                        {
                            itemObj = Instantiate(mediumIllustrateItem, mediumIllustrateItemRoot.transform);
                            itemObj.SetActive(true);
                        }
                        MediumIllustrateItem item = itemObj.GetComponent<MediumIllustrateItem>();
                        item.item_id = currentMapInfoList[i].map_id;
                        item.item_name = currentMapInfoList[i].map_name;
                        mediumItemDict[item.item_id] = item;
                        item.InitItem();
                    }

                    for (int i = needCount; i < childCount; i++)
                    {
                        mediumIllustrateItemRoot.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    foreach (var map in Dispatch_Manager._instance._player_dispatch_state._map_visited)
                    {
                        visitedMapList.Add(currentMapInfoList.Find(x => x.map_name == map));
                        Map_Info map_Info = currentMapInfoList.Find(x => x.map_name == map);
                        MediumIllustrateItem item = mediumItemDict[map_Info.map_id];
                        item.itemNameText.text = item.item_name;
                        // 设置icon
                    }

                    bigIllustrateItem.obtainedMaps = visitedMapList;

                    foreach (var map in Dispatch_Manager._instance._player_dispatch_state._new_visited_map)
                    {
                        Map_Info map_Info = currentMapInfoList.Find(x => x.map_name == map);
                        MediumIllustrateItem item = mediumItemDict[map_Info.map_id];
                        item.redPoint.SetActive(true);
                    }
                }
                else if (illustrateSlider.currentStep == 1)
                {
                    bigIllustrateItem.ShowItem(0);
                }
            }
        }

        // 移除新获得的物品
        public void RemoveNewObtainedItem(string itemName)
        {
            var newOBts = Global_Inventory_Manager.New_Obtain_Items;
            if (newOBts != null )
            {
                newOBts.Remove(itemName);
            }
            if (Dispatch_Manager._instance != null && Dispatch_Manager._instance._player_dispatch_state != null)
            {
                Dispatch_Manager._instance._player_dispatch_state._new_visited_map.Remove(itemName);
            }
        }

        // 显示小图鉴视图
        public void ShowSmallItemGridView()
        {
            SmallItemGridView.SetActive(true);
            MediumItemGridView.SetActive(false);
            BigIllustrateItem.SetActive(false);
        }

        // 显示中图鉴视图
        public void ShowMediumItemGridView()
        {
            SmallItemGridView.SetActive(false);
            MediumItemGridView.SetActive(true);
            BigIllustrateItem.SetActive(false);
        }

        // 显示大图鉴视图
        public void ShowBigIllustrateItem()
        {
            SmallItemGridView.SetActive(false);
            MediumItemGridView.SetActive(false);
            BigIllustrateItem.SetActive(true);
        }
    }
}
