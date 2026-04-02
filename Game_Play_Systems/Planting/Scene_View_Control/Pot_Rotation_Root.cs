using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Scene_View_Control
        {
            [System.Serializable]
            public class Pot_Rotation_Root : MonoBehaviour
            {
                public BoxCollider _collider;
                public Renderer _display_render;
                public Transform _plant_root;
            }
        }
    }
}