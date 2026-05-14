using UnityEngine;
using UnityEngine.UI;

namespace CLIP.Project_Mouse.UI
{
    [ExecuteAlways]
    public class FilledSliderController : MonoBehaviour
    {
        [SerializeField] private Image mainSlider;
        [SerializeField] private Image rightSlider;

        void Update()
        {
            if (isActiveAndEnabled)
            {
                AutoAdjustRightSlider();
            }
        }



        // 自动调节右侧滑动条
        private void AutoAdjustRightSlider()
        {
            RectTransform rectTransform = mainSlider.GetComponent<RectTransform>();
            float width = rectTransform.rect.width;
            float fillAmount = mainSlider.GetComponent<Image>().fillAmount;
            //Debug.Log("width = " + width + " , fillAmount = " + fillAmount);

            Vector2 pos = rightSlider.rectTransform.anchoredPosition;
            //Debug.Log("pos = " + rightSlider.rectTransform.anchoredPosition);

            pos.x = width * fillAmount;
            //pos.y = 0;

            rightSlider.rectTransform.anchoredPosition = pos;
        }
    }
}
