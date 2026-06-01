using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.Game_Play_System;
using Common;
using Newtonsoft.Json;

namespace CLIP.Project_Mouse.UI
{
    public sealed class ShopItemConfig
    {
        public int id;
        public int tabID;
        public int order;
        public int showType;
        public int type_1_1;
        public int value_1_1;
        public int size_1_1;
        public int type_2_1;
        public int value_2_1;
        public int size_2_1;
        public int limitType;
        public int limitNum;
        public int sale;
        public int label;
        public int showTypeLimit;
        public int showValue;
        public int buyType;
        public int buyValue;
        public int weight;
        public int dailyMax;
        public int cardLabel;
        public string STRVAL_tips;
        public string STRID_tips;
    }

    public sealed class ShopGoodsViewData
    {
        public UserShopGoods ServerGoods { get; }
        public ShopItemConfig ShopItem { get; }
        public Game_Item_Info ItemInfo { get; }
        public int ItemId { get; }
        public int ItemCount { get; }
        public int CostType { get; }
        public int CostItemId { get; }
        public int CostCount { get; private set; }
        public string CurrencyName { get; }
        public int LeftBuyTimes => ServerGoods.LeftBuyTimes;
        public long GoodsID => ServerGoods.GoodsID;
        public bool IsSuitGoods => ShopItem.showType == 2;

        public ShopGoodsViewData(UserShopGoods serverGoods, ShopItemConfig shopItem, Game_Item_Info itemInfo, string currencyName)
        {
            ServerGoods = serverGoods;
            ShopItem = shopItem;
            ItemInfo = itemInfo;
            ItemId = itemInfo.item_id;
            ItemCount = shopItem.size_1_1;
            CostType = shopItem.type_2_1;
            CostItemId = shopItem.value_2_1;
            CostCount = shopItem.size_2_1;
            CurrencyName = currencyName;
        }

        public void SetCostCount(int costCount)
        {
            CostCount = costCount < 0 ? 0 : costCount;
        }
    }

    public static class ShopConfigResolver
    {
        private const string ShopItemJsonName = "project_mouse_tb_shopitem";
        private static Dictionary<int, ShopItemConfig> shopItemsByGoodsId;

        public static bool TryCreateGoodsViewData(UserShopGoods goods, out ShopGoodsViewData viewData)
        {
            viewData = null;
            if (goods == null || goods.GoodsID > int.MaxValue)
                return false;

            EnsureLoaded();
            if (!shopItemsByGoodsId.TryGetValue((int)goods.GoodsID, out var shopItem))
                return false;

            if (shopItem.value_1_1 <= 0)
                return false;

            var itemInfo = Global_Inventory_Manager.GetItemInfo(shopItem.value_1_1);
            if (itemInfo == null)
                return false;

            viewData = new ShopGoodsViewData(goods, shopItem, itemInfo, ResolveCurrencyName(shopItem.value_2_1));
            return true;
        }

        public static int GetSortOrder(ShopGoodsViewData viewData)
        {
            return viewData?.ShopItem?.order ?? int.MaxValue;
        }

        public static int GetSortOrder(UserShopGoods goods)
        {
            if (goods == null || goods.GoodsID > int.MaxValue)
                return int.MaxValue;

            var shopItem = GetShopItemConfig(goods.GoodsID);
            return shopItem?.order ?? int.MaxValue;
        }

        public static int GetStableId(ShopGoodsViewData viewData)
        {
            return viewData?.ShopItem?.id ?? int.MaxValue;
        }

        public static int GetStableId(UserShopGoods goods)
        {
            if (goods == null || goods.GoodsID > int.MaxValue)
                return int.MaxValue;

            return (int)goods.GoodsID;
        }

        public static bool TryCreateSuitGoodsViewData(UserShopGoods goods, IReadOnlyList<ShopGoodsViewData> suitItems, out ShopGoodsViewData viewData)
        {
            viewData = null;
            if (goods == null || goods.GoodsID > int.MaxValue || suitItems == null || suitItems.Count == 0)
                return false;

            EnsureLoaded();
            if (!shopItemsByGoodsId.TryGetValue((int)goods.GoodsID, out var shopItem))
                return false;

            if (shopItem.showType != 2)
                return false;

            var displayItem = suitItems[0].ItemInfo;
            viewData = new ShopGoodsViewData(goods, shopItem, displayItem, ResolveCurrencyName(shopItem.value_2_1));
            viewData.SetCostCount(CalculateSuitRemainPrice(shopItem, suitItems));
            return true;
        }

        public static int CalculateSuitRemainPrice(ShopItemConfig suitShopItem, IReadOnlyList<ShopGoodsViewData> suitItems)
        {
            int boughtSingleItemsPrice = 0;
            for (int i = 0; i < suitItems.Count; i++)
            {
                var item = suitItems[i];
                if (HasItem(item.ItemInfo))
                    boughtSingleItemsPrice += item.ShopItem.size_2_1;
            }

            return suitShopItem.size_2_1 - boughtSingleItemsPrice;
        }

        public static bool HasItem(Game_Item_Info itemInfo)
        {
            var inventoryItem = Global_Inventory_Manager.GetItem(itemInfo.name);
            return inventoryItem != null && inventoryItem._item_count > 0;
        }

        public static ShopItemConfig GetShopItemConfig(long goodsID)
        {
            if (goodsID > int.MaxValue)
                return null;

            EnsureLoaded();
            shopItemsByGoodsId.TryGetValue((int)goodsID, out var shopItem);
            return shopItem;
        }

        private static void EnsureLoaded()
        {
            if (shopItemsByGoodsId != null)
                return;

            var json = JsonDataManager.Load_Single_JsonData(ShopItemJsonName);
            var list = JsonConvert.DeserializeObject<List<ShopItemConfig>>(json) ?? new List<ShopItemConfig>();
            shopItemsByGoodsId = list.ToDictionary(item => item.id, item => item);
        }

        private static string ResolveCurrencyName(int currencyItemId)
        {
            if (MoneyManager.Instance != null
                && MoneyManager.Instance.idDic != null
                && MoneyManager.Instance.idDic.TryGetValue(currencyItemId, out var currency)
                && currency != null
                && !string.IsNullOrEmpty(currency.currency_name))
            {
                return currency.currency_name;
            }

            var itemInfo = Global_Inventory_Manager.GetItemInfo(currencyItemId);
            return itemInfo != null ? itemInfo.name : string.Empty;
        }
    }
}
