using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using Cmd;
using Google.Protobuf;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GuideState
{
    public bool dispatch1finished = false;
    public bool dispatch2Finished = false;
    public bool placementFinished = false;
    public bool shopFinished = false;
}

public class GuideManager : SingletonMono<GuideManager>
{
    private const string DefaultGuideDefinitionResourcePath = "Guide/GuideDefinition_SO";

    [SerializeField]
    private GuideDefinition_SO guideDefinitionSO;

    [SerializeField]
    private List<GuideDefinition> guideDefinitions = new List<GuideDefinition>();
    public bool IsShowingSteps { get; private set; }
    public GuideId? CurrentGuideId { get; private set; }
    public string CurrentStepId { get; private set; }
    public int CurrentStepIndex { get; private set; } = -1;

    public GuideState guideState = new GuideState();

    private readonly Dictionary<GuideId, GuideDefinition> definitionMap = new Dictionary<GuideId, GuideDefinition>();
    private GuideDefinition currentDefinition;
    private Coroutine pendingMainSceneGuideCheck;

    protected override void Awake()
    {
        base.Awake();
        LoadGuideDefinitionAsset();
        RebuildDefinitionMap();
    }

    private void Start()
    {
        if (guideState == null)
        {
            guideState = new GuideState();
        }
       

        EvtDsp.AddEvt(EvtNames.Guide_Skip_Current, SkipCurrentGuide);
        SceneManager.sceneLoaded += OnSceneLoaded;

        RequestMainSceneGuideCheck(SceneManager.GetActiveScene());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EvtDsp.RemoveEvt(EvtNames.Guide_Skip_Current, SkipCurrentGuide);
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (pendingMainSceneGuideCheck != null)
        {
            StopCoroutine(pendingMainSceneGuideCheck);
            pendingMainSceneGuideCheck = null;
        }
    }

    public void SetGuideDefinition(GuideDefinition definition)
    {
        if (definition == null)
            return;

        definitionMap[definition.Id] = definition;

        int index = guideDefinitions.FindIndex(item => item.Id == definition.Id);
        if (index >= 0)
            guideDefinitions[index] = definition;
        else
            guideDefinitions.Add(definition);
    }


    public void StartGuide()
    {
        StartGuide(GuideId.Dispatch);
    }

    public void StartGuide(GuideId guideId)
    {
        if (!definitionMap.TryGetValue(guideId, out currentDefinition) ||
            currentDefinition.StepIds == null ||
            currentDefinition.StepIds.Count == 0)
        {
            Debug.LogWarning($"[GuideManager] Guide definition is missing or empty: {guideId}");
            return;
        }

        IsShowingSteps = true;
        CurrentGuideId = guideId;
        CurrentStepId = null;
        CurrentStepIndex = -1;

        EvtDsp.TriggerEvt(EvtNames.Guide_Open_Panel);
        NextStep();
    }

    public void NextStep()
    {
        if (!IsShowingSteps || currentDefinition == null)
            return;

        CurrentStepIndex++;
        if (CurrentStepIndex >= currentDefinition.StepIds.Count)
        {
            CompleteCurrentGuide();
            return;
        }

        CurrentStepId = currentDefinition.StepIds[CurrentStepIndex];
        EvtDsp.TriggerEvt(EvtNames.Guide_Step_Changed,
            new GuideStepChangedEvent(currentDefinition.Id, CurrentStepId, CurrentStepIndex));
    }

    public bool IsCurrentStep(string stepId)
    {
        return IsShowingSteps && !string.IsNullOrEmpty(stepId) && CurrentStepId == stepId;
    }

