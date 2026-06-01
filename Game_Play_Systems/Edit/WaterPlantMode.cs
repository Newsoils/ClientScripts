using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class WaterPlantMode : IEditMode
{
    private float waterTimer;
    private GameObject waterObj;
    private Task<GameObject> waterPrefabTask;

    public void Enter()
    {
        waterTimer = 0;
        waterObj = null;
        waterPrefabTask = PlantManager.Instance.GetWaterPrefabAsync();
        CameraManager.Instance.Freeze();
    }

    public void Exit()
    {
        EvtDsp.TriggerEvt(EvtNames.ClosePlantPop);
        CameraManager.Instance.Unfreeze();
    }

    public void OnDrag(Vector2 screenPos)
    {
        if (waterTimer < 0)
            return;
        if (waterObj == null)
        {
            if (waterPrefabTask == null || !waterPrefabTask.IsCompleted)
                return;
            GameObject waterPrefab = waterPrefabTask.Result;
            waterObj = GameObject.Instantiate(waterPrefab);
            AudioManager.Instance.PlayAudioByRefKey("water");
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
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, "已浇水！");
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
        if (waterObj != null)
        {
            waterObj.GetComponent<ParticleSystem>().Stop();
        }
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
