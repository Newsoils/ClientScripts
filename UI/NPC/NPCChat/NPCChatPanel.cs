using System.Collections.Generic;
using CLIP.Framework_Unity.Asset;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// NPC 聊天面板视图层。持有子视图，负责 Sprite 加载、事件注册与转发。
    /// 所有对话数据加载和播放逻辑统一由 ChatViewController 处理。
    /// </summary>
    public class NPCChatPanel : UIPanelBase
    {
        #region Child Views

        [Header("根节点")]
        public GameObject root;

        [Header("标题栏")]
        [SerializeField] private TMP_Text npcNameText;

        [Header("关闭按钮")]
        public Button btnClose;

        [Header("子视图")]
        public ChatViewController chatViewController;
        public OptionPanel optionPanel;

        [Header("头像资源")]
        [SerializeField] private Sprite xiaoTaiHeadIcon;

        #endregion

        #region Internal State

        private NPC_Info _currentNpcInfo;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            btnClose.onClick.AddListener(OnCloseClicked);
            RegisterEvents();
        }

        public override void OnDestroy()
        {
            btnClose.onClick.RemoveAllListeners();
            UnregisterEvents();
            base.OnDestroy();
        }

        #endregion

        #region Event Registration

        private void RegisterEvents()
        {
            //EvtDsp.AddEvt<NPC_Info>(EvtNames.Evt_NPCChat_Open, OnChatOpen);
            EvtDsp.AddEvt<EvtData_OpenOption>(EvtNames.Evt_NPCChat_OpenOption, OnOpenOption);
            EvtDsp.AddEvt(EvtNames.Evt_NPCChat_CloseOption, OnCloseOption);
        }

        private void UnregisterEvents()
        {
            //EvtDsp.RemoveEvt<NPC_Info>(EvtNames.Evt_NPCChat_Open, OnChatOpen);
            EvtDsp.RemoveEvt<EvtData_OpenOption>(EvtNames.Evt_NPCChat_OpenOption, OnOpenOption);
            EvtDsp.RemoveEvt(EvtNames.Evt_NPCChat_CloseOption, OnCloseOption);
        }

        #endregion

        #region Event Handlers

        ///// <summary>
        ///// 1. 展示面板
        ///// 2. 异步加载 NPC 头像
        ///// 3. 头像加载完成后调用 ChatViewController.OpenChat 统一驱动后续数据加载
        ///// </summary>
        //private void OnChatOpen(NPC_Info info)
        //{
        //    _currentNpcInfo = info;
        //    root.SetActive(true);
        //    UpdateInfo(info._npc_Base, isGroupChat: false);

        //    LoadNpcIconAsync(info._npc_Base.icon_resource_name, sprite =>
        //    {
        //        chatViewController?.OpenChat(sprite, info);
        //    });

        //    optionPanel?.ClosePanel();
        //}

        private void OnOpenOption(EvtData_OpenOption data)
        {
            optionPanel?.OpenPanel();
            optionPanel?.DisplayOptions(data.Dialogue, data.ButtonHandlers);
        }

        private void OnCloseOption()
        {
            optionPanel?.ClosePanel();
            optionPanel?.ClearOptions();
        }

        private void OnCloseClicked()
        {
            ClosePanel();
            var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
            socialPanel.SwitchTab(SocialTab.Npc);
        }

        #endregion

        #region Sprite Loading

        private void LoadNpcIconAsync(string iconResourceName, UnityEngine.Events.UnityAction<Sprite> onLoaded)
        {
            if (string.IsNullOrEmpty(iconResourceName))
            {
                onLoaded?.Invoke(null);
                return;
            }

            GameAssets.Instance.LoadAndSetByKey<Sprite>(iconResourceName, sprite =>
            {
                onLoaded?.Invoke(sprite);
            });
        }

        #endregion

        #region UIPanelBase

        public override void OpenPanel(params object[] datas)
        {
            try
            {
                var info = datas[0] as NPC_Info;
                 
                bool isGroupChat = (bool)datas[1];
                root.SetActive(true);
                UpdateInfo(info._npc_Base, isGroupChat);

                UIManager.Instance.GetPanel<SocialPanel>().SetTabsActive(false);

                _currentNpcInfo = info;
                LoadNpcIconAsync(info._npc_Base.icon_resource_name, sprite =>
                {
                    chatViewController?.OpenChat(sprite, info, () =>
                    {
                        PromptManager.ShowPrompt(PromptId.NPCNoNewDialogue, null, info._npc_Base.npc_name);
                    });
                });

                optionPanel?.ClosePanel();
        
            }
            catch
            {
                Debug.LogError("打开NPCChatPanel失败，参数错误");
            }
        }

        public override void ClosePanel()
        {
            chatViewController?.StopPlayback();
            root.SetActive(false);
            //optionPanel?.gameObject.SetActive(false);
            optionPanel?.ClearOptions();
            UIManager.Instance.GetPanel<SocialPanel>().SetTabsActive(true);
        }

        #endregion

        #region Private Helpers

        private void UpdateInfo(NPC_Base npcBase, bool isGroupChat)
        {
            npcNameText.text = isGroupChat ? "小苔的打工群" : npcBase.npc_name;
        }

        #endregion
    }
}
