using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 目前没用，拍照看Global_Photo_Manager
/// </summary>
public class PhotoManager : SingletonMono<PhotoManager>
{
    public RenderTexture dispatchPhotoTexture;
    public Dictionary<PhotoData, Texture2D> photos = new Dictionary<PhotoData, Texture2D>();

    public List<PhotoData> localData = new List<PhotoData>();

    [SerializeField] 
    private string photoPath;
    private void Start()
    {
        dispatchPhotoTexture = RenderTextureCompatUtility.EnsureCompatible(dispatchPhotoTexture, "DispatchPhotoTexture");

        DontDestroyOnLoad(gameObject);
        photoPath = Path.Combine(Application.persistentDataPath, "photo",NetWork_Center_WSS.Instance?._player_name ?? "Default_Player");
            
        _ = LoadPhoto();
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
        // TODO zhaorui
        // await EvtDsp.ReturnEvt<int, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, 60009, SavePhotoTask(data, bytes), null);
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
        //TODO zhaorui
        var req = new Cmd.GetAllClothesReq();
        ServerTask task = new ServerTask(req, (string data, ServerTask task) =>
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
        // TODO zhaorui2
        // await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "LoadPhoto", LoadPhotoTask(), null);
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
        //TODO zhaorui
        var req = new Cmd.GetAllClothesReq();
        ServerTask task = new ServerTask(req, async (string data, ServerTask task) =>
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
