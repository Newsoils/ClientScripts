using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Network;
using Newtonsoft.Json;
using UnityEngine;


public class PlantManager : SingletonMono<PlantManager>
{
    protected override bool PersistAcrossScenes => true;

    [Header("数据")]
    public Dictionary<int, PlantData> plantDatas;
    public Dictionary<string, PlantData> plantSeedNameDic;
    private Dictionary<int, PlantData> plantSeedItemIdDic;
    public Dictionary<string, FertilizerData> fertilizerDatas;
    public Dictionary<string, PotData> potDatas;
    private GameObject waterPrefab;

    [Header("实例")]
    public GameObject plantPrefab;
    public Dictionary<string, Plant> plants = new Dictionary<string, Plant>();
    public string plantSeedName;

    private List<Cmd.PlantInfo> cachedPlantInfos = new List<Cmd.PlantInfo>();
    private Dictionary<ulong, double> plantCacheTimes = new Dictionary<ulong, double>();
    private bool roomReady;
    
    private float updateTimer;
    private void Start()
    {
        InitData();
        EvtDsp.AddReturnEvt(EvtNames.InitPlant, RefreshPlantsForCurrentRoomAsync);
        _ = LoadAsset();
    }
    private async Task LoadAsset()
    {
        plantPrefab = Resources.Load<GameObject>("Prefabs/Plant");
        await GetWaterPrefabAsync();
    }
    public async Task<GameObject> GetWaterPrefabAsync()
    {
        if (waterPrefab == null)
        {
            waterPrefab = await GameAssets.Instance.LoadAsycByKey<GameObject>(ResKeys.PREFAB_WATERFALL);
        }
        return waterPrefab;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        EvtDsp.RemoveReturnEvt(EvtNames.InitPlant, RefreshPlantsForCurrentRoomAsync);
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
        var info = JsonConvert.DeserializeObject<List<PlantData>>(JsonDataManager.Load_Single_JsonData("project_mouse_tb_plant_info"));
        plantDatas = info.ToDictionary(x => x.plantId, x => x);
        plantSeedNameDic = info.ToDictionary(x => x.plantSeed, x => x);
        plantSeedItemIdDic = new Dictionary<int, PlantData>();
        foreach (var plant in info)
        {
            var seedItem = Global_Inventory_Manager.GetItemInfo(plant.plantSeed);
            plantSeedItemIdDic[seedItem.item_id] = plant;
        }
        fertilizerDatas = JsonConvert.DeserializeObject<List<FertilizerData>>(JsonDataManager.Load_Single_JsonData("project_mouse_tb_fertilizer_effect")).ToDictionary(x => x.fertilizerName, x => x);
        potDatas = JsonConvert.DeserializeObject<List<PotData>>(JsonDataManager.Load_Single_JsonData("project_mouse_tb_pot_info")).ToDictionary(x => x.itemName, x => x);
    }
    #region 植物相关
    public void RequestPlantDataFromServer()
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
            NetWork_Center_WSS.SendMsg(new Cmd.GetAllPlantReq());
    }

    private Task RefreshPlantsForCurrentRoomAsync()
    {
        roomReady = true;
        ApplyCachedPlantsToCurrentRoom();
        return Task.CompletedTask;
    }

    public void CacheAllPlants(IEnumerable<Cmd.PlantInfo> plantInfos)
    {
        cachedPlantInfos = plantInfos != null ? plantInfos.Select(x => x.Clone()).ToList() : new List<Cmd.PlantInfo>();
        plantCacheTimes.Clear();
        double now = Time.realtimeSinceStartupAsDouble;
        foreach (var plantInfo in cachedPlantInfos)
        {
            plantCacheTimes[plantInfo.UID] = now;
        }
    }

    public void ApplyCachedPlantsToCurrentRoom()
    {
        if (!roomReady)
            return;
        RebuildPlants(cachedPlantInfos);
    }

    public void ApplyAllPlants(IEnumerable<Cmd.PlantInfo> plantInfos)
    {
        CacheAllPlants(plantInfos);
        ApplyCachedPlantsToCurrentRoom();
    }

    private void RebuildPlants(IEnumerable<Cmd.PlantInfo> plantInfos)
    {
        foreach(var plant in plants.Values)
        {
            if (plant != null)
            {
                Destroy(plant.gameObject);
            }
        }
        plants.Clear();

        foreach(var plantInfo in plantInfos)
        {
            var data = ToLocalData(plantInfo);
            if(TryCreatePlant(data, out var plant))
            {
                plants.Add(data.uid, plant);
            }
        }
        EvtDsp.TriggerEvt(EvtNames.ReloadPlantData);
    }

    public void ApplyPlant(Cmd.PlantInfo plantInfo)
    {
        var data = ToLocalData(plantInfo);
        if (data.harvestTime == -1)
        {
            RemoveCachedPlant(plantInfo.UID);
            RemovePlantByUid(data.uid);
            return;
        }
        if(plants.TryGetValue(data.uid, out var plant))
        {
            plant.UpdateData(data);
        }
        else if(TryCreatePlant(data, out var newPlant))
        {
            plants.Add(data.uid, newPlant);
        }
        EvtDsp.TriggerEvt(EvtNames.ReloadPlantData);
    }

    public void ApplyPlantChange(Cmd.PlantChangeS2C change)
    {
        foreach (var plantInfo in change.PlantAdd)
        {
            UpsertCachedPlant(plantInfo);
            if (roomReady)
                ApplyPlant(plantInfo);
        }
        foreach (var plantInfo in change.PlantUpd)
        {
            UpsertCachedPlant(plantInfo);
            if (roomReady)
                ApplyPlant(plantInfo);
        }
        foreach (var plantInfo in change.PlantDel)
        {
            RemoveCachedPlant(plantInfo.UID);
            if (roomReady)
                RemovePlantByUid(plantInfo.UID.ToString());
        }
    }

    private void UpsertCachedPlant(Cmd.PlantInfo plantInfo)
    {
        int index = cachedPlantInfos.FindIndex(x => x.UID == plantInfo.UID);
        if (index >= 0)
        {
            cachedPlantInfos[index] = plantInfo.Clone();
        }
        else
        {
            cachedPlantInfos.Add(plantInfo.Clone());
        }
        plantCacheTimes[plantInfo.UID] = Time.realtimeSinceStartupAsDouble;
    }

    private void RemoveCachedPlant(ulong plantUid)
    {
        cachedPlantInfos.RemoveAll(x => x.UID == plantUid);
        plantCacheTimes.Remove(plantUid);
    }

    private void RemovePlantByUid(string plantUid)
    {
        if(plants.TryGetValue(plantUid, out var plant))
        {
            plant.Remove();
            EvtDsp.TriggerEvt(EvtNames.ReloadPlantData);
        }
    }

    public void ApplyGrowStage(string plantUid, int growStage)
    {
        if(plants.TryGetValue(plantUid, out var plant))
        {
            plant.UpdateGrowStageFromServer(growStage);
        }
    }

    private static PlantLocalData ToLocalData(Cmd.PlantInfo plantInfo)
    {
        PlantManager manager = PlantManager.Instance;
        manager.plantSeedItemIdDic.TryGetValue((int)plantInfo.PlantSeed, out var plantData);
        long adjustedPlantTime = plantInfo.PlantTime;
        if (manager.plantCacheTimes.TryGetValue(plantInfo.UID, out var cacheTime))
        {
            adjustedPlantTime = Math.Max(plantInfo.PlantTime - (long)(Time.realtimeSinceStartupAsDouble - cacheTime), 0L);
        }
        return new PlantLocalData
        {
            uid = plantInfo.UID.ToString(),
            potUid = plantInfo.PotUID,
            plantId = plantData.plantId,
            variantKey = plantInfo.VariantKey,
            growStage = plantInfo.GrowStage,
            growStage2 = plantInfo.GrowStage2,
            plantTime = adjustedPlantTime,
            harvestTime = plantInfo.HarvestTime,
            isFertilize = plantInfo.IsFertilize
        };
    }
    #region 植物交互
    /// <summary>
    /// 浇水
    /// </summary>
    public void WaterPlant()
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            NetWork_Center_WSS.SendMsg(new Cmd.PlantOpReq
            {
                Op = 1
            });
        }
    }
    /// <summary>
    /// 更新
    /// </summary>
    public void UpdatePlant()
    {
        if(updateTimer < 0)
        {
            updateTimer = 3;
        }
    }   
    /// <summary>
    /// 种植
    /// </summary>
    /// <param name="seedName"></param>
    public void PlantPlant(string seedName, string potUid)
    {
        var seedItem = Global_Inventory_Manager.GetItemInfo(seedName);
        if(NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            NetWork_Center_WSS.SendMsg(new Cmd.PlantReq
            {
                SeedID = seedItem.item_id,
                PotUID = potUid
            });
        }
    }
    /// <summary>
    /// 收获
    /// </summary>
    /// <param name="plant"></param>
    public void HarvestPlant(Plant plant)
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            NetWork_Center_WSS.SendMsg(new Cmd.PlantOpReq
            {
                Op = 3,
                PlantUID = ulong.Parse(plant.data.uid)
            });
        }
    }
    /// <summary>
    /// 移除
    /// </summary>
    /// <param name="plant"></param>
    public void RemovePlant(Plant plant)
    {
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            NetWork_Center_WSS.SendMsg(new Cmd.PlantOpReq
            {
                Op = 4,
                PlantUID = ulong.Parse(plant.data.uid)
            });
        }
    }
    public void FertilizePlant(Plant plant, string fertilizerName)
    {
        var fertilizerItem = Global_Inventory_Manager.GetItemInfo(fertilizerName);
        if (NetWork_Center_WSS.IsConnectedToPlayerServer)
        {
            NetWork_Center_WSS.SendMsg(new Cmd.PlantOpReq
            {
                Op = 2,
                ItemID = fertilizerItem.item_id,
                PlantUID = ulong.Parse(plant.data.uid)
            });
        }
    }
    private bool TryCreatePlant(PlantLocalData data, out Plant plant)
    {
        GameObject obj = Instantiate(plantPrefab);
        plant = obj.GetComponent<Plant>();
        return plant.Init(data);
    }
    #endregion

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
