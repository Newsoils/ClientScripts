using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using TMPro;
using UnityEngine;

public class PotPanel : MonoBehaviour
{
    public GameObject parent;
    public Vector3 offset;
    private Plant plant;
    private PlantData data;
    public TMP_Text timeText;
    public TMP_Text stageText;
    private void Start()
    {
        EvtDsp.AddEvt(EvtNames.OnClickNothing, DestroySelf);
    }
    private void OnDestroy()
    {
        EvtDsp.RemoveEvt(EvtNames.OnClickNothing, DestroySelf);
    }
    private void DestroySelf()
    {
        Destroy(gameObject);
    }
    public void Init(Pot pot)
    {
        parent = pot.gameObject;
        plant = PlantManager.Instance.GetPlantByPot(pot);
        data = PlantManager.Instance.plantDatas[plant.data.plantId];
    }
    private void LateUpdate()
    {
        if (parent == null) return;
        Vector3 position = Camera.main.WorldToScreenPoint(parent.transform.position + offset);
        transform.position = position;
        timeText.text = GetTimeText();
        int stageCount = data.lifeCycle.Where(x => x > 0).ToList().Count;
        int curStage = data.lifeCycle.Take(plant.data.growStage + 1).Where(x => x > 0).ToList().Count;
        stageText.text = "阶段 " + curStage + "/" + stageCount;
    }
    private string GetTimeText()
    {
        if(plant.data.growStage == 4)
        {
            return "--:--";
        }
        else
        {
            int minutes = Mathf.Max((int)plant.nextStageTime, 0);
            int seconds = Mathf.Max((int)((plant.nextStageTime - minutes) * 60), 0);
            return minutes + ":" + seconds;
        }
    }
}
