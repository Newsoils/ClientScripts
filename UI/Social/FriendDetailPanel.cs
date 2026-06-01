using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 好友详情弹窗面板，继承 UIPanelBase。
/// OpenPanel 时传入 Friend_Social_Record 作为参数。
/// </summary>
public class FriendDetailPanel : MonoBehaviour
{
    [Header("Panel Root")]
    public GameObject panelRoot;

    public Image avatarIcon;

    [Header("Info")]
    public TMP_Text txtFriendName;
    public TMP_Text txtFriendID;
    public TMP_Text txtIntimacy;

    [Header("Buttons")]
    public Button btnAddOrDelete;

    //public Button btnVisit;
    //public Button btnChat;
    public Button btnClose;
    public Button btnCopyID;

    [Header("Request Buttons (pending friend)")]
    public GameObject requestButtonGroup;
    public Button btnAccept;
    public Button btnRefuse;

    public Sprite add;
    public Sprite delete;

    private Friend_Social_Record _record;

    /// <summary>
    /// 是否已经是好友，是则显示删除按钮图片，否则显示添加
    /// </summary>
    private bool isFriend;

    Player_Social_Manager SM => Player_Social_Manager._instance;

    #region Lifecycle

    private void Start()
    {
        btnClose.onClick.AddListener(ClosePanel);
        btnAddOrDelete.onClick.AddListener(OnAddOrDelete);
        //btnVisit.onClick.AddListener(OnVisit);
        //btnChat.onClick.AddListener(OnChat);
        btnCopyID.onClick.AddListener(OnCopyID);
        btnAccept.onClick.AddListener(OnAccept);
        btnRefuse.onClick.AddListener(OnRefuse);
    }

    public void OnDestroy()
    {
        btnClose.onClick.RemoveAllListeners();
        btnAddOrDelete.onClick.RemoveAllListeners();
        //btnVisit.onClick.RemoveAllListeners();
        //btnChat.onClick.RemoveAllListeners();
        btnCopyID.onClick.RemoveAllListeners();
        btnAccept.onClick.RemoveAllListeners();
        btnRefuse.onClick.RemoveAllListeners();
    }

    #endregion

    #region UIPanelBase

    public void OpenPanel(params object[] data)
    {
        if (data == null || data.Length == 0 || data[0] is not Friend_Social_Record record)
        {
            Debug.LogWarning("[FriendDetailPanel] OpenPanel requires a Friend_Social_Record parameter");
            return;
        }

        _record = record;
        panelRoot.SetActive(true);
        RefreshDetail();
    }

    public void ClosePanel()
    {
        panelRoot.SetActive(false);
        _record = null;
    }

    #endregion

    #region Refresh

    private void RefreshDetail()
    {
        txtFriendName.text = _record.DisplayName;
        txtFriendID.text = _record.FriendId;
        txtIntimacy.text = _record.Info.RoleLv.ToString();

        var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();

        //设置头像，暂时不需要使用统一头像
        //if (socialPanel != null && avatarIcon != null)
        //    avatarIcon.sp = socialPanel.GetDefaultAvatar();

        bool isPending = SM.IsInApplyList(_record.RoleId);
        bool isAccepted = SM._current_social_info._friend_accepted_info_record
            .Exists(f => f.RoleId == _record.RoleId);

        isFriend = isAccepted;

        if (isPending)
        {
            btnAddOrDelete.gameObject.SetActive(false);
            requestButtonGroup.SetActive(true);
            //btnVisit.gameObject.SetActive(false);
            //btnChat.gameObject.SetActive(false);
        }
        else
        {
            btnAddOrDelete.gameObject.SetActive(true);
            requestButtonGroup.SetActive(false);

            if (isAccepted)
            {
                //btnVisit.gameObject.SetActive(true);
                //btnChat.gameObject.SetActive(true);
                btnAddOrDelete.image.sprite = delete;
            }
            else
            {
                //btnVisit.gameObject.SetActive(false);
                //btnChat.gameObject.SetActive(false);
                btnAddOrDelete.image.sprite = add;
            }
        }

    }



    #endregion

    #region Button Callbacks

    private void OnAddOrDelete()
    {
        if (_record == null) return;

        var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
        if (socialPanel == null) return;

        if (isFriend)
        {
            socialPanel.RemoveFriend(_record);
            ClosePanel();
        }
        else
        {
            socialPanel.AddFriend(_record.FriendId);
            ClosePanel();
        }
    }

    private void OnVisit()
    {
        if (_record == null) return;
        var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
        socialPanel?.VisitFriend(_record.DisplayName);
        ClosePanel();
    }

    private void OnChat()
    {
        if (_record == null) return;
        ClosePanel();
        UIManager.Instance.GetPanel<SocialPanel>().OpenFriendChatPanel(_record);
    }

    private void OnAccept()
    {
        if (_record == null) return;
        var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
        socialPanel?.AcceptRequest(_record);
        ClosePanel();
    }

    private void OnRefuse()
    {
        if (_record == null) return;
        var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
        socialPanel?.RefuseRequest(_record);
        ClosePanel();
    }

    private void OnCopyID()
    {
        if (_record == null) return;
        GUIUtility.systemCopyBuffer = _record.FriendId;
        Debug.Log($"已复制ID到剪贴板: {_record.FriendId}");
    }

    #endregion
}
