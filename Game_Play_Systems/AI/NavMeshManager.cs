using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Unity;
using UnityEngine;

public class NavMeshManager : SingletonMono<NavMeshManager>
{
    public List<RoomEntrance> entrances = new();
    public Dictionary<string, RoomEntrance> entranceNameDic;

    private void Start()
    {
        entranceNameDic = entrances.ToDictionary(x => x.roomName);
    }
}

[Serializable]
public class RoomEntrance
{
    public string roomName;
    public List<Transform> entrance;
}