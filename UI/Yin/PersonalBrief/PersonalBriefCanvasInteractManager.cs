using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            // 不确定是否有用，场景中并没有挂载了该脚本的物体，但是调用了 FriendsCanvasInteractManager 的引用
            public class PersonalBriefCanvasInteractManager :SingletonMono <PersonalBriefCanvasInteractManager>
            {
                [Header("UI")]
                public GameObject personalbriefCanvas;
                public GameObject changeIcon;
                public GameObject changeName;
                public GameObject photoAlbum;
                public Image expImage;
                public TMP_Text expLevelText;
                public TMP_Text expText;
                public TMP_Text nameText;
                public TMP_Text idText;
                public Image icon;
                public Image iconFrame;
                public TMP_InputField changeNameInputField;
                public GameObject deleteInput;
                public GameObject otherBriefPanel;

                [Header("ChangeAchievements")]
                public List<Image> achievementIconList = new List<Image>();
                public List<achievement_record> achievementRecordList = new List<achievement_record>();
                public Achievement_Design_Info_DB_SO achievementSO;

                [Header("ChangePhoto")]
                public RawImage imageToChange;
                public int imageToChangeIndex;
                public List<RawImage> imageList = new List<RawImage>();
                public List<photo_info_saved> photoInfoList = new List<photo_info_saved>();
                public avatar_icon_info currentIcon;
                public avatar_icon_info currentIconFrame;

                [Header("FriendList")]
                public GameObject friendListPanel;
                public TMP_Text friendListNameText;
                public RawImage friendListRawImage;
                public string friendListIconPath;

                [Header("FriendDetail")]
                public GameObject friendDetailPanel;
                public TMP_Text friendDetailNameText;
                public TMP_Text friendDetailIDText;
                public TMP_Text friendDetailIntimacyText;
                public RawImage friendDetailRawImage;
                public string friendDetailIconPath;
                public List<RawImage> friendDetailImageList = new List<RawImage>();

                private void Awake()
                {
                    DontDestroyOnLoad(this.gameObject);
                }

                private void Start()
                {
                    //friendListIconPath = FriendsCanvasInteractManager.Instance.ChangeClothesAndTakePhotoForPlayer(FriendsCanvasInteractManager.Instance.photoRT, "FriendList");
                    //friendDetailIconPath = FriendsCanvasInteractManager.Instance.ChangeClothesAndTakePhotoForPlayer(FriendsCanvasInteractManager.Instance.bigPhotoRT, "FriendList");
                }

                private void OnEnable()
                {
                    InitPersonalBrief();
                }

                // 打开好友详情视角
                public void OpenFriendDetail()
                {
                    Global_Photo_Manager.Instance.try_load_image(friendListIconPath, friendDetailRawImage);
                    friendDetailNameText.text = nameText.text;
                    friendDetailIDText.text = idText.text;
                    friendDetailIntimacyText.text = expLevelText.text;
                    StartCoroutine(LoadImagesFromPhotoInfoList(friendDetailImageList));
                    // 成就
                    friendDetailPanel.SetActive(true);
                    otherBriefPanel.SetActive(false);
                }

                // 打开好友列表视角
                public void OpenFriendListPanel()
                {
                    friendListPanel.SetActive(true);
                    friendListNameText.text = nameText.text;
                    Global_Photo_Manager.Instance.try_load_image(friendListIconPath, friendListRawImage);
                    // 成就
                    otherBriefPanel.SetActive(false);
                }

                // 选择其他视角
                public void ToggleOtherBriefPanel()
                {
                    if (otherBriefPanel.activeSelf)
                    {
                        otherBriefPanel.SetActive(false);
                    }
                    else
                    {
                        otherBriefPanel.SetActive(true);
                    }
                }

                // 初始化成就
                public void InitAchievements()
                {
                    achievementRecordList = Global_Game_Manager.Instance._player_brief.achievements_pinned;
                    StartCoroutine(LoadAchievementIconFromAchievementList());
                }

                public IEnumerator LoadAchievementIconFromAchievementList()
                {
                    for (int i = 0; i < achievementIconList.Count; i++)
                    {
                        if (i < achievementRecordList.Count)
                        {
                            var info = achievementRecordList[i];
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
                            string path = achievement.achievement_icon_res_url;
                            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                            {
                                // 加载icon
                                yield return null;
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

                    while (achievementRecordList.Count < 4)
                    {
                        achievementRecordList.Add(null);
                    }
                }

                // 初始化照片
                public void InitPhotos(List<RawImage> imageList)
                {
                    photoInfoList = Global_Game_Manager.Instance._player_brief._brief_photo_selected;
                    StartCoroutine(LoadImagesFromPhotoInfoList(imageList));
                }

                public IEnumerator LoadImagesFromPhotoInfoList(List<RawImage> imageList)
                {
                    for (int i = 0; i < imageList.Count; i++)
                    {
                        if (i < photoInfoList.Count)
                        {
                            var info = photoInfoList[i];
                            if (info == null)
                            {
                                imageList[i].texture = null;
                                continue;
                            }
                            string path = info._local_path;
                            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                            {
                                bool isLoaded = false;
                                Project_Mouse_Resource_Management.load_png_as_texture(path, (texture) =>
                                {
                                    imageList[i].texture = texture;
                                    isLoaded = true;
                                });
                                yield return new WaitUntil(() => isLoaded);
                            }
                            else
                            {
                                imageList[i].texture = null;
                            }
                        }
                        else
                        {
                            imageList[i].texture = null;
                        }
                    }

                    while (photoInfoList.Count < 4)
                    {
                        photoInfoList.Add(null);
                    }
                }

                // 打开修改头像界面
                public void OpenChangeIcon()
                {
                    changeIcon.SetActive(true);
                }

                // 删除输入内容
                public void DeleteInput()
                {
                    changeNameInputField.text = "";
                    deleteInput.SetActive(false);
                }

                // 确认修改名称
                public void ConfirmChangeName()
                {
                    if (!string.IsNullOrEmpty(changeNameInputField.text))
                    {
                        Global_Game_Manager.Instance._player_brief._player_nick_name = changeNameInputField.text;
                        nameText.text = changeNameInputField.text;
                    }
                    changeName.SetActive(false);
                }

                // 打开修改名称界面
                public void OpenChangeName()
                {
                    changeName.SetActive(true);
                    changeNameInputField.text = "";
                }

                // 初始化个人简介
                public void InitPersonalBrief()
                {
                    // 经验值和等级
                    nameText.text = Global_Game_Manager.Instance._player_brief._player_nick_name;
                    idText.text = Global_Game_Manager.Instance._current_player_id;
                    expLevelText.text = Global_Game_Manager.Instance._player_brief._affinity_with_main_character.ToString();
                    currentIcon = Global_Game_Manager.Instance._player_brief._icon_info;
                    currentIconFrame = Global_Game_Manager.Instance._player_brief._icon_frame_info;
                    // 加载头像
                    // 加载头像框
                    InitAchievements();
                    InitPhotos(imageList);
                }

                // 复制ID到剪贴板
                public void CopyIDToClipboard()
                {
                    GUIUtility.systemCopyBuffer = idText.text;
                    Debug.Log("已复制ID到剪贴板: " + idText.text);
                }

                // 关闭个人简介
                public void ClosePersonalBrief()
                {
                    personalbriefCanvas.SetActive(false);
                    //CommonInteractManager.Instance.OpenPanelWithoutUp();
                    Global_Game_Manager.Instance._player_brief._brief_photo_selected = photoInfoList;
                    Global_Game_Manager.Instance._player_brief._icon_info = currentIcon;
                    Global_Game_Manager.Instance._player_brief._icon_frame_info = currentIconFrame;
                    Global_Game_Manager.Instance._player_brief.achievements_pinned = achievementRecordList;
                    Global_Game_Manager.Instance.on_upload_player_brief_to_server();
                    // 上传相册照片到服务器
                }

                // 打开个人简介
                public void OpenPersonalBrief()
                {
                    InitPersonalBrief();

                    personalbriefCanvas.SetActive(true);

                    //StartCoroutine(OpenPersonalBriefCo());
                }

                //public IEnumerator OpenPersonalBriefCo()
                //{
                //    CommonInteractManager.Instance.ExitPhone();

                //    yield return new WaitForSeconds(1.5f);

                    
                //    CommonInteractManager.Instance.ClosePanelWithoutUp();
                //}
            }
        }
    }
}