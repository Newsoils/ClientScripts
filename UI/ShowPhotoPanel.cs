using System.Collections;
using System.Collections.Generic;
using System.IO;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Kernel;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace CLIP.Project_Mouse.UI
{
    public class ShowPhotoPanel : UIPanelBase
    {
        public GameObject panelObj;
        public RawImage photo;

        public Button save;
        public Button Weixin;
        public Button QQ;

        public Button btnExit;
        public Button btnDeletePhoto;
        public GameObject deletePhotoPage;
        public Button btnConfirmDelete;
        public Button btnCancelDelete;

        // 当前查看的照片元数据
        private photo_info_saved _currentPhotoInfo;
        private string _currentPhotoPath;

        private void Start()
        {
            btnExit.onClick.AddListener(ClosePanel);
            btnDeletePhoto.onClick.AddListener(BtnDeletePhoto);
            btnConfirmDelete.onClick.AddListener(BtnConfirmDelete);
            btnCancelDelete.onClick.AddListener(BtnCancelDelete);
            save.onClick.AddListener(() => Global_Photo_Manager.Instance.SaveToGallery(_currentPhotoPath, _currentPhotoInfo._photo_name));
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            btnExit.onClick.RemoveListener(ClosePanel);
            btnDeletePhoto.onClick.RemoveListener(BtnDeletePhoto);
            btnConfirmDelete.onClick.RemoveListener(BtnConfirmDelete);
            btnCancelDelete.onClick.RemoveListener(BtnCancelDelete);
            save.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 支持传 photo_info_saved 或照片路径。新逻辑优先传元数据，路径仍保留给旧调用兼容。
        /// </summary>
        public override void OpenPanel(params object[] data)
        {
            if (data.Length > 0 && data[0] is photo_info_saved photoInfo)
            {
                _currentPhotoInfo = photoInfo;
                _currentPhotoPath = photoInfo._local_path;
            }
            else if (data.Length > 0 && data[0] is string photoPath && !string.IsNullOrEmpty(photoPath))
            {
                _currentPhotoInfo = Global_Photo_Manager.Instance.GetPhotoInfoByPath(photoPath);
                if (_currentPhotoInfo == null)
                {
                    Log.Error($"ShowPhotoPanel: 未找到照片元数据: {photoPath}");
                    return;
                }
                _currentPhotoPath = photoPath;
            }
            else
            {
                _currentPhotoPath = Global_Photo_Manager.Instance.Last_Photo_Path;
                if (!string.IsNullOrEmpty(_currentPhotoPath))
                    _currentPhotoInfo = Global_Photo_Manager.Instance.GetPhotoInfoByPath(_currentPhotoPath);
            }

            if (_currentPhotoInfo == null)
            {
                Log.Error("ShowPhotoPanel: 照片元数据为空。");
                return;
            }

            if (string.IsNullOrEmpty(_currentPhotoPath) || !File.Exists(_currentPhotoPath))
            {
                Log.Error($"ShowPhotoPanel: 照片文件不存在: {_currentPhotoPath}");
                return;
            }

            Global_Photo_Manager.Instance.LoadTexture(_currentPhotoPath, (tex) =>
            {
                if (tex != null)
                {
                    photo.texture = tex;
                    panelObj.SetActive(true);
                }
                else
                {
                    Log.Error($"ShowPhotoPanel: 纹理加载失败: {_currentPhotoPath}");
                }
            });
        }

        public override void ClosePanel()
        {
            ReleaseCurrentTexture();
            panelObj.SetActive(false);
            deletePhotoPage?.SetActive(false);
            _currentPhotoInfo = null;
            _currentPhotoPath = null;
        }

        private void ReleaseCurrentTexture()
        {
            if (!string.IsNullOrEmpty(_currentPhotoPath))
            {
                Global_Photo_Manager.Instance.ReleaseTexture(_currentPhotoPath);
                _currentPhotoPath = null;
            }
        }

        // ================================================================
        // 删除逻辑
        // ================================================================

        private void BtnDeletePhoto()
        {
            deletePhotoPage?.SetActive(true);
        }

        private void BtnConfirmDelete()
        {
            deletePhotoPage?.SetActive(false);
            if (_currentPhotoInfo == null)
            {
                Debug.LogWarning("BtnConfirmDelete: 没有当前照片信息可删除。");
                return;
            }

            // 1. 关闭预览
            ReleaseCurrentTexture();

            // 2. 删除文件 + 元数据 + 缓存
            Global_Photo_Manager.Instance.DeletePhoto(_currentPhotoInfo);
            UIManager.Instance.GetPanel<DispatchPhotoPanel>()?.RefreshAfterDelete(); // 刷新列表

            Debug.Log($"照片已删除: {_currentPhotoInfo._photo_name}");
            _currentPhotoInfo = null;
            photo.texture = null;

            // 3. 关闭面板（照片已不存在）
            ClosePanel();
        }

        private void BtnCancelDelete()
        {
            deletePhotoPage?.SetActive(false);
        }

    }
}
