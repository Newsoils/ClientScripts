using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 好友请求面板的Unit
    /// </summary>
    public class FriendRequestUnit : MonoBehaviour
    {
        [HideInInspector]
        public Friend_Social_Record record;

        public Image avatarIcon;
        public TMP_Text nameText;

        public void InitFriendUnit()
        {
            nameText.text = record._brief_info._player_nick_name;
        }

        public void FriendDetail()
        {
            UIManager.Instance.GetPanel<SocialPanel>().OpenFriendDetail(record);
        }

        public void AcceptRequest()
        {
            UIManager.Instance.GetPanel<SocialPanel>().AcceptRequest(record);
        }

        public void RefuseRequest()
        {
            UIManager.Instance.GetPanel<SocialPanel>().RefuseRequest(record);
        }
    }

}