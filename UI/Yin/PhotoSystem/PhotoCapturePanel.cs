using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 房间拍照面板。OpenPanel 时基于 <see cref="photoCamera"/> 模板生成一台拍照子相机挂到 Camera.main 下，
    /// 并克隆 <see cref="bgCanvas"/> 当世界空间背景板；targetTexture = roomPhotoRT 即为实时取景。ClosePanel 销毁实例。
    /// </summary>
    public class PhotoCapturePanel : UIPanelBase
    {
        [Header("拍照相机")]
        [Tooltip("拍照相机模板。建议拖 Prefab；若拖场景对象，请确保 PhotoCapturePanel 不会因场景切换导致引用变 null（本面板是 DontDestroyOnLoad）。")]
        public Camera photoCamera;

        [Tooltip("拍照相机 orthographicSize / 主相机 orthographicSize。默认 7.3/17，让拍照相机截取主相机视野中央的正方形区域。")]
        [SerializeField] private float sizeRatio = 7.3f / 17f;

        [Header("背景 Canvas")]
        [Tooltip("场景里的背景 Canvas（Screen Space - Camera 模式，绑主相机）。\n" +
                 "OpenPanel 时复制一份改成 WorldSpace，作为主相机正前方的世界空间背景板：\n" +
                 "主相机看全部、拍照相机 ortho 较小只看中央 → 拍到的就是玩家屏幕中央那一块，比例完全一致。")]
        [SerializeField] private Canvas bgCanvas;

        [Tooltip("背景板放在主相机正前方的距离（世界单位）。需大于场景里最远物体的距离，且小于主/拍照相机的 farClipPlane。")]
        [SerializeField] private float bgPlaneDistance = 130f;

        [Header("UI")]
        public RawImage photoDisplay;
        public RawImage bgImage;
        public GameObject afterPhoto;
        public GameObject photoCanvas;
        public GameObject frameCanvas;
        public GameObject photoPanel;

        private Camera photoCameraInstance;
        private Camera mainCamera;
        private Canvas bgCanvasClone;


        private void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }


        #region 接口方法实现
        public override void OpenPanel(params object[] data)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[PhotoCapturePanel] Camera.main 为空，无法启用拍照。请确认场景里有 tag=MainCamera 的相机。");
                return;
            }

            RoomSystem.Instance.canSwitchRoom = false;

            SpawnPhotoCamera();

            photoCanvas.SetActive(true);
            EvtDsp.TriggerEvt(EvtNames.OnTakePhotoPanelOpen);
        }

        public override void ClosePanel()
        {
            DestroyPhotoCamera();

            photoCanvas.SetActive(false);
            RoomSystem.Instance.canSwitchRoom = true;

            EvtDsp.TriggerEvt(EvtNames.OnTakePhotoPanelClose);
        }
        #endregion


        private void Update()
        {
            if (photoCameraInstance == null || mainCamera == null) return;

            photoCameraInstance.orthographicSize = mainCamera.orthographicSize * sizeRatio;
            SyncBgCanvasCloneSize();
        }

        /// <summary>把背景板撑成主相机当前视口的世界尺寸（ortho：高=2*orthoSize，宽=高*aspect）。</summary>
        private void SyncBgCanvasCloneSize()
        {
            if (bgCanvasClone == null || mainCamera == null) return;
            var bgRect = bgCanvasClone.GetComponent<RectTransform>();
            float h = mainCamera.orthographicSize * 2f;
            float w = h * mainCamera.aspect;
            bgRect.sizeDelta = new Vector2(w, h);
        }


        #region 拍照相机实例
        /// <summary>生成拍照子相机挂到主相机下，并克隆一份 bgCanvas 作为世界空间背景板。</summary>
        private void SpawnPhotoCamera()
        {
            if (photoCamera.gameObject.scene.IsValid() && photoCamera.gameObject.activeSelf)
            {
                photoCamera.gameObject.SetActive(false);
            }

            photoCameraInstance = Instantiate(photoCamera, mainCamera.transform);
            var t = photoCameraInstance.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;

            photoCameraInstance.targetTexture = Global_Photo_Manager.Instance.roomPhotoRT;
            photoCameraInstance.orthographicSize = mainCamera.orthographicSize * sizeRatio;
            photoCameraInstance.gameObject.SetActive(true);

            var canvas = frameCanvas.GetComponent<Canvas>();
            canvas.worldCamera = photoCameraInstance;

            bgCanvasClone = Instantiate(bgCanvas, mainCamera.transform);
            bgCanvasClone.renderMode = RenderMode.WorldSpace;
            bgCanvasClone.worldCamera = null;
            var bgRect = bgCanvasClone.GetComponent<RectTransform>();
            bgRect.localScale = Vector3.one;
            bgRect.localPosition = new Vector3(0f, 0f, bgPlaneDistance);
            bgRect.localRotation = Quaternion.identity;
            SyncBgCanvasCloneSize();
        }

        /// <summary>销毁拍照子相机和背景板克隆体。</summary>
        private void DestroyPhotoCamera()
        {
            if (bgCanvasClone != null)
            {
                Destroy(bgCanvasClone.gameObject);
                bgCanvasClone = null;
            }
            if (photoCameraInstance == null) return;
            photoCameraInstance.targetTexture = null;
            Destroy(photoCameraInstance.gameObject);
            photoCameraInstance = null;
        }
        #endregion


        #region 拍照
        public void CapturePhoto()
        {
            StartCoroutine(CaptureAndShowPhoto());
        }

        private IEnumerator CaptureAndShowPhoto()
        {
            yield return new WaitForEndOfFrame();

            PhotoRecordInfo savedPhoto = null;
            Global_Photo_Manager.Instance.CaptureAndSavePhoto(PhotoType.Room, onSaved: info => savedPhoto = info);
            yield return new WaitUntil(() => savedPhoto != null);
            yield return Global_Photo_Manager.Instance.LoadImageByPath(savedPhoto.localPath, t => photoDisplay.texture = t);

            afterPhoto.SetActive(true);
            photoPanel.SetActive(true);
        }

        public void CloseAfterPhoto()
        {
            photoPanel.SetActive(false);
            afterPhoto.SetActive(false);
        }

        public void SavePhoto()
        {
            Global_Photo_Manager.Instance.SaveLastPhotoToGallery();
        }
        #endregion
    }
}
