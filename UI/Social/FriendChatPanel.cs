using System.Collections;
using System.IO;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel.Social;
using Common;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 好友聊天面板，继承 UIPanelBase。
/// OpenPanel 时传入 Friend_Social_Record 作为参数。
/// 替代旧的 FriendsChatInteractManager。
/// </summary>
public class FriendChatPanel : MonoBehaviour
{
    [Header("Data")]
    public Social_Chat_Msg_SO chatMsgSO;

    [Header("UI - Header")]
    public GameObject Obj;
    public TMP_Text friendName;
    public GameObject hotObj;
    public TMP_Text hotCount;
    public Button exitButton;

    [Header("UI - Chat Messages")]
    public ScrollRect chatScrollRect;
    public Transform messageRoot;
    public GameObject sendMsgPrefab;
    public GameObject receiveMsgPrefab;
    public GameObject sendGiftPrefab;
    public GameObject receiveGiftPrefab;

    public GameObject chatScrollView;
    public GameObject giftScrollView;

    [Header("UI - Emoji")]
    public Transform emojiRoot;
    public GameObject emojiPrefab;

    [Header("UI - Gift")]
    public Transform giftRoot;
    public GameObject giftPrefab;
    public GiftDescription giftDescription;
    public RawImage photoGift;

    [Header("UI - Send Box")]
    public TMP_Text txtSendBox;
    public Button btnSend;
    public Button btnDelete;

    private Friend_Social_Record _record;
    private Emoji _selectedEmoji;
    private GiftUnit _selectedGift;
    private Texture2D _selectedPhotoTexture;

    Player_Social_Manager SM => Player_Social_Manager._instance;

    #region Lifecycle

    private void Start()
    {
        exitButton.onClick.AddListener(ClosePanel);

        btnSend.onClick.AddListener(SendMessage);
        btnDelete.onClick.AddListener(ClearSendBox);

        if (SM != null)
        {
            SM._on_refresh_social_chat_msg.AddListener(RefreshChatMessages);
        }
    }

    public void OnDestroy()
    {
        exitButton.onClick.RemoveAllListeners();

        btnSend.onClick.RemoveAllListeners();
        btnDelete.onClick.RemoveAllListeners();

        if (SM != null)
        {
            SM._on_refresh_social_chat_msg.RemoveListener(RefreshChatMessages);
        }
    }

    #endregion

    #region UIPanelBase
    public  void OpenPanel(params object[] data)
    {
        if (data == null || data.Length == 0 || data[0] is not Friend_Social_Record record)
        {
            Debug.LogWarning("[FriendChatPanel] OpenPanel requires a Friend_Social_Record parameter");
            return;
        }

        _record = record;
        Obj.SetActive(true);

        SM.SetCurrentChatFriend(_record);

        InitHeader();
        InitEmojis();
        InitGifts();
        ShowChatView();

        SM.update_social_chat_msg_from_server(_record.FriendId);
    }

    public void ClosePanel()
    {
        Obj.SetActive(false);
        _record = null;
        ClearSendBox();

        var socialPanel = UIManager.Instance.GetPanel<SocialPanel>();
        if (socialPanel != null)
            socialPanel.SwitchTab(SocialTab.Friends);
    }

    #endregion

    #region Init

    private void InitHeader()
    {
        friendName.text = _record.DisplayName;
        bool isHot = _record.hot_daily_count > 3;
        hotObj.SetActive(isHot);
        if (isHot) hotCount.text = _record.hot_daily_count.ToString();
    }

    private void InitEmojis()
    {
        var emojiList = chatMsgSO.chat_msg_db;
        foreach(Transform emoji in emojiRoot)
        {
            Destroy(emoji.gameObject);
        }
        for (int i = 0; i < emojiList.Count; i++)
        {
            GameObject go;

            go = Instantiate(emojiPrefab, emojiRoot);

            var emoji = go.GetComponent<Emoji>();
            if (emoji != null)
                emoji.InitEmoji(emojiList[i]);
        }
    }

