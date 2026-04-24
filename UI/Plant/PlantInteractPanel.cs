using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using UnityEngine;
using UnityEngine.UI;

public class PlantInteractPanel : UIPanelBase
{
    public GameObject panelObj;
    public GameObject rightButtonRoot;
    public Button BtnRemove;
    public Button BtnHarvest;
    public Button BtnWater;
    public Button BtnRemoveGrass;
    public Button BtnEdit;
    public GameObject popSeedPrefab;
    public GameObject popFertilizerPrefab;
    public GameObject popHarvestPrefab;
    public GameObject popRemovePrefab;
    public GameObject potPanelPrefab;
    private List<GameObject> pops = new List<GameObject>();
    private float waterTime;
    
    private void Start()
    {
        BtnRemove.onClick.AddListener(() => EditManager.Instance.SetMode(new RemovePlantMode()));
        BtnHarvest.onClick.AddListener(() => EditManager.Instance.SetMode(new HarvestPlantMode()));
        BtnWater.onClick.AddListener(() =>
        {
            EditManager.Instance.SetMode(new WaterPlantMode());
            EvtDsp.TriggerEvt<string>(EvtNames.ShowUpPrompt, "现在手指可以在屏幕上划动，给植物浇水");
        });
        //BtnRemoveGrass.onClick.AddListener(() => ChangeState(PlantState.RemoveGrass));
        BtnEdit.onClick.AddListener(() => UIManager.Instance.OpenPanel<PlantPanel>());
        EvtDsp.AddEvt<PlantData>(EvtNames.ShowSeedPop, ShowSeedPops);
        EvtDsp.AddEvt(EvtNames.ShowFertilizerPop, ShowFertilizerPops);
        EvtDsp.AddEvt(EvtNames.ShowRemovePop, ShowRemovePops);
        EvtDsp.AddEvt(EvtNames.ShowHarvestPop, ShowHarvestPops);
        EvtDsp.AddEvt<Pot>(EvtNames.ShowPotState, ShowPotPanel);
        EvtDsp.AddEvt(EvtNames.ClosePlantPop, ClosePlantPops);

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
        EvtDsp.RemoveEvt<PlantData>(EvtNames.ShowSeedPop, ShowSeedPops);
        EvtDsp.RemoveEvt(EvtNames.ShowFertilizerPop, ShowFertilizerPops);
        EvtDsp.RemoveEvt(EvtNames.ShowRemovePop, ShowRemovePops);
        EvtDsp.RemoveEvt(EvtNames.ShowHarvestPop, ShowHarvestPops);
        EvtDsp.RemoveEvt<Pot>(EvtNames.ShowPotState, ShowPotPanel);
        EvtDsp.RemoveEvt(EvtNames.ClosePlantPop, ClosePlantPops);

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
        panelObj.SetActive(true);
    }
    private void ShowSeedPops(PlantData data)
    {
        foreach(var obj in pops)
        {
            Destroy(obj.gameObject);
        }
        pops.Clear();
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
        foreach (var obj in pops)
        {
            Destroy(obj.gameObject);
        }
        pops.Clear();
        var potList = RoomSystem.currentRoom.pots;
        foreach (var pot in potList)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null && !plant.data.isFertilize && plant.data.growStage < 4)
            {
                GameObject obj = Instantiate(popFertilizerPrefab, panelObj.transform);
                obj.GetComponent<PotPop>().Init(pot);
                pops.Add(obj);
            }
        }
    }
    private void ShowHarvestPops()
    {
        foreach (var obj in pops)
        {
            Destroy(obj.gameObject);
        }
        pops.Clear();
        var potList = RoomSystem.currentRoom.pots;
        foreach (var pot in potList)
        {
            Plant plant = PlantManager.Instance.GetPlantByPot(pot);
            if (plant != null && plant.data.growStage == 4)
            {
                GameObject obj = Instantiate(popHarvestPrefab, panelObj.transform);
                obj.GetComponent<PotPop>().Init(pot);
                pops.Add(obj);
            }
        }
    }
    private void ShowRemovePops()
    {
        foreach (var obj in pops)
        {
            Destroy(obj.gameObject);
        }
        pops.Clear();
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
        foreach (var obj in pops)
        {
            Destroy(obj.gameObject);
        }
        pops.Clear();
        Plant plant = PlantManager.Instance.GetPlantByPot(pot);
        if (plant != null)
        {
            GameObject obj = Instantiate(potPanelPrefab, panelObj.transform);
            obj.GetComponent<PotPanel>().Init(pot);
            pops.Add(obj);
        }
    }
    private void ClosePlantPops()
    {
        foreach (var obj in pops)
        {
            Destroy(obj.gameObject);
        }
        pops.Clear();
    }
}
