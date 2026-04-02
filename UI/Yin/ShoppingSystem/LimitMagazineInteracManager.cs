using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using CLIP.Project_Mouse.Kernel.Inventory;
using UnityEngine;
using UnityEngine.UI;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class LimitMagazineInteracManager : MonoBehaviour
            {
                [Header("商店数据")]
                public string currentPrimaryCategory = null;
                public string currentSecondaryCategory = null;
                public Shop_List_DB_SO shopListDB;

                [Header("二级分类父物体和按钮")]
                public GameObject clothesSuit;
                public GameObject furnitureSuit;
                public Button firstClothesButton;
                public Button secondClothesButton;
                public Button firstFurnitureButton;
                public Button secondFurnitureButton;
                public GameObject tryClothesImage;
                public GameObject itemPrefab;
                public Transform itemListParent;

                [Header("二级菜单")]
                private Dictionary<string, List<item_group>> suitNameToItemGroups = new Dictionary<string, List<item_group>>();
                private int currentSuitIndex = 0;
                public List<item_group> currentSuitItemGroups = new List<item_group>();

                [Header("套装物品")]
                public Transform suitImageRoot;
                public List<Game_Item_Info> currentSelectedShopItems = new List<Game_Item_Info>();
                public List<SuitImageButton> currentSelectedItemUnits = new List<SuitImageButton>();
                public Dictionary<string, GameObject> suitImages = new Dictionary<string, GameObject>();

                [Header("模型展示")]
                public RawImage rawImage;
                public List<GameObject> currentModels = new List<GameObject>();
                public Camera furnitureCamera;
                public Transform modelTransform;
                public Transform modelRootTransform;
                public Dictionary<string, GameObject> models = new Dictionary<string, GameObject>();

                private void Start()
                {
                    
                }

                private void OnEnable()
                {
                    BindSecondaryMenuButtons(); 
                    OnPrimaryCategoryButtonClick("服装套装");
                }

                public void BindSecondaryMenuButtons()
                {
                    firstFurnitureButton.onClick.RemoveAllListeners();
                    secondFurnitureButton.onClick.RemoveAllListeners();
                    firstClothesButton.onClick.RemoveAllListeners();
                    secondClothesButton.onClick.RemoveAllListeners();

                    firstClothesButton.onClick.AddListener(() => {
                        ClickTag(shopListDB._limited_cloth_item_list[0].group_item_list);
                        SetTryClothesImage(true);
                    });

                    secondClothesButton.onClick.AddListener(() => {
                        ClickTag(shopListDB._limited_cloth_item_list[0].group_item_list);
                        SetTryClothesImage(true);
                    });

                    firstFurnitureButton.onClick.AddListener(() => {
                        ClickTag(shopListDB._limited_cloth_item_list[0].group_item_list);
                        SetTryClothesImage(false);
                    });

                    secondFurnitureButton.onClick.AddListener(() => {
                        ClickTag(shopListDB._limited_cloth_item_list[0].group_item_list);
                        SetTryClothesImage(false);
                    });
                }
                private void ClickTag(List<string> itemList)
                {
                    
                }
                private void InitItemCell(List<string> itemList)
                {
                    foreach(var item in itemList)
                    {
                        GameObject obj = Instantiate(itemPrefab, itemListParent);
                        ShoppingItemUnit cell = obj.GetComponent<ShoppingItemUnit>();
                        //cell.InitGameItemUnit();
                    }
                }
                public void AddItemToCart()
                {
                    if (currentSelectedShopItems == null || currentSelectedShopItems.Count == 0)
                    {
                        PromptMessage.Instance.ShowUpPrompt("当前没有选中的物品");
                        return;
                    }

                    foreach (var item in currentSelectedShopItems)
                    {
                        var existingItem = ShoppingCanvasInteractManager.Instance.shoppingCartItems.Find(cartItem => cartItem.shoppingCartItem.name == item.name);
                        if (existingItem != null)
                        {
                            //existingItem.itemCount++;
                            return;
                        }
                        else
                        {
                            ShoppingCanvasInteractManager.Instance.CreateCartItem(item);
                        }
                    }

                    DeselectAllItem();
                    ShoppingCanvasInteractManager.Instance.InitCharacterCloth();
                }

                public void OnSelectItem(SuitImageButton suitItem)
                {
                    if (suitItem.isSelected)
                    {
                        suitItem.DeSelectItem();
                        currentSelectedShopItems.Remove(suitItem.itemInShop);
                        currentSelectedItemUnits.Remove(suitItem);
                        if (currentPrimaryCategory == "服装套装")
                        {
                            Character_Cloth_Manager.Instance.Add_Cloth(Character_Type.Shopping_Character, suitItem.suitItemName);
                        }
                        else if (currentPrimaryCategory == "家具套组")
                        {
                            if (models.TryGetValue(suitItem.itemInShop.name, out GameObject cachedModel))
                            {
                                cachedModel.SetActive(false);
                                currentModels.Remove(cachedModel);
                            }
                        }
                    }
                    else
                    {
                        suitItem.SelectItem();
                        currentSelectedShopItems.Add(suitItem.itemInShop);
                        currentSelectedItemUnits.Add(suitItem);
                        if (currentPrimaryCategory == "服装套装")
                        {
                            ShoppingCanvasInteractManager.Instance.ChangeCharacterCloth(suitItem.suitItemName);
                        }
                        else if (currentPrimaryCategory == "家具套组")
                        {
                            OnSelectFurniture(suitItem);
                        }
                    }
                }

                // 选择家具
                public void OnSelectFurniture(SuitImageButton suitItem)
                {
                    //if (models.TryGetValue(suitItem.itemInShop.name, out GameObject cachedModel))
                    //{
                    //    cachedModel.SetActive(true);
                    //    currentModels.Add(cachedModel);
                    //    AdjustCameraToFitModel(cachedModel.GetComponent<PlacementRuntime>()._grid_collider.GetComponent<BoxCollider>());
                    //    return;
                    //}

                    //if (suitItem.itemInShop.type == Item_Type.Room_Placement)
                    //{
                    //    var furnitureInfo =Indoor_Room_Game_Manager._activc_instance.placementNameDic[suitItem.itemInShop.name];
                    //    if (furnitureInfo == null)
                    //    {
                    //        Debug.LogWarning($"未找到名称为 {suitItem.itemInShop.name} 的家具信息！");
                    //        return;
                    //    }

                    //    // 加载家具模型
                    //    PM_RM.load_game_object_async(furnitureInfo.res_url, (furniturePrefab) =>
                    //    {
                    //        StartCoroutine(LoadFurnitureCo(suitItem, furniturePrefab));
                    //    });
                    //}
                }

                // 加载家具模型
                //public IEnumerator LoadFurnitureCo(SuitImageButton shoppingItemUnit, GameObject furniturePrefab)
                //{
                    //GameObject furnitureObject = Instantiate(furniturePrefab, modelTransform.position, Quaternion.identity);
                    //PlacementRuntime room_Placement_In_Level = furnitureObject.GetComponent<PlacementRuntime>();
                    //room_Placement_In_Level._rotation[0].gameObject.SetActive(false);

                    //furnitureObject.transform.SetParent(modelRootTransform, true);
                    //furnitureObject.name = shoppingItemUnit.itemInShop.name;

                    //currentModels.Add(furnitureObject);
                    //ShoppingCanvasInteractManager.Instance.SetLayer(furnitureObject, 14);

                    //models[shoppingItemUnit.itemInShop.name] = furnitureObject;

                    //yield return null;

                    //BoxCollider boxCollider = room_Placement_In_Level._grid_collider.GetComponent<BoxCollider>();

                    //if (boxCollider != null)
                    //{
                    //    AlignColliderCenterToModel(furnitureObject, boxCollider);
                    //    AdjustCameraToFitModel(boxCollider);
                    //}
                    //else
                    //{
                    //    Debug.LogError("家具物体上没有找到BoxCollider组件");
                    //}
                    //room_Placement_In_Level._rotation[0].gameObject.SetActive(true);
                //}

                // 调整模型位置使其居中
                public void AlignColliderCenterToModel(GameObject gameObject, BoxCollider targetCollider)
                {
                    Vector3 roundedSize = new Vector3(Mathf.Ceil(targetCollider.size.x), Mathf.Ceil(targetCollider.size.y), Mathf.Ceil(targetCollider.size.z));
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
                    cameraPosition.z = modelTransform.position.z - distance;
                    furnitureCamera.transform.position = cameraPosition;

                    Debug.Log($"调整摄像机位置: {cameraPosition}, 模型最大尺寸: {maxDimension}, 距离: {distance}");
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
                        OnSecondaryCategoryButtonClick(secondaryCategories[0]);
                    }

                    DeselectAllItem();
                }

                // 显示二级分类按钮
                public void RefreshSecendSecondaryCategoryButtons(string primaryCategory)
                {
                    switch (primaryCategory)
                    {
                        case "服装套装":
                            clothesSuit.SetActive(true);
                            furnitureSuit.SetActive(false);
                            if (rawImage != null && ShoppingCanvasInteractManager.Instance.characterRT != null)
                            {
                                rawImage.texture = ShoppingCanvasInteractManager.Instance.characterRT;
                            }
                            ShoppingCanvasInteractManager.Instance.InitCharacterCloth();
                            ShoppingCanvasInteractManager.Instance.ChooseMagazineOrCategory(firstClothesButton);
                            break;
                        case "家具套组":
                            clothesSuit.SetActive(false);
                            furnitureSuit.SetActive(true);
                            if (rawImage != null && ShoppingCanvasInteractManager.Instance.furnitureRT != null)
                            {
                                rawImage.texture = ShoppingCanvasInteractManager.Instance.furnitureRT;
                            }
                            ShoppingCanvasInteractManager.Instance.ChooseMagazineOrCategory(firstFurnitureButton);
                            break;
                        default:
                            break;
                    }
                }
                private void SetTryClothesImage(bool value)
                {
                    tryClothesImage.SetActive(value);
                }
                // 点击二级分类按钮
                public void OnSecondaryCategoryButtonClick(string secondaryCategory)
                {
                    if (currentSecondaryCategory == secondaryCategory) return;

                    currentSecondaryCategory = secondaryCategory;

                    //UpdateLimitShopItems(currentPrimaryCategory, secondaryCategory);

                    currentSuitItemGroups.Clear();
                    if (suitNameToItemGroups.ContainsKey(secondaryCategory))
                    {
                        currentSuitItemGroups = new List<item_group>(suitNameToItemGroups[secondaryCategory]);
                    }
                    else
                    {
                        foreach (var group in shopListDB._db._item_group)
                        {
                            if (group.parent_item_group == secondaryCategory || group.item_group_name == secondaryCategory)
                            {
                                currentSuitItemGroups.Add(group);
                            }
                        }
                        suitNameToItemGroups[secondaryCategory] = new List<item_group>(currentSuitItemGroups);
                    }

                    currentSuitIndex = 0;

                    // 显示第一个套装
                    if (currentSuitItemGroups.Count > 0)
                    {
                        UpdateSuit();
                    }
                    else
                    {
                        Debug.LogWarning($"未找到 {secondaryCategory} 对应的套装！");
                    }

                    DeselectAllItem();

                    if (currentPrimaryCategory == "服装套装")
                    {
                        ShoppingCanvasInteractManager.Instance.InitCharacterCloth();
                    }
                }

                private List<string> GetSecondaryCategories(string primaryCategory)
                {
                    switch (primaryCategory)
                    {
                        case "服装套装":
                            return new List<string> { shopListDB._limited_cloth_item_list[0].item_group_name, shopListDB._limited_cloth_item_list[1].item_group_name };
                        case "家具套组":
                            return new List<string> { shopListDB._limited_placement_item_list[0].item_group_name, shopListDB._limited_placement_item_list[1].item_group_name };
                        default:
                            return null;
                    }
                }

                public void UpdateLimitShopItems(string primaryCategory, string secondaryCategory)
                {
                    foreach (var image in suitImages.Values)
                    {
                        image.SetActive(false);
                    }

                    if (suitImages.TryGetValue(secondaryCategory, out GameObject existingItem))
                    {
                        existingItem.SetActive(true);
                        return;
                    }

                    GameObject itemObject = Resources.Load<GameObject>($"Prefabs/UI/SuitImage/{secondaryCategory}_Prefab");
                    if (itemObject != null)
                    {
                        GameObject instantiatedItem = Instantiate(itemObject, suitImageRoot);
                        instantiatedItem.SetActive(true);
                        suitImages[secondaryCategory] = instantiatedItem;

                        SuitImageButton[] suitImageButtons = instantiatedItem.GetComponentsInChildren<SuitImageButton>();
                        foreach (var button in suitImageButtons)
                        {
                            button.InitGameItemUnit(this);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"未找到 '{secondaryCategory}' 的预制体！");
                    }
                }

                public void DeselectAllItem()
                {
                    foreach (var itemUnit in currentSelectedItemUnits)
                    {
                        itemUnit.DeSelectItem();
                    }

                    foreach (var gameObject in currentModels)
                    {
                        gameObject.SetActive(false);
                    }

                    currentSelectedItemUnits.Clear();
                    currentSelectedShopItems.Clear();
                    currentModels.Clear();
                }

                public void ExitDailyMagazine()
                {
                    ShoppingCanvasInteractManager.Instance.limitMagazineCanvas.SetActive(false);
                    ShoppingCanvasInteractManager.Instance.chooseMagazine.SetActive(true);
                    ShoppingCanvasInteractManager.Instance.InitCharacterCloth();
                    DeselectAllItem();
                }
                public void UpdateSuit()
                {
                    if (currentSuitItemGroups == null || currentSuitItemGroups.Count == 0) return;

                    var suit = currentSuitItemGroups[currentSuitIndex];
                    UpdateLimitShopItems(currentPrimaryCategory, suit.item_group_name);
                    DeselectAllItem();
                }
            }
        }
    }
}