using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public class CharacterBase : MonoBehaviour
    {
        #region 基础组件
        public Animator animator;
        public NavMeshAgent agent;
        #endregion

        #region 状态机
        protected StateMachine stateMachine;
        public string curStateInfo;
        #endregion

        #region 交互
        public Vector3 targetPosition;
        public float minDist;
        #endregion

        #region 动画

        public IEnumerator PlaySingleAnimationCo(string boolName)
        {
            animator.SetBool(boolName, true);
            yield return null;
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            while (info.normalizedTime < 1.0)
            {
                yield return null;
            }
            animator.SetBool(boolName, false);
        }
        public void StartLoopAnimation(string boolName)
        {
            animator.SetBool(boolName, true);
        }
        public void StopLoopAnimation(string boolName)
        {
            animator.SetBool(boolName, false);
        }
        #endregion

    }
}
