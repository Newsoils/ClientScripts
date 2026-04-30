using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Core.LYC.TaskSystem;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
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
                public TMP_Text itemPrice;
                public Image priceIcon;
                public List<Sprite> priceIconSprites;
                public Image background;
                public List<Sprite> backgroundIcons;


                public Button detailButton;

                [Header("选择状态")]
                public Button itemButton;
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

                private const string PurchasedButtonLabel = "已购买";
                private static readonly Color32 BuyButtonTextColorPurchasable = new Color32(0x89, 0x89, 0x89, 0xFF);
                private static readonly Color32 BuyButtonTextColorUnavailable = new Color32(0xCE, 0xCE, 0xCE, 0xFF);

                private void Start()
                {
                    EvtDsp.AddEvt(EvtNames.RefreshUI, RefreshBuyButtonState);
                    RefreshBuyButtonState();

                    detailButton.onClick.AddListener(() =>
                        UIManager.Instance.OpenPanel<ItemDescriptionPanel>(itemInShop)
                    );
                }

                private void OnDestroy()
                {
                    EvtDsp.RemoveEvt(EvtNames.RefreshUI, RefreshBuyButtonState);
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
                    if (itemInShop.sell_price < 0)
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

                public void InitGameItemUnit(Game_Item_Info itemInShop)
                {
                    this.itemInShop = itemInShop;
                    itemName.text = itemInShop.name;
                    itemPrice.text = $"{itemInShop.sell_price.ToString()}";

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
                    buyButton.onClick.RemoveAllListeners();
                    buyButton.onClick.AddListener(OnClick);
                    DeSelectItem();
                    RefreshBuyButtonState();
                }
                public void OnClick()
                {
                    if (!ComputeCanPurchase())
                        return;
                    int price = itemInShop.sell_price;
                    List<(string, int)> items = new List<(string, int)> { (itemInShop.name, 1) };
                    string message = $"是否要花费{price}{itemInShop.currency_unit}购买{itemInShop.name}物品";

                    PromptMessage.Instance.ShowPrompt(message, () =>
                    {
                        Action<string> onPayComplete = (string result) =>
                        {
                            if (result == "success")
                            {
                                PromptMessage.Instance.ShowUpPrompt("购买成功");
                                Global_Inventory_Manager.Change_Items_Count(items, "商店购买");
                                ExpManager.TempAddExpForRoomPlacementGains(items);
                                EvtDsp.TriggerEvt(EvtNames.RefreshUI);

                                if (itemInShop.type == Project_Mouse.ENUM.Item_Type.Food)
                                {
                                    TaskTriggers.TriggerEventOfMultipleOperations(2, 1);
                                }
                            }
                            else
                            {
                                PromptMessage.Instance.ShowUpPrompt(result);
                            }
                        };
                        MoneyManager.Instance.ChangeCurrency(itemInShop.currency_unit, -price, "购买物品", onPayComplete);
                    });
                }
                public void SetPriceIcon()
                {
                    if (itemInShop.currency_unit == "鱼币")
                    {
                        priceIcon.sprite = priceIconSprites[0];
                    }
                    else
                    {
                        priceIcon.sprite = priceIconSprites[1];
                    }
                }
                public void SetIcon(Sprite sprite)
                {
                    icon.sprite = sprite;
                }
                public void SetBackgroundSprite()
                {
                    if(background == null) return;
                    background.sprite = backgroundIcons[((int)itemInShop.rarity)-1];
                }

                public void SelectItem()
                {
                    bgImage.color = selectedColor;
                }
                public void DeSelectItem()
                {
                    bgImage.color = deselectedColor;
                }
            }
        }
    }
}
