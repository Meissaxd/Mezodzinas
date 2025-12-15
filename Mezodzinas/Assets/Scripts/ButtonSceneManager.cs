using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a UI Button manager or any GameObject and call LoadCharacterSelect from the Button OnClick.
/// Plays an optional click SFX and then loads the target scene. Can optionally wait for the SFX to finish.
/// </summary>
public class ButtonSceneManager : MonoBehaviour
{
    [Tooltip("Name of the scene to load. Must be in Build Settings.")]
    public string targetSceneName = "character select";

    [Header("Click SFX (optional)")]
    public AudioClip clickClip;
    [Range(0f, 1f)] public float clickVolume = 1f;
    [Tooltip("Optional persistent AudioSource (e.g. an AudioManager). If assigned, PlayOneShot will be used.")]
    public AudioSource persistentAudioSource;
    [Tooltip("If true, the scene load will wait until the click clip finishes playing.")]
    public bool waitForSfx = false;
    [Tooltip("Multiplier applied to the clip length when waiting (use 0.9-1.0 to shorten wait slightly).")]
    public float waitMultiplier = 1f;

    /// <summary>
    /// Call this method from the Button OnClick() in the Inspector.
    /// </summary>
    public void LoadCharacterSelect()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("ButtonSceneManager: targetSceneName is empty. Set it in the inspector or code.");
            return;
        }

        // Play SFX (if any)
        PlayClickSound();

        if (waitForSfx && clickClip != null)
        {
            float delay = Mathf.Max(0f, clickClip.length * Mathf.Max(0f, waitMultiplier));
            StartCoroutine(LoadAfterDelay(delay));
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void PlayClickSound()
    {
        if (clickClip == null) return;

        if (persistentAudioSource != null && persistentAudioSource.gameObject != this.gameObject)
        {
            // Use provided persistent source so sound survives scene load and centralized volume control
            persistentAudioSource.PlayOneShot(clickClip, Mathf.Clamp01(clickVolume));
        }
        else
        {
            // Create a temporary GameObject that won't be destroyed on scene load so the sound continues.
            GameObject temp = new GameObject("OneShotAudio");
            AudioSource src = temp.AddComponent<AudioSource>();
            src.clip = clickClip;
            src.volume = Mathf.Clamp01(clickVolume);
            src.spatialBlend = 0f; // 2D UI sound
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
        SceneManager.LoadScene(targetSceneName);
    }
}