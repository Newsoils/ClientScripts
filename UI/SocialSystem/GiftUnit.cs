using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PM_RM = CLIP.Framework_Unity.Asset.GameAssets;

namespace CLIP.Project_Mouse.UI
{
    public class GiftUnit : MonoBehaviour
    {
        [HideInInspector]
        public Game_Item_In_Inventory _game_item_in_inventory;

        public Image Image_Icon;

        public TMP_Text Item_Name;
        public TMP_Text Item_Count;

        public Image selectBg;

        public bool isPhoto = false;
        public string localPhotoPath = "";

        public void InitGiftUnit(Game_Item_In_Inventory game_item_in_inventory)
        {
            isPhoto = false;
            localPhotoPath = "";
            _game_item_in_inventory = game_item_in_inventory;
            Item_Name.text = game_item_in_inventory.item_name;
            Item_Count.text = game_item_in_inventory._item_count.ToString();

            _on_exit_selected();


            var resUrl = game_item_in_inventory.item_info.res_url;
            if (string.IsNullOrEmpty(resUrl)) return;

            var _image_url_data = resUrl.Split("#");
            if (_image_url_data.Length != 2)
            {
                GameAssets.LoadAsync<Sprite>(resUrl, _set_sprite);
            }
            else
            {
                PM_RM.LoadSubSprite(_image_url_data[0], _image_url_data[1], _set_sprite);

            }

        }

        public void InitPhoto(string path)
        {
            _game_item_in_inventory = null;
            Image_Icon.sprite = null;
            isPhoto = true;
            Item_Name.text = "照片";
            Item_Count.text = "1";
            _on_exit_selected();
            localPhotoPath = path;
        }

        public void ShowDetail()
        {
            var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
            var friendChatPanel = socialPanel != null ? socialPanel.friendChatPanel : null;
            if (friendChatPanel == null) return;
            UIManager.Instance.OpenPanel<ItemDescriptionPanel>(_game_item_in_inventory);
        }

        public void toggle_selection()
        {
            var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
            var friendChatPanel = socialPanel != null ? socialPanel.friendChatPanel : null;
            if (friendChatPanel == null) return;
            friendChatPanel.OnSelectGift(this);
        }

        public void _set_sprite(Sprite sp)
        {
            Image_Icon.sprite = sp;
        }

        public void _on_enter_selected()
        {
            Color c = selectBg.color;
            c.a = 1f;
            selectBg.color = c;
        }
        public void _on_exit_selected()
        {
            Color c = selectBg.color;
            c.a = 0f;
            selectBg.color = c;
        }
    }
}

