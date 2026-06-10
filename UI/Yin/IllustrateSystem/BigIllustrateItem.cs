using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel.Dispatch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class BigIllustrateItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
            {
                [Header("物品信息")]
                public Game_Item_Info currentItem;
                public Map_Info currentMap;
                public List<Game_Item_Info> items;
                public List<Game_Item_Info> obtainedItems;
                public List<Map_Info> maps;
                public List<Map_Info> obtainedMaps;
                public int currentIndex = 0;

                [Header("UI")]
                public RectTransform bgImage;
                public Image icon;
                public Image rarityImage;
                public TMP_Text rarityText;
                public TMP_Text nameText;
                public TMP_Text describeText;
                public Color commonColor;
                public Color rareColor;
                public Color preciousColor;

                [Header("手势检测")]
                private Vector2 pointerDownPos;
                private Vector2 pointerUpPos;
                private float minSwipeDistance = 50f;

                public void InitItem()
                {
                    // 设置image
                    nameText.text = "?";
                    describeText.text = "detail：???";
                    rarityImage.color = Color.white;
                    rarityText.text = "?";
                }

                public void SetItems(List<Game_Item_Info> items, int startIndex = 0)
                {
                    this.items = items;
                    ShowItem(startIndex);
                }

                public void ShowItem(int index)
                {
                    //if (IllustrateInteractManager.Instance.currentCategory == "Card")
                    if (UIManager.Instance.GetPanel<IllustratePanel>().currentCategory == "Card")
                    {
                        if (maps == null || maps.Count == 0) return;
                        var item = maps[index];
                        currentMap = item;
                        currentIndex = index;

                        bool obtained = obtainedMaps != null && obtainedMaps.Exists(m => m.map_id == item.map_id);
                        if (obtained)
                        {
                            // 设置image
                            nameText.text = item.map_name;
                            describeText.text = $"detail：{item.map_name}";
                            UpdateRarity();
                            //IllustrateInteractManager.Instance.RemoveNewObtainedItem(item.map_name);
                            UIManager.Instance.GetPanel<IllustratePanel>().RemoveNewObtainedItem(item.map_name);
                        }
                        else
                        {
                            InitItem();
                        }
                    }
                    else
                    {
                        if (items == null || items.Count == 0) return;
                        var item = items[index];
                        currentItem = item;
                        currentIndex = index;
                        bool obtained = obtainedItems != null && obtainedItems.Exists(i => i.item_id == item.item_id);
                        if (obtained)
                        {
                            // 设置image
                            nameText.text = item.name;
                            describeText.text = $"detail：{item.desc}";
                            UpdateRarity();
                            //IllustrateInteractManager.Instance.RemoveNewObtainedItem(item.name);
                            UIManager.Instance.GetPanel<IllustratePanel>().RemoveNewObtainedItem(item.name);
                        }
                        else
                        {
                            InitItem();
                        }
                    }
                }

                public void UpdateRarity()
                {
                    if (currentItem.rarity == RarityType.Common)
                    {
                        rarityImage.color = commonColor;
                        rarityText.text = "普通";
                    }
                    else if (currentItem.rarity == RarityType.Rare)
                    {
                        rarityImage.color = rareColor;
                        rarityText.text = "稀有";
                    }
                    else if (currentItem.rarity == RarityType.Precious)
                    {
                        rarityImage.color = preciousColor;
                        rarityText.text = "珍贵";
                    }
                }

                public void OnPointerDown(PointerEventData eventData)
                {
                    pointerDownPos = eventData.position;
                }

                public void OnPointerUp(PointerEventData eventData)
                {
                    pointerUpPos = eventData.position;
                    DetectSwipe();
                }

                private void DetectSwipe()
                {
                    Vector2 swipe = pointerUpPos - pointerDownPos;
                    if (swipe.magnitude < minSwipeDistance) return;

                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImage, pointerDownPos, null, out localPoint);

                    // icon的rect信息
                    Rect rect = bgImage.rect;

                    // 判断是否在左下角或右下角
                    float leftBoundary = rect.xMin + rect.width * 0.3f;
                    float rightBoundary = rect.xMax - rect.width * 0.3f;
                    float bottomBoundary = rect.yMin + rect.height * 0.3f;

                    bool isLeftBottom = localPoint.x < leftBoundary && localPoint.y < bottomBoundary;
                    bool isRightBottom = localPoint.x > rightBoundary && localPoint.y < bottomBoundary;

                    if (isLeftBottom && swipe.x > 0)
                    {
                        PrevPage();
                        Debug.Log("上一页");
                    }

                    else if (isRightBottom && swipe.x < 0)
                    {
                        NextPage();
                        Debug.Log("下一页");
                    }
                }

                private void PrevPage()
                {
                    if (items == null || items.Count == 0) return;
                    if (currentIndex > 0)
                    {
                        currentIndex--;
                        ShowItem(currentIndex);
                    }
                }

                private void NextPage()
                {
                    if (items == null || items.Count == 0) return;
                    if (currentIndex < items.Count - 1)
                    {
                        currentIndex++;
                        ShowItem(currentIndex);
                    }
                }
            }
        }
    }
}