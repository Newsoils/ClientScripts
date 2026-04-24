using CLIP.Project_Mouse.ENUM;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 回收箱一级分类 Tab，用 <see cref="Recycle_First_Category"/>；必须挂在 <see cref="RecyclePanel"/> 子树下。
/// </summary>
[RequireComponent(typeof(Button))]
public class RecycleFirstLabel : MonoBehaviour
{
    [Header("分类配置")]
    public Recycle_First_Category firstCategory = Recycle_First_Category.None;

    public Image icon;
    public Sprite normal;
    public Sprite Selected;

    private Button _button;
    private RecyclePanel recyclePanel;
    private bool isSelected = false;

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnBtnClick);
        recyclePanel = GetComponentInParent<RecyclePanel>();
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
            icon.sprite = Selected;
            recyclePanel.RefreshByFirstCategory(firstCategory);
            recyclePanel.Set_FirstLabel_SelectedState(this);
        }
        else
        {
            isSelected = false;
            icon.sprite = normal;
            recyclePanel.RefreshByFirstCategory(Recycle_First_Category.None);
        }
    }

    public void SetSelectFalse()
    {
        isSelected = false;
        icon.sprite = normal;
    }
}
