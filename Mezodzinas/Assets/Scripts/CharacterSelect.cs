using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to each character GameObject (sprite) that has a Collider2D.
/// Set a unique characterIndex for each option (0..3). When the sprite is clicked/tapped,
/// the selection is saved to PlayerPrefs and the MainScene is loaded.
/// This version optionally plays a click SFX and can wait for it to finish before loading.
/// </summary>
public class CharacterSelector : MonoBehaviour
{
    [Tooltip("Unique index for this character (e.g. 0..3)")]
    public int characterIndex = 0;

    [Tooltip("Optional name for debugging / display")]
    public string characterName = "";

    [Header("Click SFX (optional)")]
    [Tooltip("Sound to play when this character is selected.")]
    public AudioClip clickClip;
    [Range(0f, 1f)] public float clickVolume = 1f;

    [Tooltip("Optional persistent AudioSource (e.g. an AudioManager). If assigned, PlayOneShot is used.")]
    public AudioSource persistentAudioSource;

    [Tooltip("If true, the scene load will wait until the click clip finishes playing.")]
    public bool waitForSfx = false;

    [Tooltip("Multiplier applied to the clip length when waiting (use 0.9-1.0 to shorten wait slightly).")]
    public float waitMultiplier = 1f;

    // Called when the object with a Collider2D is clicked (or tapped).
    private void OnMouseDown()
    {
        // Save selection so the next scene can read it
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        PlayerPrefs.SetString("SelectedCharacterName", characterName);
        PlayerPrefs.Save();

        // Play SFX if assigned
        if (clickClip != null)
            PlayClickSound();

        // Load scene now or after SFX
        if (waitForSfx && clickClip != null)
        {
            float delay = Mathf.Max(0f, clickClip.length * Mathf.Max(0f, waitMultiplier));
            StartCoroutine(LoadAfterDelay(delay));
        }
        else
        {
            // Load immediately. If you're using a persistent AudioSource (on a DontDestroyOnLoad object),
            // the sound will continue after scene load.
            SceneManager.LoadScene("MainScene");
        }
    }

    private void PlayClickSound()
    {
        if (clickClip == null) return;

        // Use provided persistent audio source if assigned (recommended for sounds that should survive scene changes)
        if (persistentAudioSource != null && persistentAudioSource.gameObject != this.gameObject)
        {
            persistentAudioSource.PlayOneShot(clickClip, Mathf.Clamp01(clickVolume));
        }
        else
        {
            // Fallback: create a temporary GameObject for the audio and mark it DontDestroyOnLoad so the sound continues across scene loads.
            GameObject temp = new GameObject("OneShotAudio");
            AudioSource src = temp.AddComponent<AudioSource>();
            src.clip = clickClip;
            src.volume = Mathf.Clamp01(clickVolume);
            src.spatialBlend = 0f; // 2D UI-like sound
            src.playOnAwake = false;
            src.loop = false;
            src.Play();

            DontDestroyOnLoad(temp);
            Destroy(temp, clickClip.length + 0.1f);
        }
    }

    private IEnumerator LoadAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("MainScene");
    }
}