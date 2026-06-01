using System;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using UnityEngine;
using UnityEngine.UI;

public class GuidePanel : UIPanelBase
{
    public Transform dispatchGuide;
    public Transform placementGuide;
    public Transform dispatchReturnGuide;
    public Transform shopGuide;

    [Tooltip("GuideStep groups. These lists are view data and are converted to GuideDefinition by GuideController.")]
    public List<GuideStep> dispatchSteps = new List<GuideStep>();
    public List<GuideStep> dispatchReturnSteps = new List<GuideStep>();
    public List<GuideStep> placementSteps = new List<GuideStep>();
    public List<GuideStep> shopSteps = new List<GuideStep>();

    [Header("UI Bindings")]
    public GameObject Root;
    public Button skipButton;
    public GuideMask mask;

    public GuideMask Mask => mask;

    private void Start()
    {
        HideAllSteps();
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipButtonClick);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (skipButton != null)
            skipButton.onClick.RemoveListener(OnSkipButtonClick);
    }

    public override void OpenPanel(params object[] data) => SetPanelActive(true);

    public override void ClosePanel() => SetPanelActive(false);

    private void OnSkipButtonClick() => EvtDsp.TriggerEvt(EvtNames.Guide_Skip_Current);

    private void SetPanelActive(bool active)
    {
        (Root ?? gameObject).SetActive(active);
        if (skipButton != null)
            skipButton.gameObject.SetActive(active);
    }

    public void StartDispatchGuide() => GuideManager.Instance?.StartGuide(GuideId.Dispatch);

    public void StartMoveFurnitureGuide() => GuideManager.Instance?.StartGuide(GuideId.MoveFurniture);

    public void StartDispatchReturnGuide() => GuideManager.Instance?.StartGuide(GuideId.DispatchReturn);

    public void StartShopGuide() => GuideManager.Instance?.StartGuide(GuideId.Shop);

    public GuideStep GetStep(GuideId guideId, string stepId)
    {
        var steps = GetSteps(guideId);
        if (steps == null || string.IsNullOrEmpty(stepId))
            return null;

        foreach (var step in steps)
        {
            if (step != null && step.StepId == stepId)
                return step;
        }

        return null;
    }

    public List<GuideStep> GetSteps(GuideId guideId)
    {
        switch (guideId)
        {
            case GuideId.Dispatch:
                return dispatchSteps;
            case GuideId.DispatchReturn:
                return dispatchReturnSteps;
            case GuideId.MoveFurniture:
                return placementSteps;
            case GuideId.Shop:
                return shopSteps;
            default:
                return null;
        }
    }

    public void HideAllSteps()
    {
        foreach (var step in dispatchSteps)
            step?.Hide();
        foreach (var step in dispatchReturnSteps)
            step?.Hide();
        foreach (var step in placementSteps)
            step?.Hide();
        foreach (var step in shopSteps)
            step?.Hide();

        mask?.Hide();
    }

   
}
