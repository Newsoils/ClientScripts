using System.Collections.Generic;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Scene_View_Control;
using UnityEngine;
using UnityEngine.AI;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// 纯逻辑类，封装主角的寻路/可达性判断。不继承 MonoBehaviour。
    /// </summary>
    public class CharacterNavigator
    {
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;

        public CharacterNavigator(NavMeshAgent agent, Transform transform)
        {
            _agent = agent;
            _transform = transform;
        }

        #region 基础寻路

        public void WrapPosition()
        {
            if (NavMesh.SamplePosition(_transform.position, out var hit, 4, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }

        public bool CanMoveTo(Vector3 target)
        {
            NavMeshPath path = new NavMeshPath();
            return _agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        public bool CanMoveToRoom(Room room, out Vector3 position)
        {
            List<NavMeshPath> paths = new List<NavMeshPath>();
            foreach (var door in NavMeshManager.Instance.entranceNameDic[room.RoomName].entrance)
            {
                if (!NavMesh.SamplePosition(door.position, out var hit, 1f, NavMesh.AllAreas)) continue;
                NavMeshPath path = new NavMeshPath();
                if (_agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    paths.Add(path);
                }
            }
            if (paths.Count > 0)
            {
                paths.Sort(new NavMeshPathLengthComparer());
                position = paths[0].corners[^1];
                return true;
            }
            position = Vector3.zero;
            return false;
        }

        public bool CanMoveToRandomRoom(Room currentRoom, out Vector3 target)
        {
            var rooms = new List<Room>(RoomSystem.Instance.rooms);
            rooms.Remove(currentRoom);
            List<Vector3> positionsCanMove = new List<Vector3>();
            foreach (var room in rooms)
            {
                if (CanMoveToRoom(room, out Vector3 position))
                {
                    positionsCanMove.Add(position);
                }
            }
            if (positionsCanMove.Count > 0)
            {
                target = positionsCanMove[Random.Range(0, positionsCanMove.Count)];
                Debug.Log(RoomSystem.GetRoomByFloorPosition(target));
                return true;
            }
            target = Vector3.zero;
            return false;
        }

        #endregion

        #region 躺地板寻路

        /// <summary>
        /// 尝试在当前房间找到一个可躺的随机位置。成功时返回 true 并输出 liePosition。
        /// </summary>
        public bool CanMoveToRandomPosToLie(Room currentRoom, bool considerFindPath, out Vector3 liePosition)
        {
            liePosition = Vector3.zero;
            if (currentRoom == null) return false;

            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = currentRoom.GetRandomFloorPosition();
                if (NavMesh.SamplePosition(pos, out var hit, 1, NavMesh.AllAreas))
                {
                    if (!Physics.BoxCast(hit.position, new Vector3(1.5f, 1.5f, 1.5f), Vector3.down, Quaternion.identity, 0, 1 << 6 | 1 << 7))
                    {
                        if (CanMoveTo(hit.position) || !considerFindPath)
                        {
                            liePosition = hit.position;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool MoveToRandomPosToLie(Room currentRoom, out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;
            if (currentRoom == null) return false;

            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = currentRoom.GetRandomFloorPosition();
                if (NavMesh.SamplePosition(pos, out var hit, 2, NavMesh.AllAreas))
                {
                    if (!Physics.BoxCast(hit.position, new Vector3(1, 1, 1), Vector3.down, Quaternion.identity, 0, 1 << 6))
                    {
                        if (CanMoveTo(hit.position))
                        {
                            targetPosition = hit.position;
                            _agent.SetDestination(targetPosition);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region 花盆寻路

        public bool CanMoveToPot(Pot pot, out Vector3 position, bool considerFindPath = true)
        {
            position = Vector3.zero;
            for (int i = 0; i < 30; i++)
            {
                Vector3 point = GetRandomPointOnCircle(pot.plantRoot);
                point = new Vector3(point.x, 1.3f, point.z);
                if (NavMesh.SamplePosition(point, out var hit, 0.1f, NavMesh.AllAreas))
                {
                    if (CanMoveTo(hit.position) || !considerFindPath)
                    {
                        position = hit.position;
                        return true;
                    }
                }
            }
            return false;
        }

        public Vector3 GetRandomPointOnCircle(Transform target, float radius = 2.5f)
        {
            float randomAngle = Random.Range(0f, 360f);
            float angleInRadians = randomAngle * Mathf.Deg2Rad;
            Vector3 point = target.position + new Vector3(
                Mathf.Cos(angleInRadians) * radius,
                0f,
                Mathf.Sin(angleInRadians) * radius
            );
            return point;
        }

        #endregion

        #region 家具周围格子寻路

        /// <summary>
        /// 尝试在家具占据格子的相邻地面格子中，找到可达且路径最短的位置。
        /// </summary>
        public bool TryFindReachablePositionAroundPlacement(
            PlacementRuntime placement,
            bool considerFindPath,
            out Vector3 navPosition)
        {
            navPosition = Vector3.zero;
            var placementRoom = placement.room;
            if (placementRoom == null) return false;

            var gridState = placementRoom.Data.GridState;
            var layerUid = placement.data.gridLayerUID;

            if (!placementRoom.TryGetLayerTag(layerUid, out var placementLayerTag))
                return false;

            GridLayer floorLayer;
            GridLayerTag floorLayerTag;

            bool isFloor = placementLayerTag.gridLayerType == GridLayerType.Floor;
            if (isFloor)
            {
                floorLayer = gridState.GetLayer(layerUid);
                floorLayerTag = placementLayerTag;
            }
            else
            {
                if (!gridState.TryGetFirstLayer(GridLayerType.Floor, out floorLayer))
                    return false;
                if (!placementRoom.TryGetLayerTag(GridLayerType.Floor, out var floorTags) || floorTags.Count == 0)
                    return false;
                floorLayerTag = floorTags[0];
            }

            if (floorLayer == null) return false;

            var occupiedPositions = GridUtility.CalculateOccpiedPos(
                placement.gridData.length, placement.gridData.width, placement.data.position);

            HashSet<Int2> floorNeighbors = new HashSet<Int2>();

            if (isFloor)
            {
                foreach (var occPos in occupiedPositions)
                {
                    for (int i = 0; i < GridUtility.FourDirections.Length; i++)
                    {
                        var neighbor = occPos + GridUtility.FourDirections[i];
                        if (occupiedPositions.Contains(neighbor)) continue;
                        floorNeighbors.Add(neighbor);
                    }
                }
            }
            else
            {
                var furnitureWorldPos = placement.transform.position;
                GridUtility.CalculateGridPosition(floorLayerTag, furnitureWorldPos, out _, out var centerGridPos);
                int searchRadius = Mathf.Max(placement.gridData.length, placement.gridData.width) + 1;
                for (int dx = -searchRadius; dx <= searchRadius; dx++)
                {
                    for (int dz = -searchRadius; dz <= searchRadius; dz++)
                    {
                        floorNeighbors.Add(new Int2(centerGridPos.x + dx, centerGridPos.y + dz));
                    }
                }
            }

            float bestPathLen = float.MaxValue;
            Vector3 bestPos = Vector3.zero;
            bool found = false;

            foreach (var gridPos in floorNeighbors)
            {
                if (!floorLayer.CheckVaild(gridPos)) continue;

                Vector3 worldPos = GridUtility.CalculateWorldPosition(floorLayerTag, gridPos)
                                   + new Vector3(0.5f, 0f, 0.5f);

                if (!NavMesh.SamplePosition(worldPos, out var hit, 1f, NavMesh.AllAreas))
                    continue;

                if (!considerFindPath)
                {
                    navPosition = hit.position;
                    return true;
                }

                NavMeshPath path = new NavMeshPath();
                if (!_agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
                    continue;

                float pathLen = CalculatePathLength(path);
                if (pathLen < bestPathLen)
                {
                    bestPathLen = pathLen;
                    bestPos = hit.position;
                    found = true;
                }
            }

            if (found) navPosition = bestPos;
            return found;
        }

        public static float CalculatePathLength(NavMeshPath path)
        {
            if (path.corners.Length < 2) return 0f;
            float total = 0f;
            for (int i = 1; i < path.corners.Length; i++)
                total += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            return total;
        }

        #endregion
    }
}
