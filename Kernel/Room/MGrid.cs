using System.Collections;
using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;

public class MGrid
{
    public Int2 pos;

    public string UId;
    public bool isOccpuied;
    public bool isBlocked;
    public GridLayerType type;

    public GridObjectData occpiedPlacement;
    public MGrid(GridLayerType type,Int2 pos)
    {
        this.type = type;
        this.pos = pos;
        isOccpuied = false;
        isBlocked = false;
        UId = System.Guid.NewGuid().ToString();
    }

    public MGrid (GridLayerType type, Int2 pos, bool isOccpuied,bool isBlocked)
    {
        this.type = type;
        this.pos = pos;
        this.isOccpuied = isOccpuied;
        this.isBlocked = isBlocked;
        UId = System.Guid.NewGuid().ToString();
    }

    public void SetOccpied(GridObjectData placement)
    {
        occpiedPlacement = placement;
        isOccpuied = true;
    }

    public void ClearOccpied()
    {
        occpiedPlacement = null;
        isOccpuied = false;
    }
    public void SetOccupied (bool occupied)
    {
        isOccpuied = occupied;
    }

    public void SetBlocked (bool blocked)
    {
        isBlocked = blocked;
    }

}

