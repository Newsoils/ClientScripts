using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 派遣相册面板。
    /// 照片纹理统一由 Global_Photo_Manager 托管（引用计数、LRU 淘汰、按需加载/卸载）。
    /// 当前页面仅加载该页需要的 2 张缩略图。
    /// </summary>
    public class DispatchPhotoPanel : UIPanelBase
    {
        [Header("相册UI")]
        public GameObject obj;
        public RawImage image1;
        public RawImage image2;
        public Button lastPage;
        public Button nextPage;
        public Button exit_Button;

        public int currentPage = 0;

        private Button _image1Button;
        private Button _image2Button;

        // 当前页面展示的路径列表（始终只有当前页的两条）
        private string[] _displayedPaths = new string[2];
        private string _viewingPath;
        private int _viewingIndex = -1;
        // 记录 ShowPhotoPanel 删除时对应的缩略图路径（用于 RefreshAfterDelete 正确释放纹理引用）
        private string _deletedThumbnailPath;

        // 获取当前所有派遣照片（从 _local_image_list 过滤）
        private List<photo_info_saved> _dispatchPhotos => Global_Photo_Manager.Instance.GetDispatchPhotos();
        private int TotalCount => _dispatchPhotos.Count;

        private void Start()
        {
            _image1Button = image1.GetComponent<Button>();
            _image2Button = image2.GetComponent<Button>();

            lastPage.onClick.AddListener(LastPage);
            nextPage.onClick.AddListener(NextPage);
            _image1Button?.onClick.AddListener(() => OnThumbnailClicked(0));
            _image2Button?.onClick.AddListener(() => OnThumbnailClicked(1));
            exit_Button.onClick.AddListener(ClosePanel);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            lastPage.onClick.RemoveAllListeners();
            nextPage.onClick.RemoveAllListeners();
            _image1Button?.onClick.RemoveAllListeners();
            _image2Button?.onClick.RemoveAllListeners();
            exit_Button.onClick.RemoveAllListeners();
            ReleaseCurrentPageTextures();
            ReleaseViewingTexture();
        }

        public override void OpenPanel(params object[] data)
        {
            TaskEvent.Trigger(TaskEvent.Task_OpenPhotoAlbum);
            obj.SetActive(true);
            UIManager.Instance.GetPanel<MainPanel>().ShowTopPanelOnly();

            currentPage = 0;
            LoadCurrentPage();
        }

        public override void ClosePanel()
        {
            MainPanel.SetPhoneBTNEnable(true);
            obj.SetActive(false);
            UIManager.Instance.GetPanel<MainPanel>().ShowAll();
            GuideManager.Instance.CheckFurnitureFinished();

            ReleaseCurrentPageTextures();
            ReleaseViewingTexture();
            _viewingPath = null;
            _viewingIndex = -1;
        }

        // ================================================================
        // 翻页
        // ================================================================

        public void LastPage()
        {
            if (currentPage > 0)
            {
                ReleaseCurrentPageTextures();
                currentPage--;
                LoadCurrentPage();
            }
        }

        public void NextPage()
        {
            if ((currentPage + 1) * 2 < TotalCount)
            {
                ReleaseCurrentPageTextures();
                currentPage++;
                LoadCurrentPage();
            }
        }

        private void LoadCurrentPage()
        {
            int start = currentPage * 2;
            _displayedPaths[0] = start < TotalCount ? _dispatchPhotos[start]._local_path : null;
            _displayedPaths[1] = (start + 1) < TotalCount ? _dispatchPhotos[start + 1]._local_path : null;

            LoadThumbnail(0, _displayedPaths[0]);
            LoadThumbnail(1, _displayedPaths[1]);

            UpdatePageButtons();
        }

        private void LoadThumbnail(int slot, string path)
        {
            RawImage target = slot == 0 ? image1 : image2;
            if (string.IsNullOrEmpty(path))
            {
                target.texture = null;
                return;
            }

            Global_Photo_Manager.Instance.LoadTexture(path, (tex) =>
            {
                if (tex != null) target.texture = tex;
            });
        }

        private void ReleaseCurrentPageTextures()
        {
            foreach (var p in _displayedPaths)
            {
                if (!string.IsNullOrEmpty(p) && p != _viewingPath)
                    Global_Photo_Manager.Instance.ReleaseTexture(p);
            }
            _displayedPaths[0] = null;
            _displayedPaths[1] = null;
        }

        // ================================================================
        // 大图预览
        // ================================================================

        private void OnThumbnailClicked(int slot)
        {
            string path = _displayedPaths[slot];
            if (string.IsNullOrEmpty(path)) return;

            _viewingPath = path;
            _viewingIndex = currentPage * 2 + slot;
            _deletedThumbnailPath = path; // 记录缩略图路径，删除时需要用到

            //string photoName = _dispatchPhotos[slot]?._photo_name;
            //if (string.IsNullOrEmpty(photoName)) return;

            UIManager.Instance.GetPanel<ShowPhotoPanel>().OpenPanel(path);
        }

        private void ReleaseViewingTexture()
        {
            if (!string.IsNullOrEmpty(_viewingPath))
            {
                Global_Photo_Manager.Instance.ReleaseTexture(_viewingPath);
                _viewingPath = null;
            }
        }


        public void RefreshAfterDelete()
        {
            // 1. 释放被删照片在缩略图槽的引用（如果它恰好还在当前页）
            string deletedPath = _deletedThumbnailPath;
            _deletedThumbnailPath = null;

            // 从 _displayedPaths 里找被删的那张并立即清槽
            for (int i = 0; i < _displayedPaths.Length; i++)
            {
                if (_displayedPaths[i] == deletedPath)
                {
                    RawImage target = i == 0 ? image1 : image2;
                    target.texture = null;
                    _displayedPaths[i] = null;
                    break;
                }
            }

            // 2. 清理预览状态
            _viewingPath = null;
            _viewingIndex = -1;

            // 3. 如果当前页为空且不是第一页，回退一页
            if (TotalCount > 0 && currentPage * 2 >= TotalCount && currentPage > 0)
                currentPage--;

            // 4. 重新加载当前页（空白槽会正确补上新照片或保持 null）
            LoadCurrentPage();
        }

        // ================================================================
        // UI 状态
        // ================================================================

        private void UpdatePageButtons()
        {
            int count = TotalCount;
            lastPage.gameObject.SetActive(currentPage > 0);
            nextPage.gameObject.SetActive((currentPage + 1) * 2 < count);
        }

        
    }
}
