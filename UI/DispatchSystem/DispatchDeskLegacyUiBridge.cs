using CLIP.Framework_Core.Event;
using UnityEngine;

public class DispatchDeskLegacyUiBridge : MonoBehaviour
{
    private DispatchPanel dispatchPanel;

    private void OnEnable()
    {
        EvtDsp.AddEvt<DispatchDeskClickAction>(EvtNames.DispatchDesk_Click_Action, OnDeskClickAction);
        EvtDsp.AddEvt<string>(EvtNames.DispatchDesk_Select_Tape, OnSelectTape);
        EvtDsp.AddEvt<Procedure_Dispatch>(EvtNames.Dispatch_Switch_Procudure, OnProcedureChanged);
    }

    private void OnDisable()
    {
        EvtDsp.RemoveEvt<DispatchDeskClickAction>(EvtNames.DispatchDesk_Click_Action, OnDeskClickAction);
        EvtDsp.RemoveEvt<string>(EvtNames.DispatchDesk_Select_Tape, OnSelectTape);
        EvtDsp.RemoveEvt<Procedure_Dispatch>(EvtNames.Dispatch_Switch_Procudure, OnProcedureChanged);
    }

    private void Start()
    {
        HideLegacySelectionButtons();
    }

    private void OnProcedureChanged(Procedure_Dispatch procedure)
    {
        if (procedure == Procedure_Dispatch.SelectFood)
            HideLegacySelectionButtons();
    }

    private void OnDeskClickAction(DispatchDeskClickAction action)
    {
        ResolveDispatchPanel();

        switch (action)
        {
            case DispatchDeskClickAction.SelectFood:
                dispatchPanel.selectFoodButton.onClick.Invoke();
                break;
            case DispatchDeskClickAction.SelectSnack:
                dispatchPanel.selectSnackButton.onClick.Invoke();
                break;
        }
    }

    private void OnSelectTape(string tapeName)
    {
        ResolveDispatchPanel();
        dispatchPanel.SetDispatchItem(CLIP.Project_Mouse.ENUM.Item_Type.Tape, tapeName);
    }

    private void HideLegacySelectionButtons()
    {
        ResolveDispatchPanel();
        SetVisuallyHidden(dispatchPanel.selectFoodButton);
        SetVisuallyHidden(dispatchPanel.selectSnackButton);
        SetVisuallyHidden(dispatchPanel.selectCDButton);
    }

    private void ResolveDispatchPanel()
    {
        if (dispatchPanel == null)
            dispatchPanel = FindFirstObjectByType<DispatchPanel>();
    }

    private static void SetVisuallyHidden(UnityEngine.UI.Button button)
    {
        var group = button.GetComponent<CanvasGroup>();
        if (group == null)
            group = button.gameObject.AddComponent<CanvasGroup>();

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }
}
