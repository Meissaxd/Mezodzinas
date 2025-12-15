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

    [Header("Click SFX (optional)")]
    [Tooltip("Audio clip to play when the bush is clicked.")]
    public AudioClip clickClip;
    [Range(0f, 1f)] public float clickVolume = 1f;
    [Tooltip("Optional shared/persistent AudioSource. If null, PlayClipAtPoint will be used.")]
    public AudioSource persistentAudioSource;
    [Tooltip("Randomize pitch for variety (only works when using persistentAudioSource).")]
    public bool randomizePitch = false;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Header("Optional integration")]
    [Tooltip("If assigned, this will call rng.Generate() on click.")]
    public RandomNumberGenerator rng;
    [Tooltip("If true, call rng.Generate() after playing the SFX/pop.")]
    public bool callRngOnClick = false;

    Vector3 originalScale;
    Coroutine running;

    void Awake()
    {
        originalScale = transform.localScale;

        // If rng not set but there is one in scene, find it (optional)
        if (callRngOnClick && rng == null)
            rng = FindObjectOfType<RandomNumberGenerator>();
    }

    // Works in editor and builds when object has a Collider2D and a Camera can see it
    void OnMouseDown()
    {
        PlayClickSound();
        PlayPop();
    }

    // Public call if you want to trigger from other scripts
    public void PlayPop()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(PopCoroutine());
    }

    private void PlayClickSound()
    {
        if (clickClip == null) return;

        float vol = Mathf.Clamp01(clickVolume);

        if (persistentAudioSource != null && persistentAudioSource.gameObject != this.gameObject)
        {
            if (randomizePitch)
            {
                float oldPitch = persistentAudioSource.pitch;
                persistentAudioSource.pitch = Random.Range(pitchMin, pitchMax);
                persistentAudioSource.PlayOneShot(clickClip, vol);
                persistentAudioSource.pitch = oldPitch;
            }
            else
            {
                persistentAudioSource.PlayOneShot(clickClip, vol);
            }
        }
        else
        {
            // fallback: temporary audio source at the bush position so sound continues even if object is destroyed
            AudioSource.PlayClipAtPoint(clickClip, transform.position, vol);
        }
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

        transform.localScale = midTarget; // ensure exact
        running = null;

        // Optionally call RNG after the pop finishes (or call earlier if you prefer)
        if (callRngOnClick && rng != null)
        {
            rng.Generate();
        }
    }
}