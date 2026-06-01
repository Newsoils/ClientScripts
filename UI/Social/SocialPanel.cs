using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Social;
using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.UI;


public enum SocialTab { Friends, Requests, Npc }

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
    public GameObject tabs;

    public Button exitButton;


    [Header("Default Avatar")]
    public Sprite defaultAvatarSprite;

    private SocialTab _currentTab = SocialTab.Friends;
    private Player_Social_Manager _boundSocialManager;


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


        TryBindSocialRefresh();

        exitButton.onClick.AddListener(ClosePanel);

        SwitchTab(SocialTab.Friends);
        obj.SetActive(false);
        //NPCManager.Instance.Ask_For_All_NPC_Data();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        tabFriends.onClick.RemoveAllListeners();
        tabRequest.onClick.RemoveAllListeners();
        tabNpc.onClick.RemoveAllListeners();

        exitButton.onClick.RemoveAllListeners();


        UnbindSocialRefresh();
    }

    #endregion

    #region UIPanelBase

    public override void OpenPanel(params object[] data)
    {
        obj.SetActive(true);
        TryBindSocialRefresh();
        SM?.on_update_social_info_from_server();
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
    public void SwitchTab(SocialTab tab)
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

        if (tab == SocialTab.Requests)
        {
            friendRequestPanel.OpenPanel();
        }
        else
        {
            friendRequestPanel.ClosePanel();
        }

        if (tab == SocialTab.Npc)
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
        switch (_currentTab)
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

    private void TryBindSocialRefresh()
    {
        if (_boundSocialManager != null)
            return;

        if (SM == null)
            return;

        _boundSocialManager = SM;
        _boundSocialManager._on_refresh_social_state.AddListener(RefreshCurrentTab);
    }

    private void UnbindSocialRefresh()
    {
        if (_boundSocialManager == null)
            return;

        _boundSocialManager._on_refresh_social_state.RemoveListener(RefreshCurrentTab);
        _boundSocialManager = null;
    }


    #endregion


    #region Request

    private void UpdateRequestRedDot()
    {
        if (requestRedDot == null) return;
        var pending = SM.ApplyInfos;
        requestRedDot.SetActive(pending != null && pending.Count > 0);
    }

    #endregion

    #region Public API (供子组件调用)

    public void AcceptRequest(Friend_Social_Record record)
    {
        SM.on_confirm_friend(record.FriendId);
    }

    public void RefuseRequest(Friend_Social_Record record)
    {
        SM.on_refuse_friend(record.FriendId);
    }

    public void RemoveFriend(Friend_Social_Record record)
    {
        if (record == null) return;
        SM.on_remove_friend(record.RoleId.ToString());
    }

    public void AddFriend(string friendId)
    {
        SM.on_try_add_friend(friendId);
    }

    public void OpenFriendDetail(Friend_Social_Record record)
    {
        friendDetailPanel.OpenPanel(record);
    }

    public void OpenFriendChatPanel(Friend_Social_Record record)
    {
        friendPanel.ClosePanel();
        friendChatPanel.OpenPanel(record);
    }
    public void OpenNPCChatPanel(NPC_Info info, bool isGroupChat)
    {
        npcPanel.ClosePanel();
        npcChatPanel.OpenPanel(info, isGroupChat);
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

    public void SetTabsActive(bool active)
    {
        tabs.SetActive(active);
    }

    #endregion





    // ========== 对话面板操作 ==========
   
}
