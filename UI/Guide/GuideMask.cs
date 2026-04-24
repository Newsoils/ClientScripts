using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GuideMask : MaskableGraphic, ICanvasRaycastFilter
{
    private RectTransform _target;
    private Vector2 _targetMin;
    private Vector2 _targetMax;
    public RectTransform _targetArea;

    //// 暴露空洞点击事件
    //public UnityEvent OnHoleClicked;


    void LateUpdate()
    {
        RefreshView();
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return !RectTransformUtility.RectangleContainsScreenPoint(_targetArea, sp, eventCamera);
    }

    public void Show(RectTransform t)
    {
        _targetArea.gameObject.SetActive(true);
        Play(t);
    }

    public void Hide()
    {
        _targetArea.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }


    private void Play(RectTransform target)
    {
        gameObject.SetActive(true);

        var screenPoint = RectTransformUtility.WorldToScreenPoint(null, target.position);

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null,
            out localPoint))
        {
            Hide();
            return;
        }

        _targetArea.anchorMax = target.anchorMax;
        _targetArea.anchorMin = target.anchorMin;
        _targetArea.anchoredPosition = target.anchoredPosition;
        _targetArea.anchoredPosition3D = target.anchoredPosition3D;
        _targetArea.offsetMax = target.offsetMax;
        _targetArea.offsetMin = target.offsetMin;
        _targetArea.pivot = target.pivot;
        _targetArea.sizeDelta = target.sizeDelta;
        _targetArea.localPosition = localPoint;

        _targetArea.ForceUpdateRectTransforms();
        _target = _targetArea;
        _target.ForceUpdateRectTransforms();
        RefreshView();
    }


    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();

        var maskRect = rectTransform.rect;

        var maskRectLeftTop = new Vector2(-maskRect.width / 2, maskRect.height / 2);
        var maskRectLeftBottom = new Vector2(-maskRect.width / 2, -maskRect.height / 2);
        var maskRectRightTop = new Vector2(maskRect.width / 2, maskRect.height / 2);
        var maskRectRightBottom = new Vector2(maskRect.width / 2, -maskRect.height / 2);

        var targetRectLeftTop = new Vector2(_targetMin.x, _targetMax.y);
        var targetRectLeftBottom = _targetMin;
        var targetRectRightTop = _targetMax;
        var targetRectRightBottom = new Vector2(_targetMax.x, _targetMin.y);

        toFill.AddVert(maskRectLeftBottom, color, Vector2.zero);
        toFill.AddVert(targetRectLeftBottom, color, Vector2.zero);
        toFill.AddVert(targetRectRightBottom, color, Vector2.zero);
        toFill.AddVert(maskRectRightBottom, color, Vector2.zero);
        toFill.AddVert(targetRectRightTop, color, Vector2.zero);
        toFill.AddVert(maskRectRightTop, color, Vector2.zero);
        toFill.AddVert(targetRectLeftTop, color, Vector2.zero);
        toFill.AddVert(maskRectLeftTop, color, Vector2.zero);

        toFill.AddTriangle(0, 1, 2);
        toFill.AddTriangle(2, 3, 0);
        toFill.AddTriangle(3, 2, 4);
        toFill.AddTriangle(4, 5, 3);
        toFill.AddTriangle(6, 7, 5);
        toFill.AddTriangle(5, 4, 6);
        toFill.AddTriangle(7, 6, 1);
        toFill.AddTriangle(1, 0, 7);
    }


    private void RefreshView()
    {
        Vector2 newMin;
        Vector2 newMax;
        if (_target != null && _target.gameObject.activeSelf)
        {
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(transform, _target);
            newMin = bounds.min;
            newMax = bounds.max;
        }
        else
        {
            newMin = Vector2.zero;
            newMax = Vector2.zero;
        }
        if (_targetMin != newMin || _targetMax != newMax)
        {
            _targetMin = newMin;
            _targetMax = newMax;
            SetAllDirty();
        }
    }

}
