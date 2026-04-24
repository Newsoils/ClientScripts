using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.NewFrame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class LimitMagazineUI : MonoBehaviour
    {
        public GameObject panelObj;

        public Button btnFirstClothes;
        public Button btnSecondClothes;
        public Button btnFirstFurniture;
        public Button btnSecondFurniture;
        public Button btnExit;
        public Button btnAddToCart1;
        public Button btnGachaOne;
        public Button btnGachaFive;
        public Button btnBuyAllItem;
        public Image btnBuyAllItemImage;
        public TMP_Text BuyAllItemText;

        private static readonly Color32 BuyButtonTextColorPurchasable = new Color32(0x89, 0x89, 0x89, 0xFF);
        private static readonly Color32 BuyButtonTextColorUnavailable = new Color32(0xCE, 0xCE, 0xCE, 0xFF);
        public Sprite canBuy;
        public Sprite cannotBuy;

        private Button curBtn;
        public Image previewFurniture;
        public Image previewClothes;

        [Header("商品")]
        public Shop_List_DB_SO shopListDB;
        public GameObject shopItemPrefab;
        public Transform clothItemListParent;
        public Transform furnitureItemListParent;
        private List<ShoppingItemUnit> itemSelected = new List<ShoppingItemUnit>();
        private List<string> curItems = new List<string>();
        public List<Sprite> previewClothesSprites;
        public List<Sprite> previewFurnitureSprites;

        [Header("面板")]
        public GameObject clothesPanel;
        public GameObject furniturePanel;
        [Header("抽卡")]
        private int currentPool;

        private void Start()
        {
            InitButtons();
            itemSelected = new List<ShoppingItemUnit>();
            curBtn = null;
            btnFirstClothes.onClick?.Invoke();
        }
        #region 按钮
        private void InitButtons()
        {
            btnFirstClothes.onClick.AddListener(() =>
            {
                if (curBtn == btnFirstClothes) return;
                ClickClothesBtn(shopListDB._limited_cloth_item_list[0].group_item_list);
                SetButtonAlpha(btnFirstClothes);
                previewClothes.sprite = previewClothesSprites[0];
                curBtn = btnFirstClothes;
            });
            btnSecondClothes.onClick.AddListener(() =>
            {
                if (curBtn == btnSecondClothes) return;
                ClickClothesBtn(shopListDB._limited_cloth_item_list[1].group_item_list);
                SetButtonAlpha(btnSecondClothes);
                previewClothes.sprite = previewClothesSprites[1];
                curBtn = btnSecondClothes;
            });
            btnFirstFurniture.onClick.AddListener(() =>
            {
                if (curBtn == btnFirstFurniture) return;
                ClickFurnitureBtn(shopListDB._limited_placement_item_list[0].group_item_list);
                SetButtonAlpha(btnFirstFurniture);
                curBtn = btnFirstFurniture;
                previewFurniture.sprite = previewFurnitureSprites[0];
                currentPool = 0;
            });
            btnSecondFurniture.onClick.AddListener(() =>
            {
                if (curBtn == btnSecondFurniture) return;
                ClickFurnitureBtn(shopListDB._limited_placement_item_list[1].group_item_list);
                SetButtonAlpha(btnSecondFurniture);
                curBtn = btnSecondFurniture;
                previewFurniture.sprite = previewFurnitureSprites[1];
                currentPool = 1;
            });
            btnExit.onClick.AddListener(ExitButton);
            btnAddToCart1.onClick.AddListener(AddToCartButton);
            //btnAddToCart2.onClick.AddListener(AddToCartButton);
            btnGachaOne.onClick.AddListener(GachaOneBtn);
            btnGachaFive.onClick.AddListener(GachaFiveBtn);
            btnBuyAllItem.onClick.AddListener(BuyAllItems);
            EvtDsp.AddEvt(EvtNames.RefreshUI, CheckBuyAllItemState);
            CheckBuyAllItemState();
        }
        private void OnDestroy()
        {
            EvtDsp.RemoveEvt(EvtNames.RefreshUI, CheckBuyAllItemState);
        }
        private void BuyAllItems()
        {
            int price = 0;
            List<(string, int)> items = new List<(string, int)>();
            List<string> itemNames = new List<string>();
            foreach (var item in curItems)
            {
                var info = Global_Inventory_Manager.GetItem(item);
                if (info == null || info._item_count <= 0)
                {
                    price += Global_Inventory_Manager.GetItemInfo(item).sell_price;
                    items.Add((item, 1));
                    itemNames.Add(item);
                }
            }

            string namesJoined = string.Join("，", itemNames);
            string message = $"是否要花费{price}罐罐购买{namesJoined}";

            PromptMessage.Instance.ShowPrompt(message, () =>
            {
                Action<string> onPayComplete = (string result) =>
                {
                    if (result == "success")
                    {
                        PromptMessage.Instance.ShowUpPrompt("购买成功");
                        Global_Inventory_Manager.Change_Items_Count(items, "商店购买");
                        EvtDsp.TriggerEvt(EvtNames.RefreshUI);
                    }
                    else
                    {
                        PromptMessage.Instance.ShowUpPrompt(result);
                    }
                };
                MoneyManager.Instance.ChangeCurrency("罐罐", -price, "购买物品", onPayComplete);
            });
        }
        private void CheckBuyAllItemState()
        {
            int price = 0;
            foreach (var item in curItems)
            {
                var info = Global_Inventory_Manager.GetItem(item);
                if (info == null || info._item_count <= 0)
                {
                    price += Global_Inventory_Manager.GetItemInfo(item).sell_price;
                }
            }
            if (price == 0)
            {
                SetBuyItemState(false);
            }
            else
            {
                SetBuyItemState(true);
            }
        }
        private void SetBuyItemState(bool isCanBuy)
        {
            if (isCanBuy)
            {
                btnBuyAllItemImage.sprite = canBuy;
                btnBuyAllItem.interactable = true;
                BuyAllItemText.text = "整套购买";
                BuyAllItemText.color = BuyButtonTextColorPurchasable;
            }
            else
            {
                btnBuyAllItemImage.sprite = cannotBuy;
                btnBuyAllItem.interactable = false;
                BuyAllItemText.text = "已购买";
                BuyAllItemText.color = BuyButtonTextColorUnavailable;
            }
        }
        private void ClickClothesBtn(List<string> items)
        {
            clothesPanel.SetActive(true);
            furniturePanel.SetActive(false);
            RefreshClothesItemCell(items, clothItemListParent, true);
            curItems = items;
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }
        private void ClickFurnitureBtn(List<string> items)
        {
            clothesPanel.SetActive(false);
            furniturePanel.SetActive(true);
            RefreshFurnitureItemCell(items, furnitureItemListParent, false);
            curItems = items;
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }
        private void SetButtonAlpha(Button button)
        {
            List<Button> buttons = new List<Button> { btnFirstClothes, btnSecondClothes, btnFirstFurniture, btnSecondFurniture };
            foreach (var b in buttons)
            {
                if (b == button)
                {
                    b.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                }
                else
                {
                    b.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
                }
            }
        }
        private void ExitButton()
        {
            var sp = UIManager.Instance.GetPanel<ShoppingPanel>();
            sp.OpenChooseMagazine();
            sp.InitCharacterCloth();
        }
        private void AddToCartButton()
        {
            foreach (var item in itemSelected)
            {

                var existingItem = UIManager.Instance.GetPanel<ShoppingPanel>().shoppingCartItems.Find(cartItem => cartItem.shoppingCartItem.name == item.itemInShop.name);

                if (existingItem != null)
                {
                    existingItem.itemCount++;
                    return;
                }
                else
                {
                    UIManager.Instance.GetPanel<ShoppingPanel>().CreateCartItem(item.itemInShop);
                }
            }

        }
        private void GachaOneBtn()
        {
            _ = GachaOneAsync();
        }
        private void GachaFiveBtn()
        {
            _ = GachaFiveAsync();
        }
        private async Task GachaOneAsync()
        {
            Action<string> action = new Action<string>((string data) =>
            {
                if (data == "罐罐不足")
                {
                    PromptMessage.Instance.ShowUpPrompt(data);
                }
                if (data == "success")
                {
                    List<int> item = GachaManager.Instance.Gacha_Multi_Pull(currentPool, 1);
                    List<(string, int)> itemNames = item.Select(x => (Global_Inventory_Manager.GetItemInfo(x).name, 1)).ToList();

                    UIManager.Instance.OpenPanel<RewardPanel>(itemNames);
                    Global_Inventory_Manager.Change_Items_Count(itemNames);
                }
            });
            await MoneyManager.Instance.ChangeCurrency("罐罐", -100, "抽奖", action);
            //action.Invoke("success");
        }
        private async Task GachaFiveAsync()
        {
            Action<string> action = new Action<string>((string data) =>
            {
                if (data == "罐罐不足")
                {
                    PromptMessage.Instance.ShowUpPrompt(data);
                }
                if (data == "success")
                {
                    List<int> item = GachaManager.Instance.Gacha_Multi_Pull(currentPool, 5);
                    List<(string, int)> itemNames = item.Select(x => (Global_Inventory_Manager.GetItemInfo(x).name, 1)).ToList();

                    UIManager.Instance.OpenPanel<RewardPanel>(itemNames);
                    Global_Inventory_Manager.Change_Items_Count(itemNames);
                }
            });
            await MoneyManager.Instance.ChangeCurrency("罐罐", -350, "抽奖", action);
        }
        #endregion

        #region 商品列表
        private void RefreshClothesItemCell(List<string> itemList, Transform itemListParent, bool isClothes)
        {
            itemSelected.Clear();
            foreach (Transform child in itemListParent)
            {
                Destroy(child.gameObject);
            }
            foreach (var item in itemList)
            {
                GameObject obj = Instantiate(shopItemPrefab, itemListParent);
                ShoppingItemUnit cell = obj.GetComponent<ShoppingItemUnit>();
                var gameItem = Global_Inventory_Manager.GameItem_DB.Find(dbItem => dbItem.name == item);
                cell.useLimitMagazineBuyRule = true;
                cell.InitGameItemUnit(gameItem);
                cell.itemButton.onClick.AddListener(() => SelectItem(cell, isClothes));
            }
        }
        private void RefreshFurnitureItemCell(List<string> itemList, Transform itemListParent, bool isClothes)
        {
            itemSelected.Clear();
            foreach (Transform child in itemListParent)
            {
                Destroy(child.gameObject);
            }
            foreach (var item in itemList)
            {
                GameObject obj = Instantiate(shopItemPrefab, itemListParent);
                ShoppingItemUnit cell = obj.GetComponent<ShoppingItemUnit>();
                var gameItem = Global_Inventory_Manager.GameItem_DB.Find(dbItem => dbItem.name == item);
                cell.useLimitMagazineBuyRule = true;
                cell.InitGameItemUnit(gameItem);
                //cell.itemButton.onClick.AddListener(() => SelectItem(cell, isClothes));
                cell.buyButton.gameObject.SetActive(false);
            }
        }
        private void SelectItem(ShoppingItemUnit cell, bool isClothes)
        {
            if (itemSelected.Contains(cell))
            {
                cell.DeSelectItem();
                itemSelected.Remove(cell);
                if (isClothes)
                {
                    CharacterClothesManager.Instance.RemoveClothes(CharacterType.Shopping, cell.itemInShop.name);
                }
            }
            else
            {
                cell.SelectItem();
                itemSelected.Add(cell);
                if (isClothes)
                {
                    CharacterClothesManager.Instance.ChangeClothes(CharacterType.Shopping, cell.itemInShop.name);
                }
            }
        }

        #endregion

        #region 面板开关
        public void OpenPanel()
        {
            curBtn = null;
            btnFirstClothes.onClick?.Invoke();
            panelObj.SetActive(true);
            UIManager.Instance.GetPanel<ShoppingPanel>().InitCharacterCloth();
            curBtn = btnFirstClothes;
        }
        public void ClosePanel()
        {
            panelObj.SetActive(false);
        }
        #endregion
    }

}
