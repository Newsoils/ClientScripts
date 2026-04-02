using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class PersonalIconUnit : MonoBehaviour
            {
                public ChangeIcon changeIconManager;
                public Image bg;
                public Image icon;
                public avatar_icon_info avatar_Icon_Info;

                public Color selectedColor;
                public Color deselectedColor;

                public void InitUnit(ChangeIcon changeIconManager, avatar_icon_info avatar_Icon_Info)
                {
                    this.avatar_Icon_Info = avatar_Icon_Info;
                    this.changeIconManager = changeIconManager;
                    DeselectUnit();
                    // 加载图片
                }

                public void SelectUnit()
                {
                    bg.color = selectedColor;
                    changeIconManager.SelectIcon(this);
                }

                public void DeselectUnit()
                {
                    bg.color = deselectedColor;
                }
            }
        }
    }
}