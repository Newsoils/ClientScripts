using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Game_Play_System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class PhotoRewardCell : MonoBehaviour
            {
                public TMP_Text photoName;
                public RawImage photo;
                public Button btnShowPhoto;
                public void Init(string photoName)
                {
                    this.photoName.text = photoName;
                    UIManager.Instance.GetPanel<DispatchPhotoPanel>().ShowLastPhoto(photo);
                    //btnShowPhoto.onClick.AddListener(ShowPhoto);
                }
                private void ShowPhoto()
                {
                    
                }
            }
        }
    }
}
