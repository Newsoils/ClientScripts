using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum GuideId
{
    Dispatch,
    DispatchReturn,
    MoveFurniture,
    Shop
}

[Serializable]
public class GuideDefinition
{
    [Title("引导ID"), GUIColor(0.1f, 0.8f, 1f)]
    [LabelText("引导ID")]
    public GuideId Id;

    [LabelText("步骤ID列表")]
    public List<string> StepIds = new List<string>();
}


public class GuideStepChangedEvent
{
    public GuideId GuideId;
    public string StepId;
    public int StepIndex;

    public GuideStepChangedEvent(GuideId guideId, string stepId, int stepIndex)
    {
        GuideId = guideId;
        StepId = stepId;
        StepIndex = stepIndex;
    }
}
