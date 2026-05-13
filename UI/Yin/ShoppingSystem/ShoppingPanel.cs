using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace CLIP.Project_Mouse.UI
{
    public class ShoppingPanel : UIPanelBase
    {
        [Header("商店数据")]
        public Shop_List_DB_SO shopListDB;

        [Header("Render Texture")]
        public RenderTexture characterRT;
        public RenderTexture furnitureRT;

        [Header("UI")]
        public GameObject obj;

        [Header("选择杂志")]
        public Button dailyMagazine;
        public Button limitMagazine;
        public GameObject chooseMagazine;

        public Button exitButton;

        [Header("购物车")]
        public GameObject shoppingCartItemPrefab;
        public Transform shoppingCartItemRoot;
        public List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();
        public GameObject shoppingCartCanvas;
        public TMP_Text totalCoin;
        public TMP_Text totalDiamond;

        [Header("面板")]
        public DailyMagazinePanel dailyMagazinePanel;
        public LimitMagazineUI limitMagazineUI;

        private GameObject dailyMagazineObj;
        private GameObject limitMagazineObj;

        private void Start()
        {
            characterRT = RenderTextureCompatUtility.EnsureCompatible(characterRT, "ShoppingCharacterRT");
            furnitureRT = RenderTextureCompatUtility.EnsureCompatible(furnitureRT, "ShoppingFurnitureRT");

            dailyMagazineObj = dailyMagazinePanel.gameObject;
            limitMagazineObj = limitMagazineUI.gameObject;

            foreach (Transform child in shoppingCartItemRoot)
            {
                shoppingCartItems.Add(child.GetComponent<ShoppingCartItem>());
            }
            OpenChooseMagazine();

            dailyMagazine.onClick.AddListener(OpenOrChooseDailyMagazine);
            limitMagazine.onClick.AddListener(OpenOrChooseLimitMagazine);

            exitButton.onClick.AddListener(ClosePanel);
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            dailyMagazine.onClick.RemoveListener(OpenOrChooseDailyMagazine);
            limitMagazine.onClick.RemoveListener(OpenOrChooseLimitMagazine);

            exitButton.onClick.RemoveAllListeners();
        }

        #region 接口方法实现


        // 打开商店面板
        public override void OpenPanel(params object[] data)
        {
            obj.SetActive(true);

            GuideManager.Instance.CheckShopFinished();
            EvtDsp.TriggerEvt(EvtNames.OnShoppingPanelOpen);
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }

        // 关闭商店面板
        public override void ClosePanel()
        {
            obj.SetActive(false);
            EvtDsp.TriggerEvt(EvtNames.OnShoppingPanelClose);
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }

        #endregion


        // 购买物品
        //public bool PurchaseItem()
        //{
        //    List<(string, int)> price = new List<(string, int)>();
        //    List<(string, int)> buyList = new List<(string, int)>();
        //    List<ShoppingCartItem> itemToRemove = new List<ShoppingCartItem>();
        //    foreach (var cartItem in shoppingCartItems)
        //    {
        //        if (cartItem.isSelected)
        //        {
        //            price.Add((cartItem.shoppingCartItem.currency_unit, cartItem.shoppingCartItem.sell_price));
        //            buyList.Add((cartItem.shoppingCartItem.name, cartItem.itemCount));
        //            itemToRemove.Add(cartItem);
        //        }
        //    }

        //    TaskTriggers.TriggerEvent(2, 1, () =>
        //    {
        //        // 是否存在物品  0 --> Food;  1 --> Tape
        //        bool[] hasItem = new bool[2];

        //        foreach (var item in itemToRemove)
        //        {
        //            if (item.shoppingCartItem.type == Project_Mouse.ENUM.Item_Type.Food)
        //            {
        //                hasItem[0] = true;
        //            }
        //            else if (item.shoppingCartItem.type == Project_Mouse.ENUM.Item_Type.Tape)
        //            {
        //                hasItem[1] = true;
        //            }
        //        }
        //        foreach (bool has in hasItem)
        //        {
        //            if (!has)
        //            {
        //                return false;
        //            }
        //        }

        //        return true;
        //    });
        //    _ = MoneyManager.Instance.ChangeCurrencyMulti(price, "商城购物", (string result) =>
        //    {
        //        if (result != "success") return;
        //        Global_Inventory_Manager.Change_Items_Count(buyList);
        //        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        //        foreach (var cartItem in itemToRemove)
        //        {
        //            if (cartItem.shoppingCartItem.type == Project_Mouse.ENUM.Item_Type.Food)
        //            {
        //                TaskTriggers.TriggerEvent(2, 1);
        //            }
        //            cartItem.DeleteItem();
        //        }
        //        CloseShoppingCart();
        //    });
        //    return true;
        //}

        // 更新购物车UI
        public void UpdateCartUI()
        {
            int totalGold = 0;
            int totalCan = 0;

            foreach (var cartItem in shoppingCartItems)
            {
                if (cartItem.isSelected)
                {
                    if (cartItem.shoppingCartItem.currency_unit == "鱼币")
                    {
                        totalGold += cartItem.shoppingCartItem.sell_price * cartItem.itemCount;
                    }
                    else if (cartItem.shoppingCartItem.currency_unit == "罐罐")
                    {
                        totalCan += cartItem.shoppingCartItem.sell_price * cartItem.itemCount;
                    }
                }
            }

            totalCoin.text = totalGold.ToString();
            totalDiamond.text = totalCan.ToString();
        }

        // 打开购物车
        public void OpenShoppingCart()
        {
            shoppingCartCanvas.SetActive(true);
            UpdateCartUI();
        }

        // 关闭购物车
        public void CloseShoppingCart()
        {
            shoppingCartCanvas.SetActive(false);
        }

        // 增加一个购物车物品
        public ShoppingCartItem CreateCartItem(Game_Item_Info item)
        {
            if (item == null) return null;

            var cartItemObject = Instantiate(shoppingCartItemPrefab, shoppingCartItemRoot);

            var cartItemComponent = cartItemObject.GetComponent<ShoppingCartItem>();
            shoppingCartItems.Add(cartItemComponent);

            cartItemComponent.shoppingCartItem = item;
            cartItemComponent.InitShoppingCartItem();

            return cartItemComponent;
        }

        // 给角色模型换装
        public void ChangeCharacterCloth(string clothName)
        {
            CharacterClothesManager.Instance.ChangeClothes(CharacterType.Shopping, clothName);
        }

        public void InitCharacterCloth()
        {
            CharacterClothesManager.Instance.InitCharacter(CharacterType.Shopping);
        }

        // 选择日刊
        public void OpenOrChooseDailyMagazine()
        {
            if (BringButtonToFront(dailyMagazine))
            {
                dailyMagazineObj.SetActive(true);
                chooseMagazine.SetActive(false);
                dailyMagazinePanel.OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory.Clothes);
                InitCharacterCloth();
            }
        }

        // 选择限定刊
        public void OpenOrChooseLimitMagazine()
        {
            if (BringButtonToFront(limitMagazine))
            {
                chooseMagazine.SetActive(false);
                limitMagazineUI.OpenPanel();
            }
        }

        public void OpenChooseMagazine()
        {
            chooseMagazine.SetActive(true);
            dailyMagazineObj.SetActive(false);
            limitMagazineObj.SetActive(false);
        }

        // 把按钮调整到最上层
        public void ChooseMagazineOrCategory(Button button)
        {
            BringButtonToFront(button);
        }

        // 将按钮调整到最上层
        public bool BringButtonToFront(Button button)
        {
            RectTransform buttonTransform = button.GetComponent<RectTransform>();

            Transform parentTransform = buttonTransform.parent;

            if (buttonTransform.GetSiblingIndex() == parentTransform.childCount - 1)
            {
                Debug.Log("按钮已经在最上层！");
                return true;
            }

            buttonTransform.SetSiblingIndex(parentTransform.childCount - 1);
            Debug.Log("按钮已调整到最上层！");
            return false;
        }

        // 设置物体的层级
        public void SetLayer(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayer(child.gameObject, layer);
            }
        }


    }

}
