using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.ENUM;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Game_Play_System.Planting_System;
using CLIP.Project_Mouse.Kernel;
using CLIP.Project_Mouse.Scene_View_Control;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace CLIP.Project_Mouse.Game_Play_System
{
    [Serializable]
    public class IndoorMainCharacter : CharacterBase, IClick
    {
        public PlacementRuntime interactPlacement;
        public Pot interactPot;
        public string interactName;
        public bool isFirstOpen;
        public Vector3 liePosition;
        public InteractPositionSO interactPositions;

        public float radius = 8;

        public static IndoorMainCharacter _instance;


        public bool _is_static = false;
        #region 状态
        public MainCharacterIdleState idleState { get; private set; }
        public MainCharacterMoveState moveState { get; private set; }
        private void InitStateMachine()
        {
            stateMachine = new StateMachine();
            idleState = new MainCharacterIdleState(stateMachine, this, "stand");
            moveState = new MainCharacterMoveState(stateMachine, this, "walk");
            InitInteract();
            stateMachine.Init(idleState);
        }
        public void EnterIdleState()
        {
            stateMachine.ChangeState(idleState);
        }
        #endregion

        #region 生命周期
        void Start()
        {
            EvtDsp.AddEvt(EvtNames.Dispatch_On_Start, OnDispatchStart);
            EvtDsp.AddEvt(EvtNames.Dispatch_On_End, OnDispatchEnd);
            EvtDsp.AddEvt<Room>(EvtNames.SwitchRoom, SwitchRendererState);

            EvtDsp.AddEvt(EvtNames.ReSetMainCharacterRenderer, SwitchRendererState);

            EvtDsp.AddEvt<bool>(EvtNames.SetMainCharacterState, SetRendererEnable);

            if (Dispatch_Manager._instance._player_dispatch_state.player_state == "On_Dispatch")
            {
                gameObject.SetActive(false);
            }
            _instance = this;
            if (isFirstOpen)
            {
                WrapPosition();
                InitStateMachine();
            }
            else
            {
                InitStateMachine();
                //等一帧，等房间系统那边初始化完
                StartCoroutine(nameof(InitRandomPositionAndInteract));
            }
            _current_room = RoomSystem.GetRoomByFloorPosition(transform.position);

        }

        private void OnDestroy()
        {
            EvtDsp.RemoveEvt(EvtNames.ReSetMainCharacterRenderer,SwitchRendererState);
            EvtDsp.RemoveEvt(EvtNames.Dispatch_On_Start, OnDispatchStart);
            EvtDsp.RemoveEvt(EvtNames.Dispatch_On_End, OnDispatchEnd);
            EvtDsp.RemoveEvt<Room>(EvtNames.SwitchRoom, SwitchRendererState);
            EvtDsp.RemoveEvt<bool>(EvtNames.SetMainCharacterState, SetRendererEnable);
        }

        private IEnumerator InitRandomPositionAndInteract()
        {
            yield return new WaitForEndOfFrame();
            SelectInteract(false);
            agent.enabled = false;

            if(curInteract!=null)
            transform.position = curInteract.position;

            agent.enabled = true;
            StartInteract();
            _current_room = RoomSystem.GetRoomByFloorPosition(transform.position);

        }

        bool isPassingRoom = false;
        public void FixedUpdate()
        {
            stateMachine.Update();
            curStateInfo = stateMachine.GetStateInfo();


            //通过桥到其他房间时，切换房间并刷新可见状态
            if(agent.isOnOffMeshLink&&!isPassingRoom)
            {
                isPassingRoom = true;
                var desPos = agent.currentOffMeshLinkData.endPos;
                var room = RoomSystem.GetRoomByFloorPosition(desPos);
                if (room != null) _current_room = room;
                SwitchRendererState(null);
            }
            if(!agent.isOnOffMeshLink&&isPassingRoom)
            {
                isPassingRoom = false;
            }
        }


        public void SwitchRendererState(Room room)
        {
            if(_current_room != null && RoomSystem.currentRoom != null)
            {
                SetRendererEnable(_current_room == RoomSystem.currentRoom);
            }
        }

        public void SwitchRendererState()
        {
            if (_current_room != null && RoomSystem.currentRoom != null)
            {
                SetRendererEnable(_current_room == RoomSystem.currentRoom);
            }
        }

        private void SetRendererEnable(bool enable)
        {
            var meshs = GetComponentsInChildren<Renderer>();
            foreach (var mesh in meshs)
            {
                mesh.enabled = enable;
            }
        }

   
        #endregion

        #region 交互

        #region 普通交互
        private List<InteractInfo> interactInfos;
        private Dictionary<InteractInfo, MainCharacterInteractState> interactStates;
        public Room _current_room;

        public Interact curInteract { get; set; }
        public void StartInteract()
        {
            if(curInteract !=null)
            stateMachine.ChangeState(interactStates[curInteract.info]);
        }
        private void InitInteract()
        {
            string json = JsonData_Manager.Load_Single_JsonData("project_mouse_tb_interact_info");
            interactInfos = JsonConvert.DeserializeObject<List<InteractInfo>>(json);
            interactStates = new Dictionary<InteractInfo, MainCharacterInteractState>();
            foreach (var info in interactInfos)
            {
                interactStates.Add(info, new MainCharacterInteractState(stateMachine, this, info));
            }
        }
        public void SelectInteract(bool considerFindPath = true)
        {
            List<Interact> interacts = new List<Interact>();
            for (int i = 0; i < interactInfos.Count; i++)
            {
                var info = interactInfos[i];
                if (!CheckRoomLimit(info)) continue;
                if (!CheckPlacementLimit(info, out Interact interact, considerFindPath)) continue;
                interacts.Add(interact);
            }
            if(interacts.Count>0)
            curInteract = interacts[Random.Range(0, interacts.Count)];
        }
        /// <summary>表字段 placementLimit 未填、空数组、或仅 None：与地面交互。</summary>
        private static bool IsInteractGroundOnly(InteractInfo info)
        {
            if (info.placementLimit == null || info.placementLimit.Count == 0)
                return true;
            return false;
        }

        private static bool InteractIncludesPlacementCategory(InteractInfo info, Placement_Second_Category category)
        {
            return info.placementLimit != null && info.placementLimit.Contains(category);
        }

        private bool CheckRoomLimit(InteractInfo info)
        {
            if (info.roomLimit == null || info.roomLimit.Count == 0) return true;
            foreach (var room in info.roomLimit)
            {
                if (_current_room != null &&room == _current_room.RoomName) return true;
            }
            return false;
        }
        private bool CheckPlacementLimit(InteractInfo info, out Interact interact, bool considerFindPath)
        {
            interact = null;
            if (IsInteractGroundOnly(info))
            {
                if (CanMoveToRandomPosToLie(considerFindPath))
                {
                    interact = Interact.CreateLie(liePosition, info);
                    return true;
                }
                return false;
            }

            if (_current_room == null) return false;

            var agg = new Interact { isLie = false, info = info };

            if (InteractIncludesPlacementCategory(info, Placement_Second_Category.Plant) && _current_room.RoomType == RoomType.Balcony && PlantManager.Instance != null)
            {
                foreach (var pot in _current_room.pots)
                {
                    if (CanMoveToPot(pot, out Vector3 position, considerFindPath))
                        agg.AddCandidate(position, null, pot, null);
                }
            }

            foreach (var placement in _current_room.placements)
            {
                if (!InteractIncludesPlacementCategory(info, placement.info.second_Category))
                    continue;
                if (!interactPositions.TryGetPositionInfo(placement.Name, info.interactName, out var data))
                {
                    Debug.LogWarning("该家具缺少交互点！已跳过该家具的交互判定");
                    continue;
                }
                if (NavMesh.SamplePosition(placement.GetInteractLocalTransform().TransformPoint(data.position), out var hit, 2f, NavMesh.AllAreas))
                {
                    if (CanMoveTo(hit.position) || !considerFindPath)
                        agg.AddCandidate(hit.position, placement, null, data);
                }
            }

            if (agg.positions.Count == 0) return false;
            agg.ChooseRandomCandidate();
            interact = agg;
            return true;
        }
        public void InteractSelectedPlacement()
        {
            List<Interact> interacts = new List<Interact>();
            for (int i = 0; i < interactInfos.Count; i++)
            {
                if (IsInteractGroundOnly(interactInfos[i])) continue;
                if (!InteractIncludesPlacementCategory(interactInfos[i], interactPlacement.info.second_Category)) continue;
                if (!interactPositions.TryGetPositionInfo(interactPlacement.Name, interactInfos[i].interactName, out var data)) continue;
                if (NavMesh.SamplePosition(interactPlacement.GetInteractLocalTransform().TransformPoint(data.position), out var hit, Mathf.Max(interactPlacement.gridData.width, interactPlacement.gridData.length) + 1, NavMesh.AllAreas))
                {
                    if (CanMoveTo(hit.position))
                        interacts.Add(Interact.CreateSinglePlacement(hit.position, interactPlacement, interactInfos[i], data));
                }
            }
            if (interacts.Count > 0)
            {
                if (string.IsNullOrEmpty(interactName))
                {
                    curInteract = interacts[Random.Range(0, interacts.Count)];
                    targetPosition = curInteract.position;
                }
                else
                {
                    foreach (var i in interacts)
                    {
                        if (i.info.interactName == interactName)
                        {
                            curInteract = i;
                            targetPosition = curInteract.position;
                            return;
                        }
                    }
                    Debug.LogWarning("该家具没有名为" + interactName + "的可执行交互！");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("该家具没有任何可执行交互！");
            }
        }

        /// <summary>
        /// 在指定 <see cref="interactPlacement"/> 与可选的 <see cref="interactName"/> 后，寻路至该家具并开始交互（需 Play 且状态机已初始化）。
        /// </summary>
        public void StartInteractWithSelectedPlacement()
        {
            if (interactPlacement == null)
            {
                Debug.LogWarning("IndoorMainCharacter: 请先在 Inspector 中指定 interactPlacement。");
                return;
            }
            if (stateMachine == null || moveState == null)
            {
                Debug.LogWarning("IndoorMainCharacter: 状态机尚未初始化，请在进入 Play 后稍候再试。");
                return;
            }
            InteractSelectedPlacement();
            if (curInteract == null)
            {
                Debug.LogWarning("IndoorMainCharacter: 未得到有效交互目标（检查家具二级分类与交互表、interactName 是否匹配）。");
                return;
            }
            var interact = curInteract;
            stateMachine.ChangeState(moveState);
            curInteract = interact;
        }

        public void InteractSlectedPot()
        {
            if (!CanMoveToPot(interactPot, out Vector3 position))
            {
                Debug.Log("该花盆没有路径前往");
                return;
            }
            List<Interact> interacts = new List<Interact>();
            for (int i = 0; i < interactInfos.Count; i++)
            {
                if (InteractIncludesPlacementCategory(interactInfos[i], Placement_Second_Category.Plant))
                    interacts.Add(Interact.CreateSinglePot(position, interactPot, interactInfos[i]));
            }
            if (interacts.Count > 0)
            {
                if (string.IsNullOrEmpty(interactName))
                {
                    curInteract = interacts[Random.Range(0, interacts.Count)];
                    targetPosition = curInteract.position;
                }
                else
                {
                    foreach (var i in interacts)
                    {
                        if (i.info.interactName == interactName)
                        {
                            curInteract = i;
                            targetPosition = curInteract.position;
                            return;
                        }
                    }
                    Debug.LogWarning("该盆栽没有名为" + interactName + "的可执行交互！");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("该盆栽没有任何可执行交互！");
            }
        }
        #endregion

        #region 点击交互
        public void ClickInteract()
        {

        }
        #endregion

        #endregion

        #region 寻路
        public void WrapPosition()
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 4, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
        public bool CanMoveToRandomPosToLie(bool considerFindPath = true)
        {
            if (liePosition != Vector3.zero)
            {
                return true;
            }
            if(_current_room ==null) return false;

            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = _current_room.GetRandomFloorPosition();
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
        public bool CanMoveToRoom(Room room, out Vector3 position)
        {
            List<NavMeshPath> paths = new List<NavMeshPath>();
            foreach (var door in NavMeshManager.Instance.entranceNameDic[room.RoomName].entrance)
            {
                if (!NavMesh.SamplePosition(door.position, out var hit, 1f, NavMesh.AllAreas)) continue;
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
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
        public bool CanMoveToRandomRoom(out Vector3 target)
        {
            var rooms = new List<Room>(RoomSystem.Instance.rooms);
            rooms.Remove(_current_room);
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
        public bool MoveToRandomPosToLie()
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = _current_room.GetRandomFloorPosition();
                if (NavMesh.SamplePosition(pos, out var hit, 2, NavMesh.AllAreas))
                {
                    if (!Physics.BoxCast(hit.position, new Vector3(1, 1, 1), Vector3.down, Quaternion.identity, 0, 1 << 6))
                    {
                        if (CanMoveTo(hit.position))
                        {
                            targetPosition = hit.position;
                            agent.SetDestination(targetPosition);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public bool CanMoveTo(Vector3 target)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
            return false;
        }
        public Vector3 GetRandomPointOnCircle(Transform target, float radius = 2.5f)
        {
            // 生成随机角度（0到360度）
            float randomAngle = Random.Range(0f, 360f);

            // 将角度转换为弧度
            float angleInRadians = randomAngle * Mathf.Deg2Rad;

            // 计算圆上的点坐标
            Vector3 point = target.position + new Vector3(
                Mathf.Cos(angleInRadians) * radius,
                0f,
                Mathf.Sin(angleInRadians) * radius
            );

            return point;
        }
        #endregion

        #region 派遣
        private void OnDispatchStart()
        {
            gameObject.SetActive(false);
        }
        private void OnDispatchEnd()
        {
            gameObject.SetActive(true);
        }
        private void InitDispatchState()
        {

        }
        #endregion

        #region 点击
        public bool OnClick(Vector3 p)
        {
            PopManager.Instance.Pop();
            return true;
        }
        #endregion
    }
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
