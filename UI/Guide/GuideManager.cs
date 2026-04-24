using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GuideState
{
    public bool current1finished = false;
    public bool dispatch2Finished = false;
    public bool moveFurntureFinished = false;
    public bool shopSteps4Finished = false;
}

public class GuideManager : SingletonMono<GuideManager>
{
    public GuideMask mask;
    public List<GuideStep> currentTargets = new List<GuideStep>();

    public List<GuideStep> dispatchReturnSteps;

    public List<GuideStep> moveFurntureSteps;

    public List<GuideStep> shopSteps;

    private bool IsShowingSteps = false;

    private Queue<GuideStep> steps = new Queue<GuideStep>();

    [HideInInspector]
    public GuideStep currentStep;

    public GuideState guideState = new GuideState();
    public static string guideStateFile = "guideStateFile.json";

    private void OnSceneLoaded(Scene _s, LoadSceneMode _m)
    {
        if (IsShowingSteps) return;

        if (SceneLoadHelper.IsMainScene)
        {
            if (guideState.current1finished && !guideState.dispatch2Finished)
            {
                StartDispatchReturnGuide();
            }
            else if (guideState.current1finished && guideState.dispatch2Finished && !guideState.moveFurntureFinished)
            {
                StartMoveFurnitureGuide();
            }
        }
    }

    private void Start()
    {
        guideState = new GuideState();
        foreach (var each in currentTargets)
        {
            each.Hide();
        }
        foreach (var each in dispatchReturnSteps)
        {
            each.Hide();
        }
        foreach (var each in moveFurntureSteps)
        {
            each.Hide();
        }
        foreach (var each in shopSteps)
        {
            each.Hide();
        }
        mask.Hide();

        SceneManager.sceneLoaded += OnSceneLoaded;

        guideState = Save_Load_Tools.Load<GuideState>(guideStateFile);
        if (guideState == null)
        {
            guideState = new GuideState();
        }

        //StartDispatchGuide();
        //StartMoveFurnitureGuide();
        //StartDispatchReturnGuide();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void StartDispatchGuide() => StartGuide(currentTargets);

    public void StartDispatchReturnGuide() => StartGuide(dispatchReturnSteps);

    public void StartMoveFurnitureGuide() => StartGuide(moveFurntureSteps);

    public void StartShopGuide() => StartGuide(shopSteps);


    public void StartGuide()
    {
        StartGuide(currentTargets);
    }

    public void StartGuide(List<GuideStep> guideSteps)
    {
        IsShowingSteps = true;
        steps.Clear();

        foreach (var step in guideSteps)
            steps.Enqueue(step);

        NextStep();
    }

    public void NextStep()
    {
        if (currentStep != null)
        {
            currentStep.Hide();
        }

        if (steps.Count == 0)
        {
            UpdateFinished();
            mask.Hide();
            IsShowingSteps = false;
            return;
        }

        currentStep = steps.Dequeue();
        ShowStep(currentStep);
    }

    private void UpdateFinished()
    {
        if (!guideState.current1finished)
        {
            guideState. current1finished = true;
            Dispatch_Manager._instance.force_dispatch_start();
        }
        else if (guideState.current1finished && !guideState.dispatch2Finished)
        {
            guideState.dispatch2Finished = true;
        }
        else if (guideState.current1finished && guideState.dispatch2Finished && !guideState.moveFurntureFinished)
        {
            guideState.moveFurntureFinished = true;
        }
        else if (guideState.current1finished && guideState.dispatch2Finished && guideState.moveFurntureFinished && !guideState.shopSteps4Finished)
        {
            guideState. shopSteps4Finished = true;
        }

        Save_Load_Tools.Save(guideStateFile, guideState);
    }


    public void CheckFurnitureFinished()
    {
        if(!guideState.moveFurntureFinished)
        {
            StartMoveFurnitureGuide();
        }
    }

    public void CheckShopFinished()
    {
        if(!guideState.shopSteps4Finished)
        {
            StartShopGuide();
        }
    }
    public void ShowStep(GuideStep step)
    {
        if (step.ResolveListenButton != null)
        {
            StartCoroutine(WaitForButtonAndShow(step));
        }
    }

    private float curStepTime = 0f;
    private float stepWaitTime = 2f;

    public IEnumerator WaitForButtonAndShow(GuideStep step)
    {
        step.ResolveListenButton?.Invoke(step);
        while (!step.IsReady && curStepTime < stepWaitTime)
        {
            yield return new WaitForSeconds(0.5f);
            curStepTime += 0.3f;
            step.ResolveListenButton?.Invoke(step);
        }
        curStepTime = 0;
        step.Show();
       
    }

   

}