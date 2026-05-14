using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    public class CellView : EnhancedScrollerCellView
    {
        [Header("气泡节点")]
        public Image bubbleBackground;

        [Header("头像")]
        public Image headIcon;

        [Header("说话人名称")]
        public TMP_Text speakerName;

        [Header("文字气泡")]
        public Image textBubbleBg;
        public TMP_Text contentText;
        public TMP_Text favorHint;

        [Header("布局参数")]
        [Tooltip("文字气泡最大宽度（px），超过则自动换行。")]
        public float maxBubbleWidth = 465f;

        public void SetData(CellData data)
        {
            headIcon.sprite = data.headIconSprite;
            speakerName.text = data.speakerName;
            contentText.text = data.contentText;
            favorHint.text = data.favorHint;

            AdjustBubbleWidth();
            RebuildLayout();
        }

        /// <summary>
        /// 根据文字内容动态设置气泡宽度：单行宽度不超过 maxBubbleWidth，
        /// 超过时由 VerticalLayoutGroup 触发自动换行。
        /// </summary>
        private void AdjustBubbleWidth()
        {
            if (contentText == null) return;

            var bubble = contentText.transform.parent as RectTransform;
            if (bubble == null) return;

            var vlg = bubble.GetComponent<VerticalLayoutGroup>();
            float padH = vlg != null ? vlg.padding.left + vlg.padding.right : 0f;

            contentText.enableWordWrapping = true;
            contentText.ForceMeshUpdate();
            float naturalWidth = contentText.preferredWidth;

            float maxInner = Mathf.Max(1f, maxBubbleWidth - padH);
            float targetWidth = Mathf.Min(naturalWidth, maxInner) + padH;

            var size = bubble.sizeDelta;
            bubble.sizeDelta = new Vector2(targetWidth, size.y);
        }

        /// <summary>
        /// 强制刷新 Layout，使 RectTransform 立即反映新内容。
        /// </summary>
        private void RebuildLayout()
        {
            if (speakerName != null) speakerName.ForceMeshUpdate();
            if (contentText != null) contentText.ForceMeshUpdate();
            if (favorHint != null) favorHint.ForceMeshUpdate();

            var rt = transform as RectTransform;
            if (rt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
    }
}
