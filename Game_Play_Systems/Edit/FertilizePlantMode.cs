using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using CLIP.Project_Mouse.Kernel;

public class FertilizePlantMode : IEditMode
{
    private LayerMask gridLayerMask = LayerMask.GetMask("GridLayer");
    private LayerMask placementMask = LayerMask.GetMask("Placement");
    private string curFertilizerName;
    private bool canInteract;
    private static FertilizePlantMode current;
    public FertilizePlantMode(string fertilizerName)
    {
        curFertilizerName = fertilizerName;
    }
    public void Enter()
    {
        current = this;
        EvtDsp.TriggerEvt(EvtNames.ShowFertilizerPop);
        canInteract = true;
    }

    public void Exit()
    {
        if (current == this)
            current = null;
        EvtDsp.TriggerEvt(EvtNames.ClosePlantPop);
    }

    public static void HandleFertilizeCompleted()
    {
        if (current == null)
            return;
        current.canInteract = true;
        EvtDsp.TriggerEvt(EvtNames.ShowFertilizerPop);
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
            TapToFertilize(screenPos);
        }

    }

    public void TapToFertilize(Vector2 screenPos)
    {
        var pot = RaycastPot(screenPos);
        if (pot != null)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null && !plant.data.isFertilize && plant.data.growStage == 1)
            {
                var fertItem = Global_Inventory_Manager.GetItem(curFertilizerName);
                int fertCount = fertItem != null ? fertItem._item_count : 0;
                if (fertCount <= 0)
                {
                    PromptManager.ShowUpPrompt(PromptId.FertilizerNotEnough);
                    EditManager.Instance.ExitCurrentMode();
                    return;
                }

                PlantManager.Instance.FertilizePlant(plant, curFertilizerName);
                return;
            }
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
