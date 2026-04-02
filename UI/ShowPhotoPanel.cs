using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
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


                private void Start()
                {
                    btnExit.onClick.AddListener(ClosePanel);
                    btnDeletePhoto.onClick.AddListener(BtnDeletePhoto);
                    btnConfirmDelete.onClick.AddListener(BtnConfirmDelete);
                    btnCancelDelete.onClick.AddListener(BtnCancelDelete);
                }
                private void BtnDeletePhoto()
                {
                    deletePhotoPage.SetActive(true);
                }
                private void BtnConfirmDelete()
                {
                    deletePhotoPage.SetActive(false);
                }
                private void BtnCancelDelete()
                {
                    deletePhotoPage.SetActive(false);
                }

                public override void OpenPanel(params object[] data)
                {
                    //panelObj.SetActive(true);
                    //PhotoData photoData = data[0] as PhotoData;
                    //photo.texture = PhotoManager.Instance.photos[photoData];

                    if(data.Length>0)
                    {
                        string photoName = data[0] as string;
                        if (string.IsNullOrEmpty(photoName))
                        {
                            _ = Global_Photo_Manager.Instance.Load_Image(photoName, (texture) =>
                            {
                                if (texture != null)
                                {
                                    photo.texture = texture;
                                    panelObj.SetActive(true);
                                }
                                else
                                {
                                    Log.Error($"Failed to load photo: {photoName}");
                                }
                            });
                        }
                    }
                    else
                    {
                        _ = Global_Photo_Manager.Instance.Load_Last_Image((texture) =>
                        {
                            if (texture != null)
                            {
                                photo.texture = texture;
                                panelObj.SetActive(true);
                            }
                            else
                            {
                                Log.Error("Failed to load photo.");
                            }
                        });
                    }
                   
                }

                public override void ClosePanel()
                {
                    panelObj.SetActive(false);
                }
            }
        }
    }
}