    private void InitGifts()
    {
        var inventoryList = Global_Inventory_Manager.Items;
        var itemDb = Global_Inventory_Manager.GameItem_DB;
        int giftChildCount = giftRoot.childCount;
        int giftIndex = 0;

        foreach (var invItem in inventoryList)
        {
            var dbItem = itemDb.Find(item => item.item_id == invItem.item_id);
            if (dbItem == null || !dbItem.can_be_present) continue;

            GameObject go;
            if (giftIndex < giftChildCount)
            {
                go = giftRoot.GetChild(giftIndex).gameObject;
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(giftPrefab, giftRoot);
            }

            var giftComp = go.GetComponent<GiftUnit>();
            if (giftComp != null)
                giftComp.InitGiftUnit(invItem);

            giftIndex++;
        }

        if (Global_Photo_Manager.Instance != null)
        {
            if (Dispatch_Manager._instance != null)
            {
                foreach (var photoRecord in Global_Photo_Manager.Instance.dispatchList)
                {
                    var path = photoRecord.localPath;
                    string fileName = Path.GetFileName(path);
                    if (fileName.StartsWith($"#PhotoWith{_record.DisplayName}#"))
                    {
                        GameObject go;
                        if (giftIndex < giftChildCount)
                        {
                            go = giftRoot.GetChild(giftIndex).gameObject;
                            go.SetActive(true);
                        }
                        else
                        {
                            go = Instantiate(giftPrefab, giftRoot);
                        }
                        var giftComp = go.GetComponent<GiftUnit>();
                        if (giftComp != null)
                            giftComp.InitPhoto(path);
                        giftIndex++;
                    }
                }
            }
        }

        for (int i = giftIndex; i < giftChildCount; i++)
            giftRoot.GetChild(i).gameObject.SetActive(false);
    }

    #endregion

