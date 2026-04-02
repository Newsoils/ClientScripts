//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
//using UnityEngine;
//using UnityEngine.UI;
//using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
//using Newtonsoft.Json;
//using TMPro;
//using CLIP.Project_Mouse.Game_Play_System;
//using CLIP.Project_Mouse.Kernel.Dispatch;
//using CLIP.Framework_Unity;
//using GameCoreResourceLoad;
//using CLIP.Framework_Core.Tools;
//using CLIP.Framework_Unity.Asset;
//using CLIP.Framework_Core.Event;
//using CLIP.Framework_Core.LYC.TaskSystem;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class DispatchPhotoInteractManager : MonoBehaviour
//            {
//                public static DispatchPhotoInteractManager Instance { get; private set; }

//                [Header("SO")]
//                public Dispatch_DB_SO dispatch_Configuration;

//                [Header("相册UI")]
//                public GameObject dispatchPhotoCanvas;
//                public GameObject panel;
//                public RawImage image1;
//                public RawImage image2;
//                public GameObject lastPage;
//                public GameObject nextPage;

//                [Header("其他Ui")]
//                public GameObject afterPhoto;
//                public Button bTN_Show_Photo;
//                public GameObject closeAfterPhoto;

//                [Header("奖励UI")]
//                public TMP_Text money;
//                public TMP_Text exp;
//                public TMP_Text photo;
//                public GameObject rewardImage;

//                [Range(0, 1)] public float alphaValue = 0.5f;

//                public List<Texture> textures = new List<Texture>();
//                public int currentPage = 0;
//                public int selectedPhotoIndex = -1;

//                public string lastPhotoPath;

//                // ===== 新加的 ===== 
//                public RawImage showPhoto;
//                public GameObject showPhotoPanel;

//                private void Awake()
//                {
//                    if (Instance != null && Instance != this)
//                    {
//                        Destroy(gameObject);
//                        return;
//                    }
//                    Instance = this;

//                }
//                private void Start()
//                {
//                    LoadDispatchPhotoPathListFromDisk();
//                    //Global_Photo_Manager.Instance.dispatchPhotoRawImage = afterPhoto.GetComponent<RawImage>();
//                    EvtDsp.AddEvt(EvtNames.Dispatch_On_End, StartCapturePhoto);

//                }

//                private void OnDestroy()
//                {
//                    EvtDsp.RemoveEvt(EvtNames.Dispatch_On_End,StartCapturePhoto);
//                }

//                // 返回到派遣主界面
//                public void BackToMainScene()
//                {
//                    MainPanel.OpenTopP();
//                    MainPanel.OpenMainFuncP();
//                    MainPanel.SetPhoneBTNEnable(true);

//                    afterPhoto.SetActive(false);
//                    SceneLoadHelper.Load_MainScene();
//                }

//                // 关闭相册界面
//                public void CloseDispatchPhotoCanvas()
//                {
//                    MainPanel.SetPhoneBTNEnable(true);
//                    dispatchPhotoCanvas.SetActive(false);

//                    // 恢复上方状态栏显示
//                    UIManager.Instance.GetPanel<MainPanel>().ShowAll();
//                }

//                // 打开相册界面
//                public void OpenDispatchPhotoCanvas()
//                {
//                    // 通知任务系统
//                    //TaskTriggers.TriggerEventOfMultipleOperations("第一次打开相册", 1);
//                    TaskTriggers.TriggerEventOfMultipleOperations(4, 1);

//                    MainPanel.SetPhoneBTNEnable(false);
//                    StartCoroutine(LoadTexturesAndUpdatePage());
//                }

//                public IEnumerator LoadTexturesAndUpdatePage()
//                {
//                    textures.Clear();
//                    foreach (string photoPath in Dispatch_Manager._instance._dispatch_photo_path_list)
//                    {
//                        if (File.Exists(photoPath))
//                        {
//                            bool isLoaded = false;
//                            PM_RM.load_png_as_texture(photoPath, (texture) =>
//                            {
//                                if (texture != null)
//                                {
//                                    textures.Add(texture);
//                                    Debug.Log($"Loaded texture from path: {photoPath}");
//                                }
//                                else
//                                {
//                                    Debug.LogError($"Failed to load texture from path: {photoPath}");
//                                }
//                                isLoaded = true;
//                            });
//                            yield return new WaitUntil(() => isLoaded);
//                        }
//                        else
//                        {
//                            Debug.LogError($"File does not exist at path: {photoPath}");
//                        }
//                    }

//                    Debug.Log($"Total textures loaded: {textures.Count}");
//                    UpdatePage();
//                    dispatchPhotoCanvas.SetActive(true);

//                    // 同时保证上方状态栏显示
//                    UIManager.Instance.GetPanel<MainPanel>().ShowTopPanelOnly();
//                }

//                // 显示选中的照片
//                public void ShowAfterPhoto(Button button)
//                {
//                    if (button == null || button.GetComponent<RawImage>() == null || button.GetComponent<RawImage>().texture == null)
//                    {
//                        return;
//                    }
//                    SetPanelTransparency(alphaValue);
//                    showPhoto.texture = button.GetComponent<RawImage>().texture;
//                    showPhotoPanel.SetActive(true);

