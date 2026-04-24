using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.NewFrame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class AnnouncementUnit : MonoBehaviour
    {
        public Announcement_Record record;
        public string iconPath;
        public GameObject redPoint;
        public Image image;
        public TMP_Text title;

        public void InitAnnouncementUnit(Announcement_Record _record)
        {
            record = _record;
            iconPath = record.anno_bg_url;
            title.text = $"{record.anno_title}\t{record.anno_date:yyyy-MM-dd}";

            string[] parts = iconPath.Split('#');
            Project_Mouse_Resource_Management.load_sub_sprite(parts[0], parts[1], (sprite) =>
            {
                image.sprite = sprite;
            });

        }


        public void SelectAnnouncementUnit()
        {
            redPoint.SetActive(false);
            UIManager.Instance.GetPanel<AnnouncementPanel>().SelectAnnouncement(record);
        }
    }
}