//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using CLIP.Project_Mouse.Game_Play_System;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class GroupPhoto : MonoBehaviour
//            {
//                public static GroupPhoto Instance { get; private set; }

//                public GameObject panel;
//                public GameObject next;
//                public GameObject last;
//                public RawImage rawImage;
//                public Button save;
//                public TMP_Text canSave;
//                public string currentPhotoPath = "";
//                public Texture2D currentPhotoTexture;

//                public int currentPhotoIndex = 0;
//                public List<string> photoKeys = new List<string>();
//                public Dictionary<string, Texture2D> photos = new Dictionary<string, Texture2D>();

//                public HashSet<string> savedPhotoPathOrName = new HashSet<string>();

//                private void Awake()
//                {
//                    if (Instance != null && Instance != this)
//                    {
//                        Destroy(gameObject);
//                        return;
//                    }
//                    Instance = this;
//                    DontDestroyOnLoad(gameObject);
//                }

//                public void SaveGroupPhoto()
//                {
//                    if (!savedPhotoPathOrName.Contains(currentPhotoPath))
//                    {
//                    Global_Photo_Manager.Instance.AddDispatchPhoto(currentPhotoPath, Path.GetFileName(currentPhotoPath));
//                        savedPhotoPathOrName.Add(currentPhotoPath);
//                        canSave.text = "已保存";
//                        save.interactable = false;
//                    }
//                }

//                public void NextPhoto()
//                {
//                    if (photoKeys.Count == 0 || currentPhotoIndex >= photoKeys.Count - 1)
//                        return;

//                    currentPhotoIndex++;
//                    ShowPhotoByIndex(currentPhotoIndex);
//                }

//                public void LastPhoto()
//                {
//                    if (photoKeys.Count == 0 || currentPhotoIndex <= 0)
//                        return;

//                    currentPhotoIndex--;
//                    ShowPhotoByIndex(currentPhotoIndex);
//                }

//                public void ShowPhotoByIndex(int index)
//                {
//                    if (index < 0 || index >= photoKeys.Count)
//                        return;

//                    currentPhotoPath = photoKeys[index];
//                    currentPhotoTexture = photos[currentPhotoPath];
//                    rawImage.texture = currentPhotoTexture;

//                    UpdateSaveState();

//                    last.SetActive(index > 0);
//                    next.SetActive(index < photoKeys.Count - 1);
//                }

//                public void OpenGroupPhoto()
//                {
//                    if (photos.Count == 0)
//                    {
//                        return;
//                    }

//                    photoKeys = new List<string>(photos.Keys);

//                    panel.SetActive(true);

//                    currentPhotoIndex = 0;
//                    ShowPhotoByIndex(currentPhotoIndex);
//                }

//                public void UpdateSaveState()
//                {
//                    if (CanSaveCurrentPhoto())
//                    {
//                        canSave.text = "保存";
//                        save.interactable = true;
//                    }
//                    else
//                    {
//                        canSave.text = "已保存";
//                        save.interactable = false;
//                    }
//                }

//                public bool CanSaveCurrentPhoto()
//                {
//                    return !string.IsNullOrEmpty(currentPhotoPath) && !savedPhotoPathOrName.Contains(currentPhotoPath);
//                }

//                //public void SendGift()
//                //{
//                //    CommonInteractManager.Instance.ClosePanelWithoutUp();

//                //    var record = Player_Social_Manager.Instance._current_social_info._friend_accepted_info_record.Find(f => f.friend_name == Player_Social_Manager.Instance._next_visit_room_friend_name);
//                //    FriendsCanvasInteractManager.Instance.OpenChatPanel(record);
//                //    FriendsCanvasInteractManager.Instance.friendsChatInteractManager.SendPhoto(lastPhotoTexture);
//                //}
//            }
//        }
//    }
//}