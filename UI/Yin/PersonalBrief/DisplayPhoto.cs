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
                }
            }
        }
    }
}