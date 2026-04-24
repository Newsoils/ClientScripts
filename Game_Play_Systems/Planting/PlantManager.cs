using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using Newtonsoft.Json;
using UnityEngine;


public class PlantManager : SingletonMono<PlantManager>
{
    [Header("数据")]
    public Dictionary<(string, int, string), GameObject> plantModels;//植物名-成长阶段-隐藏-模型
    public Dictionary<(string, string), Material> plantMats;
    public Dictionary<int, PlantData> plantDatas;
    public Dictionary<string, PlantData> plantSeedNameDic;
    public Dictionary<string, FertilizerData> fertilizerDatas;
    public Dictionary<string, PotData> potDatas;
    public GameObject waterObj;

    [Header("实例")]
    public GameObject plantPrefab;
    public Dictionary<string, Plant> plants = new Dictionary<string, Plant>();
    public string plantSeedName;
    
    private float updateTimer;
    private void Start()
    {
        InitData();
        EvtDsp.AddReturnEvt(EvtNames.InitPlant, LoadPlant);
        _ = LoadAsset();
    }
    private async Task LoadAsset()
    {
        waterObj = await GameAssets.Instance.LoadAsycByKey<GameObject>(ResKeys.PREFAB_WATERFALL);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        EvtDsp.RemoveReturnEvt(EvtNames.InitPlant, LoadPlant);
    }
    private void Update()
    {
        updateTimer -= Time.deltaTime;
        //if(Keyboard.current.qKey.wasPressedThisFrame)
        //{
        //    PlantPlant(plantSeedName);
        //}
        //if(Keyboard.current.wKey.wasPressedThisFrame)
        //{
        //    WaterPlant();
        //}
        //if(Keyboard.current.eKey.wasPressedThisFrame)
        //{
        //    HarvestPlant(plants.ToList()[0].Value);
        //}
        //if(Keyboard.current.rKey.wasPressedThisFrame)
        //{
        //    RemovePlant(plants.ToList()[0].Value);
        //}
        //if(Keyboard.current.tKey.wasPressedThisFrame)
        //{
        //    FertilizePlant(plants.ToList()[0].Value, "生长肥料");
        //}
    }
    private void InitData()
    {
        var info = JsonConvert.DeserializeObject<List<PlantData>>(JsonData_Manager.Load_Single_JsonData("project_mouse_tb_plant_info"));
        plantDatas = info.ToDictionary(x => x.plantId, x => x);
        plantSeedNameDic = info.ToDictionary(x => x.plantSeed, x => x);
        fertilizerDatas = JsonConvert.DeserializeObject<List<FertilizerData>>(JsonData_Manager.Load_Single_JsonData("project_mouse_tb_fertilizer_effect")).ToDictionary(x => x.fertilizerName, x => x);
        potDatas = JsonConvert.DeserializeObject<List<PotData>>(JsonData_Manager.Load_Single_JsonData("project_mouse_tb_pot_info")).ToDictionary(x => x.itemName, x => x);

        List<GameObject> source = Resources.LoadAll<GameObject>("Models/Plant").ToList();
        plantModels = new Dictionary<(string, int, string), GameObject>();
        foreach(var model in source)
        {
            string[] names = model.name.Split('_');
            if(names.Length < 2)//该模型命名不规范
            {
                continue;
            }
            if(names.Length == 2)
            {
                plantModels.Add((names[0], int.Parse(names[1]), "default"), model);
            }
            else if(names.Length == 3)
            {
                plantModels.Add((names[0], int.Parse(names[1]), names[2]), model);
            }
        }
        List<Material> materials = Resources.LoadAll<Material>("Materials/Plant").ToList();
        plantMats = new Dictionary<(string, string), Material>();
        foreach(var material in materials)
        {
            string[] names = material.name.Split('_');
            if (names.Length < 2)//该材质命名不规范
            {
                continue;
            }
            else
            {
                plantMats.Add((names[0], names[1]), material);
            }
        }
    }
    #region 植物相关
    private async Task LoadPlant()
    {
        await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "LoadPlant", LoadPlantTask(), null);
    }
    public ServerTask LoadPlantTask()
    {
        ServerTask task = new ServerTask("LoadPlant", (string receive, ServerTask task) =>
        {
            if (receive == "nodata")
            {
                task.isBreak = true;
                task.result = "植物数据加载失败";
                return;
            }
            foreach(var plant in plants.Values)
            {
                Destroy(plant.gameObject);
            }
            plants.Clear();
            List<PlantLocalData> datas = JsonConvert.DeserializeObject<List<PlantLocalData>>(receive);
            foreach(var data in datas)
            {
                if(TryCreatePlant(data, out var plant))
                {
                    plants.Add(data.uid, plant);
                }
            }
        });
        return task;
    }
    #region 植物交互
    /// <summary>
    /// 浇水
    /// </summary>
    public void WaterPlant()
    {
        EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "WaterPlant", WaterPlantTask(), null);
    }
    public ServerTask WaterPlantTask()
    {
        ServerTask task = new ServerTask("WaterPlant", (string receive, ServerTask task) =>
        {
            if (receive == "nodata")
            {
                task.isBreak = true;
                task.result = "植物数据加载失败";
                return;
            }
            List<PlantLocalData> datas = JsonConvert.DeserializeObject<List<PlantLocalData>>(receive);
            foreach (var data in datas)
            {
                if(plants.TryGetValue(data.uid, out var plant))
                {
                    plant.UpdateData(data);
                }
            }

        });
        return task;
    }
    /// <summary>
    /// 更新
    /// </summary>
    public void UpdatePlant()
    {
        if(updateTimer < 0)
        {
            updateTimer = 3;
            EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "UpdatePlant", UpdatePlantTask(), null);
        }
    }   
    public ServerTask UpdatePlantTask()
    {
        ServerTask task = new ServerTask("UpdatePlant", (string receive, ServerTask task) =>
        {
            if (receive == "nodata")
            {
                task.isBreak = true;
                task.result = "植物数据加载失败";
                return;
            }
            List<PlantLocalData> datas = JsonConvert.DeserializeObject<List<PlantLocalData>>(receive);
            foreach (var data in datas)
            {
                if (plants.TryGetValue(data.uid, out var plant))
                {
                    plant.UpdateData(data);
                }
            }

        });
        return task;
    }
    /// <summary>
    /// 种植
    /// </summary>
    /// <param name="seedName"></param>
    public async Task PlantPlant(string seedName, string potUid)
    {
        int plantId = -1;
        if(TryGetPlantDataByName(seedName, out PlantData data))
        {
            plantId = data.plantId;
        }
        if(plantId != -1)
        {
            await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "PlantPlant", PlantPlantTask(plantId, potUid), null);
        }
    }
    public ServerTask PlantPlantTask(int plantId, string potUid)
    {
        string send = JsonConvert.SerializeObject((plantId, potUid));
        ServerTask task = new ServerTask(send, (string receive, ServerTask task) =>
        {
            if (receive == "nodata")
            {
                task.isBreak = true;
                task.result = "植物数据加载失败";
                return;
            }
            if(receive == "fail")
            {
                task.isBreak = true;
                task.result = "生成植物失败";
                return;
            }
            (PlantLocalData newPlantData, List<PlantLocalData> datas) = JsonConvert.DeserializeObject<(PlantLocalData, List<PlantLocalData>)>(receive);
            if(TryCreatePlant(newPlantData, out var newPlant))
            {
                plants.Add(newPlantData.uid, newPlant);
            }

            foreach (var data in datas)
            {
                if (plants.TryGetValue(data.uid, out var plant))
                {
                    plant.UpdateData(data);
                }
            }
        });
        return task;
    }
    /// <summary>
    /// 收获
    /// </summary>
    /// <param name="plant"></param>
    public async Task HarvestPlant(Plant plant)
    {
        await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "HarvestPlant", HarvestPlantTask(plant.data.uid), null);
    }
    public ServerTask HarvestPlantTask(string plantUid)
    {
        ServerTask task = new ServerTask(plantUid, (string receive, ServerTask task) =>
        {
            if (receive == "nodata")
            {
                task.isBreak = true;
                task.result = "植物数据加载失败";
                return;
            }
            PlantLocalData harvestPlantData = JsonConvert.DeserializeObject<PlantLocalData>(receive);
            if (plants.TryGetValue(harvestPlantData.uid, out var harvestPlant))
            {
                harvestPlant.Harvest(harvestPlantData);
            }
        });
        return task;
    }
    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="plant"></param>
    public async Task RemovePlant(Plant plant)
    {
        await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "RemovePlant", RemovePlantTask(plant.data.uid), null);
    }
    public ServerTask RemovePlantTask(string plantUid)
    {
        ServerTask task = new ServerTask(plantUid, (string receive, ServerTask task) =>
        {
            if (receive == "nodata")
            {
                task.isBreak = true;
                task.result = "植物数据加载失败";
                return;
            }
            if(plants.TryGetValue(plantUid, out var plant))
            {
                plant.Remove();
            }
        });
        return task;
    }
    public async Task FertilizePlant(Plant plant, string fertilizerName)
    {
        int fertilizerId = fertilizerDatas[fertilizerName].fertilizerId;
        await EvtDsp.ReturnEvt<string, ServerTask, Action<string>, Task>(EvtNames.Excute_Server_Task, "FertilizePlant", FertilizePlantTask(plant.data.uid, fertilizerId), null);
    }
    public ServerTask FertilizePlantTask(string plantUid, int fertilizerId)
    {
        string send = JsonConvert.SerializeObject((plantUid, fertilizerId));
        ServerTask task = new ServerTask(send, (string receive, ServerTask task) =>
        {
            if (receive == "nodata")
            {
                task.isBreak = true;
                task.result = "植物数据加载失败";
                return;
            }
            (bool isSuccess,PlantLocalData newData) = JsonConvert.DeserializeObject<(bool, PlantLocalData)>(receive);
            if (plants.TryGetValue(plantUid, out var plant))
            {
                plant.UpdateData(newData);
            }
        });
        return task;
    }
    private bool TryCreatePlant(PlantLocalData data, out Plant plant)
    {
        GameObject obj = Instantiate(plantPrefab);
        plant = obj.GetComponent<Plant>();
        return plant.Init(data);
    }
    #endregion
    public GameObject GetModel(int plantId, int growStage, string variantName)
    {
        if(!plantDatas.TryGetValue(plantId, out var data))
        {
            Log.Error("植物ID不存在");
            return null;
        }
        string modelKey = data.modelKey;
        if(string.IsNullOrEmpty(variantName))
        {
            variantName = "default";
        }
        if(plantModels.TryGetValue((modelKey, growStage + 1, variantName), out var model))
        {
            return model;
        }
        if (plantModels.TryGetValue((modelKey, growStage + 1, "default"), out var defaultModel))
        {
            return defaultModel;
        }
        return plantModels[("default", growStage + 1, variantName)];
    }
    public Material GetMaterial(int plantId, int growStage, string variantKey)
    {
        if (!plantDatas.TryGetValue(plantId, out var data))
        {
            Log.Error("植物ID不存在");
            return null;
        }
        List<string> matKeys = data.matKey;
        if (matKeys.Count == 0)
        {
            return null;
        }
        else
        {
            string modelKey = data.modelKey;
            int index = variantKey == null ? 0 : data.variantKey.IndexOf(variantKey);
            string matKey = matKeys[index];
            if(plantMats.TryGetValue((modelKey, matKey), out Material mat))
            {
                return mat;
            }
            else
            {
                Log.Error(data.modelKey + "_" + variantKey);
                return null;
            }
        }
    }
    public Plant GetPlantByPot(Pot pot)
    {
        foreach(var plant in plants.Values)
        {
            if(plant.data.potUid == pot.UId)
            {
                return plant;
            }
        }
        return null;
    }
    public bool TryGetPlantDataByName(string seedName, out PlantData data)
    {
        data = null;
        foreach (var plant in plantDatas.Values)
        {
            if (plant.plantSeed == seedName)
            {
                data = plant;
                return true;
            }
        }
        return false;
    }
    #endregion
}
