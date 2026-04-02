//using System;
//using System.Collections;
//using System.Collections.Generic;
//using CLIP.Project_Mouse.Game_Play_System;
//using CLIP.Project_Mouse.Game_Play_System.Indoor_Room_System;
//using UnityEngine;
//using UnityEngine.Accessibility;
//using UnityEngine.EventSystems;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class CommonInteractManager : MonoBehaviour
//            {
//                public static CommonInteractManager Instance { get; private set; }

//                public IndoorMainCharacter main_Character_AI_Control;
//                public IndoorMainCharacter guest_Character_AI_Control;

//                [Header("其他UI")]
//                public List<Phone_Animator> phoneUIs = new List<Phone_Animator>();
//                public GameObject bTN_Main_Up;
//                public GameObject bTN_Main_Menu;
//                public GameObject panel_Main_Function;
//                public RectTransform phoneCanvasButtons;
//                public RectTransform phoneCanvasBg;
//                public Phone_Animator currentUIPhone;


//                [Header("场景名")]
//                public string roomSceneName = "MainScene";
//                public string dispatchSceneName = "Dispatch_Demo";

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

//                private void Start()
//                {
//                    //var roomManagers = FindObjectsOfType<Indoor_Room_Game_Manager>();
//                    //foreach (var room in roomUIPositions)
//                    //{
//                    //    foreach (var manager in roomManagers)
//                    //    {
//                    //        if (manager._level_info._room_name == room.RoomName)
//                    //        {
//                    //            room.roomGo = manager.gameObject;
//                    //            break;
//                    //        }
//                    //    }
//                    //}
//                    //phoneUIs = GameObject.FindObjectOfType<Phone_Animator>() != null ? new List<Phone_Animator>(FindObjectsOfType<Phone_Animator>(true)) : new List<Phone_Animator>();

//                    //var allCharacters = GameObject.FindObjectsOfType<IndoorMainCharacter>(true);
//                    //foreach (var ctrl in allCharacters)
//                    //{
//                    //    if (ctrl.gameObject.name == "Main_Character_20250820_ver_2")
//                    //        main_Character_AI_Control = ctrl;
//                    //    else if (ctrl.gameObject.name == "Guest_Character_20250820_ver_2")
//                    //        guest_Character_AI_Control = ctrl;
//                    //}
//                }

//                public void OnDestroy()
//                {
    
//                }

//                /// <summary>
//                /// 设置主按钮（手机）能否互动
//                /// </summary>
//                /// <param name="active"></param>
//                public void SetPhoneButtonActive(bool active)
//                {
//                    bTN_Main_Menu.gameObject.SetActive(active) ;
//                }

//                public void SetMainFunctionActive(bool active)
//                {
//                    panel_Main_Function.SetActive(active);
//                }


//                public void TakePhotoWithFriend()
//                {
//                    StartCoroutine(TakePhotoWithFriendCo());
//                }

//                public IEnumerator TakePhotoWithFriendCo()
//                {
//                    string name = $"#PhotoWith{Player_Social_Manager._instance._next_visit_room_friend_name}#" + Guid.NewGuid() + ".png";
//                    //Global_Photo_Manager.Instance.SaveCurrentScreenToPng("indoor_with_friend", name);
//                    Global_Photo_Manager.Instance.CapturePhoto(PhotoMode.Firend, name);

//                    yield return new WaitForEndOfFrame();

//                    Global_Photo_Manager.Instance.Try_Load_Last_Image(GroupPhoto.Instance.rawImage);
//                    GroupPhoto.Instance.currentPhotoPath = Global_Photo_Manager.Instance.Last_Photo_Path;

//                    yield return new WaitForSecondsRealtime(0.5f);

//                    GroupPhoto.Instance.panel.SetActive(true);
//                    GroupPhoto.Instance.canSave.text = "保存";
//                    GroupPhoto.Instance.save.interactable = true;
//                    GroupPhoto.Instance.next.SetActive(false);
//                    GroupPhoto.Instance.last.SetActive(false);
//                    GroupPhoto.Instance.currentPhotoTexture = GroupPhoto.Instance.rawImage.texture as Texture2D;
//                }

