using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class RemovePlantMode : IEditMode
{
    private LayerMask placementMask = LayerMask.GetMask("Placement");
    private bool canInteract;
    private static RemovePlantMode current;

    public void Enter()
    {
        current = this;
        EvtDsp.TriggerEvt(EvtNames.ShowRemovePop);
        canInteract = true;
    }

    public void Exit()
    {
        if (current == this)
            current = null;
        EvtDsp.TriggerEvt(EvtNames.ClosePlantPop);
    }

    public static void HandleRemoveCompleted()
    {
        if (current == null)
            return;
        current.canInteract = true;
        EvtDsp.TriggerEvt(EvtNames.ShowRemovePop);
    }

    public void OnDrag(Vector2 screenPos)
    {
    }

    public void OnDragBegin(Vector2 screenPos)
    {
    }

    public void OnDragRelease(Vector2 screenPos)
    {
    }

    public void OnLongPress(Vector2 screenPos)
    {
    }

    public void OnRotate()
    {
    }

    public void OnTap(Vector2 screenPos)
    {
        if (canInteract)
        {
            canInteract = false;
            TapToRemove(screenPos);
        }

    }

    public void TapToRemove(Vector2 screenPos)
    {
        var pot = RaycastPot(screenPos);
        if (pot != null)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null)
            {
                PlantManager.Instance.RemovePlant(plant);
                return;
            }
        }
        else
        {
            EditManager.Instance.ExitCurrentMode();
            return;
        }
        canInteract = true;
    }
    private Pot RaycastPot(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100, placementMask))
        {
            return hit.collider.GetComponentInParent<Pot>();
        }
        return null;
    }
}
