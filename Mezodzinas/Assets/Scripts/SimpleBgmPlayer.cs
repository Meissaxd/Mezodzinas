using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Very small background music player.
/// - Attach to an empty GameObject (e.g. "BGMPlayer") in your scene.
/// - Assign musicClip and set playOnStart/persistAcrossScenes in the inspector.
/// - The player can optionally stop (or fade out) when a specific scene is loaded (e.g. "Virtuve").
/// </summary>
[DisallowMultipleComponent]
public class SimpleBgmPlayer : MonoBehaviour
{
    [Tooltip("Music clip to play as background music.")]
    public AudioClip musicClip;

    [Range(0f, 1f)]
    public float musicVolume = 1f;

    [Tooltip("Play music automatically on Start.")]
    public bool playOnStart = true;

    [Tooltip("If true, this GameObject will persist across scenes.")]
    public bool persistAcrossScenes = true;

    [Header("Stop behaviour")]
    [Tooltip("If non-empty, when this scene name is loaded the BGM will stop.")]
    public string stopOnSceneName = "Virtuve";
    [Tooltip("If > 0, fade out over this many seconds before stopping. If 0 the music stops immediately.")]
    public float fadeOutTime = 0.5f;

    private AudioSource src;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Ensure AudioSource on this GameObject
        src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();

        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D music
        src.volume = Mathf.Clamp01(musicVolume);

        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // unsubscribe
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (playOnStart && musicClip != null)
            PlayMusic(musicClip, true, musicVolume);
    }

    /// <summary>Start playing the given clip as looped background music.</summary>
    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (clip == null) return;

        // If a fade is running, stop it so Play isn't interrupted
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        src.clip = clip;
        src.loop = loop;
        src.volume = Mathf.Clamp01(volume);
        src.Play();
    }

    /// <summary>Stop music immediately (or start a fade out if fadeTime &gt; 0).</summary>
    public void StopMusic(float fadeTime = 0f)
    {
        if (!src.isPlaying) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (fadeTime > 0f)
            fadeCoroutine = StartCoroutine(FadeOutAndStopCoroutine(fadeTime));
        else
            src.Stop();
    }

    /// <summary>Set music volume (0..1).</summary>
    public void SetVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (src != null) src.volume = musicVolume;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If stopOnSceneName is set and matches the loaded scene, stop or fade out the music
        if (!string.IsNullOrEmpty(stopOnSceneName) && scene.name == stopOnSceneName)
        {
            StopMusic(fadeOutTime);
        }
    }

    private IEnumerator FadeOutAndStopCoroutine(float fadeTime)
    {
        float startVol = src.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        src.Stop();
        src.volume = musicVolume; // restore configured musicVolume in case PlayMusic is called later
        fadeCoroutine = null;
    }
}