using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class StateMachine
    {
        public CharacterState curState;
        public void Init(CharacterState state)
        {
            curState = state;
            curState.Enter();
        }
        public void ChangeState(CharacterState newState)
        {
            if (curState == newState)
            {
                curState.ReEnter();
                return;
            }
            curState.Exit();
            curState = newState;
            curState.Enter();
        }
        public void Update()
        {
            curState.Update();
        }
        public string GetStateInfo()
        {
            return curState.GetStateInfo();
        }
    }
}
