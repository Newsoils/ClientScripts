using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class NpcChatGiftDiscription : MonoBehaviour
            {
                public GameObject giftDescription;
                public TMP_Text _item_Name;
                public TMP_Text _item_Description;
                public TMP_Text _sell_price;
                public TMP_Text quantity;
                public GameObject normalRarity;
                public GameObject rareRarity;
                public GameObject previousRarity;
                public NpcChatGiftUnit detailGift;

                public void InitDescription(NpcChatGiftUnit giftUnit)
                {
                    detailGift = giftUnit;
                    _item_Name.text = giftUnit._game_item_in_inventory.item_name;
                    _item_Description.text = giftUnit._game_item_in_inventory.item_info.desc;
                    _sell_price.text = "喵币 " + giftUnit._game_item_in_inventory.item_info.sell_price.ToString();
                    quantity.text = "库存 " + giftUnit._game_item_in_inventory._item_count.ToString();
                    //_obtain_date.text = "获取时间 : " + g_i_u._game_item_in_inventory._obtain_date.ToString();
                    switch (giftUnit._game_item_in_inventory.item_info.rarity)
                    {
                        case Enum_RarityType.Common:
                            normalRarity.SetActive(true);
                            rareRarity.SetActive(false);
                            previousRarity.SetActive(false);
                            break;
                        case Enum_RarityType.Rare:
                            normalRarity.SetActive(false);
                            rareRarity.SetActive(true);
                            previousRarity.SetActive(false);
                            break;
                        case Enum_RarityType.Precious:
                            normalRarity.SetActive(false);
                            rareRarity.SetActive(false);
                            previousRarity.SetActive(true);
                            break;
                        default:
                            normalRarity.SetActive(false);
                            rareRarity.SetActive(false);
                            previousRarity.SetActive(false);
                            break;
                    }

                    giftDescription.SetActive(true);
                }
            }
        }
    }
}