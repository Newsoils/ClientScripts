using UnityEngine;
namespace CLIP.Project_Mouse.Game_Play_System
{
    public interface IEditMode
    {
        void Enter();
        void Exit();
        void OnTap(Vector2 screenPos);

        void OnDragBegin(Vector2 screenPos);
        void OnDrag(Vector2 screenPos);   // 按住拖动
        void OnDragRelease(Vector2 screenPos);// 松开

        void OnLongPress(Vector2 screenPos);

        void OnRotate();

    }

}