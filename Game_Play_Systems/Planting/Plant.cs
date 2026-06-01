using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public PlantLocalData data;
    public GameObject curModel;

    private List<Renderer> renderers = new();
    private bool visible = true;
    private double dataLocalTime;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>().ToList();
    }

    public void Harvest(PlantLocalData plant)
    {
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
        dataLocalTime = Time.realtimeSinceStartupAsDouble;
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
                transform.parent = pot.plantRoot;
                flag = false;
                break;
            }
        }
        if(flag)
        {
            Destroy(gameObject);
            return false;
        }
        RefreshModel();
        return true;
    }

    public void UpdateData(PlantLocalData data)
    {
        this.data = data;
        dataLocalTime = Time.realtimeSinceStartupAsDouble;
        RefreshModel();
    }

    public void UpdateGrowStageFromServer(int growStage)
    {
        data.growStage = growStage;
        RefreshModel();
    }

    public float GetNextStageRemainingMinutes()
    {
        if(data.growStage != 1)
        {
            return 0f;
        }
        double elapsed = Time.realtimeSinceStartupAsDouble - dataLocalTime;
        return Mathf.Max((float)(data.plantTime - elapsed) / 60f, 0f);
    }

    private int CalculateModelStage(PlantData plantData)
    {
        if (data.growStage == 1)
        {
            return Mathf.Max(data.growStage2 - 1, 0);
        }
        if (data.growStage == 2)
        {
            return Mathf.Max(plantData.lifeCycle.Count - 2, 0);
        }
        if (data.growStage == 3)
        {
            return Mathf.Max(plantData.lifeCycle.Count - 1, 0);
        }
        return Mathf.Max(data.growStage2 - 1, 0);
    }

    private void RefreshModel()
    {
        if (curModel != null)
        {
            Destroy(curModel.gameObject);
        }

        if (!PlantManager.Instance.plantDatas.TryGetValue(data.plantId, out var plantData))
            return;

        var lib = PlantResourceLibrary.Instance;
        int modelStage = CalculateModelStage(plantData);

        if (!lib.TryGetModel(plantData.modelKey, modelStage + 1, data.variantKey, out var modelPrefab))
            return;

        curModel = Instantiate(modelPrefab, transform);

        string variantKey = data.variantKey;
        int variantIndex = variantKey == null ? 0 : plantData.variantKey.IndexOf(variantKey);
        if (plantData.matKey != null && plantData.matKey.Count > 0 && variantIndex >= 0 && variantIndex < plantData.matKey.Count)
        {
            if (lib.TryGetMaterial(plantData.modelKey, plantData.matKey[variantIndex], out var mat))
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
