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

        // 用来供 content size fitter 参考的矩阵转换
        public RectTransform textRectTransform;

        // 文字缓冲
        public RectOffset textBuffer;

        // 用于显示 Text 的最大宽度
        public UnityEngine.UI.Image safeRectSizeOfText;

        // 用于标定 Text 的最大宽度
        public TMP_Text textWidth;

        private float textWinHeight;
        private float cellViewHeight;
        private float safeTextAreaHeight;


        /// <summary>
        /// 设置数据，用于更新
        /// </summary>
        /// <param name="data"></param>
        public void SetData(CellData data)
        {
            headIcon.sprite = data.headIconSprite;
            speakerName.text = data.speakerName;
            someTextText.text = data.someText;
            chatHint.text = data.chatHint;

            // 对 CellView 整体进行更新
            // 文字框 高度调整
            Vector2 newSize = someTextWinBG.GetComponent<RectTransform>().sizeDelta;
            newSize.y = data.textWinHeight;
            someTextWinBG.GetComponent<RectTransform>().sizeDelta = newSize;

            // CellView 整体背景高度调整
            newSize = BG_Image.GetComponent<RectTransform>().sizeDelta;
            newSize.y = data.cellViewHeight;
            BG_Image.GetComponent<RectTransform>().sizeDelta = newSize;

            // 文字安全区 高度调整
            newSize = safeRectSizeOfText.GetComponent<RectTransform>().sizeDelta;
            newSize.y = data.safeTextAreaHeight;
            safeRectSizeOfText.GetComponent<RectTransform>().sizeDelta = newSize;
        }

        // 在这里进行关于文字内容的计算
        private void AutoCalculateTextSize(string someText, out float cellViewHeight)
        {
            someTextText.text = someText;
            someTextText.ForceMeshUpdate();

            float textHeight = someTextText.preferredHeight;
            // 计算 文字框 高度
            textWinHeight = someTextWinBG.preferredHeight - someTextText.fontSize + textHeight;

            // 计算 CellView 高度
            this.cellViewHeight = cellViewHeight = BG_Image.preferredHeight - someTextText.fontSize + textHeight;

            // 计算 文字安全区 高度
            safeTextAreaHeight = safeRectSizeOfText.preferredHeight - someTextText.fontSize + textHeight;
        }
    }
}
