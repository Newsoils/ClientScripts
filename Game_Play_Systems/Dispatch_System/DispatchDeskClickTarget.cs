using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using UnityEngine;

public class DispatchDeskClickTarget : MonoBehaviour, IClick, IPrioritizedClick
{
    public DispatchDeskController controller;
    public DispatchDeskClickAction action;
    public string clickableLayerName = "Placement";
    public int ClickPriority =>
        action == DispatchDeskClickAction.SwitchToLeftPage || action == DispatchDeskClickAction.SwitchToRightPage
            ? 0
            : 10;

    private void Awake()
    {
        int clickableLayer = LayerMask.NameToLayer(clickableLayerName);
        if (clickableLayer < 0)
            return;

        gameObject.layer = clickableLayer;
        foreach (var targetCollider in GetComponentsInChildren<Collider>(true))
            targetCollider.gameObject.layer = clickableLayer;
    }

    public bool OnClick(Vector3 position)
    {
        if (controller == null)
            controller = GetComponentInParent<DispatchDeskController>();

        if (controller == null)
            return false;

        controller.HandleDeskClick(action);
        return true;
    }
}

public enum DispatchDeskClickAction
{
    SelectFood,
    SelectSnack,
    SwitchToLeftPage,
    SwitchToRightPage,
    ToggleCdList
}