//                    selectedPhotoIndex = textures.IndexOf(button.GetComponent<RawImage>().texture);
//                }

//                // 删除选中的照片
//                public void DeleteSelecatedPhoto()
//                {
//                    if (selectedPhotoIndex < 0 || selectedPhotoIndex >= textures.Count)
//                    {
//                        Debug.LogError("No photo selected or invalid index.");
//                        return;
//                    }

//                    string photoPath = Dispatch_Manager._instance._dispatch_photo_path_list[selectedPhotoIndex];
//                    if (File.Exists(photoPath))
//                    {
//                        File.Delete(photoPath);
//                        Debug.Log($"Deleted photo at path: {photoPath}");
//                    }

//                    Dispatch_Manager._instance._dispatch_photo_path_list.RemoveAt(selectedPhotoIndex);
//                    textures.RemoveAt(selectedPhotoIndex);

//                    SaveDispatchPhotoPathListToDisk();

//                    selectedPhotoIndex = -1;
//                    showPhotoPanel.SetActive(false);

//                    UpdatePage();
//                }

//                // 关闭查看照片界面
//                public void HideAfterPhoto()
//                {
//                    SetPanelTransparency(0);
//                    showPhotoPanel.SetActive(false);
//                    selectedPhotoIndex = -1;
//                }

//                // 上一页
//                public void LastPage()
//                {
//                    if (currentPage > 0)
//                    {
//                        currentPage--;
//                        UpdatePage();
//                    }
//                }

//                // 下一页
//                public void NextPage()
//                {
//                    if ((currentPage + 1) * 2 < textures.Count)
//                    {
//                        currentPage++;
//                        UpdatePage();
//                    }
//                }

//                // 更新页面显示
//                private void UpdatePage()
//                {
//                    int startIndex = currentPage * 2;

//                    image1.texture = startIndex < textures.Count ? textures[startIndex] : null;
//                    image2.texture = (startIndex + 1) < textures.Count ? textures[startIndex + 1] : null;

//                    lastPage.SetActive(currentPage > 0);
//                    nextPage.SetActive((currentPage + 1) * 2 < textures.Count);
//                }

//                // 设置面板透明度
//                public void SetPanelTransparency(float alphaValue)
//                {
//                    Image image = panel.GetComponent<Image>();
//                    if (image != null)
//                    {
//                        var color = image.color;
//                        color.a = alphaValue;
//                        image.color = color;
//                    }
//                    else
//                    {
//                        Debug.LogError("PhotoPanel does not have an Image component.");
//                    }
//                }

//                public void ForceReturn()
//                {
//                    Dispatch_Manager._instance.force_dispatch_end();
//                    StartCapturePhoto();
//                }

//                public void StartCapturePhoto()
//                {
//                    StartCoroutine(CaptureAndShowPhotoCo());
//                }


//                private IEnumerator CaptureAndShowPhotoCo()
//                {
//                    string photoName = "";
//                    bTN_Show_Photo.onClick.RemoveAllListeners();
//                    bTN_Show_Photo.onClick.AddListener(() => CapturePhoto(Dispatch_Manager._instance._player_dispatch_state.last_reward_photo_name));

//                    photoName = Dispatch_Manager._instance._player_dispatch_state.last_reward_photo_name;

//                    yield return null;

//                    CapturePhoto(photoName);
//                }


//                // 拍照功能
//                public void CapturePhoto(string photoName)
//                {
//                    var _dispatch_config = Dispatch_Manager._instance._dispatch_configuration_so._dispatch_config;

//                    var photoInfo = _dispatch_config.photo_info_list.Find(p => p.photo_name == photoName);
//                    if (photoInfo == null)
//                    {
//                        Debug.Log($"Invalid photoInfoIndex or photo_info is null. PhotoName: {photoName}");
//                        return;
//                    }
//                    var mapIndex = _dispatch_config.map_info_list.Find(p=>p.photo_Ids.Contains(photoInfo.photo_id)).map_id;

//                    //检查一下数组下标
//                    if (_dispatch_config.map_info_list.Count < mapIndex - 1)
//                    {
//                        Log.Error("Dispatch Take Photo Error:" + photoName + "超出数组下标:" + (mapIndex - 1) + "总共只有"+ _dispatch_config.map_info_list.Count +"个地图");
//                        return;
//                    }

//                    var sceneName = _dispatch_config.map_info_list[mapIndex-1].map_Scene_Name;


//                    PM_RM.load_scene_async(sceneName, (loadedScene) =>
//                    {
//                        var loadedCharacter = GameAssets.Instance.mainCharacter_Dispatch; 
//                            StartCoroutine(CapturePhotoCo(photoInfo, loadedCharacter));
//                    });
//                }

//                private IEnumerator CapturePhotoCo(Photo_Info photoInfo, GameObject loadedCharacter)
//                {

