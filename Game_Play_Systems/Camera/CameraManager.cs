using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.UI;
using UnityEngine;

public class CameraManager : SingletonMono<CameraManager>
{
    public CameraControl cameraControl;
    public RoomCameraDatabase camera_db;
    public GameObject obj;

    private string currentRoomName;

    public CameraState state;


    private void Start()
    {
        InitState(CameraState.Normal);
        EvtDsp.AddEvt(EvtNames.OnLevelPanelOpen, ChangeFrozenState);
        EvtDsp.AddEvt(EvtNames.OnLevelPanelClose, ChangeNormalState);

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
        currentRoomName = name;
        RefreshCamera();
    }

    private void RefreshRoomName()
    {
        var room = RoomSystem.currentRoom;
        if(room!=null)
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
