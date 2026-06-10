//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using CLIP.Project_Mouse.Game_Play_System;
//using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
//using CLIP.Project_Mouse.Kernel;
//using CLIP.Project_Mouse.NewFrame.UI;
//using UnityEngine;
//using UnityEngine.UI;
//using PM_RM = CLIP.Framework_Unity.Asset.GameAssets;

//namespace CLIP
//{
//    namespace Project_Mouse
//    {
//        namespace UI
//        {
//            public class PhotoAlbum : MonoBehaviour
//            {
//                //public PersonalBriefCanvasInteractManager manager;
//                public List<string> photoPaths = new List<string>();
//                public List<Texture> textures = new List<Texture>();
//                public int currentPage = 0;
//                public RawImage image1;
//                public RawImage image2;
//                public GameObject lastPage;
//                public GameObject nextPage;

//                private int image1Index = -1;
//                private int image2Index = -1;

//                private void OnEnable()
//                {
//                    currentPage = 0;
//                    StartCoroutine(LoadTexturesAndUpdatePage());
//                }

//                public IEnumerator LoadTexturesAndUpdatePage()
//                {
//                    UIManager.Instance.OpenPanel<DispatchPhotoPanel>();

//                    yield return null;

//                    photoPaths = Dispatch_Manager.Instance.dispatchPhotoPathList;

//                    textures.Clear();
//                    foreach (string photoPath in photoPaths)
//                    {
//                        if (File.Exists(photoPath))
//                        {
//                            bool isLoaded = false;
//                            PM_RM.LoadPngAsTextureAsync(photoPath, (texture) =>
//                            {
//                                if (texture != null)
//                                {
//                                    textures.Add(texture);
//                                    Debug.Log($"Loaded texture from path: {photoPath}");
//                                }
//                                else
//                                {
//                                    Debug.LogError($"Failed to load texture from path: {photoPath}");
//                                }
//                                isLoaded = true;
//                            });
//                            yield return new WaitUntil(() => isLoaded);
//                        }
//                        else
//                        {
//                            Debug.LogError($"File does not exist at path: {photoPath}");
//                        }
//                    }

//                    Debug.Log($"Total textures loaded: {textures.Count}");
//                    UpdatePage();
//                }

//                public void SelectPhoto(RawImage rawImage)
//                {
//                    if (rawImage.texture == null) return;
//                    //manager.imageToChange.texture = rawImage.texture;
//                    UIManager.Instance.GetPanel<PersonalBriefPanel>().imageToChange.texture = rawImage.texture;

//                    int selectedIndex = -1;
//                    if (rawImage == image1)
//                        selectedIndex = image1Index;
//                    else if (rawImage == image2)
//                        selectedIndex = image2Index;

//                    if (selectedIndex >= 0 && selectedIndex < photoPaths.Count)
//                    {
//                        string url = photoPaths[selectedIndex];
//                        var infoList = Global_Photo_Manager.Instance.imageList;
//                        PhotoRecordInfo info = infoList.Find(p => p.localPath == url);
//                        if (info != null)
//                        {
//                            //manager.photoInfoList[manager.imageToChangeIndex] = info;
//                            UIManager.Instance.GetPanel<PersonalBriefPanel>().
//                                photoInfoList[UIManager.Instance.GetPanel<PersonalBriefPanel>().imageToChangeIndex] = info;
//                        }
//                        else
//                        {
//                            Debug.LogWarning($"未找到对应的 PhotoRecordInfo, url: {url}");
//                        }
//                    }

//                    this.gameObject.SetActive(false);
//                }

//                // 上一�?
//                public void LastPage()
//                {
//                    if (currentPage > 0)
//                    {
//                        currentPage--;
//                        UpdatePage();
//                    }
//                }

//                // 下一�?
//                public void NextPage()
//                {
//                    if ((currentPage + 1) * 2 < textures.Count)
//                    {
//                        currentPage++;
//                        UpdatePage();
//                    }
//                }

//                // 更新页面显示
//                private void UpdatePage()
//                {
//                    int startIndex = currentPage * 2;

//                    image1Index = startIndex < textures.Count ? startIndex : -1;
//                    image2Index = (startIndex + 1) < textures.Count ? (startIndex + 1) : -1;

//                    image1.texture = image1Index != -1 ? textures[image1Index] : null;
//                    image2.texture = image2Index != -1 ? textures[image2Index] : null;

//                    lastPage.SetActive(currentPage > 0);
//                    nextPage.SetActive((currentPage + 1) * 2 < textures.Count);
//                }
//            }
//        }
//    }
//}