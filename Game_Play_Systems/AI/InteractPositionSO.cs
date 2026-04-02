using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [CreateAssetMenu(fileName = "InteractPositionSO ", menuName = "Project_Mouse/InteractPositionSO")]
    public class InteractPositionSO : ScriptableObject
    {
        public List<PlacementInteractInfo> positionInfos = new List<PlacementInteractInfo>();
        public void AddPositionInfo(string placementName, string interactName, Vector3 position, Quaternion rotation)
        {
            var info = positionInfos.Find(x => x.placementName == placementName);
            if(info == null)
            {
                info = new PlacementInteractInfo(placementName);
                positionInfos.Add(info);
            }
            var posInfo = info.positions.Find(x => x.interactName == interactName);
            if (posInfo == null)
            {
                info.positions.Add(new InteractPositionInfo(interactName, position, rotation));
            }
            else
            {
                int index = info.positions.IndexOf(posInfo);
                info.positions[index] = new InteractPositionInfo(interactName, position, rotation);
            }
        }
        public bool TryGetPositionInfo(string placementName, string interactName, out InteractPositionInfo posInfo)
        {
            var info = positionInfos.Find(x => x.placementName == placementName);
            if (info != null)
            {
                var i = info.positions.Find(x => x.interactName == interactName);
                if(i != null)
                {
                    posInfo = i;
                    return true;
                }
            }
            posInfo = null;
            return false;
        }
    }

    [Serializable]
    public class PlacementInteractInfo
    {
        public string placementName;
        public List<InteractPositionInfo> positions = new List<InteractPositionInfo>();
        public PlacementInteractInfo(string placementName)
        {
            this.placementName = placementName;
        }
    }
    [Serializable]
    public class InteractPositionInfo
    {
        public string interactName;
        public Vector3 position;
        public Quaternion rotation;
        public InteractPositionInfo(string interactName, Vector3 position, Quaternion rotation)
        {
            this.interactName = interactName;
            this.position = position;
            this.rotation = rotation;
        }
    }
}

