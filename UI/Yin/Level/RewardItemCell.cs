using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class RewardItemCell : MonoBehaviour
            {
                public TMP_Text count;
                public TMP_Text itemName;
                public Image icon;
                public Image background;
                public List<Sprite> backgroundSprites;
                private Game_Item_Info item;
                public void Init(string itemName, int count)
                {
                    item = Global_Inventory_Manager.GetItemInfo(itemName);
                    this.itemName.text = itemName;
                    if (item == null)
                        return;
                    if (item.res_url != null && item.res_url.Length != 0)
                    {
                        var _image_url_data = item.res_url.Split("#");
                        if (_image_url_data.Length != 2)
                        {
                            GameAssets.LoadSprite(_image_url_data[0], (Sprite sprite) => icon.sprite = sprite);
                        }
                        else
                        {
                            GameAssets.LoadSubSprite(_image_url_data[0], _image_url_data[1], (Sprite sprite) => icon.sprite = sprite);
                        }
                    }
                    background.sprite = backgroundSprites[(int)item.rarity - 1];
                    this.count.text = count.ToString();
                }
            }
        }
    }
}