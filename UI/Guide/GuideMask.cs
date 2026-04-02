using UnityEngine;

public class GuideMask : MonoBehaviour
{
    public RectTransform target;

    public RectTransform topMask;
    public RectTransform bottomMask;
    public RectTransform leftMask;
    public RectTransform rightMask;

    public RectTransform clickArea;

    Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        //if (target == null) return;

        //Refresh();
    }

    public void Show(RectTransform t)
    {
        target = t;
        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        target = null;
        gameObject.SetActive(false);
    }

    void Refresh()
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);


        RectTransform parentRect = (RectTransform)transform.parent;


        //float screenW = 1080f;
        //float screenH = 1920f;
        float screenW = Screen.width;
        float screenH = Screen.height;

        SetRect(topMask, new Vector2(screenW / 2, (screenH + max.y) / 2),
            new Vector2(screenW, screenH - max.y));

        SetRect(bottomMask, new Vector2(screenW / 2, min.y / 2),
            new Vector2(screenW, min.y));

        SetRect(leftMask, new Vector2(min.x / 2, (min.y + max.y) / 2),
            new Vector2(min.x, max.y - min.y));

        SetRect(rightMask, new Vector2((screenW + max.x) / 2, (min.y + max.y) / 2),
            new Vector2(screenW - max.x, max.y - min.y));

        SetRect(clickArea, (min + max) / 2, max - min);


        var topPer = max.y / screenH;
        var downPer = min.y / screenH;
        var leftPer = min.x / screenW;
        var rightPer = max.x / screenW;

        topMask.anchorMin = new Vector2(0, topPer);
        topMask.anchorMax = new Vector2(1, 1);

        bottomMask.anchorMin = new Vector2(0, 0);
        bottomMask.anchorMax = new Vector2(1, downPer);

        leftMask.anchorMin = new Vector2(0, downPer);
        leftMask.anchorMax = new Vector2(leftPer, topPer);

        rightMask.anchorMin = new Vector2(rightPer, downPer);
        rightMask.anchorMax = new Vector2(1, topPer);

        SetPosition(topMask);
        SetPosition(bottomMask);
        SetPosition(leftMask);
        SetPosition(rightMask);

    }

    void SetPosition(RectTransform rt)
    {
       rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetRect(RectTransform rt, Vector2 center, Vector2 size)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            center,
            canvas.worldCamera,
            out Vector2 localPos);

        rt.localPosition = localPos;
        rt.sizeDelta = size;
    }

}
