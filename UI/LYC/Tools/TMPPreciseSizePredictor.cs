using TMPro;
using UnityEngine;

namespace CLIP.Project_Mouse.LYC.Tools
{
    public class TMPTextSizePredictor
    {
        public static Vector2 GetExactTextSize(
            string text,
            TMP_FontAsset fontAsset,
            float fontSize,
            Vector2 rectContainerSize, // 包含RectTransform尺寸
            bool enableWordWrapping = true,
            TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft,
            float lineSpacing = 5f,
            float characterSpacing = 0f,
            float wordSpacing = 0f,
            bool richText = true)
        {
            // 1. 创建临时对象和组件
            GameObject tempObj = new GameObject("TMP_SizePredictor");
            RectTransform rectTransform = tempObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = rectContainerSize;

            TMP_Text tmpText = tempObj.AddComponent<TextMeshProUGUI>();

            // 2. 按顺序应用所有样式设置（这是关键）
            tmpText.font = fontAsset;
            tmpText.fontSize = fontSize;
            tmpText.text = text; // 设置文本应在字体设置之后

            tmpText.alignment = alignment;
            tmpText.enableWordWrapping = enableWordWrapping;
            tmpText.overflowMode = TextOverflowModes.Overflow; // 重要：确保不截断

            tmpText.lineSpacing = lineSpacing;
            tmpText.characterSpacing = characterSpacing;
            tmpText.wordSpacing = wordSpacing;
            tmpText.richText = richText;

            // 3. 强制更新网格以进行计算
            tmpText.ForceMeshUpdate(); // 更新布局
                                       // 如果需要，可以调用第二次以确保高度计算稳定
                                       // tmpText.ForceMeshUpdate(); 

            // 4. 获取精确尺寸
            Vector2 size = new Vector2(tmpText.preferredHeight, tmpText.preferredHeight);

            // 5. 清理
            Object.DestroyImmediate(tempObj);

            return size;
        }
    }
}
