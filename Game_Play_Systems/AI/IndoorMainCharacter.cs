using System;
using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Project_Mouse.Game_Play_System.Dispatch_System;
using CLIP.Project_Mouse.Scene_View_Control;
using UnityEngine;
using UnityEngine.AI;

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
        [Tooltip("头部碰撞体，仅躺地板时启用")]
        public Collider headCollider;

        public float radius = 8;

        public static IndoorMainCharacter _instance;

        public bool _is_static = false;

        public CharacterNavigator Navigator { get; private set; }
        public InteractSelector Selector { get; private set; }

        #region 状态
        public MainCharacterIdleState idleState { get; private set; }
        public MainCharacterMoveState moveState { get; private set; }

        private void InitStateMachine()
        {
            stateMachine = new StateMachine();
            idleState = new MainCharacterIdleState(stateMachine, this, "stand");
            moveState = new MainCharacterMoveState(stateMachine, this, "walk");

            Navigator = new CharacterNavigator(agent, transform);
            Selector = new InteractSelector(Navigator, interactPositions);
            Selector.InitInteract(stateMachine, this);

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
            EvtDsp.AddEvt(EvtNames.Dispatch_VisualsSync, ApplyDispatchVisualsFromManagerState);
            EvtDsp.AddEvt<Room>(EvtNames.SwitchRoom, SwitchRendererState);
            EvtDsp.AddEvt(EvtNames.ReSetMainCharacterRenderer, SwitchRendererState);
            EvtDsp.AddEvt<bool>(EvtNames.SetMainCharacterState, SetRendererEnable);

            _instance = this;
            if (headCollider != null) headCollider.enabled = false;
            ApplyDispatchVisualsFromManagerState();

            if (isFirstOpen)
            {
                WrapPosition();
                InitStateMachine();
            }
            else
            {
                InitStateMachine();
                StartCoroutine(nameof(InitRandomPositionAndInteract));
            }
            _current_room = RoomSystem.GetRoomByFloorPosition(transform.position);
        }

        private void OnDestroy()
        {
            EvtDsp.RemoveEvt(EvtNames.ReSetMainCharacterRenderer, SwitchRendererState);
            EvtDsp.RemoveEvt(EvtNames.Dispatch_On_Start, OnDispatchStart);
            EvtDsp.RemoveEvt(EvtNames.Dispatch_On_End, OnDispatchEnd);
            EvtDsp.RemoveEvt(EvtNames.Dispatch_VisualsSync, ApplyDispatchVisualsFromManagerState);
            EvtDsp.RemoveEvt<Room>(EvtNames.SwitchRoom, SwitchRendererState);
            EvtDsp.RemoveEvt<bool>(EvtNames.SetMainCharacterState, SetRendererEnable);
        }

        private IEnumerator InitRandomPositionAndInteract()
        {
            yield return new WaitForEndOfFrame();
            SelectInteract(false);
            agent.enabled = false;

            if (curInteract != null)
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

            if (agent.isOnOffMeshLink && !isPassingRoom)
            {
                isPassingRoom = true;
                var desPos = agent.currentOffMeshLinkData.endPos;
                var room = RoomSystem.GetRoomByFloorPosition(desPos);
                if (room != null) _current_room = room;
                SwitchRendererState(null);
            }
            if (!agent.isOnOffMeshLink && isPassingRoom)
            {
                isPassingRoom = false;
            }
        }

        public void SwitchRendererState(Room room)
        {
            if (_current_room != null && RoomSystem.currentRoom != null)
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
        public Room _current_room;
        public Interact curInteract { get; set; }

        public void StartInteract()
        {
            if (curInteract != null)
                stateMachine.ChangeState(Selector.InteractStates[curInteract.info]);
        }

        public void SelectInteract(bool considerFindPath = true)
        {
            curInteract = Selector.SelectInteract(_current_room, ref liePosition, considerFindPath);
        }

        public void InteractSelectedPlacement()
        {
            var result = Selector.InteractSelectedPlacement(interactPlacement, interactName);
            if (result != null)
            {
                curInteract = result;
                targetPosition = curInteract.position;
            }
        }

        /// <summary>
        /// 在指定 <see cref="interactPlacement"/> 与可选的 <see cref="interactName"/> 后，寻路至该家具并开始交互。
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
            var result = Selector.InteractSelectedPot(interactPot, interactName);
            if (result != null)
            {
                curInteract = result;
                targetPosition = curInteract.position;
            }
        }

        public void ClickInteract()
        {
        }
        #endregion

        #region 寻路（委托给 Navigator）
        public void WrapPosition() => Navigator.WrapPosition();
        public bool CanMoveTo(Vector3 target) => Navigator.CanMoveTo(target);
        public bool CanMoveToRoom(Room room, out Vector3 position) => Navigator.CanMoveToRoom(room, out position);

        public bool CanMoveToRandomRoom(out Vector3 target) => Navigator.CanMoveToRandomRoom(_current_room, out target);

        public bool MoveToRandomPosToLie()
        {
            if (Navigator.MoveToRandomPosToLie(_current_room, out var pos))
            {
                targetPosition = pos;
                return true;
            }
            return false;
        }

        public bool CanMoveToRandomPosToLie(bool considerFindPath = true)
        {
            if (liePosition != Vector3.zero) return true;
            if (Navigator.CanMoveToRandomPosToLie(_current_room, considerFindPath, out var pos))
            {
                liePosition = pos;
                return true;
            }
            return false;
        }
        #endregion

        #region 派遣
        private void ApplyDispatchVisualsFromManagerState()
        {
            //if (Dispatch_Manager.Instance._player_dispatch_state.player_state == "On_Dispatch")
            //    gameObject.SetActive(false);
            //else
            //    gameObject.SetActive(true);
        }

        private void OnDispatchStart() => ApplyDispatchVisualsFromManagerState();
        private void OnDispatchEnd() => ApplyDispatchVisualsFromManagerState();
        #endregion

        #region 点击
        public bool OnClick(Vector3 p)
        {
            PopManager.Instance.Pop();
            return true;
        }
        #endregion
    }
}


