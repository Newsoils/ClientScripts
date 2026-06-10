using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class InventoryPanelUIHelper
{
    public static void SetSearchPanelVisible(
        GameObject searchPanel,
        TMP_Dropdown sortDropdown,
        Button searchButton,
        GameObject secondLevelMenuPanel,
        bool visible)
    {
        if (searchPanel != null) searchPanel.SetActive(visible);
        if (sortDropdown != null) sortDropdown.gameObject.SetActive(!visible);
        if (searchButton != null) searchButton.gameObject.SetActive(!visible);
        if (secondLevelMenuPanel != null) secondLevelMenuPanel.SetActive(!visible);
    }

    public static void ResetOtherLabels<TLabel>(
        IEnumerable<TLabel> labels,
        TLabel selected,
        Action<TLabel> resetLabel)
        where TLabel : class
    {
        if (labels == null || resetLabel == null) return;

        foreach (var label in labels)
        {
            if (label != null && !ReferenceEquals(label, selected))
            {
                resetLabel(label);
            }
        }
    }

    public static void ResetAllLabels<TLabel>(
        IEnumerable<TLabel> labels,
        Action<TLabel> resetLabel)
    {
        if (labels == null || resetLabel == null) return;

        foreach (var label in labels)
        {
            if (label != null)
            {
                resetLabel(label);
            }
        }
    }

    public static void UpdateSecondCategoryLabels<TLabel, TCategory>(
        IEnumerable<TLabel> labels,
        ICollection<TCategory> visibleCategories,
        TCategory noneCategory,
        Func<TLabel, TCategory> getCategory,
        Func<TLabel, GameObject> getGameObject)
    {
        if (labels == null || getCategory == null || getGameObject == null) return;

        foreach (var label in labels)
        {
            if (label == null) continue;

            var category = getCategory(label);
            bool visible = EqualityComparer<TCategory>.Default.Equals(category, noneCategory)
                           || (visibleCategories != null && visibleCategories.Contains(category));

            var go = getGameObject(label);
            if (go != null) go.SetActive(visible);
        }
    }

    public static bool ToggleInventoryArea(
        RectTransform inventoryArea,
        Button toggleButton,
        Sprite collapsedSprite,
        Sprite expandedSprite,
        bool isExpanded,
        ref Tween tween,
        float duration,
        Vector2 collapsedAnchorMax,
        Vector2 expandedAnchorMax)
    {
        if (inventoryArea == null || toggleButton == null) return isExpanded;

        Vector2 target = isExpanded ? collapsedAnchorMax : expandedAnchorMax;
        Sprite targetSprite = isExpanded ? collapsedSprite : expandedSprite;

        if (tween != null && tween.IsActive())
        {
            tween.Kill();
        }

        if (toggleButton.image != null)
        {
            toggleButton.image.sprite = targetSprite;
        }

        tween = DOTween.To(() => inventoryArea.anchorMax, x => inventoryArea.anchorMax = x, target, duration)
            .SetEase(Ease.OutCubic)
            .SetTarget(inventoryArea);

        return !isExpanded;
    }
}
