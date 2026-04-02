using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Social;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            // 在现有的 PersonalBriefPanel 中有引用，但是该组件在场景中并没有被激活，暂时不删
            public class FriendsCanvasInteractManager : MonoBehaviour, IPointerDownHandler, IDragHandler
            {
                public static FriendsCanvasInteractManager Instance { get; private set; }

                public bool isSearching = false;
                public Achievement_Design_Info_DB_SO achievementSO;
                [Header("UI")]
                public GameObject friend;
                public GameObject friendCanvas;
                public GameObject friendsItemView;
                public GameObject friendsRequestItemView;
                public GameObject npcItemView;
                public GameObject searchFriend;
                public TMP_InputField searchFriendInputField;
                public GameObject friendUnitRoot;
                public GameObject friendUnitPrefab;
                public GameObject friendRequestRedPot;
                public GameObject friendRequestRoot;
                public GameObject friendRequestUnitPrefab;
                public FriendUnit searchedAcceptedFriend;
                public UnAcceptFriendUnit searchedUnacceptedFriend;
                public GameObject deselectInputButton;
                public GameObject npcDescriptionPanel;
                public GameObject npcChatUnitRoot;
                public GameObject npcChatUnitPrefab;


                [Header("好友详情面板")]
                public GameObject friendDetailPanel;
                public TMP_Text friendNameText;
                public TMP_Text friendIDText;
                public TMP_Text friendIntimacyText;
                public TMP_Text addOrDeletebuttonText;
                public RawImage rawImage;
                public GameObject refuseOrAcceptButton;
                public GameObject visitFriendButton;
                public List<Image> achievementIconList = new List<Image>();
                public List<RawImage> photoList = new List<RawImage>();

                [Header("旋转模特")]
                private Vector2 lastPointerPos;
                public float rotateSpeed = 1f;

                [Header("头像截图")]
                public RenderTexture photoRT;
                public RenderTexture bigPhotoRT;
                public Dictionary<string, string> friendPhotoDict = new Dictionary<string, string>();

                [Header("聊天界面")]
                public FriendsChatInteractManager friendsChatInteractManager;
                public GameObject friendChatPanel;
                public NpcChatInteractManager npcChatInteractManager;
                public GameObject npcChatPanel;

                private void Awake()
                {
                    if (Instance != null && Instance != this)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    Instance = this;
                    DontDestroyOnLoad(friend);
                }

                private void Start()
                {
                    if (Player_Social_Manager._instance != null)
                    {
                        Player_Social_Manager._instance._on_refresh_social_state.AddListener(RefreshUI);
                        Player_Social_Manager._instance._on_refresh_social_chat_msg.AddListener(friendsChatInteractManager.RefreshChatRecord);
                    }
                }

                private void Update()
                {
                    if (Player_Social_Manager._instance != null)
                    {
                        if (Player_Social_Manager._instance._current_social_info._friend_pending_info_record == null || Player_Social_Manager._instance._current_social_info._friend_pending_info_record.Count == 0)
                        {
                            friendRequestRedPot.SetActive(false);
                        }
                        else
                        {
                            friendRequestRedPot.SetActive(true);
                        }
                    }
                }

                // NPC详情
                public void NpcDetail(NPC_Info info)
                {
                    npcDescriptionPanel.SetActive(true);
                    npcDescriptionPanel.GetComponent<NpcDetailPanel>().InitNpcDescription(info);
                }

                // 打开NPC聊天面板
                public void OpenChatPanel(NpcChatUnit npcChatUnit)
                {
                    npcChatPanel.SetActive(true);
                    npcChatInteractManager.InitChatPanel(npcChatUnit);
                    //NPCManager.instance.Load_NPC_static_Data();
                    NPCManager.instance.Ask_For_All_NPC_Data();
                }

                // 访问好友家
                public void VisitFriend()
                {
                    StartCoroutine(VisitFriendCo());
                }

                public IEnumerator VisitFriendCo()
                {
                    friendDetailPanel.SetActive(false);
                    CloseFriendCanvas();
                    Player_Social_Manager._instance.set_up_visit_room(friendNameText.text);
                    Player_Social_Manager._instance.on_enter_friend_room(friendNameText.text);

                    yield return new WaitForSeconds(2.5f);

                    //CommonInteractManager.Instance.FindPhoneUis();
                    //CommonInteractManager.Instance.FindAndDressCharacter();

                    yield return new WaitForSeconds(2.5f);

                    //if (Indoor_Room_Game_Manager._activc_instance != null)
                    //{
                    //    Indoor_Room_Game_Manager._activc_instance.indoorRoomInteractManager.GetComponent<IndoorRoomInteractManager>().RefreshGift();
                    //}
                }

                // 打开聊天面板
                public void OpenChatPanel(Friend_Social_Record record)
                {
                    Global_Photo_Manager.Instance. Load_Dispatch_PhotoList();
                    friendsChatInteractManager.record = record;
                    friendChatPanel.SetActive(true);
                    friendsChatInteractManager.InitChatPanel();
                    friendCanvas.SetActive(false);
                }

                // 关闭好友聊天面板
                public void CloseChatPanel()
                {
                    friendChatPanel.SetActive(false);
                    npcChatPanel.SetActive(false);
                    friendCanvas.SetActive(true);
                    FriendInGame();
                    friendsChatInteractManager.record = null;
                    npcChatInteractManager.npcChatUnit = null;
                }

                // 关闭NPC聊天面板
                public void CloseNpcChatPanel()
                {
                    friendChatPanel.SetActive(false);
                    npcChatPanel.SetActive(false);
                    friendCanvas.SetActive(true);
                    Npc();
                    friendsChatInteractManager.record = null;
                    npcChatInteractManager.npcChatUnit = null;
                }

                // 接受好友请求
                public void AcceptRequest(Friend_Social_Record record)
                {
                    Player_Social_Manager._instance.on_confirm_friend(record.friend_name);
                    Player_Social_Manager._instance._current_social_info._friend_accepted_info_record.Add(record);
                    Player_Social_Manager._instance._current_social_info._friend_pending_info_record.Remove(record);
                    RefreshUI();
                }

                // 拒绝好友请求
                public void RefuseRequest(Friend_Social_Record record)
                {
                    Player_Social_Manager._instance.on_refuse_friend(record.friend_name);
                    Player_Social_Manager._instance._current_social_info._friend_pending_info_record.Remove(record);
                    RefreshUI();
                }

                // 取消选择输入框
                public void DeselectInput()
                {
                    deselectInputButton.SetActive(false);
                    searchFriendInputField.text = "";
                    searchFriendInputField.DeactivateInputField();
                    searchedAcceptedFriend.gameObject.SetActive(false);
                    searchedUnacceptedFriend.gameObject.SetActive(false);
                    FriendInGame();
                    SetIsSearching(false);
                }

                // 搜索好友按钮
                public void SearchFriend()
                {
                    friendsItemView.SetActive(false);
                    friendsRequestItemView.SetActive(false);

                    Player_Social_Manager._instance.on_try_find_friend(searchFriendInputField.text);
                }

                // 显示好友详情
                public void FriendDetail(Friend_Social_Record record)
                {
                    //InitCharacterRotate();
                    //character_Clothes_in_Level.WearSuit(record._cloth_suit);
                    friendNameText.text = record.friend_name;
                    friendIDText.text = record.friend_id;
                    friendIntimacyText.text = record._brief_info._affinity_with_main_character.ToString();
                    var acceptedList = Player_Social_Manager._instance._current_social_info._friend_accepted_info_record;
                    bool isRequested = Player_Social_Manager._instance._current_social_info._friend_pending_info_record.Exists(f => f.friend_id == record.friend_id);
                    if (isRequested)
                    {
                        addOrDeletebuttonText.gameObject.SetActive(false);
                        refuseOrAcceptButton.SetActive(true);
                        visitFriendButton.SetActive(false);
                    }
                    else
                    {
                        addOrDeletebuttonText.gameObject.SetActive(true);
                        refuseOrAcceptButton.SetActive(false);
                        bool isAccepted = acceptedList.Exists(f => f.friend_id == record.friend_id);
                        if (isAccepted)
                        {
                            addOrDeletebuttonText.text = "删除";
                            visitFriendButton.SetActive(true);
                        }
                        else
                        {
                            addOrDeletebuttonText.text = "添加";
                            visitFriendButton.SetActive(false);
                        }
                    }

                    var achievements = record._brief_info.achievements_pinned;
                    for (int i = 0; i < 10; i++)
                    {
                        if (i < achievements.Count)
                        {
                            var info = achievements[i];
                            if (info == null)
                            {
                                achievementIconList[i].sprite = null;
                                continue;
                            }
                            var achievement = achievementSO.achievement_db.Find(x => x.achievement_name == info.achievement_name);
                            if (achievement == null)
                            {
                                achievementIconList[i].sprite = null;
                                continue;
                            }
                            string iconPath = achievement.achievement_icon_res_url;
                            if (!string.IsNullOrEmpty(iconPath) && System.IO.File.Exists(iconPath))
                            {
                                // 加载icon
                            }
                            else
                            {
                                achievementIconList[i].sprite = null;
                            }
                        }
                        else
                        {
                            achievementIconList[i].sprite = null;
                        }
                    }

                    // 相册

                    friendDetailPanel.SetActive(true);
                }

                // 删除或添加好友
                public void DeleteOrAddFriend()
                {
                    if (addOrDeletebuttonText.text == "删除")
                    {
                        Player_Social_Manager._instance.on_remove_friend(friendNameText.text);
                        friendDetailPanel.SetActive(false);
                        FriendInGame();
                        Player_Social_Manager._instance._current_social_info._friend_accepted_info_record.RemoveAll(f => f.friend_name == friendNameText.text);
                    }
                    else
                    {
                        Player_Social_Manager._instance.on_try_add_friend(friendNameText.text);
                        friendDetailPanel.SetActive(false);
                    }
                    RefreshUI();
                }

                // 刷新UI
                public void RefreshUI()
                {
                    // 游戏好友列表
                    var friends = Player_Social_Manager._instance._current_social_info._friend_accepted_info_record;
                    int friendCount = friends.Count;

                    var unitList = new List<GameObject>();
                    for (int i = 0; i < friendUnitRoot.transform.childCount; i++)
                    {
                        unitList.Add(friendUnitRoot.transform.GetChild(i).gameObject);
                    }

                    for (int i = 0; i < friendCount; i++)
                    {
                        GameObject unit;
                        if (i < unitList.Count)
                        {
                            unit = unitList[i];
                            unit.SetActive(true);
                        }
                        else
                        {
                            unit = Instantiate(friendUnitPrefab, friendUnitRoot.transform);
                        }
                        var friendUnitScript = unit.GetComponent<FriendUnit>();
                        friendUnitScript.record = friends[i];
                        friendUnitScript.InitFriendUnit();
                    }

                    for (int i = friendCount; i < unitList.Count; i++)
                    {
                        unitList[i].SetActive(false);
                    }

                    // 搜索好友
                    if (isSearching)
                    {
                        var results = Player_Social_Manager._instance._temp_search_result;
                        if (results.Count > 0)
                        {
                            var id = results[0].friend_id;
                            if (string.IsNullOrWhiteSpace(id))
                            {
                                PromptMessage.Instance.ShowUpPrompt("未找到该玩家");
                            }
                            else
                            {
                                var acceptedList = Player_Social_Manager._instance._current_social_info._friend_accepted_info_record;
                                bool isAccepted = acceptedList.Exists(f => f.friend_id == id);

                                if (isAccepted)
                                {
                                    searchedAcceptedFriend.record = results[0];
                                    searchedAcceptedFriend.InitFriendUnit();
                                    searchedAcceptedFriend.gameObject.SetActive(true);
                                    searchedUnacceptedFriend.gameObject.SetActive(false);
                                }
                                else
                                {
                                    searchedUnacceptedFriend.record = results[0];
                                    searchedUnacceptedFriend.InitFriendUnit("添加");
                                    searchedAcceptedFriend.gameObject.SetActive(false);
                                    searchedUnacceptedFriend.gameObject.SetActive(true);
                                }
                            }
                        }
                        else
                        {
                            PromptMessage.Instance.ShowUpPrompt("未找到该玩家");
                        }
                    }

                    // 好友请求列表
                    if (Player_Social_Manager._instance._current_social_info._friend_pending_info_record == null || Player_Social_Manager._instance._current_social_info._friend_pending_info_record.Count == 0)
                    {
                        friendRequestRedPot.SetActive(false);
                    }
                    else
                    {
                        friendRequestRedPot.SetActive(true);
                    }

                    var friendsRequests = Player_Social_Manager._instance._current_social_info._friend_pending_info_record;
                    int friendRequestCount = friendsRequests.Count;

                    var list = new List<GameObject>();
                    for (int i = 0; i < friendRequestRoot.transform.childCount; i++)
                    {
                        list.Add(friendRequestRoot.transform.GetChild(i).gameObject);
                    }

                    for (int i = 0; i < friendRequestCount; i++)
                    {
                        GameObject unit;
                        if (i < list.Count)
                        {
                            unit = list[i];
                            unit.SetActive(true);
                        }
                        else
                        {
                            unit = Instantiate(friendRequestUnitPrefab, friendRequestRoot.transform);
                        }
                        var friendUnitScript = unit.GetComponent<FriendRequestUnit>();
                        friendUnitScript.record = friendsRequests[i];
                        friendUnitScript.InitFriendUnit();
                    }

                    for (int i = friendRequestCount; i < list.Count; i++)
                    {
                        list[i].SetActive(false);
                    }

                    // NPC列表
                    var npcDict = CLIP.Project_Mouse.Game_Play_System.NPCManager.instance.NPC_Info_Dict;
                    var acquaintedNpcs = new List<NPC_Info>();
                    foreach (var kv in npcDict)
                    {
                        if (kv.Value._npc_RuntimeData != null /*&& kv.Value._npc_RuntimeData.is_acquainted*/)
                            acquaintedNpcs.Add(kv.Value);
                    }

                    var npcUnitList = new List<GameObject>();
                    for (int i = 1; i < npcChatUnitRoot.transform.childCount; i++)
                    {
                        npcUnitList.Add(npcChatUnitRoot.transform.GetChild(i).gameObject);
                    }

                    for (int i = 0; i < acquaintedNpcs.Count; i++)
                    {
                        GameObject unit;
                        if (i < npcUnitList.Count)
                        {
                            unit = npcUnitList[i];
                            unit.SetActive(true);
                        }
                        else
                        {
                            unit = Instantiate(npcChatUnitPrefab, npcChatUnitRoot.transform);
                        }
                        var npcUnitScript = unit.GetComponent<NpcChatUnit>();
                        npcUnitScript.info = acquaintedNpcs[i];
                        npcUnitScript.InitNpcChatUnit();
                    }

                    for (int i = acquaintedNpcs.Count; i < npcUnitList.Count; i++)
                    {
                        npcUnitList[i].SetActive(false);
                    }
                }

                // 设置是否在搜索状态
                public void SetIsSearching(bool value)
                {
                    isSearching = value;
                }

                // 复制ID到剪贴板
                public void CopyFriendIDToClipboard()
                {
                    GUIUtility.systemCopyBuffer = friendIDText.text;
                    Debug.Log("已复制ID到剪贴板: " + friendIDText.text);
                }

                // 更换服装并拍照
                public string ChangeClothesAndTakePhoto(Friend_Social_Record record)
                {
                    //InitCharacterRotate();
                    var suit = record._cloth_suit;
                    //character_Clothes_in_Level.WearSuit(suit);

                    string photoPath = System.IO.Path.Combine(Application.persistentDataPath, record.friend_id);
                    //Global_Photo_Manager.Instance.Save_RT_to_PNG(photoRT, null, record.friend_id);
                    Global_Photo_Manager.Instance.CapturePhoto(PhotoMode.Default, record.friend_id);

                    friendPhotoDict[record.friend_id] = photoPath;
                    return photoPath;
                }

                // 给玩家拍照
                public string ChangeClothesAndTakePhotoForPlayer(RenderTexture RT, string RTName)
                {
                    //InitCharacterRotate();

                    string fileName = Global_Game_Manager._instance._current_player_id.ToString() + "_" + RTName;
                    string photoPath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
                    //Global_Photo_Manager.Instance.Save_RT_to_PNG(RT, null, fileName);
                    //Global_Photo_Manager.Instance.CapturePhoto(PhotoMode.Default, fileName);

                    return photoPath;
                }

                // 游戏好友按钮
                public void FriendInGame()
                {
                    RefreshUI();

                    friendsItemView.SetActive(true);
                    friendsRequestItemView.SetActive(false);
                    npcItemView.SetActive(false);
                    searchedAcceptedFriend.gameObject.SetActive(false);
                    searchedUnacceptedFriend.gameObject.SetActive(false);
                    searchFriend.SetActive(true);
                }

                // 好友请求按钮
                public void FriendRequest()
                {
                    RefreshUI();

                    friendsItemView.SetActive(false);
                    friendsRequestItemView.SetActive(true);
                    npcItemView.SetActive(false);
                    searchedAcceptedFriend.gameObject.SetActive(false);
                    searchedUnacceptedFriend.gameObject.SetActive(false);
                    searchFriend.SetActive(true);
                }

                // NPC按钮
                public void Npc()
                {
                    RefreshUI();

                    friendsItemView.SetActive(false);
                    friendsRequestItemView.SetActive(false);
                    npcItemView.SetActive(true);
                    searchedAcceptedFriend.gameObject.SetActive(false);
                    searchedUnacceptedFriend.gameObject.SetActive(false);
                    searchFriend.SetActive(false);
                }

                // 打开好友界面
                public void OpenFriendCanvas()
                {
                    // 在打开聊天面板的时候初始化 NPC Favor Runtime Data
                    NPCChatPanelMgr.Instance.InitNPCFavorRuntimeDatas();
                    //CommonInteractManager.Instance.ExitPhone();
                    //StartCoroutine(OpenFriendCanvasCo());
                }

                //public IEnumerator OpenFriendCanvasCo()
                //{
                //    CommonInteractManager.Instance.ExitPhone();

                //    yield return new WaitForSeconds(1.5f);

                //    FriendInGame();
                //    friendChatPanel.SetActive(false);

                //    CommonInteractManager.Instance.ClosePanelWithoutUp();
                //    friendCanvas.SetActive(true);
                //}

                // 关闭好友界面
                public void CloseFriendCanvas()
                {
                    friendCanvas.SetActive(false);
                    //CommonInteractManager.Instance.OpenPanelWithoutUp();
                }

                public void OnPointerDown(PointerEventData eventData)
                {
                    if (IsPointerInRawImage(eventData.position))
                        lastPointerPos = eventData.position;
                }

                public void OnDrag(PointerEventData eventData)
                {
                    if (!IsPointerInRawImage(eventData.position)) return;

                    float deltaX = lastPointerPos.x - eventData.position.x;
                    lastPointerPos = eventData.position;

                    //if (character_Clothes_in_Level != null)
                    //{
                    //    var t = character_Clothes_in_Level.transform;
                    //    float newY = t.eulerAngles.y + deltaX * rotateSpeed;
                    //    t.eulerAngles = new Vector3(t.eulerAngles.x, newY, t.eulerAngles.z);
                    //}
                }

                public bool IsPointerInRawImage(Vector2 screenPos)
                {
                    if (rawImage == null) return false;
                    RectTransform rt = rawImage.rectTransform;
                    // 将屏幕坐标转换为UI坐标
                    Vector2 localPoint;
                    var canvas = rawImage.canvas;
                    if (canvas == null) canvas = rawImage.GetComponentInParent<Canvas>();
                    Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                    return RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, cam, out localPoint)
                        && rt.rect.Contains(localPoint);
                }

                //public void InitCharacterRotate()
                //{
                //    var t = character_Clothes_in_Level.transform;
                //    t.eulerAngles = new Vector3(t.eulerAngles.x, 180f, t.eulerAngles.z);
                //}

                private void OnDisable()
                {
                    if (Player_Social_Manager._instance != null)
                    {
                        Player_Social_Manager._instance._on_refresh_social_state.RemoveListener(RefreshUI);
                        Player_Social_Manager._instance._on_refresh_social_chat_msg.RemoveListener(friendsChatInteractManager.RefreshChatRecord);
                    }
                }
            }
        }
    }
}