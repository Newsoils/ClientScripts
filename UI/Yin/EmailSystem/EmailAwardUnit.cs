using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CLIP.Framework_Unity.Asset;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class EmailAwardUnit : MonoBehaviour
            {
                public string iconPath;
                public Image icon;
                public TMP_Text quantityText;
                public bool isPhoto = false;

                public void InitAwardUnit(string iconPath, int quantity, bool isPhoto = false)
                {
                    this.iconPath = iconPath;
                    this.isPhoto = isPhoto;

                    if (isPhoto)
                    {
                        // icon
                    }
                    else
                    {
                        string[] parts = iconPath.Split('#');
                        GameAssets.LoadSubSprite(parts[0], parts[1], (sprite) =>
                        {
                            icon.sprite = sprite;
                        });
                    }

                    //quantityText.text = quantity > 1 ? quantity.ToString() : "";
                    quantityText.text = quantity.ToString();
                }
            }
        }
    }
}