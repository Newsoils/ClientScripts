using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using UnityEngine;
using UnityEngine.UI;
using PM_RM = CLIP.Framework_Unity.Asset.Project_Mouse_Resource_Management;
using CLIP.Project_Mouse.Game_Play_System;

using CLIP.Framework_Core.LYC.TaskSystem;

namespace CLIP.Project_Mouse.UI
{

    public class DispatchPhotoPanel : UIPanelBase
    {
        [Header("相册UI")]
        public GameObject obj;
        public RawImage image1;
        public RawImage image2;
        public Button lastPage;
        public Button nextPage;

        public Button exit_Button;

        public GameObject buttonsFatherGO;

        [Range(0, 1)] public float alphaValue = 0.5f;

        public List<Texture> textures = new List<Texture>();
        public int currentPage = 0;
        public int selectedPhotoIndex = -1;

        public string lastPhotoPath;

        // ===== 新加的 ===== 
        public RawImage showPhoto;
        public GameObject showPhotoPanel;

        private Button image1Button;
        private Button image2Button;

        private void Start()
        {
            Global_Photo_Manager.Instance.Load_Dispatch_PhotoList();

            lastPage.onClick.AddListener(LastPage);
            nextPage.onClick.AddListener(NextPage);

            image1Button = image1.GetComponent<Button>();
            image2Button = image2.GetComponent<Button>();


            image1Button?.onClick.AddListener(() => ShowAfterPhoto(image1Button));
            image2Button?.onClick.AddListener(() => ShowAfterPhoto(image2Button));

            exit_Button.onClick.AddListener(ClosePanel);
        }

        public override void OnDestroy()
        {
            lastPage.onClick.RemoveAllListeners();
            nextPage.onClick.RemoveAllListeners();

            image1Button?.onClick.RemoveAllListeners();
            image2Button?.onClick.RemoveAllListeners();

            exit_Button.onClick.RemoveAllListeners();
        }



        public override void ClosePanel()
        {
            MainPanel.SetPhoneBTNEnable(true);
            obj.SetActive(false);
            buttonsFatherGO.SetActive(false);

            // 恢复上方状态栏显示
            UIManager.Instance.GetPanel<MainPanel>().ShowAll();

            GuideManager.Instance.CheckFurnitureFinished();
        }


        // 打开相册界面
        public override void OpenPanel(params object[] data)
        {
            // 通知任务系统
            //TaskTriggers.TriggerEventOfMultipleOperations("第一次打开相册", 1);
            TaskTriggers.TriggerEventOfMultipleOperations(4, 1);

            obj.SetActive(true);

            StartCoroutine(LoadTexturesAndUpdatePage());

            buttonsFatherGO.SetActive(true);

            // 同时保证上方状态栏显示
            UIManager.Instance.GetPanel<MainPanel>().ShowTopPanelOnly();
        }

        public IEnumerator LoadTexturesAndUpdatePage()
        {
            //textures.Clear();
            //foreach (string photoPath in Dispatch_Manager._instance._dispatch_photo_path_list)
            //{
            //    if (File.Exists(photoPath))
            //    {
            //        bool isLoaded = false;
            //        _ = PM_RM.load_png_as_texture(photoPath, (texture) =>
            //        {
            //            if (texture != null)
            //            {
            //                textures.Add(texture);
            //                Debug.Log($"Loaded texture from path: {photoPath}");
            //            }
            //            else
            //            {
            //                Debug.LogError($"Failed to load texture from path: {photoPath}");
            //            }
            //            isLoaded = true;
            //        });
            //        yield return new WaitUntil(() => isLoaded);
            //    }
            //    else
            //    {
            //        Debug.LogError($"File does not exist at path: {photoPath}");
            //    }
            //}

            //Debug.Log($"Total textures loaded: {textures.Count}");
            //UpdatePage();


            textures.Clear();

            foreach (string photoPath in Dispatch_Manager._instance._dispatch_photo_path_list)
            {
                if (File.Exists(photoPath))
                {
                    // 启动异步加载，不等待
                    PM_RM.load_png_as_texture(photoPath, (texture) =>
                    {
                        if (texture != null)
                        {
                            textures.Add(texture);
                            Debug.Log($"Loaded texture from path: {photoPath}");
                        }
                        else
                        {
                            Debug.LogError($"Failed to load texture from path: {photoPath}");
                        }

                        // 每加载完一张（无论成功与否）立即更新页面
                        UpdatePage();
                    });
                }
                else
                {
                    Debug.LogError($"File does not exist at path: {photoPath}");
                }
            }

            // 立即结束协程，让加载在后台进行
            yield break;
        }

