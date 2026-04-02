using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{

    public class UnAcceptFriendUnit : MonoBehaviour
    {
        [HideInInspector]
        public Friend_Social_Record record;

        public Image headIcon;
        public TMP_Text nameText;
        //public List<Image> achievementIconList = new List<Image>();

        public void InitFriendUnit(string buttonText)
        {
            nameText.text = record._brief_info._player_nick_name;
        }

        public void FriendDetail()
        {
            UIManager.Instance.GetPanel<SocialPanel>().OpenFriendDetail(record);
        }

        public void TryAddFriend()
        {
            Player_Social_Manager._instance.on_try_add_friend(record.friend_id);
        }
    }

}

