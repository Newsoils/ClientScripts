using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class BgImage : MonoBehaviour
            {
                public Canvas bgCanvas;
                public Image bgImage;
                public Sprite blue;
                public Sprite green;

                void Start()
                {
                    bgCanvas = GetComponent<Canvas>();
                }

                void Update()
                {
                    bgCanvas.worldCamera = Camera.main;
                }

                public void ChangeImage(Sprite sprite)
                {
                    bgImage.sprite = sprite;
                }
            }
        }
    }
}

