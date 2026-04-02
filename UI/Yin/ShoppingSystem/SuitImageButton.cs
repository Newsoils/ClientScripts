using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class SuitImageButton : MonoBehaviour
            {
                [Header("选择状态")]
                public Button button;
                public Image image;
                public TMP_Text price;
                public TMP_Text itemName;
                public Color selectedColor;
                public Color deselectedColor;
                public bool isSelected = false;

                [Header("物品信息")]
                public Game_Item_Info itemInShop;
                public LimitMagazineInteracManager limitMagazineInteracManager;

                [Header("物品名")]
                public string suitItemName;

                private void Start()
                {

                }

                public void InitGameItemUnit(LimitMagazineInteracManager manager)
                {
                    StartCoroutine(Initialize(manager));
                }

                public IEnumerator Initialize(LimitMagazineInteracManager manager)
                {
                    image = GetComponent<Image>();
                    button = GetComponent<Button>();

                    yield return null;

                    InitializeItemFromShopList();

                    yield return null;

                    limitMagazineInteracManager = manager;
                    itemName.text = itemInShop.name;
                    price.text = $"{itemInShop.sell_price.ToString()}{itemInShop.currency_unit}";

                    button.onClick.RemoveAllListeners();

                    DeSelectItem();

                    button.onClick.AddListener(ToggleSelection);
                }

                public void InitializeItemFromShopList()
                {
                    if (ShoppingCanvasInteractManager.Instance.shopListDB == null || string.IsNullOrEmpty(suitItemName))
                    {
                        Debug.LogWarning("Shop_List_DB_SO 或 suitItemName 未正确设置！");
                        return;
                    }

                    Game_Item_Info matchingItem = Global_Inventory_Manager.GameItem_DB.Find(item => item.name == suitItemName);
                    if (matchingItem != null)
                    {
                        itemInShop = matchingItem;
                    }
                    else
                    {
                        Debug.LogWarning($"未能找到物品名为 '{suitItemName}' 的 shop_item！");
                    }
                }

                public void ToggleSelection()
                {
                    limitMagazineInteracManager.OnSelectItem(this);
                }

                public void SelectItem()
                {
                    image.color = selectedColor;
                    isSelected = true;
                }
                public void DeSelectItem()
                {
                    image.color = deselectedColor;
                    isSelected = false;
                }
            }
        }
    }
}