    public void SkipCurrentGuide()
    {
        if (!IsShowingSteps) return;


        if (CurrentGuideId == GuideId.DispatchReturn && CurrentStepIndex <= 2)
        {
            EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, new CatBackImmediatelyReq());
        }
        CompleteCurrentGuide();
    }

    private void CompleteCurrentGuide()
    {
        EvtDsp.TriggerEvt(EvtNames.Guide_Clear_View);

        IsShowingSteps = false;
        CurrentGuideId = null;
        CurrentStepId = null;
        CurrentStepIndex = -1;
        currentDefinition = null;

        UpdateFinished();
        EvtDsp.TriggerEvt(EvtNames.Guide_Close_Panel);
    }

    private void RebuildDefinitionMap()
    {
        if ((guideDefinitions == null || guideDefinitions.Count == 0) && guideDefinitionSO != null)
            guideDefinitions = guideDefinitionSO.guideDefinitions;

        definitionMap.Clear();
        foreach (var definition in guideDefinitions)
        {
            if (definition != null)
                definitionMap[definition.Id] = definition;
        }
    }

    private void LoadGuideDefinitionAsset()
    {
        if (guideDefinitionSO != null)
            return;

        guideDefinitionSO = Resources.Load<GuideDefinition_SO>(DefaultGuideDefinitionResourcePath);
        if (guideDefinitionSO == null)
            Debug.LogWarning($"[GuideManager] GuideDefinition_SO not found in Resources/{DefaultGuideDefinitionResourcePath}.");
    }

    private void UpdateFinished()
    {
        if (!guideState.dispatch1finished)
        {
            guideState.dispatch1finished = true;
            Dispatch_Manager._instance.force_dispatch_start();
        }
        else if (guideState.dispatch1finished && !guideState.dispatch2Finished)
        {
            guideState.dispatch2Finished = true;
        }
        else if (guideState.dispatch1finished && guideState.dispatch2Finished && !guideState.placementFinished)
        {
            guideState.placementFinished = true;
        }
        else if (guideState.dispatch1finished && guideState.dispatch2Finished && guideState.placementFinished && !guideState.shopFinished)
        {
            guideState.shopFinished = true;
        }

        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, new SaveGuideReq() { GuideData = JsonConvert.SerializeObject(guideState) });
    }

    public void CheckFurnitureFinished()
    {
        if (SceneManager.GetActiveScene().name != SceneLoadHelper.MainSceneName || IsShowingSteps)
            return;

        if (guideState.dispatch1finished && guideState.dispatch2Finished && !guideState.placementFinished)
            StartGuide(GuideId.MoveFurniture);
    }

    public void CheckShopFinished()
    {
        if (SceneManager.GetActiveScene().name != SceneLoadHelper.MainSceneName || IsShowingSteps)
            return;

        if (guideState.dispatch1finished && guideState.dispatch2Finished && guideState.placementFinished && !guideState.shopFinished)
            StartGuide(GuideId.Shop);
    }

    private void OnSceneLoaded(Scene _s, LoadSceneMode _m)
    {
        RequestMainSceneGuideCheck(_s);
    }

    private void RequestMainSceneGuideCheck(Scene scene)
    {
        EvtDsp.TriggerEvt<IMessage>(EvtNames.Send_Req_To_Server, new GetGuideReq());
        if (scene.name != SceneLoadHelper.MainSceneName)
            return;

        if (pendingMainSceneGuideCheck != null)
            StopCoroutine(pendingMainSceneGuideCheck);

        pendingMainSceneGuideCheck = StartCoroutine(CheckMainSceneGuideWhenViewReady());
    }

 
    private IEnumerator CheckMainSceneGuideWhenViewReady()
    {
        yield return null;

        while (SceneManager.GetActiveScene().name == SceneLoadHelper.MainSceneName &&
               !EvtDsp.ReturnEvt<bool>(EvtNames.Guide_Is_View_Ready))
        {
            yield return null;
        }

        pendingMainSceneGuideCheck = null;

        if (SceneManager.GetActiveScene().name == SceneLoadHelper.MainSceneName && !IsShowingSteps)
            CheckGuideState();
    }


    private void CheckGuideState()
    {
        var firstInfoFlag = Global_Game_Manager.Instance._current_player_role_info.FirstInfoFlag;
        bool firstRewardClaimed = (firstInfoFlag & (1 << 0)) != 0;

        //如果没领新手奖励，直接退出（新手引导所用物品依赖新手奖励）
        if (!firstRewardClaimed ) return;
        if (!hasDataReceived) return;
        if (!guideState.dispatch1finished)
        {
            StartGuide(GuideId.Dispatch);
        }
        else if (guideState.dispatch1finished && !guideState.dispatch2Finished)
        {
            StartGuide(GuideId.DispatchReturn);
        }
        else if (guideState.dispatch1finished && guideState.dispatch2Finished && !guideState.placementFinished)
        {
            StartGuide(GuideId.MoveFurniture);
        }
    }

    bool hasDataReceived = false;

    /// <summary>
    /// 收到新手教程历史记录
    /// </summary>
    public void ReceiveGuideHistory(string json)
    {
        hasDataReceived = true;
        guideState = JsonConvert.DeserializeObject<GuideState>(json);
        if (guideState == null)
        {
            guideState = new GuideState();
        }
        if (SceneManager.GetActiveScene().name == SceneLoadHelper.MainSceneName && !IsShowingSteps)
            CheckGuideState();
    }

    /// <summary>
    /// 保存成功
    /// </summary>
    public void ReceiveGuideSaveSuccess()
    {
    }
}
