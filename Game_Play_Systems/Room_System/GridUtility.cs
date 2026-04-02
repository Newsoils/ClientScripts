using System;
using System.Collections.Generic;
using System.Linq;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
public enum GridDirection
{
    NorthWest, North, NorthEast,
    West, East,
    SouthWest, South, SouthEast, None
}

/// <summary>
/// 墙角的方向
/// </summary>
public enum CornerDir
{
    None = -1,
    NorthEast = 0,
    NorthWest = 1,
    SouthEast = 2,
    SouthWest = 3
}


public class GridUtility
{

    #region 方向查找

    public static readonly Int2[] FourDirections = new Int2[]
    {
                new Int2(0,1), // North
                new Int2(1,0), // East
                new Int2(0,-1), // South
                new Int2(-1,0) // West
    };

    public static readonly Dictionary<GridDirection, Int2> FourDirectionDic = new Dictionary<GridDirection, Int2>
            {
                { GridDirection.North, new Int2(0,1) },
                { GridDirection.East, new Int2(1,0) },
                { GridDirection.South, new Int2(0,-1) },
                { GridDirection.West, new Int2(-1,0) }
            };


    public static readonly GridDirection[] FourDirectionArray = new GridDirection[]
    {
                GridDirection.North,
                GridDirection.East,
                GridDirection.South,
                GridDirection.West
    };

    public static readonly GridDirection[] EightDirectionArray = new GridDirection[]
    {
                GridDirection.North,
                GridDirection.NorthEast,
                GridDirection.East,
                GridDirection.SouthEast,
                GridDirection.South,
                GridDirection.SouthWest,
                GridDirection.West,
                GridDirection.NorthWest
    };

    /// <summary>
    /// 全局静态邻居偏移表
    /// </summary>
    public static readonly Dictionary<GridDirection, Int2> EightDirectionOffsetsDic = new Dictionary<GridDirection, Int2> {
                { GridDirection.NorthWest, new Int2(-1, 1) },
                { GridDirection.North,     new Int2(0, 1) },
                { GridDirection.NorthEast, new Int2(1, 1) },
                { GridDirection.West,      new Int2(-1, 0) },
                { GridDirection.East,      new Int2(1, 0) },
                { GridDirection.SouthWest, new Int2(-1, -1) },
                { GridDirection.South,     new Int2(0, -1) },
                { GridDirection.SouthEast, new Int2(1, -1) },
            };

    /// <summary>
    /// 静态数组，可以用来遍历附近的格子
    /// </summary>
    public static readonly Int2[] EightDirectionOffsetsArray = {
                new Int2(-1, 1), new Int2(0, 1), new Int2(1, 1),
                new Int2(-1, 0),                  new Int2(1, 0),
                new Int2(-1,-1), new Int2(0,-1), new Int2(1,-1)
            };


    public static readonly Dictionary<GridDirection, GridDirection> oppositeDir = new Dictionary<GridDirection, GridDirection>
                {
                    { GridDirection.North, GridDirection.South },
                    { GridDirection.South, GridDirection.North },
                    { GridDirection.East, GridDirection.West },
                    { GridDirection.West, GridDirection.East },
                    { GridDirection.NorthEast, GridDirection.SouthWest },
                    { GridDirection.NorthWest, GridDirection.SouthEast },
                    { GridDirection.SouthEast, GridDirection.NorthWest },
                    { GridDirection.SouthWest, GridDirection.NorthEast }
                };

    public static GridDirection GetOppositeDirect(GridDirection originalDir)
    {
        oppositeDir.TryGetValue(originalDir, out var opp);
        return opp;
    }

    public static GridDirection[] GetOppositeDirects(GridDirection[] originalDirs)
    {
        return originalDirs.Select(d => GetOppositeDirect(d)).ToArray();
    }
    /// <summary>
    /// 通过两个边方向 (North/East/West/South) 找到对应的 CornerDir
    /// </summary>
    public static CornerDir GetCornerDir(GridDirection dir1, GridDirection dir2)
    {
        // 保证两个方向不重复，并且是正交的
        var dirs = new HashSet<GridDirection> { dir1, dir2 };

        if (dirs.Contains(GridDirection.North) && dirs.Contains(GridDirection.East))
            return CornerDir.NorthEast;
        if (dirs.Contains(GridDirection.North) && dirs.Contains(GridDirection.West))
            return CornerDir.NorthWest;
        if (dirs.Contains(GridDirection.South) && dirs.Contains(GridDirection.East))
            return CornerDir.SouthEast;
        if (dirs.Contains(GridDirection.South) && dirs.Contains(GridDirection.West))
            return CornerDir.SouthWest;

        return CornerDir.None;
    }

