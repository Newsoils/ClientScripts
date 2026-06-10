using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using UnityEngine;
using UnityEngine.UI;

public class PlantFirstLabel : MonoBehaviour
{
    [Header("分类配置")]
    public Plant_First_Category firstCategory = Plant_First_Category.None;

    // 新增：是否作为“收藏”筛选项
    public bool isFavorite = false;

    public Image icon;
    public Sprite normal;
    public Sprite Selected;

    private Button _button;

    private PlantPanel plantPanel;
    private bool isSelected = false;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnBtnClick);
        plantPanel = UIManager.Instance.GetPanel<PlantPanel>();
    }

    public void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    private void OnBtnClick()
    {
        if (!isSelected)
        {
            isSelected = true;
            if (icon != null && Selected != null) icon.sprite = Selected;
            //icon.color = Color.blue;
            plantPanel.RefreshByFirstCategory(firstCategory, isFavorite);
            plantPanel.Set_FirstLabel_SelectedState(this);
        }
        else
        {
            //icon.color = Color.white;
            //更新一下所有的一级标签（包括自己）
            isSelected = false;
            if (icon != null && Selected != null) icon.sprite = normal;
            plantPanel.RefreshByFirstCategory(Plant_First_Category.None, isFavorite);
        }
    }


    public void SetSelectFalse()
    {
        //icon.color = Color.white;
        isSelected = false;
        if (icon != null && normal != null) icon.sprite = normal;
    }


}
