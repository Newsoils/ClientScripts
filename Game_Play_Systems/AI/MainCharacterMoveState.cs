using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
public class MainCharacterMoveState : MainCharacterState
    {
        public MainCharacterMoveState(StateMachine stateMachine, IndoorMainCharacter character, string animationBoolName) : base(stateMachine, character, animationBoolName)
        {
        }
        public override void Enter()
        {
            base.Enter();
            mainCharacter.agent.SetDestination(mainCharacter.targetPosition);
        }
        public override void Update()
        {
            base.Update();
            if(Vector3.Distance(mainCharacter.targetPosition ,character.transform.position)<character.minDist || character.agent.isStopped)
            {
                if(mainCharacter.curInteract == null)
                {
                    mainCharacter.SelectInteract();
                    if(mainCharacter.curInteract!=null)
                    {
                        mainCharacter.targetPosition = mainCharacter.curInteract.position;
                        mainCharacter.agent.SetDestination(mainCharacter.targetPosition);
                    }
                }
                else
                {
                    mainCharacter.StartInteract();
                }
            }
        }
        public override void ReEnter()
        {
            base.ReEnter();
            mainCharacter.agent.SetDestination(mainCharacter.targetPosition);
        }
        public override string GetStateInfo()
        {
            if(mainCharacter.curInteract != null)
            {
                return "正在移动至" + mainCharacter.targetPosition + mainCharacter.curInteract.info.interactName;
            }
            else
            {
                return "正在移动至" + mainCharacter.targetPosition + "切换房间到" + RoomSystem.GetRoomByFloorPosition(mainCharacter.targetPosition);
            }
        }
    }
}

