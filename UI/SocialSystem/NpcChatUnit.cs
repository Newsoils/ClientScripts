using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLIP.Project_Mouse.Kernel;
using CLIP.Framework_Unity.Asset;

namespace CLIP.Project_Mouse.UI
{
    public class NpcChatUnit : MonoBehaviour
    {
        public bool isGroupChat = false;
        public NPC_Info info;

        [Header("UI")]
        public Image icon;
        public Button avatarButton;
        public TMP_Text chatNameText;
        public TMP_Text chatTextText;
        public GameObject intimacy;
        public TMP_Text intimacyText;
        public Button chatButton;


        private void Start()
        {
            avatarButton.onClick.AddListener(NpcDetail);
            chatButton.onClick.AddListener(OpenChatPanel);
        }

        void OnDestroy()
        {
            avatarButton.onClick.RemoveListener(NpcDetail);
            chatButton.onClick.RemoveListener(OpenChatPanel);
        }

        public void InitNpcChatUnit()
        {
            if (isGroupChat)
            {
                // 兜底：保持旧的关键字匹配（可能会误命中）
                GameAssets.Instance.LoadAndSet<Sprite>(ResKeys.ASSET_QUN_ICON, s => icon.sprite = s);
                avatarButton.interactable = false;
                intimacy.SetActive(false);
                return;
            }
            chatTextText.gameObject.SetActive(false);
            chatNameText.text = info._npc_Base.npc_name;
            // 头像
            // chatText
            GameAssets.Instance.LoadAndSet<Sprite>(info._npc_Base.icon_resource_name, s => icon.sprite = s);
            intimacy.SetActive(true);
            intimacyText.text = info._npc_RuntimeData.favor_level.ToString();
        }

        public void NpcDetail()
        {
            if (!isGroupChat)
            {
                UIManager.Instance.GetPanel<SocialPanel>().OpenNPCDetail(info);
            }
        }

        // 挂载在每个用于打开 NPC 聊天窗口的按钮单元上
        public void OpenChatPanel()
        {
            UIManager.Instance.GetPanel<SocialPanel>().OpenNPCChatPanel(info, isGroupChat);
        }
    }

}