    #region Chat Messages
    public void RefreshChatMessages()
    {
        if (_record == null) return;

        for (int i = messageRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(messageRoot.GetChild(i).gameObject);

        var messages = _record.ChatInfo?.Content;
        if (messages == null || messages.Count == 0)
            return;

        foreach (var chatMsg in messages)
        {
            bool isReceive = chatMsg.Person?.RoleInfo?.RoleID == _record.RoleId
                || chatMsg.Person?.RoleInfo?.RoleName == _record.DisplayName;
            bool isExpression = chatMsg.MsgType == ChatMsgType.Expression;
            bool isGift = chatMsg.MsgType != ChatMsgType.Text && !isExpression;

            GameObject prefab;
            if (isGift)
                prefab = isReceive ? receiveGiftPrefab : sendGiftPrefab;
            else
                prefab = isReceive ? receiveMsgPrefab : sendMsgPrefab;

            GameObject msgObj = Instantiate(prefab, messageRoot);
            var msgComp = msgObj.GetComponent<MessageUnit>();

            if (!isGift && msgComp != null && !string.IsNullOrEmpty(chatMsg.Text))
                ApplyChatImageSprite(msgComp, chatMsg.Text);
        }

        ScrollToBottom();
    }

    /// <summary>
    /// 将聊天 Text（res_url）加载为 <see cref="MessageUnit.message"/> 精灵。
    /// 含 <c>#</c> 时为「图集路径#子精灵名」，否则为完整 Resources 精灵路径（与 <see cref="Emoji.InitEmoji"/> 一致）。
    /// </summary>
    private void ApplyChatImageSprite(MessageUnit msgComp, string text)
    {
        if (msgComp == null || string.IsNullOrEmpty(text))
            return;

        LoadSpriteFromResUrl(msgComp, text);
    }

    private static void LoadSpriteFromResUrl(MessageUnit msgComp, string resUrl)
    {
        if (msgComp == null || string.IsNullOrEmpty(resUrl))
            return;

        if (resUrl.Contains("#"))
        {
            string[] parts = resUrl.Split('#');
            if (parts.Length < 2)
                return;

            Project_Mouse_Resource_Management.load_sub_sprite(parts[0], parts[1], sprite =>
            {
                if (msgComp != null && sprite != null)
                    msgComp.message.sprite = sprite;
            });
            return;
        }

        Project_Mouse_Resource_Management.load_sprite_async(resUrl, sprite =>
        {
            if (msgComp != null && sprite != null)
                msgComp.message.sprite = sprite;
        });
    }

    private void ScrollToBottom()
    {
        chatScrollRect.verticalNormalizedPosition = 0f;
        chatScrollRect.verticalScrollbar.value = 0f;
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        chatScrollRect.verticalNormalizedPosition = 0f;
        chatScrollRect.verticalScrollbar.value = 0f;
    }

    #endregion

    #region Send Box

    public void OnSelectEmoji(Emoji emoji)
    {
        if (!string.IsNullOrEmpty(txtSendBox.text)) return;

        _selectedEmoji = emoji;
        txtSendBox.text = emoji.text;
        btnDelete.gameObject.SetActive(true);
    }

    public void OnSelectGift(GiftUnit giftUnit)
    {
        _selectedGift = giftUnit;
        if (giftUnit.isPhoto)
        {
            txtSendBox.text = "送给好友照片";
            StartCoroutine(LoadPhotoTexture(giftUnit.localPhotoPath));
        }
        else
        {
            txtSendBox.text = $"送给好友{giftUnit._game_item_in_inventory.item_name}";
        }
        btnDelete.gameObject.SetActive(true);
    }

    public void ShowGiftDetail(GiftUnit giftUnit)
    {
        if (giftUnit.isPhoto)
        {
            _=Global_Photo_Manager.Instance.LoadImageByPath(giftUnit.localPhotoPath, t=> photoGift.texture =t);
            photoGift.gameObject.SetActive(true);
        }
        else
        {
            giftDescription.InitDescription(giftUnit);
        }
    }

    private void ClearSendBox()
    {
        if (_selectedGift != null)
            _selectedGift._on_exit_selected();

        _selectedEmoji = null;
        _selectedGift = null;
        _selectedPhotoTexture = null;
        txtSendBox.text = "";
        btnDelete.gameObject.SetActive(false);
    }

    private void SendMessage()
    {
        if (string.IsNullOrEmpty(txtSendBox.text) || _record == null) return;

        bool isGiftMsg = txtSendBox.text.StartsWith("送给好友");

        if (!isGiftMsg)
        {
            if (_selectedEmoji == null) return;

            GameObject msgObj = Instantiate(sendMsgPrefab, messageRoot);
            var msgComp = msgObj.GetComponent<MessageUnit>();
            if (msgComp != null)
                msgComp.message.sprite = _selectedEmoji.mSprite;

            SM.on_send_social_chat_msg(_record.DisplayName, _selectedEmoji.id);
        }
        else if (txtSendBox.text == "送给好友照片")
        {
            SendPhoto();
        }
        else
        {
            if (_selectedGift == null) return;

            Instantiate(sendGiftPrefab, messageRoot);
            SM.on_send_present_to_friend(_record.DisplayName,
                _selectedGift._game_item_in_inventory.item_name);

            _selectedGift._game_item_in_inventory._item_count--;
            //if (Global_Inventory_Manager.Instance != null)
            //    // TODO zhaorui
            //    Global_Inventory_Manager.Instance.Send_inventory_to_server();
        }

        ScrollToBottom();
        ClearSendBox();
    }

    private void SendPhoto()
    {
        Instantiate(sendGiftPrefab, messageRoot);
        ScrollToBottom();
    }

    #endregion

    #region View Toggle

    public void ShowChatView()
    {
        chatScrollView.SetActive(true);
        giftScrollView.SetActive(false);
    }

    public void ShowGiftView()
    {
        chatScrollView.SetActive(false);
        giftScrollView.SetActive(true);
    }

    #endregion

    #region Utility

    private IEnumerator LoadPhotoTexture(string photoPath)
    {
        if (!File.Exists(photoPath))
        {
            _selectedPhotoTexture = null;
            yield break;
        }
        byte[] data = File.ReadAllBytes(photoPath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(data))
            _selectedPhotoTexture = tex;
        else
            _selectedPhotoTexture = null;
    }

    #endregion
}
