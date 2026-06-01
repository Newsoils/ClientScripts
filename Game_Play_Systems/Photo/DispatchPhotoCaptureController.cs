using System;
using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Kernel.Dispatch;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 拍照具体流程
    /// </summary>
    public sealed class DispatchPhotoCaptureController
    {
        private readonly Global_Photo_Manager _photoManager;

        public string LastDispatchPhotoPath { get; private set; } = string.Empty;

        public DispatchPhotoCaptureController(Global_Photo_Manager photoManager)
        {
            _photoManager = photoManager;
        }


        public void CaptureFromServerIds(PhotoRecordInfo photoRecord, string wearsSnapshotJson, Action onFlowComplete = null)
        {
            if (photoRecord == null)
            {
                Debug.LogWarning("CaptureFromServerIds: photoRecord is null.");
                return;
            }

            if (_photoManager.dispatchPhotoRT == null)
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
                _photoManager.StartCoroutine(CaptureThenInvoke(photoRecord, photoInfo, loadedCharacter, wearsSnapshotJson, onFlowComplete));
            });

            //SceneLoadHelper.LoadSceneAsync(mapInfo.map_Scene_Name,(_)=>
            //{
            //    var loadedCharacter = GameAssets.Instance.mainCharacter_Dispatch;
            //    _photoManager.StartCoroutine(CaptureThenInvoke(photoRecord, photoInfo, loadedCharacter, wearsSnapshotJson, onFlowComplete));
            //});
                
        }

        private IEnumerator CaptureThenInvoke(PhotoRecordInfo photoRecord, Photo_Info photoInfo, GameObject loadedCharacter,string wearsSnapshotJson, Action onFlowComplete)
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

            camera.targetTexture = _photoManager.dispatchPhotoRT;
            camera.gameObject.SetActive(true);
            BindWeatherCanvas(camera);

            yield return null;

            EvtDsp.TriggerEvt(EvtNames.SceneLoading_Close);

            string fileName = string.IsNullOrEmpty(photoRecord.FileName)
                ? LocalSaveManager.BuildDispatchPhotoFileName(photoRecord.photoUId, photoRecord.photoName, photoInfo.camera_id)
                : photoRecord.FileName;
            _photoManager.CaptureAndSavePhoto(PhotoType.Dispatch, fileName, false, photoRecord.photoConfigName, null, photoRecord);

            BackToMainScene();
            Debug.Log($"Photo saved to: {_photoManager.GetPhotoSavePath(PhotoType.Dispatch, fileName)}");

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

        private static void SpawnCharacter(GameObject loadedCharacter, Model_Placeholder characterPoint,string wearsSnapshotJson, string animationName)
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

        public void BackToMainScene()
        {
            SceneLoadHelper.LoadSceneAsync(
                SceneLoadHelper.MainSceneName,
                (_) => EvtDsp.TriggerEvt(EvtNames.Set_MainPanel_All_Active));
        }
    }
}
