using System.Collections;
using System.IO;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel.Social;
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

        InitHeader();
        InitEmojis();
        InitGifts();
        ShowChatView();

        SM.update_social_chat_msg_from_server(_record.friend_name);
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
        friendName.text = _record._brief_info._player_nick_name;
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
            Global_Photo_Manager.Instance.Load_Dispatch_PhotoList();
            if (Dispatch_Manager._instance != null)
            {
                foreach (string path in Global_Photo_Manager.Instance._dispatch_photo_path_list)
                {
                    string fileName = Path.GetFileName(path);
                    if (fileName.StartsWith($"#PhotoWith{_record.friend_name}#"))
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

        foreach (var chatRecord in _record._chat_msg)
        {
            bool isReceive = chatRecord._msg_sender == _record.friend_name;
            bool isGift = !string.IsNullOrEmpty(chatRecord._msg_good_item_name);

            GameObject prefab;
            if (isGift)
                prefab = isReceive ? receiveGiftPrefab : sendGiftPrefab;
            else
                prefab = isReceive ? receiveMsgPrefab : sendMsgPrefab;

            GameObject msgObj = Instantiate(prefab, messageRoot);
            var msgComp = msgObj.GetComponent<MessageUnit>();

            if (!isGift && msgComp != null)
            {
                var msgContent = chatRecord._msg_content;
                if (msgContent != null && !string.IsNullOrEmpty(msgContent.res_url))
                {
                    var dbMsg = chatMsgSO.chat_msg_db.Find(m => m.msg_id == msgContent.msg_id);
                    if (dbMsg != null && !string.IsNullOrEmpty(dbMsg.res_url))
                    {
                        string[] _image_url_data = dbMsg.res_url.Split('#');
                        if (_image_url_data.Length == 2)
                        {
                            Project_Mouse_Resource_Management.load_sub_sprite(_image_url_data[0], _image_url_data[1], (sprite) =>
                            {
                                msgComp.message.sprite = sprite;
                            });
                        }
                        else
                        {
                            Project_Mouse_Resource_Management.load_sprite_async(_image_url_data[0], (sprite) =>
                            {
                                msgComp.message.sprite = sprite;
                            });
                        }
                    }
                }
            }
        }

        ScrollToBottom();
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
            Global_Photo_Manager.Instance.try_load_image(giftUnit.localPhotoPath, photoGift);
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

            SM.on_send_social_chat_msg(_record.friend_name, _selectedEmoji.id);
        }
        else if (txtSendBox.text == "送给好友照片")
        {
            SendPhoto();
        }
        else
        {
            if (_selectedGift == null) return;

            Instantiate(sendGiftPrefab, messageRoot);
            SM.on_send_present_to_friend(_record.friend_name,
                _selectedGift._game_item_in_inventory.item_name);

            _selectedGift._game_item_in_inventory._item_count--;
            if (Global_Inventory_Manager._instance != null)
                Global_Inventory_Manager._instance.Send_inventory_to_server();
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
