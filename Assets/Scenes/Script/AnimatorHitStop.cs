using System.Collections;
using UnityEngine;

/// <summary>
/// 在不暂停全局 Time.timeScale 的前提下短暂暂停 Animator。
/// 多次命中会延长暂停时间，并在结束后恢复原来的 Animator.speed。
/// </summary>
[DisallowMultipleComponent]
public sealed class AnimatorHitStop : MonoBehaviour
{
    private Animator targetAnimator;
    private Coroutine restoreCoroutine;
    private float originalSpeed = 1f;
    private float resumeAtRealtime;
    private bool isPaused;

    public void Apply(float duration)
    {
        if (duration <= 0f)
            return;

        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();
        if (targetAnimator == null)
            return;

        if (!isPaused)
        {
            originalSpeed = targetAnimator.speed;
            isPaused = true;
        }

        resumeAtRealtime = Mathf.Max(resumeAtRealtime, Time.realtimeSinceStartup + duration);
        targetAnimator.speed = 0f;

        if (restoreCoroutine == null)
            restoreCoroutine = StartCoroutine(RestoreWhenElapsed());
    }

    private IEnumerator RestoreWhenElapsed()
    {
        while (Time.realtimeSinceStartup < resumeAtRealtime)
            yield return null;

        Restore();
        restoreCoroutine = null;
    }

    private void OnDisable()
    {
        Restore();
        restoreCoroutine = null;
    }

    private void Restore()
    {
        if (isPaused && targetAnimator != null)
            targetAnimator.speed = originalSpeed;

        isPaused = false;
        resumeAtRealtime = 0f;
    }
}
