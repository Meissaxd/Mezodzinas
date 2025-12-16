using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Simple manager that shows a victory UI and plays a victory clip while stopping a background audio source.
/// - Assign a disabled UI panel (victoryUI) in the inspector.
/// - Assign the current background AudioSource (optional). The manager will Stop() it when victory happens.
/// - Assign a victoryClip to play (manager uses an internal AudioSource so the clip continues even if other objects are destroyed).
/// - Optionally assign a UI Button (quitButton) in the victory UI; it will be wired to call QuitGame().
/// </summary>
public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Assign your victory UI GameObject (panel/canvas). Should be disabled by default.")]
    public GameObject victoryUI;

    [Tooltip("Optional UI Button inside the victory UI that will be wired to QuitGame().")]
    public Button quitButton;

    [Header("Audio")]
    [Tooltip("Assign the AudioSource currently playing background music so it can be stopped. Optional.")]
    public AudioSource backgroundAudioSource;
    [Tooltip("Clip to play for victory.")]
    public AudioClip victoryClip;
    [Range(0f, 1f)] public float victoryVolume = 1f;

    [Header("Behaviour")]
    [Tooltip("If true, the manager will only allow victory to trigger once.")]
    public bool singleUse = true;

    bool hasShown = false;
    AudioSource internalAudio;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ensure victory UI is hidden at start
        if (victoryUI != null)
            victoryUI.SetActive(false);

        // create a private audio source for victory music (2D)
        internalAudio = gameObject.AddComponent<AudioSource>();
        internalAudio.playOnAwake = false;
        internalAudio.loop = false;
        internalAudio.spatialBlend = 0f;

        // Wire quit button if provided
        if (quitButton != null)
        {
            // remove previous listeners to avoid duplication
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    /// <summary>
    /// Call this to show the victory UI and play the victory music.
    /// </summary>
    public void ShowVictory()
    {
        if (singleUse && hasShown) return;
        hasShown = true;

        // show the UI
        if (victoryUI != null)
            victoryUI.SetActive(true);

        // stop background music (if assigned)
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
            backgroundAudioSource.Stop();

        // play victory clip via internal audio source so it is not cut off
        if (victoryClip != null && internalAudio != null)
        {
            internalAudio.volume = Mathf.Clamp01(victoryVolume);
            internalAudio.PlayOneShot(victoryClip);
        }
    }

    /// <summary>
    /// Hide the victory UI and stop victory audio (if playing).
    /// </summary>
    public void HideVictory()
    {
        if (victoryUI != null)
            victoryUI.SetActive(false);

        if (internalAudio != null && internalAudio.isPlaying)
            internalAudio.Stop();
    }

    /// <summary>
    /// Quit the game. Works in editor (stops play mode) and in builds (Application.Quit).
    /// You can assign this method to a Button OnClick in the inspector, or set the 'quitButton' field so it gets wired automatically.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Stop play mode inside the editor
        EditorApplication.isPlaying = false;
#else
        // Quit the built application
        Application.Quit();
#endif
    }
}