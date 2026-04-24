using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.UI
{
    [RequireComponent(typeof(GridLayoutGroup))]
    [ExecuteAlways]
    public class RewardIconSizeController : MonoBehaviour
    {
        // 初始值
        private const float ori_width = 159;
        private const float ori_height = 108;

        private const float ori_paddingTop = 12;
        private const float ori_cellSize = 60;
        private const float ori_spacing = 12;

        void Update()
        {
            if (isActiveAndEnabled)
            {
                AutoFitSize();
            }
        }

        // 自动调整奖励格子的大小适配
        private void AutoFitSize()
        {
            float curWidth = this.GetComponent<RectTransform>().rect.width;
            //Debug.Log(curWidth);
            // 根据父物体的宽度计算比率
            float ratio = curWidth / ori_width;

            GridLayoutGroup g = this.GetComponent<GridLayoutGroup>();
            g.padding.top = (int)(ori_paddingTop * ratio);
            g.cellSize = new Vector2(ori_cellSize * ratio, ori_cellSize * ratio);
            g.spacing = new Vector2(ori_spacing * ratio, ori_spacing * ratio);
        }
    }
}

