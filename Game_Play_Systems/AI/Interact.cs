using System.Collections.Generic;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using UnityEngine.AI;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class Interact
    {
        public bool isLie;
        public readonly List<Vector3> positions = new List<Vector3>();
        public readonly List<PlacementRuntime> placements = new List<PlacementRuntime>();
        public readonly List<Pot> pots = new List<Pot>();
        public readonly List<InteractPositionInfo> positionInfos = new List<InteractPositionInfo>();
        public InteractInfo info;

        public int chosenIndex { get; private set; }

        public Vector3 position =>
            positions.Count > 0 ? positions[Mathf.Clamp(chosenIndex, 0, positions.Count - 1)] : Vector3.zero;

        public PlacementRuntime placement =>
            placements != null && chosenIndex >= 0 && chosenIndex < placements.Count ? placements[chosenIndex] : null;

        public Pot pot =>
            pots != null && chosenIndex >= 0 && chosenIndex < pots.Count ? pots[chosenIndex] : null;

        public InteractPositionInfo ChosenPositionInfo =>
            positionInfos != null && chosenIndex >= 0 && chosenIndex < positionInfos.Count
                ? positionInfos[chosenIndex]
                : null;

        public void AddCandidate(Vector3 pos, PlacementRuntime placement, Pot pot, InteractPositionInfo posInfo)
        {
            positions.Add(pos);
            placements.Add(placement);
            pots.Add(pot);
            positionInfos.Add(posInfo);
        }

        public void ChooseRandomCandidate()
        {
            if (positions == null || positions.Count == 0)
            {
                chosenIndex = 0;
                return;
            }
            chosenIndex = Random.Range(0, positions.Count);
        }

        public static Interact CreateLie(Vector3 liePos, InteractInfo interactInfo)
        {
            var x = new Interact { isLie = true, info = interactInfo };
            x.AddCandidate(liePos, null, null, null);
            x.ChooseRandomCandidate();
            return x;
        }

        public static Interact CreateSinglePlacement(Vector3 worldNavPos, PlacementRuntime placement, InteractInfo interactInfo, InteractPositionInfo posInfo)
        {
            var x = new Interact { isLie = false, info = interactInfo };
            x.AddCandidate(worldNavPos, placement, null, posInfo);
            x.ChooseRandomCandidate();
            return x;
        }

        public static Interact CreateSinglePot(Vector3 navPos, Pot pot, InteractInfo interactInfo)
        {
            var x = new Interact { isLie = false, info = interactInfo };
            x.AddCandidate(navPos, null, pot, null);
            x.ChooseRandomCandidate();
            return x;
        }
    }

    public class NavMeshPathLengthComparer : IComparer<NavMeshPath>
    {
        public int Compare(NavMeshPath x, NavMeshPath y)
        {
            return (int)Mathf.Sign(CalculatePathLength(x) - CalculatePathLength(y));
        }

        private float CalculatePathLength(NavMeshPath path)
        {
            if (path.corners.Length < 2) return 0f;
            float totalLength = 0f;
            Vector3 prevCorner = path.corners[0];
            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 currentCorner = path.corners[i];
                totalLength += Vector3.Distance(prevCorner, currentCorner);
                prevCorner = currentCorner;
            }
            return totalLength;
        }
    }
}
