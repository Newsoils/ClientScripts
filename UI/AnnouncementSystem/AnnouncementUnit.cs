using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
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
            iconPath = record.anno_bg_url ?? string.Empty;
            title.text = $"{record.anno_title}\t{record.anno_date:yyyy-MM-dd}";

            if (image != null && TryParseIconPath(iconPath, out var atlasPath, out var spriteName))
            {
                Project_Mouse_Resource_Management.load_sub_sprite(atlasPath, spriteName, sprite =>
                {
                    if (image != null && sprite != null)
                        image.sprite = sprite;
                });
            }
        }

        /// <summary>资源路径格式为 atlasPath#spriteName；服务端 NoticePic 可能仅为占位数字。</summary>
        static bool TryParseIconPath(string path, out string atlasPath, out string spriteName)
        {
            atlasPath = null;
            spriteName = null;
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var parts = path.Split('#');
            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                return false;

            atlasPath = parts[0].Trim();
            spriteName = parts[1].Trim();
            return true;
        }


        public void SelectAnnouncementUnit()
        {
            redPoint.SetActive(false);
            UIManager.Instance.GetPanel<AnnouncementPanel>().SelectAnnouncement(record);
        }
    }
}