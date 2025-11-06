using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomNumberGenerator : MonoBehaviour
{
    [Header("Per-number sprites (index 0 -> number 1, index 4 -> number 5)")]
    [SerializeField] private Sprite[] numberSprites = new Sprite[5];

    [Header("Display (UI Image)")]
    [Tooltip("Target UI Image used when displaying sprites. If left empty a Canvas + Image will be created automatically.")]
    [SerializeField] private Image targetImage;

    [Header("Audio")]
    [Tooltip("Audio clip to play when the final image is shown.")]
    [SerializeField] private AudioClip finalAudioClip;
    [Tooltip("Optional AudioSource used to play the final sound. If empty one will be created automatically.")]
    [SerializeField] private AudioSource targetAudioSource;
    [SerializeField, Range(0f, 1f), Tooltip("Volume at which final sound is played (PlayOneShot).")] private float audioVolume = 1f;

    [Header("Display Timing")]
    [SerializeField, Tooltip("Seconds the final generated image remains visible before hiding. Set <= 0 to keep it visible indefinitely.")]
    private float displayDuration = 2f;

    [Header("Generation sequence")]
    [SerializeField, Tooltip("Enable the visual 'generating' animation (flashing). If disabled the final number is chosen immediately.")]
    private bool enableVisualGeneration = true;
    [SerializeField, Tooltip("Seconds the 'generating' animation runs before the final number is chosen.")]
    private float generationDuration = 1.5f;
    [SerializeField, Tooltip("Flash interval at the start (seconds). Smaller = faster flashes.")]
    private float initialFlashInterval = 0.03f;
    [SerializeField, Tooltip("Flash interval at the end (seconds). Larger = slower flashes).")]
    private float finalFlashInterval = 0.20f;
    [SerializeField, Tooltip("Exponent used to ease the flash interval (higher => stronger slow-down).")]
    private float easeExponent = 2f;

    [Header("Cooldown")]
    [SerializeField, Tooltip("Seconds to wait after generating before another generation is allowed.")]
    private float cooldownSeconds = 1f;

    // Next time generation is allowed
    private float nextAvailableTime = 0f;

    // Last generated value (1..5)
    public int LastValue { get; private set; } = 1;

    // Last sprite for LastValue (may be null)
    public Sprite LastSprite => GetSpriteForNumber(LastValue);

    // Runtime-created Image used when no targetImage is assigned
    private Image runtimeImage;

    // Runtime-created AudioSource used when no targetAudioSource is assigned
    private AudioSource runtimeAudioSource;

    // Sequence state
    private bool isGenerating;

    // Hide coroutine handle (so we can cancel/reschedule hides)
    private Coroutine hideCoroutine;

    // Remaining cooldown in seconds (read-only)
    public float CooldownRemaining => Mathf.Max(0f, nextAvailableTime - Time.time);
    public bool IsOnCooldown => Time.time < nextAvailableTime;
    public bool IsGenerating => isGenerating;

    // Initiates the generation sequence.
    // If on cooldown or a sequence is already running, prints "not so fast".
    public int Generate()
    {
        if (isGenerating || Time.time < nextAvailableTime)
        {
            Debug.Log("not so fast");
            return LastValue;
        }

        // Cancel any pending auto-hide so the image won't disappear mid-sequence
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (enableVisualGeneration && generationDuration > 0f)
            StartCoroutine(GenerationSequence());
        else
            FinalizeGenerationImmediate();

        return LastValue;
    }

    private IEnumerator GenerationSequence()
    {
        isGenerating = true;

        var image = targetImage ?? EnsureRuntimeImage();
        if (image == null)
        {
            Debug.LogWarning("RandomNumberGenerator: no Image available to display generation.");
            isGenerating = false;
            yield break;
        }

        // Cancel any pending hide and ensure visible for the sequence
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        image.enabled = true;

        // build list of valid sprite indices (only non-null)
        var validIndices = new List<int>();
        if (numberSprites != null)
        {
            for (int i = 0; i < numberSprites.Length; i++)
                if (numberSprites[i] != null)
                    validIndices.Add(i);
        }

        float startTime = Time.time;

        // Flash random sprites while timer runs, starting fast and slowing down
        while (Time.time - startTime < Mathf.Max(0f, generationDuration))
        {
            float elapsed = Time.time - startTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, generationDuration));
            float eased = Mathf.Pow(t, Mathf.Max(0.0001f, easeExponent)); // ease-out curve
            float currInterval = Mathf.Lerp(initialFlashInterval, finalFlashInterval, eased);
            currInterval = Mathf.Max(0.001f, currInterval);

            // pick and show a random sprite for the flash
            if (validIndices.Count > 0)
            {
                int pick = validIndices[Random.Range(0, validIndices.Count)];
                var s = numberSprites[pick];
                image.sprite = s;
                if (s != null) image.SetNativeSize();
            }
            else if (numberSprites != null && numberSprites.Length > 0)
            {
                int pick = Random.Range(0, numberSprites.Length);
                var s = numberSprites[pick];
                image.sprite = s;
                if (s != null) image.SetNativeSize();
            }

            yield return new WaitForSeconds(currInterval);
        }

        // After animation finishes pick final number and display its sprite
        FinalizeGenerationInternal(image);

        // start cooldown after the sequence completes
        nextAvailableTime = Time.time + Mathf.Max(0f, cooldownSeconds);
        isGenerating = false;
    }

    // Immediate finalization (used when visuals disabled)
    private void FinalizeGenerationImmediate()
    {
        // Cancel any pending hide so the new display is scheduled fresh
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        var image = targetImage ?? EnsureRuntimeImage();
        FinalizeGenerationInternal(image);
        nextAvailableTime = Time.time + Mathf.Max(0f, cooldownSeconds);
    }

    private void FinalizeGenerationInternal(Image image)
    {
        LastValue = Random.Range(1, 6); // 1..5 inclusive
        var finalSprite = GetSpriteForNumber(LastValue);

        if (image == null)
        {
            if (finalSprite != null)
                Debug.Log($"RandomNumberGenerator generated: {LastValue} -> Sprite \"{finalSprite.name}\" (no Image to display)");
            else
                Debug.LogWarning($"RandomNumberGenerator generated: {LastValue} -> No sprite assigned for this number.");
            return;
        }

        if (finalSprite != null)
        {
            image.sprite = finalSprite;
            image.enabled = true;
            image.SetNativeSize();
            image.transform.SetAsLastSibling();
            Debug.Log($"RandomNumberGenerator generated: {LastValue} -> Sprite \"{finalSprite.name}\" (UI Image)");

            // play single final audio clip (if any)
            if (finalAudioClip != null)
            {
                var audio = targetAudioSource ?? EnsureRuntimeAudioSource();
                if (audio != null)
                {
                    audio.PlayOneShot(finalAudioClip, Mathf.Clamp01(audioVolume));
                }
                else
                {
                    Debug.LogWarning("RandomNumberGenerator: audio clip assigned but no AudioSource available to play it.");
                }
            }

            // schedule auto-hide if requested
            if (displayDuration > 0f)
            {
                // cancel previous hide if any
                if (hideCoroutine != null)
                {
                    StopCoroutine(hideCoroutine);
                    hideCoroutine = null;
                }
                hideCoroutine = StartCoroutine(HideAfterDelay(image, displayDuration));
            }
        }
        else
        {
            Debug.LogWarning($"RandomNumberGenerator generated: {LastValue} -> No sprite assigned for this number. Assign one in the inspector.");
        }
    }

    private IEnumerator HideAfterDelay(Image image, float seconds)
    {
        if (seconds <= 0f)
        {
            if (image != null) image.enabled = false;
            hideCoroutine = null;
            yield break;
        }

        yield return new WaitForSeconds(seconds);

        if (image != null)
            image.enabled = false;

        hideCoroutine = null;
    }

    // Ensure a runtime UI Image exists (tries to reuse any on this GameObject/children, otherwise creates a Canvas + Image)
    private Image EnsureRuntimeImage()
    {
        if (runtimeImage != null) return runtimeImage;
        if (targetImage != null) { runtimeImage = targetImage; return runtimeImage; }

        runtimeImage = GetComponentInChildren<Image>();
        if (runtimeImage != null) return runtimeImage;

        // Find or create Canvas
        var canvas = FindObjectOfType<Canvas>();
        GameObject canvasGO;
        if (canvas == null)
        {
            canvasGO = new GameObject("GeneratedCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        else
        {
            canvasGO = canvas.gameObject;
        }

        var go = new GameObject("GeneratedImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(canvasGO.transform, false);
        runtimeImage = go.GetComponent<Image>();

        // center on screen
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // hide by default until first generation
        if (runtimeImage != null)
            runtimeImage.enabled = false;

        return runtimeImage;
    }

    // Helper to create/find a runtime audio source when none was assigned
    private AudioSource EnsureRuntimeAudioSource()
    {
        if (runtimeAudioSource != null) return runtimeAudioSource;
        if (targetAudioSource != null) { runtimeAudioSource = targetAudioSource; return runtimeAudioSource; }

        runtimeAudioSource = GetComponentInChildren<AudioSource>();
        if (runtimeAudioSource != null) return runtimeAudioSource;

        var go = new GameObject("GeneratedAudioSource");
        go.transform.SetParent(transform, false);
        runtimeAudioSource = go.AddComponent<AudioSource>();
        runtimeAudioSource.playOnAwake = false;
        runtimeAudioSource.spatialBlend = 0f; // 2D sound
        return runtimeAudioSource;
    }

    // Returns the sprite assigned to a 1-based number, or null if not assigned / out of range
    public Sprite GetSpriteForNumber(int number)
    {
        int index = number - 1;
        if (index < 0 || numberSprites == null || index >= numberSprites.Length)
            return null;
        return numberSprites[index];
    }

    // Ensure image is hidden at start
    void Start()
    {
        // Do not auto-generate on start.
        // Hide any assigned image so nothing is visible before the first generation.
        if (targetImage != null)
            targetImage.enabled = false;
        else
        {
            var childImage = GetComponentInChildren<Image>();
            if (childImage != null)
                childImage.enabled = false;
        }
    }

    // Also allow manual triggering by pressing Q (Play mode, Game view focused)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Generate();
    }

    void OnDisable()
    {
        // Reset running flag and stop coroutine(s) if object is disabled
        isGenerating = false;
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        StopAllCoroutines();
    }
}
