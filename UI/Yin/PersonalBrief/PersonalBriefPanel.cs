using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.NewFrame.UI
{
    public class PersonalBriefPanel : UIPanelBase
    {
        [Header("UI")]
        public GameObject obj;
        public GameObject changeIcon;
        public GameObject changeName;
        public Image expImage;
        public TMP_Text expLevelText;
        public TMP_Text expText;
        public Slider expSlider;
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
        //public string friendListIconPath;

        [Header("FriendDetail")]
        public GameObject friendDetailPanel;
        public TMP_Text friendDetailNameText;
        public TMP_Text friendDetailIDText;
        public TMP_Text friendDetailIntimacyText;
        public RawImage friendDetailRawImage;
        //public string friendDetailIconPath;
        public List<RawImage> friendDetailImageList = new List<RawImage>();

        [Header("PlayerDetail")]
        private string filterString = "";
        public float debounceTime = 0.25f;
        public bool canchange = false;
        Coroutine currentCoroutine;

        public override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            //friendListIconPath = FriendsCanvasInteractManager.Instance.ChangeClothesAndTakePhotoForPlayer(FriendsCanvasInteractManager.Instance.photoRT, "FriendList");
            //friendDetailIconPath = FriendsCanvasInteractManager.Instance.ChangeClothesAndTakePhotoForPlayer(FriendsCanvasInteractManager.Instance.bigPhotoRT, "FriendList");
            changeNameInputField.onSubmit.AddListener(value =>
            {
                canchange = false;
                filterString = value;

                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(DebouncedValidate(filterString));

            });
            changeNameInputField.onValueChanged.AddListener(value =>
            {
                canchange = false;
                filterString = value;

                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(DebouncedValidate(filterString));

            });
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InitPersonalBrief();
        }


        #region 接口方法实现

        public override void OpenPanel(params object[] data)
        {
            canchange = false;

            obj.SetActive(true);
            InitPersonalBrief();
            MainPanel.CloseMainFuncP();
            MainPanel.SetPhoneBTNEnable(false);
        }


        IEnumerator DebouncedValidate(string text)
        {
            yield return new WaitForSeconds(debounceTime);
            // 执行实际校验逻辑
            canchange = true;
            filterString = SensitiveWordManager.Instance.FilterText(filterString);
            FilterSpecialCharacters(filterString);
            changeNameInputField.text = filterString;
        }


        public override void ClosePanel()
        {
            obj.SetActive(false);
            MainPanel.OpenMainFuncP();
            MainPanel.SetPhoneBTNEnable(true);
            Global_Game_Manager.Instance._player_brief._brief_photo_selected = photoInfoList;
            Global_Game_Manager.Instance._player_brief._icon_info = currentIcon;
            Global_Game_Manager.Instance._player_brief._icon_frame_info = currentIconFrame;
            Global_Game_Manager.Instance._player_brief.achievements_pinned = achievementRecordList;
            Global_Game_Manager.Instance.on_upload_player_brief_to_server();
            // 上传相册照片到服务器
        }

        #endregion


        #region 内部方法

        // 初始化个人简介
        private void InitPersonalBrief()
        {
            CloseChangeName();
            // 经验值和等级
            nameText.text = Global_Game_Manager.Instance._player_brief._player_nick_name;
            idText.text = Global_Game_Manager.Instance._current_player_id;
            expLevelText.text = ExpManager.instance.curLevel.ToString();
            expSlider.value = 0;
            if (ExpManager.instance.curLevelInfo == null || ExpManager.instance.curLevelInfo.nextLevelExp == 0)
            {
                expSlider.value = 0;
            }
            else
            {
                expSlider.value =  ExpManager.instance.curExp * 1f / ExpManager.instance.curLevelInfo.nextLevelExp;
            }

            expText.text = $"{ExpManager.instance.curExp}/{ExpManager.instance.curLevelInfo.nextLevelExp}";

            currentIcon = Global_Game_Manager.Instance._player_brief._icon_info;
            currentIconFrame = Global_Game_Manager.Instance._player_brief._icon_frame_info;
            // 加载头像
            // 加载头像框
            InitAchievements();
            InitPhotos(imageList);
        }

        // 初始化成就
        private void InitAchievements()
        {
            achievementRecordList = Global_Game_Manager.Instance._player_brief.achievements_pinned;
            StartCoroutine(LoadAchievementIconFromAchievementList());
        }

        // 初始化照片
        private void InitPhotos(List<RawImage> imageList)
        {
            photoInfoList = Global_Game_Manager.Instance._player_brief._brief_photo_selected;
            StartCoroutine(LoadImagesFromPhotoInfoList(imageList));
        }

        #endregion




        // 打开好友详情视角
        public void OpenFriendDetail()
        {
            //Global_Photo_Manager.Instance.try_load_image(friendListIconPath, friendDetailRawImage);
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
            //Global_Photo_Manager.Instance.try_load_image(friendListIconPath, friendListRawImage);
            // 成就
            otherBriefPanel.SetActive(false);
        }

        // 打开修改头像界面
        public void OpenChangeIcon()
        {
            changeIcon.SetActive(true);
        }

        // 打开修改名称界面
        public void OpenChangeName()
        {
            changeName.SetActive(true);
            changeNameInputField.text = "";
        }
        // 打开修改名称界面
        public void CloseChangeName()
        {
            changeName.SetActive(false);
            changeNameInputField.text = "";
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

        // 删除输入内容
        public void DeleteInput()
        {
            changeNameInputField.text = "";
            deleteInput.SetActive(false);
        }

        // 确认修改名称
        public void ConfirmChangeName()
        {
            if (!canchange)
            {
                return;
            }

            if (!string.IsNullOrEmpty(changeNameInputField.text))
            {
                if (changeNameInputField.text.Contains("*"))
                {
                    return;
                }

                Global_Game_Manager.Instance.on_upload_player_brief_to_server();

                Global_Game_Manager.Instance._player_brief._player_nick_name = changeNameInputField.text;
                nameText.text = changeNameInputField.text;
                changeName.SetActive(false);
            }
        }


        /// <summary>
        /// 实时过滤特殊字符（输入时立即生效）
        /// </summary>
        /// <param name="inputStr">输入的原始字符串</param>
        private void FilterSpecialCharacters(string inputStr)
        {
            // 1. 限制长度
            if (inputStr.Length > 14)
            {
                inputStr = inputStr.Substring(0, 14);
            }

            // 2. 字符白名单：仅保留 中文、英文、数字、下划线
            char[] validChars = inputStr.ToCharArray();
            int validIndex = 0;

            for (int i = 0; i < validChars.Length; i++)
            {
                char c = validChars[i];
                // 判断是否为合法字符：
                // a-z/A-Z（英文）、0-9（数字）、_（下划线）、中文（Unicode范围）
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_'
                    || (c >= 0x4E00 && c <= 0x9FFF)) // 中文Unicode范围（常用）
                {
                    validChars[validIndex] = c;
                    validIndex++;
                }
            }

            // 3. 重构合法字符串
            string filteredStr = new string(validChars, 0, validIndex);

            // 4. 避免死循环：仅当内容变化时更新输入框
            if (filteredStr != changeNameInputField.text)
            {
                changeNameInputField.text = filteredStr;
                // 保持光标位置（优化体验）
                changeNameInputField.caretPosition = filteredStr.Length;
            }
        }

        /// <summary>
        /// 最终校验（输入完成后）
        /// </summary>
        /// <param name="finalStr">最终输入的名称</param>
        private void ValidateFinalName(string finalStr)
        {
            // 1. 检查是否为空
            if (string.IsNullOrEmpty(finalStr) || finalStr.Trim() == "")
            {
                Debug.LogError("名称不能为空！");
                // 可选：重置输入框提示
                //changeNameInputField.placeholder.GetComponent<Text>().text = "请输入合法名称（不能为空）";
                return;
            }

            // 2. 二次校验（防止漏过滤）
            FilterSpecialCharacters(finalStr);

            //Debug.Log($"名称校验通过：{nameInput.text}");
        }

        // 对外提供获取合法名称的方法
        public string GetValidName()
        {
            return changeNameInputField.text.Trim();
        }


        // 复制ID到剪贴板
        public void CopyIDToClipboard()
        {
            GUIUtility.systemCopyBuffer = idText.text;
            Debug.Log("已复制ID到剪贴板: " + idText.text);
        }

        // 协程：从相片列表中加载图片
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

        // 协程：从成就列表中加载成就图标
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



    }
}
