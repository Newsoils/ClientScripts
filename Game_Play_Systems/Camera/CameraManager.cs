using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class CameraManager : SingletonMono<CameraManager>
{
    public CameraControl cameraControl;
    public RoomCameraDatabase camera_db;
    public List<CameraCtrlData> cameraControllers;
    public Dictionary<string, Dictionary<CameraType, CameraCtrlData>> cameras; //房间名-照相机种类-相机
    public CinemachineCameraController curCtrl;
    public CinemachineBrain mainCamera;
    public GameObject obj;

    private string currentRoomName;

    public CameraState state;


    private void Start()
    {
        InitState(CameraState.Normal);
        EvtDsp.AddEvt(EvtNames.OnLevelPanelOpen, ChangeFrozenState);
        EvtDsp.AddEvt(EvtNames.OnLevelPanelClose, ChangeNormalState);
        InitCamera();
        //SwitchCamera("客厅", CameraType.Normal);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        EvtDsp.RemoveEvt(EvtNames.OnLevelPanelOpen, ChangeFrozenState);
        EvtDsp.RemoveEvt(EvtNames.OnLevelPanelClose, ChangeNormalState);
    }

    private void LateUpdate()
    {
        switch (state)
        {
            case CameraState.Normal:
                UpdateNormalState();
                break;
            case CameraState.Placement:
                UpdatePlacementState();
                break;
        }
    }


    public void ChangeRoom(string name)
    {
        //SwitchCamera(name, CameraType.Normal);
        currentRoomName = name;
        RefreshCamera();
    }
    private void InitCamera()
    {
        cameras = cameraControllers
        .GroupBy(data => data.roomName)
        .ToDictionary(
            group => group.Key,
            group => group.ToDictionary(data => data.type, data => data)
        );
    }
    private void SwitchCamera(string roomName, CameraType type)
    {
        curCtrl?.gameObject.SetActive(false);
        curCtrl = cameras[roomName][type].ctrl;
        curCtrl.gameObject.SetActive(true);
    }
    private void RefreshRoomName()
    {
        var room = RoomSystem.currentRoom;
        if (room != null)
            currentRoomName = room.RoomName;
    }


    private void RefreshCamera()
    {
        if (string.IsNullOrEmpty(currentRoomName)) return;
        var camera = camera_db.GetView(currentRoomName, state);
        cameraControl.SnapToPivot(state, camera);
    }



    #region 状态
    public void InitState(CameraState newState)
    {
        state = newState;
        EnterState(state);
    }
    public void ChangeState(CameraState newState)
    {
        ExitState(state);
        state = newState;

        RefreshRoomName();


        EnterState(state);
    }
    private void EnterState(CameraState state)
    {
        switch (state)
        {
            case CameraState.Normal:
                EnterNormalState();
                break;
            case CameraState.Placement:
                EnterPlacementState();
                break;
        }
    }
    private void ExitState(CameraState state)
    {
        switch (state)
        {
            case CameraState.Normal:
                ExitNormalState();
                break;
            case CameraState.Placement:
                ExitPlacementState();
                break;
        }
    }
    #region 普通状态
    private void EnterNormalState()
    {
        RefreshCamera();
    }
    private void UpdateNormalState()
    {
        cameraControl.CheckMove();
        cameraControl.CheckScale();
        cameraControl.CheckReset();
        cameraControl.CheckRotate();
    }
    private void ExitNormalState()
    {

    }

    #endregion
    #region 家具
    public void EnterPlacementState()
    {
        RefreshCamera();
    }
    public void UpdatePlacementState()
    {
        //cameraControl.CheckMove(true);
    }
    public void ExitPlacementState()
    {
        cameraControl.Reset();

        //yaw.enabled = true;
        //controlCamera.orthographicSize = 18f;
    }
    #endregion
    #region 冻结
    public void ChangeFrozenState()
    {
        ChangeState(CameraState.Frozen);
    }
    public void ChangeNormalState()
    {
        ChangeState(CameraState.Normal);
    }
    #endregion
    #endregion



    public void CloseCamera()
    {
        obj.SetActive(false);
    }

    public void OpenCamera()
    {
        obj.SetActive(true);
    }
}

[Serializable]
public class CameraCtrlData
{
    public string roomName;
    public CameraType type;
    public CinemachineCameraController ctrl;
}
public enum CameraType
{
    Normal,
    Placement
}