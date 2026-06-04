using System;
using System.Collections;
using System.IO;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Dispatch;
using Cmd;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 拍照具体流程
    /// </summary>
    public sealed class PhotoCaptureController : MonoBehaviour
    {

        public RenderTexture roomPhotoRT;
        public RenderTexture dispatchPhotoRT;
        public RenderTexture defaultPhotoRT;


        private void Start()
        {

            roomPhotoRT = RenderTextureCompatUtility.EnsureCompatible(roomPhotoRT, "DefaultPhotoRT");
            dispatchPhotoRT = RenderTextureCompatUtility.EnsureCompatible(dispatchPhotoRT, "DispatchPhotoRT");
            defaultPhotoRT = RenderTextureCompatUtility.EnsureCompatible(defaultPhotoRT, "DefaultPhotoRT");

            if (dispatchPhotoRT == null)
            {
                Log.Error("PhotoManager: DispatchRT Misssing!");
            }

            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {

        }



        public void CaptureFromServerIds(PhotoRecordInfo photoRecord, string wearsSnapshotJson, Action onFlowComplete = null)
        {
            if (photoRecord == null)
            {
                Debug.LogWarning("CaptureFromServerIds: photoRecord is null.");
                return;
            }

            if (dispatchPhotoRT == null)
            {
                Log.Error("CaptureDispatchPhotoCo: dispatchPhotoRT is null.");
                return;
            }
            if (photoRecord.mapConfigId <= 0 || photoRecord.photoConfigId <= 0)
            {
                Debug.LogWarning($"BeginCaptureDispatchPhotoByServerIds: invalid MapID={photoRecord.mapConfigId} or PhotoConfigID={photoRecord.photoConfigId}.");
                return;
            }

            var photoInfo = Global_Photo_Manager.PhotoInfos.Find(p => p.photo_id == photoRecord.photoConfigId);
            if (photoInfo == null)
            {
                Debug.LogWarning($"BeginCaptureDispatchPhotoByServerIds: Photo_Info not found, photo_id = {photoRecord.photoConfigId}.");
                return;
            }

            var mapInfo = Global_Photo_Manager.MapInfos.Find(m => m.map_id == photoRecord.mapConfigId);
            if (mapInfo == null)
            {
                Debug.LogWarning($"BeginCaptureDispatchPhotoByServerIds: Map_Info not found, map_id = {photoRecord.mapConfigId}.");
                return;
            }

            if (mapInfo.photo_Ids != null && mapInfo.photo_Ids.Count > 0 && !mapInfo.photo_Ids.Contains(photoRecord.photoConfigId))
                Debug.LogWarning($"BeginCaptureDispatchPhotoByServerIds: map_id={photoRecord.mapConfigId} photo_Ids does not contain photo_id={photoRecord.photoConfigId}; continue with server photo id.");

            if (string.IsNullOrEmpty(mapInfo.map_Scene_Name))
            {
                Log.Error($"BeginCaptureDispatchPhotoByServerIds: map_id={photoRecord.mapConfigId} has empty map_Scene_Name.");
                return;
            }

            SceneLoadingHelper.Instance.LoadScene(mapInfo.map_Scene_Name, () =>
            {
                var loadedCharacter = GameAssets.Instance.mainCharacter_Dispatch;
                StartCoroutine(CaptureThenInvoke(photoRecord, photoInfo, loadedCharacter, wearsSnapshotJson, onFlowComplete));
            });

        }

        private IEnumerator CaptureThenInvoke(PhotoRecordInfo photoRecord, Photo_Info photoInfo, GameObject loadedCharacter, string wearsSnapshotJson, Action onFlowComplete)
        {
            ApplyRainImageState();

            if (loadedCharacter == null)
            {
                Log.Error("CaptureDispatchPhotoCo: mainCharacter_Dispatch is null.");
                BackToMainScene();
                yield break;
            }

            var photoRoot = GameObject.Find("PhotoRoot")?.transform;
            if (photoRoot == null)
            {
                Log.Error("CaptureDispatchPhotoCo: PhotoRoot not found in the scene.");
                BackToMainScene();
                yield break;
            }

            if (!TrySelectCameraGroup(photoRoot, photoInfo.camera_id, out var cameraGroup))
            {
                BackToMainScene();
                yield break;
            }

            var camera = cameraGroup.GetComponentInChildren<Camera>(true);
            if (camera == null)
            {
                Log.Error($"CaptureDispatchPhotoCo: Camera not found under camera group \"{cameraGroup.name}\".");
                BackToMainScene();
                yield break;
            }

            var characterPoint = cameraGroup.GetComponentInChildren<Model_Placeholder>();
            if (characterPoint == null)
            {
                Log.Error($"CaptureDispatchPhotoCo: Model_Placeholder not found under camera group \"{cameraGroup.name}\".");
                BackToMainScene();
                yield break;
            }

            SpawnCharacter(loadedCharacter, characterPoint, wearsSnapshotJson, photoInfo.main_character_pose_name);

            yield return new WaitForSeconds(0.2f);

            camera.targetTexture = dispatchPhotoRT;
            camera.gameObject.SetActive(true);
            BindWeatherCanvas(camera);

            yield return null;

            EvtDsp.TriggerEvt(EvtNames.SceneLoading_Close);

            string fileName = string.IsNullOrEmpty(photoRecord.FileName)
                ? LocalSaveManager.BuildDispatchPhotoFileName(photoRecord.photoUId, photoRecord.photoName, photoInfo.camera_id)
                : photoRecord.FileName;
            CaptureAndSavePhoto(PhotoType.Dispatch, fileName, false, photoRecord.photoConfigName, null, photoRecord);

            BackToMainScene();
            Debug.Log($"Photo saved to: {Global_Photo_Manager.Instance.GetPhotoSavePath(PhotoType.Dispatch, fileName)}");

            onFlowComplete?.Invoke();
        }


        private static void ApplyRainImageState()
        {
            GameObject rainImage = GameObject.Find("Rain_Image");
            if (rainImage == null)
            {
                Debug.LogWarning("Rain_Image not found in the scene.");
                return;
            }

            var weather = Global_Game_Manager.Instance?._weather_state;
            rainImage.SetActive(weather != null && weather._current_weather == "Rain");
        }

        private static bool TrySelectCameraGroup(Transform photoRoot, int cameraId, out Transform selected)
        {
            selected = null;
            int groupCount = photoRoot.childCount;

            if (groupCount == 0)
            {
                Log.Error("CaptureDispatchPhotoCo: PhotoRoot has no camera groups.");
                return false;
            }

            if (cameraId <= 0 || cameraId > groupCount)
            {
                Log.Error($"CaptureDispatchPhotoCo: photo_Camera_ID={cameraId} is out of range. PhotoRoot child count = {groupCount}, valid range = 1-{groupCount}.");
                return false;
            }

            for (int i = 1; i <= groupCount; i++)
            {
                var cameraGroup = photoRoot.GetChild(i - 1);
                bool isSelected = i == cameraId;
                cameraGroup.gameObject.SetActive(isSelected);
                if (isSelected)
                    selected = cameraGroup;
            }

            return selected != null;
        }

        private static void SpawnCharacter(GameObject loadedCharacter, Model_Placeholder characterPoint, string wearsSnapshotJson, string animationName)
        {
            Unity_Tools.ClearAllChildren(characterPoint.transform);

            var character = UnityEngine.Object.Instantiate(
                loadedCharacter,
                characterPoint.transform.position,
                characterPoint.transform.rotation,
                characterPoint.transform);

            Unity_Tools.IdentityGameObject(character);

            if (!string.IsNullOrEmpty(wearsSnapshotJson))
            {
                var clothesController = character.GetComponentInChildren<CharacterClothesController>();
                if (clothesController == null)
                {
                    Debug.Log("该派遣角色没有服装组件！");
                }
                else
                {
                    CharacterClothesManager.Instance.ApplyClothesSnapshotToController(clothesController, wearsSnapshotJson);
                }
            }

            var animator = character.GetComponentInChildren<Animator>();
            if (animator == null || string.IsNullOrEmpty(animationName))
                return;

            var controller = animator.runtimeAnimatorController;
            if (controller?.animationClips == null)
                return;

            foreach (var clip in controller.animationClips)
            {
                if (clip != null && clip.name == animationName)
                {
                    animator.Play(animationName);
                    return;
                }
            }
        }

        private static void BindWeatherCanvas(Camera camera)
        {
            Canvas weatherCanvas = GameObject.Find("WeatherCanvas")?.GetComponent<Canvas>();
            if (weatherCanvas != null)
                weatherCanvas.worldCamera = camera;
            else
                Debug.LogWarning("WeatherCanvas not found in the scene.");
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
                    PhotoRecordInfo info = Global_Photo_Manager.Instance. AddPhotoInfo(path, fileName, mode, photoConfigName, recordInfo);
                    Destroy(tex);
                    onSaved?.Invoke(info);
                }));
            }
            else
            {
                StartCoroutine(CaptureRT(mode, (tex) =>
                {
                    string path = SaveTexture(tex, fileName, mode, saveDirectoryOverride);
                    PhotoRecordInfo info = Global_Photo_Manager.Instance.AddPhotoInfo(path, fileName, mode, photoConfigName, recordInfo);
                    Destroy(tex);
                    onSaved?.Invoke(info);
                }));
            }
        }

        private IEnumerator CaptureRT(PhotoType mode, Action<Texture2D> onFinish = null)
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
            switch (mode)
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

        public void BackToMainScene()
        {
            SceneLoadHelper.LoadSceneAsync(
                SceneLoadHelper.MainSceneName,
                (_) => EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active));
        }
    }
}
