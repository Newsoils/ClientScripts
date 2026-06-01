using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Network;
using Cmd;
using Common;
using Newtonsoft.Json;
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
        public GameObject shopItemPrefab;
        public GameObject gachaItemPrefab;
        public Transform clothItemListParent;
        public Transform furnitureItemListParent;
        private readonly List<ShoppingItemUnit> itemSelected = new List<ShoppingItemUnit>();
        private readonly List<ShopGoodsViewData> limitedClothes1 = new List<ShopGoodsViewData>();
        private readonly List<ShopGoodsViewData> limitedClothes2 = new List<ShopGoodsViewData>();
        private ShopGoodsViewData limitedClothesSuit1;
        private ShopGoodsViewData limitedClothesSuit2;
        private ShopGoodsViewData currentSuitGoods;
        private Cmd.UserShop serverClothShop1;
        private Cmd.UserShop serverClothShop2;
        public List<Sprite> previewClothesSprites;
        public List<Sprite> previewFurnitureSprites;

        [Header("面板")]
        public GameObject clothesPanel;
        public GameObject furniturePanel;

        [Header("许愿薄荷")]
        public Button btnBuyColorMint;
        public TMP_Text txtColorMintCount;

        [Header("抽卡")]
        private int currentPool;
        private bool pendingSingleMintPurchase;
        private int pendingGachaAfterMintPullCount;
        private static List<GachaPoolDisplayConfig> gachaPoolDisplayConfigs;

        private const string ColorMintItemName = "许愿薄荷";
        private const string GachaPoolConfigFileName = "project_mouse_tb_gacha_pool";

        private class GachaPoolDisplayConfig
        {
            [JsonProperty("pool_id")]
            public int PoolId;

            [JsonProperty("items")]
            public List<int> Items;
        }

        /// <summary>用罐罐补足许愿薄荷时的单价（枚），读表 sell_price，缺省 60。</summary>
        private static int GetColorMintPriceInCans()
        {
            var info = Global_Inventory_Manager.GetItemInfo(ColorMintItemName);
            return info != null && info.sell_price > 0 ? info.sell_price : 60;
        }

        private static int GetMintCostForPullCount(int pullCount) => pullCount;

        private static int GetColorMintHeld()
        {
            var inv = Global_Inventory_Manager.GetItem(ColorMintItemName);
            return inv == null ? 0 : inv._item_count;
        }

        private void Start()
        {
            InitButtons();
            curBtn = null;
            btnFirstClothes.onClick?.Invoke();
        }
        #region 按钮
        private void InitButtons()
        {
            btnFirstClothes.onClick.AddListener(() =>
            {
                if (curBtn == btnFirstClothes) return;
                curBtn = btnFirstClothes;
                ClickClothesBtn(1);
                SetButtonAlpha(btnFirstClothes);
                previewClothes.sprite = previewClothesSprites[0];
            });
            btnSecondClothes.onClick.AddListener(() =>
            {
                if (curBtn == btnSecondClothes) return;
                curBtn = btnSecondClothes;
                ClickClothesBtn(2);
                SetButtonAlpha(btnSecondClothes);
                previewClothes.sprite = previewClothesSprites[1];
            });
            btnFirstFurniture.onClick.AddListener(() =>
            {
                if (curBtn == btnFirstFurniture) return;
                curBtn = btnFirstFurniture;
                currentPool = 1;
                ClickFurnitureBtn(currentPool);
                SetButtonAlpha(btnFirstFurniture);
                previewFurniture.sprite = previewFurnitureSprites[0];
            });
            btnSecondFurniture.onClick.AddListener(() =>
            {
                if (curBtn == btnSecondFurniture) return;
                curBtn = btnSecondFurniture;
                currentPool = 2;
                ClickFurnitureBtn(currentPool);
                SetButtonAlpha(btnSecondFurniture);
                previewFurniture.sprite = previewFurnitureSprites[1];
            });
            btnExit.onClick.AddListener(ExitButton);
            btnGachaOne.onClick.AddListener(GachaOneBtn);
            btnGachaFive.onClick.AddListener(GachaFiveBtn);
            btnBuyAllItem.onClick.AddListener(BuyAllItems);
            btnBuyColorMint.onClick.AddListener(OnBuyColorMintClicked);
            EvtDsp.AddEvt(EvtNames.RefreshUI, OnRefreshUi);
            EvtDsp.AddEvt<BuyTicketsRes>(EvtNames.OnBuyTicketsReceived, OnBuyTicketsRes);
            OnRefreshUi();
        }
        private void OnDestroy()
        {
            EvtDsp.RemoveEvt(EvtNames.RefreshUI, OnRefreshUi);
            EvtDsp.RemoveEvt<BuyTicketsRes>(EvtNames.OnBuyTicketsReceived, OnBuyTicketsRes);
        }

        private void OnRefreshUi()
        {
            CheckBuyAllItemState();
            RefreshColorMintCountDisplay();
        }

        private void RefreshColorMintCountDisplay()
        {
            txtColorMintCount.text = GetColorMintHeld().ToString();
        }

        private void OnBuyColorMintClicked()
        {
            int price = GetColorMintPriceInCans();
            PromptMessage.Instance.ShowPrompt($"是否花费{price}罐罐购买1枚许愿薄荷？", BuyOneColorMintWithTicketsReq);
        }

        private void BuyOneColorMintWithTicketsReq()
        {
            if (!TryGetColorMintItemId(out long itemId))
                return;

            pendingSingleMintPurchase = true;
            NetWork_Center_WSS.SendMsg(new BuyTicketsReq
            {
                ItemID = itemId,
                Count = 1
            });
        }

        private bool TryGetColorMintItemId(out long itemId)
        {
            var info = Global_Inventory_Manager.GetItemInfo(ColorMintItemName);
            if (info == null)
            {
                itemId = 0;
                PromptMessage.Instance.ShowUpPrompt("许愿薄荷配置不存在");
                return false;
            }

            itemId = info.item_id;
            return true;
        }

        private void OnBuyTicketsRes(BuyTicketsRes res)
        {
            if (pendingSingleMintPurchase)
            {
                pendingSingleMintPurchase = false;
                PromptMessage.Instance.ShowUpPrompt("购买成功");
                RefreshColorMintCountDisplay();
                return;
            }

            if (pendingGachaAfterMintPullCount > 0)
            {
                int pullCount = pendingGachaAfterMintPullCount;
                pendingGachaAfterMintPullCount = 0;
                SendGachaRequestToServer(pullCount);
            }
        }
        private void BuyAllItems()
        {
            if (currentSuitGoods == null)
                return;

            ulong shopUID = GetCurrentShopUID();
            if (shopUID == 0UL)
            {
                PromptMessage.Instance.ShowUpPrompt("商店数据未同步，请重新打开商店");
                return;
            }

            int price = currentSuitGoods.CostCount;
            string currencyName = string.IsNullOrEmpty(currentSuitGoods.CurrencyName) ? "鱼币" : currentSuitGoods.CurrencyName;
            PromptMessage.Instance.ShowPrompt($"是否花费{price}{currencyName}购买整套？", () =>
            {
                NetWork_Center_WSS.SendMsg(new BuyGoodsReq
                {
                    ShopUID = shopUID,
                    GoodsID = currentSuitGoods.GoodsID,
                    BuyAmount = 1
                });
            });
        }
        private ulong GetCurrentShopUID()
        {
            if (currentSuitGoods == limitedClothesSuit1)
                return serverClothShop1.ShopUID;
            if (currentSuitGoods == limitedClothesSuit2)
                return serverClothShop2.ShopUID;
            return 0UL;
        }
        private void CheckBuyAllItemState()
        {
            RefreshCurrentSuitGoods();
            bool isCanBuy = currentSuitGoods != null && currentSuitGoods.CostCount > 0 && currentSuitGoods.LeftBuyTimes != 0;
            SetBuyItemState(isCanBuy);
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
        private void ClickClothesBtn(int suitIndex)
        {
            clothesPanel.SetActive(true);
            furniturePanel.SetActive(false);
            RefreshClothesItemCell(suitIndex);
            RefreshCurrentSuitGoods();
            CheckBuyAllItemState();
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }
        private void ClickFurnitureBtn(int poolId)
        {
            clothesPanel.SetActive(false);
            furniturePanel.SetActive(true);
            currentSuitGoods = null;
            RefreshFurnitureItemCell(poolId);
            CheckBuyAllItemState();
            EvtDsp.TriggerEvt(EvtNames.RefreshUI);
        }
        private void RefreshCurrentSuitGoods()
        {
            if (curBtn == btnFirstClothes)
                currentSuitGoods = limitedClothesSuit1;
            else if (curBtn == btnSecondClothes)
                currentSuitGoods = limitedClothesSuit2;
            else
                currentSuitGoods = null;

            var items = curBtn == btnFirstClothes ? limitedClothes1 : curBtn == btnSecondClothes ? limitedClothes2 : null;
            if (currentSuitGoods != null && items != null)
                currentSuitGoods.SetCostCount(ShopConfigResolver.CalculateSuitRemainPrice(currentSuitGoods.ShopItem, items));
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
        private void GachaOneBtn()
        {
            TryStartGacha(1);
        }

        private void GachaFiveBtn()
        {
            TryStartGacha(5);
        }

        private void SendGachaRequestToServer(int pullCount)
        {
            if (GachaManager.Instance == null || !GachaManager.Instance.TryGetRecordUIDByPoolID(currentPool, out ulong recordUID))
            {
                PromptMessage.Instance.ShowUpPrompt("奖池数据未同步，请稍后再试");
                return;
            }

            var req = new GachaTakeReq
            {
                RecordUID = recordUID,
                Times = pullCount
            };
            NetWork_Center_WSS.SendMsg(req);
        }

        private void TryStartGacha(int pullCount)
        {
            int mintCost = GetMintCostForPullCount(pullCount);
            int haveMint = GetColorMintHeld();
            string pullLabel = pullCount == 1 ? "单" : "五";

            if (haveMint >= mintCost)
            {
                PromptMessage.Instance.ShowPrompt($"是否花费{mintCost}枚许愿薄荷进行{pullLabel}次许愿？", () => SendGachaRequestToServer(pullCount));
                return;
            }

            int shortfall = mintCost - haveMint;
            int cansPerMint = GetColorMintPriceInCans();
            int cansNeeded = shortfall * cansPerMint;

            PromptMessage.Instance.ShowPrompt(
                $"许愿薄荷不足，是否花费{cansNeeded}罐罐购买{shortfall}枚许愿薄荷并进行{pullLabel}次许愿？",
                () => BuyMintWithTicketsThenGacha(pullCount, shortfall));
        }

        private void BuyMintWithTicketsThenGacha(int pullCount, int shortfall)
        {
            if (!TryGetColorMintItemId(out long itemId))
                return;

            pendingGachaAfterMintPullCount = pullCount;
            NetWork_Center_WSS.SendMsg(new BuyTicketsReq
            {
                ItemID = itemId,
                Count = shortfall
            });
        }

        #endregion

        #region 商品列表
        public void ApplyServerShopData(Cmd.UserShop clothShop1, Cmd.UserShop clothShop2)
        {
            serverClothShop1 = clothShop1;
            serverClothShop2 = clothShop2;
            RefreshLimitedClothesCache(serverClothShop1, limitedClothes1, out limitedClothesSuit1);
            RefreshLimitedClothesCache(serverClothShop2, limitedClothes2, out limitedClothesSuit2);

            if (curBtn == btnFirstClothes)
                RefreshClothesItemCell(1);
            else if (curBtn == btnSecondClothes)
                RefreshClothesItemCell(2);
        }

        private void RefreshLimitedClothesCache(Cmd.UserShop shop, List<ShopGoodsViewData> target, out ShopGoodsViewData suitGoods)
        {
            target.Clear();
            suitGoods = null;
            if (shop == null)
                return;

            UserShopGoods suitServerGoods = null;
            foreach (var goods in shop.Goodss)
            {
                var shopItem = ShopConfigResolver.GetShopItemConfig(goods.GoodsID);
                if (shopItem == null)
                    continue;

                if (shopItem.showType == 2)
                {
                    suitServerGoods = goods;
                    continue;
                }

                if (ShopConfigResolver.TryCreateGoodsViewData(goods, out var viewData))
                    target.Add(viewData);
            }

            target.Sort((a, b) =>
            {
                int orderCompare = ShopConfigResolver.GetSortOrder(a).CompareTo(ShopConfigResolver.GetSortOrder(b));
                return orderCompare != 0 ? orderCompare : ShopConfigResolver.GetStableId(a).CompareTo(ShopConfigResolver.GetStableId(b));
            });

            if (suitServerGoods != null)
                ShopConfigResolver.TryCreateSuitGoodsViewData(suitServerGoods, target, out suitGoods);
        }

        private void ClearItemList(Transform itemListParent)
        {
            foreach (Transform child in itemListParent)
                Destroy(child.gameObject);
        }

        private void RefreshClothesItemCell(int suitIndex)
        {
            itemSelected.Clear();
            ClearItemList(clothItemListParent);

            var clothes = suitIndex == 1 ? limitedClothes1 : limitedClothes2;
            var shop = suitIndex == 1 ? serverClothShop1 : serverClothShop2;
            if (shop == null)
                return;

            foreach (var goods in clothes)
            {
                GameObject obj = Instantiate(shopItemPrefab, clothItemListParent);
                ShoppingItemUnit cell = obj.GetComponent<ShoppingItemUnit>();
                cell.useLimitMagazineBuyRule = true;
                cell.InitShopGoodsUnit(goods, shop.ShopUID);
                cell.itemButton.onClick.AddListener(() => SelectItem(cell, true));
            }
        }

        private void RefreshFurnitureItemCell(int poolId)
        {
            itemSelected.Clear();
            ClearItemList(furnitureItemListParent);

            var itemIds = GetGachaPoolItemIds(poolId);
            foreach (int itemId in itemIds)
            {
                var itemInfo = Global_Inventory_Manager.GetItemInfo(itemId);
                if (itemInfo == null)
                    continue;

                GameObject obj = Instantiate(gachaItemPrefab, furnitureItemListParent);
                ShoppingItemUnit cell = obj.GetComponent<ShoppingItemUnit>();
                cell.InitGachaPoolPreviewUnit(itemInfo);
            }
        }

        private static List<int> GetGachaPoolItemIds(int poolId)
        {
            EnsureGachaPoolDisplayConfigsLoaded();
            foreach (var config in gachaPoolDisplayConfigs)
            {
                if (config.PoolId == poolId)
                    return config.Items ?? new List<int>();
            }

            return new List<int>();
        }

        private static void EnsureGachaPoolDisplayConfigsLoaded()
        {
            if (gachaPoolDisplayConfigs != null)
                return;

            string json = JsonDataManager.Load_Single_JsonData(GachaPoolConfigFileName);
            gachaPoolDisplayConfigs = string.IsNullOrEmpty(json)
                ? new List<GachaPoolDisplayConfig>()
                : JsonConvert.DeserializeObject<List<GachaPoolDisplayConfig>>(json) ?? new List<GachaPoolDisplayConfig>();
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
            RefreshColorMintCountDisplay();
        }
        public void ClosePanel()
        {
            panelObj.SetActive(false);
        }
        #endregion
    }

}
