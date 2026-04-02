using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

namespace CLIP
{
    namespace AI_Editor_Test
    {
        public static class UICreator
        {
            [MenuItem("Tools/Create UI Canvas and Button")]
            public static void Create_UI_Canvas_And_Button()
            {
                // 创建或查找Canvas
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    // 创建Canvas对象
                    GameObject canvasObj = new GameObject("Canvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                    Selection.activeObject = canvasObj;
                }

                // 创建Button
                GameObject buttonObj = new GameObject("Button");
                buttonObj.transform.SetParent(canvas.transform);

                RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
                Button button = buttonObj.AddComponent<Button>();

                // 设置Button位置和大小
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(200, 60);

                // 创建Text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform);
                Text text = textObj.AddComponent<Text>();
                text.text = "请点击此按钮";
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.alignment = TextAnchor.MiddleCenter;

                // 设置Text位置和大小
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                Debug.Log("UI Canvas和Button创建完成");
            }
        }
    }
}