        // 显示选中的照片
        public void ShowAfterPhoto(Button button)
        {
            if (button == null || button.GetComponent<RawImage>() == null || button.GetComponent<RawImage>().texture == null)
            {
                return;
            }
            SetPanelTransparency(alphaValue);
            showPhoto.texture = button.GetComponent<RawImage>().texture;
            showPhotoPanel.SetActive(true);

            selectedPhotoIndex = textures.IndexOf(button.GetComponent<RawImage>().texture);
        }

        // 删除选中的照片
        public void DeleteSelecatedPhoto()
        {
            if (selectedPhotoIndex < 0 || selectedPhotoIndex >= textures.Count)
            {
                Debug.LogError("No photo selected or invalid index.");
                return;
            }

            string photoPath = Dispatch_Manager._instance._dispatch_photo_path_list[selectedPhotoIndex];
            if (File.Exists(photoPath))
            {
                File.Delete(photoPath);
                Debug.Log($"Deleted photo at path: {photoPath}");
            }

            Dispatch_Manager._instance._dispatch_photo_path_list.RemoveAt(selectedPhotoIndex);
            textures.RemoveAt(selectedPhotoIndex);

            Global_Photo_Manager.Instance.Save_Dispatch_PhotoList();

            selectedPhotoIndex = -1;
            showPhotoPanel.SetActive(false);

            UpdatePage();
        }

        // 关闭查看照片界面
        public void HideAfterPhoto()
        {
            SetPanelTransparency(0);
            showPhotoPanel.SetActive(false);
            selectedPhotoIndex = -1;
        }

        // 上一页
        public void LastPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdatePage();
            }
        }

        // 下一页
        public void NextPage()
        {
            if ((currentPage + 1) * 2 < textures.Count)
            {
                currentPage++;
                UpdatePage();
            }
        }

        // 更新页面显示
        private void UpdatePage()
        {
            int startIndex = currentPage * 2;

            image1.texture = startIndex < textures.Count ? textures[startIndex] : null;
            image2.texture = (startIndex + 1) < textures.Count ? textures[startIndex + 1] : null;

            lastPage.gameObject.SetActive(currentPage > 0);
            nextPage.gameObject.SetActive((currentPage + 1) * 2 < textures.Count);
        }

        // 设置面板透明度
        public void SetPanelTransparency(float alphaValue)
        {
            Image image = obj.GetComponent<Image>();
            if (image != null)
            {
                var color = image.color;
                color.a = alphaValue;
                image.color = color;
            }
            else
            {
                Debug.LogError("PhotoPanel does not have an Image component.");
            }
        }


        public void ShowLastPhoto(RawImage rawImage)
        {
            Global_Photo_Manager.Instance.try_load_image(lastPhotoPath, rawImage);
        }


        public void ForceReturn()
        {
            Dispatch_Manager._instance.force_dispatch_end();
            Global_Photo_Manager.Instance.StartCaptureDispatchPhoto();
        }



        // 返回到派遣主界面
        //public void BackToMainScene()
        //{
        //    MainPanel.OpenTopP();
        //    MainPanel.OpenMainFuncP();
        //    MainPanel.SetPhoneBTNEnable(true);

        //    SceneLoadHelper.Load_MainScene();
        //}
    }


}
