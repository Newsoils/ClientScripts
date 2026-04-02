using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class CharacterState
    {
        protected StateMachine stateMachine;
        protected CharacterBase character;
        protected string animationBoolName;
        public Action onEnter;
        public Action onExit;
        public CharacterState(StateMachine stateMachine, CharacterBase character, string animationBoolName)
        {
            this.stateMachine = stateMachine;
            this.character = character;
            this.animationBoolName = animationBoolName;
        }
        public virtual void Enter()
        {
            onEnter?.Invoke();
            character.animator.SetBool(animationBoolName, true);
        }
        public virtual void Update()
        {

        }
        public virtual void Exit()
        {
            onExit?.Invoke();
            character.animator.SetBool(animationBoolName, false);
        }
        public virtual void ReEnter()
        {

        }
        public virtual string GetStateInfo()
        {
            return "";
        }
    }
}