using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class WaterPlantMode : IEditMode
{
    private float waterTimer;
    private GameObject waterObj;
    public void Enter()
    {
        CameraManager.Instance.ChangeState(CameraState.Frozen);
    }

    public void Exit()
    {
        EvtDsp.TriggerEvt(EvtNames.ClosePlantPop);
        CameraManager.Instance.ChangeState(CameraState.Normal);
    }

    public void OnDrag(Vector2 screenPos)
    {
        if(waterTimer == 0)
        {
            AudioManager.Instance.PlayAudioByRefKey("water");
            waterObj = GameObject.Instantiate(PlantManager.Instance.waterObj);
        }
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 1));
        waterObj.transform.position = worldPos;
        waterTimer += Time.deltaTime;
        if (waterTimer > 3)
        {
            Log.Info("已浇水");
            PlantManager.Instance.WaterPlant();
            waterTimer = -9999;
            EditManager.Instance.ExitCurrentMode();
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, "已浇水");
            waterObj.GetComponent<ParticleSystem>().Stop();
        }
    }

    public void OnDragBegin(Vector2 screenPos)
    {
    }

    public void OnDragRelease(Vector2 screenPos)
    {
        waterTimer = 0;
        EditManager.Instance.ExitCurrentMode();
        waterObj.GetComponent<ParticleSystem>().Stop();
    }

    public void OnLongPress(Vector2 screenPos)
    {

    }

    public void OnRotate()
    {
    }

    public void OnTap(Vector2 screenPos)
    {

    }
}
