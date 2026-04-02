using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class MainCharacterState : CharacterState
    {
        protected IndoorMainCharacter mainCharacter;
        public MainCharacterState(StateMachine stateMachine, IndoorMainCharacter character, string animationBoolName) : base(stateMachine, character, animationBoolName)
        {
            mainCharacter = character;
        }
    }
}