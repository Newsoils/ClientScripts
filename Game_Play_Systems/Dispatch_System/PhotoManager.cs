using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Dispatch;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;

public class PhotoManager : SingletonMono<PhotoManager>
{
    public RenderTexture dispatchPhotoTexture;
    public Dictionary<PhotoData, Texture2D> photos = new Dictionary<PhotoData, Texture2D>();
    public List<PhotoData> localData = new List<PhotoData>();
    private List<Photo_Info> photoInfos;
    [SerializeField] private string photoPath;
    private void Start()
    {
        string info = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_photo_info");
        photoInfos = JsonConvert.DeserializeObject<List<Photo_Info>>(info);
        DontDestroyOnLoad(gameObject);
        photoPath = Path.Combine(Application.persistentDataPath, "photo", NetWork_Center_WSS.instance._player_name);
        _ = LoadPhoto();
    }
    private void Update()
    {
        if(Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            StartCoroutine(CapturePhotoCoroutine("奶茶店_普通1"));
        }
    }
    public IEnumerator CapturePhotoCoroutine(string photoName)
    {
        var info = photoInfos.Find(x => x.photo_name == photoName);
        yield return SceneManager.LoadSceneAsync(info.sceneName);

        var photo_Camera_ID = info.camera_id;
        var photoRoot = GameObject.Find("PhotoRoot")?.transform;

        if (photoRoot == null)
        {
            Log.Error("Photo_Root not found in the scene.");
            yield return null;
        }

        if (photo_Camera_ID < 0 || photo_Camera_ID > photoRoot.childCount)
        {
            Log.Error("photo_Camera_ID is out of range.");
            yield return null;
        }

        Transform currentCameraT = null;

        for (int i = 0; i < photoRoot.childCount; i++)
        {
            var cameraGroup = photoRoot.GetChild(i);
            if (i != photo_Camera_ID)
                cameraGroup.gameObject.SetActive(false);
            else
            {
                currentCameraT = cameraGroup;
                cameraGroup.gameObject.SetActive(true);
            }
        }

        var camera = currentCameraT.GetComponentInChildren<Camera>();

        var characterPoint = currentCameraT.GetComponentInChildren<Model_Placeholder>();

        var characterOB = Instantiate(GameAssets.Instance.mainCharacter_Dispatch, characterPoint.transform.position, characterPoint.transform.rotation);
        Unity_Tools.IdentityGameObject(characterOB);
        var animator = characterOB.GetComponentInChildren<Animator>();
        var aniName = info.main_character_pose_name;
        if (animator != null && string.IsNullOrEmpty(aniName))
        {
            bool hasAnim = false;

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == aniName)
                {
                    hasAnim = true;
                    break;
                }
            }
            if (hasAnim)
                animator.Play(aniName);
        }

        yield return new WaitForSeconds(1f);

        camera.targetTexture = dispatchPhotoTexture;
        camera.gameObject.SetActive(true);

        yield return null;

        string fileName = $"{info.photo_name}_{info.camera_id}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        
        SaveImage(fileName, photoName,TimeManager.Instance.currentTime, GetTextureFromRT(dispatchPhotoTexture));

        yield return new WaitForEndOfFrame();

        string photoPath = Path.Combine(Application.persistentDataPath, $"{fileName}");
        Dispatch_Manager._instance._dispatch_photo_path_list.Add(photoPath);

        yield return null;

        yield return SceneManager.LoadSceneAsync("MainScene");
        //yield return SceneManager.UnloadSceneAsync(info.sceneName);
        //Global_Game_Manager.Instance.load_scene("MainScene", (loadedScene) =>
        //{
        //    CommonInteractManager.Instance.phoneUIs = FindObjectOfType<UI_Control_Main_Phone>() != null ? new List<UI_Control_Main_Phone>(FindObjectsOfType<UI_Control_Main_Phone>()) : new List<UI_Control_Main_Phone>();
        //});
        //CommonInteractManager.Instance.SetMainFunctionActive(true);
        //CommonInteractManager.Instance.SetPhoneButtonActive(true);
        //Save_Dispatch_PhotoList();
        //Debug.Log($"Photo saved to: {photoPath}");
        //lastPhotoPath = photoPath;
    }
    private void SaveImage(string fileName, string photoName, string obtainTime,  Texture2D tex)
    {
        PhotoData data = new PhotoData(photoName, fileName, obtainTime);
        localData.Add(data);
        // 编码为 PNG
        byte[] bytes = tex.EncodeToPNG();
        Directory.CreateDirectory(photoPath);
        string path = Path.Combine(photoPath, fileName);
        File.WriteAllBytes(path, bytes);
        EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "SavePhoto", SavePhotoTask(data, bytes), null);
    }
    public bool TryGetPhotoByIndex(int index, out Texture2D texture)
    {
        if(index >= localData.Count || index < 0)
        {
            texture = null;
            return false;
        }
        else
        {
            texture = photos[localData[index]];
            return true;
        }
    }
    private Texture2D GetTextureFromRT(RenderTexture texture)
    {
        RenderTexture.active = texture;
        var tex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        tex.Apply();
        return tex;
    }
    public ServerTask SavePhotoTask(PhotoData newData, byte[] bytes)
    {
        string msg = JsonConvert.SerializeObject((newData, bytes));
        ServerTask task = new ServerTask(msg, (string data, ServerTask task) =>
        {
            if(data == "success")
            {
                Log.Info("照片保存成功");
            }
        });
        return task;
    }
    public async Task LoadPhoto()
    {
        await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "LoadPhoto", LoadPhotoTask(), null);
        foreach(var data in localData)
        {
            string path = Path.Combine(photoPath, data.fileName);
            byte[] fileData = await File.ReadAllBytesAsync(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            photos.Add(data, texture);
        }
    }
    
    /// <summary>
    /// 同步照片
    /// </summary>
    /// <returns></returns>
    public ServerTask LoadPhotoTask()
    {
        List<string> files = Directory.GetFiles(photoPath).ToList();
        List<string> fileNames = new List<string>();
        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            fileNames.Add(fileName);
        }
        string send = JsonConvert.SerializeObject(fileNames);
        ServerTask task = new ServerTask(send, async (string data, ServerTask task) =>
        {
            if (data != "nodata")
            {
                (List<PhotoData> photoDatas,List<PhotoData> photoNeedToDownload, List<byte[]> photos) = JsonConvert.DeserializeObject<(List<PhotoData>, List < PhotoData >, List<byte[]>)>(data);
                localData = photoDatas;

                for(int i = 0; i < photoNeedToDownload.Count; i++)
                {
                    string path = Path.Combine(photoPath, photoNeedToDownload[i].fileName);
                    await File.WriteAllBytesAsync(path, photos[i]);
                }
            }
            else
            {
                localData = new List<PhotoData>();
            }
        });
        return task;
    }
}

