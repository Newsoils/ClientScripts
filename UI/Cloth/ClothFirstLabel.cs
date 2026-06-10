using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using UnityEngine;
using UnityEngine.UI;

public class ClothFirstLabel : MonoBehaviour
{
    [Header("分类配置")]
    public Cloth_First_Category firstCategory = Cloth_First_Category.None;

    // 新增：表示该一级分类是否代表“收藏/喜爱”筛选
    public bool isFavorite = false;

    public Image icon;
    public Sprite normal;
    public Sprite Selected;

    private Button _button;

    private ClothPanel clothPanel;
    private bool isSelected = false;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnBtnClick);
        clothPanel = UIManager.Instance.GetPanel<ClothPanel>();
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
            // 传入 isFavorite 到面板
            clothPanel.RefreshByFirstCategory(firstCategory, isFavorite);
            clothPanel.Set_FirstLabel_SelectedState(this);
        }
        else
        {
            //icon.color = Color.white;
            //更新一下所有的一级标签（包括自己）
            isSelected = false;
            if (icon != null && Selected != null) icon.sprite = normal;
            // 点击取消时也传回 isFavorite，行为与 Placement_First_Label 保持一致
            clothPanel.RefreshByFirstCategory(Cloth_First_Category.None, isFavorite);
        }
    }


    public void SetSelectFalse()
    {
        //icon.color = Color.white;
        isSelected = false;
        if (icon != null && normal != null) icon.sprite = normal;
    }


}
