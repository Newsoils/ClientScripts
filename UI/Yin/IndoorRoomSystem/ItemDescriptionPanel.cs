using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class ItemDescriptionPanel : UIPanelBase
    {
        public GameObject panelObj;
        public Image itemIcon;

        public Button btnExit;
        public List<Sprite> btnExits;

        public Image decorationFrame;
        public List<Sprite> decorationFrames;

        public Image rarityTag;
        public List<Sprite> rarityTags;

        public Image favor;
        public List<Sprite> favors;

        public Image background;
        public List<Sprite> backgrounds;


        public GameObject itemNumBG;

        public Button btnFavor;
        public TMP_Text itemNum;
        public TMP_Text itemName;
        public TMP_Text itemSellPrice;
        public TMP_Text itemDescription;
        private Game_Item_In_Inventory item;
        private void Start()
        {
            btnExit.onClick.AddListener(ClosePanel);
            btnFavor.onClick.AddListener(BtnSetFavor);

            EvtDsp.AddEvt<Game_Item_In_Inventory>(EvtNames.ShowItemDetail, OpenPanel);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            EvtDsp.RemoveEvt<Game_Item_In_Inventory>(EvtNames.ShowItemDetail, OpenPanel);
        }

        public override void OpenPanel(params object[] data)
        {
            if (data != null && data.Length > 0)
            {
                if (data[0] is Game_Item_In_Inventory item)
                {
                    OpenPanel(item);
                }
                else if (data[0] is Game_Item_Info info)
                {
                    OpenPanel(info);
                }
            }
        }


        public void OpenPanel(Game_Item_Info itemInfo)
        {
           
            panelObj.SetActive(true);
            if (itemInfo.res_url != null && itemInfo.res_url.Length != 0)
            {
                var _image_url_data = itemInfo.res_url.Split("#");
                if (_image_url_data.Length != 2)
                {
                    Project_Mouse_Resource_Management.Load_Sprite(_image_url_data[0], (Sprite icon) => itemIcon.sprite = icon);
                }
                else
                {
                    Project_Mouse_Resource_Management.load_sub_sprite(_image_url_data[0], _image_url_data[1], (Sprite icon) => itemIcon.sprite = icon);
                }
            }

            btnExit.GetComponent<Image>().sprite = btnExits[(int)itemInfo.rarity - 1];
            decorationFrame.sprite = decorationFrames[(int)itemInfo.rarity - 1];
            rarityTag.sprite = rarityTags[(int)itemInfo.rarity - 1];

            itemNumBG.gameObject.SetActive(false);
            favor.gameObject.SetActive(false);
            background.sprite = backgrounds[(int)itemInfo.rarity - 1];

            itemNum.gameObject.SetActive(false); 
            itemName.text = itemInfo.name;
            itemSellPrice.text = itemInfo.sell_price.ToString();
            itemDescription.text = itemInfo.desc;
        }
        public void OpenPanel(Game_Item_In_Inventory item)
        {
            this.item = item;
            panelObj.SetActive(true);
            if (item.item_info.res_url != null && item.item_info.res_url.Length != 0)
            {
                var _image_url_data = item.item_info.res_url.Split("#");
                if (_image_url_data.Length != 2)
                {
                    Project_Mouse_Resource_Management.Load_Sprite(_image_url_data[0], (Sprite icon) => itemIcon.sprite = icon);
                }
                else
                {
                    Project_Mouse_Resource_Management.load_sub_sprite(_image_url_data[0], _image_url_data[1], (Sprite icon) => itemIcon.sprite = icon);
                }
            }

            favor.gameObject.SetActive(true);
            itemNum.gameObject.SetActive(true);
            itemNumBG.gameObject.SetActive(true);

            btnExit.GetComponent<Image>().sprite = btnExits[(int)item.item_info.rarity - 1];
            decorationFrame.sprite = decorationFrames[(int)item.item_info.rarity - 1];
            rarityTag.sprite = rarityTags[(int)item.item_info.rarity - 1];
            favor.sprite = favors[((int)item.item_info.rarity - 1) * 2 + (item.is_favorite ? 1 : 0)];
            background.sprite = backgrounds[(int)item.item_info.rarity - 1];

            itemNum.text = item._item_count.ToString();
            itemName.text = item.item_name;
            itemSellPrice.text = item.item_info.sell_price.ToString();
            itemDescription.text = item.item_info.desc;
        }
        public override void ClosePanel()
        {
            panelObj.SetActive(false);
        }
        private void BtnSetFavor()
        {
            item.is_favorite = !item.is_favorite;
            favor.sprite = favors[((int)item.item_info.rarity - 1) * 2 + (item.is_favorite ? 1 : 0)];
        }
    }

}
