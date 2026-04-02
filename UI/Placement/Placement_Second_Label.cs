using CLIP.Project_Mouse.ENUM;
using UnityEngine.UI;
using UnityEngine;

public class Placement_Second_Label : MonoBehaviour
{
    public Placement_Second_Category second_Category = Placement_Second_Category.None;
    public Sprite normal;
    public Sprite select;

    public Image icon;

    public Button button;

    private Placement_Panel placement_Panel;
    private bool isSelected = false;

    private void Start()
    {
        button.onClick.AddListener(OnBtnClick);
        placement_Panel = UIManager.Instance.GetPanel<Placement_Panel>();
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
            placement_Panel.RefreshBySecondCategory(second_Category);
            placement_Panel.Set_AllSecondLabel_SelectState(this);
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