using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class PhotoRewardCell : MonoBehaviour
    {
        public TMP_Text photoName;
        public RawImage photo;
        public Button btnShowPhoto;
        public void Init(string photoName)
        {
            this.photoName.text = photoName;
            UIManager.Instance.OpenPanel<ShowPhotoPanel>(photoName);
        }
    }

}
