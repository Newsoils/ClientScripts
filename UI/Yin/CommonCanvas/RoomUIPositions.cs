using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            [Serializable]
            public class RoomUIPositions
            {
                public string roomName;
                public RectTransform topLeft;
                public RectTransform topRight;
                public RectTransform bottomLeft;
                public RectTransform bottomRight;
                public GameObject roomGo;
            }

        }
    }
}
