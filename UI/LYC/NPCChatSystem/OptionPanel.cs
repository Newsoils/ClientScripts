using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.LYC.DialogueSystem
{
    // NPCPanel 的子面板，由 NPCPanel 统一进行保护
    public class OptionPanel : MonoBehaviour
    {
        // 滑动窗口
        [SerializeField] private ScrollRect optionScrollRect;

        // 选项预制体
        [SerializeField] private OptionView optionButtonPrefab;

        private int optionCount = -1;
        private List<OptionData> options = null;

        private void Awake()
        {
            optionCount = -1;
        }

        public void DisplayOptions(OptionDialogueModel optionDialogue, List<UnityAction> buttonClickHandlers)
        {
            SetOptionDatas(optionDialogue, buttonClickHandlers);

            if (optionScrollRect == null || optionScrollRect.content == null || optionButtonPrefab == null)
            {
                Debug.LogError("ScrollRect, Content 或 Prefab 未赋值！");
                return;
            }

            Transform content = optionScrollRect.content;
            RectTransform contentRect = content.GetComponent<RectTransform>();

            // 清空现有内容
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            if (optionCount == -1)
            {
                Debug.LogError("请检查是否调用了 SetOptions() 方法设置数据");
                return;
            }

            // 生成选项
            for (int i = 0; i < optionCount; i++)
            {
                OptionView newOption = Instantiate(optionButtonPrefab, content);
                newOption.name = $"Option_{i + 1}";
                Debug.LogWarning($"已生成 Option_{i + 1}");

                // 动态调整按钮宽度
                Vector2 tmp = new Vector2(content.GetComponent<RectTransform>().rect.width, 200);
                content.GetComponent<GridLayoutGroup>().cellSize = tmp;

                // 重置按钮布局属性，交给GridLayoutGroup管理
                RectTransform optionRect = newOption.GetComponent<RectTransform>();
                optionRect.anchorMin = Vector2.zero;
                optionRect.anchorMax = Vector2.one;
                optionRect.pivot = new Vector2(0.5f, 0.5f);
                optionRect.anchoredPosition = Vector2.zero;
                optionRect.sizeDelta = Vector2.zero;

                // 设置按钮文本和点击事件
                newOption.optionText.text = options[i].optionText;
                newOption.option.onClick.AddListener(options[i].buttonClickHandler);
                newOption.option.onClick.AddListener(ClearOptions);
            }

            // 强制重建布局，让GridLayoutGroup和ContentSizeFitter生效
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            // 延迟重置滚动位置
            StartCoroutine(ResetScrollPositionAfterLayout());
        }

        // 新增协程方法
        private IEnumerator ResetScrollPositionAfterLayout()
        {
            yield return null;
            optionScrollRect.verticalNormalizedPosition = 1f;
        }

        public void ClearOptions()
        {
            Transform content = optionScrollRect.content;

            // 清空现有内容
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }

        private void SetOptionDatas(OptionDialogueModel optionDialogue, List<UnityAction> buttonClickHandlers)
        {
            optionCount = optionDialogue.Options.Count;
            options = new List<OptionData>();
            int i = 0;
            foreach (var op in optionDialogue.Options)
            {
                options.Add(new OptionData(op.Key, buttonClickHandlers[i]));
                i++;
            }
        }

        private class OptionData
        {
            public string optionText;
            public UnityAction buttonClickHandler;

            public OptionData(string optionText, UnityAction buttonClickHandler)
            {
                this.optionText = optionText;
                this.buttonClickHandler = buttonClickHandler;
            }
        }
    }

}
