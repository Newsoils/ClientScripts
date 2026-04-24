using System.Collections.Generic;
using System.Diagnostics;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Social;
using CLIP.Project_Mouse.LYC.DialogueSystem;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 社交主面板：好友列表 / 好友请求 / NPC 三个 Tab 页签切换。
/// 继承 UIPanelBase，通过 UIManager 统一管理。
/// 替代旧的 FriendsCanvasInteractManager。
/// </summary>
public class SocialPanel : UIPanelBase
{
    [Header("Panel Root")]
    public GameObject obj;

    [Header("Tab Buttons")]
    public ZButton tabFriends;
    public ZButton tabRequest;
    public ZButton tabNpc;

    [Header("Tab Pages")]
    public FriendPanel friendPanel;
    public FriendRequestPanel friendRequestPanel;
    public NPCPanel npcPanel;

    public FriendDetailPanel friendDetailPanel;
    public FriendChatPanel friendChatPanel;
    public NPCChatPanel npcChatPanel;
    public NpcDetailPanel npcDetailPanel;

    public GameObject requestRedDot;

    public Button exitButton;


    [Header("Default Avatar")]
    public Sprite defaultAvatarSprite;

    private SocialTab _currentTab = SocialTab.Friends;

    private enum SocialTab { Friends, Requests, Npc }

    Player_Social_Manager SM => Player_Social_Manager._instance;

    #region Lifecycle

    private void Start()
    {
        tabFriends.enableHoverScale = true;
        tabNpc.enableHoverScale = true;
        tabRequest.enableHoverScale = true;

        tabFriends.onClick.AddListener(() => SwitchTab(SocialTab.Friends));
        tabRequest.onClick.AddListener(() => SwitchTab(SocialTab.Requests));
        tabNpc.onClick.AddListener(() => SwitchTab(SocialTab.Npc));


        if (SM != null)
        {
            SM._on_refresh_social_state.AddListener(RefreshCurrentTab);
        }

        exitButton.onClick.AddListener(ClosePanel);

        NPCManager.instance.Ask_For_All_NPC_Data();
        NPCChatPanelMgr.Instance.InitNPCFavorRuntimeDatas();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        tabFriends.onClick.RemoveAllListeners();
        tabRequest.onClick.RemoveAllListeners();
        tabNpc.onClick.RemoveAllListeners();

        exitButton.onClick.RemoveAllListeners();


        if (SM != null)
        {
            SM._on_refresh_social_state.RemoveListener(RefreshCurrentTab);
        }
    }

    #endregion

    #region UIPanelBase

    public override void OpenPanel(params object[] data)
    {
        obj.SetActive(true);
        SwitchTab(SocialTab.Friends);
        EvtDsp.TriggerEvt(EvtNames.Show_TopPanel_Close_Other);
    }

    public override void ClosePanel()
    {
        obj.SetActive(false);
        EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active);
    }

    #endregion

    #region Tab
    private void SwitchTab(SocialTab tab)
    {
        _currentTab = tab;

        if (tab == SocialTab.Friends)
        {
            friendPanel.OpenPanel();
        }
        else
        {
            friendPanel.ClosePanel();
        }

        if(tab == SocialTab.Requests) 
        {
            friendRequestPanel.OpenPanel();
        }
        else
        {
            friendRequestPanel.ClosePanel();
        }

        if(tab == SocialTab.Npc) 
        {
            npcPanel.OpenPanel();
        }
        else
        {
            npcPanel.ClosePanel();
        }

        RefreshCurrentTab();
    }

    private void RefreshCurrentTab()
    {
        if (SM == null) return;
        switch(_currentTab)
            {
            case SocialTab.Requests:
                friendRequestPanel.RefreshPanel();
                break;
            case SocialTab.Friends:
                friendPanel.RefreshPanel();
                break;
            case SocialTab.Npc:
                npcPanel.RefreshNPCList();
                break;
        }
        UpdateRequestRedDot();
    }

    #endregion


    #region Request

    private void UpdateRequestRedDot()
    {
        if (requestRedDot == null) return;
        var pending = SM._current_social_info._friend_pending_info_record;
        requestRedDot.SetActive(pending != null && pending.Count > 0);
    }

    #endregion

    #region Public API (供子组件调用)

    public void AcceptRequest(Friend_Social_Record record)
    {
        SM.on_confirm_friend(record.friend_name);
        SM._current_social_info._friend_accepted_info_record.Add(record);
        SM._current_social_info._friend_pending_info_record.Remove(record);
        RefreshCurrentTab();
        friendRequestPanel.RefreshPanel();
        friendPanel.RefreshPanel();
    }

    public void RefuseRequest(Friend_Social_Record record)
    {
        SM.on_refuse_friend(record.friend_name);
        SM._current_social_info._friend_pending_info_record.Remove(record);
        RefreshCurrentTab();
        friendRequestPanel.RefreshPanel();
    }

    public void RemoveFriend(string friendName)
    {
        SM.on_remove_friend(friendName);
        SM._current_social_info._friend_accepted_info_record.RemoveAll(f => f.friend_name == friendName);
        RefreshCurrentTab();
        friendPanel.RefreshPanel();
    }

    public void AddFriend(string friendName)
    {
        SM.on_try_add_friend(friendName);
    }

    public void OpenFriendDetail(Friend_Social_Record record)
    {
        friendDetailPanel.OpenPanel(record);
    }

    public void OpenFriendChatPanel(Friend_Social_Record record)
    {
        friendChatPanel.OpenPanel(record);
    }


    public void OpenNpcChat(NpcChatUnit npcChatUnit)
    {
        // NPC 聊天沿用旧逻辑，如果后续需要也可以迁移
        var npcPanel = FindObjectOfType<NpcChatInteractManager>();
        if (npcPanel != null)
        {
            npcPanel.gameObject.SetActive(true);
            npcPanel.InitChatPanel(npcChatUnit);
            NPCManager.instance.Ask_For_All_NPC_Data();
        }
        obj.SetActive(false);
    }

    public void OpenNPCDetail(NPC_Info info)
    {
        npcDetailPanel.OpenPanel(info);
    }

    public void VisitFriend(string friendName)
    {
        ClosePanel();
        SM.set_up_visit_room(friendName);
        SM.on_enter_friend_room(friendName);
    }

    /// <summary>
    /// 获取默认头像 Sprite（暂时所有人用同一张）。
    /// </summary>
    public Sprite GetDefaultAvatar()
    {
        return defaultAvatarSprite;
    }

    #endregion


    


    // ========== 对话面板操作 ==========
    public void OpenChatPanel(NPC_Base NPC_Base, bool isGroupChat)
    {
        npcChatPanel.OpenPanel(NPC_Base, isGroupChat);
    }
}
