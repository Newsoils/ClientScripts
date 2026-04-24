using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.UI
{
    public class CellView : EnhancedScrollerCellView
    {
        // CellView 背景图片
        public Image BG_Image;

        // 聊天人头像图片
        public Image headIcon;

        // 聊天人名称
        public TMP_Text speakerName;

        // 聊天内容背景图片（文字背景）
        public Image someTextWinBG;

        // 聊天内容
        public TMP_Text someTextText;

        // 聊天提示（比如：好感度+5）
        public TMP_Text chatHint;

        // 保留字段：旧的"可见最大宽度"辅助节点；现在尺寸交给 prefab 自带的
        // VerticalLayoutGroup + ContentSizeFitter 驱动，CellView 脚本不再主动写入。
        public UnityEngine.UI.Image safeRectSizeOfText;

        // 保留字段：旧的"最大宽度"测量文本；同上，不再由脚本写入。
        public TMP_Text textWidth;

        [Header("气泡宽度")]
        [Tooltip("文字气泡最大宽度（含 padding，单位像素）。实际显示宽度 = Min(文本自然宽度, 本值)；超过则文字自动换行。")]
        public float maxBubbleWidth = 465f;


        /// <summary>
        /// 设置数据，用于更新。
        ///
        /// 高度由 prefab 上的 <c>VerticalLayoutGroup + ContentSizeFitter</c> 驱动；
        /// 宽度由 <see cref="AdjustBubbleWidth"/> 根据文字内容动态写入气泡 RectTransform，
        /// 受 <see cref="maxBubbleWidth"/> 封顶，超过则文字自动换行。
        /// 最后强制刷一次 Layout，保证 Scroller 复用实例时尺寸立刻反映新内容。
        /// </summary>
        public void SetData(CellData data)
        {
            headIcon.sprite = data.headIconSprite;
            speakerName.text = data.speakerName;
            someTextText.text = data.someText;
            chatHint.text = data.chatHint;

            AdjustBubbleWidth();
            RebuildLayout();
        }

        /// <summary>
        /// 根据当前 <see cref="someTextText"/> 的自然文本宽度，设置气泡（<c>someText</c> 的父节点）
        /// RectTransform 的宽度；超过 <see cref="maxBubbleWidth"/> 时钳制到最大值，
        /// 此时 VerticalLayoutGroup 会把子 TMP 宽度约束到 `max - padding`，触发自动换行。
        /// </summary>
        private void AdjustBubbleWidth()
        {
            if (someTextText == null) return;
            var bubble = someTextText.transform.parent as RectTransform;
            if (bubble == null) return;

            // 横向 padding 从气泡节点的 VLG 读，保证左右边距与美术一致。
            var vlg = bubble.GetComponent<VerticalLayoutGroup>();
            float padH = vlg != null ? (vlg.padding.left + vlg.padding.right) : 0f;

            // 确保自动换行开着，并让 TMP 先计算 preferredWidth（单行自然宽度）。
            someTextText.enableWordWrapping = true;
            someTextText.ForceMeshUpdate();
            float natural = someTextText.preferredWidth;

            float maxInner = Mathf.Max(1f, maxBubbleWidth - padH);
            float targetW = Mathf.Min(natural, maxInner) + padH;

            var sd = bubble.sizeDelta;
            bubble.sizeDelta = new Vector2(targetW, sd.y);
        }

        /// <summary>
        /// 强制刷新当前 CellView 下所有 LayoutGroup / ContentSizeFitter，
        /// 保证 <see cref="RectTransform.sizeDelta"/> 立即反映新内容而非等下一帧。
        /// </summary>
        private void RebuildLayout()
        {
            // 先让 TMP 的 mesh 更新一次 preferredHeight，不然 LayoutRebuilder 读到的还是旧值。
            if (speakerName != null) speakerName.ForceMeshUpdate();
            if (someTextText != null) someTextText.ForceMeshUpdate();
            if (chatHint != null) chatHint.ForceMeshUpdate();

            var rt = transform as RectTransform;
            if (rt != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

    }
}
