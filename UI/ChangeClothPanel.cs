using UnityEngine;
using UnityEngine.UI;

public class ChangeClothPanel : UIPanelBase
{
    public Transform root;
    public Button exitButton;

    private void Start()
    {
        exitButton.onClick.AddListener(ClosePanel);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        exitButton.onClick.RemoveListener(ClosePanel);
    }


    public override void OpenPanel(params object[] data)
    {
        Character_Cloth_Manager.Instance.Init_Target_Character_Cloth();
        UIManager.Instance.OpenPanel<ClothPanel>();
        root.gameObject.SetActive(true);
        MainPanel.CloseMainFuncP();

        // 显示上方状态栏 TopPanel，并使其优先级为最高
        UIManager.Instance.GetPanel<MainPanel>().ShowTopPanelOnly();
    }

    public override void ClosePanel()
    {
        root.gameObject.SetActive(false);
        MainPanel.OpenMainFuncP();
        UIManager.Instance.GetPanel<ClothPanel>().ClosePanel();

        // 恢复 MainPanel 所有 UI 的正常显示
        UIManager.Instance.GetPanel<MainPanel>().ShowAll();
        Character_Cloth_Manager.Instance.Sync_Main_Character_Cloth();
    }

}
