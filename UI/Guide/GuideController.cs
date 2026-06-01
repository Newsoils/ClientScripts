using System.Collections;
using CLIP.Framework_Core.Event;
using UnityEngine;

[RequireComponent(typeof(GuidePanel))]
public class GuideController : MonoBehaviour
{
    private const float StepWaitTime = 2f;

    private GuidePanel panel;
    private GuideStep currentStep;
    private Coroutine waitForButtonCoroutine;
    private float curStepTime;

    private void Awake()
    {
        panel = GetComponent<GuidePanel>();
    }

    private void Start()
    {
        EvtDsp.AddEvt(EvtNames.Guide_Open_Panel, OnGuideOpen);
        EvtDsp.AddEvt(EvtNames.Guide_Close_Panel, OnGuideClose);
        EvtDsp.AddEvt<GuideStepChangedEvent>(EvtNames.Guide_Step_Changed, OnGuideStepChanged);
        EvtDsp.AddEvt(EvtNames.Guide_Clear_View, ClearView);
        EvtDsp.AddReturnEvt<bool>(EvtNames.Guide_Is_View_Ready, IsViewReady);

        panel.ClosePanel();
    }

    private void OnDestroy()
    {
        EvtDsp.RemoveEvt(EvtNames.Guide_Open_Panel, OnGuideOpen);
        EvtDsp.RemoveEvt(EvtNames.Guide_Close_Panel, OnGuideClose);
        EvtDsp.RemoveEvt<GuideStepChangedEvent>(EvtNames.Guide_Step_Changed, OnGuideStepChanged);
        EvtDsp.RemoveEvt(EvtNames.Guide_Clear_View, ClearView);
        EvtDsp.RemoveReturnEvt<bool>(EvtNames.Guide_Is_View_Ready, IsViewReady);
    }

    private bool IsViewReady()
    {
        return panel != null && panel.isActiveAndEnabled;
    }

    private void OnGuideOpen()
    {
        panel.OpenPanel();
    }

    private void OnGuideClose() => panel.ClosePanel();



    private void OnGuideStepChanged(GuideStepChangedEvent evt)
    {
        StopWaitForButtonCoroutine();

        if (currentStep != null)
        {
            currentStep.OnStepCompleted -= OnCurrentStepCompleted;
            currentStep.Hide();
        }

        currentStep = panel.GetStep(evt.GuideId, evt.StepId);
        if (currentStep == null)
        {
            Debug.LogWarning($"[GuideController] Missing GuideStep. guide={evt.GuideId}, step={evt.StepId}");
            GuideManager.Instance?.NextStep();
            return;
        }

        ShowStep(currentStep);
    }

    private void ShowStep(GuideStep step)
    {
        step.OnStepCompleted -= OnCurrentStepCompleted;
        step.OnStepCompleted += OnCurrentStepCompleted;

        if (step.ResolveListenButton != null)
            waitForButtonCoroutine = StartCoroutine(WaitForButtonAndShow(step));
        else
            step.Show(panel.Mask);
    }

    private IEnumerator WaitForButtonAndShow(GuideStep step)
    {
        step.ResolveListenButton?.Invoke(step);
        while (!step.IsReady && curStepTime < StepWaitTime)
        {
            yield return new WaitForSeconds(0.5f);
            curStepTime += 0.3f;
            step.ResolveListenButton?.Invoke(step);
        }

        curStepTime = 0f;
        step.Show(panel.Mask);
    }

    private void OnCurrentStepCompleted()
    {
        GuideManager.Instance?.NextStep();
    }

    private void StopWaitForButtonCoroutine()
    {
        if (waitForButtonCoroutine == null)
            return;

        StopCoroutine(waitForButtonCoroutine);
        waitForButtonCoroutine = null;
        curStepTime = 0f;
    }

    private void ClearView()
    {
        StopWaitForButtonCoroutine();

        if (currentStep != null)
        {
            currentStep.OnStepCompleted -= OnCurrentStepCompleted;
            currentStep = null;
        }

        panel.HideAllSteps();
    }
}
