using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Plant : MonoBehaviour
{
    public PlantLocalData data;
    public GameObject curModel;
    public float nextStageTime;
    [SerializeField]private float water;
    private bool isWater;
    private Room room;
    private List<Renderer> renderers = new();
    bool visible = true;
    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>().ToList();
    }
    private void Update()
    {
        if(Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            Grow();
        }
        if(data != null)
        {
            water -= Time.deltaTime * 0.069f / 60;
            nextStageTime -= Time.deltaTime / 60;
            if(IsNeedUpdate())
            {
                PlantManager.Instance.UpdatePlant();
            }
        }
    }
    private bool IsNeedUpdate()
    {
        if(!PlantManager.Instance.plants.Values.Contains(this))
        {
            Log.Error("初始化有错误，检查初始化过程");
            Destroy(gameObject);
            return false;
        }
        if(data.growStage >= 5)
        {
            return false;
        }
        if(isWater && water <= 0)
        {
            return true;
        }
        if(nextStageTime <= 0)
        {
            return true;
        }
        return false;
    }
    public void Grow()
    {
        data.growStage++;
        RefreshModel();
    }
    public void Harvest(PlantLocalData plant)
    {
        PlantData plantData = PlantManager.Instance.plantDatas[data.plantId];
        int num = Random.Range(plantData.harvestRange.Item1, plantData.harvestRange.Item2 + 1);
        string plantVariant = data.variantKey == null ? "default" : data.variantKey;
        List<(string, int)> item = new List<(string, int)> { (plantData.harvestItem[plantData.variantKey.IndexOf(plantVariant)], num) };
        Global_Inventory_Manager.Change_Items_Count(item, "收获植物");
        if(plant.harvestTime == -1)
        {
            Remove();
        }
        else
        {
            UpdateData(plant);
        }
    }
    public void Remove()
    {
        PlantManager.Instance.plants.Remove(data.uid);
        Destroy(gameObject);
    }
    public bool Init(PlantLocalData data)
    {
        this.data = data;
        bool flag = true;
        if(data.potUid == null)
        {
            Destroy(gameObject);
            return false;
        }
        foreach(var room in RoomSystem.Instance.rooms)
        {
            if (room.potsDic.TryGetValue(data.potUid, out var pot))
            {
                transform.position = pot.plantRoot.position;
                flag = false;
                break;
            }
        }
        if(flag)
        {
            Destroy(gameObject);
            return false;
        }
        room = RoomSystem.GetRoomByFloorPosition(transform.position);
        RefreshModel();
        return true;
    }
    public void UpdateData(PlantLocalData data)
    {
        this.data = data;
        water = data.water;
        isWater = data.water > 0;
        nextStageTime = (float)(PlantManager.Instance.plantDatas[data.plantId].lifeCycle[data.growStage] - data.curGrowTime) / (isWater ? 1 : 0.7f);
        RefreshModel();
    }
    private void RefreshModel()
    {
        if (curModel != null)
        {
            Destroy(curModel.gameObject);
        }
        curModel = Instantiate(PlantManager.Instance.GetModel(data.plantId, data.growStage, data.variantKey),transform);
        Material mat = PlantManager.Instance.GetMaterial(data.plantId, data.growStage, data.variantKey);
        if (mat != null)
        {
            curModel.GetComponent<MeshRenderer>().material = mat;
        }
        curModel.transform.localPosition = Vector3.zero;
        renderers = GetComponentsInChildren<Renderer>().ToList();
        SetRenderState(visible);
    }
    public void SetRenderState(bool visible)
    {
        this.visible = visible;
        foreach (var r in renderers)
            if (r) r.enabled = visible;
    }
}
