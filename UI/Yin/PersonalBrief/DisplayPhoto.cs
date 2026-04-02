using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.NewFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class DisplayPhoto : MonoBehaviour
            {
                //public PersonalBriefCanvasInteractManager panel;
                //public PersonalBriefPanel panel;
                public RawImage rawImage;
                public int index;

                public void ChangeDisplayPhoto()
                {
                    //panel.imageToChange = rawImage;
                    //panel.imageToChangeIndex = index;
                    //panel.photoAlbum.SetActive(true);
                    UIManager.Instance.GetPanel<PersonalBriefPanel>().imageToChange = rawImage;
                    UIManager.Instance.GetPanel<PersonalBriefPanel>().imageToChangeIndex = index;
                    UIManager.Instance.GetPanel<PersonalBriefPanel>().photoAlbum.SetActive(true);
                }
            }
        }
    }
}