//                public void FindPhoneUis()
//                {
//                    phoneUIs = GameObject.FindObjectOfType<Phone_Animator>() != 
//                        null ? new List<Phone_Animator>(FindObjectsOfType<Phone_Animator>(true)) : new List<Phone_Animator>();
//                }

//                public void FindAndDressCharacter()
//                {
//                    StartCoroutine(FindAndDressCharacterCo());
//                }

//                public IEnumerator FindAndDressCharacterCo()
//                {

//                    yield return new WaitForSecondsRealtime(1f);

//                    var friendInfo = Player_Social_Manager._instance._current_social_info._friend_accepted_info_record.Find(f => f.friend_name == Player_Social_Manager._instance._next_visit_room_friend_name);
//                }

//                //public void InitOnLoadRoomScene()
//                //{
//                //    //var roomManagers = FindObjectsOfType<Indoor_Room_Game_Manager>();
//                //    //foreach (var room in roomUIPositions)
//                //    //{
//                //    //    foreach (var manager in roomManagers)
//                //    //    {
//                //    //        if (manager._level_info._room_name == room.RoomName)
//                //    //        {
//                //    //            room.roomGo = manager.gameObject;
//                //    //            break;
//                //    //        }
//                //    //    }
//                //    //}
//                //    //phoneUIs = GameObject.FindObjectOfType<Phone_Animator>() != null ? new List<Phone_Animator>(FindObjectsOfType<Phone_Animator>(true)) : new List<Phone_Animator>();
                    
//                //    //var allCharacters = GameObject.FindObjectsOfType<IndoorMainCharacter>(true);
//                //    //foreach (var ctrl in allCharacters)
//                //    //{
//                //    //    if (ctrl.gameObject.name == "Main_Character_20250820_ver_2")
//                //    //        main_Character_AI_Control = ctrl;
//                //    //    else if (ctrl.gameObject.name == "Guest_Character_20250820_ver_2")
//                //    //        guest_Character_AI_Control = ctrl;
//                //    //}

//                //}

//                public void ClosePanelWithoutUpAndMain()
//                {
//                    panel_Main_Function.SetActive(false);
//                }

//                public void OpenPanelWithoutUpAndMain()
//                {
//                    panel_Main_Function.SetActive(true);
//                }

//                public void ClosePanelWithoutUp()
//                {
//                    panel_Main_Function.SetActive(false);
//                    bTN_Main_Menu.SetActive(false);
//                    CloseUIPhone();
//                }

//                public void OpenPanelWithoutUp()
//                {
//                    panel_Main_Function.SetActive(true);
//                    bTN_Main_Menu.SetActive(true);
//                    OpenUIPhone();
//                }

//                // 加载房间场景
//                public void LoadRoomScene(bool fromPhone)
//                {
//                    if (fromPhone)
//                    {
//                        if (SceneManager.GetActiveScene().name != roomSceneName)
//                        {
//                            StartCoroutine(LoadRoomSceneCo());
//                        }
//                        else
//                        {
//                            OpenPanelWithoutUp();
//                            Indoor_Room_Game_Manager._activc_instance.uiPhone.GetComponent<Phone_Animator>().Close_Phone();

//                            if (Player_Social_Manager._instance != null && Player_Social_Manager._instance._on_visit_friend_room)
//                            {
//                                Player_Social_Manager._instance.clear_visit_room();
//                                StartCoroutine(LoadRoomSceneCo());
//                            }

//                            foreach (var canvas in Indoor_Room_Game_Manager._activc_instance._all_canvas)
//                            {
//                                canvas.gameObject.SetActive(true);
//                            }
//                        }
//                    }
//                    else
//                    {
//                        PM_RM.load_scene_async(roomSceneName, (loadedScene) =>
//                        {
//                            FindPhoneUis();
//                            //InitOnLoadRoomScene();
//                            OpenPanelWithoutUp();
//                        });
//                    }
//                }

