using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.UI
{
    // 用在邮件系统中每条 EmailUnit 的自动布局
    [RequireComponent(typeof(ContentSizeFitter))]
    public class VerticalSpacer : MonoBehaviour
    {
        [Header("垂直间距")]
        [SerializeField] private float spacing = 0f;

        [Header("内边距")]
        [SerializeField] private float paddingTop = 0f;
        [SerializeField] private float paddingBottom = 0f;

        private RectTransform rectTransform;
        private ContentSizeFitter contentSizeFitter;
        private int lastChildCount = 0;
        private bool isUpdating = false;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            contentSizeFitter = GetComponent<ContentSizeFitter>();

            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        void Start()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(UpdateLayoutCoroutine());
            }
        }

        void Update()
        {
            if (transform.childCount != lastChildCount && !isUpdating && isActiveAndEnabled)
            {
                StartCoroutine(UpdateLayoutCoroutine());
            }
        }

        [ContextMenu("更新布局")]
        public void UpdateLayout()
        {
            if (!isUpdating && isActiveAndEnabled)
            {
                StartCoroutine(UpdateLayoutCoroutine());
            }
        }

        private IEnumerator UpdateLayoutCoroutine()
        {
            if (isUpdating) yield break;
            isUpdating = true;

            yield return null;

            lastChildCount = transform.childCount;

            if (lastChildCount == 0)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, paddingTop + paddingBottom);
                isUpdating = false;
                yield break;
            }

            // 获取父物体当前宽度
            float parentWidth = GetParentWidth();

            // 第一步：统一设置所有子物体的宽度
            SetAllChildrenWidth(parentWidth);

            yield return null;

            // 存储每个子物体的高度
            List<float> childHeights = new List<float>();

            // 重要：对于pivot在(0,1)的情况，起始Y坐标就是 -paddingTop
            // 因为Y轴向下为负，所以从0开始往下就要用负数
            float currentY = -paddingTop;

            // 逐个处理子物体
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = transform.GetChild(i) as RectTransform;

                if (child == null || !child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                // 确保宽度正确
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);

                // 强制刷新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);

                // 等待高度计算
                yield return null;

                // 获取子物体的最终高度
                float childHeight = child.rect.height;

                // 确保高度有效
                int maxWaitFrames = 5;
                int waitCount = 0;
                while (childHeight <= 0 && waitCount < maxWaitFrames)
                {
                    yield return null;
                    childHeight = child.rect.height;
                    waitCount++;
                }

                Debug.Log($"[子物体{i}] 高度: {childHeight}, 当前位置Y: {currentY}");

                childHeights.Add(childHeight);

                // === 关键修改：针对 pivot = (0,1) 的位置计算 ===
                // 对于pivot在左上角的物体，anchoredPosition直接表示左上角的位置
                // 所以第一个子物体的左上角应该在 (0, -paddingTop)
                // X坐标：因为是左上角锚点，所以X=0就是左对齐
                child.anchoredPosition = new Vector2(
                    0f,  // X=0 表示左对齐
                    currentY  // 直接使用currentY，不需要减去一半高度
                );

                // 更新下一个子物体的起始Y位置
                // 下一个物体的左上角 = 当前物体的左上角 - 当前物体的高度 - 间距
                currentY -= childHeight + spacing;

                yield return null;
            }

            // 计算总高度
            float totalHeight = paddingTop + paddingBottom;
            foreach (float height in childHeights)
            {
                totalHeight += height;
            }
            totalHeight += spacing * (childHeights.Count - 1);

            // 设置父容器高度
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

            yield return null;

            // 重新验证所有子物体的位置
            currentY = -paddingTop;
            int childIndex = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = transform.GetChild(i) as RectTransform;
                if (child == null || !child.gameObject.activeInHierarchy) continue;

                float childHeight = childHeights[childIndex];

                // 再次确认位置
                child.anchoredPosition = new Vector2(0f, currentY);

                Debug.Log($"最终位置 - 子物体[{i}] Y={currentY}, 高度={childHeight}");

                currentY -= childHeight + spacing;
                childIndex++;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            isUpdating = false;
            Debug.Log("布局更新完成！");
        }

        private float GetParentWidth()
        {
            float width = rectTransform.rect.width;
            if (width <= 0)
            {
                width = rectTransform.sizeDelta.x;
            }
            return Mathf.Max(width, 1f);
        }

        private void SetAllChildrenWidth(float width)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = transform.GetChild(i) as RectTransform;
                if (child != null)
                {
                    child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
                }
            }
        }

        public void AddChild(RectTransform newChild)
        {
            if (newChild == null) return;
            newChild.SetParent(transform, false);
            StartCoroutine(UpdateLayoutCoroutine());
        }

        public void AddChildren(List<RectTransform> newChildren)
        {
            if (newChildren == null || newChildren.Count == 0) return;

            StartCoroutine(AddChildrenCoroutine(newChildren));
        }

        private IEnumerator AddChildrenCoroutine(List<RectTransform> newChildren)
        {
            foreach (var child in newChildren)
            {
                if (child != null)
                {
                    child.SetParent(transform, false);
                }
            }

            yield return null;
            yield return StartCoroutine(UpdateLayoutCoroutine());
        }

        public void RemoveChild(RectTransform child)
        {
            if (child == null) return;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);

            StartCoroutine(UpdateLayoutCoroutine());
        }

        void OnValidate()
        {
            if (Application.isPlaying && gameObject.activeInHierarchy)
            {
                StopAllCoroutines();
                StartCoroutine(UpdateLayoutCoroutine());
            }
        }

        [ContextMenu("打印布局信息")]
        void PrintLayoutInfo()
        {
            Debug.Log($"=== 布局信息 ===");
            Debug.Log($"父物体: 宽={rectTransform.rect.width}, 高={rectTransform.rect.height}, Pivot={rectTransform.pivot}");

            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = transform.GetChild(i) as RectTransform;
                if (child != null && child.gameObject.activeInHierarchy)
                {
                    Debug.Log($"子物体[{i}]: pos={child.anchoredPosition}, 宽={child.rect.width}, 高={child.rect.height}, Pivot={child.pivot}");
                }
            }
        }
    }
}