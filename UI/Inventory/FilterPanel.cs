using System.Collections.Generic;
using System;
using CLIP.Project_Mouse.ENUM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterPanel : UIPanelBase
{
    public GameObject obj;
    public Transform bg;
    public ZButton option;
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.7f, 0.9f, 1f, 1f);

    private PlantPanel plantPanel;
    private readonly List<ZButton> createdOptions = new List<ZButton>();

    private void Start()
    {
        plantPanel = UIManager.Instance.GetPanel<PlantPanel>();
        if (option != null)
        {
            normalColor = option.image != null ? option.image.color : normalColor;
            option.gameObject.SetActive(false);
        }
        ClosePanel();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        ClearOptions();
    }

    public override void ClosePanel()
    {
        if (obj != null) obj.SetActive(false);
    }

    public override void OpenPanel(params object[] data)
    {
        if (plantPanel == null)
        {
            plantPanel = UIManager.Instance.GetPanel<PlantPanel>();
        }

        if (obj != null) obj.SetActive(true);
        BuildOptions();
    }

    private void BuildOptions()
    {
        ClearOptions();
        if (bg == null || option == null || plantPanel == null) return;

        foreach (PlantType plantType in Enum.GetValues(typeof(PlantType)))
        {
            if (plantType == PlantType.None) continue;
            CreateOption(
                "属性：" + GetPlantTypeName(plantType),
                plantPanel.CurrentSeedPlantTypeFilter == plantType,
                () =>
                {
                    var next = plantPanel.CurrentSeedPlantTypeFilter == plantType ? PlantType.None : plantType;
                    plantPanel.SetSeedPlantTypeFilter(next);
                    BuildOptions();
                });
        }

        foreach (ObtainSource obtainSource in Enum.GetValues(typeof(ObtainSource)))
        {
            if (obtainSource == ObtainSource.Unknown) continue;
            CreateOption(
                "获取：" + GetObtainSourceName(obtainSource),
                plantPanel.CurrentSeedObtainSourceFilter == obtainSource,
                () =>
                {
                    var next = plantPanel.CurrentSeedObtainSourceFilter == obtainSource ? ObtainSource.Unknown : obtainSource;
                    plantPanel.SetSeedObtainSourceFilter(next);
                    BuildOptions();
                });
        }
    }

    private void CreateOption(string text, bool selected, UnityEngine.Events.UnityAction onClick)
    {
        var button = Instantiate(option, bg);
        button.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);

        SetOptionText(button, text);
        SetOptionSelected(button, selected);
        createdOptions.Add(button);
    }

    private void ClearOptions()
    {
        for (int i = 0; i < createdOptions.Count; i++)
        {
            if (createdOptions[i] != null)
            {
                createdOptions[i].onClick.RemoveAllListeners();
                Destroy(createdOptions[i].gameObject);
            }
        }
        createdOptions.Clear();
    }

    private void SetOptionText(ZButton button, string text)
    {
        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }

        var legacyText = button.GetComponentInChildren<Text>(true);
        if (legacyText != null)
        {
            legacyText.text = text;
        }
    }

    private void SetOptionSelected(ZButton button, bool selected)
    {
        if (button.image != null)
        {
            button.image.color = selected ? selectedColor : normalColor;
        }
    }

    private static string GetPlantTypeName(PlantType plantType)
    {
        switch (plantType)
        {
            case PlantType.Flower: return "花卉";
            case PlantType.Fruit: return "果实";
            case PlantType.Greenery: return "绿植";
            default: return plantType.ToString();
        }
    }

    private static string GetObtainSourceName(ObtainSource obtainSource)
    {
        switch (obtainSource)
        {
            case ObtainSource.ShopPurchase: return "商店购买";
            case ObtainSource.EventReward: return "活动获得";
            case ObtainSource.FriendGift: return "伙伴赠送";
            case ObtainSource.DispatchReward: return "派遣获得";
            default: return obtainSource.ToString();
        }
    }
}