//                public IEnumerator LoadRoomSceneCo()
//                {
//                    yield return new WaitForSeconds(0.1f);

//                    if (phoneUIs.Count > 0)
//                    {
//                        if (Indoor_Room_Game_Manager._activc_instance != null)
//                        {
//                            Indoor_Room_Game_Manager._activc_instance.uiPhone.GetComponent<Phone_Animator>().Close_Phone();
//                        }
//                        else
//                        {
//                            phoneUIs[0].Close_Phone();
//                        }
//                    }

//                    yield return null;

//                    PM_RM.load_scene_async(roomSceneName, (loadedScene) =>
//                    {
//                        FindPhoneUis();
//                        //InitOnLoadRoomScene();
//                        OpenPanelWithoutUp();
                     

//                        if (Player_Social_Manager._instance != null)
//                        {
//                            if (!Player_Social_Manager._instance._on_visit_friend_room)
//                            {
//                                guest_Character_AI_Control.gameObject.SetActive(false);
//                                EventTrigger eventTrigger = guest_Character_AI_Control.gameObject.GetComponent<EventTrigger>();
//                                foreach (var entry in eventTrigger.triggers)
//                                {
//                                    if (entry.eventID == EventTriggerType.PointerClick)
//                                    {
//                                        entry.callback.RemoveListener((data) => TakePhotoWithFriend());
//                                    }
//                                }
//                            }
//                        }
//                    });
//                }


//                // 加载派遣场景
//                public void LoadDispatchScene()
//                {
//                    StartCoroutine(LoadDispatchSceneCo());
//                }

//                public IEnumerator LoadDispatchSceneCo()
//                {
//                    if (SceneManager.GetActiveScene().name != dispatchSceneName)
//                    {
//                        //Indoor_Room_Game_Manager._activc_instance.uiPhone.GetComponent<Phone_Animator>().Close_Phone();

//                        yield return new WaitForSeconds(1.5f);

//                        PM_RM.load_scene_async( dispatchSceneName, (loadedScene) =>
//                        {
//                            phoneUIs = GameObject.FindObjectOfType<Phone_Animator>() != null ? new List<Phone_Animator>(FindObjectsOfType<Phone_Animator>(true)) : new List<Phone_Animator>();
//                            ClosePanelWithoutUpAndMain();
//                            bTN_Main_Menu.SetActive(true);
//                        });
//                    }
//                    else
//                    {
//                        ClosePanelWithoutUpAndMain();
//                        bTN_Main_Menu.SetActive(true);
//                        OpenUIPhone();
//                        if (phoneUIs.Count > 0)
//                        {
//                            phoneUIs[0].Close_Phone();
//                        }

//                    }
//                }

//                // 检查状态并打开手机
//                public void CheckStateAndOpenPhone()
//                {
//                    if (SceneManager.GetActiveScene().name == roomSceneName)
//                    {
//                        if (PlantingInteractManager.Instance.isPlantingState && !(PlantingInteractManager.Instance.curState is NonePlantState)) return;
//                        var indoorRoomManagers = FindObjectsOfType<IndoorRoomInteractManager>();
//                        if (indoorRoomManagers != null && indoorRoomManagers.Length > 0)
//                        {
//                            foreach (var manager in indoorRoomManagers)
//                            {
//                                if (manager.isEditing)
//                                {
//                                    return;
//                                }
//                            }
//                        }
//                    }
//                    OpenPhone();
//                }

//                // 打开手机
//                public void OpenPhone()
//                {
//                    StartCoroutine(OpenPhoneCo());
//                }

//                public IEnumerator OpenPhoneCo()
//                {
//                    panel_Main_Function.SetActive(false);
//                    bTN_Main_Menu.SetActive(false);

