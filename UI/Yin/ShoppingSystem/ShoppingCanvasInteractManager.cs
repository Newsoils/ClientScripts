using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.LYC.TaskSystem;
using CLIP.Project_Mouse.Client_Event_Systems;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            // 先保留，因为场景中还有 DailyMagazineInteracManager 仍在引用
            public class ShoppingCanvasInteractManager : MonoBehaviour
            {
                public static ShoppingCanvasInteractManager Instance { get; private set; }

                [Header("商店数据")]
                public Shop_List_DB_SO shopListDB;

                [Header("Render Texture")]
                public RenderTexture characterRT;
                public RenderTexture furnitureRT;

                [Header("UI")]
                public GameObject shopping;
                public GameObject shopPanel;

                [Header("选择杂志")]
                public Button dailyMagazine;
                public Button limitMagazine;
                public GameObject chooseMagazine;
                public GameObject dailyMagazineCanvas;
                public GameObject limitMagazineCanvas;


                [Header("购物车")]
                public GameObject shoppingCartItemPrefab;
                public Transform shoppingCartItemRoot;
                public List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();
                public GameObject shoppingCartCanvas;
                public TMP_Text totalCoin;
                public TMP_Text totalDiamond;

                [Header("InteractManager")]
                public DailyMagazineInteracManager dailyMagazineInteracManager;
                public LimitMagazineUI limitMagazineUI;

                [Header("Event System")]
                public Shopping_Interaction_Event_Hub_SO shoppingInteractionEventHub;

                private void Awake()
                {
                    if (Instance != null && Instance != this)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    Instance = this;
                    //DontDestroyOnLoad(gameObject);
                }

                private void Start()
                {
                    foreach (Transform child in shoppingCartItemRoot)
                    {
                        shoppingCartItems.Add(child.GetComponent<ShoppingCartItem>());
                    }
                    DontDestroyOnLoad(shopping);
                }

                public void OpenShop()
                {
                    shopPanel.SetActive(true);

                    //StartCoroutine(OpenShopCo());
                }

                //public IEnumerator OpenShopCo()
                //{
                    //CommonInteractManager.Instance.ExitPhone();

                    //yield return new WaitForSeconds(1.5f);

                    //CommonInteractManager.Instance.ClosePanelWithoutUp();
                //}

                // 购买物品
                public void PurchaseItem()
                {
                    _ = BuyItemAsync();
                }
                public async Task<bool> BuyItemAsync()
                {
                    List<(string, int)> price = new List<(string, int)>();
                    List<(string, int)> buyList = new List<(string, int)>();
                    List<ShoppingCartItem> itemToRemove = new List<ShoppingCartItem>();
                    foreach (var cartItem in shoppingCartItems)
                    {
                        if (cartItem.isSelected)
                        {
                            price.Add((cartItem.shoppingCartItem.currency_unit, cartItem.shoppingCartItem.sell_price));
                            buyList.Add((cartItem.shoppingCartItem.name, cartItem.itemCount));
                            itemToRemove.Add(cartItem);
                        }
                    }

                    // 判断是否触发某个事件
                    //TaskTriggers.TriggerEventOfMulOprByJudgeJunc("首次购买基础便当和唱片", 1, () =>
                    TaskTriggers.TriggerEventOfMulOprByJudgeJunc(2, 1, () =>
                    {
                        // 是否存在物品  0 --> Food;  1 --> Tape
                        bool[] hasItem = new bool[2];

                        foreach(var item in itemToRemove)
                        {
                            if(item.shoppingCartItem.type == Project_Mouse.ENUM.Item_Type.Food)
                            {
                                hasItem[0] = true;
                            }
                            else if(item.shoppingCartItem.type == Project_Mouse.ENUM.Item_Type.Tape)
                            {
                                hasItem[1] = true;
                            }
                        }
                        foreach(bool has in hasItem)
                        {
                            if (!has)
                            {
                                return false;
                            }
                        }

                        return true;
                    });
                    MoneyManager.Instance.ChangeCurrencyMulti(price, "商城购物", (string result) =>
                    {
                        if (result != "success") return;
                        Global_Inventory_Manager.Change_Items_Count(buyList);
                        foreach (var cartItem in itemToRemove)
                        {
                            cartItem.DeleteItem();
                        }
                        CloseShoppingCart();
                    });
                    return true;
                }
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

                // 关闭商店
                public void CloseShop()
                {
                    shopPanel.SetActive(false);
                    //if (SceneManager.GetActiveScene().name == CommonInteractManager.Instance.dispatchSceneName)
                    //{
                    //    CommonInteractManager.Instance.bTN_Main_Menu.SetActive(true);
                    //    CommonInteractManager.Instance.OpenUIPhone();
                    //}
                    //else if (SceneManager.GetActiveScene().name == CommonInteractManager.Instance.roomSceneName)
                    //{
                    //    CommonInteractManager.Instance.OpenPanelWithoutUp();
                    //}
                }

                // 给角色模型换装
                public void ChangeCharacterCloth(string clothName)
                {
                    Character_Cloth_Manager.Instance.Add_Cloth(Character_Type.Target_Character, clothName);
                }

                public void InitCharacterCloth()
                {
                    Character_Cloth_Manager.Instance.Init_ShoppongCharacter_Cloth();
                }

                // 选择日刊
                public void OpenOrChooseDailyMagazine()
                {
                    if (BringButtonToFront(dailyMagazine))
                    {
                        dailyMagazineCanvas.SetActive(true);
                        chooseMagazine.SetActive(false);
                        dailyMagazineInteracManager.OnPrimaryCategoryButtonClick("服装");
                        InitCharacterCloth();
                    }
                }

                // 选择限定刊
                public void OpenOrChooseLimitMagazine()
                {
                    if (BringButtonToFront(limitMagazine))
                    {
                        limitMagazineUI.OpenPanel();
                        chooseMagazine.SetActive(false);
                    }
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
    }
}