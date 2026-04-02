using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Project_Mouse/Camera/Room Camera Database")]
public class RoomCameraDatabase : ScriptableObject
{
    public List<RoomCameraData> rooms = new();

    private Dictionary<string, RoomCameraData> _cache;

    public void Init()
    {
        _cache = new();

        foreach (var r in rooms)
        {
            if (!_cache.ContainsKey(r.roomName))
                _cache.Add(r.roomName, r);
        }
    }

    public CameraViewData GetView(string room, CameraState mode)
    {
        if (_cache == null) Init();
        if (room == null) return null;
        if (_cache.TryGetValue(room, out var data))
        {
            return data.GetView(mode);
        }

        Debug.LogError($"No camera data for room: {room}");
        return null;
    }


    public void ResetOffsets()
    {
        foreach (var room in rooms)
        {
            ResetView(room.normalView);
            ResetView(room.placementView);
        }
    }

    public void ClearAllData()
    {
        rooms.Clear();
    }

    private void ResetView(CameraViewData view)
    {
        if (view == null) return;

        view.positionOffset = Vector3.zero;
        view.euler = Vector3.zero;

        view.pivotOffset = Vector3.zero;
        view.pivotEuler = Vector3.zero;

        view.orthographicSize = 0f;
    }


}