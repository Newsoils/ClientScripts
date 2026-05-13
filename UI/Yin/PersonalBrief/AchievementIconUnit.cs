using UnityEngine;
using UnityEngine.UI;
using CLIP.Project_Mouse.Kernel.Achievement;

namespace CLIP.Project_Mouse.UI
{
    public class AchievementIconUnit : MonoBehaviour
    {
        public achievement_design_info info;
        //public AchievementPanelInteractManager panel;
        public Image bg;
        public Image icon;

        public Color selectedColor;
        public Color deselectedColor;

        public void InitUnit(achievement_design_info info)
        {
            this.info = info;
            DeselectUnit();
            // 加载图片
        }

        public void SelectUnit()
        {
            bg.color = selectedColor;
            //panel.SelectAchievement(this);
            UIManager.Instance.GetPanel<AchievementPanel>().SelectAchievement(this);
        }

        public void DeselectUnit()
        {
            bg.color = deselectedColor;
        }
    }
}