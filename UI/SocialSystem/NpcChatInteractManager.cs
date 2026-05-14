using System.Collections;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class NpcChatInteractManager : MonoBehaviour
            {
                [Header("Data")]
                public Social_Chat_Msg_SO social_Chat_Msg_SO;
                public NpcChatUnit npcChatUnit;
                public NpcChatGiftUnit selectedGift;
                public NpcChatGiftDiscription giftDescription;
                public NpcGroupChatDescription groupChatDescription;

                [Header("UI")]
                public TMP_Text chatName;
                public Transform chatTextRoot;
                public GameObject chatTextPrefab;
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
                public GameObject chatDescriptionButton;
                public GameObject chatDescriptionPanel;


                // 初始化聊天面板
                public void InitChatPanel(NpcChatUnit npcChatUnit)
                {
                    this.npcChatUnit = npcChatUnit;
                    inputText.text = "";
                    if (npcChatUnit.info == null || npcChatUnit.isGroupChat)
                    {
                        chatDescriptionButton.SetActive(true);
                        chatName.text = "小苔的打工群";
                    }
                    else
                    {
                        chatDescriptionButton.SetActive(false);
                        chatName.text = npcChatUnit.info._npc_Base.npc_name;
                    }

                    // 加载文字信息

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

                            var giftComp = giftObj.GetComponent<NpcChatGiftUnit>();
                            if (giftComp != null)
                            {
                                giftComp.InitGiftUnit(invItem, this);
                            }
                            giftCount++;
                        }
                    }

                    for (int i = giftCount; i < giftChildCount; i++)
                    {
                        giftRoot.GetChild(i).gameObject.SetActive(false);
                    }

                    // 加载聊天记录

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
                    for (int i = messageRoot.childCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(messageRoot.GetChild(i).gameObject);
                    }

                    // 加载聊天记录

                    chatScrollRect.verticalNormalizedPosition = 0f;
                    chatScrollRect.verticalScrollbar.value = 0f;

                    StartCoroutine(ResetScrollPosition());
                }

                // 选择表情
                public void OnSelectMessage(NpcChatMessage message)
                {
                    if (string.IsNullOrEmpty(inputText.text))
                    {
                        inputText.text = message.text;
                        delete.SetActive(true);
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
                    selectedGift = null;
                    inputText.text = "";
                    delete.SetActive(false);
                }

                // 发送消息
                public void SendMessage()
                {
                    if (!string.IsNullOrEmpty(inputText.text))
                    {
                        bool isGiftMsg = inputText.text.StartsWith("送给你一件礼物！");

                        if (!isGiftMsg)
                        {
                            // 发送文字消息

                            chatScrollRect.verticalNormalizedPosition = 0f;
                            chatScrollRect.verticalScrollbar.value = 0f;
                        }
                        else
                        {
                            GameObject messageObj = Instantiate(sendGiftPrefab, messageRoot);
                            var messageComp = messageObj.GetComponent<MessageUnit>();
                            if (messageComp != null)
                            {
                                // 玩家头像
                            }

                            NPCManager.Instance.Give_Gift_To_NPC(npcChatUnit.info._npc_Base.npc_id, selectedGift.type);

                            chatScrollRect.verticalNormalizedPosition = 0f;
                            chatScrollRect.verticalScrollbar.value = 0f;

                            selectedGift._game_item_in_inventory._item_count--;
                            if (Global_Inventory_Manager._instance != null)
                            {
                                Global_Inventory_Manager._instance.Send_inventory_to_server();
                            }
                        }

                        inputText.text = "";
                        delete.SetActive(false);
                    }
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
                public void ShowGiftDetail(NpcChatGiftUnit giftUnit)
                {
                    giftDescription.InitDescription(giftUnit);
                }

                // 选择礼物
                public void SelectGift(NpcChatGiftUnit giftUnit)
                {
                    inputText.text = $"送给你一件礼物！";
                    selectedGift = giftUnit;
                    delete.SetActive(true);
                }

                public void OpenChatDescription()
                {
                    chatDescriptionPanel.SetActive(true);
                    groupChatDescription.Init();
                }
            }
        }
    }
}