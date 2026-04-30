using CLIP.Project_Mouse.ENUM;
using UnityEngine;
using UnityEngine.UI;

public class Placement_First_Label : MonoBehaviour
{
    [Header("分类配置")]
    public Placement_First_Category firstCategory = Placement_First_Category.None;

    public bool isFavorite = false;
    public Image icon;
    public Sprite normal;
    public Sprite Selected;

    private Button _button;

    private Placement_Panel placement_Panel;
    private bool isSelected = false;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnBtnClick);
        placement_Panel = UIManager.Instance.GetPanel<Placement_Panel>();
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
            if(icon!=null&& Selected!=null) icon.sprite = Selected;
            //icon.color = Color.blue;
            placement_Panel.RefreshByFirstCategory(firstCategory,isFavorite);
            placement_Panel.Set_FirstLabel_SelectedState(this);
        }
        else
        {
            //icon.color = Color.white;
            //更新一下所有的一级标签（包括自己）
            isSelected = false;
            if (icon != null && Selected != null) icon.sprite = normal;
            placement_Panel.RefreshByFirstCategory(Placement_First_Category.None, isFavorite);
        }
    }


    public void SetSelectFalse()
    {
        //icon.color = Color.white;
        isSelected = false;
        if(icon!=null&&normal !=null) icon.sprite = normal;
    }


}
