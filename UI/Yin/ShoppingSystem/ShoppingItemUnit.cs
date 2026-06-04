using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Cmd;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class ShoppingItemUnit : MonoBehaviour
            {
                [Header("物品信息")]
                public Game_Item_Info itemInShop;

                [Header("图标UI")]
                public Image icon;
                public TMP_Text itemName;
                [Tooltip("可选：抽奖奖池预览格没有价格标签时可留空")]
                public TMP_Text itemPrice;
                [Tooltip("可选：抽奖奖池预览格没有价格图标时可留空")]
                public Image priceIcon;
                public List<Sprite> priceIconSprites;
                public Image background;
                public List<Sprite> backgroundIcons;


                public Button detailButton;

                [Header("选择状态")]
                public Button itemButton;
                [Tooltip("可选：抽奖奖池预览格没有购买按钮时可留空")]
                public Button buyButton;
                public Image bgImage;
                public Color selectedColor;
                public Color deselectedColor;

                [Header("购买按钮状态（参考 LimitMagazineUI）")]
                [Tooltip("可买时按钮图；与 cannotBuy 都设置且能解析到 Image 时才换图")]
                public Sprite canBuy;
                [Tooltip("不可买时按钮图")]
                public Sprite cannotBuy;
                [Tooltip("用于切换精灵的 Image；留空则用 buyButton 上的 Image")]
                public Image buyButtonImage;
                [Tooltip("限购用字段；单件购买态见 ComputeCanPurchase：服装+磁带(唱片) 背包已有时不可再买")]
                public bool useLimitMagazineBuyRule;
                [Tooltip("购买按钮上的 TMP 文本；不指定则在 buyButton 子节点上查找")]
                public TMP_Text buyButtonText;
                [Tooltip("可购买时显示的文案；不可购买时固定为「已购买」")]
                public string purchasableBuyButtonLabel = "购买";

                private ulong shopUID;
                private long goodsID;
                private int leftBuyTimes = -1;
                private ShopGoodsViewData shopGoodsData;

                private const string PurchasedButtonLabel = "已购买";
                private static readonly Color32 BuyButtonTextColorPurchasable = new Color32(0x89, 0x89, 0x89, 0xFF);
                private static readonly Color32 BuyButtonTextColorUnavailable = new Color32(0xCE, 0xCE, 0xCE, 0xFF);

                private void Start()
                {
                    EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshBuyButtonState);
                    RefreshBuyButtonState();

                    if (detailButton != null)
                    {
                        detailButton.onClick.AddListener(() =>
                            UIManager.Instance.OpenPanel<ItemDescriptionPanel>(itemInShop)
                        );
                    }
                }

                private void OnDestroy()
                {
                    EvtDsp.RemoveEvt(EvtNames.RefreshUI, RefreshBuyButtonState);
                    if (detailButton != null)
                        detailButton.onClick.RemoveAllListeners();
                }

                /// <summary>根据当前数据刷新购买按钮图与 interactable；限购杂志在生成后设 useLimitMagazineBuyRule 再调一次。</summary>
                public void RefreshBuyButtonState()
                {
                    if (buyButton == null || itemInShop == null)
                        return;

                    bool canPurchase = ComputeCanPurchase();
                    ApplyBuyButtonVisualState(canPurchase);
                }

                private bool ComputeCanPurchase()
                {
                    if (leftBuyTimes == 0)
                        return false;

                    if (shopGoodsData == null && itemInShop.sell_price < 0)
                        return false;

                    if (itemInShop.type == ENUM.Item_Type.Cloth || itemInShop.type == ENUM.Item_Type.Tape)
                    {
                        var info = Global_Inventory_Manager.GetItem(itemInShop.name);
                        if (info == null) return true;
                        if (info._item_count > 0) return false;
                    }

                    return true;
                }

                private void ApplyBuyButtonVisualState(bool canPurchase)
                {
                    var img = buyButtonImage != null ? buyButtonImage : buyButton.GetComponent<Image>();
                    if (img != null && canBuy != null && cannotBuy != null)
                        img.sprite = canPurchase ? canBuy : cannotBuy;

                    buyButton.interactable = canPurchase;

                    var label = buyButtonText != null ? buyButtonText : buyButton.GetComponentInChildren<TMP_Text>(true);
                    if (label != null)
                    {
                        label.text = canPurchase ? purchasableBuyButtonLabel : PurchasedButtonLabel;
                        label.color = canPurchase ? BuyButtonTextColorPurchasable : BuyButtonTextColorUnavailable;
                    }
                }

                public void InitGameItemUnit(Game_Item_Info itemInShop, ulong serverShopUID = 0UL, long serverGoodsID = 0L, int serverLeftBuyTimes = -1)
                {
                    InitGameItemUnit(itemInShop, null, serverShopUID, serverGoodsID, serverLeftBuyTimes);
                }

                public void InitShopGoodsUnit(ShopGoodsViewData goodsData, ulong serverShopUID)
                {
                    InitGameItemUnit(goodsData.ItemInfo, goodsData, serverShopUID, goodsData.GoodsID, goodsData.LeftBuyTimes);
                }

                public void InitGachaPoolPreviewUnit(Game_Item_Info itemInfo)
                {
                    InitGameItemUnit(itemInfo);
                    if (itemPrice != null)
                        itemPrice.gameObject.SetActive(false);
                    if (priceIcon != null)
                        priceIcon.gameObject.SetActive(false);
                    if (buyButton != null)
                        buyButton.gameObject.SetActive(false);
                }

                private void InitGameItemUnit(Game_Item_Info itemInShop, ShopGoodsViewData goodsData, ulong serverShopUID, long serverGoodsID, int serverLeftBuyTimes)
                {
                    shopGoodsData = goodsData;
                    this.itemInShop = itemInShop;
                    shopUID = serverShopUID;
                    goodsID = serverGoodsID;
                    leftBuyTimes = serverLeftBuyTimes;
                    itemName.text = itemInShop.name;
                    if (itemPrice != null)
                        itemPrice.text = goodsData != null ? goodsData.CostCount.ToString() : itemInShop.sell_price.ToString();

                    if (itemInShop.res_url != null && itemInShop.res_url.Length != 0)
                    {
                        var _image_url_data = itemInShop.res_url.Split("#");
                        if (_image_url_data.Length != 2)
                        {
                            Project_Mouse_Resource_Management.Load_Sprite(_image_url_data[0], SetIcon);
                        }
                        else
                        {
                            Project_Mouse_Resource_Management.load_sub_sprite(_image_url_data[0], _image_url_data[1], SetIcon);
                        }
                    }
                    SetPriceIcon();
                    SetBackgroundSprite();
                    if (buyButtonText == null && buyButton != null)
                        buyButtonText = buyButton.GetComponentInChildren<TMP_Text>(true);
                    itemButton.onClick.RemoveAllListeners();
                    if (buyButton != null)
                    {
                        buyButton.onClick.RemoveAllListeners();
                        buyButton.onClick.AddListener(OnClick);
                    }
                    DeSelectItem();
                    RefreshBuyButtonState();
                }
                public void OnClick()
                {
                    if (!ComputeCanPurchase())
                        return;
                    if (shopUID == 0UL || goodsID == 0L)
                    {
                        PromptManager.ShowUpPrompt(PromptId.ShopDataOutOfSync);
                        return;
                    }

                    int price = shopGoodsData != null ? shopGoodsData.CostCount : itemInShop.sell_price;
                    string currencyName = shopGoodsData != null && !string.IsNullOrEmpty(shopGoodsData.CurrencyName)
                        ? shopGoodsData.CurrencyName
                        : itemInShop.currency_unit;
                    PromptManager.ShowPrompt(PromptId.BuyItemConfirm, () =>
                    {
                        var req = new BuyGoodsReq
                        {
                            ShopUID = shopUID,
                            GoodsID = goodsID,
                            BuyAmount = 1
                        };
                        NetWork_Center_WSS.SendMsg(req);
                    }, price, currencyName, itemInShop.name);
                }
                public void SetPriceIcon()
                {
                    string currencyName = shopGoodsData != null && !string.IsNullOrEmpty(shopGoodsData.CurrencyName)
                        ? shopGoodsData.CurrencyName
                        : itemInShop.currency_unit;

                    if (priceIcon == null || priceIconSprites == null || priceIconSprites.Count == 0)
                        return;

                    if (currencyName == "鱼币")
                    {
                        if (priceIconSprites.Count > 0)
                            priceIcon.sprite = priceIconSprites[0];
                    }
                    else
                    {
                        if (priceIconSprites.Count > 1)
                            priceIcon.sprite = priceIconSprites[1];
                    }
                }
                public void SetIcon(Sprite sprite)
                {
                    if (icon != null)
                        icon.sprite = sprite;
                }
                public void SetBackgroundSprite()
                {
                    if(background == null || backgroundIcons == null || backgroundIcons.Count == 0) return;
                    int index = ((int)itemInShop.rarity) - 1;
                    if (index < 0 || index >= backgroundIcons.Count) return;
                    background.sprite = backgroundIcons[index];
                }

                public void SelectItem()
                {
                    if (bgImage != null)
                        bgImage.color = selectedColor;
                }
                public void DeSelectItem()
                {
                    if (bgImage != null)
                        bgImage.color = deselectedColor;
                }
            }
        }
    }
}
