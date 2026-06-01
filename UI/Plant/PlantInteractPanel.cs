using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.UI;

public class PlantInteractPanel : UIPanelBase
{
    private enum PopType
    {
        None,
        Seed,
        Fertilizer,
        Harvest,
        Remove,
        PotState
    }

    public GameObject panelObj;
    public GameObject rightButtonRoot;
    public Button BtnRemove;
    public Button BtnHarvest;
    public Button BtnWater;
    public Button BtnRemoveGrass;
    public Button BtnEdit;
    public Button BtnShop;
    public GameObject popSeedPrefab;
    public GameObject popFertilizerPrefab;
    public GameObject popHarvestPrefab;
    public GameObject popRemovePrefab;
    public GameObject potPanelPrefab;
    private List<GameObject> pops = new List<GameObject>();
    private PopType currentPopType = PopType.None;
    private PlantData currentSeedData;
    private Pot currentPot;
    
    private void Start()
    {
        BtnRemove.onClick.AddListener(() => EditManager.Instance.SetMode(new RemovePlantMode()));
        BtnHarvest.onClick.AddListener(() => EditManager.Instance.SetMode(new HarvestPlantMode()));
        BtnWater.onClick.AddListener(() =>
        {
            EditManager.Instance.SetMode(new WaterPlantMode());
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, "现在手指可以在屏幕上划动，给植物浇水！");
        });
        BtnEdit.onClick.AddListener(() => UIManager.Instance.OpenPanel<PlantPanel>());
        BtnShop.onClick.AddListener(() => EvtDsp.TriggerEvt(EvtNames.OpenShoppingPanel));
        EvtDsp.AddEvt<PlantData>(EvtNames.ShowSeedPop, ShowSeedPops);
        EvtDsp.AddEvt(EvtNames.ShowFertilizerPop, ShowFertilizerPops);
        EvtDsp.AddEvt(EvtNames.ShowRemovePop, ShowRemovePops);
        EvtDsp.AddEvt(EvtNames.ShowHarvestPop, ShowHarvestPops);
        EvtDsp.AddEvt<Pot>(EvtNames.ShowPotState, ShowPotPanel);
        EvtDsp.AddEvt(EvtNames.ClosePlantPop, ClosePlantPops);
        EvtDsp.AddEvt(EvtNames.ReloadPlantData, RefreshCurrentPops);

        EvtDsp.AddEvt(EvtNames.OnPhonePanelOpen, ClosePanel);
        ClosePanel();
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        BtnRemove.onClick.RemoveAllListeners();
        BtnHarvest.onClick.RemoveAllListeners();
        BtnWater.onClick.RemoveAllListeners();
        BtnRemoveGrass.onClick.RemoveAllListeners();
        BtnShop.onClick.RemoveAllListeners();
        EvtDsp.RemoveEvt<PlantData>(EvtNames.ShowSeedPop, ShowSeedPops);
        EvtDsp.RemoveEvt(EvtNames.ShowFertilizerPop, ShowFertilizerPops);
        EvtDsp.RemoveEvt(EvtNames.ShowRemovePop, ShowRemovePops);
        EvtDsp.RemoveEvt(EvtNames.ShowHarvestPop, ShowHarvestPops);
        EvtDsp.RemoveEvt<Pot>(EvtNames.ShowPotState, ShowPotPanel);
        EvtDsp.RemoveEvt(EvtNames.ClosePlantPop, ClosePlantPops);
        EvtDsp.RemoveEvt(EvtNames.ReloadPlantData, RefreshCurrentPops);

        EvtDsp.RemoveEvt(EvtNames.OnPhonePanelOpen, ClosePanel);
    }
    
    public override void ClosePanel()
    {
        panelObj.SetActive(false);
        EditManager.Instance.ExitCurrentMode();
    }
    public void SetRight(bool value)
    {
        rightButtonRoot.SetActive(value);
        BtnEdit.gameObject.SetActive(value);
    }
    public override void OpenPanel(params object[] data)
    {
        ClosePlantPops();
        panelObj.SetActive(true);
    }
    private void ShowSeedPops(PlantData data)
    {
        currentPopType = PopType.Seed;
        currentSeedData = data;
        currentPot = null;
        ClearPops();
        var potList = RoomSystem.currentRoom.pots;
        foreach(var pot in potList)
        {
            if(PlantManager.Instance.GetPlantByPot(pot) == null && pot.CheckPlant(data))
            {
                GameObject obj = Instantiate(popSeedPrefab, panelObj.transform);
                obj.GetComponent<PotPop>().Init(pot);
                pops.Add(obj);
            }
        }
    }

    private void ShowFertilizerPops()
    {
        currentPopType = PopType.Fertilizer;
        currentSeedData = null;
        currentPot = null;
        ClearPops();
        var potList = RoomSystem.currentRoom.pots;
        foreach (var pot in potList)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null && !plant.data.isFertilize && plant.data.growStage == 1)
            {
                GameObject obj = Instantiate(popFertilizerPrefab, panelObj.transform);
                obj.GetComponent<PotPop>().Init(pot);
                pops.Add(obj);
            }
        }
    }
    private void ShowHarvestPops()
    {
        currentPopType = PopType.Harvest;
        currentSeedData = null;
        currentPot = null;
        ClearPops();
        var potList = RoomSystem.currentRoom.pots;
        foreach (var pot in potList)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null && plant.data.growStage == 2)
            {
                GameObject obj = Instantiate(popHarvestPrefab, panelObj.transform);
                obj.GetComponent<PotPop>().Init(pot);
                pops.Add(obj);
            }
        }
    }
    private void ShowRemovePops()
    {
        currentPopType = PopType.Remove;
        currentSeedData = null;
        currentPot = null;
        ClearPops();
        var potList = RoomSystem.currentRoom.pots;
        foreach (var pot in potList)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null)
            {
                GameObject obj = Instantiate(popRemovePrefab, panelObj.transform);
                obj.GetComponent<PotPop>().Init(pot);
                pops.Add(obj);
            }
        }
    }
    private void ShowPotPanel(Pot pot)
    {
        currentPopType = PopType.PotState;
        currentSeedData = null;
        currentPot = pot;
        ClearPops();
        Plant plant = PlantManager.Instance.GetPlantByPot(pot);
        if (plant != null)
        {
            GameObject obj = Instantiate(potPanelPrefab, panelObj.transform);
            obj.GetComponent<PotPanel>().Init(pot);
            pops.Add(obj);
        }
    }
    private void RefreshCurrentPops()
    {
        switch (currentPopType)
        {
            case PopType.Seed:
                if (currentSeedData != null)
                    ShowSeedPops(currentSeedData);
                break;
            case PopType.Fertilizer:
                ShowFertilizerPops();
                break;
            case PopType.Harvest:
                ShowHarvestPops();
                break;
            case PopType.Remove:
                ShowRemovePops();
                break;
            case PopType.PotState:
                if (currentPot != null)
                    ShowPotPanel(currentPot);
                break;
        }
    }
    private void ClosePlantPops()
    {
        currentPopType = PopType.None;
        currentSeedData = null;
        currentPot = null;
        ClearPops();
    }
    private void ClearPops()
    {
        foreach (var obj in pops)
        {
            Destroy(obj.gameObject);
        }
        pops.Clear();
    }
}
