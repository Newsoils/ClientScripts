using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class MainCharacterIdleState : MainCharacterState
    {
        float idleTimer;
        public MainCharacterIdleState(StateMachine stateMachine, IndoorMainCharacter character, string animationBoolName) : base(stateMachine, character, animationBoolName)
        {
        }
        public override void Enter()
        {
            base.Enter();
            idleTimer = 3;
        }
        public override void Update()
        {
            base.Update();
            idleTimer -= Time.deltaTime;
            if(idleTimer < 0)
            {
                List<bool> isMoveToOtherRoom = new List<bool> { true, true, false};
                if(isMoveToOtherRoom[Random.Range(0, isMoveToOtherRoom.Count)])
                {
                    if(mainCharacter.CanMoveToRandomRoom(out Vector3 position))
                    {
                        mainCharacter.targetPosition = position;
                        stateMachine.ChangeState(mainCharacter.moveState);
                        return;
                    }
                }
                mainCharacter.SelectInteract(true);
                if(mainCharacter.curInteract!=null)
                {
                    mainCharacter.targetPosition = mainCharacter.curInteract.position;
                    stateMachine.ChangeState(mainCharacter.moveState);
                }
            }
        }
        public override string GetStateInfo()
        {
            return "正在等待,剩余"+idleTimer.ToString("0.0")+"秒";
        }
    }
}
