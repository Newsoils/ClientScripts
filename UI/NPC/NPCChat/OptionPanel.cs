using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    /// <summary>
    /// 动态选项按钮面板。选项按钮由 prefab 实例化，清除时统一销毁。
    /// 继承 <see cref="UIPanelBase"/> 以纳入 UI 管理体系。
    /// </summary>
    public class OptionPanel : UIPanelBase
    {
        #region Components

        [Header("组件引用")]
        [SerializeField] private ScrollRect optionScrollRect;
        [SerializeField] private OptionView optionButtonPrefab;

        public GameObject obj;

        #endregion

        #region Internal State

        private List<OptionData> _options = new List<OptionData>();

        #endregion

        #region Unity Lifecycle

        public override void Awake()
        {
            base.Awake();
            ClearOptions();
        }


        #endregion

        #region UIPanelBase

        public override void OpenPanel(params object[] datas)
        {
            obj.SetActive(true);
        }

        public override void ClosePanel()
        {
            obj.SetActive(false);
            ClearOptions();
        }

        #endregion

        #region Public API

        /// <summary>
        /// 展示一组选项按钮。点击任一选项后，先触发 handler，再调用 onDismiss。
        /// </summary>
        public void DisplayOptions(DialogueModel dialogue, List<UnityAction> buttonHandlers, UnityAction onDismiss = null)
        {
            if (optionScrollRect == null || optionScrollRect.content == null || optionButtonPrefab == null)
            {
                Debug.LogError("[OptionPanel] 必要的组件引用未赋值。");
                return;
            }

            gameObject.SetActive(true);
            ClearOptions();

            Transform content = optionScrollRect.content;
            RectTransform contentRect = content.GetComponent<RectTransform>();

            int index = 0;
            foreach (var opt in dialogue.options)
            {
                if (index >= buttonHandlers.Count) break;
                int capturedIndex = index;
                _options.Add(new OptionData(opt.content, () =>
                {
                    buttonHandlers[capturedIndex]?.Invoke();
                    StartCoroutine(DeferredDismiss(onDismiss));
                }));
                index++;
            }

            float cellWidth = contentRect.rect.width;
            var grid = content.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                var cell = grid.cellSize;
                cell.x = cellWidth;
                grid.cellSize = cell;
            }

            for (int i = 0; i < _options.Count; i++)
            {
                OptionView btn = Instantiate(optionButtonPrefab, content);
                btn.name = $"Option_{i + 1}";

                RectTransform rt = btn.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;

                btn.optionText.text = _options[i].optionText;
                btn.option.onClick.AddListener(_options[i].buttonClickHandler);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            StartCoroutine(ResetScrollPosition());
        }

        /// <summary>
        /// 销毁所有选项按钮。
        /// </summary>
        public void ClearOptions()
        {
            if (optionScrollRect == null || optionScrollRect.content == null)
                return;

            foreach (Transform child in optionScrollRect.content)
                Destroy(child.gameObject);

            _options.Clear();
        }

        #endregion

        #region Private

        private IEnumerator DeferredDismiss(UnityAction onDismiss)
        {
            yield return null;
            onDismiss?.Invoke();
            ClearOptions();
        }

        private IEnumerator ResetScrollPosition()
        {
            yield return null;
            if (optionScrollRect != null)
                optionScrollRect.verticalNormalizedPosition = 1f;
        }

        private class OptionData
        {
            public readonly string optionText;
            public readonly UnityAction buttonClickHandler;

            public OptionData(string text, UnityAction handler)
            {
                optionText = text;
                buttonClickHandler = handler;
            }
        }

        #endregion
    }
}
