using CLIP.Framework_Unity.Asset;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class AchievementAwardUnit : MonoBehaviour
            {
                public string mysteryIconPath;
                public string iconPath;
                public Image icon;
                public TMP_Text quantityText;

                public void InitAwardUnit(string iconPath, int quantity, bool isMystery = false)
                {
                    if (isMystery)
                    {
                        // 加载空图�?
                        quantityText.text = "";
                        return;
                    }

                    this.iconPath = iconPath;

                    string[] parts = iconPath.Split('#');
                    GameAssets.LoadSubSprite(parts[0], parts[1], (sprite) =>
                    {
                        icon.sprite = sprite;
                    });

                    //quantityText.text = quantity > 1 ? quantity.ToString() : "";
                    quantityText.text = quantity.ToString();
                }
            }
        }
    }
}