//                    if (SceneManager.GetActiveScene().name == roomSceneName)
//                    {
//                        foreach (var canvas in Indoor_Room_Game_Manager._activc_instance._all_canvas)
//                        {
//                            canvas.gameObject.SetActive(false);
//                        }
//                        //Indoor_Room_Game_Manager._activc_instance.controlCanvas.SetActive(false);
//                    }

//                    yield return null;

//                    if (SceneManager.GetActiveScene().name == dispatchSceneName)
//                    {
//                        if (phoneUIs.Count > 0)
//                        {
//                            foreach (var ui in phoneUIs)
//                            {
//                                if (ui.gameObject.activeSelf)
//                                {
//                                    ui.Open_Phone();
//                                    break;
//                                }
//                            }
//                        }
//                    }
//                    else if (SceneManager.GetActiveScene().name == roomSceneName)
//                    {
//                        //Indoor_Room_Game_Manager._activc_instance.uiPhone.GetComponent<Phone_Animator>().Open_Phone();
//                    }

//                    yield return new WaitForSeconds(1.5f);

//                    OpenPhoneCanvas();
//                }

//                public void OpenPhoneCanvas()
//                {
//                    currentUIPhone.phonePanel_RT = phoneCanvasBg;
//                    currentUIPhone.buttons = phoneCanvasButtons;
//                    currentUIPhone.MatchUI();
//                }

//                public static Transform FindDeepChild(Transform parent, string name)
//                {
//                    foreach (Transform child in parent)
//                    {
//                        if (child.name == name)
//                            return child;
//                        var result = FindDeepChild(child, name);
//                        if (result != null)
//                            return result;
//                    }
//                    return null;
//                }

//                // 退出手机
//                public void ExitPhone()
//                {
//                    if (SceneManager.GetActiveScene().name == dispatchSceneName)
//                    {
//                        if (phoneUIs.Count > 0)
//                        {
//                            foreach (var ui in phoneUIs)
//                            {
//                                if (ui.gameObject.activeSelf)
//                                {
//                                    ui.Close_Phone();
//                                    break;
//                                }
//                            }
//                        }
//                        ClosePanelWithoutUpAndMain();
//                        bTN_Main_Menu.SetActive(true);
//                        OpenUIPhone();
//                    }
//                    else if (SceneManager.GetActiveScene().name == roomSceneName)
//                    {
//                        Indoor_Room_Game_Manager._activc_instance.uiPhone.GetComponent<Phone_Animator>().Close_Phone();
//                        OpenPanelWithoutUp();
//                        foreach (var canvas in Indoor_Room_Game_Manager._activc_instance._all_canvas)
//                        {
//                            canvas.gameObject.SetActive(true);
//                        }
//                        Indoor_Room_Game_Manager._activc_instance.controlCanvas.SetActive(true);
//                    }
//                }

//                public void OpenUIPhone()
//                {
//                    if (SceneManager.GetActiveScene().name == dispatchSceneName)
//                    {
//                        foreach (var ui in phoneUIs)
//                        {
//                            ui.gameObject.SetActive(true);
//                        }
//                    }
//                    else if (SceneManager.GetActiveScene().name == roomSceneName)
//                    {
//                        if (Indoor_Room_Game_Manager._activc_instance != null)
//                        {
//                            Indoor_Room_Game_Manager._activc_instance.uiPhone.SetActive(true);
//                        }
                    
//                    }
//                }

//                public void CloseUIPhone()
//                {
//                    if (SceneManager.GetActiveScene().name == dispatchSceneName)
//                    {
//                        foreach (var ui in phoneUIs)
//                        {
//                            ui.gameObject.SetActive(false);
//                        }
//                    }
//                    else if (SceneManager.GetActiveScene().name == roomSceneName)
//                    {
//                        Indoor_Room_Game_Manager._activc_instance.uiPhone.SetActive(false);
//                    }
//                }
//            }

//        }
//    }
//}