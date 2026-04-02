using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CLIP.Project_Mouse.NewFrame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class IllustrateSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
            {
                public Slider slider;
                public float currentStep = 0f;
                private float[] steps = { 0f, 0.5f, 1f };

                public float handleScaleUp = 1.5f;
                public float handleScaleNormal = 1f;
                public float scaleDuration = 0.15f;
                public RectTransform handleRect;

                void Start()
                {
                    slider.onValueChanged.AddListener(OnSliderValueChanged);
                    SnapToNearestStep(slider.value);
                }

                public void OnSliderValueChanged(float value)
                {
                    float nearest = FindNearestStep(value);

                    // 吸附到最近档位
                    if (Mathf.Abs(nearest - slider.value) > 0.001f)
                    {
                        slider.SetValueWithoutNotify(nearest);
                    }

                    // 只有切换档位时才执行
                    if (Mathf.Abs(currentStep - nearest) > 0.001f)
                    {
                        currentStep = nearest;
                        OnStepChanged(nearest);
                    }
                }

                public float FindNearestStep(float value)
                {
                    float nearest = steps[0];
                    float minDist = Mathf.Abs(value - steps[0]);
                    for (int i = 1; i < steps.Length; i++)
                    {
                        float dist = Mathf.Abs(value - steps[i]);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearest = steps[i];
                        }
                    }
                    return nearest;
                }

                public void SnapToNearestStep(float value)
                {
                    slider.value = FindNearestStep(value);
                    currentStep = slider.value;
                }

                public void OnStepChanged(float step)
                {
                    if (step == 0f)
                    {
                        //IllustrateInteractManager.Instance.ShowSmallItemGridView();
                        UIManager.Instance.GetPanel<IllustratePanel>().ShowSmallItemGridView();
                    }
                    else if (step == 0.5f)
                    {
                        //IllustrateInteractManager.Instance.ShowMediumItemGridView();
                        UIManager.Instance.GetPanel<IllustratePanel>().ShowMediumItemGridView();
                    }
                    else if (step == 1f)
                    {
                        //IllustrateInteractManager.Instance.ShowBigIllustrateItem();
                        UIManager.Instance.GetPanel<IllustratePanel>().ShowBigIllustrateItem();
                    }
                    //IllustrateInteractManager.Instance.OnChooseCategory(IllustrateInteractManager.Instance.currentCategory);
                    UIManager.Instance.GetPanel<IllustratePanel>().OnChooseCategory
                        (UIManager.Instance.GetPanel<IllustratePanel>().currentCategory);
                }

                public void OnPointerDown(PointerEventData eventData)
                {
                    if (handleRect == null) return;
                    StartCoroutine(ScaleHandle(handleRect.localScale.x, handleScaleUp));
                }

                public void OnPointerUp(PointerEventData eventData)
                {
                    StartCoroutine(ScaleHandle(handleRect.localScale.x, handleScaleNormal));
                }

                private IEnumerator ScaleHandle(float from, float to)
                {
                    float time = 0f;
                    Vector3 start = Vector3.one * from;
                    Vector3 end = Vector3.one * to;
                    while (time < scaleDuration)
                    {
                        time += Time.unscaledDeltaTime;
                        float t = time / scaleDuration;
                        handleRect.localScale = Vector3.Lerp(start, end, t);
                        yield return null;
                    }
                    handleRect.localScale = end;
                }
            }
        }
    }
}
