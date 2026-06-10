using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Dispatch;
using CLIP.Project_Mouse.Network;
using Cmd;
using Google.Protobuf;
using Sych.ShareAssets.Runtime;
using UnityEngine;
using GF_SP = CLIP.Framework_Core.Serialization.Serialization_Provider;
using PM_RM = CLIP.Framework_Unity.Asset.GameAssets;


namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Global_Photo_Manager : SingletonMono<Global_Photo_Manager>
    {
        protected override bool PersistAcrossScenes => true;

        /// <summary>
        /// 本地相册照片记录。
        /// </summary>
        public List<PhotoRecordInfo> imageList = new List<PhotoRecordInfo>();

        /// <summary>
        /// 派遣照片记录。该列表以服务器 GetPhotosRes 为准，不从本地存档恢复。
        /// </summary>
        public List<PhotoRecordInfo> dispatchList = new List<PhotoRecordInfo>();

        private List<Map_Info> mapInfos = new List<Map_Info>();
        private List<Photo_Info> photoInfos = new List<Photo_Info>();

        public static List<Map_Info> MapInfos => Instance.mapInfos;
        public static List<Photo_Info> PhotoInfos => Instance.photoInfos;

        public RenderTexture roomPhotoRT;
        public RenderTexture dispatchPhotoRT;
        public RenderTexture defaultPhotoRT;

        public string Last_Photo_Path => imageList != null && imageList.Count > 0 ? imageList[^1]?.localPath : string.Empty;
        private string Last_Photo_Name => imageList != null && imageList.Count > 0 ? imageList[^1]?.FileName : string.Empty;

        private DispatchPhotoCaptureController _dispatchPhotoCaptureController;
        private PhotoTextureCache _textureCache;

        private DispatchPhotoCaptureController DispatchPhotoCapture =>
            _dispatchPhotoCaptureController ??= new DispatchPhotoCaptureController(this);

        private PhotoTextureCache TextureCache =>  _textureCache ??= new PhotoTextureCache();

        void Start()
        {
            roomPhotoRT = RenderTextureCompatUtility.EnsureCompatible(roomPhotoRT, "DefaultPhotoRT");
            dispatchPhotoRT = RenderTextureCompatUtility.EnsureCompatible(dispatchPhotoRT, "DispatchPhotoRT");
            defaultPhotoRT = RenderTextureCompatUtility.EnsureCompatible(defaultPhotoRT, "DefaultPhotoRT");

            if (dispatchPhotoRT == null)
            {
                Log.Error("PhotoManager: DispatchRT Misssing!");
            }

            photoInfos = JsonDataManager.LoadPhotoInfo();
            mapInfos = JsonDataManager.LoadMapInfo();

            LoadtInfoLocal();
            RequestDispatchPhotosFromServer();
        }


        /// <summary>
        /// 使用服务器 BackToHomeRewardS2C 下发的 MapID 和 PhotoConfigID 拍摄派遣照片。
        /// </summary>
        /// <param name="onFlowComplete">派遣拍照流程完成后的回调。</param>
        public void CaptureDispatchPhotoFromServerIds(Cmd.RolePhotoInfo rolePhotoInfo, Action onFlowComplete = null)
        {
            var photoRecord = BuildRecodInfo(rolePhotoInfo);

            DispatchPhotoCapture.CaptureFromServerIds(photoRecord, rolePhotoInfo.Wears, onFlowComplete);
        }

        /// <summary>
        /// 拍摄并保存照片到本地。
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="name"></param>
        /// <param name="isScreen"></param>
        /// <param name="photoConfigName"></param>
        /// <param name="saveDirectoryOverride"></param>
        public void CaptureAndSavePhoto(PhotoType mode, string name = "", bool isScreen = false, string photoConfigName = "", string saveDirectoryOverride = null, PhotoRecordInfo recordInfo = null, Action<PhotoRecordInfo> onSaved = null)
        {
            string fileName = name;

            if (string.IsNullOrEmpty(name))
            {
                fileName = mode.ToString() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            }

            if (isScreen == true)
            {
                StartCoroutine(CaptureScreen((tex) =>
                {
                    string path = SaveTexture(tex, fileName, mode, saveDirectoryOverride);
                    PhotoRecordInfo info = AddPhotoInfo(path, fileName, mode, photoConfigName, recordInfo);
                    Destroy(tex);
                    onSaved?.Invoke(info);
                }));
            }
            else
            {
                StartCoroutine(CaptureRT(mode, (tex) =>
                {
                    string path = SaveTexture(tex, fileName, mode, saveDirectoryOverride);
                    PhotoRecordInfo info = AddPhotoInfo(path, fileName, mode, photoConfigName, recordInfo);
                    Destroy(tex);
                    onSaved?.Invoke(info);
                }));
            }
        }

        private IEnumerator CaptureRT(PhotoType mode, Action<Texture2D> onFinish= null)
        {
            yield return new WaitForEndOfFrame();

            RenderTexture rt = GetRenderTexture(mode);
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            onFinish?.Invoke(tex);
        }

        private IEnumerator CaptureScreen(Action<Texture2D> onFinish = null)
        {
            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            tex.Apply();

            onFinish?.Invoke(tex);
        }

        private RenderTexture GetRenderTexture(PhotoType mode)
        {
            switch(mode)
            {
                case PhotoType.Dispatch:
                    return dispatchPhotoRT;
                case PhotoType.Room:
                    return roomPhotoRT;
                default:
                    return defaultPhotoRT;
            }
        }

        public string SaveTexture(Texture2D tex, string fileName, PhotoType type, string folderOverride = null)
        {
            if (tex == null)
            {
                Debug.LogError("LocalSaveManager.SaveTexture: texture is null.");
                return string.Empty;
            }

            string photoLocalPath = LocalSaveManager.GetPhotoPath(type, fileName, folderOverride);
            string folder = Path.GetDirectoryName(photoLocalPath);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllBytes(photoLocalPath, tex.EncodeToPNG());
            Debug.Log("Screenshot saved to: " + photoLocalPath);

            return photoLocalPath;
        }

        public string GetPhotoSavePath(PhotoType type, string fileName, string folderOverride = null)
        {
            return LocalSaveManager.GetPhotoPath(type, fileName, folderOverride);
        }

        public PhotoRecordInfo AddPhotoInfo(string path, string fileName, PhotoType type, string photoConfigName = "", PhotoRecordInfo recordInfo = null)
        {
            PhotoRecordInfo info = recordInfo ?? new PhotoRecordInfo();
            info.photoUId = EnsurePhotoUid(info.photoUId);
            info.photoName = !string.IsNullOrEmpty(info.photoName)
                ? info.photoName
                : (!string.IsNullOrEmpty(photoConfigName) ? photoConfigName : Path.GetFileNameWithoutExtension(fileName));
            info.photoConfigName = !string.IsNullOrEmpty(info.photoConfigName) ? info.photoConfigName : photoConfigName;
            info.localPath = path;
            info.captureTime = DateTime.Now;
            info.photoType = type;

            UpsertPhotoRecord(info);
            SaveInfoLocal();
            return info;
        }

        private void UpsertPhotoRecord(PhotoRecordInfo info)
        {
            if (info == null)
                return;

            imageList.RemoveAll(p => IsSamePhotoRecord(p, info));
            imageList.Add(info);

            if (info.photoType == PhotoType.Dispatch)
            {
                dispatchList.RemoveAll(p => IsSamePhotoRecord(p, info));
                dispatchList.Add(info);
            }
        }

    
        /// <summary>
        /// 获取派遣照片列表。数据来自服务器刷新后的 dispatchList。
        /// </summary>
        public List<PhotoRecordInfo> GetDispatchPhotos()
        {
            return dispatchList;
        }

        /// <summary>
        /// 根据照片名或配置名查找照片记录。
        /// </summary>
        public PhotoRecordInfo GetPhotoInfo(string photoName)
        {
            if (string.IsNullOrEmpty(photoName)) return null;
            return imageList.Find(p => p != null && (p.photoName == photoName || p.photoConfigName == photoName));
        }

        public PhotoRecordInfo GetPhotoInfoByUid(long photoUid)
        {
            if (photoUid == 0) return null;
            return imageList.Find(p => p != null && p.photoUId == photoUid);
        }

        public PhotoRecordInfo ResolvePhotoInfo(object key)
        {
            switch (key)
            {
                case null:
                    return null;
                case PhotoRecordInfo info:
                    return info;
                case long uid:
                    return GetPhotoInfoByUid(uid);
                case ulong uid:
                    return uid <= long.MaxValue ? GetPhotoInfoByUid((long)uid) : null;
                case int uid:
                    return GetPhotoInfoByUid(uid);
                case string text:
                    return ResolvePhotoInfo(text);
                default:
                    return null;
            }
        }

        private PhotoRecordInfo ResolvePhotoInfo(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if (long.TryParse(text, out long uid))
                return GetPhotoInfoByUid(uid);

            return imageList.Find(p => p != null &&
                (p.localPath == text || p.photoName == text || p.photoConfigName == text));
        }

        public PhotoRecordInfo GetLatestDispatchPhotoByConfigName(string photoConfigName)
        {
            if (string.IsNullOrEmpty(photoConfigName)) return null;

            for (int i = dispatchList.Count - 1; i >= 0; i--)
            {
                var info = dispatchList[i];
                if (info != null && info.photoConfigName == photoConfigName)
                {
                    return info;
                }
            }

            return null;
        }

        /// <summary>
        /// 删除照片记录和本地文件；派遣照片会同步请求服务器删除。
        /// </summary>
        public void DeletePhoto(PhotoRecordInfo info)
        {
            if (info == null) return;

            ReleaseTextureByInfo(info);
            imageList.RemoveAll(p => IsSamePhotoRecord(p, info));

            LocalSaveManager.DeletePhotoFile(info);

            if (info.photoType == PhotoType.Dispatch) dispatchList.RemoveAll(p => IsSamePhotoRecord(p, info));
            SaveInfoLocal();

            if (info.photoType == PhotoType.Dispatch && info.photoUId > 0)
            {
                Cmd.DelPhotosReq delPhotosReq = new Cmd.DelPhotosReq() { UID = (ulong)info.photoUId };
                EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, delPhotosReq);
            }
        }

        public void SaveLastPhotoToGallery()
        {
            var last = imageList[^1];
            SaveToGallery(last.localPath, last.FileName);
            PromptManager.ShowUpPrompt(PromptId.PhotoSaved);
        }

        public void SaveToGallery(string path, string fileName)
        {
#if UNITY_ANDROID || UNITY_IOS
            NativeGallery.SaveImageToGallery(path, "XiaoTai", fileName);
#endif
        }


        public async Task<Texture2D> Load_Last_Image(Action<Texture2D> callback )
        {
            if (File.Exists(Last_Photo_Path))
            {
                return await PM_RM.LoadPngAsTextureAsync(Last_Photo_Path, callback);
            }
            return null;
        }

        public async Task<Texture2D> LoadImageByName(string imageName, Action<Texture2D> callback= null)
        {
            var imageInfo = imageList.Find(info => info.photoName == imageName);
            if(imageInfo != null && File.Exists(imageInfo.localPath))
            {
                return await PM_RM.LoadPngAsTextureAsync(imageInfo.localPath, callback);
            }
            else
            {
                Debug.LogWarning($"Image with name {imageName} not found in local image list or file does not exist.");
                return null;
            }
        }

        public async Task<Texture2D> LoadImageByPath(string imagePath, Action<Texture2D> callback = null)
        {
            if (File.Exists(imagePath))
            {
                return await PM_RM.LoadPngAsTextureAsync(imagePath, callback);
            }
            else
            {
                Debug.LogWarning($"File does not exist at photLocalPath: {imagePath}");
                return null;
            }
        }

        public void Share_previous_saved_image_via_native()
        {
            string image_path = Last_Photo_Path;
            Debug.Log("Share_previous_saved_image_via_native()_image_path_=_" + image_path);
#if UNITY_ANDROID || UNITY_IOS
            var _paths = new List<string>();
            _paths.Add(image_path);
            Share.Item(image_path, Share_callback);
#endif
        }

        public void Share_callback(bool _flag)
        {
            if (_flag == true)
            {
                Debug.Log("Share operation completed successfully.");
            }
            else
            {
                Debug.LogError("Share operation failed or was cancelled.");
            }
        }


        #region Photo record persistence
        public void SaveInfoLocal()
        {
            LocalSaveManager.SavePhotoRecords(imageList, dispatchList);
        }
        public void LoadtInfoLocal()
        {
            LocalSaveManager.LoadPhotoRecords(out imageList, out dispatchList);
            imageList ??= new List<PhotoRecordInfo>();
            dispatchList ??= new List<PhotoRecordInfo>();

            imageList.RemoveAll(p => p != null && p.photoType == PhotoType.Dispatch);
            dispatchList.RemoveAll(p => p == null);
            foreach (var info in dispatchList)
            {
                info.photoType = PhotoType.Dispatch;
                imageList.Add(info);
            }
        }

        #endregion

        #region Texture cache

        public void LoadTexture(string path, Action<Texture2D> callback)
        {
            TextureCache.LoadTexture(path, callback);
        }

        public void ReleaseTexture(string path)
        {
            TextureCache.ReleaseTexture(path);
        }

        public void UnloadUnusedTextures()
        {
            TextureCache.UnloadUnusedTextures();
        }

        public void UnloadAllTextures()
        {
            TextureCache.UnloadAllTextures();
        }

        public bool IsTextureLoaded(string path)
        {
            return TextureCache.IsTextureLoaded(path);
        }

        public void ReleaseTextureByInfo(PhotoRecordInfo info)
        {
            TextureCache.ReleaseTextureByInfo(info);
        }

        #endregion


        private void RequestDispatchPhotosFromServer()
        {
            EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, new Cmd.GetPhotosReq());
        }

        public void ReceiveGetPhotosRes(GetPhotosRes res)
        {
            if (res == null)
                return;

            dispatchList ??= new List<PhotoRecordInfo>();
            var mergedDispatchList = new List<PhotoRecordInfo>();
            foreach (var info in dispatchList)
            {
                if (info == null || HasSamePhotoConfig(mergedDispatchList, info))
                    continue;

                if (LocalSaveManager.TryFindLocalPhotoFile(info, out string localPath))
                {
                    info.localPath = localPath;
                    mergedDispatchList.Add(info);
                }
            }

            foreach (var data in res.Photos)
            {
                PhotoRecordInfo photoRecordInfo = BuildRecodInfo(data);
                if (photoRecordInfo == null)
                    continue;

                if (HasSamePhotoConfig(mergedDispatchList, photoRecordInfo))
                    continue;

                if (LocalSaveManager.TryFindLocalPhotoFile(photoRecordInfo, out string localPath))
                {
                    photoRecordInfo.localPath = localPath;
                    UpsertPhotoRecord(mergedDispatchList, photoRecordInfo);
                }

            }

            dispatchList.Clear();
            dispatchList.AddRange(mergedDispatchList);

            imageList.RemoveAll(p => p != null && p.photoType == PhotoType.Dispatch);
            imageList.AddRange(dispatchList);

            SaveInfoLocal();
        }

        private PhotoRecordInfo BuildRecodInfo(RolePhotoInfo data)
        {
            var photoInfo = photoInfos.Find(p => p.photo_id == data.PhotoConfigID);
            string photoName = photoInfo != null && !string.IsNullOrEmpty(photoInfo.photo_name)
                ? photoInfo.photo_name
                : $"DispatchPhoto_{data.PhotoConfigID}";
            int cameraId = photoInfo != null ? photoInfo.camera_id : 0;
            long photoUid = EnsurePhotoUid((long)data.UID);
            string fileName = LocalSaveManager.BuildDispatchPhotoFileName(photoUid, photoName, cameraId);

            return new PhotoRecordInfo
            {
                photoUId = photoUid,
                photoConfigId = data.PhotoConfigID,
                mapConfigId = data.MapID,
                photoName = photoName,
                localPath = LocalSaveManager.GetPhotoPath(PhotoType.Dispatch, fileName),
                photoType = PhotoType.Dispatch,
                photoConfigName = photoName
            };
        }

        private long EnsurePhotoUid(long photoUid)
        {
            return photoUid != 0 ? photoUid : LocalSaveManager.GenerateLocalPhotoUid();
        }

        private bool IsSamePhotoRecord(PhotoRecordInfo a, PhotoRecordInfo b)
        {
            if (a == null || b == null)
                return false;

            if (a.photoUId != 0 && b.photoUId != 0)
                return a.photoUId == b.photoUId;

            return !string.IsNullOrEmpty(a.localPath) && a.localPath == b.localPath;
        }

        private void UpsertPhotoRecord(List<PhotoRecordInfo> list, PhotoRecordInfo info)
        {
            if (list == null || info == null)
                return;

            int index = list.FindIndex(p => IsSamePhotoRecord(p, info));
            if (index < 0)
            {
                list.Add(info);
                return;
            }

            PhotoRecordInfo existing = list[index];
            if (string.IsNullOrEmpty(info.localPath) && existing != null)
                info.localPath = existing.localPath;

            if (info.captureTime == default && existing != null)
                info.captureTime = existing.captureTime;

            list[index] = info;
        }

        private bool HasSamePhotoConfig(List<PhotoRecordInfo> list, PhotoRecordInfo info)
        {
            if (list == null || info == null || info.photoConfigId == 0)
                return false;

            return list.Exists(p => p != null && p.photoConfigId == info.photoConfigId);
        }
    }
}
