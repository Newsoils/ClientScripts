using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using Lean.Common;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.NewFrame.UI
{
    public class PhotoCapturePanel : UIPanelBase
    {
        [Header("相机切换")]
        public GameObject photoCameras;
        public List<GameObject> photoCameraButtons = new List<GameObject>();

        private Dictionary<string, List<GameObject>> roomCameraDict = new Dictionary<string, List<GameObject>>();
        public GameObject currentCameraButtonsRoot;

        [Header("相机模式")]
        public PhotoCameraType currentPhotoCameraType = PhotoCameraType.PhotoCamera0;
        public GameObject currentPhotoCamerRoot;
        public Camera currentCamera;
        public PhotoCameraType choosedPhotoCameraType;
        public GameObject choosedPhotoCameraRoot;

        [Header("拍照设置")]
        public Camera photoCamera;
        public RenderTexture renderTexture;
        public RawImage photoDisplay;
        [Range(0, 1080)] public int captureX = 0, captureWidth = 1080;
        [Range(0, 1920)] public int captureY = 0, captureHeight = 1920;
        //captureX：截图区域左下角的横坐标
        //captureY：截图区域左下角的纵坐标
        //captureWidth：截图区域的宽度
        //captureHeight：截图区域的高度

        [Header("相机切换按钮父物体")]
        public GameObject livingroomCameraButtonsRoots;
        public GameObject balconyCameraButtonsRoots;
        public GameObject bedroomCameraButtonsRoots;
        public GameObject toiletCameraButtonsRoots;

        [Header("相机Root父物体")]
        public GameObject livingroomCameraRoots;
        public GameObject balconyCameraRoots;
        public GameObject bedroomCameraRoots;
        public GameObject toiletCameraRoots;

        [Header("UI")]
        public RawImage bgImage;
        public GameObject afterPhoto;
        public GameObject photoCanvas;
        public GameObject frameCanvas;
        public GameObject photoPanel;
        [Range(0, 1)] public float alphaValue = 0.5f;


        private void Start()
        {
            //takePhoto = GameObject.Find("TakePhoto");
            AddChildButtonsRecursively(photoCameras.transform);
            InitializedRoomCameraDict();
            //takePhoto.GetComponent<Button>().onClick.AddListener(OpenTakePhoto);
            //Global_Photo_Manager.Instance.roomPhotoRawImage = photoDisplay;
            DontDestroyOnLoad(this.gameObject);
            //bgImage.texture = Global_Photo_Manager.Instance.roomPhotoRT;
            //renderTexture = Global_Photo_Manager.Instance.roomPhotoRT;
        }


        #region 接口方法实现

        // 打开拍照界面
        public override void OpenPanel(params object[] data)
        {
            string currentRoomName = GetCurrentRoomName();
            RoomSystem.Instance.canSwitchRoom = false;
            switch (currentRoomName)
            {
                case "客厅":
                    currentCameraButtonsRoot = livingroomCameraButtonsRoots;
                    break;
                case "卧室":
                    currentCameraButtonsRoot = bedroomCameraButtonsRoots;
                    break;
                case "浴室":
                    currentCameraButtonsRoot = toiletCameraButtonsRoots;
                    break;
                case "阳台":
                    currentCameraButtonsRoot = balconyCameraButtonsRoots;
                    break;
                default:
                    Debug.Log("当前房间不存在");
                    return;
            }

            currentPhotoCamerRoot = roomCameraDict[currentRoomName][0];
            currentPhotoCamerRoot.SetActive(true);

            Camera foundCamera = currentPhotoCamerRoot.GetComponentInChildren<Camera>(true);
            currentCamera = foundCamera;
            frameCanvas.GetComponent<Canvas>().worldCamera = foundCamera;
            foundCamera.targetTexture = null;

            photoCanvas.SetActive(true);
            InputManager.Instance.AllowTouchOnUI = true;
            CameraManager.Instance.CloseCamera();
            //ResetCamera resetCamera = currentPhotoCamerRoot.GetComponentInChildren<ResetCamera>(true);
            //resetCamera.Reset();
            EvtDsp.TriggerEvt(EvtNames.OnTakePhotoPanelOpen);
        }

        // 关闭拍照界面
        public override void ClosePanel()
        {
            currentPhotoCamerRoot.SetActive(false);
            photoCanvas.SetActive(false);
            InputManager.Instance.AllowTouchOnUI = false;
            RoomSystem.Instance.canSwitchRoom = true;

            CameraManager.Instance.OpenCamera();
            EvtDsp.TriggerEvt(EvtNames.OnTakePhotoPanelClose);
        }

        #endregion


        #region 内部方法

        // 初始化相机Root字典
        private void InitializedRoomCameraDict()
        {
            //foreach(var room in RoomSystem.Instance.Room_SO.roomConfigs)
            //{
       
            //}
            roomCameraDict.Add("客厅", new List<GameObject>());
            roomCameraDict.Add("卧室", new List<GameObject>());
            roomCameraDict.Add("浴室", new List<GameObject>());
            roomCameraDict.Add("阳台", new List<GameObject>());
            foreach (Transform child in livingroomCameraRoots.transform)
            {
                roomCameraDict["客厅"].Add(child.gameObject);
            }
            foreach (Transform child in balconyCameraRoots.transform)
            {
                roomCameraDict["阳台"].Add(child.gameObject);
            }
            foreach (Transform child in bedroomCameraRoots.transform)
            {
                roomCameraDict["卧室"].Add(child.gameObject);
            }
            foreach (Transform child in toiletCameraRoots.transform)
            {
                roomCameraDict["浴室"].Add(child.gameObject);
            }
        }

        public string GetEnglishName(string roomName)
        {
            switch (roomName)
            {
                case "客厅":
                    return "living_room";
                case "卧室":
                    return "bed_room";
                case "浴室":
                    return "toilet";
                case "阳台":
                    return "balcony";
                default:
                    Debug.Log("当前房间不存在");
                    return null;
            }
        }

        // 递归添加子物体中的按钮
        private void AddChildButtonsRecursively(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.GetComponent<PhotoCameraSetting>() != null && child.GetComponent<Button>() != null)
                {
                    photoCameraButtons.Add(child.gameObject);
                }

                AddChildButtonsRecursively(child);
            }
        }


        // 获取当前房间名称
        private string GetCurrentRoomName()
        {
           return RoomSystem.currentRoom != null ? RoomSystem.currentRoom.RoomName : null;
        }

        private void SetPhotoPanelTransparency(float alphaValue)
        {
            //return;
            //Image image = photoPanel.GetComponent<Image>();
            //if (image != null)
            //{
            //    var color = image.color;
            //    color.a = alphaValue;
            //    image.color = color;
            //}
            //else
            //{
            //    Debug.LogError("PhotoPanel does not have an Image component.");
            //}
        }

        #endregion

        // 切换相机类型
        public void SwitchPhotoCameraType()
        {
            string currentRoomName = GetCurrentRoomName();
            if (currentRoomName == null) return;

            int nextType = ((int)currentPhotoCameraType + 1) % 3;
            PhotoCameraType targetType = (PhotoCameraType)nextType;

            var cameraRoots = roomCameraDict[currentRoomName];
            if (cameraRoots == null || cameraRoots.Count < 3) return;

            GameObject targetCameraRoot = cameraRoots[nextType];

            LeanPinchCamera newPinchCamera = targetCameraRoot.GetComponentInChildren<LeanPinchCamera>(true);
            LeanPitchYaw newPitchYaw = targetCameraRoot.GetComponentInChildren<LeanPitchYaw>(true);
            LeanPinchCamera pinchCamera = currentPhotoCamerRoot.GetComponentInChildren<LeanPinchCamera>(true);
            LeanPitchYaw pitchYaw = currentPhotoCamerRoot.GetComponentInChildren<LeanPitchYaw>(true);

            newPitchYaw.Pitch = pitchYaw.Pitch;
            newPitchYaw.Yaw = pitchYaw.Yaw;
            newPinchCamera.Zoom = pinchCamera.Zoom;
            newPinchCamera.gameObject.transform.localPosition = pinchCamera.gameObject.transform.localPosition;

            currentPhotoCameraType = targetType;
            currentPhotoCamerRoot.SetActive(false);
            targetCameraRoot.SetActive(true);
            currentPhotoCamerRoot = targetCameraRoot;
            Camera foundCamera = currentPhotoCamerRoot.GetComponentInChildren<Camera>(true);
            currentCamera = foundCamera;
            frameCanvas.GetComponent<Canvas>().worldCamera = foundCamera;
            foundCamera.targetTexture = null;
        }

        // 打开选择相机界面
        public void ShowPhotoCameraButtons()
        {
            string currentRoomName = GetCurrentRoomName();
            if (currentRoomName == null) return;

            currentCameraButtonsRoot.SetActive(true);

            for (int i = 0; i < photoCameraButtons.Count; i++)
            {
                var cameraSetting = photoCameraButtons[i].GetComponent<PhotoCameraSetting>();
                if (cameraSetting != null)
                {
                    photoCameraButtons[i].SetActive(cameraSetting.roomName == currentRoomName && cameraSetting.cameraType != currentPhotoCameraType);
                }
            }

            var layout = currentCameraButtonsRoot.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(currentCameraButtonsRoot.GetComponent<RectTransform>());
            }
        }

        // 确认选择相机类型
        public void ConfirmToChangePhotoCameraType()
        {
            LeanPinchCamera newPinchCamera = choosedPhotoCameraRoot.GetComponentInChildren<LeanPinchCamera>(true);
            LeanPitchYaw newPitchYaw = choosedPhotoCameraRoot.GetComponentInChildren<LeanPitchYaw>(true);
            LeanPinchCamera pinchCamera = currentPhotoCamerRoot.GetComponentInChildren<LeanPinchCamera>(true);
            LeanPitchYaw pitchYaw = currentPhotoCamerRoot.GetComponentInChildren<LeanPitchYaw>(true);

            newPinchCamera.gameObject.transform.position = pinchCamera.gameObject.transform.position;
            newPinchCamera.Zoom = pinchCamera.Zoom;
            newPitchYaw.Pitch = pitchYaw.Pitch;
            newPitchYaw.Yaw = pitchYaw.Yaw;

            currentPhotoCameraType = choosedPhotoCameraType;
            currentPhotoCamerRoot.SetActive(false);
            choosedPhotoCameraRoot.SetActive(true);
            currentPhotoCamerRoot = choosedPhotoCameraRoot;
            Camera foundCamera = currentPhotoCamerRoot.GetComponentInChildren<Camera>(true);
            currentCamera = foundCamera;
            frameCanvas.GetComponent<Canvas>().worldCamera = foundCamera;
            foundCamera.targetTexture = null;
            foundCamera.targetTexture = Global_Photo_Manager.Instance.roomPhotoRT;
        }

        // 拍照
        public void CapturePhoto()
        {
            StartCoroutine(CaptureAndShowPhoto());

            IEnumerator CaptureAndShowPhoto()
            {
                currentCamera.targetTexture = Global_Photo_Manager.Instance.roomPhotoRT;
                yield return new WaitForEndOfFrame();

                //string fileName = $"screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
                //Global_Photo_Manager.Instance.Save_RT_to_PNG(0, 0, Screen.safeArea.width, Screen.safeArea.height, "indoor", fileName);

                Global_Photo_Manager.Instance.CapturePhoto(PhotoMode.Room);

                yield return new WaitForEndOfFrame();

                currentCamera.targetTexture = null;
                Global_Photo_Manager.Instance.Try_Load_Last_Image(photoDisplay);

                if (afterPhoto != null)
                {
                    SetPhotoPanelTransparency(alphaValue);
                    afterPhoto.SetActive(true);
                    photoPanel.SetActive(true);
                }
            }
        }

        public void CloseAfterPhoto()
        {
            if (afterPhoto != null)
            {
                photoPanel.SetActive(false);
                afterPhoto.SetActive(false);
                SetPhotoPanelTransparency(0);
            }
        }

        public void SavePhoto()
        {
            Global_Photo_Manager.Instance.SaveLastPhotoToGallery();
        }

    }

    public enum PhotoCameraType
    {
        PhotoCamera0,
        PhotoCamera1,
        PhotoCamera2,
    }
}
