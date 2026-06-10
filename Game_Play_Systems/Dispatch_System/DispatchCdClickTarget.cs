using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System.Dispatch_System
{
    public class DispatchCdClickTarget : MonoBehaviour, IClick, IPrioritizedClick
    {
        public CD3DCarousel carousel;
        public int index;
        public int ClickPriority => 20;

        private void Awake()
        {
            int clickableLayer = LayerMask.NameToLayer("Placement");
            if (clickableLayer >= 0)
                gameObject.layer = clickableLayer;
        }

        public bool OnClick(Vector3 position)
        {
            if (carousel == null)
                return false;

            carousel.SelectCD(index);
            return true;
        }
    }
}
