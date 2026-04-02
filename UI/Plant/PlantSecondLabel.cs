using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using UnityEngine;
using UnityEngine.UI;

public class PlantSecondLabel : MonoBehaviour
{
    public Plant_Second_Category second_Category = Plant_Second_Category.None;
    public Sprite normal;
    public Sprite select;

    public Image icon;

    public Button button;

    private PlantPanel plantPanel;
    private bool isSelected = false;

    private void Start()
    {
        button.onClick.AddListener(OnBtnClick);
        plantPanel = UIManager.Instance.GetPanel<PlantPanel>();
    }

    public void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    private void OnBtnClick()
    {
        if (!isSelected)
        {
            isSelected = true;
            button.image.sprite = select;
            plantPanel.RefreshBySecondCategory(second_Category);
            plantPanel.Set_AllSecondLabel_SelectState(this);
        }
        else
        {
            isSelected = false;
            button.image.sprite = normal;
        }
    }

    public void SetSelectFalse()
    {
        isSelected = false;
        //icon.color = Color.white;
        button.image.sprite = normal;
    }


}