    /// <summary>
    /// 通过两个边方向 (North/East/West/South) 找到对应的斜角 GridDirection (NorthEast / SouthWest 等)
    /// </summary>
    public static GridDirection GetCornerGridDirection(GridDirection dir1, GridDirection dir2)
    {
        var dirs = new HashSet<GridDirection> { dir1, dir2 };

        if (dirs.Contains(GridDirection.North) && dirs.Contains(GridDirection.East))
            return GridDirection.NorthEast;
        if (dirs.Contains(GridDirection.North) && dirs.Contains(GridDirection.West))
            return GridDirection.NorthWest;
        if (dirs.Contains(GridDirection.South) && dirs.Contains(GridDirection.East))
            return GridDirection.SouthEast;
        if (dirs.Contains(GridDirection.South) && dirs.Contains(GridDirection.West))
            return GridDirection.SouthWest;

        return GridDirection.None;
    }
    #endregion


    #region 查

    /// <summary>
    /// 尝试获得大格,重叠部分获取时优先获得靠左靠下的大格
    /// </summary>
    /// <param name="pos">小格坐标</param>
    public bool TryGetMGrid(Dictionary<Int2, MGrid> mGrids, Int2 pos, out MGrid grid)
    {
        grid = null;
        if (mGrids.TryGetValue(pos, out grid))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryGetNeighborGrid(Dictionary<Int2, MGrid> mGrids, Int2 pos, GridDirection direction, out MGrid gird)
    {
        var neightborPos = pos + EightDirectionOffsetsDic[direction];
        if (mGrids.TryGetValue(neightborPos, out gird))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public HashSet<Int2> GetNearMGridsPos(Int2 pos, int range = 5)
    {
        HashSet<Int2> result = new HashSet<Int2>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                // 这里定义范围，可以选择曼哈顿距离或欧几里得距离
                // 方式1：方形范围（正方形框）
                if (Math.Abs(dx) <= range && Math.Abs(dy) <= range)
                {
                    Int2 nearPos = new Int2(pos.x + dx, pos.y + dy);
                    result.Add(nearPos);
                }

                // 方式2：圆形范围（欧几里得距离）
                //if (dx * dx + dy * dy <= range * range)
                //{
                //    int2 nearPos = new int2(pos.x + dx, pos.y + dy);
                //    result.Add(nearPos);
                //}
            }
        }
        return result;
    }

