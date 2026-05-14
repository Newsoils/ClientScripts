using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

public static class GridObjectRaycastUtility
{
    private static Camera _mainCam;

    private static Camera MainCam
    {
        get
        {
            if (_mainCam == null)
                _mainCam = Camera.main;
            return _mainCam;
        }
    }

    public static GridObject RaycastPlacement(Vector2 screenPos, LayerMask mask, out Vector3 hitPos)
    {
        Ray ray = MainCam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100, mask))
        {
            hitPos = hit.point;
            return hit.collider.GetComponentInParent<GridObject>();
        }

        hitPos = Vector3.zero;
        return null;
    }

    public static bool RaycastGrid(
        Vector2 screenPos,
        LayerMask gridMask,
        GridObject selected,
        out GridLayerTag gridTag,
        out Vector3 alignPos,
        out Int2 gridPos)
    {
        //foreach (var hit in hits)
        //{
        //    Debug.Log("打到物体： " + hit.collider.gameObject.name);
        //}
        gridTag = null;
        alignPos = Vector3.zero;
        gridPos = default;

        Ray ray = MainCam.ScreenPointToRay(screenPos);
        //Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red, 2f);

        var hits = Physics.RaycastAll(ray, 1000, gridMask);

        //if (!Physics.Raycast(ray, out RaycastHit hit, 1000, gridMask))
        //    return false;

        foreach (var hit in hits)
        {
            var tag = hit.collider.GetComponent<GridLayerTag>();
            if (tag == null) continue;

            // 禁止将家具放置到其他房间的网格平面上
            if (RoomSystem.currentRoom == null || tag.roomName != RoomSystem.currentRoom.RoomName)
                continue;

            var placingType = selected.placingType;
            if (!Enum_Helper.GridLayerMap.TryGetValue(placingType, out var validLayers))
                continue;

            if (!validLayers.Contains(tag.gridLayerType))
                continue;

            var (x, y) = selected.GetCurSize();
            var pos = hit.point;
            switch (tag.gridLayerType)
            {
                case GridLayerType.Floor:
                case GridLayerType.Ceiling:
                case GridLayerType.Surface:
                    pos -= new Vector3(x / 2f, 0, y / 2f);
                    break;
                case GridLayerType.Wall_N:
                case GridLayerType.Wall_S:
                    //pos -= new Vector3(x / 2f, y / 2f, 0);
                    pos.z = tag.girdOrginalPoint.position.z - selected.gridData.width;
                    break;
                case GridLayerType.Wall_W:
                case GridLayerType.Wall_E:
                    //pos -= new Vector3(0, y / 2f, x / 2f);
                    pos.x = tag.girdOrginalPoint.position.z - selected.gridData.width;
                    break;
            }

            GridUtility.CalculateGridPosition(tag, pos, out alignPos, out gridPos);
            Debug.Log("POs" + pos +"\t"+ "alignPos" + alignPos + "\t" + "gridPos" +gridPos);
            gridTag = tag;

            return true;
        }
        return false;
    }
}
