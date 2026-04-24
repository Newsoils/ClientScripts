using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel.Social;
using RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

namespace CLIP.Project_Mouse.UI
{
    public class Emoji : MonoBehaviour
    {
        public int id;
        public string text;
        public Image image;

        [HideInInspector]
        public Sprite mSprite;
        [HideInInspector]
        public Social_Chat_Msg social_Chat_Msg;

        public void InitEmoji(Social_Chat_Msg social_Chat_Msg)
        {
            this.social_Chat_Msg = social_Chat_Msg;
            text = $"[{social_Chat_Msg.msg_name}]";
            id = social_Chat_Msg.msg_id;

            if (!string.IsNullOrEmpty(social_Chat_Msg.res_url))
            {
                string[] _image_url_data = social_Chat_Msg.res_url.Split('#');
                if (_image_url_data.Length == 2)
                {
                    RM.load_sub_sprite(_image_url_data[0], _image_url_data[1], (sprite) =>
                    {
                        this.mSprite = sprite;
                        image.sprite = sprite;
                        if (sprite == null)
                        {
                            Debug.LogWarning($"[Emoji] 加载资源失败: {social_Chat_Msg.res_url}");
                        }
                    });
                }
                else
                {
                    RM.load_sprite_async(_image_url_data[0], (sprite) =>
                    {
                        this.mSprite = sprite;
                        image.sprite = sprite;
                        if (sprite == null)
                        {
                            Debug.LogWarning($"[Emoji] 加载资源失败: {social_Chat_Msg.res_url}");
                        }
                    });
                }
            }
        }

        public void OnClick()
        {
            var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
            var friendChatPanel = socialPanel != null ? socialPanel.friendChatPanel : null;
            if (friendChatPanel == null) return;
            friendChatPanel.OnSelectEmoji(this);
        }
    }

}

