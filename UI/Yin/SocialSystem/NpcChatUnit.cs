using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLIP.Project_Mouse.Kernel;
using CLIP.Framework_Unity.Asset;
using CLIP.Framework_Core.Event;

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
                GameAssets.Instance.GetAssetByKeyword<Sprite>(s => icon.sprite = s, "qun".ToUpper(), "icon".ToUpper());
                avatarButton.interactable = false;
                intimacy.SetActive(false);
                return;
            }
            chatTextText.gameObject.SetActive(false);
            chatNameText.text = info._npc_Base.npc_name;
            // 头像
            // chatText
            GameAssets.Instance.GetAssetByKeyword<Sprite>(s => icon.sprite = s, info._npc_Base.icon_resource_name);
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
            //FriendsCanvasInteractManager.Instance.OpenChatPanel(this);
            //NPCChatPanelMgr.Instance.OpenChatPanel(this);
            // 再打开面板
            UIManager.Instance.GetPanel<SocialPanel>().OpenChatPanel(info._npc_Base, isGroupChat);
            if (!isGroupChat) EvtDsp.TriggerEvt<NPC_Info>(EvtNames.Check_Chat_Play, info);
        }
    }

}