using System.Collections.Generic;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using UnityEngine;

public class CameraManager : SingletonMono<CameraManager>
{
    private static readonly Dictionary<string, CinemachineCameraController> s_cameras = new();

    public CinemachineCameraController curCtrl;

    private string currentRoomName;

    public CameraState State { get; private set; } = CameraState.Normal;

    public bool InputDisabled { get; private set; }


    #region Controller 自注册
    /// <summary>把 controller 注册到全局相机表，key = ctrl.CameraName。</summary>
    public static void Register(CinemachineCameraController ctrl)
    {
        var name = ctrl.CameraName;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError($"[CameraManager] '{ctrl.name}' 的 cameraName 为空，无法注册。请在 prefab Inspector 上填好。");
            return;
        }
        if (s_cameras.TryGetValue(name, out var existing) && existing != ctrl)
        {
            Debug.LogWarning(
                $"[CameraManager] 相机名重复 '{name}'：已存在 '{existing.name}'，被 '{ctrl.name}' 覆盖。" +
                "请检查 prefab 上的 cameraName 是否冲突。");
        }
        s_cameras[name] = ctrl;
    }

    /// <summary>从全局相机表移除 controller（仅当表里登记的就是它本身才移除）。</summary>
    public static void Unregister(CinemachineCameraController ctrl)
    {
        var name = ctrl.CameraName;
        if (string.IsNullOrEmpty(name)) return;
        if (s_cameras.TryGetValue(name, out var existing) && existing == ctrl)
        {
            s_cameras.Remove(name);
        }
    }
    #endregion


    #region 切相机
    /// <summary>按相机名切换：关掉所有其他相机、激活目标相机、复位状态、更新 curCtrl。重复切到同一台跳过。</summary>
    public void SwitchCamera(string cameraName)
    {
        if (string.IsNullOrEmpty(cameraName)) return;
        if (!s_cameras.TryGetValue(cameraName, out var next))
        {
            Debug.LogError($"[CameraManager] 找不到相机 '{cameraName}'，请检查对应 prefab 是否在场景中且 cameraName 配置正确。");
            return;
        }
        if (next == curCtrl) return;

        foreach (var c in s_cameras.Values)
        {
            if (c != next && c != null && c.gameObject.activeSelf)
            {
                c.gameObject.SetActive(false);
            }
        }
        if (!next.gameObject.activeSelf) next.gameObject.SetActive(true);
        next.ResetCamera();
        curCtrl = next;
    }

    /// <summary>进入新房间，记下 currentRoomName 并切到 (roomName, Normal) 房间相机。</summary>
    public void ChangeRoom(string roomName)
    {
        currentRoomName = roomName;
        State = CameraState.Normal;
        SwitchCamera(MakeRoomCameraName(roomName, CameraState.Normal));
    }

    /// <summary>切换当前房间相机的状态（Normal ↔ Placement），currentRoomName 必须先经 ChangeRoom 设置。</summary>
    public void ChangeState(CameraState newState)
    {
        State = newState;
        SwitchCamera(MakeRoomCameraName(currentRoomName, newState));
    }

    /// <summary>房间相机命名规则：&lt;房间名&gt;_&lt;state&gt;。所有房间相机 prefab 的 cameraName 都应按此格式填写。</summary>
    public static string MakeRoomCameraName(string roomName, CameraState state)
    {
        return $"{roomName}_{state}";
    }
    #endregion


    #region 输入冻结
    /// <summary>禁用所有 controller 的输入处理（依然渲染，只是不响应输入）。</summary>
    public void Freeze() => InputDisabled = true;

    /// <summary>恢复 controller 的输入处理。</summary>
    public void Unfreeze() => InputDisabled = false;
    #endregion
}
