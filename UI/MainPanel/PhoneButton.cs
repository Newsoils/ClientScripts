using System;
using System.Collections;
using CLIP.Framework_Core.Event;
using CLIP.Framework_Unity;
using UnityEngine;
using UnityEngine.UI;

public class PhoneButton : SingletonMono<PhoneButton>
{
    public Button phoneButton;
    public Animator animator1;
    public Animator animator2;
    public GameObject phonePanel;
    private void Start()
    {
        phoneButton.gameObject.SetActive(true);
        phoneButton.onClick.AddListener(() => StartCoroutine(OpenPhone()));
        animator1.gameObject.SetActive(false);
        animator2.gameObject.SetActive(false);
        phonePanel.gameObject.SetActive(false);
    }
    public IEnumerator OpenPhone()
    {
        phoneButton.gameObject.SetActive(false);
        animator1.gameObject.SetActive(true);
        yield return PlayAnimation(animator1, "OpenPhone", "OpenPhone");
        EvtDsp.TriggerEvt(EvtNames.OnPhonePanelOpen);
        animator1.gameObject.SetActive(false);
        animator2.gameObject.SetActive(true);
        yield return PlayAnimation(animator2, "OpenPhone2", "OpenPhone2");
        phonePanel.gameObject.SetActive(true);
        animator2.SetBool("Open", true);
    }
    public void ClosePhone(Action onComplete = null)
    {
        StartCoroutine(ClosePhoneRoutine(onComplete));
    }
    public IEnumerator ClosePhoneRoutine(Action onComplete = null)
    {
        animator2.SetBool("Open", false);
        phonePanel.gameObject.SetActive(false);
        yield return PlayAnimationInReverse(animator2, "OpenPhone2", "OpenPhone2");
        animator2.gameObject.SetActive(false);
        animator1.gameObject.SetActive(true);
        EvtDsp.TriggerEvt(EvtNames.OnPhonePanelClose);
        yield return PlayAnimationInReverse(animator1, "OpenPhone", "OpenPhone");
        phoneButton.gameObject.SetActive(true);
        animator1.gameObject.SetActive(false);
        onComplete?.Invoke();

    }
    private IEnumerator PlayAnimation(Animator animator, string triggerName, string animationName)
    {
        animator.SetBool(triggerName, true);
        yield return null;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        while (!info.IsName(animationName) || info.normalizedTime < 1.0)
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        animator.SetBool(triggerName, false);
        Debug.Log("动画播放完毕");
    }

    private IEnumerator PlayAnimationInReverse(Animator animator, string animationName, string boolName)
    {
        animator.SetBool(boolName, false);
        animator.Play(animationName, 0, 1f);
        animator.Update(0f);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.length;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = 1f - Mathf.Clamp01(elapsed / duration);
            animator.Play(animationName, 0, normalizedTime);
            animator.Update(0f);
            yield return null;
        }

        animator.Play(animationName, 0, 0f);
        animator.Update(0f);
        Debug.Log("动画播放完毕");
    }
    public void OpenPanel()
    {
        phoneButton.gameObject.SetActive(true);
    }
    public void ClosePanel()
    {
        phoneButton.gameObject.SetActive(false);
    }
}
