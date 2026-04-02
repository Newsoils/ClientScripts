using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity.Asset;
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
            public class EmailItem : MonoBehaviour
            {
                public Image icon;
                public Image background;
                public TMP_Text num;
                public TMP_Text itemName;
                public List<Sprite> backgroundSprites;
                public void InitItem(Game_Item_Info item, int num)
                {
                    if (item.res_url != null && item.res_url.Length != 0)
                    {
                        var _image_url_data = item.res_url.Split("#");
                        if (_image_url_data.Length != 2)
                        {
                            Project_Mouse_Resource_Management.Load_Sprite(_image_url_data[0], SetIcon);
                        }
                        else
                        {
                            Project_Mouse_Resource_Management.load_sub_sprite(_image_url_data[0], _image_url_data[1], SetIcon);
                        }
                    }
                    if(item.name == "鱼币" || item.name == "罐罐")
                    {
                        background.gameObject.SetActive(false);
                    }
                    else
                    {
                        background.gameObject.SetActive(true);
                        background.sprite = backgroundSprites[(int)item.rarity - 1];
                    }
                    if(num == 1)
                    {
                        this.num.text = "";
                    }
                    else
                    {
                        this.num.text = num.ToString();
                    }
                    itemName.text = item.name;
                }
                public void SetIcon(Sprite sprite)
                {
                    icon.sprite = sprite;
                }
                
            }
        }
    }
}
