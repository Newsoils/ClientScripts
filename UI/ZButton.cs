using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZButton : Button, IPointerEnterHandler, IPointerExitHandler ,IBeginDragHandler,IDragHandler,IEndDragHandler
    ,IPointerDownHandler,IPointerUpHandler
{
    [Header("ZButton Events")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    /// <summary>
    /// 这个事件代表着完全退出这个面板，即包括子元素
    /// 普通的onHoverExit在移动到子元素的时候也会退出
    /// </summary>
    public UnityEvent onHoverExitAll;


    public UnityEvent onPointDown;
    public UnityEvent onPointUp;


    [Header("ZButton Drag Events")]
    public UnityEvent onBeginDrag;
    public UnityEvent<Vector2> onDrag;      // 传递位移 delta
    public UnityEvent onEndDrag;

    [Header("ZButton UI")]
    [SerializeField] private Image icon; // 确保有SerializeField属性
    public Image Icon => icon; // 只读暴露


    [Header("ZButton Hover Scale")]
    public bool enableHoverScale = false;   // 开关
    [SerializeField] private float scaleUp = 1.2f;            // 放大倍数
    [SerializeField] private float duration = 0.1f;           // 动画时间

    private Tween scaleTween;



    protected override void Awake()
    {
        base.Awake();

        // 确保不会获取到自身的Image组件
        if (icon == null)
        {
            // 获取所有子对象的Image（包括非激活状态）
            var childImages = GetComponentsInChildren<Image>(true)
                .Where(img => img.transform.parent == transform) // 只查找直接子物体
                .Where(img => img.gameObject != gameObject)     // 排除自身
                .ToList();

            // 优先查找名为"Icon"的对象
            icon = childImages.FirstOrDefault(img => img.name == "Icon");

            // 如果没找到，使用第一个符合条件的Image
            if (icon == null && childImages.Count > 0)
            {
                icon = childImages[0];
            }

#if UNITY_EDITOR
            if (icon == null)
            {
                //Debug.LogWarning($"{name} 未找到图标，请手动分配或添加名为'Icon'的子对象", this);
            }
#endif
        }
    }



    // 鼠标移入
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        onHoverEnter?.Invoke();


        if (enableHoverScale)
        {
            scaleTween?.Kill();
            scaleTween = transform.DOScale(scaleUp, duration);
        }
    }

    // 鼠标移出
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // 如果鼠标还在 panelArea 的 RectTransform 范围内，就不收回
        if (!RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
             eventData.position,
            eventData.pressEventCamera))
        {
            onHoverExitAll?.Invoke();
        }
        onHoverExit?.Invoke();

        if (enableHoverScale)
        {
            scaleTween?.Kill();
            scaleTween = transform.DOScale(1f, duration);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        onPointDown?.Invoke();
    }


    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        onPointUp?.Invoke();
    }

    // 拖拽相关 -------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        onBeginDrag?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        onDrag?.Invoke(eventData.delta);  // delta 是每帧的鼠标/触摸位移
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEndDrag?.Invoke();
    }


    public Tween Fade()
    {
        // 返回主 tween（比如背景图），用 Join 合并 icon 的
        Sequence seq = DOTween.Sequence();
        seq.Join(GetComponent<Image>().DOFade(0f, 0.15f).SetEase(Ease.Linear));

        if (icon != null)
        {
            seq.Join(icon.DOFade(0f, 0.15f).SetEase(Ease.Linear));
        }

        return seq;
    }

    public void Appear()
    {
        GetComponent<Image>().DOFade(1f, 0.5f).SetEase(Ease.InOutQuad);
        if(icon != null)
        {
            icon.DOFade(1f, 0.5f).SetEase(Ease.InOutQuad);
        }
    }

    public void ImAppear()
    {
        GetComponent<Image>().DOFade(1f, 0.5f).SetEase(Ease.InOutQuad);
        if (icon != null)
        {
            icon.DOFade(1f, 0.5f).SetEase(Ease.InOutQuad);
        }
    }


}