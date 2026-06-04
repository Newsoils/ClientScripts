using System.Collections;
using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System;
using MoreMountains.Tools;
using UnityEngine;

namespace CLIP.Project_Mouse.UI
{
    public class JoystickPanel : UIPanelBase
    {
        public GameObject joystickObj;
        public MMTouchJoystick joystick;
        private Vector3 originPosition;
        public Vector3 offset;
        private void Start()
        {
            joystick.JoystickValue.AddListener(SetInputValue);
            originPosition = joystickObj.GetComponent<RectTransform>().anchoredPosition;
            EvtDsp.AddEvt(EvtNames.OnPlacementPanelOpen,SetUp);
            EvtDsp.AddEvt(EvtNames.OnPlantPanelOpen, SetUp);
            EvtDsp.AddEvt(EvtNames.OnClothPanelOpen, SetUp);
            EvtDsp.AddEvt(EvtNames.OnPlacementPanelClose, SetDown);
            EvtDsp.AddEvt(EvtNames.OnPlantPanelClose, SetDown);
            EvtDsp.AddEvt(EvtNames.OnClothPanelClose, SetDown);
            EvtDsp.AddEvt(EvtNames.OnDispatchPanelOpen, ClosePanel);
            EvtDsp.AddEvt(EvtNames.OnDispatchPanelClose, OpenPanel);
            EvtDsp.AddEvt(EvtNames.OnShoppingPanelOpen, ClosePanel);
            EvtDsp.AddEvt(EvtNames.OnShoppingPanelClose, OpenPanel);
            EvtDsp.AddEvt(EvtNames.OnTakePhotoPanelOpen, ClosePanel);
            EvtDsp.AddEvt(EvtNames.OnTakePhotoPanelClose, OpenPanel);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            EvtDsp.RemoveEvt(EvtNames.OnPlacementPanelOpen, SetUp);
            EvtDsp.RemoveEvt(EvtNames.OnPlantPanelOpen, SetUp);
            EvtDsp.RemoveEvt(EvtNames.OnClothPanelOpen, SetUp);
            EvtDsp.RemoveEvt(EvtNames.OnPlacementPanelClose, SetDown);
            EvtDsp.RemoveEvt(EvtNames.OnPlantPanelClose, SetDown);
            EvtDsp.RemoveEvt(EvtNames.OnClothPanelClose, SetDown);
            EvtDsp.RemoveEvt(EvtNames.OnDispatchPanelOpen, ClosePanel);
            EvtDsp.RemoveEvt(EvtNames.OnDispatchPanelClose, OpenPanel);
            EvtDsp.RemoveEvt(EvtNames.OnShoppingPanelOpen, ClosePanel);
            EvtDsp.RemoveEvt(EvtNames.OnShoppingPanelClose, OpenPanel);
            EvtDsp.RemoveEvt(EvtNames.OnTakePhotoPanelOpen, ClosePanel);
            EvtDsp.RemoveEvt(EvtNames.OnTakePhotoPanelClose, OpenPanel);
        }
        public void SetInputValue(Vector2 input)
        {
            InputManager.Instance.SetJoystickInput(input);
        }
        public void SetJoystickPosition(Vector3 position)
        {
            joystickObj.GetComponent<RectTransform>().anchoredPosition = position;
            joystick.SetNeutralPosition();

        }
        public void ShowJoystick(bool show)
        {
            joystickObj.SetActive(show);
        }
        public void ResetPosition()
        {
            SetJoystickPosition(originPosition);
        }
        public void SetUp()
        {
            SetJoystickPosition(originPosition + offset);
        }
        public void SetDown()
        {
            SetJoystickPosition(originPosition);
        }
        public override void OpenPanel(params object[] data)
        {
            ShowJoystick(true);
        }
        private void OpenPanel()
        {
            OpenPanel(null);
        }

        public override void ClosePanel()
        {
            ShowJoystick(false);
        }
    }
}

