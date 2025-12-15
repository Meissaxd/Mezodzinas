using UnityEngine;

/// <summary>
/// Very small background music player.
/// - Attach to an empty GameObject (e.g. "BGMPlayer") in your scene.
/// - Assign musicClip and set playOnStart/persistAcrossScenes in the inspector.
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

    private AudioSource src;

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

    void Start()
    {
        if (playOnStart && musicClip != null)
            PlayMusic(musicClip, true, musicVolume);
    }

    /// <summary>Start playing the given clip as looped background music.</summary>
    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (clip == null) return;
        src.clip = clip;
        src.loop = loop;
        src.volume = Mathf.Clamp01(volume);
        src.Play();
    }

    /// <summary>Stop music immediately.</summary>
    public void StopMusic()
    {
        if (src.isPlaying) src.Stop();
    }

    /// <summary>Set music volume (0..1).</summary>
    public void SetVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (src != null) src.volume = musicVolume;
    }
}