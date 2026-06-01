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
        int stageCount = GetStageCount();
        int curStage = GetCurrentStage(stageCount);
        stageText.text = "阶段 " + curStage + "/" + stageCount;
    }
    private int GetStageCount()
    {
        return data.lifeCycle.Count(x => x > 0);
    }
    private int GetCurrentStage(int stageCount)
    {
        if (plant.data.growStage == 1)
        {
            return Mathf.Clamp(data.lifeCycle.Take(plant.data.growStage2).Count(x => x > 0), 1, stageCount);
        }
        return stageCount;
    }
    private string GetTimeText()
    {
        if(plant.data.growStage != 1)
        {
            return "--:--";
        }
        else
        {
            float nextStageTime = plant.GetNextStageRemainingMinutes();
            int minutes = Mathf.Max((int)nextStageTime, 0);
            int seconds = Mathf.Max((int)((nextStageTime - minutes) * 60), 0);
            return minutes + ":" + seconds;
        }
    }
}
