using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLIP.Project_Mouse.Kernel.Social;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using System.IO;
using CLIP.Framework_Unity.Asset;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class FriendsChatInteractManager : MonoBehaviour
            {
                [Header("Data")]
                public Social_Chat_Msg_SO social_Chat_Msg_SO;
                public GameItem_DB_SO game_Inventory_SO;
                public Friend_Social_Record record;
                public Emoji sendEmoji;
                public GiftUnit selectedGift;
                public Texture2D selectedPhotoTexture;
                public GiftDescription giftDescription;

                [Header("UI")]
                public TMP_Text friendName;
                public GameObject hot;
                public TMP_Text hotText;
                public Transform emojiRoot;
                public GameObject emojiPrefab;
                public Transform giftRoot;
                public GameObject giftPrefab;
                public TMP_Text inputText;
                public GameObject delete;
                public Transform messageRoot;
                public GameObject sendMessagePrefab;
                public GameObject receiveMessagePrefab;
                public GameObject sendGiftPrefab;
                public GameObject receiveGiftPrefab;
                public GameObject chatScrollView;
                public GameObject giftScrollView;
                public ScrollRect chatScrollRect;
                public RawImage photoGift;

                // 初始化聊天面板
                public void InitChatPanel()
                {
                    inputText.text = "";
                    friendName.text = record.friend_name;
                    if (record.hot_daily_count > 3)
                    {
                        hot.SetActive(true);
                        hotText.text = record.hot_daily_count.ToString();
                    }
                    else
                    {
                        hot.SetActive(false);
                    }

                    // 加载表情信息
                    var emojiList = social_Chat_Msg_SO.chat_msg_db;
                    int emojiCount = emojiList.Count;

                    int childCount = emojiRoot.childCount;

                    for (int i = 0; i < emojiCount; i++)
                    {
                        GameObject emojiObj;
                        if (i < childCount)
                        {
                            emojiObj = emojiRoot.GetChild(i).gameObject;
                            emojiObj.SetActive(true);
                        }
                        else
                        {
                            emojiObj = Instantiate(emojiPrefab, emojiRoot);
                        }

                        var emojiComp = emojiObj.GetComponent<Emoji>();
                        if (emojiComp != null)
                        {
                            emojiComp.InitEmoji(emojiList[i]);
                        }
                    }

                    for (int i = emojiCount; i < childCount; i++)
                    {
                        emojiRoot.GetChild(i).gameObject.SetActive(false);
                    }

                    // 加载礼物信息
                    var inventoryList = Global_Inventory_Manager.Items;
                    var itemDb = Global_Inventory_Manager.GameItem_DB;
                    int giftCount = 0;
                    int giftChildCount = giftRoot.childCount;

                    foreach (var invItem in inventoryList)
                    {
                        var dbItem = itemDb.Find(item => item.item_id == invItem.item_id);
                        if (dbItem != null && dbItem.can_be_present)
                        {
                            GameObject giftObj;
                            if (giftCount < giftChildCount)
                            {
                                giftObj = giftRoot.GetChild(giftCount).gameObject;
                                giftObj.SetActive(true);
                            }
                            else
                            {
                                giftObj = Instantiate(giftPrefab, giftRoot);
                            }

                            var giftComp = giftObj.GetComponent<GiftUnit>();
                            if (giftComp != null)
                            {
                                giftComp.InitGiftUnit(invItem);
                            }
                            giftCount++;
                        }
                    }

                    Global_Photo_Manager.Instance.Load_Dispatch_PhotoList();
                    foreach (string path in Dispatch_Manager._instance._dispatch_photo_path_list)
                    {
                        string fileName = Path.GetFileName(path);
                        if (fileName.StartsWith($"#PhotoWith{friendName.text}#"))
                        {
                            GameObject giftObj;
                            if (giftCount < giftChildCount)
                            {
                                giftObj = giftRoot.GetChild(giftCount).gameObject;
                                giftObj.SetActive(true);
                            }
                            else
                            {
                                giftObj = Instantiate(giftPrefab, giftRoot);
                            }
                            var giftComp = giftObj.GetComponent<GiftUnit>();
                            if (giftComp != null)
                            {
                                giftComp.InitPhoto(path);
                            }
                            giftCount++;
                        }
                    }

                    for (int i = giftCount; i < giftChildCount; i++)
                    {
                        giftRoot.GetChild(i).gameObject.SetActive(false);
                    }

                    // 加载聊天记录
                    Player_Social_Manager._instance.update_social_chat_msg_from_server(record.friend_name);
                    OpenChatScrollView();
                }

                public IEnumerator ResetScrollPosition()
                {
                    yield return null;
                    chatScrollRect.verticalNormalizedPosition = 0f;
                    chatScrollRect.verticalScrollbar.value = 0f;
                }

                // 刷新聊天记录
                public void RefreshChatRecord()
                {
                    if (record == null) return;

                    for (int i = messageRoot.childCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(messageRoot.GetChild(i).gameObject);
                    }

                    foreach (var chatRecord in record._chat_msg)
                    {
                        bool isReceive = chatRecord._msg_sender == record.friend_name;
                        bool isGift = !string.IsNullOrEmpty(chatRecord._msg_good_item_name);

                        GameObject prefab;
                        if (isGift)
                        {
                            prefab = isReceive ? receiveGiftPrefab : sendGiftPrefab;
                        }
                        else
                        {
                            prefab = isReceive ? receiveMessagePrefab : sendMessagePrefab;
                        }

                        GameObject messageObj = Instantiate(prefab, messageRoot);
                        var messageComp = messageObj.GetComponent<MessageUnit>();

                        if (isGift)
                        {
                            // 玩家头像
                        }
                        else
                        {
                            var msgContent = chatRecord._msg_content;
                            if (msgContent != null && !string.IsNullOrEmpty(msgContent.res_url))
                            {
                                var dbMsg = social_Chat_Msg_SO.chat_msg_db.Find(m => m.msg_id == msgContent.msg_id);
                                if (dbMsg != null && !string.IsNullOrEmpty(dbMsg.res_url))
                                {
                                    // 玩家头像
                                    string[] parts = dbMsg.res_url.Split('#');
                                    Project_Mouse_Resource_Management.load_sub_sprite(parts[0], parts[1], (sprite) =>
                                    {
                                        messageComp.message.sprite = sprite;
                                        if (sprite == null)
                                        {
                                            Debug.LogWarning($"[Emoji] 加载资源失败: {dbMsg.res_url}");
                                        }
                                    });
                                }
                            }
                        }
                    }

                    chatScrollRect.verticalNormalizedPosition = 0f;
                    chatScrollRect.verticalScrollbar.value = 0f;

                    StartCoroutine(ResetScrollPosition());
                }

                // 选择表情
                public void OnSelectEmoji(Emoji emoji)
                {
                    if (string.IsNullOrEmpty(inputText.text))
                    {
                        inputText.text = emoji.text;
                        delete.SetActive(true);
                        sendEmoji = emoji;
                    }
                    else
                    {
                        return;
                    }
                }

                // 删除输入框内容
                public void DeleteSendBox()
                {
                    if (selectedGift != null)
                    {
                        selectedGift._on_exit_selected();
                    }
                    sendEmoji = null;
                    selectedGift = null;
                    selectedPhotoTexture = null;
                    inputText.text = "";
                    delete.SetActive(false);
                }

                // 发送消息
                public void SendMessage()
                {
                    if (!string.IsNullOrEmpty(inputText.text))
                    {
                        bool isGiftMsg = inputText.text.StartsWith("送给好友");

                        if (!isGiftMsg)
                        {
                            GameObject messageObj = Instantiate(sendMessagePrefab, messageRoot);
                            var messageComp = messageObj.GetComponent<MessageUnit>();
                            if (messageComp != null)
                            {
                                // 玩家头像
                                messageComp.message.sprite = sendEmoji.mSprite;
                                //Debug.Log("发送信息");
                            }

                            Player_Social_Manager._instance.on_send_social_chat_msg(record.friend_name, sendEmoji.id);

                            chatScrollRect.verticalNormalizedPosition = 0f;
                            chatScrollRect.verticalScrollbar.value = 0f;
                        }
                        else
                        {
                            if (inputText.text == "送给好友照片")
                            {
                                SendPhoto(selectedPhotoTexture);
                            }
                            else
                            {
                                GameObject messageObj = Instantiate(sendGiftPrefab, messageRoot);
                                var messageComp = messageObj.GetComponent<MessageUnit>();
                                if (messageComp != null)
                                {
                                    // 玩家头像
                                }

                                Player_Social_Manager._instance.on_send_present_to_friend(record.friend_name, selectedGift._game_item_in_inventory.item_name);

                                chatScrollRect.verticalNormalizedPosition = 0f;
                                chatScrollRect.verticalScrollbar.value = 0f;

                                selectedGift._game_item_in_inventory._item_count--;
                                if (Global_Inventory_Manager._instance != null)
                                {
                                    Global_Inventory_Manager._instance.Send_inventory_to_server();
                                }
                            }
                        }

                        inputText.text = "";
                        delete.SetActive(false);
                    }
                }

                // 发送照片礼物
                public void SendPhoto(Texture2D texture2D)
                {
                    Debug.Log("发送照片礼物");
                    GameObject messageObj = Instantiate(sendGiftPrefab, messageRoot);
                    var messageComp = messageObj.GetComponent<MessageUnit>();
                    if (messageComp != null)
                    {
                        // 玩家头像
                    }

                    //string photoName = Global_Photo_Manager.Instance.upload_photo_to_server_and_return_photo_name(texture2D);
                    //Player_Social_Manager._instance.on_send_present_to_friend(record.friend_name, photoName);

                    chatScrollRect.verticalNormalizedPosition = 0f;
                    chatScrollRect.verticalScrollbar.value = 0f;
                }

                // 打开聊天界面
                public void OpenChatScrollView()
                {
                    chatScrollView.SetActive(true);
                    giftScrollView.SetActive(false);
                }

                // 打开礼物界面
                public void OpenGiftScrollView()
                {
                    chatScrollView.SetActive(false);
                    giftScrollView.SetActive(true);
                }

                // 显示礼物详情
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

                // 选择礼物
                public void SelectGift(GiftUnit giftUnit)
                {
                    if (giftUnit.isPhoto)
                    {
                        inputText.text = $"送给好友照片";
                        StartCoroutine(LoadPhotoTexture(giftUnit.localPhotoPath));
                    }
                    else
                    {
                        inputText.text = $"送给好友{giftUnit._game_item_in_inventory.item_name}";
                    }
                    selectedGift = giftUnit;
                    delete.SetActive(true);
                }

                public IEnumerator LoadPhotoTexture(string photoPath)
                {
                    if (!System.IO.File.Exists(photoPath))
                    {
                        Debug.LogWarning($"照片文件不存在: {photoPath}");
                        selectedPhotoTexture = null;
                        yield break;
                    }
                    byte[] fileData = System.IO.File.ReadAllBytes(photoPath);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(fileData))
                    {
                        selectedPhotoTexture = tex;
                        Debug.Log("照片已加载为Texture2D");
                    }
                    else
                    {
                        Debug.LogWarning("照片加载失败");
                        selectedPhotoTexture = null;
                    }
                    yield break;
                }
            }
        }
    }
}