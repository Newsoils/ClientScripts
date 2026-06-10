using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Network;
using Cmd;
using Common;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Project_Mouse.Kernel;

namespace CLIP.Project_Mouse.UI
{
    public class DailyMagazinePanel : MonoBehaviour
    {
        [Header("商店数据")]
        public GameObject shopItemPrefab;
        public Transform shopItemRoot;
        public DailyShoppingTab currentTab = DailyShoppingTab.None;
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

        private readonly Dictionary<long, UserShop> serverShopsByID = new Dictionary<long, UserShop>();
        private UserShop serverShop;
        private const int DailyFreeRefreshTimesMax = 2;
        private const int ManualRefreshCostFishCoin = 50;
        private ShoppingPanel _shoppingPanel;
        private ShoppingPanel shoppingPanel => _shoppingPanel ??= UIManager.Instance.GetPanel<ShoppingPanel>();

        private void Start()
        {
            InitButtons();

            OnTabButtonClick(DailyShoppingTab.Clothes);
        }

        private void InitButtons()
        {
            btnClothes.onClick.RemoveAllListeners();
            btnFurniture.onClick.RemoveAllListeners();
            btnPlanting.onClick.RemoveAllListeners();
            btnBelonging.onClick.RemoveAllListeners();
            btnPot.onClick.RemoveAllListeners();
            btnSeed.onClick.RemoveAllListeners();
            btnFertilizer.onClick.RemoveAllListeners();
            btnFood.onClick.RemoveAllListeners();
            btnSnack.onClick.RemoveAllListeners();
            btnTape.onClick.RemoveAllListeners();

            btnClothes.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Clothes));
            btnFurniture.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Furniture));
            btnPlanting.onClick.AddListener(() => OnTabGroupButtonClick(btnPot, DailyShoppingTab.Pot));
            btnBelonging.onClick.AddListener(() => OnTabGroupButtonClick(btnFood, DailyShoppingTab.Food));

            btnPot.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Pot));
            btnSeed.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Seed));
            btnFertilizer.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Fertilizer));

            btnFood.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Food));
            btnSnack.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Snack));
            btnTape.onClick.AddListener(() => OnTabButtonClick(DailyShoppingTab.Tape));

            refreshShop.onClick.AddListener(RefreshItemButton);
        }

        public void ApplyServerShopData(IEnumerable<UserShop> shops)
        {
            if (shops != null)
            {
                foreach (var shop in shops)
                {
                    if (shop != null && shop.ShopID >= 1 && shop.ShopID <= 8)
                        serverShopsByID[shop.ShopID] = shop;
                }
            }
            UpdateDailyShopItems();
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
            shoppingPanel.EnsureRenderTextureBindings();
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

        private bool isLoadingModel;

        public async void OnSelectFurnitureOrPot(ShoppingItemUnit shoppingItemUnit)
        {
            if (isLoadingModel) return;
            isLoadingModel = true;

            shoppingPanel.EnsureRenderTextureBindings();
            rawImage.gameObject.SetActive(true);
            rawImage.texture = shoppingPanel.furnitureRT;

            if (currentModel != null)
                currentModel.SetActive(false);

            string itemName = shoppingItemUnit.itemInShop.name;

            if (models.TryGetValue(itemName, out currentModel))
            {
                currentModel.SetActive(true);
                StartCoroutine(FitModelToViewCoroutine(currentModel));
                isLoadingModel = false;
                return;
            }

            GameObject prefab = null;
            if (shoppingItemUnit.itemInShop.type == Item_Type.Room_Placement)
            {
                var info = GridObjectSystem.GetPlacementInfo(itemName);
                if (info != null)
                    prefab = await GameAssets.LoadAsync<GameObject>(info.res_url);
            }
            else if (shoppingItemUnit.itemInShop.type == Item_Type.Pot)
            {
                var data = GridObjectSystem.GetPotData(itemName);
                if (data != null)
                    prefab = await GameAssets.LoadAsync<GameObject>(data.resUrl);
            }

            if (prefab == null)
            {
                Debug.LogWarning($"[DailyMagazinePanel] 无法加载模型: {itemName}");
                isLoadingModel = false;
                return;
            }

            currentModel = Instantiate(prefab, modelRootTransform);
            ApplyLayerRecursive(currentModel, 14);
            models[itemName] = currentModel;
            StartCoroutine(FitModelToViewCoroutine(currentModel));
            isLoadingModel = false;
        }

        private System.Collections.IEnumerator FitModelToViewCoroutine(GameObject model)
        {
            yield return null;

            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) yield break;

            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;

            Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
            foreach (var r in renderers)
                bounds.Encapsulate(r.bounds);

            float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxDimension < 0.001f) maxDimension = 1f;
            float targetSize = 1.5f;
            float scale = targetSize / maxDimension;

            model.transform.localScale = Vector3.one * scale;

            Vector3 offset = bounds.center - modelRootTransform.position;
            model.transform.localPosition = -offset * scale;
        }

        private void ApplyLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                ApplyLayerRecursive(child.gameObject, layer);
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
                    if (!string.IsNullOrEmpty(food_Res_Name)) obj = await GameAssets.Instance.LoadAsyncByKey<GameObject>(food_Res_Name);
                    break;
                case Item_Type.Snack:
                    var snack_Res_Name = Dispatch_Manager.GetSnackInfo(itemName)?.model_resource_name;
                    if (!string.IsNullOrEmpty(snack_Res_Name)) obj = await GameAssets.Instance.LoadAsyncByKey<GameObject>(snack_Res_Name);
                    break;
                case Item_Type.Tape:
                    obj = await GameAssets.Instance.LoadAsyncByKey<GameObject>(ResKeys.PREFAB_CD_MODEL);
                    var cd_mat_name = Dispatch_Manager.GetCDInfo(itemName)?.mat_resource_name;
                    if (!string.IsNullOrEmpty(cd_mat_name)) cd_mt = await GameAssets.Instance.LoadAsyncByKey<Material>(cd_mat_name);
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
                var cd_obj = await GameAssets.Instance.LoadAsyncByKey<GameObject>(ResKeys.PREFAB_CD_MODEL);
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

        public void OnTabButtonClick(DailyShoppingTab tab)
        {
            if (currentTab == tab) return;
            RefreshTabGroup(tab);
            currentTab = tab;
            UpdateDailyShopItems();
            DeselectItem();
        }

        private void OnTabGroupButtonClick(Button defaultButton, DailyShoppingTab defaultTab)
        {
            shoppingPanel.ChooseMagazineOrCategory(defaultButton);
            OnTabButtonClick(defaultTab);
        }

        private void RefreshTabGroup(DailyShoppingTab tab)
        {
            clothes.SetActive(false);
            furniture.SetActive(false);
            planting.SetActive(false);
            belonging.SetActive(false);

            switch (tab)
            {
                case DailyShoppingTab.Clothes:
                    clothes.SetActive(true);
                    break;
                case DailyShoppingTab.Furniture:
                    furniture.SetActive(true);
                    break;
                case DailyShoppingTab.Pot:
                case DailyShoppingTab.Seed:
                case DailyShoppingTab.Fertilizer:
                    planting.SetActive(true);
                    break;
                case DailyShoppingTab.Food:
                case DailyShoppingTab.Snack:
                case DailyShoppingTab.Tape:
                    belonging.SetActive(true);
                    break;
            }
        }

        public void UpdateDailyShopItems()
        {
            long shopID = (long)currentTab;
            if (!serverShopsByID.TryGetValue(shopID, out serverShop))
            {
                RefreshShopItemUI(new List<ShopGoodsViewData>());
                return;
            }

            var items = new List<ShopGoodsViewData>();
            foreach (var goods in serverShop.Goodss)
            {
                if (ShopConfigResolver.TryCreateGoodsViewData(goods, out var viewData))
                    items.Add(viewData);
            }

            items.Sort((a, b) =>
            {
                int orderCompare = ShopConfigResolver.GetSortOrder(a).CompareTo(ShopConfigResolver.GetSortOrder(b));
                return orderCompare != 0 ? orderCompare : ShopConfigResolver.GetStableId(a).CompareTo(ShopConfigResolver.GetStableId(b));
            });
            RefreshShopItemUI(items);
        }

        public void RefreshItemButton()
        {
            if (serverShop == null || serverShop.ShopUID == 0UL)
            {
                PromptManager.ShowUpPrompt(PromptId.ShopDataOutOfSync);
                return;
            }

            int freeRefreshTimesLeft = GetFreeRefreshTimesLeft();
            if (freeRefreshTimesLeft > 0)
            {
                PromptManager.ShowPrompt(PromptId.ShopFreeRefreshConfirm, SendManualRefreshShopReq, freeRefreshTimesLeft);
                return;
            }

            PromptManager.ShowPrompt(PromptId.ShopCoinRefreshConfirm, SendManualRefreshShopReq, ManualRefreshCostFishCoin);
        }

        private int GetFreeRefreshTimesLeft()
        {
            var roleInfo = Global_Game_Manager.Instance.get_current_player_role_info();
            int usedTimes = roleInfo != null ? roleInfo.TodayShopRefreshTimes : 0;
            return Mathf.Max(0, DailyFreeRefreshTimesMax - usedTimes);
        }

        private void SendManualRefreshShopReq()
        {
            NetWork_Center_WSS.SendMsg(new ManualRefreshShopReq
            {
                ShopUID = serverShop.ShopUID
            });
        }

        public void RefreshShopItemUI(List<ShopGoodsViewData> items)
        {
            int existingItemCount = shopItemRoot.childCount;

            for (int i = 0; i < items.Count; i++)
            {
                var goods = items[i];
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
                shoppingItemUnit.itemButton.onClick.RemoveAllListeners();
                shoppingItemUnit.InitShopGoodsUnit(goods, serverShop.ShopUID);
                shoppingItemUnit.itemButton.onClick.AddListener(() => OnSelectItem(shoppingItemUnit));
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
