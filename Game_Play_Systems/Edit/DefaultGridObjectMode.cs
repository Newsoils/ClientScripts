using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class DefaultGridObjectMode : IEditMode
{
    private LayerMask placementMask = LayerMask.GetMask("Placement");

    public void Enter()
    {
        Debug.Log("进入 DefaultPlacementMode");

        // 关闭网格
        GridSystem.CloseGridView();

        // 相机恢复正常
        //CameraManager.Instance.ChangeState(CameraState.Normal);
    }

    public void Exit()
    {
    }

    public void OnTap(Vector2 screenPos)
    {
        var curP = GridObjectRaycastUtility.RaycastPlacement(screenPos, placementMask, out var hitpos);
        if (curP != null)
        {
            EditManager.Instance.SetMode(new MoveGridObjectMode(curP));
        }
    }

    public void OnLongPress(Vector2 screenPos)
    {
        // 和 Tap 行为一致（可以扩展成长按进入特殊模式）
        OnTap(screenPos);
    }

    public void OnDragBegin(Vector2 screenPos) { }
    public void OnDrag(Vector2 screenPos) { }
    public void OnDragRelease(Vector2 screenPos) { }

    public void OnRotate() { }

}