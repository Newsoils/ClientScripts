using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class HarvestPlantMode : IEditMode
{
    private LayerMask gridLayerMask = LayerMask.GetMask("GridLayer");
    private LayerMask placementMask = LayerMask.GetMask("Placement");
    private bool canInteract;

    public void Enter()
    {
        EvtDsp.TriggerEvt(EvtNames.ShowHarvestPop);
        canInteract = true;
    }

    public void Exit()
    {
        EvtDsp.TriggerEvt(EvtNames.ClosePlantPop);
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
            _ = TapToHarvest(screenPos);
        }

    }

    public async Task TapToHarvest(Vector2 screenPos)
    {
        var pot = RaycastPot(screenPos);
        if (pot != null)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null && plant.data.growStage == 4)
            {
                await PlantManager.Instance.HarvestPlant(plant);
                EvtDsp.TriggerEvt(EvtNames.ShowHarvestPop);
            }
        }
        else
        {
            EditManager.Instance.ExitCurrentMode();
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
