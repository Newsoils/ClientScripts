using CLIP.Project_Mouse.Kernel;
using DG.Tweening;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
public class MainCharacterInteractState : MainCharacterState
    {
        public InteractInfo info;
        float interactTimer;
        public MainCharacterInteractState(StateMachine stateMachine, IndoorMainCharacter character, InteractInfo info) : base(stateMachine, character, info.animationBoolName)
        {
            this.info = info;
        }
        public override void Enter()
        {
            if (!string.IsNullOrEmpty(info.stateName))
            {
                float stateLen = GetStateLengthByStateName(mainCharacter.animator, info.stateName);
                interactTimer = stateLen > 0f ? stateLen : Random.Range(info.minInteractTime, info.maxInteractTime);
            }
            else
            {
                interactTimer = Random.Range(info.minInteractTime, info.maxInteractTime);
            }
            base.Enter();

            if(mainCharacter.curInteract.placement != null)
            {
                mainCharacter.agent.enabled = false;
                var posInfo = GetInteractPosition();
                var interactSpace = mainCharacter.curInteract.placement.GetInteractLocalTransform();
                mainCharacter.transform.position = interactSpace.TransformPoint(posInfo.position);
                mainCharacter.transform.rotation = interactSpace.rotation * posInfo.rotation;
            }
            else if(mainCharacter.curInteract.pot != null)
            {
                Vector3 direction = mainCharacter.curInteract.pot.plantRoot.position - mainCharacter.transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                mainCharacter.agent.enabled = false;
                // 使用DORotate进行旋转
                mainCharacter.transform.DORotate(targetRotation.eulerAngles, 0.3f)
                    .SetEase(Ease.OutQuad);
            }
            if(!string.IsNullOrEmpty(info.itemRequire))
            {
                if(mainCharacter.interactItemDic.TryGetValue(info.itemRequire, out InteractItem item))
                {
                    item.itemObj.SetActive(true);
                }
            }
        }

        /// <summary>按 Animator 状态名取当前层上该 state's clip 时长（秒）；在 layer 0 上短暂 Play 采样后恢复原状态。</summary>
        private static float GetStateLengthByStateName(Animator animator, string stateName, int layerIndex = 0)
        {
            if (animator == null || string.IsNullOrEmpty(stateName) || !animator.isActiveAndEnabled)
                return 0f;

            float savedSpeed = animator.speed;
            animator.speed = 1f;

            var prev = animator.GetCurrentAnimatorStateInfo(layerIndex);
            int prevPathHash = prev.fullPathHash;
            float prevNorm = prev.normalizedTime;

            animator.Play(stateName, layerIndex, 0f);
            animator.Update(0.0001f);
            var sampled = animator.GetCurrentAnimatorStateInfo(layerIndex);
            float length = 0f;
            if (sampled.IsName(stateName) || sampled.shortNameHash == Animator.StringToHash(stateName))
                length = sampled.length;

            animator.Play(prevPathHash, layerIndex, prevNorm);
            animator.Update(0.0001f);
            animator.speed = savedSpeed;
            return length;
        }
        public override void Update()
        {
            base.Update();
            interactTimer -= Time.deltaTime;
            if (interactTimer < 0)
            {
                stateMachine.ChangeState(mainCharacter.idleState);
            }

        }
        public override void Exit()
        {
            base.Exit();
            if (!string.IsNullOrEmpty(info.itemRequire))
            {
                if (mainCharacter.interactItemDic.TryGetValue(info.itemRequire, out InteractItem item))
                {
                    item.itemObj.SetActive(false);
                }
            }
            mainCharacter.liePosition = Vector3.zero;
            mainCharacter.curInteract = null;
            mainCharacter.agent.enabled = true;
            mainCharacter.WrapPosition();
        }
        private InteractPositionInfo GetInteractPosition()
        {
            Interact interact = mainCharacter.curInteract;
            if (interact.ChosenPositionInfo != null)
                return interact.ChosenPositionInfo;
            if (interact.placement != null && mainCharacter.interactPositions.TryGetPositionInfo(interact.placement.info.room_placement_name, interact.info.interactName, out var posInfo))
                return posInfo;
            return null;
        }
        public override string GetStateInfo()
        {
            return "正在" + info.interactName + ",剩余" + interactTimer.ToString("0.0") + "秒";
        }
    }
}
