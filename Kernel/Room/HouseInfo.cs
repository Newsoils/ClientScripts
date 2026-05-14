using System;
using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;

/// <summary>
/// 以后可能要拓展成一个房子，多个房间
/// 现在暂时不用
/// </summary>
[System.Serializable]
public class HouseInfo
{
    public string HouseUID { get; private set; }

    private Dictionary<string, RoomData> rooms = new();

    public HouseInfo()
    {
        HouseUID = Guid.NewGuid().ToString();
    }

    public void AddRoom(RoomData room)
    {
        rooms[room.roomUID] = room;
    }

    public IEnumerable<RoomData> GetAllRooms()
    {
        return rooms.Values;
    }
}
