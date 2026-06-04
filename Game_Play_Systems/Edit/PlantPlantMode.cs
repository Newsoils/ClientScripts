using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using CLIP.Project_Mouse.Kernel;

public class PlantPlantMode : IEditMode
{
    private LayerMask gridLayerMask = LayerMask.GetMask("GridLayer");
    private LayerMask placementMask = LayerMask.GetMask("Placement");
    private string curSeedName;
    private PlantData plantData;
    private bool canInteract;
    private static PlantPlantMode current;
    public PlantPlantMode(string seedName)
    {
        curSeedName = seedName;
        PlantManager.Instance.TryGetPlantDataByName(seedName, out plantData);
    }
    public void Enter()
    {
        current = this;
        EvtDsp.TriggerEvt<PlantData>(EvtNames.ShowSeedPop, plantData);
        canInteract = true;
    }

    public void Exit()
    {
        if (current == this)
            current = null;
        EvtDsp.TriggerEvt(EvtNames.ClosePlantPop);
    }

    public static void HandlePlantCompleted()
    {
        if (current == null)
            return;
        current.canInteract = true;
        EvtDsp.TriggerEvt<PlantData>(EvtNames.ShowSeedPop, current.plantData);
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
        if(canInteract)
        {
            canInteract = false;
            TapToPlant(screenPos);
        }

    }

    public void TapToPlant(Vector2 screenPos)
    {
        var pot = RaycastPot(screenPos);
        if(pot != null)
        {
            if(pot.CheckPlant(plantData))
            {
                var seedItem = Global_Inventory_Manager.GetItem(curSeedName);
                int seedCount = seedItem != null ? seedItem._item_count : 0;
                if (seedCount <= 0)
                {
                    PromptManager.ShowUpPrompt(PromptId.SeedNotEnough);
                    EditManager.Instance.ExitCurrentMode();
                    return;
                }

                PlantManager.Instance.PlantPlant(curSeedName, pot.UId);
                return;
            }
            else
            {
                string result = "";
                switch (plantData.plantType)
                {
                    case Plant_Second_Category.NormalPlant:
                        switch (plantData.plantSize)
                        {
                            case 1:
                                result = "小花盆";
                                break;
                            case 2:
                                result = "中花盆";
                                break;
                            case 3:
                                result = "大花盆";
                                break;
                        }
                        break;
                    case Plant_Second_Category.VinePlant:
                        result = "吊顶花盆";
                        break;
                }
                PromptManager.ShowUpPrompt(PromptId.PlantTypeLimit, result);
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
