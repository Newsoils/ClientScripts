using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class DailyMagazinePanel : MonoBehaviour
    {
        [Header("商店数据")]
        public GameObject shopItemPrefab;
        public Transform shopItemRoot;
        public DailyShoppingPrimaryCategory currentPrimaryCategory = DailyShoppingPrimaryCategory.None;
        public DailyShoppingSecondaryCategory currentSecondaryCategory = DailyShoppingSecondaryCategory.None;
        public Game_Item_Info currentSelectedShopItem = null;
        public ShoppingItemUnit currentSelectedShopItemUI = null;

        [Header("一级分类按钮")]
        public Button btnClothes;
        public Button btnFurniture;
        public Button btnPlanting;
        public Button btnBelonging;

        [Header("二级分类父物体")]
        public GameObject clothes;
        public GameObject furniture;
        public GameObject planting;
        public GameObject belonging;

        [Header("二级分类按钮 - 种植")]
        public Button btnPot;
        public Button btnSeed;
        public Button btnFertilizer;

        [Header("二级分类按钮 - 携带物")]
        public Button btnFood;
        public Button btnSnack;
        public Button btnTape;

        [Header("其他")]
        public Button refreshShop;

        [Header("模型展示")]
        public RawImage rawImage;
        public GameObject currentModel;
        public Camera furnitureCamera;
        public Transform modelTransform;
        public Transform modelRootTransform;
        public Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();

        public Texture shoppingDispatchItemTexture;
        public Transform dispatchItemTransRoot;

        private ShoppingPanel _shoppingPanel;
        private ShoppingPanel shoppingPanel => _shoppingPanel ??= UIManager.Instance.GetPanel<ShoppingPanel>();

        private void Start()
        {
            InitButtons();
            shoppingPanel.shopListDB.refreshCount = 0;
            OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory.Clothes);
        }

        private void InitButtons()
        {
            btnClothes.onClick.RemoveAllListeners();
            btnFurniture.onClick.RemoveAllListeners();
            btnPlanting.onClick.RemoveAllListeners();
            btnPot.onClick.RemoveAllListeners();
            btnSeed.onClick.RemoveAllListeners();
            btnFertilizer.onClick.RemoveAllListeners();
            btnFood.onClick.RemoveAllListeners();
            btnSnack.onClick.RemoveAllListeners();
            btnTape.onClick.RemoveAllListeners();

            btnClothes.onClick.AddListener(() => OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory.Clothes));
            btnFurniture.onClick.AddListener(() => OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory.Furniture));
            btnPlanting.onClick.AddListener(() => OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory.Planting));
            btnBelonging.onClick.AddListener(() => OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory.Belonging));

            btnPot.onClick.AddListener(() => OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Pot));
            btnSeed.onClick.AddListener(() => OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Seed));
            btnFertilizer.onClick.AddListener(() => OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Fertilizer));

            btnFood.onClick.AddListener(() => OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Food));
            btnSnack.onClick.AddListener(() => OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Snack));
            btnTape.onClick.AddListener(() => OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory.Tape));

            refreshShop.onClick.AddListener(RefreshItemButton);
        }

        public void AddItemToCart()
        {
            if (currentSelectedShopItem == null)
            {
                PromptMessage.Instance.ShowUpPrompt("当前没有选中的物品");
                return;
            }

            var existingItem = shoppingPanel.shoppingCartItems.Find(cartItem => cartItem.shoppingCartItem.name == currentSelectedShopItem.name);

            if (existingItem != null)
            {
                return;
            }
            else
            {
                shoppingPanel.CreateCartItem(currentSelectedShopItem);
            }
        }

        public void OnSelectItem(ShoppingItemUnit shoppingItemUnit)
        {
            Unity_Tools.ClearAllChildren(dispatchItemTransRoot);
            if (shoppingItemUnit == currentSelectedShopItemUI)
            {
                if (currentSelectedShopItemUI != null && shoppingItemUnit.itemInShop.type == Item_Type.Cloth)
                {
                    CharacterClothesManager.Instance.RemoveClothes(CharacterType.Shopping, shoppingItemUnit.itemInShop.name);
                    shoppingItemUnit.DeSelectItem();
                    DeselectItem();
                    return;
                }
                else
                {
                    return;
                }
            }

            if (currentSelectedShopItemUI != null && currentSelectedShopItem != null)
            {
                currentSelectedShopItemUI.DeSelectItem();
                DeselectItem();
            }

            currentSelectedShopItem = shoppingItemUnit.itemInShop;
            currentSelectedShopItemUI = shoppingItemUnit;

            shoppingItemUnit.SelectItem();

            switch (shoppingItemUnit.itemInShop.type)
            {
                case Item_Type.Cloth:
                    OnSelectClothingItem(shoppingItemUnit);
                    break;
                case Item_Type.Room_Placement:
                case Item_Type.Pot:
                    OnSelectFurnitureOrPot(shoppingItemUnit);
                    break;
                default:
                    currentModel?.SetActive(false);
                    _ = OnSelectOtherItem(shoppingItemUnit);
                    break;
            }
        }

        public void OnSelectClothingItem(ShoppingItemUnit shoppingItemUnit)
        {
            rawImage.gameObject.SetActive(true);
            if (rawImage != null && shoppingPanel.characterRT != null)
            {
                rawImage.texture = shoppingPanel.characterRT;
            }
            else
            {
                Debug.LogError("RawImage 或 characterRT 未正确设置！");
                return;
            }
            CharacterClothesManager.Instance.ChangeClothes(CharacterType.Shopping, shoppingItemUnit.itemInShop.name);
        }

        public void OnSelectFurnitureOrPot(ShoppingItemUnit shoppingItemUnit)
        {
        }

        public void AlignColliderCenterToModel(GameObject gameObject, BoxCollider targetCollider)
        {
            Vector3 roundedSize = new Vector3(Mathf.Ceil(targetCollider.size.x), Mathf.Ceil(targetCollider.size.y), Mathf.Ceil(targetCollider.size.z));
            Vector3 offset = new Vector3(roundedSize.x / 2f, roundedSize.y / 2f, roundedSize.z / 2f);
            gameObject.transform.position -= offset;
        }

        public void AdjustCameraToFitModel(BoxCollider targetCollider)
        {
            Bounds bounds = targetCollider.bounds;

            float maxDimension = Mathf.Max(Mathf.Ceil(bounds.size.x) + 1, Mathf.Ceil(bounds.size.y) + 1);

            float fieldOfView = furnitureCamera.fieldOfView;

            float distance = (maxDimension / 2) / Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);

            Vector3 cameraPosition = furnitureCamera.transform.position;
            cameraPosition.z = modelTransform.position.z - distance - 2f;
            furnitureCamera.transform.position = cameraPosition;

            Debug.Log($"调整摄像机位置: {cameraPosition}, 模型最大尺寸: {maxDimension}, 距离: {distance}");
        }

        public async Task OnSelectOtherItem(ShoppingItemUnit shoppingItemUnit)
        {
            var itemName = shoppingItemUnit.itemInShop.name;

            GameObject obj = null;
            Material cd_mt = null;
            var dispatch_Config = Dispatch_Manager._instance._dispatch_configuration_so._dispatch_config;

            switch (shoppingItemUnit.itemInShop.type)
            {
                case Item_Type.Food:
                    var food_Res_Name = Dispatch_Manager.GetFoodInfo(itemName)?.model_resource_name;
                    if (!string.IsNullOrEmpty(food_Res_Name)) obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(food_Res_Name);
                    break;
                case Item_Type.Snack:
                    var snack_Res_Name = Dispatch_Manager.GetSnackInfo(itemName)?.model_resource_name;
                    if (!string.IsNullOrEmpty(snack_Res_Name)) obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(snack_Res_Name);
                    break;
                case Item_Type.Tape:
                    obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(ResKeys.PREFAB_CD_MODEL);
                    var cd_mat_name = Dispatch_Manager.GetCDInfo(itemName)?.mat_resource_name;
                    if (!string.IsNullOrEmpty(cd_mat_name)) cd_mt = await GameAssets.Instance.LoadAsycByKey<Material>(cd_mat_name);
                    break;
                default:
                    rawImage.texture = null;
                    rawImage.gameObject.SetActive(false);
                    break;
            }

            if (obj != null)
            {
                Unity_Tools.ClearAllChildren(dispatchItemTransRoot);
                rawImage.gameObject.SetActive(true);
                rawImage.texture = shoppingDispatchItemTexture;
                GameObject _obj = Instantiate(obj, dispatchItemTransRoot.position, Quaternion.identity);
                _obj.transform.parent = dispatchItemTransRoot;
                _obj.transform.localEulerAngles = Vector3.zero;
            }
            if (cd_mt != null)
            {
                var cd_obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(ResKeys.PREFAB_CD_MODEL);
                if (cd_obj != null)
                {
                    rawImage.gameObject.SetActive(true);
                    rawImage.texture = shoppingDispatchItemTexture;
                    Unity_Tools.ClearAllChildren(dispatchItemTransRoot);
                    var meshRenderer = cd_obj.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer != null) meshRenderer.material = cd_mt;
                    Instantiate(cd_obj, dispatchItemTransRoot);
                }
            }
        }

        public void OnPrimaryCategoryButtonClick(DailyShoppingPrimaryCategory primaryCategory)
        {
            if (currentPrimaryCategory == primaryCategory) return;
            RefreshSecondaryCategoryButtons(primaryCategory);

            currentPrimaryCategory = primaryCategory;

            var secondaries = DailyShoppingCategory.GetSecondaries(primaryCategory);
            if (secondaries.Count > 0)
            {
                currentSecondaryCategory = secondaries[0];
                UpdateDailyShopItems();
            }
            else
            {
                currentSecondaryCategory = DailyShoppingSecondaryCategory.None;
                UpdateDailyShopItems();
            }

            DeselectItem();
        }

        public void RefreshSecondaryCategoryButtons(DailyShoppingPrimaryCategory primaryCategory)
        {
            clothes.SetActive(false);
            furniture.SetActive(false);
            planting.SetActive(false);
            belonging.SetActive(false);
            switch (primaryCategory)
            {
                case DailyShoppingPrimaryCategory.Clothes:
                    clothes.SetActive(true);
                    break;
                case DailyShoppingPrimaryCategory.Furniture:
                    furniture.SetActive(true);
                    break;
                case DailyShoppingPrimaryCategory.Planting:
                    planting.SetActive(true);
                    shoppingPanel.ChooseMagazineOrCategory(btnPot);
                    break;
                case DailyShoppingPrimaryCategory.Belonging:
                    belonging.SetActive(true);
                    shoppingPanel.ChooseMagazineOrCategory(btnFood);
                    break;
                default:
                    break;
            }
        }

        public void OnSecondaryCategoryButtonClick(DailyShoppingSecondaryCategory secondaryCategory)
        {
            if (currentSecondaryCategory == secondaryCategory) return;
            currentSecondaryCategory = secondaryCategory;

            UpdateDailyShopItems();

            DeselectItem();
        }

        public void UpdateDailyShopItems()
        {
            var shopItems = shoppingPanel.shopListDB._daily_shop_list;

            var filteredItems = shopItems.FindAll(item =>
            {
                var gameItem = Global_Inventory_Manager.GameItem_DB.Find(dbItem => dbItem.name == item.item_name);
                if (gameItem == null) return false;

                if (!DailyShoppingCategory.MatchesPrimary(gameItem.type, currentPrimaryCategory)) return false;

                if (currentSecondaryCategory != DailyShoppingSecondaryCategory.None
                    && !DailyShoppingCategory.MatchesSecondary(gameItem.type, currentSecondaryCategory)) return false;

                return true;
            });

            if (DailyShoppingCategory.LimitDailyListToFourSlots(currentPrimaryCategory))
            {
                if (filteredItems.Count > 4)
                {
                    filteredItems = filteredItems.GetRange(0, 4);
                }
            }

            RefreshShopItemUI(filteredItems);
        }

        public void RefreshItemButton()
        {
            int count = shoppingPanel.shopListDB.refreshCount;
            int cost = 0;
            if (count >= 2) cost = 50;
            PromptMessage.Instance.ShowPrompt("是否花费" + cost + "鱼币刷新商店？", () =>
            {
                MoneyManager.Instance.ChangeCurrency("鱼币", -50, "刷新商店", RefreshItem);
            });
        }

        private void RefreshItem(string result)
        {
            if (result == "success")
            {
                shoppingPanel.shopListDB.refreshCount++;
                shoppingPanel.shopListDB.get_daily_shop_list();
                UpdateDailyShopItems();
            }
            else
            {
                PromptMessage.Instance.ShowUpPrompt(result);
            }
        }

        public void RefreshShopItemUI(List<shop_item> items)
        {
            int existingItemCount = shopItemRoot.childCount;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                GameObject itemUnit;

                if (i < existingItemCount)
                {
                    itemUnit = shopItemRoot.GetChild(i).gameObject;
                    itemUnit.SetActive(true);
                }
                else
                {
                    itemUnit = Instantiate(shopItemPrefab, shopItemRoot);
                }

                var shoppingItemUnit = itemUnit.GetComponent<ShoppingItemUnit>();
                var gameItem = Global_Inventory_Manager.GameItem_DB.Find(dbItem => dbItem.name == item.item_name);
                if (gameItem != null)
                {
                    shoppingItemUnit.InitGameItemUnit(gameItem);
                    shoppingItemUnit.itemButton.onClick.AddListener(() => OnSelectItem(shoppingItemUnit));
                }
            }

            for (int i = items.Count; i < existingItemCount; i++)
            {
                shopItemRoot.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void SetIcon(Sprite sprite)
        {
            rawImage.texture = sprite.texture;
        }

        public void DeselectItem()
        {
            currentSelectedShopItem = null;
            currentSelectedShopItemUI = null;
            if (currentModel != null)
                currentModel.SetActive(false);
            currentModel = null;
            shoppingPanel.InitCharacterCloth();
        }

        public void ExitDailyMagazine()
        {
            shoppingPanel.OpenChooseMagazine();
            DeselectItem();
        }
    }
}
