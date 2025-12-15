using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SimpleScaleOnClick : MonoBehaviour
{
    [Tooltip("Target scale multiplier to pop to (1 = original, 1.2 = +20%).")]
    public float targetScale = 1.2f;
    [Tooltip("Time to scale up (seconds).")]
    public float upTime = 0.12f;
    [Tooltip("Time to scale back down (seconds).")]
    public float downTime = 0.18f;
    [Tooltip("Optional overshoot (punch).")]
    public float overshoot = 1.08f;

    Vector3 originalScale;
    Coroutine running;

    void Awake() => originalScale = transform.localScale;

    // Works in editor and builds when object has a Collider2D and a Camera can see it
    void OnMouseDown()
    {
        PlayPop();
    }

    // Public call if you want to trigger from other scripts
    public void PlayPop()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(PopCoroutine());
    }

    System.Collections.IEnumerator PopCoroutine()
    {
        // scale up with a tiny overshoot
        Vector3 upTarget = originalScale * targetScale * overshoot;
        float t = 0f;
        while (t < upTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / upTime);
            transform.localScale = Vector3.Lerp(originalScale, upTarget, progress);
            yield return null;
        }

        // scale back to targetScale (slightly smaller than overshoot) quickly
        t = 0f;
        Vector3 midTarget = originalScale * targetScale;
        while (t < downTime)
        {
            t += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, t / downTime);
            transform.localScale = Vector3.Lerp(upTarget, midTarget, progress);
            yield return null;
        }

        // optional: settle back to original if you want a blink, otherwise keep midTarget
        // Uncomment to return to original size:
        // t = 0f;
        // while (t < downTime)
        // {
        //     t += Time.deltaTime;
        //     float progress = Mathf.SmoothStep(0f, 1f, t / downTime);
        //     transform.localScale = Vector3.Lerp(midTarget, originalScale, progress);
        //     yield return null;
        // }

        transform.localScale = midTarget; // ensure exact
        running = null;
    }
}