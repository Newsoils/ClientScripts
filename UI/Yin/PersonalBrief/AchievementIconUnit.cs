using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CLIP.Project_Mouse.Kernel.Achievement;
using CLIP.Project_Mouse.NewFrame.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class AchievementIconUnit : MonoBehaviour
            {
                public achievement_design_info info;
                //public AchievementPanelInteractManager panel;
                public Image bg;
                public Image icon;

                public Color selectedColor;
                public Color deselectedColor;

                //public void InitUnit(AchievementPanelInteractManager panel, achievement_design_info info)
                public void InitUnit(achievement_design_info info)
                {
                    this.info = info;
                    //this.panel = panel;
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
    }
}