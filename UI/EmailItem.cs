using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{

    public class EmailItem : UIBase
    {
        public Image icon;
        public Image background;
        public TMP_Text textCount;
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
            if (item.name == "鱼币" || item.name == "罐罐")
            {
                background.gameObject.SetActive(false);
            }
            else
            {
                background.gameObject.SetActive(true);
                background.sprite = backgroundSprites[(int)item.rarity - 1];
            }
            if (num == 1)
            {
                this.textCount.text = "";
            }
            else
            {
                this.textCount.text = num.ToString();
            }
            itemName.text = item.name;
        }
        public void SetIcon(Sprite sprite)
        {
            icon.sprite = sprite;
        }

    }

}
