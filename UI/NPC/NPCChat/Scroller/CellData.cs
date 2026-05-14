using UnityEngine;

namespace CLIP.Project_Mouse.UI
{
    public class CellData
    {
        public enum CellType
        {
            MyText = 1,
            OtherText = 2
        }

        public CellType cellType;
        public Sprite headIconSprite;
        public string speakerName;
        public string contentText;
        public string favorHint;
        public float defaultCellViewHeight;
        public float textWinHeight;
        public float cellViewHeight;
        public float safeTextAreaHeight;
    }
}
