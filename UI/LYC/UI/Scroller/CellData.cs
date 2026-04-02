using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.UI
{
    public class CellData
    {
        // 聊天格子物体的潜在类型
        public enum CellType
        {
            MyText = 1,
            OtherText = 2
        }

        // 聊天格类型
        public CellType cellType;

        // 聊天人头像
        public Sprite headIconSprite;

        // 聊天人名称
        public string speakerName;

        // 聊天内容
        public string someText;

        // 聊天提示（比如：好感度+5）
        public string chatHint;

        // 默认的聊天格子尺寸
        public float defaultCellViewHeight;

        // 实际的文字框高度
        public float textWinHeight;

        // 实际的聊天格子尺寸（聊天格子的实际高度）
        public float cellViewHeight;

        // 实际的文字安全区高度
        public float safeTextAreaHeight;
    }
}

