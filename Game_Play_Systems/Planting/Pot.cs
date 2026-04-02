using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class Pot : GridObject, IClick
{
    public Transform plantRoot;
    public PotData info;
    public List<Renderer> renderers = new();
    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>().ToList();
    }
    public void Init(PotData info)
    {
        if(info != null)
        {
            var gridData = new GridData(info.length, info.width, info.height);
            var placingType = info.category == Plant_Second_Category.HangingPot ? Room_Placing_Type.Ceiling_Furniture : Room_Placing_Type.Surface_Furniture;
            Init(info.itemName, info.id, gridData, placingType);
            this.info = info;
        }

    }
    public void SetRenderState(bool visible)
    {
        foreach (var r in renderers)
            if (r) r.enabled = visible;
        Plant plant = PlantManager.Instance.GetPlantByPot(this);
        if (plant != null)
        {
            plant.SetRenderState(visible);
        }
    }
    public bool OnClick(Vector3 position)
    {
        if(EditManager.Instance.isNoneState)
        {
            EvtDsp.TriggerEvt<Pot>(EvtNames.ShowPotState, this);
            return true;
        }
        return false;
    }
    public bool CheckPlant(PlantData plant)
    {
        if (plant.plantType == Plant_Second_Category.NormalPlant)
        {
            if(info.category != Plant_Second_Category.HangingPot)
            {
                return true;//info.potSize == plant.plantSize;
            }
        }
        if (plant.plantType == Plant_Second_Category.VinePlant)
        {
            return info.category == Plant_Second_Category.HangingPot;
        }
        return false;
    }
}
