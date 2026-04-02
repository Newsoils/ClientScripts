using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Tools;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using CLIP.Project_Mouse.Game_Play_System.Planting_System;
using CLIP.Project_Mouse.Kernel.Inventory;
using CLIP.Project_Mouse.NewFrame.UI;
using CLIP.Project_Mouse.Scene_View_Control;
using UnityEngine;
using UnityEngine.UI;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class DailyMagazineInteracManager : MonoBehaviour
            {
                [Header("商店数据")]
                public GameObject shopItemPrefab;
                public Transform shopItemRoot;
                public string currentPrimaryCategory = null;
                public string currentSecondaryCategory = null;
                public Game_Item_Info currentSelectedShopItem = null;
                public ShoppingItemUnit currentSelectedShopItemUI = null;

                [Header("二级分类父物体和首个标签")]
                public GameObject clothes;
                public GameObject furniture;
                public GameObject planting;
                public GameObject belonging;
                public Button pot;
                public Button food;
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

                private void Start()
                {
                    ShoppingCanvasInteractManager.Instance.shoppingInteractionEventHub._on_primary_category_change.AddListener(OnPrimaryCategoryButtonClick);
                    ShoppingCanvasInteractManager.Instance.shoppingInteractionEventHub._on_secondary_category_change.AddListener(OnSecondaryCategoryButtonClick);
                    ShoppingCanvasInteractManager.Instance.shopListDB.refreshCount = 0;
                    refreshShop.onClick.AddListener(RefreshItemButton);
                    OnPrimaryCategoryButtonClick("服装");
                }

                // 加入购物车
                public void AddItemToCart()
                {
                    if (currentSelectedShopItem == null)
                    {
                        PromptMessage.Instance.ShowUpPrompt("当前没有选中的物品");
                        return;
                    }

                    var existingItem = UIManager.Instance.GetPanel<ShoppingPanel>().shoppingCartItems.Find(cartItem => cartItem.shoppingCartItem.name == currentSelectedShopItem.name);

                    if (existingItem != null)
                    {
                        //existingItem.itemCount++;
                        return;
                    }
                    else
                    {
                        UIManager.Instance.GetPanel<ShoppingPanel>().CreateCartItem(currentSelectedShopItem);
                    }
                }

                // 选择商品
                public void OnSelectItem(ShoppingItemUnit shoppingItemUnit)
                {
                    Unity_Tools.ClearAllChildren(dispatchItemTransRoot);
                    if (shoppingItemUnit == currentSelectedShopItemUI)
                    {
                        if (currentSelectedShopItemUI != null && shoppingItemUnit.itemInShop.type == Item_Type.Cloth)
                        {
                            Character_Cloth_Manager.Instance.Remove_Cloth(Character_Type.Shopping_Character, shoppingItemUnit.itemInShop.name);
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

                    //ShoppingCanvasInteractManager.Instance.shoppingInteractionEventHub._invoke_on_selected_item_change(shoppingItemUnit.name);
                    shoppingItemUnit.SelectItem();

                    switch(shoppingItemUnit.itemInShop.type)
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
                            _=OnSelectOtherItem(shoppingItemUnit);
                            break;
                    }

                }

                // 展示衣服模型
                public void OnSelectClothingItem(ShoppingItemUnit shoppingItemUnit)
                {
                    rawImage.gameObject.SetActive(true);
                    if (rawImage != null && ShoppingCanvasInteractManager.Instance.characterRT != null)
                    {
                        rawImage.texture = ShoppingCanvasInteractManager.Instance.characterRT;
                    }
                    else
                    {
                        Debug.LogError("RawImage 或 characterRT 未正确设置！");
                        return;
                    }

                    Character_Cloth_Manager.Instance.Add_Cloth(Character_Type.Shopping_Character, shoppingItemUnit.itemInShop.name);

                }

                // 展示家具或花盆模型
                public void OnSelectFurnitureOrPot(ShoppingItemUnit shoppingItemUnit)
                {
                    //rawImage.gameObject.SetActive(true);
                    //if (rawImage != null && ShoppingCanvasInteractManager.Instance.furnitureRT != null)
                    //{
                    //    rawImage.texture = ShoppingCanvasInteractManager.Instance.furnitureRT;
                    //}
                    //else
                    //{
                    //    Debug.LogError("RawImage 或 furnitureRT 未正确设置！");
                    //    return;
                    //}
                  

                    //if (currentModel != null)
                    //{
                    //    currentModel.SetActive(false);
                    //}

                    //if (models.TryGetValue(shoppingItemUnit.itemInShop.name, out GameObject cachedModel))
                    //{
                    //    cachedModel.SetActive(true);
                    //    if (shoppingItemUnit.itemInShop.type == Item_Type.Room_Placement)
                    //    {
                    //        AdjustCameraToFitModel(cachedModel.GetComponent<PlacementRuntime>()._grid_collider.GetComponent<BoxCollider>());
                    //    }
                    //    else if (shoppingItemUnit.itemInShop.type == Item_Type.Pot)
                    //    {
                    //        AdjustCameraToFitModel(cachedModel.GetComponent<Pot_In_Scene>()._collider.GetComponent<BoxCollider>());
                    //    }
                    //    currentModel = cachedModel;
                    //    return;
                    //}

                    //if (shoppingItemUnit.itemInShop.type == Item_Type.Room_Placement)
                    //{
                    //    var furnitureInfo = Indoor_Room_Game_Manager._activc_instance.placementNameDic[shoppingItemUnit.itemInShop.name];
                    //    if (furnitureInfo == null)
                    //    {
                    //        Debug.LogWarning($"未找到名称为 {shoppingItemUnit.itemInShop.name} 的家具信息！");
                    //        return;
                    //    }

                    //    if (furnitureInfo.placing_type == Room_Placing_Type.Floor_Finishes || furnitureInfo.placing_type == Room_Placing_Type.Wall_Finishes)
                    //    {
                    //        rawImage.texture = null;
                    //        return;
                    //    }
                    //    else
                    //    {
                    //        PM_RM.load_game_object_async(furnitureInfo.res_url, (furniturePrefab) =>
                    //        {
                    //            StartCoroutine(LoadFurnitureCo(shoppingItemUnit, furniturePrefab));
                    //        });
                    //    }
                    //    // 加载家具模型
                       
                    //}
                    //else if (shoppingItemUnit.itemInShop.type == Item_Type.Pot)
                    //{
                        
                    //    var potInfo = Planting_System_Manager.Instance._plant_Config_SO._planting_config.flower_pot_info_list.Find(_p => _p.pot_name == shoppingItemUnit.itemInShop.name);
                    //    if (potInfo == null)
                    //    {
                    //        Debug.LogWarning($"未找到名称为 {shoppingItemUnit.itemInShop.name} 的花盆信息！");
                    //        return;
                    //    }

                    //    // 加载花盆模型
                    //    PM_RM.load_game_object_async(potInfo.res_url, (potPrefab) =>
                    //    {
                    //        StartCoroutine(LoadPotCo(shoppingItemUnit, potPrefab));
                    //    });
                    //}
                }

                //// 加载花盆模型
                //public IEnumerator LoadPotCo(ShoppingItemUnit shoppingItemUnit, GameObject potPrefab)
                //{
                //    GameObject furnitureObject = Instantiate(potPrefab, modelTransform.position, Quaternion.identity);
                //    Pot_In_Scene pot_In_Scene = furnitureObject.GetComponent<Pot_In_Scene>();
                //    pot_In_Scene._root_from_sub_rotation[0].gameObject.SetActive(false);

                //    furnitureObject.transform.SetParent(modelRootTransform, true);
                //    furnitureObject.name = shoppingItemUnit.itemInShop.name;

                //    currentModel = furnitureObject;
                //    ShoppingCanvasInteractManager.Instance.SetLayer(furnitureObject, 14);

                //    models[shoppingItemUnit.itemInShop.name] = furnitureObject;

                //    yield return null;

                //    BoxCollider boxCollider = pot_In_Scene._collider.GetComponent<BoxCollider>();

                //    if (boxCollider != null)
                //    {
                //        AlignColliderCenterToModel(furnitureObject, boxCollider);
                //        AdjustCameraToFitModel(boxCollider);
                //    }
                //    else
                //    {
                //        Debug.LogError("花盆物体上没有找到BoxCollider组件");
                //    }
                //    pot_In_Scene._root_from_sub_rotation[0].gameObject.SetActive(true);
                //    var renders = pot_In_Scene._root_from_sub_rotation[0].GetComponentsInChildren<MeshRenderer>();
                //    foreach(var each in renders)
                //    {
                //        each.enabled = true;
                //    }
                //}

                //// 加载家具模型
                //public IEnumerator LoadFurnitureCo(ShoppingItemUnit shoppingItemUnit, GameObject furniturePrefab)
                //{
                //    GameObject furnitureObject = Instantiate(furniturePrefab, modelTransform.position, Quaternion.identity);
                //    PlacementRuntime room_Placement_In_Level = furnitureObject.GetComponent<PlacementRuntime>();
                //    room_Placement_In_Level._rotation[0].gameObject.SetActive(false);

                //    furnitureObject.transform.SetParent(modelRootTransform, true);
                //    furnitureObject.name = shoppingItemUnit.itemInShop.name;

                //    currentModel = furnitureObject;
                //    ShoppingCanvasInteractManager.Instance.SetLayer(furnitureObject, 14);

                //    models[shoppingItemUnit.itemInShop.name] = furnitureObject;

                //    yield return null;

                //    BoxCollider boxCollider = room_Placement_In_Level._grid_collider.GetComponent<BoxCollider>();

                //    if (boxCollider != null)
                //    {
                //        AlignColliderCenterToModel(furnitureObject, boxCollider);
                //        AdjustCameraToFitModel(boxCollider);
                //    }
                //    else
                //    {
                //        Debug.LogError("家具物体上没有找到BoxCollider组件");
                //    }
                //    room_Placement_In_Level._rotation[0].gameObject.SetActive(true);
                //}

                // 调整模型位置使其居中
                public void AlignColliderCenterToModel(GameObject gameObject, BoxCollider targetCollider)
                {
                    Vector3 roundedSize = new Vector3(Mathf.Ceil(targetCollider.size.x),Mathf.Ceil(targetCollider.size.y),Mathf.Ceil(targetCollider.size.z));
                    Vector3 offset = new Vector3(roundedSize.x / 2f, roundedSize.y / 2f, roundedSize.z / 2f);
                    gameObject.transform.position -= offset;
                }

                // 调整摄像机位置以适应模型
                public void AdjustCameraToFitModel(BoxCollider targetCollider)
                {
                    Bounds bounds = targetCollider.bounds;

                    float maxDimension = Mathf.Max(Mathf.Ceil(bounds.size.x) + 1, Mathf.Ceil(bounds.size.y) + 1);

                    // 获取摄像机的视野角度（垂直方向）
                    float fieldOfView = furnitureCamera.fieldOfView;

                    // 计算摄像机到模型的距离
                    // 使用三角函数：tan(FOV / 2) = (模型高度 / 2) / 距离
                    float distance = (maxDimension / 2) / Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);

                    // 设置摄像机的位置
                    Vector3 cameraPosition = furnitureCamera.transform.position;
                    cameraPosition.z = modelTransform.position.z - distance-2f;
                    furnitureCamera.transform.position = cameraPosition;

                    Debug.Log($"调整摄像机位置: {cameraPosition}, 模型最大尺寸: {maxDimension}, 距离: {distance}");
                }

                // 展示其他类型物品图标
                public async Task OnSelectOtherItem(ShoppingItemUnit shoppingItemUnit)
                {

                    // 展示物品的图标
                    //if (shoppingItemUnit.itemInShop.res_url != null && shoppingItemUnit.itemInShop.res_url.Length != 0)
                    //{
                    //    var imageUrlData = shoppingItemUnit.itemInShop.res_url.Split("#");
                    //    if (imageUrlData.Length == 2)
                    //    {
                    //        PM_RM.load_sub_sprite(imageUrlData[0], imageUrlData[1], SetIcon);
                    //    }
                    //}
                    var itemName = shoppingItemUnit.itemInShop.name;

                    GameObject obj =null;
                    Material cd_mt = null;
                    var dispatch_Config =  Dispatch_Manager._instance._dispatch_configuration_so._dispatch_config;

                    switch (shoppingItemUnit.itemInShop.type)
                    {
                        case Item_Type.Food:
                            var food_Res_Name = Dispatch_Manager.GetFoodInfo(itemName)?.model_resource_name;
                            if(!string.IsNullOrEmpty(food_Res_Name)) obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(food_Res_Name);
                            break;
                        case Item_Type.Snack:
                            var snack_Res_Name = Dispatch_Manager.GetSnackInfo(itemName)?.model_resource_name;
                            if (!string.IsNullOrEmpty(snack_Res_Name)) obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(snack_Res_Name);
                            break;
                        case Item_Type.Tape:
                            obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(ResKeys.PREFAB_CD_MODEL);
                            var cd = dispatch_Config.tape_info_list.Find(cd => cd.tape_name == shoppingItemUnit.itemInShop.name);

                            var cd_mat_name = Dispatch_Manager.GetCDInfo(itemName)?.mat_resource_name;
                            if(!string.IsNullOrEmpty(cd_mat_name)) cd_mt = await GameAssets.Instance.LoadAsycByKey<Material>(cd_mat_name);
                            break;
                        default:
                            rawImage.texture = null;
                            rawImage.gameObject.SetActive(false);
                            break;
                    }


                    if (obj!=null)
                    {
                        Unity_Tools.ClearAllChildren(dispatchItemTransRoot);
                        rawImage.gameObject.SetActive(true);
                        rawImage.texture = shoppingDispatchItemTexture;
                        GameObject _obj = Instantiate(obj, dispatchItemTransRoot.position, Quaternion.identity);
                        _obj.transform.parent = dispatchItemTransRoot;
                        _obj.transform.localEulerAngles = Vector3.zero;
                    }
                    if(cd_mt !=null)
                    {
                        var cd_obj = await GameAssets.Instance.LoadAsycByKey<GameObject>(ResKeys.PREFAB_CD_MODEL);
                        if(cd_obj !=null)
                        {
                            rawImage.gameObject.SetActive(true);
                            rawImage.texture = shoppingDispatchItemTexture;
                            Unity_Tools.ClearAllChildren(dispatchItemTransRoot);
                            var meshRenderer = cd_obj.GetComponentInChildren<MeshRenderer>();
                            if (meshRenderer !=null) meshRenderer .material= cd_mt;
                            Instantiate(cd_obj, dispatchItemTransRoot);
                        }
                    }
                    //else
                    //{
                    //    rawImage.texture = null;
                    //    rawImage.gameObject.SetActive(false);
                    //}
                }

                // 点击一级分类按钮
                public void OnPrimaryCategoryButtonClick(string primaryCategory)
                {
                    if (currentPrimaryCategory == primaryCategory) return;
                    RefreshSecendSecondaryCategoryButtons(primaryCategory);

                    currentPrimaryCategory = primaryCategory;

                    var secondaryCategories = GetSecondaryCategories(primaryCategory);

                    if (secondaryCategories != null && secondaryCategories.Count > 0)
                    {
                        currentSecondaryCategory = secondaryCategories[0];
                        UpdateDailyShopItems();
                    }
                    else
                    {
                        currentSecondaryCategory = null;
                        UpdateDailyShopItems();
                    }

                    DeselectItem();
                }

                // 显示二级分类按钮
                public void RefreshSecendSecondaryCategoryButtons(string primaryCategory)
                {
                    clothes.SetActive(false);
                    furniture.SetActive(false);
                    planting.SetActive(false);
                    belonging.SetActive(false);
                    switch (primaryCategory)
                    {
                        case "服装":
                            clothes.SetActive(true);
                            break;
                        case "家具":
                            furniture.SetActive(true);
                            break;
                        case "种植":
                            planting.SetActive(true);
                            ShoppingCanvasInteractManager.Instance.ChooseMagazineOrCategory(pot);
                            break;
                        case "携带物":
                            belonging.SetActive(true);
                            ShoppingCanvasInteractManager.Instance.ChooseMagazineOrCategory(food);
                            break;
                        default:
                            break;
                    }
                }

                // 点击二级分类按钮
                public void OnSecondaryCategoryButtonClick(string secondaryCategory)
                {
                    if (currentSecondaryCategory == secondaryCategory) return;
                    currentSecondaryCategory = secondaryCategory;

                    UpdateDailyShopItems();

                    DeselectItem();
                }

                private List<string> GetSecondaryCategories(string primaryCategory)
                {
                    switch (primaryCategory)
                    {
                        case "种植":
                            return new List<string> { "花盆", "种子", "肥料" };
                        case "携带物":
                            return new List<string> { "便当", "幸运小物", "磁带" };
                        default:
                            return null;
                    }
                }

                // 更新商店物品
                public void UpdateDailyShopItems()
                {
                    var shopItems = ShoppingCanvasInteractManager.Instance.shopListDB._daily_shop_list;

                    // 筛选商品
                    var filteredItems = shopItems.FindAll(item =>
                    {
                        var gameItem = Global_Inventory_Manager.GameItem_DB.Find(dbItem => dbItem.name == item.item_name);
                        if (gameItem == null) return false;

                        if (!IsPrimaryCategoryMatch(gameItem.type, currentPrimaryCategory)) return false;

                        if (!string.IsNullOrEmpty(currentSecondaryCategory) && !IsSecondaryCategoryMatch(gameItem.type, currentSecondaryCategory)) return false;

                        return true;
                    });

                    if (currentPrimaryCategory == "服装" || currentPrimaryCategory == "家具")
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
                    int count = ShoppingCanvasInteractManager.Instance.shopListDB.refreshCount;
                    int cost = 0;
                    if (count >= 2) cost = 50;
                    PromptMessage.Instance.ShowPrompt("是否花费" + cost + "鱼币刷新商店？", () =>
                    {
                        MoneyManager.Instance.ChangeCurrency("鱼币", -50, "刷新商店", RefreshItem);
                    });
                }
                private void RefreshItem(string result)
                {
                    if(result == "success")
                    {
                        ShoppingCanvasInteractManager.Instance.shopListDB.refreshCount++;
                        ShoppingCanvasInteractManager.Instance.shopListDB.get_daily_shop_list();
                        UpdateDailyShopItems();
                    }
                    else
                    {
                        PromptMessage.Instance.ShowUpPrompt(result);
                    }

                }
                // 更新商店物品图标
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


                // 判断物品类型是否属于一级分类
                public bool IsPrimaryCategoryMatch(Item_Type itemType, string primaryCategory)
                {
                    switch (primaryCategory)
                    {
                        case "服装":
                            return itemType == Item_Type.Cloth;
                        case "家具":
                            return itemType == Item_Type.Room_Placement;
                        case "种植":
                            return itemType == Item_Type.Pot || itemType == Item_Type.Seed || itemType == Item_Type.Fertilizer;
                        case "携带物":
                            return itemType == Item_Type.Food || itemType == Item_Type.Snack || itemType == Item_Type.Tape;
                        default:
                            return false;
                    }
                }

                // 判断物品类型是否属于二级分类
                public bool IsSecondaryCategoryMatch(Item_Type itemType, string secondaryCategory)
                {
                    switch (secondaryCategory)
                    {
                        case "花盆":
                            return itemType == Item_Type.Pot;
                        case "种子":
                            return itemType == Item_Type.Seed;
                        case "肥料":
                            return itemType == Item_Type.Fertilizer;
                        case "便当":
                            return itemType == Item_Type.Food;
                        case "幸运小物":
                            return itemType == Item_Type.Snack;
                        case "磁带":
                            return itemType == Item_Type.Tape;
                        default:
                            return false;
                    }
                }

                // 设置RawImage图标
                public void SetIcon(Sprite sprite)
                {
                    rawImage.texture = sprite.texture;
                }

                // 取消选择物品
                public void DeselectItem()
                {
                    currentSelectedShopItem = null;
                    currentSelectedShopItemUI = null;
                    if (currentModel != null)
                        currentModel.SetActive(false);
                    currentModel = null;
                    ShoppingCanvasInteractManager.Instance.InitCharacterCloth();
                }
       
                public void ExitDailyMagazine()
                {
                    ShoppingCanvasInteractManager.Instance.dailyMagazineCanvas.SetActive(false);
                    ShoppingCanvasInteractManager.Instance.chooseMagazine.SetActive(true);
                    DeselectItem();
                }
            }
        }
    }
}

