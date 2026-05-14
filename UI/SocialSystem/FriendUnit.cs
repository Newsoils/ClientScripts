using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class FriendUnit : MonoBehaviour
    {
        [HideInInspector]
        public Friend_Social_Record record;

        //public Image headIcon;
        public TMP_Text nameText;
        public Image hot;
        public TMP_Text hotText;
        //public List<Image> achievementIconList = new List<Image>();

        public void InitFriendUnit()
        {
            nameText.text = record._brief_info._player_nick_name;
            if (record.hot_daily_count > 3)
            {
                hot.gameObject.SetActive(true);
                hotText.text = record.hot_daily_count.ToString();
            }
            else
            {
                hot.gameObject.SetActive(false);
            }
        }

        public void FriendDetail()
        {
            UIManager.Instance.GetPanel<SocialPanel>().OpenFriendDetail(record);
        }

        public void OpenChatPanel()
        {
            UIManager.Instance.GetPanel<SocialPanel>().OpenFriendChatPanel(record);
        }
    }

}