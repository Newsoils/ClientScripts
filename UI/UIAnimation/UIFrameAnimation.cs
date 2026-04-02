using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class UIFrameAnimation : MonoBehaviour
{
    [System.Serializable]
    public class AnimationEvent : UnityEvent { }

    [Header("序列帧设置")]
    public Sprite[] frames;
    [Range(1, 60)] public float frameRate = 30f;

    [Header("播放设置")]
    public PlayMode playMode = PlayMode.Loop;
    public bool autoPlay = true;
    public bool unscaledTime = false;

    [Header("事件")]
    public AnimationEvent onAnimationStart;
    public AnimationEvent onAnimationEnd;
    public AnimationEvent onFrameChanged;

    public Image image;
    private Coroutine animationCoroutine;

    public enum PlayMode
    {
        Once,           // 播放一次
        Loop,           // 循环播放
        PingPong,       // 来回播放
        Reverse         // 反向播放
    }

    void Start()
    {
        image = GetComponent<Image>();
        if (autoPlay && frames.Length > 0)
        {
            Play();
        }
    }

    public void Play()
    {
        if (frames == null || frames.Length <= 0) return;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(PlayAnimation());
        onAnimationStart?.Invoke();
    }

    public void Stop()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    public void Pause()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }

    public void SetFrame(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < frames.Length)
        {
            image.sprite = frames[frameIndex];
            onFrameChanged?.Invoke();
        }
    }

    private IEnumerator PlayAnimation()
    {
        int currentIndex = 0;
        bool forward = true;
        float waitTime = 1f / frameRate;

        while (true)
        {
            // 设置当前帧
            image.sprite = frames[currentIndex];
            onFrameChanged?.Invoke();

            // 等待下一帧
            if (unscaledTime)
            {
                yield return new WaitForSecondsRealtime(waitTime);
            }
            else
            {
                yield return new WaitForSeconds(waitTime);
            }

            // 计算下一帧索引
            switch (playMode)
            {
                case PlayMode.Once:
                    currentIndex++;
                    if (currentIndex >= frames.Length)
                    {
                        onAnimationEnd?.Invoke();
                        yield break;
                    }
                    break;

                case PlayMode.Loop:
                    currentIndex = (currentIndex + 1) % frames.Length;
                    break;

                case PlayMode.PingPong:
                    if (forward)
                    {
                        currentIndex++;
                        if (currentIndex >= frames.Length - 1)
                        {
                            forward = false;
                            currentIndex = frames.Length - 1;
                        }
                    }
                    else
                    {
                        currentIndex--;
                        if (currentIndex <= 0)
                        {
                            forward = true;
                            currentIndex = 0;
                        }
                    }
                    break;

                case PlayMode.Reverse:
                    currentIndex--;
                    if (currentIndex < 0)
                    {
                        currentIndex = frames.Length - 1;
                    }
                    break;
            }
        }
    }
}