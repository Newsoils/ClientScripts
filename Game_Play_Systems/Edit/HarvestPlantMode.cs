using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class HarvestPlantMode : IEditMode
{
    private LayerMask placementMask = LayerMask.GetMask("Placement");
    private bool canInteract;
    private static HarvestPlantMode current;

    public void Enter()
    {
        current = this;
        EvtDsp.TriggerEvt(EvtNames.ShowHarvestPop);
        canInteract = true;
    }

    public void Exit()
    {
        if (current == this)
            current = null;
        EvtDsp.TriggerEvt(EvtNames.ClosePlantPop);
    }

    public static void HandleHarvestCompleted()
    {
        if (current == null)
            return;
        current.canInteract = true;
        EvtDsp.TriggerEvt(EvtNames.ShowHarvestPop);
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
            TapToHarvest(screenPos);
        }

    }

    public void TapToHarvest(Vector2 screenPos)
    {
        var pot = RaycastPot(screenPos);
        if (pot != null)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null && plant.data.growStage == 2)
            {
                PlantManager.Instance.HarvestPlant(plant);
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
