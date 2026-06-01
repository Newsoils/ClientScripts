using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class MainCharacterMoveState : MainCharacterState
    {
        private const float StuckCheckInterval = 0.5f;
        private const float StuckMoveThreshold = 0.05f;
        private const float StuckTimeout = 5f;

        private Vector3 lastCheckPosition;
        private float stuckCheckTimer;
        private float stuckDuration;

        public MainCharacterMoveState(StateMachine stateMachine, IndoorMainCharacter character, string animationBoolName) : base(stateMachine, character, animationBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            ResetStuckCheck();
            mainCharacter.agent.SetDestination(mainCharacter.targetPosition);
        }

        public override void Update()
        {
            base.Update();
            if (Vector3.Distance(mainCharacter.targetPosition, character.transform.position) < character.minDist || character.agent.isStopped)
            {
                if (mainCharacter.curInteract == null)
                {
                    mainCharacter.SelectInteract();
                    if (mainCharacter.curInteract != null)
                    {
                        mainCharacter.targetPosition = mainCharacter.curInteract.position;
                        mainCharacter.agent.SetDestination(mainCharacter.targetPosition);
                        ResetStuckCheck();
                    }
                }
                else
                {
                    mainCharacter.StartInteract();
                }
                return;
            }

            UpdateStuckCheck();
        }

        public override void ReEnter()
        {
            base.ReEnter();
            ResetStuckCheck();
            mainCharacter.agent.SetDestination(mainCharacter.targetPosition);
        }

        public override string GetStateInfo()
        {
            if (mainCharacter.curInteract != null)
            {
                return "正在移动至" + mainCharacter.targetPosition + mainCharacter.curInteract.info.interactName;
            }
            else
            {
                return "正在移动至" + mainCharacter.targetPosition + "切换房间到" + RoomSystem.GetRoomByFloorPosition(mainCharacter.targetPosition);
            }
        }

        private void ResetStuckCheck()
        {
            lastCheckPosition = character.transform.position;
            stuckCheckTimer = 0f;
            stuckDuration = 0f;
        }

        private void UpdateStuckCheck()
        {
            stuckCheckTimer += Time.deltaTime;
            if (stuckCheckTimer < StuckCheckInterval)
                return;

            var currentPosition = character.transform.position;
            var movedDistance = Vector3.Distance(currentPosition, lastCheckPosition);
            if (movedDistance < StuckMoveThreshold)
            {
                stuckDuration += stuckCheckTimer;
                if (stuckDuration >= StuckTimeout)
                {
                    GiveUpTargetAndEnterIdle();
                    return;
                }
            }
            else
            {
                stuckDuration = 0f;
            }

            lastCheckPosition = currentPosition;
            stuckCheckTimer = 0f;
        }

        private void GiveUpTargetAndEnterIdle()
        {
            mainCharacter.curInteract = null;
            mainCharacter.agent.ResetPath();
            stateMachine.ChangeState(mainCharacter.idleState);
        }
    }
}

