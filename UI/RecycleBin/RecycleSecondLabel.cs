using CLIP.Project_Mouse.ENUM;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 回收箱二级分类 Tab，用 <see cref="Recycle_Second_Category"/>；必须挂在 <see cref="RecyclePanel"/> 子树下。
/// </summary>
public class RecycleSecondLabel : MonoBehaviour
{
    public Recycle_Second_Category second_Category = Recycle_Second_Category.None;
    public Sprite normal;
    public Sprite select;

    public Image icon;
    public Button button;

    private RecyclePanel recyclePanel;
    private bool isSelected = false;

    private void Start()
    {
        button.onClick.AddListener(OnBtnClick);
        recyclePanel = GetComponentInParent<RecyclePanel>();
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
            recyclePanel.RefreshBySecondCategory(second_Category);
            recyclePanel.Set_AllSecondLabel_SelectState(this);
        }
        else
        {
            isSelected = false;
            button.image.sprite = normal;
            recyclePanel.RefreshBySecondCategory(Recycle_Second_Category.None);
        }
    }

    public void SetSelectFalse()
    {
        isSelected = false;
        button.image.sprite = normal;
    }
}