    public HashSet<Int2> GetNearMGridsPos(HashSet<Int2> Pos, int range = 5)
    {
        HashSet<Int2> result = new HashSet<Int2>();

        foreach (var pos in Pos)
        {
            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    // 这里定义范围，可以选择曼哈顿距离或欧几里得距离
                    // 方式1：方形范围（正方形框）
                    if (Math.Abs(dx) <= range && Math.Abs(dy) <= range)
                    {
                        Int2 nearPos = new Int2(pos.x + dx, pos.y + dy);
                        result.Add(nearPos);
                    }

                    // 方式2：圆形范围（欧几里得距离）
                    //if (dx * dx + dy * dy <= range * range)
                    //{
                    //    int2 nearPos = new int2(pos.x + dx, pos.y + dy);
                    //    result.Add(nearPos);
                    //}
                }
            }
        }
        return result;
    }

    public static HashSet<Int2> CalculateOccpiedPos(int x, int y, Int2 pos)
    {
        HashSet<Int2> occupiedPositions = new HashSet<Int2>();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                occupiedPositions.Add(pos + new Int2(i, j));
            }
        }

        return occupiedPositions;
    }

    public static Int2 CalculateGridLayerPosition(string gridLayerUid, Vector3 postion)
    {
        if (RoomSystem.currentRoom == null) return Int2.zero;
        if (RoomSystem.currentRoom.TryGetLayerTag(gridLayerUid, out var gridLayerTag))
        {
            return CalculateGridLayerPosition(gridLayerTag, postion);
        }
        return Int2.zero;
    }

    public static Int2 CalculateGridLayerPosition(GridLayerTag gridLayerTag, Vector3 postion)
    {
        Int2 res = Int2.zero;

        Vector3 relativePostion = postion - gridLayerTag.girdOrginalPoint.position;
        switch (gridLayerTag.gridLayerType)
        {
            case GridLayerType.Floor:
            case GridLayerType.Ceiling:
            case GridLayerType.Surface:
                res = new Int2((int)relativePostion.x, (int)relativePostion.z);
                break;
            case GridLayerType.Wall_S:
            case GridLayerType.Wall_N:
                res = new Int2((int)relativePostion.x, (int)relativePostion.y);
                break;
            case GridLayerType.Wall_W:
            case GridLayerType.Wall_E:
                res = new Int2((int)relativePostion.z, (int)relativePostion.y);
                break;

        }
        return res;

    }

    public static void CalculateGridPosition(GridLayerTag gridLayerTag, Vector3 postion, out Vector3 alignPos, out Int2 gridPostion)
    {
        alignPos = postion;
        var zeroPos = gridLayerTag.girdOrginalPoint.position;
        gridPostion = Int2.zero;
        Vector3 relativePostion = postion - zeroPos;
        switch (gridLayerTag.gridLayerType)
        {
            case GridLayerType.Floor:
            case GridLayerType.Ceiling:
            case GridLayerType.Surface:
                var x = Mathf.FloorToInt(relativePostion.x);
                var z = Mathf.FloorToInt(relativePostion.z);
                alignPos = new Vector3(x, postion.y, z) + new Vector3(zeroPos.x,0,zeroPos.z);
                gridPostion = new Int2(x, z);
                break;
            case GridLayerType.Wall_S:
            case GridLayerType.Wall_N:
                x = Mathf.FloorToInt(relativePostion.x);
                var y = Mathf.FloorToInt(relativePostion.y);
                alignPos = new Vector3(x, y, postion.z) + new Vector3(zeroPos.x, zeroPos.y, 0);
                gridPostion = new Int2(x, y);
                break;
            case GridLayerType.Wall_W:
            case GridLayerType.Wall_E:
                z = Mathf.FloorToInt(relativePostion.z);
                y = Mathf.FloorToInt(relativePostion.y);
                alignPos = new Vector3(postion.x, y, z) + new Vector3(0,zeroPos.y, zeroPos.z); ;
                gridPostion = new Int2(z, y);
                break;
        }

    }



    /// <summary>
    /// 从Grid平面坐标换算到世界坐标
    /// </summary>
    /// <param name="gridLayerUid"></param>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public static Vector3 CalculateWorldPosition(string gridLayerUid, Int2 gridPos)
    {
        if (RoomSystem.currentRoom == null) return Vector3.zero;
        if (RoomSystem.currentRoom.TryGetLayerTag(gridLayerUid, out var gridLayerTag))
        {
            return CalculateWorldPosition(gridLayerTag, gridPos);
        }
        return Vector3.zero;
    }

    /// <summary>
    /// 从Grid平面坐标换算到世界坐标
    /// </summary>
    /// <param name="gridLayerTag"></param>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public static Vector3 CalculateWorldPosition(GridLayerTag gridLayerTag, Int2 gridPos)
    {
        var res = Vector3.zero;
        switch (gridLayerTag.gridLayerType)
        {
            case GridLayerType.Floor:
            case GridLayerType.Ceiling:
            case GridLayerType.Surface:
                res = gridLayerTag.girdOrginalPoint.position + new Vector3(gridPos.x, 0, gridPos.y);
                break;
            case GridLayerType.Wall_S:
            case GridLayerType.Wall_N:
                res = gridLayerTag.girdOrginalPoint.position + new Vector3(gridPos.x, gridPos.y, 0);
                break;
            case GridLayerType.Wall_W:
            case GridLayerType.Wall_E:
                res = gridLayerTag.girdOrginalPoint.position + new Vector3(0, gridPos.y, gridPos.x);
                break;
        }
        return res;

    }
    #endregion

    public static Vector3 RotationIndexToDirection(Placement_Rotation rotation)
    {
        return rotation switch
        {
            Placement_Rotation.Deg0 => Vector3.zero,
            Placement_Rotation.Deg90 => Vector3.up * 90,
            Placement_Rotation.Deg180 => Vector3.up * 180,
            Placement_Rotation.Deg270 => Vector3.up * 270,
            _ => Vector3.zero
        };
    }

}