//                    GameObject rain_Image = GameObject.Find("Rain_Image");
//                    if (rain_Image != null)
//                    {
//                        if (Global_Game_Manager._instance._weather_state._current_weather == "Rain")
//                        {
//                            rain_Image.SetActive(true);
//                        }
//                        else
//                        {
//                            rain_Image.SetActive(false);
//                        }
//                    }
//                    else
//                    {
//                        Debug.LogWarning("Rain_Image not found in the scene.");
//                    }


//                    var photoRoot = GameObject.Find("PhotoRoot")?.transform;

//                    if(photoRoot == null)
//                    {
//                        Log.Error("Photo_Root not found in the scene.");
//                        yield return null;
//                    }

//                    var photo_Camera_ID = photoInfo.camera_id;

//                    var groupCount = photoRoot.childCount;
                   
//                    if(photo_Camera_ID<0|| photo_Camera_ID > photoRoot.childCount)
//                    {
//                        Log.Error("photo_Camera_ID is out of range.");
//                        yield return null;
//                    }
               
//                    Transform currentCameraT = null;

//                    for (int i = 0; i < groupCount; i++)
//                    {
//                        var cameraGroup = photoRoot.GetChild(i);
//                        if(i!=photo_Camera_ID)
//                            cameraGroup.gameObject.SetActive(false);
//                        else
//                        {
//                            currentCameraT = cameraGroup;
//                            cameraGroup.gameObject.SetActive(true);
//                        }
//                    }

//                    var camera = currentCameraT.GetComponentInChildren<Camera>();

//                    var characterPoint = currentCameraT.GetComponentInChildren<Model_Placeholder>();

//                    var characterOB = Instantiate(loadedCharacter, characterPoint.transform.position, characterPoint.transform.rotation);
//                    Unity_Tools.IdentityGameObject(characterOB);
//                    var animator = characterOB.GetComponentInChildren<Animator>();
//                    var aniName = photoInfo.main_character_pose_name;
//                    if (animator != null && string.IsNullOrEmpty(aniName))
//                    {
//                        bool hasAnim = false;

//                        foreach (var clip in animator.runtimeAnimatorController.animationClips)
//                        {
//                            if (clip.name == aniName)
//                            {
//                                hasAnim = true;
//                                break;
//                            }
//                        }
//                        if(hasAnim)
//                        animator.Play(aniName);
//                    }

//                    yield return new WaitForSeconds(1f);


//                    camera.targetTexture = Global_Photo_Manager._instance.dispatchPhotoRT;
//                    camera.gameObject.SetActive(true);


//                    Canvas weatherCanvas = GameObject.Find("WeatherCanvas")?.GetComponent<Canvas>();
//                    if (weatherCanvas != null)
//                    {
//                        weatherCanvas.worldCamera = camera;
//                    }
//                    else
//                    {
//                        Debug.LogWarning("Canvas not found in the scene.");
//                    }


//                    yield return null;

//                    string fileName = $"{photoInfo.photo_name}_{photoInfo.camera_id}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

//                    Global_Photo_Manager._instance.Save_RT_to_PNG("dispatch", fileName);

//                    yield return new WaitForEndOfFrame();

//                    string photoPath = Path.Combine(Application.persistentDataPath, $"{fileName}");
//                    Dispatch_Manager._instance._dispatch_photo_path_list.Add(photoPath);

//                    yield return null;

//                    BackToMainScene();
                    
//                    SaveDispatchPhotoPathListToDisk();
//                    Debug.Log($"Photo saved to: {photoPath}");
//                    lastPhotoPath = photoPath;
  
//                }

//                public void ShowLastPhoto()
//                {
//                    Global_Photo_Manager._instance.try_load_image(lastPhotoPath, afterPhoto.GetComponent<RawImage>());
//                    afterPhoto.SetActive(true);
//                }

//                public void ShowLastPhoto(RawImage rawImage)
//                {
//                    Global_Photo_Manager._instance.try_load_image(lastPhotoPath, rawImage);
//                }

//                // 序列化照片路径列表
//                public void SaveDispatchPhotoPathListToDisk()
//                {
//                    string filePath = Path.Combine(Application.persistentDataPath, "dispatch_photo_path_list.json");

//                    string json = JsonConvert.SerializeObject(Dispatch_Manager._instance._dispatch_photo_path_list);
//                    File.WriteAllText(filePath, json);
//                    Debug.Log($"Dispatch photo path list saved to: {filePath}");
//                }

//                // 反序列化照片路径列表
//                public void LoadDispatchPhotoPathListFromDisk()
//                {
//                    string filePath = Path.Combine(Application.persistentDataPath, "dispatch_photo_path_list.json");

//                    if (File.Exists(filePath))
//                    {
//                        string json = File.ReadAllText(filePath);
//                        try {
//                            Dispatch_Manager._instance._dispatch_photo_path_list = JsonConvert.DeserializeObject<List<string>>(json);
//                            Debug.Log($"Dispatch photo path list loaded from: {filePath}");
//                        }
//                        catch
//                        {
//                            Debug.Log($"Dispatch photo path list json parser error");
//                        }
                        
//                    }
//                    else
//                    {
//                        Debug.LogWarning($"File not found at path: {filePath}");
//                    }
//                }
//            }

//        }
//    }
//}
